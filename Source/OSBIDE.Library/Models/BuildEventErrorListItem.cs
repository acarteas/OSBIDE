using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using OSBIDE.Library.Events;
using System.ComponentModel.DataAnnotations.Schema;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class BuildEventErrorListItem : IModelBuilderExtender
    {
        [Key]
        [Required]
        [Column(Order = 0)]
        public int BuildEventId { get; set; }

        [ForeignKey("BuildEventId")]
        public virtual BuildEvent BuildEvent { get; set; }

        [Key]
        [Required]
        [Column(Order = 1)]
        public int ErrorListItemId { get; set; }

        [ForeignKey("ErrorListItemId")]
        public virtual ErrorListItem ErrorListItem { get; set; }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<BuildEventErrorListItem>()
                .HasRequired(b => b.BuildEvent)
                .WithMany(b => b.ErrorItems)
                .WillCascadeOnDelete(true);

            modelBuilder.Entity<BuildEventErrorListItem>()
                .HasRequired(b => b.ErrorListItem)
                .WithMany()
                .WillCascadeOnDelete(true);
        }
    }
}
