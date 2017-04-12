using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class StackFrameVariable
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StackFrameId { get; set; }

        [ForeignKey("StackFrameId")]
        public virtual StackFrame StackFrame { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Value { get; set; }
    }
}
