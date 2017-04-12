using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    public interface IOsbideEventGenerator
    {
        event EventHandler<SubmitAssignmentArgs> SolutionSubmitRequest;
        event EventHandler<SolutionDownloadedEventArgs> SolutionDownloaded;
        event EventHandler<SubmitEventArgs> SubmitEventRequested;
    }
}
