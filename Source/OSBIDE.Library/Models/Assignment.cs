using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class Assignment
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public bool IsDeleted { get; set; }

        [Required]
        [Display(Name="Release Date")]
        [DataType(DataType.Date)]
        public DateTime ReleaseDate { get; set; }

        [Required]
        public int UtcOffsetMinutes { get; set; }

        [Required]
        public bool AllowWebSubmit { get; set; }

        [NotMapped]
        [Display(Name = "Release Time")]
        [DataType(DataType.Time)]
        public DateTime ReleaseTime
        {
            get
            {
                return ReleaseDate;
            }
            set
            {
                //first, zero out the release date's time component
                ReleaseDate = DateTime.Parse(ReleaseDate.ToShortDateString());
                ReleaseDate = ReleaseDate.AddHours(value.Hour);
                ReleaseDate = ReleaseDate.AddMinutes(value.Minute);
            }
        }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [NotMapped]
        [Display(Name = "Due Time")]
        [DataType(DataType.Time)]
        public DateTime DueTime
        {
            get
            {
                return DueDate;
            }
            set
            {
                //first, zero out the due date's time component
                DueDate = DateTime.Parse(DueDate.ToShortDateString());
                DueDate = DueDate.AddHours(value.Hour);
                DueDate = DueDate.AddMinutes(value.Minute);
            }
        }

        public Assignment()
        {
            ReleaseDate = DateTime.UtcNow;
            DueDate = DateTime.UtcNow;
            UtcOffsetMinutes = 0;
            AllowWebSubmit = false;
        }

        public Assignment(Assignment other)
            : this()
        {
            Id = other.Id;
            CourseId = other.CourseId;
            DueDate = other.DueDate;
            ReleaseDate = other.ReleaseDate;
            UtcOffsetMinutes = other.UtcOffsetMinutes;
            Name = other.Name;
        }
    }
}
