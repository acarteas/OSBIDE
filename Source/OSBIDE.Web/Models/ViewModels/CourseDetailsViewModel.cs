using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class CourseDetailsViewModel
    {
        public IList<Assignment> Assignments { get; set; }
        public List<string> CourseDocuments { get; set; }
        public Dictionary<int, List<string>> AssignmentFiles { get; set; }
        public Course CurrentCourse { get; set; }
        public OsbideUser CurrentUser { get; set; }
        public List<OsbideUser> Coordinators { get; set; }
        public CourseDetailsViewModel()
        {
            Assignments = new List<Assignment>();
            CourseDocuments = new List<string>();
            AssignmentFiles = new Dictionary<int, List<string>>();
        }
    }
}