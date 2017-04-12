using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    /// <summary>
    /// Represents a list of people (subjects) that a particular user (observer) follows.  The institution IDs of each user are used to
    /// identify each user.  Institution IDs are used rather instead of User IDs in order to allow the mass importing of subscriptions
    /// from class roster files.  By using institution IDs, we can create relationships before a user has been registered in the system.
    /// Then, upon registration, a user will automatically be subscribed to their core user group.
    /// </summary>
    [Serializable]
    public class UserSubscription : IModelBuilderExtender
    {
        [Key]
        [Column(Order=0)]
        public int ObserverInstitutionId { get; set; }

        [Key]
        [Column(Order = 1)]
        public int ObserverSchoolId { get; set; }
        public virtual School ObserverSchool { get; set; }

        [Key]
        [Column(Order = 2)]
        public int SubjectInstitutionId { get; set; }

        [Key]
        [Column(Order = 3)]
        public int SubjectSchoolId { get; set; }
        public virtual School SubjectSchool { get; set; }

        /// <summary>
        /// Some subscriptions may be required and should not be allowed to be deleted by users.  
        /// For example, a student should always be subscribed to his or her instructor's feed.
        /// </summary>
        public bool IsRequiredSubscription { get; set; }

        public UserSubscription()
        {
            IsRequiredSubscription = false;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserSubscription>()
                .HasRequired(s => s.ObserverSchool)
                .WithMany()
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<UserSubscription>()
                .HasRequired(s => s.SubjectSchool)
                .WithMany()
                .WillCascadeOnDelete(false);
        }
    }
}
