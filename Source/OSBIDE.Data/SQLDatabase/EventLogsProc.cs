using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;

namespace OSBIDE.Data.SQLDatabase
{
    public static class EventLogsProc
    {
        public static IEnumerable<FeedItem> GetActivityFeeds(DateTime dateReceivedMin, DateTime dateReceivedMax,
            IEnumerable<int> logIds, IEnumerable<int> eventTypes,
            int? courseId, int? roleId, string commentFilter,
            IEnumerable<int> senderIds, int? minLogId, int? maxLogId, int offsetN, int? topN)
        {
            using (var context = new OsbideProcs())
            {
                var c = commentFilter.Length > 0 ? string.Format("%{0}%", commentFilter) : string.Empty;
                var l = string.Join(",", logIds.Where(i => i > 0).ToArray());
                var t = eventTypes.Count() > 0 ? string.Format("{0}", string.Join(",", eventTypes)) : string.Empty;
                var s = string.Join(",", senderIds.Where(i => i > 0).ToArray());

                return ReadResults(context.GetActivityFeeds(dateReceivedMin, dateReceivedMax, l, t, courseId, roleId, c, s, minLogId, maxLogId, offsetN, topN));
            }
        }

        /// <summary>
        /// build the domain objects from custom result sets
        /// note: the result sets are ordered and they need to be read in the same order
        /// </summary>
        /// <param name="resultsR"></param>
        /// <returns></returns>
        private static IEnumerable<FeedItem> ReadResults(System.Data.Entity.Core.Objects.ObjectResult<GetEventLogs_Result> resultsR)
        {
            // the domain objects to build
            var feedItems = new SortedDictionary<DateTime, FeedItem>();

            #region read and process the criteria-qualified events shared data

            // the first result set, mainly comes from EventLog table
            var logs = new List<GetEventLogs_Result>();
            logs.AddRange(resultsR);

            // the second image result is removed since it not used
            // the third result set includes a distinct list of involved users
            var usersR = resultsR.GetNextResult<GetOsbideUsers_Result>();
            var users = new List<OsbideUser>();
            users.AddRange(usersR.Select(u => new OsbideUser
                                                          {
                                                              Id = u.Id,
                                                              FirstName = u.FirstName,
                                                              LastName = u.LastName,
                                                              Email = u.Email,
                                                              DefaultCourse = new Course { Id = u.DefaultCourseId, Prefix = u.DefaultCourseNamePrefix, CourseNumber = u.DefaultCourseNumber },
                                                              InstitutionId = u.InstitutionId,
                                                              SchoolId = u.SchoolId,
                                                              ReceiveEmailOnNewAskForHelp = u.ReceiveEmailOnNewAskForHelp,
                                                              ReceiveEmailOnNewFeedPost = u.ReceiveEmailOnNewFeedPost,
                                                              ReceiveNotificationEmails = u.ReceiveNotificationEmails,
                                                              LastVsActivity = u.LastVsActivity,
                                                          }));

            // the fourth result set includes the distinct list of comments on the criteria-qualified events
            var commentsR = usersR.GetNextResult<GetLogComments_Result>();
            var comments = new List<LogCommentEvent>();
            comments.AddRange(commentsR.Select(e => new LogCommentEvent
                                                          {
                                                              EventDate = e.EventDate,
                                                              EventLog = new EventLog
                                                                             {
                                                                                 Id = e.EventLogId,
                                                                                 LogType = "LogCommentEvent"
                                                                             },
                                                              EventLogId = e.EventLogId,
                                                              Id = e.Id,
                                                              SolutionName = e.SolutionName,
                                                              Content = e.Content,
                                                              SourceEventLogId = e.SourceEventLogId
                                                          }));

            // the fifth result set includes the user-event subscriptions
            var subscriptionsR = commentsR.GetNextResult<GetSubscriptions_Result>();
            var subscriptions = new List<EventLogSubscription>();
            subscriptions.AddRange(subscriptionsR.Select(s => new EventLogSubscription { LogId = s.LogId, UserId = s.UserId }));

            #endregion

            #region read and process the criteria-qualified event details

            // populate the detailed activity data into osbideEvents
            var osbideEvents = new List<IOsbideEvent>();

            // the sixth result set, AskForHelpEvent
            var askforhelpR = subscriptionsR.GetNextResult<GetAskForHelpEvents_Result>();
            osbideEvents.AddRange(askforhelpR.Select(e => new AskForHelpEvent
                                                          {
                                                              EventDate = e.EventDate,
                                                              EventLog = new EventLog
                                                                              {
                                                                                  Id = e.EventLogId,
                                                                                  LogType = "AskForHelpEvent"
                                                                              },
                                                              EventLogId = e.EventLogId,
                                                              Id = e.Id,
                                                              SolutionName = e.SolutionName,
                                                              UserComment = e.UserComment,
                                                              Code = e.Code
                                                          }));

            // the 7th result set, BuildEvent
            var buildR = askforhelpR.GetNextResult<GetBuildEvents_Result>();
            osbideEvents.AddRange(buildR.GroupBy(e=>new { e.EventLogId, e.Id, e.EventDate, e.SolutionName })
                                        .Select(e => new BuildEvent
                                                          {
                                                              EventDate = e.Key.EventDate,
                                                              EventLog = new EventLog
                                                                             {
                                                                                 Id = e.Key.EventLogId,
                                                                                 LogType = "BuildEvent"
                                                                             },
                                                              EventLogId = e.Key.EventLogId,
                                                              Id = e.Key.Id,
                                                              SolutionName = e.Key.SolutionName,
                                                              CriticalErrorNames = e.Select(c=>c.CriticalErrorName).ToList()
                                                          }));

            // the 8th result set, ExceptionEvent
            var exceptionR = buildR.GetNextResult<GetExceptionEvents_Result>();
            osbideEvents.AddRange(exceptionR.Select(e => new ExceptionEvent
                                                          {
                                                              EventDate = e.EventDate,
                                                              EventLog = new EventLog
                                                                             {
                                                                                 Id = e.EventLogId,
                                                                                 LogType = "ExceptionEvent"
                                                                             },
                                                              EventLogId = e.EventLogId,
                                                              Id = e.Id,
                                                              SolutionName = e.SolutionName,
                                                              ExceptionAction = e.ExceptionAction,
                                                              DocumentName = e.DocumentName,
                                                              ExceptionCode = e.ExceptionCode,
                                                              ExceptionDescription = e.ExceptionDescription,
                                                              ExceptionName = e.ExceptionName,
                                                              ExceptionType = e.ExceptionType,
                                                              LineContent = e.LineContent,
                                                              LineNumber = e.LineNumber
                                                          }));

            // the 9th result set, FeedPostEvent
            var feedpostR = exceptionR.GetNextResult<GetFeedPostEvents_Result>();
            osbideEvents.AddRange(feedpostR.Select(e => new FeedPostEvent
                                                          {
                                                              EventDate = e.EventDate,
                                                              EventLog = new EventLog
                                                                             {
                                                                                 Id = e.EventLogId,
                                                                                 LogType = "FeedPostEvent"
                                                                             },
                                                              EventLogId = e.EventLogId,
                                                              Id = e.Id,
                                                              SolutionName = e.SolutionName,
                                                              Comment = e.Comment
                                                          }));

            // the 10th result set, HelpfulMarkGivenEvent
            var helpfulmarkR = feedpostR.GetNextResult<GetHelpfulMarkGivenEvents_Result>();
            //var test = helpfulmarkR.ToList();
            osbideEvents.AddRange(helpfulmarkR.Select(e => new HelpfulMarkGivenEvent
                                                          {
                                                              EventDate = e.EventDate,
                                                              EventLog = new EventLog
                                                                             {
                                                                                 Id = e.EventLogId,
                                                                                 LogType = "HelpfulMarkGivenEvent"
                                                                             },
                                                              EventLogId = e.EventLogId,
                                                              Id = e.Id,
                                                              SolutionName = e.SolutionName,
                                                              LogCommentEventId = e.LogCommentEventId
                                                          }));

            // the 11th result set, LogCommentEvent
            var logcommentR = helpfulmarkR.GetNextResult<GetLogCommentEvents_Result>();
            osbideEvents.AddRange(logcommentR.Select(e => new LogCommentEvent
                                                          {
                                                              EventDate = e.EventDate,
                                                              EventLog = new EventLog
                                                                             {
                                                                                 Id = e.EventLogId,
                                                                                 LogType = "LogCommentEvent"
                                                                             },
                                                              EventLogId = e.EventLogId,
                                                              Id = e.Id,
                                                              SolutionName = e.SolutionName,
                                                              Content = e.Content,
                                                              SourceEventLogId = e.SourceEventLogId
                                                          }));

            // the 12th result set, SubmitEvent
            var submitR = logcommentR.GetNextResult<GetSubmitEvents_Result>();
            osbideEvents.AddRange(submitR.Select(e => new SubmitEvent
                                                          {
                                                              EventDate = e.EventDate,
                                                              EventLog = new EventLog
                                                                             {
                                                                                 Id = e.EventLogId,
                                                                                 LogType = "SubmitEvent"
                                                                             },
                                                              EventLogId = e.EventLogId,
                                                              Id = e.Id,
                                                              SolutionName = e.SolutionName,
                                                              AssignmentId = e.AssignmentId
                                                          }));
            #endregion

            // build the FeedItem damain objects
            foreach (var r in logs.Where(l => l.IsResult == true))
            {
                // event comments
                var eventComments = comments.Where(c => c.SourceEventLogId == r.Id).ToList();

                // osbide event of this log
                var osbideEventO = osbideEvents.First(oe => oe.EventLogId == r.Id);

                // check if this event is LogCommentEvent or HelpfulMarkGivenEvent
                var commentO = osbideEventO as LogCommentEvent;
                var helpfulmarkO = osbideEventO as HelpfulMarkGivenEvent;

                #region handles LogCommentEvent and HelpfulMarkGivenEvent object comments

                //  LogCommentEvent's comments are consumed through source EventLog's Comments property
                if (commentO != null)
                {
                    commentO.SourceEventLog = logs.Where(cs => cs.Id == commentO.SourceEventLogId)
                                                  .Select(cs => new EventLog
                                                                      {
                                                                          Id = cs.Id,
                                                                          LogType = cs.LogType,
                                                                          SenderId = cs.SenderId,
                                                                          Sender = users.Single(u => u.Id == cs.SenderId),
                                                                          DateReceived = cs.DateReceived,
                                                                      })
                                                  .First();

                    commentO.SourceEventLog.Comments = eventComments;
                }

                //  HelpfulMarkGivenEvent's comments are consumed through LogCommentEvent's source EventLog's Comments property
                if (helpfulmarkO != null)
                {
                    helpfulmarkO.LogCommentEvent = comments.Where(cs => cs.Id == helpfulmarkO.LogCommentEventId)
                                                  .Select(cs => new LogCommentEvent
                                                  {
                                                      Id = cs.Id,
                                                      EventLogId = cs.EventLogId,
                                                      SourceEventLogId = cs.SourceEventLogId,
                                                      Content = cs.Content,
                                                      EventDate = cs.EventDate,
                                                  })
                                                  .First();

                    helpfulmarkO.LogCommentEvent.EventLog = logs.Where(cs => cs.Id == helpfulmarkO.LogCommentEvent.EventLogId)
                                                 .Select(cs => new EventLog
                                                 {
                                                     Id = cs.Id,
                                                     LogType = cs.LogType,
                                                     SenderId = cs.SenderId,
                                                     Sender = users.Single(u => u.Id == cs.SenderId),
                                                     DateReceived = cs.DateReceived,
                                                 })
                                                 .First();

                    helpfulmarkO.LogCommentEvent.SourceEventLog = logs.Where(cs => cs.Id == helpfulmarkO.LogCommentEvent.SourceEventLogId)
                                                 .Select(cs => new EventLog
                                                 {
                                                     Id = cs.Id,
                                                     LogType = cs.LogType,
                                                     SenderId = cs.SenderId,
                                                     Sender = users.Single(u => u.Id == cs.SenderId),
                                                     DateReceived = cs.DateReceived,
                                                 })
                                                 .First();

                    helpfulmarkO.LogCommentEvent.SourceEventLog.Comments = eventComments;
                }

                #endregion

                // instantiate a log object
                var log = new EventLog
                                {
                                    Id = r.Id,
                                    LogType = r.LogType,
                                    SenderId = r.SenderId,
                                    Sender = users.Single(u => u.Id == r.SenderId),
                                    DateReceived = r.DateReceived,
                                };

                // assign the event comment source
                foreach (var c in eventComments)
                {
                    c.SourceEventLog = log;
                }
                log.Comments = eventComments;

                // create a FeedItem instance
                var feedItem = new FeedItem
                {
                    LogId = r.Id,
                    Log = log,
                    EventId = osbideEventO.Id,
                    Event = (commentO ?? (helpfulmarkO ?? osbideEventO)),
                    Comments = eventComments,
                    HelpfulComments = eventComments.Count,
                };

                // inherited from original osbide code
                // in case dup logs having exactly the same time tag
                while (feedItems.ContainsKey(feedItem.Log.DateReceived))
                {
                    feedItem.Log.DateReceived = feedItem.Log.DateReceived.AddMilliseconds(1);
                }
                // add the feeditem to the ordered dictionary
                feedItems.Add(feedItem.Log.DateReceived, feedItem);
            }

            return feedItems.Values.Reverse().ToList();
        }
    }
}
