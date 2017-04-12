using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class UserScore
    {
        [Key]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public OsbideUser User { get; set; }

        public int Score { get; set; }

        public DateTime LastCalculated { get; set; }
    }
}
