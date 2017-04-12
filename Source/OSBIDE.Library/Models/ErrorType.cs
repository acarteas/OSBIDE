using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class ErrorType
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
