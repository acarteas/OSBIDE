using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using System.Windows.Input;
using OSBIDE.Library.Commands;
using Microsoft.Win32;
using System.IO;

namespace OSBIDE.Controls.ViewModels
{
    public class SubmissionEntryViewModel : ViewModelBase
    {
        private EventLog _submissionLog;

        public SubmissionEntryViewModel()
        {
            DownloadCommand = new DelegateCommand(Download, CanIssueCommand);
        }

        /// <summary>
        /// Only accepts EventLogs for Submissions
        /// </summary>
        public EventLog SubmissionLog
        {
            get
            {
                return Submission.EventLog;
            }
        }

        private SubmitEvent _submission;
        public SubmitEvent Submission
        {
            get
            {
                return _submission;
            }
            set
            {
                _submission = value;
                OnPropertyChanged("Submission");
                OnPropertyChanged("SubmissionLog");
            }
        }

        public ICommand DownloadCommand { get; set; }


        private void Download(object param)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "zip|*.zip";
            save.Title = "Download Submission";
            save.ShowDialog();
            if (save.FileName != "")
            {
                using (FileStream stream = (FileStream)save.OpenFile())
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(Submission.SolutionData);
                    }
                }

                //Notify OSBIDE that a submission has been downloaded
                EventGenerator generator = EventGenerator.GetInstance();

                throw new NotImplementedException("This code needs to be updated.");
                //generator.NotifySolutionDownloaded(OsbideUser.CurrentUser, Submission);
            }
        }


        private bool CanIssueCommand(object param)
        {
            return true;
        }
    }
}
