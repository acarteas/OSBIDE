using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class AnswerCoding : IModelBuilderExtender
    {
        [Key]
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public virtual Post Question { get; set; }
        public int AnswerId { get; set; }
        public virtual Post Answer { get; set; }
        public bool AcknowledgedReply { get; set; }
        public int AuthorStudentId { get; set; }

        /// <summary>
        /// Turns off cascade on delete for FK relationships
        /// </summary>
        /// <param name="modelBuilder"></param>
        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AnswerCoding>()
                .HasRequired(ac => ac.Question)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AnswerCoding>()
                .HasRequired(ac => ac.Answer)
                .WithMany()
                .WillCascadeOnDelete(false);

        }
    }
}
