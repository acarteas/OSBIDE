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
    public class SolutionDownloadEvent : IOsbideEvent
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
        public int AssignmentId { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual Assignment Assignment { get; set; }

        [Required]
        public int DownloadingUserId { get; set; }

        [Required]
        public int AuthorId { get; set; }
        
        [Required]
        public string EventName { get { return SolutionDownloadEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "SolutionDownloadEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Solution Download"; } }

        public SolutionDownloadEvent()
        {
            EventDate = DateTime.UtcNow;
        }

        IOsbideEvent IOsbideEvent.FromDict(Dictionary<string, object> values)
        {
            SolutionDownloadEvent evt = new SolutionDownloadEvent();
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
            if (values.ContainsKey("AssignmentName"))
            {
                evt.AssignmentId = (int)values["AssignmentId"];
            }
            if (values.ContainsKey("DownloadingUserId"))
            {
                evt.DownloadingUserId = (int)values["DownloadingUserId"];
            }
            if (values.ContainsKey("AuthorId"))
            {
                evt.AuthorId = (int)values["AuthorId"];
            }
            return evt;
        }
    }
}
