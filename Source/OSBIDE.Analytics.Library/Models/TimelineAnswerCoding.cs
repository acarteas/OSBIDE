using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class TimelineAnswerCoding
    {
        [Key]
        public int Id { get; set; }

        
        public int CommentTimelineId { get; set; }
        [ForeignKey("CommentTimelineId")]
        public virtual CommentTimeline CommentTimeline { get; set; }

        public int AnswerCodingId { get; set; }
        [ForeignKey("AnswerCodingId")]
        public virtual AnswerCoding AnswerCoding { get; set; }
    }
}
