using Ionic.Zip;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using OSBIDE.Web.Models.Attributes;
using OSBIDE.Web.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Web.Controllers
{
    [OsbideAuthorize]
    [DenyAccess(SystemRole.Student)]
    public class AssignmentController : ControllerBase
    {
        //
        // GET: /Assignment/
        [NotForStudents]
        public ActionResult Index(int assignmentId = -1)
        {
            Assignment currentAssignment = Db.Assignments.Where(a => a.Id == assignmentId).FirstOrDefault();
            if (currentAssignment == null)
            {
                return RedirectToAction("Index", "Feed");
            }
            List<Assignment> allAssignments = Db.Assignments.Where(a => a.CourseId == currentAssignment.CourseId).ToList();

            //build the view model and return
            AssignmentsViewModel vm = new AssignmentsViewModel();
            vm.Assignments = allAssignments;
            vm.CurrentAssignment = currentAssignment;
            vm.Submissions = GetMostRecentSubmissions(assignmentId);

            return View(vm);
        }

        private FileStreamResult PackageFiles(List<SubmitEvent> submits, bool useStudentId = false)
        {
            //AC: for some reason, I can't use a USING statement for automatic closure.  Is this 
            //    a potential memory leak?
            MemoryStream finalZipStream = new MemoryStream();
            using (ZipFile finalZipFile = new ZipFile())
            {
                foreach (SubmitEvent submit in submits)
                {
                    using (MemoryStream zipStream = new MemoryStream())
                    {
                        zipStream.Write(submit.SolutionData, 0, submit.SolutionData.Length);
                        zipStream.Position = 0;
                        try
                        {
                            using (ZipFile zip = ZipFile.Read(zipStream))
                            {
                                foreach (ZipEntry entry in zip)
                                {
                                    using (MemoryStream entryStream = new MemoryStream())
                                    {
                                        string entryName = string.Format("{0}/{1}", submit.EventLog.Sender.FullName, entry.FileName);
                                        if(useStudentId)
                                        {
                                            entryName = string.Format("{0}/{1}", submit.EventLog.Sender.InstitutionId + "_" + submit.EventLog.Sender.FullName, entry.FileName);
                                        }
                                        entry.Extract(entryStream);
                                        entryStream.Position = 0;
                                        finalZipFile.AddEntry(entryName, entryStream.ToArray());
                                    }
                                }
                            }
                        }
                        catch (ZipException)
                        {
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
                finalZipFile.Save(finalZipStream);
                finalZipStream.Position = 0;
            }

            string assignmentName = "Unknown.zip";
            if (submits.Count > 0)
            {
                assignmentName = submits.FirstOrDefault().Assignment.Name + ".zip";
            }
            return new FileStreamResult(finalZipStream, "application/zip") { FileDownloadName = assignmentName };
        }

        [NotForStudents]
        public FileStreamResult Download(int id)
        {
            List<SubmitEvent> submits = GetMostRecentSubmissions(id);
            return PackageFiles(submits);
        }

        [NotForStudents]
        public FileStreamResult DownloadWithIds(int id)
        {
            List<SubmitEvent> submits = GetMostRecentSubmissions(id);
            return PackageFiles(submits, true);
        }

        public FileStreamResult DownloadStudentAssignment(int id)
        {
            //students can only download their own work
            SubmitEvent submit = Db.SubmitEvents.Where(s => s.EventLogId == id).FirstOrDefault();
            if (submit != null && submit.EventLog.SenderId == CurrentUser.Id)
            {
                return DownloadSingle(id);
            }
            else
            {
                return new FileStreamResult(new MemoryStream(), "application/zip") { FileDownloadName = "bad file" };
            }
        }

        [NotForStudents]
        public FileStreamResult DownloadSingle(int id)
        {
            MemoryStream stream = new MemoryStream();
            SubmitEvent submit = Db.SubmitEvents.Where(s => s.EventLogId == id).FirstOrDefault();
            string fileName = "bad download";
            if (submit != null)
            {
                stream.Write(submit.SolutionData, 0, submit.SolutionData.Length);
                stream.Position = 0;
                fileName = string.Format("{0} - {1}.zip", submit.Assignment.Name, submit.EventLog.Sender.FullName);
            }
            return new FileStreamResult(stream, "application/zip") { FileDownloadName = fileName };
        }

        private List<SubmitEvent> GetMostRecentSubmissions(int assignmentId)
        {
            //the DB query will get all student submits.  We only want their last submission...
            List<SubmitEvent> submits = Db.SubmitEvents
                .Include("EventLog")
                .Include("EventLog.Sender")
                .Where(s => s.AssignmentId == assignmentId)
                .OrderBy(s => s.EventDate)
                .ToList();

            //...which we will store into this dictionary
            Dictionary<int, SubmitEvent> lastSubmits = new Dictionary<int, SubmitEvent>();
            foreach (SubmitEvent submit in submits)
            {
                int key = submit.EventLog.SenderId;
                if (lastSubmits.ContainsKey(key) == false)
                {
                    lastSubmits[key] = submit;
                }
                else
                {
                    if (lastSubmits[key].EventDate < submit.EventDate)
                    {
                        lastSubmits[key] = submit;
                    }
                }
            }

            //convert the dictionary back into a list
            submits.Clear();
            foreach (int key in lastSubmits.Keys)
            {
                submits.Add(lastSubmits[key]);
            }

            //order by last name
            submits = submits.OrderBy(s => s.EventLog.Sender.FullName).ToList();
            return submits;
        }

        //
        // Get latest submission time (if any)
        public DateTime LatestSubmissionTime(int assignmentId, int currentUserId)
        {
            //get the newest submit event for this assignment and user
            SubmitEvent submitEvent = Db.SubmitEvents
                                        .Where(se => se.AssignmentId == assignmentId)
                                        .Where(s => s.EventLog.SenderId == currentUserId)
                                        .OrderByDescending(se => se.EventLog.DateReceived)
                                        .FirstOrDefault();

            if (submitEvent != null)
            {
                return submitEvent.EventLog.DateReceived;                
            }
            else
            {
                //return default date value
                return new DateTime();
            }
        }

    }
}
