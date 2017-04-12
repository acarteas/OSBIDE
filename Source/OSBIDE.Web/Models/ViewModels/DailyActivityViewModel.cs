using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class DailyActivityViewModel
    {
        public List<OsbideUser> Students { get; set; }

        [DataType(DataType.Date)]
        public DateTime SelectedDate { get; set; }
        public int SelectedStudentId { get; set; }
        public SortedDictionary<DateTime, object> ActivityItems { get; set; }
        public DailyActivityViewModel()
        {
            ActivityItems = new SortedDictionary<DateTime, object>();
            SelectedDate = DateTime.Now;
            Students = new List<OsbideUser>();
        }
    }
}