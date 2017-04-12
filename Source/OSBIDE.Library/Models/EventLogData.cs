using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace OSBIDE.Library.Models
{
    [Serializable]
    [DataContract]
    public class EventLogData
    {
        [Key]
        [Required]
        [DataMember]
        public int LogId { get; set; }

        [IgnoreDataMember]
        public virtual EventLog Log { get; set; }

        [Required]
        [DataMember]
        [Column(TypeName = "image")]
        public byte[] BinaryData { get; set; } 
    }
}
