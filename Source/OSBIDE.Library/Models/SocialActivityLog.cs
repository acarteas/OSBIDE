using OSBIDE.Library.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Models
{
    public class CommentActivityLog : IModelBuilderExtender
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int TargetUserId { get; set; }

        [ForeignKey("TargetUserId")]
        public virtual OsbideUser TargetUser { get; set; }

        [Required]
        public int LogCommentEventId { get; set; }

        [ForeignKey("LogCommentEventId")]
        public virtual LogCommentEvent LogCommentEvent { get; set; }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
        }
    }
}
