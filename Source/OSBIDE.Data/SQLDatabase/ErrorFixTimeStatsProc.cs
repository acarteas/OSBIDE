using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class ErrorFixTimeStatsProc
    {
        public static List<ErrorFixTimeStats> Get()
        {
            using (var context = new OsbideProcs())
            {
                return (from e in context.GetErrorFixTimeStats()
                        select new ErrorFixTimeStats
                        {
                            ErrorTypeId = e.ErrorTypeId,
                            Mean = e.Mean,
                            SD = e.StandardDeviation,
                            SDP = e.PopulationStandardDeviation,
                        }).ToList();
            }
        }
    }
}
