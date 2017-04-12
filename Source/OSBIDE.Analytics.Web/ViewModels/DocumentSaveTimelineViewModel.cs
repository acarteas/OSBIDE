using OSBIDE.Analytics.Library.Models;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Analytics.Web.ViewModels
{
    public class DocumentSaveTimelineViewModel
    {
        public List<Post> Discussion { get; set; }
        public Dictionary<int, BuildEvent> BuildsBefore { get; set; }
        public Dictionary<int, BuildEvent> BuildsAfter { get; set; }
        public Dictionary<int, TimelineState> StatesBefore { get; set; }
        public Dictionary<int, TimelineState> StatesAfter { get; set; }
        public CommentTimeline Timeline { get; set; }
        public EventLog TimelineLog { get; set; }
        public OsbideUser User { get; set; }
    }
}