using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class TimelineQuestionCoding
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("CommentTimeline")]
        public int CommentTimelineId { get; set; }
        public virtual CommentTimeline CommentTimeline { get; set; }

        [ForeignKey("QuestionCoding")]
        public int QuestionCodingId { get; set; }
        public virtual QuestionCoding QuestionCoding { get; set; }
    }
}
