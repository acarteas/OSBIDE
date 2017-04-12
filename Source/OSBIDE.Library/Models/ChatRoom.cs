using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class ChatRoom
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }

        public int SchoolId { get; set; }

        [ForeignKey("SchoolId")]
        public School School { get; set; }

        public bool IsDefaultRoom { get; set; }

        [NotMapped]
        public int UserCount { get; set; }

        [NotMapped]
        public string Url { get; set; }

        [NotMapped]
        public bool IsActiveRoom { get; set; }

        [NotMapped]
        public List<OsbideUser> Users { get; set; }

        public ChatRoom()
        {
            Users = new List<OsbideUser>();
            IsDefaultRoom = false;
        }
    }
}
