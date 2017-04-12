using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase;
using OSBIDE.Library.CSV;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;

namespace OSBIDE.Web.Controllers
{
    [AllowAccess(SystemRole.Instructor, SystemRole.Admin)]
    public class AdminController : ControllerBase
    {
        //
        // GET: /Admin/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult DailyActivity(int? SelectedStudentId, DateTime? SelectedDate)
        {
            DailyActivityViewModel vm = new DailyActivityViewModel();
            if (SelectedStudentId != null && SelectedDate != null)
            {
                vm.SelectedDate = (DateTime)SelectedDate;
                vm.SelectedStudentId = (int)SelectedStudentId;
                DateTime tomorrow = vm.SelectedDate.AddDays(1);

                //pull event logs
                List<EventLog> logs = Db.EventLogs
                                        .Where(u => u.SenderId == vm.SelectedStudentId)
                                        .Where(e => e.DateReceived > vm.SelectedDate)
                                        .Where(e => e.DateReceived < tomorrow)
                                        .ToList();
                foreach (EventLog log in logs)
                {
                    vm.ActivityItems.Add(log.DateReceived, log);
                }

                //pull request logs from azure storage
                var requestLogs = DomainObjectHelpers.GetActionRequests(1, vm.SelectedStudentId)
                                        .Where(l => l.CreatorId == vm.SelectedStudentId)
                                        .Where(l => l.AccessDate > vm.SelectedDate)
                                        .Where(l => l.AccessDate < tomorrow)
                                        .ToList();
                foreach (OSBIDE.Data.DomainObjects.ActionRequestLog log in requestLogs)
                {
                    vm.ActivityItems.Add(log.AccessDate, log);
                }

                //pull comments
                List<LogCommentEvent> comments = Db.LogCommentEvents
                                              .Where(c => c.EventLog.SenderId == vm.SelectedStudentId)
                                              .Where(c => c.EventDate > vm.SelectedDate)
                                              .Where(c => c.EventDate < tomorrow)
                                              .ToList();
                foreach (LogCommentEvent comment in comments)
                {
                    vm.ActivityItems.Add(comment.EventDate, comment);
                }

            }
            vm.Students = Db.Users.Where(u => u.RoleValue == (int)SystemRole.Student).OrderBy(s => s.LastName).ToList();

            return View(vm);
        }

        public ActionResult GetCourseDeliverables(int courseId)
        {
            return Json(CriteriaLookupsProc.GetDeliverables(courseId), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UploadRoster(AdminDataImport dataImport)
        {
            ViewBag.UploadResult = true;

            if (Request.Files.Count > 0 && Request.Files[0].ContentLength > 0)
            {
                try
                {
                    var fileExtension = System.IO.Path.GetExtension(dataImport.File.FileName);
                    if (dataImport.Schema != FileUploadSchema.CSV.ToString())
                    {
                        ProcessExcel(dataImport, fileExtension);
                    }
                    else
                    {
                        ProcessCSV(dataImport.File);
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.UploadResult = false;
                    ViewBag.ErrorMessage = ex.Message;
                }
            }

            return View("Index");
        }

        private void ProcessExcel(AdminDataImport dataImport, string fileExtension)
        {
            var postedFile = Request.Files[0];
            var filePath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, System.IO.Path.GetFileName(postedFile.FileName));
            if (System.IO.File.Exists(filePath))
            {

                System.IO.File.Delete(filePath);
            }
            postedFile.SaveAs(filePath);

            if (dataImport.Schema == FileUploadSchema.Grade.ToString())
            {
                ExcelImport.UploadGrades(filePath
                                         , fileExtension
                                         , dataImport.CourseId
                                         , dataImport.Deliverable
                                         , CurrentUser.Id);
            }
            else
            {
                ExcelImport.UploadSurveys(filePath
                                         , fileExtension
                                         , dataImport.CourseId
                                         , CurrentUser.Id);
            }
        }

        private void ProcessCSV(HttpPostedFileBase file)
        {
            List<List<string>> csvList = new List<List<string>>();

            using (CsvReader reader = new CsvReader(file.InputStream))
            {
                csvList = reader.Parse();
            }

            //Process of creating automatic user subscriptions:
            //1. Group all users by section
            //2. For each student in a given section, assign them to all other students in that section
            Dictionary<int, List<int>> sectionGroups = new Dictionary<int, List<int>>();

            //this loop takes care of process step #1
            foreach (List<string> row in csvList)
            {
                if (row.Count == 2)
                {
                    int studentId = 0;
                    int section = 0;
                    if (Int32.TryParse(row[0], out studentId) == true)
                    {
                        if (Int32.TryParse(row[1], out section) == true)
                        {
                            if (sectionGroups.ContainsKey(section) == false)
                            {
                                sectionGroups.Add(section, new List<int>());
                            }
                            if (sectionGroups[section].Contains(studentId) == false)
                            {
                                sectionGroups[section].Add(studentId);
                            }
                        }
                    }
                }
            }

            //this loop takes care of process step #2
            foreach (int section in sectionGroups.Keys)
            {
                List<int> sectionList = sectionGroups[section];
                List<UserSubscription> dbSubs = (from sub in Db.UserSubscriptions
                                                 where
                                                 (
                                                     sectionList.Contains(sub.ObserverInstitutionId)
                                                     || sectionList.Contains(sub.SubjectInstitutionId)
                                                 )
                                                 && sub.ObserverSchoolId == CurrentUser.SchoolId
                                                 select sub
                                                ).ToList();

                foreach (int observerId in sectionGroups[section])
                {
                    foreach (int subjectId in sectionGroups[section])
                    {
                        if (observerId.CompareTo(subjectId) != 0)
                        {
                            UserSubscription dbSub = dbSubs.Where(s => s.ObserverInstitutionId == observerId).Where(s => s.SubjectInstitutionId == subjectId).FirstOrDefault();
                            if (dbSub == null)
                            {
                                UserSubscription sub = new UserSubscription()
                                {
                                    ObserverInstitutionId = observerId,
                                    SubjectInstitutionId = subjectId,

                                    //assume that the subject and observer attend the same institution as the 
                                    //person uploading the CSV file
                                    ObserverSchoolId = CurrentUser.SchoolId,
                                    SubjectSchoolId = CurrentUser.SchoolId
                                };
                                Db.UserSubscriptions.Add(sub);
                            }
                        }
                    }
                }
            }

            //save any DB changes
            Db.SaveChanges();
        }
    }
}
