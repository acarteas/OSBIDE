using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using OSBIDE.Controls.WebServices;
using OSBIDE.Library.Commands;
using OSBIDE.Library.Models;
using OSBIDE.Library.ServiceClient;

namespace OSBIDE.Controls.ViewModels
{
    public class OsbideLoginViewModel : ViewModelBase
    {
        private string _email = "";
        private string _password = "";
        private bool _loadingIconVisible = false;
        private OsbideWebServiceClient _serviceClient = null;
        private bool _buttonsEnabled;
        private string _authenticationHash = "";
        private string _errorText = "";
        private bool _isLoggedIn = false;
        
        public event EventHandler RequestClose = delegate { };
        public event EventHandler RequestCreateAccount = delegate { };

        public OsbideLoginViewModel()
        {
            _serviceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);
            LoginCommand = new DelegateCommand(Login, CanIssueCommand);
            LogoutCommand = new DelegateCommand(Logout, CanIssueCommand);
            CancelCommand = new DelegateCommand(Cancel, CanIssueCommand);
            CreateAccountCommand = new DelegateCommand(CreateAccount, CanIssueCommand);
            _serviceClient.LoginCompleted += LoginCompleted;
            _buttonsEnabled = true;
            _loadingIconVisible = false;
        }

        #region properties
        public ICommand LoginCommand { get; set; }
        public ICommand LogoutCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand CreateAccountCommand { get; set; }
        public MessageBoxResult Result { get; private set; }

        public bool IsLoggedIn
        {
            get
            {
                return _isLoggedIn;
            }
            set
            {
                _isLoggedIn = value;
                OnPropertyChanged("IsLoggedIn");
            }
        }

        public string ErrorText
        {
            get
            {
                return _errorText;
            }
            set
            {
                _errorText = value;
                OnPropertyChanged("ErrorText");
            }
        }

        public string AuthenticationHash
        {
            get
            {
                return _authenticationHash;
            }
            set
            {
                _authenticationHash = value;
                OnPropertyChanged("AuthenticationHash");
            }
        }

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

        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                OnPropertyChanged("Email");
            }
        }

        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
                OnPropertyChanged("Password");
            }
        }
        #endregion

        private void LoginCompleted(object sender, LoginCompletedEventArgs e)
        {
            LoadingIconVisible = false;
            ButtonsEnabled = true;

            //hash longer than 0 means success
            try
            {
                if (e.Result.Length > 0)
                {
                    AuthenticationHash = e.Result;
                    Result = MessageBoxResult.OK;
                    RequestClose(this, EventArgs.Empty);
                }
                else
                {
                    ErrorText = "Invalid email or password.";
                }
            }
            catch (Exception)
            {
                ErrorText = "Error processing request (http).";
            }
        }

        private void Logout(object param)
        {
            Result = MessageBoxResult.No;
            RequestClose(this, EventArgs.Empty);
        }

        private void Login(object param)
        {
            //begin login attempt
            LoadingIconVisible = true;
            ButtonsEnabled = false;
            ErrorText = "";
            _serviceClient.LoginAsync(Email, UserPassword.EncryptPassword(Password, Email));
        }

        private void CreateAccount(object param)
        {
            RequestCreateAccount(this, EventArgs.Empty);
            Cancel(this);
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

        public string CreateAccountUrl
        {
            get
            {
#if DEBUG
                return "http://localhost:24867/Account/Create";
#else
                return "http://osbide.com/Account/Create";
#endif
            }
        }
    }
}
