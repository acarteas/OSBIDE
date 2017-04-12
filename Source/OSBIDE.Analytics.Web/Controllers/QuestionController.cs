using OSBIDE.Analytics.Library.Models;
using OSBIDE.Analytics.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Analytics.Web.Controllers
{
    public class QuestionController : ControllerBase
    {
        // GET: Question
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(QuestionCoding model)
        {
            if (model.AuthorStudentId > 0)
            {
                return RedirectToAction("GetNextQuestion", new { studentId = model.AuthorStudentId });
            }
            else
            {
                return View();
            }
        }

        public ActionResult GetNextQuestion(int studentId)
        {

            var answeredQuestionsQuery = (from question in Db.QuestionCodings
                                          where question.AuthorStudentId == studentId
                                          select question.PostId).ToList();
            
            var findQuestionQuery = (from question in Db.QuestionResponseCounts
                                     where answeredQuestionsQuery.Contains(question.Id) == false
                                     orderby question.Count ascending
                                     select question).Take(1);
            var toFind = findQuestionQuery.FirstOrDefault();
            var post = (from question in Db.Posts
                          where question.Id == toFind.ContentId
                          select question).FirstOrDefault();
            GetNextQuestionViewModel vm = new GetNextQuestionViewModel() 
            {
                Post = post,
                StudentId = studentId
            };
            return View(vm);
        }

        [HttpPost]
        public ActionResult GetNextQuestion(GetNextQuestionViewModel vm)
        {
            //add coding
            QuestionCoding coding = new QuestionCoding()
            {
                AuthorStudentId = vm.StudentId,
                PostId = vm.Post.Id,
                IsQuestion = vm.IsQuestion,
            };
            Db.QuestionCodings.Add(coding);

            //update response count
            QuestionResponseCount count = Db.QuestionResponseCounts.Where(c => c.ContentId == vm.Post.Id).FirstOrDefault();
            count.Count++;
            Db.Entry(count).State = System.Data.Entity.EntityState.Modified;
            Db.SaveChanges();
            return RedirectToAction("GetNextQuestion", new { studentId = vm.StudentId });
        }
    }
}