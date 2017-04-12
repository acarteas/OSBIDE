using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public class WhatsNewItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime DatePosted { get; set; }

        [Required]
        public string NewsHeader { get; set; }

        [Required]
        public string Content { get; set; }
    }
}
