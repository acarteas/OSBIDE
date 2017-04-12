using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Library.Models;

namespace OSBIDE.Web.Models.Queries
{
    public class BuildErrorQuery : ActivityFeedQuery, IOsbideQuery<FeedItem>
    {
        private readonly OsbideContext _db ;
        public int BuildErrorTypeId { get; set; }

        public BuildErrorQuery(OsbideContext db)
        {
            _db = db;
        }

        public override IEnumerable<FeedItem> Execute()
        {
            if (BuildErrorTypeId <= 0) return new List<FeedItem>();

            var query = from error in _db.BuildErrors
                join log in _db.EventLogs on error.LogId equals log.Id
                join be in _db.BuildEvents on log.Id equals be.EventLogId
                join comm in _db.LogCommentEvents on log.Id equals comm.SourceEventLogId into logComments
                where log.DateReceived >= StartDate
                      && log.DateReceived <= EndDate
                      && log.Id > MinLogId
                      && error.BuildErrorTypeId == BuildErrorTypeId
                orderby log.DateReceived descending
                select new
                {
                    Log = log,
                    BuildEvent = be,
                    Comments = logComments,
                    HelpfulMarks = (from helpful in _db.HelpfulMarkGivenEvents
                        where logComments.Select(l => l.Id).Contains(helpful.LogCommentEventId)
                        select helpful).Count()
                };

            //were we supplied with a maximum ID number?
            if (MaxLogId > 0)
            {
                query = from q in query
                    where q.Log.Id < MaxLogId
                    select q;
            }

            //if we were asked to retrieve a certain list of events, add that into the query
            if (EventIds.Count > 0)
            {
                query = from q in query
                    where EventIds.Contains(q.Log.Id)
                    select q;
            }

            //get list of subscription ids
            var subjectIds = SubscriptionSubjects.Select(s => s.Id).ToArray();
            if (subjectIds.Length > 0)
            {
                query = from q in query
                    where subjectIds.Contains(q.Log.SenderId)
                    select q;
            }

            //did we only want a certain number of results returned?
            if (MaxQuerySize > 0)
            {
                query = query.Take(MaxQuerySize);
            }
                
            //finally, loop through the query and build our results
            return query.Select(row => new FeedItem
            {
                LogId = row.Log.Id,
                Log = row.Log,
                Comments = row.Comments.ToList(),
                HelpfulComments = row.HelpfulMarks,
                EventId = row.BuildEvent.Id,
                Event = row.BuildEvent
            })
            .ToList();
        }
    }
}