using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class CodeDocumentBreakPoint
    {
        [Key]
        [Required]
        [Column(Order=0)]
        public int CodeFileId { get; set; }

        [ForeignKey("CodeFileId")]
        public virtual CodeDocument CodeDocument { get; set; }

        [Key]
        [Required]
        [Column(Order = 1)]
        public int BreakPointId { get; set; }

        [ForeignKey("BreakPointId")]
        public virtual BreakPoint BreakPoint { get; set; }

    }
}
