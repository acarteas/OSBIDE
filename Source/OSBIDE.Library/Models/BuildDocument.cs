using OSBIDE.Library.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class BuildDocument
    {
        [Key]
        [Column(Order=0)]
        public int BuildId { get; set; }
        
        [ForeignKey("BuildId")]
        public virtual BuildEvent Build { get; set; }

        [Key]
        [Column(Order = 1)]
        public int DocumentId { get; set; }

        [ForeignKey("DocumentId")]
        public virtual CodeDocument Document { get; set; }

        public int? NumberOfInserted { get; set; }

        public int? NumberOfModified { get; set; }

        public int? NumberOfDeleted { get; set; }

        public string ModifiedLines { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public int? UpdatedBy { get; set; }

        [ForeignKey("UpdatedBy")]
        public virtual OsbideUser UpdatedByUser { get; set; }
    }
}
