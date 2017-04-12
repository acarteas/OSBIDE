using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    public interface IOsbideEvent
    {
        int Id { get; set; }
        int EventLogId { get; set; }
        EventLog EventLog { get; set; }
        DateTime EventDate { get; set; }
        string SolutionName { get; set; }
        string EventName { get; }
        string PrettyName { get; }
        IOsbideEvent FromDict(Dictionary<string, object> values);
    }
}
