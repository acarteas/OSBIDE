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
using OSBIDE.Library;
using OSBIDE.Library.Events;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using OSBIDE.Controls.WebServices;

namespace OSBIDE.Controls
{
    /// <summary>
    /// Interaction logic for OsbideStatusControl.xaml
    /// </summary>
    public partial class OsbideStatusControl : UserControl
    {

        public OsbideStatusControl()
        {
            InitializeComponent();
        }

    }
}
