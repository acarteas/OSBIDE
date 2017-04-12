using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.FileSystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    [RequiresVisualStudioConnectionForStudents]
    public class FileController : ControllerBase
    {
        //
        // GET: /File/
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Returns the requested file attached to the supplied assignment.
        /// </summary>
        /// <param name="assignmentId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ActionResult GetAssignmentAttachment(int assignmentId, string file)
        {
            Assignment assignment = Db.Assignments.Where(a => a.Id == assignmentId).FirstOrDefault();
            if(assignment == null)
            {
                //assignment does not exist
                return RedirectToAction("Index", "Feed");
            }

            FileSystem fs = new FileSystem();
            FileCollection collection = fs.Course(assignment.CourseId).Assignment(assignment).Attachments().File(file);
            if(collection.Count == 0)
            {
                //file does not exist
                return RedirectToAction("Details", "Course", new { id = assignment.CourseId });
            }
            FileStream stream = System.IO.File.OpenRead(collection.FirstOrDefault());
            return new FileStreamResult(stream, "application/octet-stream") { FileDownloadName = Path.GetFileName(collection.FirstOrDefault()) };
        }

        /// <summary>
        /// Returns the requested course document.
        /// </summary>
        /// <param name="courseId"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ActionResult GetCourseDocument(int courseId, string file)
        {
            Course course = Db.Courses.Where(a => a.Id == courseId).FirstOrDefault();
            if (course == null)
            {
                //course does not exist
                return RedirectToAction("Index", "Feed");
            }

            FileSystem fs = new FileSystem();
            FileCollection collection = fs.Course(course).CourseDocs().File(file);
            if (collection.Count == 0)
            {
                //file does not exist
                return RedirectToAction("Details", "Course", new { id = course.Id });
            }
            FileStream stream = System.IO.File.OpenRead(collection.FirstOrDefault());
            return new FileStreamResult(stream, "application/octet-stream") { FileDownloadName = Path.GetFileName(collection.FirstOrDefault()) };
        }
	}
}