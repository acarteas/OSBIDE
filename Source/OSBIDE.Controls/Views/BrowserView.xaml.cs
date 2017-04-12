using Awesomium.Core;
using Awesomium.Windows.Controls;
using OSBIDE.Controls.ViewModels;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OSBIDE.Controls.Views
{
    public class BrowserViewModelChangedEventArgs : EventArgs
    {
        public BrowserViewModel OldModel { get; private set; }
        public BrowserViewModel NewModel { get; private set; }
        public BrowserViewModelChangedEventArgs(BrowserViewModel old, BrowserViewModel newModel)
            : base()
        {
            OldModel = old;
            NewModel = newModel;
        }
    }

    /// <summary>
    /// Interaction logic for BrowserView.xaml
    /// </summary>
    public partial class BrowserView : UserControl
    {
        public event EventHandler<BrowserViewModelChangedEventArgs> BrowserViewModelChanged = delegate { };

        public BrowserView()
        {
            InitializeComponent();
        }

        private void UpdateUrl()
        {
            if (ViewModel.Url.Length > 0)
            {
                BrowserWindow.Source = new Uri(string.Format("{0}?auth={1}", ViewModel.Url, ViewModel.AuthKey));
            }
        }

        public BrowserViewModel ViewModel
        {
            get
            {
                return this.DataContext as BrowserViewModel;
            }
            set
            {
                if (ViewModel != null)
                {
                    ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                }
                BrowserViewModel oldModel = this.DataContext as BrowserViewModel;
                this.DataContext = value;
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                Dispatcher.Invoke(new Action(UpdateUrl));
                BrowserViewModelChanged(this, new BrowserViewModelChangedEventArgs(oldModel, value));
            }
        }

        void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Url")
            {
                Dispatcher.Invoke(new Action(UpdateUrl));
            }
        }

        private void BrowserWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (WebCore.ResourceInterceptor == null)
            {
                WebCore.ResourceInterceptor = OsbideResourceInterceptor.Instance;
            }
        }

        private void BrowserWindow_LoadingFrame(object sender, LoadingFrameEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = BrowserWindow.Opacity;
            animation.To = 0;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            //BrowserWindow.BeginAnimation(OpacityProperty, animation);
        }

        private void BrowserWindow_LoadingFrameComplete(object sender, FrameEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation();
            animation.From = BrowserWindow.Opacity;
            animation.To = 1;
            animation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
            //BrowserWindow.BeginAnimation(OpacityProperty, animation);
        }
    }
}
