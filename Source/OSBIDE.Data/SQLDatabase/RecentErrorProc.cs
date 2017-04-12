using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.SQLDatabase.Edmx;
using OSBIDE.Library.Models;

namespace OSBIDE.Data.SQLDatabase
{
    public static class RecentErrorProc
    {
        public static IEnumerable<ErrorListItem> Get(int? senderId, DateTime? timeframe)
        {
            using (var context = new OsbideProcs())
            {
                return ReadResults(context.GetRecentCompileErrors(senderId, timeframe));
            }
        }

        private static IEnumerable<ErrorListItem> ReadResults(IEnumerable<GetRecentCompileErrors_Result> resultsR)
        {
            var results = new List<ErrorListItem>();
            results.AddRange(resultsR.Select(e => new ErrorListItem
                                                      {
                                                          Id=e.Id,
                                                          Column=e.Column,
                                                          Description=e.Description,
                                                          File=e.File,
                                                          Line=e.Line,
                                                          Project=e.Project,
                                                      }).ToList());

            return results;
        }
    }
}
