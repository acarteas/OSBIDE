using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using System.Windows.Input;
using OSBIDE.Library.Commands;
using Microsoft.Win32;
using System.IO;
using Ionic.Zip;
using System.Windows.Forms;
using OSBIDE.Library;
using System.Windows.Threading;

namespace OSBIDE.Controls.ViewModels
{
    public class AssignmentSubmissionsViewModel : ViewModelBase
    {
        private string _selectedAssignment = "";
        private OsbideContext _db;
        private List<EventLog> _allSubmissions = new List<EventLog>();
        private string _errorMessage = "";
        private DispatcherTimer _timer = new DispatcherTimer();

        public ObservableCollection<string> AvailableAssignments { get; set; }
        public ObservableCollection<SubmissionEntryViewModel> SubmissionEntries { get; set; }
        public ICommand DownloadCommand { get; set; }
        public ICommand RefreshAssignments { get; set; }
        public string SelectedAssignment
        {
            get
            {
                return _selectedAssignment;
            }
            set
            {
                _selectedAssignment = value;
                GetSubmissionEntries();
                OnPropertyChanged("SelectedAssignment");
            }
        }

        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
            private set
            {
                _errorMessage = value;
                OnPropertyChanged("ErrorMessage");
            }
        }

        public AssignmentSubmissionsViewModel()
            : this(OsbideContext.DefaultLocalInstance)
        {
        }

        public AssignmentSubmissionsViewModel(OsbideContext db)
        {
            DownloadCommand = new DelegateCommand(Download, CanIssueCommand);
            RefreshAssignments = new DelegateCommand(RefreshAssignmentList, CanIssueCommand);
            AvailableAssignments = new ObservableCollection<string>();
            SubmissionEntries = new ObservableCollection<SubmissionEntryViewModel>();

            /* AC: turned off for fall study
            _timer.Interval = new TimeSpan(0, 0, 1, 0, 0);
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();

            _db = db;
            UpdateAssignmentListing();
             * */
        }

        private void UpdateAssignmentListing()
        {
            /*
            var names = (from submit in _db.SubmitEvents
                         select submit.AssignmentName).Distinct().ToList();
            foreach (string name in names)
            {
                if (AvailableAssignments.Contains(name) == false)
                {
                    AvailableAssignments.Add(name);
                }
            }
             * */
        }

        void timer_Tick(object sender, EventArgs e)
        {
            UpdateAssignmentListing();
        }

        private void GetSubmissionEntries()
        {
            /*
            var entries = from submit in _db.SubmitEvents
                          where submit.AssignmentName == SelectedAssignment
                          //&& submit.EventLog.SenderId == OsbideUser.CurrentUser.Id
                          orderby submit.EventDate ascending
                          select submit;
            Dictionary<string, SubmitEvent> events = new Dictionary<string, SubmitEvent>();
            foreach (SubmitEvent evt in entries)
            {
                events[evt.EventLog.Sender.FullName] = evt;
            }
             * 

            SubmissionEntries.Clear();
            foreach (string key in events.Keys.OrderBy(k => k.ToString()))
            {
                SubmitEvent evt = events[key];
                SubmissionEntryViewModel vm = new SubmissionEntryViewModel();
                vm.Submission = evt;
                SubmissionEntries.Add(vm);
            }
             * */
        }

        private void RefreshAssignmentList(object param)
        {
            GetSubmissionEntries();
        }

        private void Download(object param)
        {
            FolderBrowserDialog save = new FolderBrowserDialog();
            DialogResult result = save.ShowDialog();
            if (result == DialogResult.OK)
            {
                string directory = save.SelectedPath;
                foreach (SubmissionEntryViewModel vm in SubmissionEntries)
                {
                    string unpackDir = Path.Combine(directory, vm.SubmissionLog.Sender.FullName);
                    using (MemoryStream zipStream = new MemoryStream())
                    {
                        zipStream.Write(vm.Submission.SolutionData, 0, vm.Submission.SolutionData.Length);
                        zipStream.Position = 0;
                        try
                        {
                            using (ZipFile zip = ZipFile.Read(zipStream))
                            {
                                foreach (ZipEntry entry in zip)
                                {
                                    try
                                    {
                                        entry.Extract(unpackDir, ExtractExistingFileAction.OverwriteSilently);
                                    }
                                    catch (BadReadException)
                                    {
                                    }
                                    catch (Exception)
                                    {
                                    }
                                }
                            }
                        }
                        catch (ZipException)
                        {
                            MessageBox.Show("extract failed for " + vm.SubmissionLog.Sender.FullName);
                        }
                        catch (Exception)
                        {
                        }
                    }

                    //notify OSBIDE that someone has downloaded a submission
                    EventGenerator generator = EventGenerator.GetInstance();
                    throw new NotImplementedException("This code needs to be updated.");
                    //generator.NotifySolutionDownloaded(OsbideUser.CurrentUser, vm.Submission);
                }
            }
        }


        private bool CanIssueCommand(object param)
        {
            return true;
        }
    }
}
