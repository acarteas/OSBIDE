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
    public class CourseUserRelationship : IModelBuilderExtender
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        public int CourseId { get; set; }
        public virtual Course Course { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int UserId { get; set; }
        public virtual OsbideUser User { get; set; }

        [Key]
        [Column(Order = 2)]
        [Required]
        public int RoleType { get; set; }

        [NotMapped]
        public CourseRole Role
        {
            get
            {
                return (CourseRole)RoleType;
            }
            set
            {
                RoleType = (int)value;
            }
        }

        public bool IsApproved { get; set; }

        public bool IsActive { get; set; }

        public CourseUserRelationship()
        {
            IsApproved = true;
            IsActive = true;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseUserRelationship>()
                .HasRequired(c => c.Course)
                .WithMany(c => c.CourseUserRelationships)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<CourseUserRelationship>()
                .HasRequired(c => c.User)
                .WithMany(c => c.CourseUserRelationships)
                .WillCascadeOnDelete(false);
        }
    }
}
