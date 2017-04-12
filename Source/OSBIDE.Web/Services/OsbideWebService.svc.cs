using OSBIDE.Data.SQLDatabase.Edmx;
using OSBIDE.Library;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Text.RegularExpressions;

namespace OSBIDE.Web.Services
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class OsbideWebService
    {
        public OsbideContext Db { get; private set; }

        public OsbideWebService()
        {
#if DEBUG
            Db = new OsbideContext("OsbideDebugContext");
            //Database.SetInitializer<OsbideContext>(new CreateDatabaseIfNotExists<OsbideContext>());
            //Database.SetInitializer<OsbideContext>(new OsbideContextModelChangeInitializer());
            Database.SetInitializer<OsbideContext>(new CreateDatabaseIfNotExists<OsbideContext>());
#else
            Db = new OsbideContext("OsbideReleaseContext");
#endif
        }

        [OperationContract]
        public string Echo(string toEcho)
        {
            return toEcho;
        }

        /// <summary>
        /// Logs a VS transaction for the given auth key
        /// </summary>
        /// <param name="authKey"></param>
        private void LogUserTransaction(string authToken)
        {
            OsbideUser user = GetActiveUser(authToken);
            if (user != null)
            {
                LogUserTransaction(user);
            }
        }

        private void LogUserTransaction(OsbideUser user)
        {
            OsbideUser dbUser = Db.Users.Where(u => u.Id == user.Id).FirstOrDefault();
            if (dbUser != null)
            {
                dbUser.LastVsActivity = DateTime.UtcNow;
                Db.SaveChanges();
            }
        }

        /// <summary>
        /// Attempts to log the user into OSBIDE.  Returns a hash that can be used to authenticate future requests.  
        /// If the login fails, the hash will be an empty string.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns>A hashed string that can be used to authenticate future requests</returns>
        [OperationContract]
        public string Login(string email, string hashedPassword)
        {
            string hash = "";
            if (UserPassword.ValidateUserHashedPassword(email, hashedPassword, Db))
            {
                Authentication auth = new Authentication();
                OsbideUser user = Db.Users.Where(u => u.Email.CompareTo(email) == 0).FirstOrDefault();
                if (user != null)
                {
                    hash = auth.LogIn(user);
                    LogUserTransaction(user);
                }
            }
            return hash;
        }

        /// <summary>
        /// Will return the user identified by the supplied authentication token
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        [OperationContract]
        public OsbideUser GetActiveUser(string authToken)
        {
            Authentication auth = new Authentication();
            return new OsbideUser(auth.GetActiveUser(authToken));
        }

        /// <summary>
        /// Tells the client whether or not the supplied token is valid.  May be used as a
        /// "keep alive" method.
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        [OperationContract]
        public bool IsValidKey(string authToken)
        {
            Authentication auth = new Authentication();
            bool isValid = false;
            if (auth.IsValidKey(authToken))
            {
                LogUserTransaction(authToken);
                isValid = true;
            }
            return isValid;
        }

        /// <summary>
        /// Will return the date of the most recent whats new item.  
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        public DateTime GetMostRecentWhatsNewItem()
        {
            var query = (from news in Db.WhatsNewItems
                         orderby news.DatePosted descending
                         select news)
                        .Take(1)
                        .FirstOrDefault();
            DateTime result = DateTime.MinValue;
            if (query != null)
            {
                result = query.DatePosted;
            }
            return result;
        }

        /// <summary>
        /// Will return the time of the last social event that the user was involved with
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        [OperationContract]
        public DateTime GetMostRecentSocialActivity(string authToken)
        {
            DateTime lastSocialActivity = DateTime.MinValue;

            Authentication auth = new Authentication();
            if (auth.IsValidKey(authToken) == true)
            {
                OsbideUser authUser = GetActiveUser(authToken);
                var query = (from social in Db.CommentActivityLogs
                             where 1 == 1
                             && (social.TargetUserId == authUser.Id || social.LogCommentEvent.SourceEventLog.SenderId == authUser.Id)
                             orderby social.LogCommentEvent.EventDate descending
                             select social)
                                                  .Take(1)
                                                  .FirstOrDefault();
                if (query != null)
                {
                    lastSocialActivity = query.LogCommentEvent.EventDate;
                }
            }

            return lastSocialActivity;
        }

        /// <summary>
        /// Will return a list of courses in which the user is presently enrolled
        /// </summary>
        /// <param name="authToken"></param>
        /// <returns></returns>
        [OperationContract]
        public List<Course> GetCoursesForUser(string authToken)
        {
            List<Course> courses = new List<Course>();

            Authentication auth = new Authentication();
            if (auth.IsValidKey(authToken) == true)
            {
                OsbideUser authUser = GetActiveUser(authToken);

                //log the last activity date
                LogUserTransaction(authUser);

                //AC: I'm getting an exception when I try sorting by name USING EF/LINQ, so I guess I'll do it the hard way
                SortedDictionary<string, Course> sorted = new SortedDictionary<string, Course>();
                var query = (from cur in Db.CourseUserRelationships
                             where cur.UserId == authUser.Id
                             && cur.Course.IsDeleted == false
                             select cur.Course)
                             .ToList();
                foreach (Course c in query)
                {
                    if (sorted.ContainsKey(c.Name) == false)
                    {
                        sorted.Add(c.Name, c);
                    }
                }
                foreach (KeyValuePair<string, Course> kvp in sorted)
                {
                    courses.Add(new Course(kvp.Value));
                }
            }
            return courses;
        }

        /// <summary>
        /// Will return a list of assignments attached to a given course
        /// </summary>
        /// <param name="courseId"></param>
        /// <returns></returns>
        [OperationContract]
        public List<Assignment> GetAssignmentsForCourse(int courseId, string authToken)
        {
            List<Assignment> assignments = new List<Assignment>();

            Authentication auth = new Authentication();
            if (auth.IsValidKey(authToken) == true)
            {
                List<Assignment> efAssignments = Db.Assignments
                    .Where(a => a.CourseId == courseId)
                    .Where(a => a.IsDeleted == false)
                    .OrderBy(a => a.Name).ToList();
                foreach (Assignment assignment in efAssignments)
                {
                    assignments.Add(new Assignment(assignment));
                }
            }
            return assignments;
        }

        /// <summary>
        /// Will submit an assignment for a given student
        /// </summary>
        /// <param name="assignmentId"></param>
        /// <param name="zipData"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        [OperationContract]
        public int SubmitAssignment(int assignmentId, EventLog assignmentLog, string authToken)
        {
            int result = -1;
            Authentication auth = new Authentication();
            if (auth.IsValidKey(authToken) == true)
            {
                //replace sender information with what is contained in the auth key
                OsbideUser authUser = GetActiveUser(authToken);

                //log the last activity date
                LogUserTransaction(authUser);

                result = SubmitAssignment(assignmentId, assignmentLog, authUser);
            }
            return result;
        }

        public int SubmitAssignment(int assignmentId, EventLog assignmentLog, OsbideUser currentUser)
        {
            EventLog submittedLog = SubmitLog(assignmentLog, currentUser);
            if (submittedLog != null)
            {
                return submittedLog.Id;
            }
            return -1;
        }

        /// <summary>
        /// Will return the date of the last submit for a given user
        /// </summary>
        /// <param name="assignmentId"></param>
        /// <param name="authToken"></param>
        /// <returns></returns>
        [OperationContract]
        public DateTime GetLastAssignmentSubmitDate(int assignmentId, string authToken)
        {
            DateTime lastSubmit = DateTime.MinValue;
            Authentication auth = new Authentication();
            if (auth.IsValidKey(authToken) == true)
            {
                OsbideUser authUser = GetActiveUser(authToken);

                //log the last activity date
                LogUserTransaction(authUser);

                //find the time of the last submit
                var query = (from log in Db.SubmitEvents
                             where log.AssignmentId == assignmentId
                             && log.EventLog.SenderId == authUser.Id
                             orderby log.EventLog.DateReceived descending
                             select log).FirstOrDefault();
                if (query != null)
                {
                    lastSubmit = query.EventLog.DateReceived;
                }
            }
            return lastSubmit;
        }

        [OperationContract]
        [ApplyDataContractResolver]
        public int SubmitLocalErrorLog(LocalErrorLog errorLog, string authToken)
        {
            Authentication auth = new Authentication();
            if (auth.IsValidKey(authToken) == true)
            {
                OsbideUser authUser = GetActiveUser(authToken);
                LogUserTransaction(authUser);

                //replace the error log's sender information with what we obtained from the auth key
                errorLog.Sender = null;
                errorLog.SenderId = authUser.Id;

                //add to the db and give it a try
                Db.LocalErrorLogs.Add(errorLog);
                try
                {
                    Db.SaveChanges();
                }
                catch (Exception)
                {
                    return (int)Enums.ServiceCode.DatabaseError;
                }
                return errorLog.Id;
            }
            else
            {
                return (int)Enums.ServiceCode.AuthenticationError;
            }
        }

        public EventLog SubmitLog(EventLog log, OsbideUser user)
        {
            LocalErrorLog errorLogger = new LocalErrorLog();
            errorLogger.SenderId = user.Id;
            errorLogger.Content = "About to save log " + log.LogType + " to DB.  ";
            errorLogger.LogDate = DateTime.Now;

            log.Sender = null;
            log.SenderId = user.Id;
            log.DateReceived = DateTime.UtcNow;
            log.EventTypeId = Convert.ToInt32(Enum.Parse(typeof(EventTypes), log.LogType));

            //insert into the DB
            Db.EventLogs.Add(log);
            try
            {
                Db.SaveChanges();
                errorLogger.Content += "Item saved.  ";
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                errorLogger.Content += "Error saving:  ";
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        errorLogger.Content += string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                Db.LocalErrorLogs.Add(errorLogger);
                Db.SaveChanges();
                return null;
            }
            catch (Exception ex)
            {
                errorLogger.Content += "Error saving: " + ex.Message;
                System.Diagnostics.Trace.TraceInformation(ex.Message);
                Db.LocalErrorLogs.Add(errorLogger);
                Db.SaveChanges();
                return null;
            }

            //Tease apart log information and insert into the appropriate DB table
            IOsbideEvent evt = null;
            try
            {
                errorLogger.Content += "About to unzip event.";
                evt = EventFactory.FromZippedBinary(log.Data.BinaryData, new OsbideDeserializationBinder());
                evt.EventLogId = log.Id;
            }
            catch (Exception)
            {
                errorLogger.Content += "Error unzipping event.";
                Db.LocalErrorLogs.Add(errorLogger);
                Db.SaveChanges();
                return null;
            }

            var hashtags = string.Empty;
            var usertags = string.Empty;
            if (log.LogType == AskForHelpEvent.Name)
            {
                Db.AskForHelpEvents.Add((AskForHelpEvent)evt);
                AskForHelpEvent ask = evt as AskForHelpEvent;

                //send email to interested parties
                //find all of this user's subscribers and send them an email
                List<OsbideUser> observers = new List<OsbideUser>();

                observers = (from subscription in Db.UserSubscriptions
                             join dbUser in Db.Users on
                                               new { InstitutionId = subscription.ObserverInstitutionId, SchoolId = subscription.ObserverSchoolId }
                                               equals new { InstitutionId = user.InstitutionId, SchoolId = user.SchoolId }
                             where subscription.SubjectSchoolId == user.SchoolId
                                && subscription.SubjectInstitutionId == user.InstitutionId
                                && dbUser.ReceiveEmailOnNewFeedPost == true
                             select dbUser).ToList();
                if (observers.Count > 0)
                {
                    string url = StringConstants.GetActivityFeedDetailsUrl(log.Id);
                    string body = "Greetings,<br />{0} asked for help regarding the following item:<br />\"{1}\"<br />To view this "
                    + "conversation online, please visit {2} or visit your OSBIDE user profile.<br /><br />Thanks,<br />OSBIDE<br /><br />"
                    + "These automated messages can be turned off by editing your user profile.";
                    body = string.Format(body, user.FirstAndLastName, ask.UserComment, url);
                    List<MailAddress> to = new List<MailAddress>();
                    foreach (OsbideUser observer in observers)
                    {
                        to.Add(new MailAddress(observer.Email));
                    }
                    Email.Send("[OSBIDE] Someone has asked for help!", body, to);
                }
            }
            else if (log.LogType == BuildEvent.Name)
            {
                BuildEvent build = (BuildEvent)evt;
                Db.BuildEvents.Add(build);

                string pattern = "error ([^:]+)";

                //strip out non-critical errors
                List<BuildEventErrorListItem> errorItems = build.ErrorItems.Where(e => e.ErrorListItem.CriticalErrorName.Length > 0).ToList();
                build.ErrorItems.Clear();
                build.ErrorItems = errorItems;

                //log all errors in their own DB for faster search
                List<string> errors = new List<string>();
                Dictionary<string, ErrorType> errorTypes = new Dictionary<string, ErrorType>();
                foreach (BuildEventErrorListItem item in build.ErrorItems)
                {
                    Match match = Regex.Match(item.ErrorListItem.Description, pattern);

                    //ignore bad matches
                    if (match.Groups.Count == 2)
                    {
                        string errorCode = match.Groups[1].Value.ToLower().Trim();
                        ErrorType type = Db.ErrorTypes.Where(t => t.Name == errorCode).FirstOrDefault();
                        if (type == null)
                        {
                            if (errorTypes.ContainsKey(errorCode) == false)
                            {
                                type = new ErrorType()
                                {
                                    Name = errorCode
                                };
                                Db.ErrorTypes.Add(type);
                            }
                            else
                            {
                                type = errorTypes[errorCode];
                            }
                        }
                        if (errorCode.Length > 0 && errors.Contains(errorCode) == false)
                        {
                            errors.Add(errorCode);
                        }
                        errorTypes[errorCode] = type;
                    }
                }
                Db.SaveChanges();
                foreach (string errorType in errors)
                {
                    Db.BuildErrors.Add(new BuildError()
                    {
                        BuildErrorTypeId = errorTypes[errorType].Id,
                        LogId = log.Id
                    });
                }

            }
            else if (log.LogType == CutCopyPasteEvent.Name)
            {
                Db.CutCopyPasteEvents.Add((CutCopyPasteEvent)evt);
            }
            else if (log.LogType == DebugEvent.Name)
            {
                Db.DebugEvents.Add((DebugEvent)evt);
            }
            else if (log.LogType == EditorActivityEvent.Name)
            {
                Db.EditorActivityEvents.Add((EditorActivityEvent)evt);
            }
            else if (log.LogType == ExceptionEvent.Name)
            {
                Db.ExceptionEvents.Add((ExceptionEvent)evt);
            }
            else if (log.LogType == FeedPostEvent.Name)
            {
                hashtags = string.Join(",", ParseHashtags(((FeedPostEvent)evt).Comment));
                usertags = string.Join(",", ParseUserTags(((FeedPostEvent)evt).Comment));
                Db.FeedPostEvents.Add((FeedPostEvent)evt);
            }
            else if (log.LogType == HelpfulMarkGivenEvent.Name)
            {
                Db.HelpfulMarkGivenEvents.Add((HelpfulMarkGivenEvent)evt);
            }
            else if (log.LogType == LogCommentEvent.Name)
            {
                Db.LogCommentEvents.Add((LogCommentEvent)evt);
            }
            else if (log.LogType == SaveEvent.Name)
            {
                Db.SaveEvents.Add((SaveEvent)evt);
            }
            else if (log.LogType == SubmitEvent.Name)
            {
                errorLogger.Content += "Submit event detected.  ";
                Db.SubmitEvents.Add((SubmitEvent)evt);
            }
            try
            {
                errorLogger.Content += "Attempting to save to DB.  ";
                Db.SaveChanges();
                /*
                if(hashtags.Length >= 0 || usertags.Length >= 0 )
                    
                using (var context = new OsbideProcs())
                {
                    context.InsertPostTags(log.Id, usertags, hashtags);
                }
                     * */
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
            {
                errorLogger.Content += "Error saving to DB:  ";
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        errorLogger.Content += string.Format("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                        Db.LocalErrorLogs.Add(errorLogger);
                        Db.SaveChanges();
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                errorLogger.Content += "Error saving to DB: " + ex.Message;
                Db.LocalErrorLogs.Add(errorLogger);
                Db.SaveChanges();
                System.Diagnostics.Trace.TraceInformation(ex.Message);
                return null;
            }

            //Db.LocalErrorLogs.Add(errorLogger);
            //Db.SaveChanges();
            return log;
        }

        private List<string> ParseHashtags(string content)
        {
            string HashRegex = "#[A-Za-z][A-Za-z0-9]+";
            MatchCollection matchList = Regex.Matches(content, HashRegex);
            return matchList.Cast<Match>().Select(match => match.Value.Substring(1)).ToList();
        }

        private List<string> ParseUserTags(string content)
        {
            string HashRegex = "@[A-Za-z][A-Za-z0-9]+";
            MatchCollection matchList = Regex.Matches(content, HashRegex);
            return matchList.Cast<Match>().Select(match => match.Value.Substring(1)).ToList();
        }

        [OperationContract]
        [ApplyDataContractResolver]
        public int SubmitLog(EventLog log, string authToken)
        {

            //verify request before continuing
            Authentication auth = new Authentication();
            if (auth.IsValidKey(authToken) == false)
            {
                return (int)Enums.ServiceCode.AuthenticationError;
            }

            //AC: kind of hackish, but event logs that we receive should already have an ID
            //attached to them from being stored in the machine's local DB.  We can use 
            //that ID to track the success/failure of asynchronous calls.
            int localId = log.Id;

            //we don't want the local id, so be sure to clear
            log.Id = 0;

            //replace sender information with what is contained in the auth key
            OsbideUser authUser = GetActiveUser(authToken);

            //log the last activity date
            LogUserTransaction(authUser);

            //students: send all events.
            //other: send only "ask for help" events
            if (authUser.Role != SystemRole.Student)
            {
                if (log.LogType != AskForHelpEvent.Name)
                {
                    return localId;
                }
            }

            EventLog submittedLog = SubmitLog(log, authUser);
            if (submittedLog == null)
            {
                return (int)Enums.ServiceCode.DatabaseError;
            }
            else
            {
                //Return the ID number of the local object so that the caller knows that it's been successfully
                //saved into the main system.
                return localId;
            }
        }

        [OperationContract]
        public string LibraryVersionNumber()
        {
            return OSBIDE.Library.StringConstants.LibraryVersion;
        }

        [OperationContract]
        public string OsbidePackageUrl()
        {
            return OSBIDE.Library.StringConstants.OsbidePackageUrl;
        }
    }
}
