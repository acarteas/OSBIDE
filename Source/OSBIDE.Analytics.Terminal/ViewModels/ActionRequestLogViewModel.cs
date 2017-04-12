using OSBIDE.Analytics.Terminal.Models;
using OSBIDE.Library.CSV;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.ViewModels
{
    public class ActionRequestLogViewModel
    {
        private OsbideContext _db { get; set; }
        private FileCache _cache { get; set; }
        public Dictionary<string, Dictionary<string, string>> ControllerActions
        {
            get
            {
                //from all controller names, determine all possible action names
                Dictionary<string, Dictionary<string, string>> controllerActions = new Dictionary<string, Dictionary<string, string>>();

                //first, check to see if we have this information in the cache
                if (_cache.Contains("controllerActions") == false)
                {
                    List<string> controllerNames = (from log in _db.ActionRequestLogs
                                                    select log.ControllerName)
                                                .Distinct()
                                                .ToList();

                    foreach (string controllerName in controllerNames)
                    {
                        //add record in names dictionary
                        controllerActions.Add(controllerName, new Dictionary<string, string>());

                        //find all possible actions
                        List<string> actionNames = (from log in _db.ActionRequestLogs
                                                    where log.ControllerName == controllerName
                                                    select log.ActionName)
                                                    .Distinct()
                                                    .ToList();

                        //record actions for this controller
                        foreach (string action in actionNames)
                        {
                            controllerActions[controllerName].Add(action, action);
                        }
                    }
                    _cache["controllerActions"] = controllerActions;
                    return controllerActions;   
                }
                else
                {
                    return (Dictionary<string, Dictionary<string, string>>)_cache["controllerActions"];
                }
            }
        }
        private Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, int>>>> WeeklyAggregate { get; set; }
        public ActionRequestLogViewModel()
        {
            _db = OsbideContext.DefaultWebConnection;
            _cache = new FileCache("ActionRequestCache");

            //set really long timeout for db queries
            ((IObjectContextAdapter)_db).ObjectContext.CommandTimeout = 60 * 60 * 24;
            WeeklyAggregate = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, int>>>>();
        }
        private List<ActionRequestLog> _rawLogs = new List<ActionRequestLog>();

        public Dictionary<string, List<ActionRequestLog>> GenerateDetailsViewStatistics()
        {
            Dictionary<string, List<ActionRequestLog>> viewCounts = new Dictionary<string,List<ActionRequestLog>>();
            Dictionary<int, EventLog> loadedLogs = new Dictionary<int,EventLog>();

            //min and max log IDs pulled from DB query
            int minLogId = 2308;
            int maxLogId = 41065355;
            int increment = 50000;
            int currentId = minLogId;

            //pull all records from DB.  Note that there's a lot of records and EF is slow so
            //I'm only pulling a few at a time.
            while (currentId + increment < maxLogId + increment)
            {
                List<ActionRequestLog> logs = GetLogs(currentId, currentId + increment);
                
                //go up by one so we don't double count
                currentId += increment + 1;

                foreach (ActionRequestLog arl in logs)
                {
                    //ignore everything but details view
                    if (arl.ControllerName.CompareTo("Feed") != 0)
                    {
                        continue;
                    }
                    if (arl.ActionName.CompareTo("Details") != 0)
                    {
                        continue;
                    }

                    Regex reg = new Regex(@"id=(\d+)", RegexOptions.IgnoreCase);
                    Match match = reg.Match(arl.ActionParameters);
                    if (match.Success)
                    {
                        
                        int key = -1;
                        string keyString = match.Groups[1].ToString();
                        if (Int32.TryParse(keyString, out key) == true)
                        {
                            //log already loaded?
                            EventLog log = null;
                            if (loadedLogs.ContainsKey(key))
                            {
                                log = loadedLogs[key];
                            }
                            else
                            {
                                log = _db.EventLogs.Where(l => l.Id == key).FirstOrDefault();
                                loadedLogs[key] = log;
                            }
                            if (viewCounts.ContainsKey(log.LogType) == false)
                            {
                                viewCounts.Add(log.LogType, new List<ActionRequestLog>());
                            }
                            viewCounts[log.LogType].Add(arl);
                        }
                    }
                }

            }
            return viewCounts;
        }

        public void LoadRawLogsFromCache()
        {
            _rawLogs.Clear();
            string[] keys = _cache.GetKeys();
            foreach(string key in keys)
            {
                string[] pieces = key.Split('_');
                if(pieces.Length == 3)
                {
                    if(pieces[0] == "RawLogs")
                    {
                        //key found, open and add to list of raw logs
                        List<ActionRequestLog> chunk = _cache[key] as List<ActionRequestLog>;
                        if(chunk != null)
                        {
                            _rawLogs.AddRange(chunk);
                        }
                    }
                }
            }
        }

        public void LoadRawLogsFromDb()
        {
            //min and max log IDs pulled from DB query
            int minLogId = 2308;
            int maxLogId = 41065355;
            int increment = 50000;
            int currentId = minLogId;

            //pull all records from DB.  Note that there's a lot of records and EF is slow so
            //I'm only pulling a few at a time.
            while (currentId + increment < maxLogId + increment)
            {
                List<ActionRequestLog> logs = GetLogs(currentId, currentId + increment);
                string key = string.Format("RawLogs_{0}_{1}", currentId, currentId + increment);

                //go up by one so we don't double count
                currentId += increment + 1;

                //write logs to the cache
                _cache[key] = logs;
            }
        }

        public void LoadLogsByDate()
        {
            //min and max log IDs pulled from DB query
            int minLogId = 2308;
            int maxLogId = 41065355;
            int increment = 2000;
            int currentId = minLogId;


            //prime activity by date by creating base dates, controller names, and action names
            //dates that cap the semester
            Dictionary<DateTime, Dictionary<string, Dictionary<string, Dictionary<int, int>>>> activityByDate = new Dictionary<DateTime, Dictionary<string, Dictionary<string, Dictionary<int, int>>>>();
            DateTime startingDate = new DateTime(2014, 1, 1);
            DateTime endingDate = new DateTime(2014, 5, 25);  //last day was 5/4, but go one over for loop
            DateTime currentDate = startingDate;
            

            //beginning at our starting date and going for the entire semester, figure out user activity
            //for each day
            while (currentDate < endingDate)
            {
                activityByDate.Add(currentDate.Date, new Dictionary<string, Dictionary<string, Dictionary<int, int>>>());

                //for each controller name
                foreach (var controllerKvp in ControllerActions)
                {
                    activityByDate[currentDate.Date].Add(controllerKvp.Key, new Dictionary<string, Dictionary<int, int>>());

                    //for each action
                    foreach (var actionKvp in controllerKvp.Value)
                    {
                        activityByDate[currentDate.Date][controllerKvp.Key].Add(actionKvp.Key, new Dictionary<int, int>());
                    }
                }
                currentDate = currentDate.AddDays(1);
            }

            //pull all records from DB.  Note that there's a lot of records and EF is slow so
            //I'm only pulling a few at a time.
            while (currentId + increment < maxLogId + increment)
            {
                List<ActionRequestLog> logs = GetLogs(currentId, currentId + increment);

                foreach (ActionRequestLog log in logs)
                {
                    if(activityByDate[log.AccessDate.Date][log.ControllerName][log.ActionName].ContainsKey(log.CreatorId) == false)
                    {
                        activityByDate[log.AccessDate.Date][log.ControllerName][log.ActionName].Add(log.CreatorId, 0);
                    }
                    activityByDate[log.AccessDate.Date][log.ControllerName][log.ActionName][log.CreatorId] += 1;
                }

                //go up by one so we don't double count
                currentId += increment + 1;
            }

            //save data to cache
            _cache["ActivityByDate"] = activityByDate;
        }

        public List<List<string>> AggregateLogsByWeek()
        {
            Dictionary<DateTime, Dictionary<string, Dictionary<string, Dictionary<int, int>>>> activityByDate = 
                (Dictionary<DateTime, Dictionary<string, Dictionary<string, Dictionary<int, int>>>>)_cache["ActivityByDate"];

            DateTime startingDate = new DateTime(2014, 01, 13);
            DateTime endingDate = new DateTime(2014, 05, 09);
            DateTime currentDate = startingDate;
            DateTime previousDate = currentDate;
            Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, int>>>> weeklyAggregate = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<int, int>>>>();
            int weekNumber = 1;
            
            //keeps track of all users, used for spreadsheet creation below WHILE loop
            Dictionary<int, string> users = new Dictionary<int, string>();
            List<string> weeks = new List<string>();
            weeks.Add("Week 1");

            while(currentDate <= endingDate)
            {
                //did we go up in weeks?
                if (currentDate.AddDays(7 - (int)currentDate.DayOfWeek).Date != previousDate.AddDays(7 - (int)previousDate.DayOfWeek).Date)
                {
                    weekNumber += 1;
                    weeks.Add("Week " + weekNumber);
                }

                //aggregate data key
                string key = string.Format("Week {0}", weekNumber);

                //add new record if needed
                if(weeklyAggregate.ContainsKey(key) == false)
                {
                    weeklyAggregate.Add(key, new Dictionary<string,Dictionary<string,Dictionary<int,int>>>());
                }

                //add in data for the current day to the current week
                foreach(string controllerName in activityByDate[currentDate.Date].Keys)
                {
                    if(weeklyAggregate[key].ContainsKey(controllerName) == false)
                    {
                        weeklyAggregate[key].Add(controllerName, new Dictionary<string, Dictionary<int, int>>());
                    }
                    foreach(string actionName in activityByDate[currentDate.Date][controllerName].Keys)
                    {
                        if(weeklyAggregate[key][controllerName].ContainsKey(actionName) == false)
                        {
                            weeklyAggregate[key][controllerName].Add(actionName, new Dictionary<int, int>());
                        }
                        foreach(int userId in activityByDate[currentDate.Date][controllerName][actionName].Keys)
                        {
                            //remember user ID
                            users[userId] = userId.ToString();

                            if(weeklyAggregate[key][controllerName][actionName].ContainsKey(userId) == false)
                            {
                                weeklyAggregate[key][controllerName][actionName].Add(userId, 0);
                            }
                            weeklyAggregate[key][controllerName][actionName][userId] += activityByDate[currentDate.Date][controllerName][actionName][userId];
                        }
                    }
                }

                //move to the next day
                previousDate = currentDate;
                currentDate = currentDate.AddDays(1);
            }

            //save in VM for future use
            WeeklyAggregate = weeklyAggregate;

            //finally, turn the aggregated data into a 2D matrix suitable for a spreadsheet
            List<List<string>> spreadsheet = new List<List<string>>();
            var controllerActions = ControllerActions;
            
            //first row is headers
            spreadsheet.Add(new List<string>());
            spreadsheet[0].Add("User");

            //add in data columns
            foreach(string week in weeks)
            {
                foreach(string controllerName in controllerActions.Keys)
                {
                    foreach(string actionName in controllerActions[controllerName].Keys)
                    {
                        string header = string.Format("{0} - {1} - {2}", week, controllerName, actionName);
                        spreadsheet[0].Add(header);
                    }
                }
            }

            //first column is user IDs
            int counter = 1;
            foreach(string user in users.Values)
            {
                spreadsheet.Add(new List<string>());
                spreadsheet[counter].Add(user);
                counter++;
            }

            //now, add in data
            counter = 1;
            foreach (int user in users.Keys)
            {
                foreach (string week in weeks)
                {
                    foreach (string controllerName in controllerActions.Keys)
                    {
                        foreach (string actionName in controllerActions[controllerName].Keys)
                        {
                            int numActions = 0;
                            if(weeklyAggregate[week][controllerName][actionName].ContainsKey(user) == true)
                            {
                                numActions = weeklyAggregate[week][controllerName][actionName][user];
                            }
                            spreadsheet[counter].Add(numActions.ToString());
                        }
                    }
                }
                counter++;
            }
            return spreadsheet;
        }

        public void GroupDetailsViewByEventType()
        {
            
        }

        /// <summary>
        /// Attempts to calculate the number of profile visits made.  Differentiates between viewing own
        /// profile and viewing others' profile.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string,int> CountProfileVisits()
        {
            //pull all action requests from the DB in which 
            var query = from actionRequest in _db.ActionRequestLogs
                        join user in _db.Users on actionRequest.CreatorId equals user.Id
                        join cur in _db.CourseUserRelationships on user.Id equals cur.UserId
                        join course in _db.Courses on cur.CourseId equals course.Id
                        where actionRequest.ActionName == "Index"
                        && actionRequest.ControllerName == "Profile"
                        && user.HasInformedConsent == true
                        && cur.CourseId == 3
                        select actionRequest;

            int selfCounter = 0;
            int otherCounter = 0;
            foreach (var log in query)
            {
                //count "self" views as logs that have no ID (null) or their own ID
                if(log.ActionParameters.Contains("null"))
                {
                    selfCounter++;
                }
                else if(log.ActionParameters.Contains(log.CreatorId.ToString()))
                {
                    selfCounter++;
                }
                else
                {
                    otherCounter++;
                }
            }
            Dictionary<string, int> result = new Dictionary<string, int>();
            result["self"] = selfCounter;
            result["other"] = otherCounter;
            return result;
        }

        public List<List<string>> FilterActivity(string controller, string action)
        {
            List<List<string>> spreadsheet = new List<List<string>>();
            var controllerActions = ControllerActions;
            Dictionary<int, string> users = new Dictionary<int, string>();

            //first row is headers
            spreadsheet.Add(new List<string>());
            spreadsheet[0].Add("User");

            //add in data columns
            foreach (string week in WeeklyAggregate.Keys)
            {
                foreach (string controllerName in controllerActions.Keys)
                {
                    if(controllerName.CompareTo(controller) == 0)
                    {
                        foreach (string actionName in controllerActions[controllerName].Keys)
                        {
                            //add in user information
                            foreach(var user in WeeklyAggregate[week][controllerName][actionName])
                            {
                                users[user.Key] = user.Key.ToString();
                            }

                            //only add in categories whose controller and action match
                            if(actionName.CompareTo(action) == 0)
                            {
                                string header = string.Format("{0} - {1} - {2}", week, controllerName, actionName);
                                spreadsheet[0].Add(header);
                            }
                        }
                    }
                }
            }

            //first column is user IDs
            int counter = 1;
            foreach (string user in users.Values)
            {
                spreadsheet.Add(new List<string>());
                spreadsheet[counter].Add(user);
                counter++;
            }

            //now, add in data
            counter = 1;
            foreach (int user in users.Keys)
            {
                foreach (string week in WeeklyAggregate.Keys)
                {
                    foreach (string controllerName in controllerActions.Keys)
                    {
                        if (controllerName.CompareTo(controller) == 0)
                        {
                            foreach (string actionName in controllerActions[controllerName].Keys)
                            {
                                //only add in categories whose controller and action match
                                if (actionName.CompareTo(action) == 0)
                                {
                                    int numActions = 0;
                                    if (WeeklyAggregate[week][controllerName][actionName].ContainsKey(user) == true)
                                    {
                                        numActions = WeeklyAggregate[week][controllerName][actionName][user];
                                    }
                                    spreadsheet[counter].Add(numActions.ToString());
                                }
                            }
                        }
                    }
                }
                counter++;
            }
            return spreadsheet;
        }

        public void WriteToCsv(List<List<string>> matrix, string fileName)
        {
            //write results to file
            CsvWriter writer = new CsvWriter();
            foreach(List<string> line in matrix)
            {
                foreach(string cell in line)
                {
                    writer.AddToCurrentLine(cell);
                }
                writer.CreateNewRow();
            }

            //write to file
            using (TextWriter tw = File.CreateText(fileName))
            {
                tw.Write(writer.ToString());
            }
        }

        private List<ActionRequestLog> GetLogs(int startingId = 0, int endingId = 1000)
        {
            if (endingId <= startingId)
            {
                return new List<ActionRequestLog>();
            }
            using (OsbideContext db = OsbideContext.DefaultWebConnection)
            {
                DateTime startDate = new DateTime(2014, 01, 01);
                DateTime endDate = new DateTime(2014, 05, 15);
                var sql = from log in db.ActionRequestLogs
                          join user in db.Users on log.CreatorId equals user.Id
                          join cur in db.CourseUserRelationships on user.Id equals cur.UserId
                          join course in db.Courses on cur.CourseId equals course.Id
                          where
                          course.Id == 3
                          && log.AccessDate >= startDate
                          && log.AccessDate <= endDate
                          && log.Id >= startingId
                          && log.Id <= endingId
                          && user.HasInformedConsent == true
                          select log;
                return sql.ToList();
            }
        }

    }
}
