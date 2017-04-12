using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class LogCommentEvent : IOsbideEvent, IModelBuilderExtender
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int EventLogId { get; set; }

        [ForeignKey("EventLogId")]
        public virtual EventLog EventLog { get; set; }

        [Required]
        public int SourceEventLogId { get; set; }

        [ForeignKey("SourceEventLogId")]
        public virtual EventLog SourceEventLog { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required(AllowEmptyStrings = true)]
        public string SolutionName { get; set; }

        [Required]
        public string EventName { get { return LogCommentEvent.Name; } }

        [Required]
        public string Content { get; set; }

        [NotMapped]
        public static string Name { get { return "LogCommentEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Comment"; } }

        [NonSerialized]
        [IgnoreDataMember]
        private IList<HelpfulMarkGivenEvent> _helpfulMarks;
        public virtual IList<HelpfulMarkGivenEvent> HelpfulMarks
        {
            get
            {
                return _helpfulMarks;
            }
            set
            {
                _helpfulMarks = value;
            }
        }

        public LogCommentEvent()
        {
            SolutionName = "OSBIDE";
            EventDate = DateTime.UtcNow;
            HelpfulMarks = new List<HelpfulMarkGivenEvent>();
        }

        public LogCommentEvent(LogCommentEvent other)
            : this()
        {
            if (other != null)
            {
                Id = other.Id;
                EventLogId = other.EventLogId;
                SourceEventLogId = other.SourceEventLogId;
                EventDate = other.EventDate;
                Content = other.Content;
            }
        }

        public string GetShortComment(int maxLength, bool includeEllipsis = true)
        {
            string result = "";
            if (Content.Length < maxLength)
            {
                result = Content;
            }
            else
            {
                result = Content.Substring(0, maxLength) + "...";
            }
            return result;
        }

        public IOsbideEvent FromDict(Dictionary<string, object> values)
        {
            LogCommentEvent evt = new LogCommentEvent();
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
            if (values.ContainsKey("Content"))
            {
                evt.Content = values["Content"].ToString();
            }
            if (values.ContainsKey("SourceEventLogId"))
            {
                evt.SourceEventLogId = (int)values["SourceEventLogId"];
            }
            if (values.ContainsKey("SourceEventLog"))
            {
                evt.SourceEventLog = (EventLog)values["SourceEventLog"];
            }
            return evt;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {

            modelBuilder.Entity<LogCommentEvent>()
                .HasRequired(f => f.EventLog)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<LogCommentEvent>()
                .HasRequired(f => f.SourceEventLog)
                .WithMany(f => f.Comments)
                .WillCascadeOnDelete(false);

        }
    }
}
