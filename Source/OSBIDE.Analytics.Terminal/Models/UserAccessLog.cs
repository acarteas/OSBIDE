using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Models
{
    [Serializable]
    public class UserAccessLog
    {
        public OsbideUser User { get; set; }
        
        public Dictionary<DateTime, List<ActionRequestLog>> Logs { get; set; }

        public UserAccessLog()
        {
            Logs = new Dictionary<DateTime, List<ActionRequestLog>>();
        }

        public void AddRecord(DateTime time, ActionRequestLog log)
        {
            DateTime truncated = time.Date;
            if(Logs.ContainsKey(truncated) == false)
            {
                Logs.Add(truncated, new List<ActionRequestLog>());
            }
            Logs[truncated].Add(log);
        }

        public override int GetHashCode()
        {
            return User.Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if(obj is OsbideUser)
            {
                return User.Id.Equals((obj as OsbideUser).Id);
            }
            else
            {
                return base.Equals(obj);
            }
        }
    }
}
