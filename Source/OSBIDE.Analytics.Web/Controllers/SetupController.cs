using OSBIDE.Analytics.Library.Models;
using OSBIDE.Library.CSV;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Analytics.Web.Controllers
{
    public class SetupController : ControllerBase
    {
        [HttpGet]
        public ActionResult Index(string message = "")
        {
            ViewBag.message = message;
            return View();
        }

        [HttpPost]
        public ActionResult Index(List<HttpPostedFileBase> files)
        {
            //clear existing states
            Db.Database.ExecuteSqlCommand("TRUNCATE TABLE [TimelineStates]");
            foreach (HttpPostedFileBase file in files)
            {
                Dictionary<int, StudentTimeline> timelines;
                ParseTimeline(file.InputStream, out timelines);
                foreach(var kvp in timelines)
                {
                    Db.TimelineStates.AddRange(kvp.Value.RawStates);
                    Db.SaveChanges();
                }
            }
            return RedirectToAction("Index", new { message = "CSV files parsed successfully." });
        }

        /// <summary>
        /// Pulled from TimeLineAnalysisViewModel inside OSBIDE.Analytics.Terminal project.
        /// Converts a CSV file into an object that can then be stored inside a DB
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="userStates"></param>
        /// <returns></returns>
        private Dictionary<int, StudentTimeline> ParseTimeline(Stream fileStream, out Dictionary<int, StudentTimeline> userStates)
        {
            userStates = new Dictionary<int, StudentTimeline>();

            //get raw data from CSV file
            List<List<string>> rawData = new List<List<string>>();
            CsvReader csv = new CsvReader(fileStream);
            rawData = csv.Parse();

            //convert raw data into object form
            foreach (List<string> pieces in rawData)
            {
                //pull user ID
                int userId = -1;
                Int32.TryParse(pieces[0], out userId);

                if (userStates.ContainsKey(userId) == false)
                {
                    userStates.Add(userId, new StudentTimeline());
                }

                foreach (string entry in pieces)
                {
                    //split data elements
                    string[] parts = entry.Split(new Char[] { ';' });

                    //ignore first record, which is user ID
                    if (parts.Length < 2)
                    {
                        continue;
                    }

                    //build current state
                    TimelineState currentState = new TimelineState();
                    currentState.State = parts[0];
                    currentState.UserId = userId;
                    DateTime tempDate = DateTime.MinValue;
                    DateTime.TryParse(parts[1], out tempDate);
                    currentState.StartTime = tempDate;

                    //two items = social event
                    if (parts.Length == 2)
                    {
                        currentState.IsSocialEvent = true;

                        //social events do not have an end time
                        currentState.EndTime = currentState.StartTime;
                    }
                    else
                    {
                        tempDate = DateTime.MinValue;
                        DateTime.TryParse(parts[2], out tempDate);
                        currentState.EndTime = tempDate;
                    }

                    //add to dictionary
                    userStates[userId].OsbideId = userId;
                    userStates[userId].RawStates.Add(currentState);
                }
            }
            return userStates;
        }

    }
}