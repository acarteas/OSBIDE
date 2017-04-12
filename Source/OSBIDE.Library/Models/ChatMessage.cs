using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public class ChatMessage : IModelBuilderExtender
    {
        [Key]
        public int Id { get; set; }

        public int RoomId { get; set; }

        [ForeignKey("RoomId")]
        public ChatRoom Room { get; set; }

        public int AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public OsbideUser Author { get; set; }

        public DateTime MessageDate { get; set; }

        public string Message { get; set; }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChatMessage>()
                .HasRequired(m => m.Author)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}
