using OSBIDE.Controls.Models;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Controls.ViewModels
{
    public class ActivityFeedMetricsViewModel : ViewModelBase
    {
        private OsbideContext _db = null;
        public TimeSpan ActivityFeedSessionTimeout { get; set; }

    }
}
