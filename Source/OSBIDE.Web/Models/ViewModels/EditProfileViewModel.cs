using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;

namespace OSBIDE.Web.Models.ViewModels
{
    public class EditProfileViewModel
    {
        public OsbideUser User { get; set; }

        [Email(ErrorMessage = "Invalid email address.")]
        [DataType(DataType.EmailAddress)]
        public string NewEmail { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("NewEmail", ErrorMessage = "Email addresses do not match.")]
        public string NewEmailConfirm { get; set; }
        public string NewPassword { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("NewPassword", ErrorMessage = "Passwords do not match.")]
        public string NewPasswordConfirm { get; set; }
        public string OldPassword { get; set; }

        public bool ReceiveEmailNotificationsForPosts { get; set; }
        public bool ReceiveEmailsOnNewAskForHelp { get; set; }
        public bool ReceiveEmailsOnFeedPost { get; set; }

        public string UpdateEmailSuccessMessage { get; set; }
        public string UpdatePasswordSuccessMessage { get; set; }
        public string UpdateEmailSettingsMessage { get; set; }
        public string UpdateBasicSettingsMessage { get; set; }
        public string RemoveCourseMessage { get; set; }

        public int LastActivePane { get; set; }

        public List<OsbideUser> UsersInCourse { get; set; }
        public Dictionary<int, UserSubscription> UserSubscriptions { get; set; }

        public EditProfileViewModel()
        {
            ReceiveEmailNotificationsForPosts = false;
            ReceiveEmailsOnNewAskForHelp = false;
            ReceiveEmailsOnFeedPost = false;
            UsersInCourse = new List<OsbideUser>();
            UserSubscriptions = new Dictionary<int, UserSubscription>();
        }
    }
}
