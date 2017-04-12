using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class ContentCoding : IComparable<ContentCoding>, IComparer<ContentCoding>
    {
        [Key]
        public int Id { get; set; }
        public float PostId { get; set; }
        public DateTime Date { get; set; }
        public int AuthorId { get; set; }
        public string AuthorRole { get; set; }
        public string Content { get; set; }
        public float ReadingEase { get; set; }
        public float ReadingGradeLevel { get; set; }
        public bool ToCode { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string PrimaryModifier { get; set; }
        public string SecondaryModifier { get; set; }
        public string TertiaryModifier { get; set; }
        public string CodeModifier { get; set; }
        public bool HelpAcknowledged { get; set; }

        public ContentCoding()
        {
            Date = DateTime.Now;
        }

        public int Compare(ContentCoding x, ContentCoding y)
        {
            return x.CompareTo(y);
        }

        public int CompareTo(ContentCoding other)
        {
            if(Date == other.Date)
            {
                return Id.CompareTo(other.Id);
            }
            return Date.CompareTo(other.Date);
        }
    }
}
