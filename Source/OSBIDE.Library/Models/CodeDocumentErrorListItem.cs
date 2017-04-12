using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class CodeDocumentErrorListItem
    {
        [Key]
        [Required]
        [Column(Order = 0)]
        public int CodeFileId { get; set; }

        [ForeignKey("CodeFileId")]
        public virtual CodeDocument CodeDocument { get; set; }

        [Key]
        [Required]
        [Column(Order = 1)]
        public int ErrorListItemId { get; set; }

        [ForeignKey("ErrorListItemId")]
        public virtual ErrorListItem ErrorListItem { get; set; }
    }
}
