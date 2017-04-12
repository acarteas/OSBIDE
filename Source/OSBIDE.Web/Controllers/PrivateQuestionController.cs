using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    public class PrivateQuestionController : ControllerBase
    {
        public ActionResult Index()
        {
            ViewBag.QuestionSubmitted = false;
            PrivateQuestion question = new PrivateQuestion();
            question.UserId = CurrentUser.Id;
            return View(question);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(PrivateQuestion model)
        {
            model.SubmissionDate = DateTime.UtcNow;
            try
            {
                Db.PrivateQuestions.Add(model);
                Db.SaveChanges();
            }
            catch (Exception)
            {
            }
            model.Question = "";
            ViewBag.QuestionSubmitted = true;
            return View(model);
        }

        [DenyAccess(SystemRole.Student)]
        public ActionResult ViewQuestions()
        {
            DateTime tenDaysAgo = DateTime.Now.AddDays(-10);
            List<PrivateQuestion> questions = Db.PrivateQuestions.Where(q => q.SubmissionDate >= tenDaysAgo).OrderBy(q => q.SubmissionDate).ToList();
            return View(questions);
        }

    }
}
