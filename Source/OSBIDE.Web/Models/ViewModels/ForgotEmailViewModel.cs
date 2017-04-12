using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using OSBIDE.Library.Models;

namespace OSBIDE.Web.Models.ViewModels
{
    public class ForgotEmailViewModel
    {
        [Required(ErrorMessage="Please select a school or instituion.")]
        [Display(Name="School / Institution")]
        public int SchoolId { get; set; }

        [Required(ErrorMessage = "Please enter your school or instituion ID number.")]
        [Display(Name = "School / Institution ID")]
        public int InstitutionId { get; set; }

        public List<School> Schools { get; set; }

        public string EmailAddress { get; set; }

        public ForgotEmailViewModel()
        {
            Schools = new List<School>();
            EmailAddress = null;
        }
    }
}