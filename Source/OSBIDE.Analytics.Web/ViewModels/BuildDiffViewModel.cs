using OSBIDE.Analytics.Library.Models;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Analytics.Web.ViewModels
{
    public class BuildDiffViewModel
    {
        public List<BuildEvent> BuildsBefore { get; set; }
        public List<BuildEvent> BuildsAfter { get; set; }
        public Dictionary<int, TimelineState> BuildStates { get; set; }
        public CommentTimeline Comment { get; set; }
        public OsbideUser User { get; set; }
        public EventLog OriginalEvent { get; set; }

        public BuildDiffViewModel()
        {
            BuildsBefore = new List<BuildEvent>();
            BuildsAfter = new List<BuildEvent>();
            BuildStates = new Dictionary<int, TimelineState>();
        }
    }
}