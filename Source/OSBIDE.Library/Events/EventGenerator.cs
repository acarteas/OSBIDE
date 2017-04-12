using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;

namespace OSBIDE.Library.Events
{
    public class EventGenerator : IOsbideEventGenerator
    {
        private static EventGenerator _instance = null;

        public event EventHandler<SubmitAssignmentArgs> SolutionSubmitRequest = delegate { };
        public event EventHandler<SolutionDownloadedEventArgs> SolutionDownloaded = delegate { };
        public event EventHandler<SubmitEventArgs> SubmitEventRequested = delegate { };
        private EventGenerator()
        {

        }

        public static EventGenerator GetInstance()
        {
            if (_instance == null)
            {
                _instance = new EventGenerator();
            }
            return _instance;
        }

        public void SubmitEvent(IOsbideEvent evt)
        {
            SubmitEventRequested(this, new SubmitEventArgs(evt));
        }

        /// <summary>
        /// Triggers a request for the system to save the active solution
        /// </summary>
        public void RequestSolutionSubmit(int assignmentId)
        {
            SolutionSubmitRequest(this, new SubmitAssignmentArgs(assignmentId));
        }

        public void NotifySolutionDownloaded(OsbideUser downloadingUser, SubmitEvent downloadedSubmission)
        {
            SolutionDownloaded(downloadingUser, new SolutionDownloadedEventArgs(downloadingUser, downloadedSubmission));
        }
    }

    public class SubmitEventArgs : EventArgs
    {
        public IOsbideEvent Event { get; private set; }
        public SubmitEventArgs(IOsbideEvent evt)
        {
            Event = evt;
        }
    }

    public class SubmitAssignmentArgs : EventArgs
    {
        public int AssignmentId { get; private set; }
        public SubmitAssignmentArgs(int assignmentId)
        {
            AssignmentId = assignmentId;
        }
    }

    public class SolutionDownloadedEventArgs : EventArgs
    {
        public OsbideUser DownloadingUser { get; private set; }
        public SubmitEvent DownloadedSubmission { get; private set; }

        public SolutionDownloadedEventArgs(OsbideUser downloadingUser, SubmitEvent downloadedSubmission)
        {
            DownloadingUser = downloadingUser;
            DownloadedSubmission = downloadedSubmission;
        }
    }
}
