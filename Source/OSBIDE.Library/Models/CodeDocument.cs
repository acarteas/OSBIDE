using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Events;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class CodeDocument : IVSDocument
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FileName { get; set; }

        private string _content = "";
        [Required]
        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value;
                Lines = TextToList(Content);
            }
        }

        public List<CodeDocumentBreakPoint> BreakPoints { get; set; }
        public List<CodeDocumentErrorListItem> ErrorItems { get; set; }

        /// <summary>
        /// Converts a single string into 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static List<string> TextToList(string text)
        {
            return text.Split(new string[] { "\r\n", "\n", Environment.NewLine }, StringSplitOptions.None).ToList();
        }

        [NotMapped]
        public List<string> Lines { get; private set; }

        public CodeDocument()
        {
            BreakPoints = new List<CodeDocumentBreakPoint>();
            ErrorItems = new List<CodeDocumentErrorListItem>();
            Lines = new List<string>();
        }
    }
}
