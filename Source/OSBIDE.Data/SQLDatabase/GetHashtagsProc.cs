using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class GetHashtagsProc
    {
        public static List<Tag> Run(string query, bool isHandle)
        {
            using (var context = new OsbideProcs())
            {
                return context.GetHashtags(query, isHandle).Select(t => new Tag { Id = t.Id, Name = t.Tag, IsHandle = t.IsHandle }).ToList();
            }
        }

        public static List<TrendAndNotification> GetTrendAndNotification(int userId, int topN, bool getAll)
        {
            using (var context = new OsbideProcs())
            {
                return context.GetTrendsAndNotifications(userId, topN, getAll)
                                .Select(t => new TrendAndNotification
                                {
                                    FirstName = t.FirstName,
                                    LastName = t.LastName,
                                    Viewed = t.Viewed,
                                    UserId = t.UserId,
                                    EventLogId = t.EventLogId,
                                    HashtagId =t.HashtagId,
                                    Hashtag=t.Hashtag,
                                    HashtagCounts=t.Counts,
                                })
                                .ToList();
            }
        }
    }
}
