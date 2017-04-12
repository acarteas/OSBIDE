using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class HelpfulMarkGivenEvent : IOsbideEvent, IModelBuilderExtender
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int EventLogId { get; set; }

        [ForeignKey("EventLogId")]
        public virtual EventLog EventLog { get; set; }

        [Required]
        public int LogCommentEventId { get; set; }

        [ForeignKey("LogCommentEventId")]
        public virtual LogCommentEvent LogCommentEvent { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string SolutionName { get; set; }

        [Required]
        public string EventName { get { return HelpfulMarkGivenEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "HelpfulMarkGivenEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Helpful Mark"; } }

        public HelpfulMarkGivenEvent()
        {
            SolutionName = "OSBIDE";
            EventDate = DateTime.UtcNow;
        }

        public HelpfulMarkGivenEvent(HelpfulMarkGivenEvent other)
        {
            Id = other.Id;
            EventLogId = other.EventLogId;
            EventLog = other.EventLog;
            LogCommentEventId = other.LogCommentEventId;
            LogCommentEvent = other.LogCommentEvent;
            EventDate = other.EventDate;
            SolutionName = other.SolutionName;
        }

        public IOsbideEvent FromDict(Dictionary<string, object> values)
        {
            HelpfulMarkGivenEvent evt = new HelpfulMarkGivenEvent();
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
            if (values.ContainsKey("LogCommentEvent"))
            {
                evt.LogCommentEvent = (LogCommentEvent)values["LogCommentEvent"];
            }
            if (values.ContainsKey("LogCommentEventId"))
            {
                evt.LogCommentEventId = (int)values["LogCommentEventId"];
            }
            return evt;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
        }
    }
}
