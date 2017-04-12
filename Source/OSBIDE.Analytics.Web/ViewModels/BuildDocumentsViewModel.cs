using OSBIDE.Analytics.Library.Models;
using OSBIDE.Library.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Analytics.Web.ViewModels
{
    public class BuildDocumentsViewModel
    {
        public BuildEvent CurrentBuild { get; set; }
        public BuildEvent NextInterestingBuild { get; set; }
        public BuildEvent PreviousInterestingBuild { get; set; }
        public List<BuildEvent> FutureBuilds { get; set; }
        public TimelineState BuildState { get; set; }
        public List<BuildEvent> PastBuilds { get; set; }

        public BuildDocumentsViewModel()
        {
            FutureBuilds = new List<BuildEvent>();
        }
    }
}