using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public class CodedOsbideFeedItem
    {
        [Key]
        public int Id { get; set; }
        public double? PostId { get; set; }
        public DateTime? Date { get; set; }
        public int? Author { get; set; }
        public string AuthorRole { get; set; }
        public string Content { get; set; }
        public double? ReadingEase { get; set; }
        public double? ReadingGradeLevel { get; set; }
        public string Cat { get; set; }
        public string SubCat { get; set; }
        public string Mod { get; set; }
        public string Mod2 { get; set; }
        public string Mod3 { get; set; }
        public string CodeMod { get; set; }
        public int? HelpAck { get; set; }
    }
}
