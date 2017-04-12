using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    [Serializable]
    public class EventLogSubscription : IModelBuilderExtender
    {
        [Key]
        [Column(Order = 0)]
        [Required]
        public int UserId { get; set; }
        public virtual OsbideUser User { get; set; }

        [Key]
        [Column(Order = 1)]
        [Required]
        public int LogId { get; set; }
        public virtual EventLog Log { get; set; }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            //explicity tell EF what the FK for UserId should be
            modelBuilder.Entity<EventLogSubscription>()
                .HasRequired(s => s.User)
                .WithMany(s => s.LogSubscriptions)
                .HasForeignKey(s => s.UserId);

            //turn off cascade deleting because SQL Server can't handle multiple cascade paths
            modelBuilder.Entity<EventLogSubscription>()
                .HasRequired(s => s.User)
                .WithMany(s => s.LogSubscriptions)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<EventLogSubscription>()
                .HasRequired(s => s.Log)
                .WithMany(s => s.Subscriptions)
                .HasForeignKey(s => s.LogId);

            modelBuilder.Entity<EventLogSubscription>()
                .HasRequired(s => s.Log)
                .WithMany(s => s.Subscriptions)
                .WillCascadeOnDelete(true);
        }
    }
}
