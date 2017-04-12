using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models.ViewModels
{
    public class AssignmentsViewModel
    {
        public List<Assignment> Assignments { get; set; }
        public Assignment CurrentAssignment { get; set; }
        public List<SubmitEvent> Submissions { get; set; }
    }
}