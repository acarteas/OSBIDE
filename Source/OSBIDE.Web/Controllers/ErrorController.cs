using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    public class ErrorController : ControllerBase
    {
        //
        // GET: /Error/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GenericError(string message="")
        {
            return View(message);
        }

        public ActionResult RequiresActiveVsConnection()
        {
            return View();
        }

        public ActionResult FeedDown()
        {
            return View();
        }
    }
}
