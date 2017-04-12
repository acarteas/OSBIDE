using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using OSBIDE.Library;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;
using OSBIDE.Library.Events;
using OSBIDE.Web.Models.Queries;

namespace OSBIDE.Web.Controllers
{
    public class AccountController : ControllerBase
    {
        //
        // GET: /Account/

        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Controller action used to create new accounts
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            List<School> schools = Db.Schools.ToList();
            ViewBag.Schools = schools;
            return View();
        }

        /// <summary>
        /// Postback logic for account creation
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(CreateAccountViewModel vm)
        {
            if (ModelState.IsValid)
            {
                bool userExists = false;
                bool identificationExists = false;
                ViewBag.UserExistsError = false;
                ViewBag.SchoolIdExistsError = false;

                //make sure that the email address is unique
                OsbideUser user = Db.Users.Where(u => u.Email.CompareTo(vm.Email) == 0).FirstOrDefault();
                if (user != null)
                {
                    ModelState.AddModelError("", "A user with that email already exists.");
                    userExists = true;
                    ViewBag.UserExistsError = true;
                }

                //make sure that the institution id has not been used for the selected school
                user = (from u in Db.Users
                        where u.SchoolId.CompareTo(vm.User.SchoolId) == 0
                        && u.InstitutionId.CompareTo(vm.User.InstitutionId) == 0
                        select u
                       ).FirstOrDefault();
                if (user != null)
                {
                    ModelState.AddModelError("", "There already exists a user at the selected institution with that ID number.");
                    identificationExists = true;
                    ViewBag.SchoolIdExistsError = true;
                }

                //only continue if we were provided with a unique email and school id
                if (userExists == false && identificationExists == false)
                {
                    vm.User.Email = vm.Email;
                    vm.User.Id = 0;
                    vm.User.DefaultCourseId = 1;

                    Db.Users.Add(vm.User);
                    Db.SaveChanges();

                    //add password information
                    UserPassword password = new UserPassword();
                    password.UserId = vm.User.Id;
                    password.Password = UserPassword.EncryptPassword(vm.Password, vm.User);
                    Db.UserPasswords.Add(password);
                    Db.SaveChanges();

                    //add default feed options
                    UserFeedSetting feedSetting = new UserFeedSetting();
                    feedSetting.UserId = vm.User.Id;
                    foreach (var evt in ActivityFeedQuery.GetSocialEvents())
                    {
                        feedSetting.SetSetting(evt, true);
                    }
                    Db.UserFeedSettings.Add(feedSetting);
                    Db.SaveChanges();

                    //enroll them in OSBIDE 101
                    Db.CourseUserRelationships.Add(new CourseUserRelationship() { UserId = vm.User.Id, CourseId = 1, Role = CourseRole.Student });
                    vm.User.DefaultCourseId = 1;
                    Db.SaveChanges();

                    //log user in
                    Authentication auth = new Authentication();
                    auth.LogIn(vm.User);

                    //redirect to profile page
                    return RedirectToAction("Index", "Feed");
                }
            }

            //shouldn't get here unless we received an error
            //call base Create logic
            Create();
            return View();
        }

        public ActionResult CreateComplete()
        {
            return View();
        }

        /// <summary>
        /// Action responsible for logging users into the system
        /// </summary>
        /// <returns></returns>
        public ActionResult Login(string returnUrl = null)
        {
            LoginViewModel vm = new LoginViewModel() { ReturnUrl = returnUrl };
            return View(vm);
        }

        /// <summary>
        /// Postback logic for login
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(LoginViewModel vm)
        {
            if (ModelState.IsValid)
            {
                if (UserPassword.ValidateUser(vm.UserName, vm.Password, Db))
                {
                    Authentication auth = new Authentication();
                    OsbideUser user = Db.Users.Where(u => u.Email.CompareTo(vm.UserName) == 0).FirstOrDefault();
                    if (user != null)
                    {
                        auth.LogIn(user);

                        //did the user come from somewhere?
                        if (string.IsNullOrEmpty(vm.ReturnUrl) == false)
                        {
                            Response.Redirect(vm.ReturnUrl);
                        }

                        return RedirectToAction("Index", "Profile");
                    }
                }
            }

            //if we got this far, must've had a bad user name or password
            ModelState.AddModelError("", "Invalid email or password.");
            return View();
        }

        /// <summary>
        /// Logs the user out of the system
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOut()
        {
            Authentication auth = new Authentication();
            auth.LogOut();
            return RedirectToAction("Login");
        }

        public ActionResult ForgotPassword()
        {
            ForgotPasswordViewModel vm = new ForgotPasswordViewModel();
            vm.Schools = Db.Schools.ToList();
            return View(vm);
        }

        [HttpPost]
        public ActionResult ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (ModelState.IsValid)
            {
                OsbideUser user = Db.Users.Where(e => e.Email.ToLower() == vm.EmailAddress.ToLower()).FirstOrDefault();
                if (user != null)
                {

                    if (user.SchoolId == vm.SchoolId && user.InstitutionId == vm.InstitutionId)
                    {
                        Authentication auth = new Authentication();
                        string newPassword = auth.GenerateRandomString(7);
                        UserPassword password = Db.UserPasswords.Where(up => up.UserId == user.Id).FirstOrDefault();
                        if (password != null)
                        {
                            //update password
                            password.Password = UserPassword.EncryptPassword(newPassword, user);
                            Db.SaveChanges();

                            //send email
                            string body = "Your OSBIDE password has been reset.\n Your new password is: \"" + newPassword + "\".\n\nPlease change this password as soon as possible.";
                            List<MailAddress> to = new List<MailAddress>();
                            to.Add(new MailAddress(user.Email));
                            Email.Send("[OSBIDE] Password Reset Request", body, to);
                            vm.PasswordResetRequestComplete = true;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Account not found.  Please check the supplied email address and institution information.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Account not found.  Please check the supplied email address and institution information.");
                }
            }
            else
            {
                ModelState.AddModelError("", "Account not found.  Please check the supplied email address and institution information.");
            }

            vm.Schools = Db.Schools.ToList();
            return View(vm);
        }

        public ActionResult ForgotEmail()
        {
            ForgotEmailViewModel vm = new ForgotEmailViewModel();
            vm.Schools = Db.Schools.ToList();
            return View(vm);
        }

        [HttpPost]
        public ActionResult ForgotEmail(ForgotEmailViewModel vm)
        {
            if (ModelState.IsValid)
            {
                OsbideUser user = (from u in Db.Users
                                   where u.SchoolId.CompareTo(vm.SchoolId) == 0
                                   && u.InstitutionId.CompareTo(vm.InstitutionId) == 0
                                   select u).FirstOrDefault();
                if (user != null)
                {
                    vm.EmailAddress = user.Email;
                }
                else
                {
                    ModelState.AddModelError("", "No account matches the supplied institution and ID number provided.");
                }
            }
            vm.Schools = Db.Schools.ToList();
            return View(vm);
        }
    }
}
