using OSBIDE.Analytics.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Analytics.Web.ViewModels
{
    public class PostCoding
    {
        public int OsbidePostId { get; set; }
        public List<QuestionCoding> Codings { get; set; }
        public List<AnswerCoding> Responses { get; set; }
        public PostCoding()
        {
            Codings = new List<QuestionCoding>();
            Responses = new List<AnswerCoding>();
        }
    }
}