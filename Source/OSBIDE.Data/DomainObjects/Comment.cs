using System;

namespace OSBIDE.Data.DomainObjects
{
    public class Comment
    {
        public int CommentId { get; set; }
        public int OriginalId { get; set; }
        public int ActualId { get; set; }
        public string CourseNumber { get; set; }
        public string CoursePrefix { get; set; }
        public string Content { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public int SenderId { get; set; }
        public DateTime EventDate { get; set; }
        public int HelpfulMarkCounts { get; set; }
        public bool IsHelpfulMarkSender { get; set; }
        public string FirstAndLastName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }
        public string CourseName
        {
            get
            {
                return string.Format("{0} {1}", CoursePrefix, CourseNumber);
            }
        }
    }
}