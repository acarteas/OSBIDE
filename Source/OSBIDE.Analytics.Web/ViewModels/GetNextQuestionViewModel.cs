using OSBIDE.Analytics.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Analytics.Web.ViewModels
{
    public class GetNextQuestionViewModel
    {
        public int StudentId { get; set; }
        public bool IsQuestion { get;set;}
        public Post Post { get; set; }
    }
}