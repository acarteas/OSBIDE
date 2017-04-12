using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using OSBIDE.Library.Models;

namespace OSBIDE.Web.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage="Please select a school or instituion.")]
        [Display(Name="School / Institution")]
        public int SchoolId { get; set; }

        [Required(ErrorMessage = "Please enter your school or instituion ID number.")]
        [Display(Name = "School / Institution ID")]
        public int InstitutionId { get; set; }

        public List<School> Schools { get; set; }

        [Required(ErrorMessage = "Please enter your email address.")]
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }

        public bool PasswordResetRequestComplete { get; set; }

        public ForgotPasswordViewModel()
        {
            Schools = new List<School>();
            PasswordResetRequestComplete = false;
        }
    }
}