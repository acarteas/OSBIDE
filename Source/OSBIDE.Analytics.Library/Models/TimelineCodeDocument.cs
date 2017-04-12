using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class TimelineCodeDocument
    {
        [Key]
        public int Id { get; set; }
        public int CodeDocumentId { get; set; }
        
        public int CommentTimelineId { get; set; }

        [ForeignKey("CommentTimelineId")]
        public virtual CommentTimeline CommentTimeline { get; set; }

        public string DocumentName { get; set; }
        
        public bool isBeforeComment { get; set; }

        [NotMapped]
        public List<string> Lines { get; private set; }
        private string _content = "";
        
        public string DocumentContent
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                Lines = TextToList(DocumentContent);
            }
        }

        /// <summary>
        /// Converts a single string into 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> TextToList(string text)
        {
            return text.Split(new string[] { "\r\n", "\n", Environment.NewLine }, StringSplitOptions.None).ToList();
        }
    }
}
