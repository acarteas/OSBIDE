using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Controls.Models
{
    public class OsbideActivity
    {
        public OsbideUser User { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Source { get; set; }
    }
}
