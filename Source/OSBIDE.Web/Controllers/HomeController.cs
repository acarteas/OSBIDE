using DiffMatchPatch;
using OSBIDE.Library;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.ViewModels;
using OSBIDE.Web.Services;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    public class HomeController : ControllerBase
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public int PostComment(string logId, string comment, string returnUrl = "")
        {
            //default return to activity feed
            if (string.IsNullOrEmpty(returnUrl) == true)
            {
                returnUrl = Url.Action("Index", "Feed");
            }
            base.PostComment(logId, comment);
            Response.Redirect(returnUrl);
            return 0;
        }
    }
}
