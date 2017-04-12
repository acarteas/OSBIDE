using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public static class CommentsProc
    {
        public static IEnumerable<Comment> Get(string logIds, int currentUserId)
        {
            using (var context = new OsbideProcs())
            {
                return ReadResults(context.GetComments(logIds, currentUserId));
            }
        }

        private static IEnumerable<Comment> ReadResults(IEnumerable<GetComments_Result> resultsR)
        {
            var results = new List<Comment>();
            results.AddRange(resultsR.Select(e => new Comment
                                                      {
                                                          CommentId = e.CommentId.Value,
                                                          OriginalId = e.OriginalId.Value,
                                                          ActualId = e.ActualId.Value,
                                                          CourseNumber = e.CourseNumber,
                                                          CoursePrefix = e.CoursePrefix,
                                                          Content = e.Content,
                                                          LastName = e.LastName,
                                                          FirstName = e.FirstName,
                                                          SenderId = e.SenderId.Value,
                                                          EventDate = e.EventDate.Value,
                                                          HelpfulMarkCounts = e.HelpfulMarkCounts.Value,
                                                          IsHelpfulMarkSender = e.IsHelpfulMarkSender.Value,

                                                      }).ToList());

            return results;
        }
    }
}
