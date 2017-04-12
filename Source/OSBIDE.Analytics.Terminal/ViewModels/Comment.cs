using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.ViewModels
{
    public enum CommentType { FeedPost, LogComment}
    public class Comment
    {
        public int UserId { get; set; }
        public int StudentId { get; set; }
        public int CommentId { get; set; }
        public int ParentId { get; set; }
        public DateTime DateReceived { get; set; }
        public CommentType CommentType { get; set; }
        public string Content { get; set; }
        public int WordCount { get; set; }
        public int SentenceCount { get; set; }
        public double AverageSyllablesPerWord { get; set; }
        public double FleschReadingEase { get; set; }
        public double FleschKincaidGradeLevel { get; set; }
        public int HelpfulMarks { get; set; }
        public OsbideUser User { get; set; }

        public List<Comment> ChildComments { get; set; }

        public Comment()
        {
            ChildComments = new List<Comment>();
        }
    }
}
