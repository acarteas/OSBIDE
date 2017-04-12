using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    public class WhatsNewController : ControllerBase
    {
        //
        // GET: /WhatsNew/

        public ActionResult Index()
        {
            return View(Db.WhatsNewItems.Take(25).ToList());
        }

        [AllowAccess(SystemRole.Instructor, SystemRole.Admin)]
        [HttpGet]
        public ActionResult Add()
        {
            return View(new WhatsNewItem());
        }

        [ValidateInput(false)]
        [HttpPost]
        public ActionResult Add(WhatsNewItem model)
        {
            if (ModelState.IsValid == true)
            {
                model.DatePosted = DateTime.UtcNow;
                Db.WhatsNewItems.Add(model);
                Db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(model);
        }
    }
}
