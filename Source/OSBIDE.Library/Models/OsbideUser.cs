using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Drawing;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;

namespace OSBIDE.Library.Models
{
    [Flags]
    public enum SystemRole : int
    {
        Student = 1,
        TA = 2,
        Instructor = 4,
        Admin = 8
    }

    public enum Gender : int
    { 
        Unknown = 1,
        Male,
        Female
    }

    [Serializable]
    [DataContract]
    public class OsbideUser : INotifyPropertyChanged, IModelBuilderExtender
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };
        private int _id;
        private string _email;
        private string _firstName = "";
        private string _lastName = "";
        private int _schoolId;
        private int _institutionId = -1;
        private bool _receiveNotificationEmails = false;
        private bool _receiveEmailOnNewAskForHelp = false;
        private bool _receiveEmailOnNewFeedPost = false;
        private int _defaultCourseId;
        private bool _hasInformedConsent = false;
        private DateTime _lastVsActivity;

        [NonSerialized]
        private ProfileImage _profileImage;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        [Key]
        [DataMember]
        public int Id
        {
            get
            {
                return _id;
            }
            set
            {
                _id = value;
                OnPropertyChanged("Id");
            }
        }

        public bool ReceiveNotificationEmails
        {
            get
            {
                return _receiveNotificationEmails;
            }
            set
            {
                _receiveNotificationEmails = value;
                OnPropertyChanged("ReceiveNotificationEmails");
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your email address.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        [DataMember]
        public string Email
        {
            get
            {
                return _email;
            }
            set
            {
                _email = value;
                OnPropertyChanged("Email");
            }
        }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your first name.")]
        [Display(Name = "First Name")]
        [DataMember]
        public string FirstName
        {
            get
            {
                return _firstName;
            }
            set
            {
                _firstName = value.Trim();
                OnPropertyChanged("FirstName");
            }
        }



        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your last name.")]
        [Display(Name = "Last Name")]
        [DataMember]
        public string LastName
        {
            get
            {
                return _lastName;
            }
            set
            {
                _lastName = value.Trim();
                OnPropertyChanged("LastName");
            }
        }

        /// <summary>
        /// SchoolId references the name of the school or institution that the user belongs to.  For example,
        /// if a student attended Washington State University, the SchoolId would match that of Washington State University.
        /// I know this is confusing :(
        /// </summary>
        [Display(Name = "School / Institution")]
        [Required(ErrorMessage = "Please select your school or institution.")]
        [DataMember]
        public int SchoolId
        {
            get
            {
                return _schoolId;
            }
            set
            {
                _schoolId = value;
                OnPropertyChanged("SchoolId");
            }
        }

        [ForeignKey("SchoolId")]
        public virtual School SchoolObj { get; set; }

        /// <summary>
        /// InstitutionId references the user's ID number at their particular institution.  For example,
        /// If a student attended WSU and their school ID was 123456, their InstitutionId would be "123456."  
        /// I know this is confusing :(
        /// </summary>
        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your school or institution ID.")]
        [Display(Name = "School / Institution / Student ID")]
        [DataMember]
        public int InstitutionId
        {
            get
            {
                return _institutionId;
            }
            set
            {
                _institutionId = value;
                OnPropertyChanged("InstitutionId");
            }
        }

        [DataMember]
        public virtual ProfileImage ProfileImage
        {
            get
            {
                return _profileImage;
            }
            set
            {
                _profileImage = value;
            }
        }

        /// <summary>
        /// The numeric representation of the user's role within OSBIDE.  
        /// Use <see cref="Role"/> instead.
        /// </summary>
        [DataMember]
        [Required]
        public int RoleValue { get; protected set; }

        /// <summary>
        /// The user's role within the OSBIDE system
        /// </summary>
        [NotMapped]
        public SystemRole Role
        {
            get
            {
                return (SystemRole)RoleValue;
            }
            set
            {
                RoleValue = (int)value;
                OnPropertyChanged("Role");
            }
        }

        /// <summary>
        /// The numeric representation of the user's gender within OSBIDE.  
        /// Use <see cref="Gender"/> instead.
        /// </summary>
        [DataMember]
        [Required]
        [Display(Name = "Gender")]
        public int GenderValue { get; protected set; }

        /// <summary>
        /// The user's gender within the OSBIDE system
        /// </summary>
        [NotMapped]
        public Gender Gender
        {
            get
            {
                return (Gender)GenderValue;
            }
            set
            {
                GenderValue = (int)value;
                OnPropertyChanged("Gender");
            }
        }

        /// <summary>
        /// Tracks when the user's Visual Studio client was last active.
        /// Users can only access the web portion of OSBIDE if they have an
        /// active connection through Visual Studio.
        /// </summary>
        [DataMember]
        [Required]
        public DateTime LastVsActivity
        {
            get
            {
                return _lastVsActivity;
            }
            set
            {
                _lastVsActivity = value;
                OnPropertyChanged("LastVsActivity");
            }
        }



        /// <summary>
        /// Returns the User's full name in "Last, First" format.
        /// </summary>
        [NotMapped]
        public string FullName
        {
            get
            {
                return string.Format("{0}, {1}", LastName, FirstName);
            }
        }

        /// <summary>
        /// Returns the user's full name in "First Last" format
        /// </summary>
        [NotMapped]
        public string FirstAndLastName
        {
            get
            {
                return string.Format("{0} {1}", FirstName, LastName);
            }
        }

        public bool ReceiveEmailOnNewAskForHelp
        {
            get
            {
                return _receiveEmailOnNewAskForHelp;
            }
            set
            {
                _receiveEmailOnNewAskForHelp = value;
                OnPropertyChanged("ReceiveEmailOnNewAskForHelp");
            }
        }

        public bool ReceiveEmailOnNewFeedPost
        {
            get
            {
                return _receiveEmailOnNewFeedPost;
            }
            set
            {
                _receiveEmailOnNewFeedPost = value;
                OnPropertyChanged("ReceiveEmailOnNewFeedPost");
            }
        }

        public bool HasInformedConsent
        {
            get
            {
                return _hasInformedConsent;
            }
            set
            {
                _hasInformedConsent = value;
                OnPropertyChanged("ReceiveEmailOnNewFeedPost");
            }
        }

        public int DefaultCourseId
        {
            get
            {
                return _defaultCourseId;
            }
            set
            {
                _defaultCourseId = value;
                OnPropertyChanged("DefaultCourseId");
            }
        }
        public virtual Course DefaultCourse { get; set; }

        [IgnoreDataMember]
        public virtual IList<EventLogSubscription> LogSubscriptions { get; set; }

        [IgnoreDataMember]
        public virtual IList<CourseUserRelationship> CourseUserRelationships { get; set; }

        public virtual UserScore Score { get; set; }

        public void SetProfileImage(System.Drawing.Bitmap bmp)
        {
            if (ProfileImage == null)
            {
                ProfileImage = new ProfileImage();
            }
            ProfileImage.SetProfileImage(bmp);
            OnPropertyChanged("ProfileImage");
        }

        public static OsbideUser GenericUser()
        {
            return new OsbideUser()
            {
                FirstName = "Ann",
                LastName = "Onymous",
                InstitutionId = -1
            };
        }

        public OsbideUser()
        {
            LogSubscriptions = new List<EventLogSubscription>();
            CourseUserRelationships = new List<CourseUserRelationship>();

            Role = SystemRole.Student;
            LastVsActivity = DateTime.UtcNow;
            ReceiveNotificationEmails = false;
            Gender = Gender.Unknown;
        }

        public OsbideUser(OsbideUser copyUser)
            : this()
        {
            Role = copyUser.Role;
            Id = copyUser.Id;
            FirstName = copyUser.FirstName;
            LastName = copyUser.LastName;
            InstitutionId = copyUser.InstitutionId;
            Email = copyUser.Email;
            SchoolId = copyUser.SchoolId;
            LastVsActivity = copyUser.LastVsActivity;
            ReceiveNotificationEmails = copyUser.ReceiveNotificationEmails;
            ReceiveEmailOnNewAskForHelp = copyUser.ReceiveEmailOnNewAskForHelp;
            ReceiveEmailOnNewFeedPost = copyUser.ReceiveEmailOnNewFeedPost;
            Gender = copyUser.Gender;
        }

        public void BuildRelationship(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OsbideUser>()
                .HasRequired(u => u.SchoolObj)
                .WithMany()
                .WillCascadeOnDelete(true);
        }
    }
}
