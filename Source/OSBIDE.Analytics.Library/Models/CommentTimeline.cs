using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class CommentTimeline
    {
        [Key]
        public int Id { get; set; }
        
        public int ProgrammingStateId { get; set; }
        [ForeignKey("ProgrammingStateId")]
        public virtual TimelineState ProgrammingState { get; set; }
        
        public virtual List<TimelineQuestionCoding> QuestionCodings { get; set; }
        public virtual List<TimelineAnswerCoding> AnswerCodings { get; set; }

        public int ExpertCodingId { get; set; }
        [ForeignKey("ExpertCodingId")]
        public virtual ContentCoding ExpertCoding { get; set; }
        public string Comment { get; set; }
        public int EventLogId { get; set; }
        public virtual List<TimelineCodeDocument> TimelineCodeDocuments { get; set; }
        public int AuthorId { get; set; }
        public CommentTimeline()
        {
            QuestionCodings = new List<TimelineQuestionCoding>();
            AnswerCodings = new List<TimelineAnswerCoding>();

        }
        /*
        public int CodeDiff
        {
            get
            {
                int beforeTotalLines = 0;
                int afterTotalLines = 0;

                foreach (var kvp in CodeBeforeComment)
                {
                    beforeTotalLines += kvp.Value.Lines.Count;
                }
                foreach (var kvp in CodeAfterComment)
                {
                    afterTotalLines += kvp.Value.Lines.Count;
                }

                return afterTotalLines - beforeTotalLines;
            }
        }
         * */
        public int CrowdQuestionCount
        {
            get
            {
                return QuestionCodings.Where(c => c.QuestionCoding.IsQuestion == true).Count();
            }
        }
        public int CrowdHelpAcknowledged
        {
            get
            {
                return AnswerCodings.Where(r => r.AnswerCoding.AcknowledgedReply == true).Count();
            }
        }
    }
}
