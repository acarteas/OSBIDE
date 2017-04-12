using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public class StudentGrade
    {
        [Key]
        public int Id { get; set; }

        public int StudentId { get; set; }
        public string Deliverable { get; set; }
        public double Grade { get; set; }
        public int CourseId { get; set; }
        public DateTime CreatedOn { get; set; }
        public int CreatedBy { get; set; }
    }
}
