using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using OSBIDE.Data.NoSQLStorage;

namespace OSBIDE.Data.DomainObjects
{
    public static class DomainObjectHelpers
    {
        public static void LogActionRequest(ActionRequestLog log)
        {
            using (var storage = new ActionRequestLogStorage())
            {
                var utc = DateTime.UtcNow;
                var entity = new ActionRequestLogEntry
                {
                    PartitionKey = log.SchoolId.ToString(CultureInfo.InvariantCulture),
                    RowKey = string.Format("{0}_{1}_{2}_{3}_{4}_{5}",
                                            log.CreatorId.ToString(CultureInfo.InstalledUICulture),
                                            utc.ToLongDateString(),
                                            utc.ToLongTimeString(),
                                            log.ControllerName,
                                            log.ActionName,
                                            log.ActionParameters),
                    ControllerName = log.ControllerName,
                    ActionParameters = log.ActionParameters,
                    ActionName = log.ActionName,
                    AccessDate = log.AccessDate,
                    IpAddress = log.IpAddress,
                    CreatorId=log.CreatorId,
                };
                storage.Insert(entity);
            }
        }

        public static IEnumerable<ActionRequestLog> GetActionRequests(int schoolId, int studentId)
        {
            using (var storage = new ActionRequestLogStorage())
            {
                return storage.Select(schoolId.ToString(CultureInfo.InstalledUICulture), studentId.ToString(CultureInfo.InstalledUICulture))
                       .Select(log => new ActionRequestLog
                       {
                           SchoolId = Convert.ToInt32(log.PartitionKey),
                           CreatorId = Convert.ToInt32(log.RowKey.Split('_')[0]),
                           // from db context to populate the Creator = creator,
                           AccessDate = log.AccessDate,
                           ControllerName = log.ControllerName,
                           ActionName = log.ActionName,
                           ActionParameters = log.ActionParameters,
                           IpAddress = log.IpAddress,
                       });
            }
        }

        internal static IEnumerable<ActionRequestLogEntry> GetPassiveSocialActivities(string tableName, int schoolId)
        {
            using (var storage = new ActionRequestLogStorage())
            {
                return storage.Select(tableName, schoolId.ToString(CultureInfo.InstalledUICulture)).ToList();
            }
        }
    }
}
