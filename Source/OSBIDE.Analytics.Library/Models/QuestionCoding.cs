using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class QuestionCoding
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// This will be a student's ID number
        /// </summary>
        [Display(Name="Student Id")]
        [Required(ErrorMessage="This needs to be a valid number!")]
        public int AuthorStudentId { get; set; }
        [ForeignKey("Post")]
        public int PostId { get; set; }
        public virtual Post Post { get; set; }
        public bool IsQuestion { get; set; }
    }
}
