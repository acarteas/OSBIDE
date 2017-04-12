using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class QuestionResponseCount
    {
        [Key]
        public int Id { get; set; }
        public int ContentId { get; set; }
        public int Count { get; set; }

    }
}
