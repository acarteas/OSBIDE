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
    public class SaveEvent : IOsbideEvent
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
        public string EventName { get { return SaveEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "SaveEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Save"; } }

        [Required]
        public int DocumentId { get; set; }

        [Required]
        [ForeignKey("DocumentId")]
        public virtual CodeDocument Document { get; set; }

        IOsbideEvent IOsbideEvent.FromDict(Dictionary<string, object> values)
        {
            SaveEvent evt = new SaveEvent();
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
            if (values.ContainsKey("DocumentId"))
            {
                evt.DocumentId = (int)values["DocumentId"];
            }
            if (values.ContainsKey("Document"))
            {
                evt.Document = (CodeDocument)values["Document"];
            }
            return evt;
        }

        public SaveEvent()
        {
            EventDate = DateTime.UtcNow;
        }
    }
}
