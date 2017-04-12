using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class Reply
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Parent")]
        public int ParentId { get; set; }
        public Post Parent { get; set; }
        public int OsbideId { get; set; }
        public int AuthorId { get; set; }
        public DateTime DatePosted { get; set; }
        public string Content { get; set; }
    }
}
