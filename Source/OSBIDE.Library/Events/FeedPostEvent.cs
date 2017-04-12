using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    [Serializable]
    public class FeedPostEvent : IOsbideEvent
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
        public string EventName { get { return FeedPostEvent.Name; } }

        [NotMapped]
        public static string Name { get { return "FeedPostEvent"; } }

        [NotMapped]
        public string PrettyName { get { return "Feed Post"; } }

        [Required]
        public string Comment { get; set; }

        public FeedPostEvent()
        {
            EventDate = DateTime.UtcNow;
            SolutionName = "";
        }

        public string GetShortComment(int maxLength)
        {
            if (Comment.Length < maxLength)
            {
                return Comment;
            }
            while(Comment[maxLength] != ' ' && maxLength >= 0)
            {
                maxLength--;
            }
            return Comment.Substring(0, maxLength);
        }

        IOsbideEvent IOsbideEvent.FromDict(Dictionary<string, object> values)
        {
            FeedPostEvent evt = new FeedPostEvent();
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
            if (values.ContainsKey("Comment"))
            {
                evt.Comment = values["Comment"].ToString();
            }
            return evt;
        }
    }
}
