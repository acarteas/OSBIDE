using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.Edmx;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.StoredProcs
{
    public class EventLogTypeProc
    {
        public List<EventLogType> Get()
        {
            using (var context = new OsbideProcs())
            {
                return (from e in context.GetEventTypes()
                        select new EventLogType
                        {
                            LogTypeId = (EventType)e.LogTypeId,
                            LogyTypeName = e.LogTypeName,
                            DisplayName = e.DisplayName,
                            IsSocial = e.IsSocial,
                            IsIDE = e.IsIDE,
                            IsFeed = e.IsFeed,
                        }).ToList();
            }
        }
    }
}
