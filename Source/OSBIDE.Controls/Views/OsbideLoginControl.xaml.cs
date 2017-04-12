using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OSBIDE.Controls.ViewModels;
using System.Diagnostics;
namespace OSBIDE.Controls.Views
{
    /// <summary>
    /// Interaction logic for OsbideLoginControl.xaml
    /// </summary>
    public partial class OsbideLoginControl : Window
    {
        public OsbideLoginControl()
        {
            InitializeComponent();
            LoadingIcon.Image = OSBIDE.Controls.Properties.Resources.ajax_loader;
            LoadingIcon.VisibleChanged += LoadingIcon_VisibleChanged;
            LoadingIcon.Visible = false;
            PasswordTextBox.KeyDown += PasswordTextBox_KeyDown;
            PasswordTextBox.PasswordChanged += PasswordTextBox_PasswordChanged;

            CreateAccountLink.RequestNavigate += RequestNavigate;
            PrivacyPolicyLink.RequestNavigate += RequestNavigate;
            ForgotEmailLink.RequestNavigate += RequestNavigate;
            ForgotPasswordLink.RequestNavigate += RequestNavigate;
        }

        void PasswordTextBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Password = (e.OriginalSource as PasswordBox).Password;
            }
        }

        void RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        void LoadingIcon_VisibleChanged(object sender, EventArgs e)
        {
            //a little heavy-handed, but the loading icon keeps getting set to "on" when it should be off.
            //This should correct any problem.
            if (ViewModel != null)
            {
                LoadingIcon.Visible = ViewModel.LoadingIconVisible;
            }
        }

        void PasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ViewModel != null)
                {
                    ViewModel.LoginCommand.Execute(this);
                    e.Handled = true;
                }
            }
        }

        public OsbideLoginViewModel ViewModel
        {
            get
            {
                return this.DataContext as OsbideLoginViewModel;
            }
            set
            {
                if (ViewModel != null)
                {
                    ViewModel.RequestClose -= new EventHandler(ViewModel_RequestClose);
                    ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                }
                this.DataContext = value;
                ViewModel.RequestClose += new EventHandler(ViewModel_RequestClose);
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;

                //we have to bind passwords manually
                PasswordTextBox.Password = ViewModel.Password;
            }
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LoadingIconVisible")
            {
                LoadingIcon.Visible = ViewModel.LoadingIconVisible;
            }
            else if (e.PropertyName == "Password")
            {
                if (PasswordTextBox.Password.CompareTo(ViewModel.Password) != 0)
                {
                    PasswordTextBox.Password = ViewModel.Password;
                }
            }
        }

        void ViewModel_RequestClose(object sender, EventArgs e)
        {
            this.Close();
        }

        public static MessageBoxResult ShowModalDialog(OsbideLoginViewModel vm)
        {
            OsbideLoginControl window = new OsbideLoginControl();
            window.ViewModel = vm;
            window.ShowDialog();
            return vm.Result;
        }
    }
}
