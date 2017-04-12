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
    public class CreateAccountViewModel
    {
        public OsbideUser User { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Please enter your email address.")]
        [Email(ErrorMessage = "Invalid email address.")]
        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email Address")]
        public string Email { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("Email", ErrorMessage = "Email addresses do not match.")]
        [Display(Name = "Confirm Email Address")]
        public string EmailVerification { get; set; }

        [MinLength(4, ErrorMessage = "Please enter a password (minimum lenght = 4).")]
        [Required(AllowEmptyStrings = false,
                  ErrorMessage = "Please enter a password (minimum lenght = 4)."
                  )
        ]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string PasswordVerification { get; set; }

    }
}