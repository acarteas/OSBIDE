using OSBIDE.Controls.WebServices;
using OSBIDE.Library.Commands;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace OSBIDE.Controls.ViewModels
{
    public class AskForHelpViewModel : ViewModelBase
    {
        public ICommand SubmitCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        private OsbideWebServiceClient _serviceClient = null;
        public event EventHandler RequestClose = delegate { };
        

        public OsbideUser CurrentUser { get; set; }
        public MessageBoxResult Result { get; private set; }

        private bool _loadingIconVisible = false;
        public bool LoadingIconVisible
        {
            get
            {
                return _loadingIconVisible;
            }
            set
            {
                _loadingIconVisible = value;
                OnPropertyChanged("LoadingIconVisible");
            }
        }

        private bool _buttonsEnabled = true;
        public bool ButtonsEnabled
        {
            get
            {
                return _buttonsEnabled;
            }
            set
            {
                _buttonsEnabled = value;
                OnPropertyChanged("ButtonsEnabled");
            }
        }

        private string _code = "";
        public string Code
        {
            get
            {
                return _code;
            }
            set
            {
                _code = value;
                OnPropertyChanged("Code");
            }
        }

        private string _userText = "";
        public string UserText
        {
            get
            {
                return _userText;
            }
            set
            {
                _userText = value;
                OnPropertyChanged("UserText");
            }
        }

        public AskForHelpViewModel()
        {
            SubmitCommand = new DelegateCommand(Submit, CanIssueCommand);
            CancelCommand = new DelegateCommand(Cancel, CanIssueCommand);
        }

        private void Submit(object param)
        {
            Result = MessageBoxResult.OK;
            RequestClose(this, EventArgs.Empty);
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
