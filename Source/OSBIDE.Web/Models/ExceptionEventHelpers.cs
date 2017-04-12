using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models
{
    public static class ExceptionEventHelpers
    {
        /// <summary>
        /// Will return the build event associated with the given exception event.
        /// </summary>
        /// <param name="exEvent"></param>
        /// <returns></returns>
        public static List<CodeDocument> GetCodeDocuments(this ExceptionEvent exEvent)
        {
            List<CodeDocument> docs = new List<CodeDocument>();
            using (OsbideContext db = OsbideContext.DefaultWebConnection)
            {
                var query = from log in db.EventLogs
                            join be in db.BuildEvents on log.Id equals be.EventLogId
                            where log.Id < exEvent.EventLogId
                            && log.LogType == BuildEvent.Name
                            orderby log.Id descending
                            select be;
                var result = query.Take(1);
                BuildEvent build = result.FirstOrDefault();
                if (build != null)
                {
                    foreach (BuildDocument doc in build.Documents)
                    {
                        docs.Add(doc.Document);
                    }
                }

            }
            return docs;
        }

        /// <summary>
        /// Returns the build event that is related to this exception.  Note that the underlying EF DB connection
        /// will be closed at the end of this function call, thereby preventing access to some BuildEvent member objects.
        /// </summary>
        /// <param name="exEvent"></param>
        /// <returns></returns>
        public static BuildEvent GetBuildEvent(this ExceptionEvent exEvent)
        {
            BuildEvent evt = new BuildEvent();
            using (OsbideContext db = OsbideContext.DefaultWebConnection)
            {
                var query = from log in db.EventLogs
                            join be in db.BuildEvents on log.Id equals be.EventLogId
                            where log.Id < exEvent.EventLogId
                            && log.LogType == BuildEvent.Name
                            orderby log.Id descending
                            select be;
                var result = query.Take(1);
                evt = result.FirstOrDefault();
            }
            return evt;
        }
    }
}