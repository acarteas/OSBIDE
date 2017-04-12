using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public class ChatRoomUser : IModelBuilderExtender
    {
        [Key]
        [Column(Order=0)]
        [ForeignKey("Room")]
        public int RoomId { get; set; }
        public ChatRoom Room { get; set; }

        [Key]
        [Column(Order=1)]
        [ForeignKey("User")]
        public int UserId { get; set; }
        public OsbideUser User { get; set; }

        public DateTime LastActivity { get; set; }

        public ChatRoomUser()
        {
            LastActivity = DateTime.UtcNow;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatRoomUser>()
                .HasRequired(c => c.Room)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ChatRoomUser>()
                .HasRequired(c => c.User)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}
