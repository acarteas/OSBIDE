using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public class Syllable
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(255)]
        public string Word { get; set; }

        public int SyllableCount { get; set; }
    }
}
