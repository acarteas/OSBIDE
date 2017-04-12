using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Data.SQLDatabase.DataAnalytics
{
    public class ErrorQuotientAnalytics
    {
        public static List<ScoringResult> GetResults(ErrorQuotientParams eparams, DateTime? dateFrom, DateTime? dateTo, List<int> users)
        {
            var results = new List<ScoringResult>();

            if (users != null && users.Count > 0)
            {
                var eventSessions = BuildEventSessionDataProc.Get(dateFrom, dateTo, users);

                foreach (var u in users)
                {
                    if (!results.Any(r => r.UserId == u))
                    {
                        results.Add(new ScoringResult
                        {
                            UserId = u,
                            Score = ErrorQuotient.Calculate(eparams, eventSessions.Where(e => e.UserId == u))
                        });
                    }
                }
            }

            return results;
        }
    }
}
