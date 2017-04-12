using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class CutCopyPasteEvent : IOsbideEvent
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
        public int EventAction { get; set; }

        [NotMapped]
        public CutCopyPasteActions Action
        {
            get
            {
                return (CutCopyPasteActions)EventAction;
            }
            set
            {
                EventAction = (int)value;
            }
        }

        [Required(AllowEmptyStrings=true)]
        public string DocumentName { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string Content { get; set; }

        [Required]
        public string EventName { get { return CutCopyPasteEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "CutCopyPasteEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Cut, Copy, & Paste"; } }

        public CutCopyPasteEvent()
        {
            EventDate = DateTime.UtcNow;
            DocumentName = "";
            Content = "";
        }

        IOsbideEvent IOsbideEvent.FromDict(Dictionary<string, object> values)
        {
            CutCopyPasteEvent evt = new CutCopyPasteEvent();
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
            if (values.ContainsKey("EventAction"))
            {
                evt.EventAction = (int)values["EventAction"];
            }
            if (values.ContainsKey("Content"))
            {
                evt.Content = values["Content"].ToString();
            }
            return evt;
        }
    }
}
