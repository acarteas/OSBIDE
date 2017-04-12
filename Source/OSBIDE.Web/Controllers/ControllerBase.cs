using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Caching;
using System.Web.Mvc;

using OSBIDE.Data.SQLDatabase;
using OSBIDE.Library;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Services;

namespace OSBIDE.Web.Controllers
{
    public abstract class ControllerBase : Controller
    {
        protected OsbideContext Db { get; private set; }
        protected OsbideUser CurrentUser { get; set; }
        protected FileCache GlobalCache { get; private set; }
        protected FileCache UserCache { get; private set; }

        public static string DefaultConnectionString
        {
            get
            {
                var conn = string.Empty;
#if DEBUG
                conn = System.Configuration.ConfigurationManager.ConnectionStrings["OsbideDebugContext"].ConnectionString;
#else
                conn = System.Configuration.ConfigurationManager.ConnectionStrings["OsbideReleaseContext"].ConnectionString;
#endif
                return conn;
            }
        }

        public ControllerBase()
        {
            //set up DB
            Db = OsbideContext.DefaultWebConnection;

            //set up current user
            Authentication auth = new Authentication();
            string authKey = auth.GetAuthenticationKey();
            int id = auth.GetActiveUserId(authKey);
            
            //make sure that we got back a good key
            if (id > 0)
            {
                CurrentUser = Db.Users.Find(id);
                if (CurrentUser != null)
                {
                    CurrentUser.PropertyChanged += CurrentUser_PropertyChanged;
                }
                else
                {
                    CurrentUser = new OsbideUser();
                }
            }
            else
            {
                CurrentUser = new OsbideUser();
            }
            
            //set up caches
            GlobalCache = FileCacheHelper.GetGlobalCacheInstance();
            UserCache = FileCacheHelper.GetCacheInstance(CurrentUser);

            //update all users scores if necessary
            object lastScoreUpdate = GlobalCache["lastScoreUpdate"];
            bool needsScoreUpdate = true;
            if (lastScoreUpdate != null)
            {
                DateTime lastUpdate = (DateTime)lastScoreUpdate;
                if (lastUpdate.AddDays(1) > DateTime.UtcNow)
                {
                    needsScoreUpdate = false;
                }
            }
            if (needsScoreUpdate == true)
            {
                //UpdateUserScores();
                GlobalCache["lastScoreUpdate"] = DateTime.UtcNow;
            }

            //make current user available to all views
            ViewBag.CurrentUser = CurrentUser;
        }

        /// <summary>
        /// Updates all users' scores and saves the information in the database
        /// </summary>
        private void UpdateUserScores()
        {
            //scoring:
            // 1 pt for each FeedPostEvent / AskForHelpEvent
            // 3 pts for each log comment
            // 7 pts for each helpful mark
            var onePtQuery = from log in Db.EventLogs
                             where (log.LogType == FeedPostEvent.Name || log.LogType == AskForHelpEvent.Name)
                             group log by log.SenderId into logGroup
                             select new { UserId = logGroup.Key, LogCount = logGroup.Count() };

            var threePtQuery = from comment in Db.LogCommentEvents
                               group comment by comment.EventLog.SenderId into commentGroup
                               select new { UserId = commentGroup.Key, CommentCount = commentGroup.Count() };

            var sevenPtQuery = from helpful in Db.HelpfulMarkGivenEvents
                               group helpful by helpful.LogCommentEvent.EventLog.SenderId into helpfulGroup
                               select new { UserId = helpfulGroup.Key, HelpfulCount = helpfulGroup.Count() };

            Dictionary<int, int> scores = new Dictionary<int, int>();
            foreach (var row in onePtQuery)
            {
                if (scores.ContainsKey(row.UserId) == false)
                {
                    scores.Add(row.UserId, 0);
                }
                scores[row.UserId] += row.LogCount;
            }

            foreach (var row in threePtQuery)
            {
                if (scores.ContainsKey(row.UserId) == false)
                {
                    scores.Add(row.UserId, 0);
                }
                scores[row.UserId] += (row.CommentCount * 3);
            }
            foreach (var row in sevenPtQuery)
            {
                if (scores.ContainsKey(row.UserId) == false)
                {
                    scores.Add(row.UserId, 0);
                }
                scores[row.UserId] += (row.HelpfulCount * 7);
            }

            //remove all user scores
            Db.DeleteUserScores();

            //add in new ones
            foreach (int userKey in scores.Keys)
            {
                UserScore score = new UserScore()
                {
                    UserId = userKey,
                    Score = scores[userKey],
                    LastCalculated = DateTime.UtcNow
                };
                Db.UserScores.Add(score);
            }
            Db.SaveChanges();
        }

        /// <summary>
        /// Updates the event log subscription list for the supplied user.  
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public OsbideUser UpdateLogSubscriptions(OsbideUser user)
        {
            user.LogSubscriptions = Db.EventLogSubscriptions.Where(u => u.UserId == user.Id).ToList();
            return user;
        }

        /// <summary>
        /// Called whenever the system modifies the current user.  This function will update the DB and also the local cookie-based cache
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CurrentUser_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Db.Entry(CurrentUser).State = EntityState.Modified;
            try
            {
                Db.SaveChanges();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }
            Authentication auth = new Authentication();
            auth.LogIn(CurrentUser);
        }

        /// <summary>
        /// Will return a list of recent compile errors for the given user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="timeframe">How far back the system should look</param>
        /// <returns></returns>
        protected string[] GetRecentCompileErrors(OsbideUser user, DateTime timeframe)
        {
            var errors = new List<string>();

            var errorItems = RecentErrorProc.Get(user.Id, timeframe);
            return errorItems.Where(e => e.CriticalErrorName.Length > 0).Select(e => e.CriticalErrorName).ToArray();
        }
        
        /// <summary>
        /// Will return a list of recent compile errors for the given user.  Will pull errors that occurred within the last
        /// 48 hours.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected string[] GetRecentCompileErrors(OsbideUser user)
        {
            return GetRecentCompileErrors(user, DefaultErrorLookback);
        }

        /// <summary>
        /// The default amount of time to "look back" for similar errors caused by other users
        /// </summary>
        protected DateTime DefaultErrorLookback
        {
            get
            {
                return DateTime.UtcNow.Subtract(new TimeSpan(0, 48, 0, 0, 0));
            }
        }

        protected List<int> ParseIdString(string idStr)
        {
            //get out list of ID numbers
            string[] rawIds = idStr.Split(',');
            List<int> ids = new List<int>(rawIds.Length);
            for (int i = 0; i < rawIds.Length; i++)
            {
                int tempId = -1;
                if (Int32.TryParse(rawIds[i], out tempId) == true)
                {
                    ids.Add(tempId);
                }
            }
            return ids;
        }

        /// <summary>
        /// Helper method for posting a comment.  Will return true if everything went OK, false otherwise
        /// </summary>
        /// <param name="logId"></param>
        /// <param name="comment"></param>
        /// <returns></returns>
        protected bool PostComment(string logId, string comment)
        {
            int id = -1;
            if (Int32.TryParse(logId, out id) == true)
            {
                //comments made on comments or mark helpful events need to be routed back to the original source
                IOsbideEvent checkEvent = Db.LogCommentEvents.Where(l => l.EventLogId == id).FirstOrDefault();
                if (checkEvent != null)
                {
                    id = (checkEvent as LogCommentEvent).SourceEventLogId;
                }
                else
                {
                    checkEvent = Db.HelpfulMarkGivenEvents.Where(l => l.EventLogId == id).FirstOrDefault();
                    if (checkEvent != null)
                    {
                        id = (checkEvent as HelpfulMarkGivenEvent).LogCommentEvent.SourceEventLogId;
                    }
                }

                LogCommentEvent logComment = new LogCommentEvent()
                {
                    Content = comment,
                    SourceEventLogId = id,
                    SolutionName = "OSBIDE"
                };
                OsbideWebService client = new OsbideWebService();
                Authentication auth = new Authentication();
                string key = auth.GetAuthenticationKey();
                EventLog log = null;
                if (string.IsNullOrEmpty(comment) == false)
                {
                    log = new EventLog(logComment, CurrentUser);
                    log = client.SubmitLog(log, CurrentUser);
                }
                else
                {
                    return false;
                }
                logComment = Db.LogCommentEvents.Where(l => l.EventLogId == log.Id).FirstOrDefault();

                //the code below performs two functions:
                // 1. Send interested parties email notifications
                // 2. Log the comment in the social activity log (displayed on an individual's profile page)

                //find others that have posted on this same thread
                List<OsbideUser> interestedParties = Db.LogCommentEvents
                    .Where(l => l.SourceEventLogId == id)
                    .Where(l => l.EventLog.SenderId != CurrentUser.Id)
                    .Select(l => l.EventLog.Sender)
                    .ToList();

                //(email only) find those that are subscribed to this thread
                List<OsbideUser> subscribers = (from logSub in Db.EventLogSubscriptions
                                                join user in Db.Users on logSub.UserId equals user.Id
                                                where logSub.LogId == id
                                                && logSub.UserId != CurrentUser.Id
                                                && user.ReceiveNotificationEmails == true
                                                select user).ToList();

                //check to see if the author wants to be notified of posts
                OsbideUser eventAuthor = Db.EventLogs.Where(l => l.Id == id).Select(l => l.Sender).FirstOrDefault();

                //master list shared between email and social activity log
                Dictionary<int, OsbideUser> masterList = new Dictionary<int, OsbideUser>();
                if (eventAuthor != null)
                {
                    masterList.Add(eventAuthor.Id, eventAuthor);
                }
                foreach (OsbideUser user in interestedParties)
                {
                    if (masterList.ContainsKey(user.Id) == false)
                    {
                        masterList.Add(user.Id, user);
                    }
                }

                //add the current user for activity log tracking, but not for emails
                OsbideUser creator = new OsbideUser(CurrentUser);
                creator.ReceiveNotificationEmails = false;  //force no email send on the current user
                if (masterList.ContainsKey(creator.Id) == true)
                {
                    masterList.Remove(creator.Id);
                }
                masterList.Add(creator.Id, creator);

                //update social activity
                foreach (OsbideUser user in masterList.Values)
                {
                    CommentActivityLog social = new CommentActivityLog()
                    {
                        TargetUserId = user.Id,
                        LogCommentEventId = logComment.Id
                    };
                    Db.CommentActivityLogs.Add(social);
                }
                Db.SaveChanges();

                //form the email list
                SortedDictionary<int, OsbideUser> emailList = new SortedDictionary<int, OsbideUser>();

                //add in interested parties from our master list
                foreach (OsbideUser user in masterList.Values)
                {
                    if (user.ReceiveNotificationEmails == true)
                    {
                        if (emailList.ContainsKey(user.Id) == false)
                        {
                            emailList.Add(user.Id, user);
                        }
                    }
                }

                //add in subscribers to email list
                foreach (OsbideUser user in subscribers)
                {
                    if (emailList.ContainsKey(user.Id) == false)
                    {
                        emailList.Add(user.Id, user);
                    }
                }

                //send emails
                if (emailList.Count > 0)
                {
                    //send email
                    string url = StringConstants.GetActivityFeedDetailsUrl(id);
                    string body = "Greetings,<br />{0} has commented on a post that you have previously been involved with:<br />\"{1}\"<br />To view this "
                    + "conversation online, please visit {2} or visit your OSBIDE user profile.<br /><br />Thanks,<br />OSBIDE<br /><br />"
                    + "These automated messages can be turned off by editing your user profile.";
                    body = string.Format(body, logComment.EventLog.Sender.FirstAndLastName, logComment.Content, url);
                    List<MailAddress> to = new List<MailAddress>();
                    foreach (OsbideUser user in emailList.Values)
                    {
                        to.Add(new MailAddress(user.Email));
                    }
                    Email.Send("[OSBIDE] Activity Notification", body, to);
                }
            }
            return true;
        }
        public void LogErrorMessage(Exception ex)
        {
            var msg = string.Format("message: {0}, source: {1}, stack trace: {2}, TargetSite: {3}", ex.Message, ex.Source, ex.StackTrace, ex.TargetSite);

            if (ex.InnerException != null && !string.IsNullOrWhiteSpace(ex.InnerException.Message))
            {
                msg = string.Format("{0} {1}Inner Exception: {2}", msg, Environment.NewLine, ex.InnerException.Message);
            }

            if (ex.Data != null && ex.Data.Keys != null)
            {
                foreach (var key in ex.Data.Keys)
                {
                    msg = string.Format("{0}, {1}|{2},", msg, key, ex.Data[key]);
                }
            }

            Db.LocalErrorLogs.Add(new LocalErrorLog
            {
                SenderId = CurrentUser.Id,
                LogDate = DateTime.Now,
                Content = msg
            });
            Db.SaveChanges();
        }
    }
}
