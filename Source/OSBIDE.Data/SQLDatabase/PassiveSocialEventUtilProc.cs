using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;
using OSBIDE.Data.NoSQLStorage;

namespace OSBIDE.Data.SQLDatabase
{
    public class PassiveSocialEventUtilProc
    {
        private const int BATCH_SIZE = 100;
        private const string DESTINATIONTABLE = "[PassiveSocialEvents]";

        private static Dictionary<string, string> EVENT_CODE = new Dictionary<string, string>
        {
            {"assignment_downloadstudentassignment", "AD"},
			{"buildEvent_diff", "BD"},
			{"course_details", "CD"},
			{"course_index", "CI"},
			{"course_makedefault", "CM"},
			{"course_search", "CS"},
			{"feed_applyfeedfilter", "FA"},
			{"feed_details", "FD"},
			{"feed_followpost", "FF"},
			{"feed_markcommenthelpful", "FM"},
			{"feed_oldfeeditems", "FO"},
			{"feed_unfollowpost", "FU"},
			{"file_getassignmentattachment", "FA"},
			{"file_getcoursedocument", "FC"},
			{"profile_edit", "PE"},
			{"profile_index", "PI"},
        };

        public static bool Run(int schoolId)
        {
            using (var context = new OsbideProcs())
            {
                // storage tables not completely processed
                foreach (var table in context.GetPassiveSocialEventProcessLog())
                {
                    var passiveSocialEvents = DomainObjectHelpers.GetPassiveSocialActivities(table.SourceTableName, schoolId).ToList();
                    var totalCounts = passiveSocialEvents.Count;

                    var processedCounts = table.ProcessedRecordCounts.HasValue ? table.ProcessedRecordCounts.Value : 0;
                    var batches = (totalCounts - processedCounts) / BATCH_SIZE;
                    for (var b = 0; b < batches + 1; b++)
                    {
                        var tsql = new StringBuilder();

                        var firstRow = processedCounts + b * BATCH_SIZE;
                        var lastRow = processedCounts + (b + 1) * BATCH_SIZE > totalCounts ? totalCounts : processedCounts + (b + 1) * BATCH_SIZE;

                        for (var idx = firstRow; idx < lastRow; idx++)
                        {
                            var sql = GetSQLRowStatement(passiveSocialEvents[idx]);
                            if (sql.Length > 0)
                            {
                                // build batch insert statements
                                tsql.AppendFormat("{0}{1}", sql, Environment.NewLine);
                            }
                        }

                        // batch execution and log updates
                        DynamicSQLExecutor.Execute(tsql.ToString());
                        context.UpdatePassiveSocialEventProcessLog(table.Id, DESTINATIONTABLE, lastRow == totalCounts, lastRow);
                    }
                }

                return true;
            }
        }

        private static string GetSQLRowStatement(ActionRequestLogEntry logEntry)
        {
            if (Convert.ToInt32(logEntry.CreatorId) > 0)
            {
                var paramTokens = string.IsNullOrWhiteSpace(logEntry.ActionParameters)
                                ? null
                                : logEntry.ActionParameters.Split("|||".ToCharArray()).Where(pt => pt.Length > 0).Select(pt => pt).ToArray();

                var eventCodeDictionaryKey = string.Format("{0}_{1}", logEntry.ControllerName.ToLower(), logEntry.ActionName.ToLower());
                var eventCode = EVENT_CODE.Keys.Contains(eventCodeDictionaryKey) ? EVENT_CODE[eventCodeDictionaryKey] : string.Empty;
                if (paramTokens != null && string.IsNullOrWhiteSpace(eventCode))
                {
                    if (string.Compare(logEntry.ControllerName, "Feed", true) == 0)
                    {
                        eventCode = GetFeedEventCode(logEntry.ActionName, paramTokens);
                    }
                }

                if (!string.IsNullOrWhiteSpace(eventCode))
                {
                    var param1 = paramTokens == null ? "null" : string.Format("'{0}'", paramTokens[0]);
                    var param2 = paramTokens == null || paramTokens.Length < 2 ? "null" : string.Format("'{0}'", paramTokens[1]);
                    var param3 = paramTokens == null || paramTokens.Length < 3 ? "null" : string.Format("'{0}'", paramTokens[2]);

                    return SQLTemplatePassiveSocialEvents.Insert
                                                         .Replace("DESTINATIONTABLE", DESTINATIONTABLE)
                                                         .Replace("USERID", logEntry.CreatorId.ToString())
                                                         .Replace("CONTROLLERNAME", logEntry.ControllerName)
                                                         .Replace("ACTIONNAME", logEntry.ActionName)
                                                         .Replace("ACTIONPARAM1", param1)
                                                         .Replace("ACTIONPARAM2", param2)
                                                         .Replace("ACTIONPARAM3", param3)
                                                         .Replace("ACCESSDATE", logEntry.AccessDate.ToString())
                                                         .Replace("ACTIONPARAMS", string.IsNullOrWhiteSpace(logEntry.ActionParameters) ? "null" : string.Format("'{0}'", logEntry.ActionParameters))
                                                         .Replace("EVENTCODE", eventCode);
                }
            }

            return string.Empty;
        }

        private static string GetFeedEventCode(string actionName, string[] paramTokens)
        {
            var eventCode = string.Empty;

            if (string.Compare(actionName, "Index", true) == 0 && paramTokens.Length > 3)
            {
                // params timestamp=-1|||errorType=-1|||errorTypeStr=|||keyword=|||
                var errorType = Convert.ToInt32(paramTokens[1].Split('=')[1]);
                var errorString = paramTokens[2].Split('=')[1];
                var searchString = string.IsNullOrWhiteSpace(paramTokens[3]) || paramTokens[3].Split('=').Length < 2 ? string.Empty : paramTokens[3].Split('=')[1];
                if (errorType < 0 && errorString.Length == 0)
                {
                    eventCode = "FI";
                }
                else if (errorType > 0)
                {
                    eventCode = "FEO";
                }
                else if (errorString.Length > 0)
                {
                    eventCode = "FEW";
                }
                else if (searchString.Length > 0)
                {
                    // keyword search???
                    eventCode = "FS";
                }
            }
            else if (string.Compare(actionName, "GetComments", true) == 0 && paramTokens.Length > 1)
            {
                if (paramTokens[0].Split('=')[1] != "[null]")
                {
                    eventCode = "GC";
                }
            }

            return eventCode;
        }
    }
}
