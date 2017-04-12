using OSBIDE.Analytics.Library.Models;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Analytics.Web.ViewModels
{
    public class CommentTimelineViewModel
    {
        public CommentTimeline Timeline { get; set; }
        public EventLog Log { get; set; }
        public Dictionary<string, TimelineCodeDocument> CodeBeforeComment { get; set; }
        public Dictionary<string, TimelineCodeDocument> CodeAfterComment { get; set; }
        public OsbideUser Author { get; set; }
        public CommentTimelineViewModel()
        {
            Timeline = new CommentTimeline();
            CodeBeforeComment = new Dictionary<string, TimelineCodeDocument>();
            CodeAfterComment = new Dictionary<string, TimelineCodeDocument>();
            Log = new EventLog();
            Author = new OsbideUser();
        }

        public CommentTimelineViewModel(CommentTimeline timeline) : base()
        {
            Timeline = timeline;
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

    }
}