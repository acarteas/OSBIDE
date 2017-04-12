using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Analytics.Web.Models.ViewModels.CommentAnalyzer
{
    public class IndexViewModel
    {
        public List<OsbideUser> Users { get; set; }
        public Course Course { get; set; }
        public Dictionary<int, int> PostsByUser { get; set; }
        public Dictionary<int, int> RepliesByUser { get; set; }
        public Dictionary<int, int> SavesByUser { get; set; }

        public IndexViewModel()
        {
            Users = new List<OsbideUser>();
            PostsByUser = new Dictionary<int, int>();
            RepliesByUser = new Dictionary<int, int>();
            SavesByUser = new Dictionary<int, int>();
        }
    }
}