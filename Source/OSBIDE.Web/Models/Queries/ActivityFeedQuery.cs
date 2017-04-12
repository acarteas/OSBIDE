using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;

namespace OSBIDE.Web.Models.Queries
{
    public class ActivityFeedQuery : IOsbideQuery<FeedItem>
    {
        private readonly List<EventTypes> _eventSelectors = new List<EventTypes>();
        protected List<OsbideUser> SubscriptionSubjects = new List<OsbideUser>();
        protected readonly List<int> EventIds = new List<int>();

        public ActivityFeedQuery()
        {
            StartDate = new DateTime(2010, 1, 1);
            EndDate = DateTime.Today.AddDays(3);
            CommentFilter = string.Empty;
            MinLogId = -1;
            MaxLogId = -1;
            MaxQuerySize = 20;
            CourseRoleFilter = CourseRole.Student;
            CourseFilter = new Course() { Id = -1 };
        }

        /// <summary>
        /// Sets a limit on the newest post to be retrieved.  Example: if <see cref="EndDate"/> is set to
        /// 2010-01-01, no posts after 2010-01-01 will be retrieved.
        /// </summary>
        public DateTime EndDate { get; private set; }

        /// <summary>
        /// Sets a limit on the oldest post to be retrieved.  Example: if <see cref="StartDate"/> is set to
        /// 2010-01-01, no posts before 2010-01-01 will be retrieved.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Used to set a floor on the logs to retrieve.  Example: if <see cref="MinLogId"/> is set to 5,
        /// no posts with an Id less than 6 will be retrieved.
        /// </summary>
        public int MinLogId { protected get; set; }

        /// <summary>
        /// Used to set a ceiling on the logs to retrieve.  Example: if <see cref="MaxLogId"/> is set to 5,
        /// no posts with an Id greater than 4 will be retrieved.
        /// </summary>
        public int MaxLogId { protected get; set; }

        /// <summary>
        /// Used to limit the number of query results.  Default of -1 means to return all results.
        /// </summary>B
        public int MaxQuerySize { protected get; set; }

        /// <summary>
        /// Used to select posts made by only certain users.  This works by restricting posts below 
        /// the supplied threshold.  E.g. CourseRole.Student will select everyone whereas 
        /// CourseRole.Coordinator will only select course coordinators.
        /// </summary>
        public CourseRole CourseRoleFilter { private get; set; }

        /// <summary>
        /// Used to select only posts made by students in a given course.  Default value is all
        /// courses.
        /// </summary>
        public Course CourseFilter { private get; set; }

        /// <summary>
        /// Comment search token entered by the user
        /// </summary>
        public string CommentFilter { private get; set; }

        /// <summary>
        /// returns a lits of all social events in OSBLE
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EventTypes> GetSocialEvents()
        {
            return new List<EventTypes>
            {
                EventTypes.FeedPostEvent,
                EventTypes.AskForHelpEvent,
                EventTypes.LogCommentEvent,
                EventTypes.HelpfulMarkGivenEvent,
                EventTypes.SubmitEvent,
            };
        }

        /// <summary>
        /// returns a list of IDE-based events in OSBLE
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EventTypes> GetIdeEvents()
        {
            return new List<EventTypes>
            {
                EventTypes.BuildEvent,
                EventTypes.ExceptionEvent,
            };
        }

        /// <summary>
        /// returns a list of all possible events that a user can subscribe to
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<EventTypes> GetAllEvents()
        {
            return GetIdeEvents().Concat(GetSocialEvents());
        }

        /// <summary>
        /// add user selected event types
        /// </summary>
        /// <param name="evt"></param>
        public void AddEventType(EventTypes evt)
        {
            if (_eventSelectors.All(e => e != evt))
            {
                _eventSelectors.Add(evt);
            }
        }

        /// <summary>
        /// get user selected event types
        /// </summary>
        public List<EventTypes> ActiveEvents
        {
            get
            {
                return _eventSelectors.ToList();
            }
        }

        /// <summary>
        /// add user subscriptions
        /// </summary>
        /// <param name="user"></param>
        public void AddSubscriptionSubject(OsbideUser user)
        {
            if (user != null)
            {
                SubscriptionSubjects.Add(user);
            }
        }

        /// <summary>
        /// combine current and the new user subscriptions
        /// </summary>
        /// <param name="users"></param>
        public void AddSubscriptionSubject(IEnumerable<OsbideUser> users)
        {
            SubscriptionSubjects = SubscriptionSubjects.Union(users).ToList();
        }

        /// <summary>
        /// clear user subscriptions
        /// </summary>
        public void ClearSubscriptionSubjects()
        {
            SubscriptionSubjects = new List<OsbideUser>();
        }

        /// <summary>
        /// add sepecific event id for query filter
        /// </summary>
        /// <param name="id"></param>
        public void AddEventId(int id)
        {
            EventIds.Add(id);
        }

        /// <summary>
        /// execute the query
        /// </summary>
        /// <returns></returns>
        public virtual IEnumerable<FeedItem> Execute()
        {
            return EventLogsProc.GetActivityFeeds( StartDate
                                    , EndDate
                                    , EventIds.Select(eid => (int)eid).ToList()
                                    , _eventSelectors.Select(e => (int)e)
                                    , CourseFilter != null && CourseFilter.Id > 0 ? CourseFilter.Id : 0
                                    , (int)CourseRoleFilter
                                    , CommentFilter
                                    , SubscriptionSubjects.Select(s => s.Id).ToList()
                                    , MinLogId
                                    , MaxLogId
                                    , 0
                                    , MaxQuerySize);

        }
    }
}