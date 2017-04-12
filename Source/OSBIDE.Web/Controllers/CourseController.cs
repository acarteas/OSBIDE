using Ionic.Zip;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.FileSystem;
using OSBIDE.Web.Models.ViewModels;
using OSBIDE.Web.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    public class CourseController : ControllerBase
    {
        //
        // GET: /Courses/

        public ActionResult Index()
        {
            //try to find default course
            int courseId = CurrentUser.DefaultCourseId;
            if (courseId <= 0)
            {
                CourseUserRelationship cur = CurrentUser.CourseUserRelationships.FirstOrDefault();
                if (cur != null)
                {
                    courseId = cur.CourseId;
                    return RedirectToAction("Details", new { id = courseId });
                }
            }
            else
            {
                return RedirectToAction("Details", new { id = courseId });
            }

            //no course found, redirect
            return RedirectToAction("NoCourses");
        }

        public ActionResult NoCourses()
        {
            return View();
        }

        /// <summary>
        /// Will set the supplied course ID as the default course for the student
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult MakeDefault(int id)
        {
            Course defaultCourse = Db.Courses.Where(c => c.Id == id).FirstOrDefault();
            if (defaultCourse != null)
            {
                CurrentUser.DefaultCourseId = defaultCourse.Id;
            }
            return RedirectToAction("Details", new { id = id });
        }

        /// <summary>
        /// Will create a new assignment for a given course.  When complete, will redirect to the assignment's course page
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateAssignment(Assignment vm)
        {
            //make sure that assignment creation is allowed
            CourseUserRelationship relationship = Db.CourseUserRelationships
                                                    .Where(cr => cr.CourseId == vm.CourseId)
                                                    .Where(cr => cr.UserId == CurrentUser.Id)
                                                    .FirstOrDefault();
            if (relationship != null)
            {
                if (relationship.Role == CourseRole.Coordinator)
                {
                    vm.IsDeleted = false;

                    //Adjust to UTC
                    vm.DueDate = vm.DueDate.AddMinutes(vm.UtcOffsetMinutes);
                    vm.ReleaseDate = vm.ReleaseDate.AddMinutes(vm.UtcOffsetMinutes);

                    //AC note: I'm not using ModelState.IsValid because it's flagging the non-mapped ReleaseTime/DueTime as invalid. 
                    //As such, there's potential for the db insert to go bad.  Thus, the try/catch.
                    try
                    {
                        Db.Assignments.Add(vm);
                        Db.SaveChanges();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return RedirectToAction("Details", new { id = vm.CourseId });
        }

        /// <summary>
        /// Deletes a given assignment from the system
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult DeleteAssignment(int id)
        {
            Assignment some_assignment = Db.Assignments.Where(a => a.Id == id).FirstOrDefault();
            if (some_assignment != null)
            {
                //make sure the current user is a course coordinator
                CourseUserRelationship relationship = some_assignment.Course.CourseUserRelationships.Where(c => c.UserId == CurrentUser.Id).FirstOrDefault();
                if (relationship != null)
                {
                    if (relationship.Role == CourseRole.Coordinator)
                    {
                        some_assignment.IsDeleted = true;
                        Db.SaveChanges();
                        return RedirectToAction("Details", new { id = some_assignment.CourseId });
                    }
                }
            }
            return RedirectToAction("MyCourses");
        }

        /// <summary>
        /// Attaches a document to the "CourseDocs" section of a course
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UploadCourseFile(int id)
        {
            Course vm = Db.Courses.Where(a => a.Id == id).FirstOrDefault();
            return View(vm);
        }

        /// <summary>
        /// Attaches a document to the "CourseDocs" section of a course
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadCourseFile()
        {
            //make sure that we have a course id
            int courseId = 0;
            Int32.TryParse(Request.Form["CourseId"], out courseId);
            if (courseId < 1)
            {
                return RedirectToAction("MyCourses");
            }
            FileSystem fs = new FileSystem();

            //save files to assignment
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase file = Request.Files[i];
                string fileName = Path.GetFileName(file.FileName);
                fs.Course(courseId).CourseDocs().AddFile(fileName, file.InputStream);
            }
            return RedirectToAction("Details", new { id = courseId });
        }

        /// <summary>
        /// Deletes the supplied file from a course's "CourseDocs" section.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ActionResult DeleteCourseFile(int id, string file)
        {
            Course course = Db.Courses.Where(a => a.Id == id).FirstOrDefault();
            if (course == null)
            {
                //course not found
                return RedirectToAction("MyCourses");
            }

            //can the user delete files?
            CourseUserRelationship relationship = course.CourseUserRelationships.Where(cu => cu.UserId == CurrentUser.Id).FirstOrDefault();
            if (relationship != null)
            {
                if (relationship.Role == CourseRole.Coordinator)
                {
                    //it's okay to delete files
                    FileSystem fs = new FileSystem();
                    FileCollection collection = fs.Course(course).CourseDocs().File(file);
                    collection.Delete();
                }
            }
            return RedirectToAction("Details", new { id = course.Id });
        }

        /// <summary>
        /// Attaches one or more files to the specified assignment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult UploadAssignmentFile(int id)
        {
            Assignment vm = Db.Assignments.Where(a => a.Id == id).FirstOrDefault();
            return View(vm);
        }

        /// <summary>
        /// Attaches one or more files to the specified assignment
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadAssignmentFile()
        {
            //make sure that we have both a course id and assignment id
            int assignmentId = 0;
            int courseId = 0;
            Int32.TryParse(Request.Form["AssignmentId"], out assignmentId);
            Int32.TryParse(Request.Form["CourseId"], out courseId);
            if (courseId < 1 || assignmentId < 1)
            {
                return RedirectToAction("MyCourses");
            }
            FileSystem fs = new FileSystem();

            //save files to assignment
            for (int i = 0; i < Request.Files.Count; i++)
            {
                HttpPostedFileBase file = Request.Files[i];
                string fileName = Path.GetFileName(file.FileName);
                fs.Course(courseId).Assignment(assignmentId).Attachments().AddFile(fileName, file.InputStream);
            }
            return RedirectToAction("Details", new { id = courseId });
        }

        /// <summary>
        /// Deletes the specified from from the specified assignment
        /// </summary>
        /// <param name="id">The ID of the assignment that the file is attached to</param>
        /// <param name="file"></param>
        /// <returns></returns>
        public ActionResult DeleteAssignmentFile(int id, string file)
        {
            Assignment assignment = Db.Assignments.Where(a => a.Id == id).FirstOrDefault();
            if (assignment == null)
            {
                //assignment not found
                return RedirectToAction("MyCourses");
            }

            //can the user delete files?
            CourseUserRelationship relationship = assignment.Course.CourseUserRelationships.Where(cu => cu.UserId == CurrentUser.Id).FirstOrDefault();
            if (relationship != null)
            {
                if (relationship.Role == CourseRole.Coordinator)
                {
                    //it's okay to delete files
                    FileSystem fs = new FileSystem();
                    FileCollection collection = fs.Course(assignment.CourseId).Assignment(assignment).Attachments().File(file);
                    collection.Delete();
                }
            }
            return RedirectToAction("Details", new { id = assignment.CourseId });
        }

        /// <summary>
        /// Loads the details view of a course.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Details(int id = -1)
        {
            Course currentCourse = (from course in Db.Courses
                                   .Include("Assignments")
                                   .Include("CourseUserRelationships")
                                   .Include("CourseUserRelationships.User")
                                    where course.Id == id
                                    select course).FirstOrDefault();

            //bad ID or invalid course, redirect
            if (id == -1 || currentCourse == null)
            {
                return RedirectToAction("Index", "Feed");
            }

            //build VM
            CourseDetailsViewModel vm = new CourseDetailsViewModel();
            vm.CurrentCourse = currentCourse;
            vm.Assignments = currentCourse.Assignments.Where(a => a.IsDeleted == false).ToList();
            vm.CurrentUser = CurrentUser;
            vm.Coordinators = currentCourse.CourseUserRelationships.Where(c => c.Role == CourseRole.Coordinator).Select(u => u.User).ToList();

            //figure out what files are attached to various assignments
            FileSystem fs = new FileSystem();
            foreach (Assignment assignment in currentCourse.Assignments)
            {
                //ignore "deleted" assignments
                if (assignment.IsDeleted == true)
                {
                    continue;
                }
                vm.AssignmentFiles.Add(assignment.Id, new List<string>());
                FileCollection files = fs.Course(currentCourse).Assignment(assignment).Attachments().AllFiles();
                foreach (string file in files)
                {
                    vm.AssignmentFiles[assignment.Id].Add(Path.GetFileName(file));
                }
            }

            //find all course documents
            List<string> courseFiles = fs.Course(currentCourse).CourseDocs().AllFiles().ToList();
            foreach (string file in courseFiles)
            {
                vm.CourseDocuments.Add(Path.GetFileName(file));
            }

            return View(vm);
        }

        public JsonResult GetAllCourses()
        {
            CoursesViewModel vm = BuildCoursesViewModel();
            var simpleCourse = vm.CoursesByPrefix.Select(c => new
            {
                Prefix = c.Key,
                Courses = c.Value.Select(
                    co => new
                    {
                        Prefix = c.Key,
                        CourseNumber = co.Value.CourseNumber,
                        Description = co.Value.Description
                    }
                )
            });
            return this.Json(simpleCourse, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Provides an interface for finding new courses in OSBIDE
        /// </summary>
        /// <returns></returns>
        public ActionResult Search()
        {
            CoursesViewModel vm = BuildCoursesViewModel();
            return View(vm);
        }

        /// <summary>
        /// Provides an interface for finding new courses in OSBIDE
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Search(CoursesViewModel vm)
        {
            vm = BuildCoursesViewModel(vm);
            if (vm.SelectedCourse > 0)
            {
                Course toJoin = Db.Courses.Where(c => c.Id == vm.SelectedCourse).FirstOrDefault();
                if (toJoin != null)
                {
                    CourseUserRelationship cur = new CourseUserRelationship()
                    {
                        CourseId = toJoin.Id,
                        UserId = CurrentUser.Id,
                        Role = CourseRole.Student,
                        IsApproved = !toJoin.RequiresApprovalBeforeAdmission,
                        IsActive = true
                    };
                    Db.CourseUserRelationships.Add(cur);
                    Db.SaveChanges();

                    if (toJoin.RequiresApprovalBeforeAdmission == true)
                    {
                        vm.ServerMessage = string.Format("A request to join {0} has been sent to the course instructor.  Until then, certain features related to the course may be unavailable.", toJoin.Name);
                    }
                    else
                    {
                        vm.ServerMessage = string.Format("You are now enrolled in {0}.", toJoin.Name);
                    }
                }
                else
                {
                    vm.ServerMessage = "There server experienced an error when trying to add you to the selected course.  Please try again.  If the problem persists, please contact support@osbide.com.";
                }
            }
            return View(vm);
        }

        private CoursesViewModel BuildCoursesViewModel(CoursesViewModel vm = null)
        {
            if (vm == null)
            {
                vm = new CoursesViewModel();
            }
            vm.AllCourses = Db.Courses
                .Where(c => c.SchoolId == CurrentUser.SchoolId)
                .OrderBy(c => c.Prefix)
                .ToList();
            vm.CurrentUser = CurrentUser;

            foreach (Course course in vm.AllCourses)
            {
                string key = string.Format("{0}_{1}_{2}", course.Prefix, course.CourseNumber, course.Season);
                if (vm.CoursesByPrefix.ContainsKey(course.Prefix) == false)
                {
                    vm.CoursesByPrefix.Add(course.Prefix, new SortedDictionary<string, Course>());
                }
                vm.CoursesByPrefix[course.Prefix].Add(key, course);
            }
            return vm;
        }

        // <summary>
        /// Submits one or more files to the specified assignment
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult SubmitAssignmentFile(int id)
        {
            Assignment vm = Db.Assignments.Where(a => a.Id == id).FirstOrDefault();
            return View(vm);
        }

        /// <summary>
        /// Attaches one or more files to the specified assignment
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult SubmitAssignmentFile(HttpPostedFileBase file)
        {
            //make sure that we have both a course id and assignment id
            int assignmentId = 0;
            int courseId = 0;
            Int32.TryParse(Request.Form["AssignmentId"], out assignmentId);
            Int32.TryParse(Request.Form["CourseId"], out courseId);

            if (courseId < 1 || assignmentId < 1)
            {
                return RedirectToAction("MyCourses");
            }
            
            //get file information and continue if not null
            if (file != null)
            {
                //create submit event
                SubmitEvent submitEvent = new SubmitEvent();

                if (file.ContentLength > 0 && file.ContentLength < 5000000) //limit size to 5 MB
                {                    
                    submitEvent.SolutionName = Path.GetFileName(file.FileName);

                    byte[] fileData = null;
                    using (var binaryReader = new BinaryReader(file.InputStream))
                    {
                        fileData = binaryReader.ReadBytes(file.ContentLength);
                    }

                    MemoryStream stream = new MemoryStream();
                    using (ZipFile zip = new ZipFile())
                    {                           
                        zip.AddEntry(submitEvent.SolutionName, fileData);
                        zip.Save(stream);
                        stream.Position = 0;
                    }                    
                    //add the solution data to the event
                    submitEvent.CreateSolutionBinary(stream.ToArray());
                }
                else
                {
                    //TODO: handle specific errors
                    return RedirectToAction("GenericError", "Error");
                }

                submitEvent.AssignmentId = assignmentId;
                //create event log with solution to submit
                EventLog eventLog = new EventLog(submitEvent);
                eventLog.Sender = CurrentUser;
                eventLog.SenderId = CurrentUser.Id;
                //create client to submit assignment to the db
                OsbideWebService client = new OsbideWebService();
                client.SubmitAssignment(assignmentId, eventLog, CurrentUser);

                return RedirectToAction("Details", new { id = courseId });

            }  
            else
            {
                //TODO: handle specific errors
                return RedirectToAction("GenericError", "Error");
            }
        }
    }
}
