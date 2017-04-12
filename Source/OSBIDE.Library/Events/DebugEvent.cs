using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Events
{

    [Serializable]
    public class DebugEvent : IOsbideEvent
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int EventLogId { get; set; }

        [ForeignKey("EventLogId")]
        public virtual EventLog EventLog { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string SolutionName { get; set; }

        [Required]
        public int ExecutionAction { get; set; }

        [Required]
        public string DocumentName { get; set; }

        /// <summary>
        /// If the event was a Step (over, into, out of), will track the line number
        /// on which the event took place.
        /// </summary>
        [Required]
        public int LineNumber { get; set; }

        /// <summary>
        /// Contains information in the debug output window.  As the output window is cumulative, there's no
        /// reason to set this unless you're dealing with a "StopDebugging" event
        /// </summary>
        [Required(AllowEmptyStrings=true)]
        public string DebugOutput { get; set; }

        [Required]
        public string EventName { get { return DebugEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "DebugEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Debug"; } }

        public DebugEvent()
        {
            EventDate = DateTime.UtcNow;
            DebugOutput = "";
            LineNumber = -1;
        }

        IOsbideEvent IOsbideEvent.FromDict(Dictionary<string, object> values)
        {
            DebugEvent evt = new DebugEvent();
            if (values.ContainsKey("Id"))
            {
                evt.Id = (int)values["Id"];
            }
            if (values.ContainsKey("EventLogId"))
            {
                evt.EventLogId = (int)values["EventLogId"];
            }
            if (values.ContainsKey("EventLog"))
            {
                evt.EventLog = (EventLog)values["EventLog"];
            }
            if (values.ContainsKey("EventDate"))
            {
                evt.EventDate = (DateTime)values["EventDate"];
            }
            if (values.ContainsKey("SolutionName"))
            {
                evt.SolutionName = values["SolutionName"].ToString();
            }
            if (values.ContainsKey("ExecutionAction"))
            {
                evt.ExecutionAction = (int)values["ExecutionAction"];
            }
            if (values.ContainsKey("DocumentName"))
            {
                evt.DocumentName = values["DocumentName"].ToString();
            }
            if (values.ContainsKey("LineNumber"))
            {
                evt.LineNumber = (int)values["LineNumber"];
            }
            if (values.ContainsKey("DebugOutput"))
            {
                evt.DebugOutput = values["DebugOutput"].ToString();
            }
            return evt;
        }
    }
}
