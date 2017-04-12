using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class BuildError
    {
        [Key]
        [Column(Order = 0)]
        [ForeignKey("Log")]
        public int LogId { get; set; }
        public virtual EventLog Log { get; set; }

        [Key]
        [Column(Order = 1)]
        [ForeignKey("BuildErrorType")]
        public int BuildErrorTypeId { get; set; }
        public virtual ErrorType BuildErrorType { get; set; }
    }
}
