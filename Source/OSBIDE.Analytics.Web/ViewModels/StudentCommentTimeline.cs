using OSBIDE.Analytics.Library.Models;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Analytics.Web.ViewModels
{
    public class StudentCommentTimeline
    {
        public TimelineState ProgrammingState { get; set; }
        public PostCoding CrowdCodings { get; set; }
        public ContentCoding ExpertCoding { get; set; }
        public string Comment { get; set; }
        public EventLog Log { get; set; }
        public Dictionary<string, CodeDocument> CodeBeforeComment { get; set; }
        public Dictionary<string, CodeDocument> CodeAfterComment { get; set; }
        public OsbideUser Author { get; set; }
        public StudentCommentTimeline()
        {
            CrowdCodings = new PostCoding();
            CodeBeforeComment = new Dictionary<string, CodeDocument>();
            CodeAfterComment = new Dictionary<string, CodeDocument>();

            ProgrammingState = new TimelineState();
            ProgrammingState.State = "not available";

            ExpertCoding = new ContentCoding();
            Log = new EventLog();
            Author = new OsbideUser();
        }
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
        public int CrowdQuestionCount
        {
            get
            {
                return CrowdCodings.Codings.Where(c => c.IsQuestion == true).Count();
            }
        }
        public int CrowdHelpAcknowledged
        {
            get
            {
                return CrowdCodings.Responses.Where(r => r.AcknowledgedReply == true).Count();
            }
        }
    }
}