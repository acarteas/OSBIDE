using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using OSBIDE.Library.Models;
using OSBIDE.Library.Events;
using System.Windows;
using System.Windows.Input;
using OSBIDE.Library.Commands;
using System.Runtime.Caching;
using OSBIDE.Library;
using OSBIDE.Controls.WebServices;
using OSBIDE.Library.ServiceClient;
using System.IO;

namespace OSBIDE.Controls.ViewModels
{
    public class SubmitAssignmentViewModel : ViewModelBase
    {
        private OsbideWebServiceClient _client = null;
        private string _authToken = "";
        private SubmitEvent _submitEvent = null;
        public SubmitAssignmentViewModel(string userName, string authToken, SubmitEvent submitEvent)
        {
            UserName = userName;
            _authToken = authToken;
            _submitEvent = submitEvent;
            SolutionName = Path.GetFileNameWithoutExtension(submitEvent.SolutionName);
            _client = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);
            ContinueCommand = new DelegateCommand(Continue, CanIssueCommand);
            CancelCommand = new DelegateCommand(Cancel, CanIssueCommand);
            Assignments = new ObservableCollection<Assignment>();
            Courses = new ObservableCollection<Course>();
            LastSubmitted = "N/A";

            //set up event listeners
            _client.GetCoursesForUserCompleted += _client_GetCoursesForUserCompleted;
            _client.GetAssignmentsForCourseCompleted += _client_GetAssignmentsForCourseCompleted;
            _client.SubmitAssignmentCompleted += _client_SubmitAssignmentCompleted;
            _client.GetLastAssignmentSubmitDateCompleted += _client_GetLastAssignmentSubmitDateCompleted;

            //load courses
            IsLoading = true;
            _client.GetCoursesForUserAsync(authToken);
        }

        void _client_GetLastAssignmentSubmitDateCompleted(object sender, GetLastAssignmentSubmitDateCompletedEventArgs e)
        {
            try
            {
                //not submitted
                if (e.Result == DateTime.MinValue)
                {
                    LastSubmitted = "N/A";
                }
                else
                {
                    //convert from UTC to local time
                    DateTime utc = new DateTime(e.Result.Ticks, DateTimeKind.Utc);
                    LastSubmitted = utc.ToLocalTime().ToString("MM/dd @ hh:mm tt");
                }
            }
            catch(Exception)
            {
                LastSubmitted = "Unknown";
            }
        }

        void _client_SubmitAssignmentCompleted(object sender, SubmitAssignmentCompletedEventArgs e)
        {
            IsLoading = false;
            int result = -1;
            try
            {
                result = e.Result;
                
            }
            catch (Exception)
            {
            }
            if (result == -1)
            {
                ServerMessage = "Transmission error.  If the problem persists, please contact your course instructor.";
            }
            else
            {
                ServerMessage = "Your assignment was successfully submitted.  Your confirmation number is: \"" + result + "\".";
            }
            _client.GetLastAssignmentSubmitDateAsync(SelectedAssignment, _authToken);
        }

        //will populate the assignment dropdown
        void _client_GetAssignmentsForCourseCompleted(object sender, GetAssignmentsForCourseCompletedEventArgs e)
        {
            IsLoading = false;
            Assignments.Clear();
            try
            {
                foreach (Assignment a in e.Result)
                {
                    Assignments.Add(a);
                }
            }
            catch (Exception)
            {
            }
        }

        //will populate the courses dropdown based on the current user's settings
        void _client_GetCoursesForUserCompleted(object sender, GetCoursesForUserCompletedEventArgs e)
        {
            IsLoading = false;
            Courses.Clear();
            try
            {
                foreach (Course c in e.Result)
                {
                    Courses.Add(c);
                }
            }
            catch (Exception)
            {
            }
        }

        #region properties

        public event EventHandler RequestClose = delegate { };

        public ObservableCollection<Assignment> Assignments { get; set; }
        public ObservableCollection<Course> Courses { get; set; }
        public MessageBoxResult Result { get; private set; }
        public ICommand ContinueCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        private string _serverMessage = "";
        public string ServerMessage
        {
            get
            {
                return _serverMessage;
            }
            set
            {
                _serverMessage = value;
                OnPropertyChanged("ServerMessage");
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get
            {
                return _isLoading;
            }
            set
            {
                _isLoading = value;
                OnPropertyChanged("IsLoading");
            }
        }

        private int _selectedAssignment = -1;
        public int SelectedAssignment
        {
            get
            {
                return _selectedAssignment;
            }
            set
            {
                _selectedAssignment = value;

                //find last submit date
                _client.GetLastAssignmentSubmitDateAsync(value, _authToken);

                OnPropertyChanged("SelectedAssignment");
                OnPropertyChanged("HasAssignmentSelected");
            }
        }

        public bool HasAssignmentSelected
        {
            get
            {
                return _selectedAssignment > 0;
            }
        }

        private int _selectedCourse = -1;
        public int SelectedCourse
        {
            get
            {
                return _selectedCourse;
            }
            set
            {
                _selectedCourse = value;

                //load assignments
                IsLoading = true;
                _client.GetAssignmentsForCourseAsync(_selectedCourse, _authToken);

                OnPropertyChanged("SelectedCourse");
                OnPropertyChanged("HasCourseSelected");
            }
        }

        public bool HasCourseSelected
        {
            get
            {
                return _selectedCourse > 0;
            }
        }

        public string UserName
        {
            private set;
            get;
        }

        private string _lastSubmitted = "";
        public string LastSubmitted
        {
            get
            {
                return _lastSubmitted;
            }
            set
            {
                _lastSubmitted = value;
                OnPropertyChanged("LastSubmitted");
            }
        }

        private string _solutionName = "";
        public string SolutionName
        {
            get
            {
                return _solutionName;
            }
            set
            {
                _solutionName = value;
                OnPropertyChanged("SolutionName");
            }
        }

        #endregion

        private void Continue(object param)
        {
            Result = MessageBoxResult.OK;

            //submit the solution to OSBIDE
            _submitEvent.CreateSolutionBinary();
            _submitEvent.AssignmentId = SelectedAssignment;
            IsLoading = true;
            EventLog eventLog = new EventLog(_submitEvent);
            _client.SubmitAssignmentAsync(_submitEvent.AssignmentId, eventLog, _authToken);
        }

        private void Cancel(object param)
        {
            Result = MessageBoxResult.Cancel;
            RequestClose(this, EventArgs.Empty);
        }

        private bool CanIssueCommand(object param)
        {
            return true;
        }
    }
}
