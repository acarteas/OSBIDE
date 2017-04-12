using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class BuildEventSessionDataProc
    {
        /// <summary>
        /// build a list of qualified error quotient events with error types and documents
        /// </summary>
        /// <param name="dateFrom"></param>
        /// <param name="dateTo"></param>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public static List<BuildErrorEvent> Get(DateTime? dateFrom, DateTime? dateTo, IEnumerable<int> userIds, bool includeErrorFixInfo = false)
        {
            using (var context = new OsbideProcs())
            {
                #region filter qualified build error event for the selected users
                // filter qualified build error event for the selected users
                var minDate = new DateTime(2000, 1, 1);
                var events = (from e in context.GetErrorQuotientSessionData((!dateFrom.HasValue || dateFrom.Value < minDate) ? minDate : dateFrom,
                                                                            (!dateTo.HasValue || dateTo.Value < minDate) ? DateTime.Today : dateTo,
                                                                            string.Join(",", userIds))
                              select new BuildErrorEvent
                                         {
                                             BuildId = e.BuildId,
                                             LogId = e.LogId,
                                             UserId = e.SenderId,
                                             EventDate = e.EventDate,
                                         }).ToList();
                #endregion

                var eventLogIds = string.Join(",", events.Select(e => e.LogId));

                // error types for the resultant build error events
                var errorTypes = GetErrorTypes(context, eventLogIds, includeErrorFixInfo);

                #region error messages for the resultant build error events
                // error messages for the resultant build error events
                var errorMessages = new List<GetBuildErrorMessages_Result>();
                if (includeErrorFixInfo)
                {
                    errorMessages = (from e in context.GetBuildErrorMessages(eventLogIds) select e).ToList();
                }
                #endregion

                // error documents for the resultant build error events
                var errorDocs = (from d in context.GetErrorQuotientDocumentData(string.Join(",", events.Select(e => e.BuildId))) select d).ToList();

                #region associate error types and documents to the build error events
                // associate error types and documents to the build error events
                foreach (var e in events)
                {
                    // error types
                    var et = errorTypes.Where(t => t.LogId == e.LogId).ToList();
                    if (et.Count > 0)
                    {
                        e.ErrorTypes = et;
                    }

                    // error messages
                    var em = errorMessages.Where(m => m.LogId == e.LogId).Select(m => m.ErrorMessage).ToList();
                    if (em.Count > 0)
                    {
                        e.ErrorMessages = em;
                    }

                    // error documents
                    var ed = errorDocs.Where(d => d.BuildId == e.BuildId)
                                      .Select(d => new ErrorDocumentInfo
                                      {
                                          DocumentId = d.DocumentId,
                                          Line = d.Line,
                                          Column = d.Column,
                                          FileName = d.FileName,
                                          NumberOfModified = d.NumberOfModified != null && d.NumberOfModified.HasValue ? d.NumberOfModified.Value : 0,
                                          ModifiedLines = d.ModifiedLines != null && d.ModifiedLines.Length > 0 ? d.ModifiedLines.Split(',').Select(l => Convert.ToInt32(l)).ToList() : null
                                      })
                                      .ToList();
                    if (ed.Count > 0)
                    {
                        e.Documents = ed;
                    }
                }
                #endregion

                return events;
            }
        }

        private static List<ErrorTypeDetails> GetErrorTypes(OsbideProcs context, string eventLogIds, bool includeErrorFixInfo)
        {
            if (includeErrorFixInfo)
            {
                return (from e in context.GetWatwinScoringErrorTypeData(eventLogIds)
                        select new ErrorTypeDetails
                        {
                            LogId = e.LogId,
                            ErrorTypeId = e.BuildErrorTypeId,
                            ErrorType = e.BuildErrorType,
                            FixingTime = e.TimeToFix.HasValue ? e.TimeToFix.Value : 2592000, /*30 days in seconds*/
                        }).ToList();
            }

            return (from e in context.GetErrorQuotientErrorTypeData(eventLogIds)
                    select new ErrorTypeDetails
                    {
                        LogId = e.LogId,
                        ErrorTypeId = e.ErrorTypeId,
                    }).ToList();
        }
    }
}
