using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class Post
    {
        [Key]
        public int Id { get;set;}
        public int ParentId { get; set; }
        public int OsbideId { get; set; }
        public int AuthorId { get; set; }
        public DateTime DatePosted { get; set; }
        public string Content { get; set; }
    }
}
