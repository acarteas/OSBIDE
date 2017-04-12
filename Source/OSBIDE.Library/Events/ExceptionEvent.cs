using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class ExceptionEvent : IOsbideEvent
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
        public string EventName { get { return ExceptionEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "ExceptionEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Exception"; } }

        [Required]
        public string ExceptionType { get; set; }

        [Required]
        public string ExceptionName { get; set; }

        [Required]
        public int ExceptionCode { get; set; }

        [Required]
        public string ExceptionDescription { get; set; }

        [Required]
        public int ExceptionAction { get; set; }

        [Required]
        public string DocumentName { get; set; }

        [Required]
        public int LineNumber { get; set; }

        [Required]
        public string LineContent { get; set; }

        public virtual IList<StackFrame> StackFrames { get; set; }

        public ExceptionEvent()
        {
            EventDate = DateTime.UtcNow;
            StackFrames = new List<StackFrame>();
        }

        IOsbideEvent IOsbideEvent.FromDict(Dictionary<string, object> values)
        {
            ExceptionEvent evt = new ExceptionEvent();
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
            if (values.ContainsKey("ExceptionType"))
            {
                evt.ExceptionType = values["ExceptionType"].ToString();
            }
            if (values.ContainsKey("ExceptionName"))
            {
                evt.ExceptionName = values["ExceptionName"].ToString();
            }
            if (values.ContainsKey("ExceptionCode"))
            {
                evt.ExceptionCode = (int)values["ExceptionCode"];
            }
            if (values.ContainsKey("ExceptionDescription"))
            {
                evt.ExceptionDescription = values["ExceptionDescription"].ToString();
            }
            if (values.ContainsKey("ExceptionAction"))
            {
                evt.ExceptionAction = (int)values["ExceptionAction"];
            }
            if (values.ContainsKey("DocumentName"))
            {
                evt.DocumentName = values["DocumentName"].ToString();
            }
            if (values.ContainsKey("LineNumber"))
            {
                evt.LineNumber = (int)values["LineNumber"];
            }
            if (values.ContainsKey("LineContent"))
            {
                evt.LineContent = values["LineContent"].ToString();
            }
            if (values.ContainsKey("StackFrames"))
            {
                evt.StackFrames = values["StackFrames"] as List<StackFrame>;
            }
            return evt;
        }

    }
}
