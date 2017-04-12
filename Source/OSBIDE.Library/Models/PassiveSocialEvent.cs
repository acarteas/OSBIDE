using System;
using System.ComponentModel.DataAnnotations;

namespace OSBIDE.Library.Models
{
    public class PassiveSocialEvent
    {
        [Required]
        public int EventLogId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public DateTime EventDate { get; set; }
    }
}
