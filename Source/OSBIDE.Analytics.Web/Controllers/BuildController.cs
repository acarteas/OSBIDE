using OSBIDE.Analytics.Library.Models;
using OSBIDE.Analytics.Web.Models.ViewModels.CommentAnalyzer;
using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using OSBIDE.Analytics.Web.ViewModels;
using System.Data.Linq.SqlClient;

namespace OSBIDE.Analytics.Web.Controllers
{
    public class BuildController : ControllerBase
    {
        /// <summary>
        /// Finds 50 build events centered around the supplied id parameter
        /// </summary>
        /// <param name="eventLogId">The id of the <see cref="EventLog"/> to use as an anchor</param>
        /// <returns></returns>
        public ActionResult Index(int eventLogId)
        {
            CommentTimeline timeline = Db.CommentTimelines.Where(t => t.EventLogId == eventLogId).FirstOrDefault();
            EventLog commentLog = OsbideDb.EventLogs.Include(u => u.Sender).Where(el => el.Id == timeline.EventLogId).FirstOrDefault();
            BuildDiffViewModel viewModel = new BuildDiffViewModel();
            viewModel.User = commentLog.Sender;
            viewModel.Comment = timeline;
            viewModel.OriginalEvent = commentLog;

            //grab 10 prior builds
            viewModel.BuildsBefore = (
                                            from el in OsbideDb.EventLogs
                                            join be in OsbideDb.BuildEvents
                                            .Include(b => b.Documents.Select(d => d.Document))
                                            .Include(b => b.EventLog)
                                            on el.Id equals be.EventLogId
                                            where el.Id < commentLog.Id
                                            && el.SenderId == commentLog.SenderId
                                            orderby el.Id descending
                                            select be).Take(25).ToList();

            //and 10 builds after
            viewModel.BuildsAfter = (   
                                            from el in OsbideDb.EventLogs
                                            join be in OsbideDb.BuildEvents
                                            .Include(b => b.Documents.Select(d => d.Document))
                                            .Include(b => b.EventLog)
                                            on el.Id equals be.EventLogId
                                            where el.Id > commentLog.Id
                                            && el.SenderId == commentLog.SenderId
                                            orderby el.Id ascending
                                            select be).Take(25).ToList();


            //for each build event, grab NPSM state
            foreach(BuildEvent build in viewModel.BuildsBefore.Union(viewModel.BuildsAfter))
            {
                TimelineState priorBuildState = (from npsm in Db.TimelineStates
                                                 where npsm.StartTime >= build.EventLog.DateReceived
                                                 && npsm.IsSocialEvent == false
                                                 && npsm.UserId == commentLog.SenderId
                                                 && npsm.State != "--"
                                                 orderby npsm.Id ascending
                                                 select npsm).Take(1).FirstOrDefault();
                if(priorBuildState == null)
                {
                    priorBuildState = new TimelineState();
                }
                viewModel.BuildStates.Add(build.Id, priorBuildState);
            }

            return View(viewModel);
        }

        /// <summary>
        /// Shows all documents associated with a given build document
        /// </summary>
        /// <param name="eventLogId"></param>
        /// <returns></returns>
        public ActionResult Documents(int eventLogId)
        {
            BuildEvent build = OsbideDb.BuildEvents
                .Include(be => be.Documents.Select(d => d.Document))
                .Include(be => be.EventLog)
                .Where(be => be.EventLogId == eventLogId)
                .FirstOrDefault();

            //find NPSM state for this build
            TimelineState buildState = (from npsm in Db.TimelineStates
                                             where npsm.StartTime >= build.EventLog.DateReceived
                                             && npsm.IsSocialEvent == false
                                             && npsm.UserId == build.EventLog.SenderId
                                             && npsm.State != "--"
                                             orderby npsm.Id ascending
                                             select npsm).Take(1).FirstOrDefault();

            //find next "interested" build, defined as follows:
            //document contains +/- 10% more lines
            //new document added to build
            List<BuildEvent> futureBuilds = (from be in OsbideDb.BuildEvents
                                             join el in OsbideDb.EventLogs on be.EventLogId equals el.Id
                                             where el.Id > build.EventLogId 
                                             && el.EventTypeId == 2 //2 = build event log type
                                             && el.SenderId == build.EventLog.SenderId
                                             select be)
                                            .Include(be => be.EventLog)
                                            .Include(be => be.Documents.Select(d => d.Document))
                                            .Take(100)
                                            .ToList();
            BuildEvent futureInterstingBuild = FindInterestingBuild(build, futureBuilds);

            //do the same thing, finding the most recent previously interesting build
            List<BuildEvent> pastBuilds = (from be in OsbideDb.BuildEvents
                                                          join el in OsbideDb.EventLogs on be.EventLogId equals el.Id
                                                          where el.Id < build.EventLogId
                                                          && el.EventTypeId == 2 //2 = build event log type
                                                          && el.SenderId == build.EventLog.SenderId
                                                          orderby be.EventLogId descending
                                                          select be
                                                          )
                                            .Include(be => be.EventLog)
                                            .Include(be => be.Documents.Select(d => d.Document))
                                            .Take(100)
                                            .ToList();
            BuildEvent pastInterestingBuild = FindInterestingBuild(build, pastBuilds);

            BuildDocumentsViewModel viewModel = new BuildDocumentsViewModel()
            {
                CurrentBuild = build,
                NextInterestingBuild = futureInterstingBuild,
                FutureBuilds = futureBuilds,
                PreviousInterestingBuild = pastInterestingBuild,
                PastBuilds = pastBuilds,
                BuildState = buildState
            };

            return View(viewModel);
        }

        /// <summary>
        /// Finds the first interesting build in the sequence using the following rules:
        /// 1. A file that exists or does not exist in baseDocument exists
        /// 2. A file that exists in baseDocument differes by more than 10%
        /// </summary>
        /// <param name="documents"></param>
        /// <returns></returns>
        private BuildEvent FindInterestingBuild(BuildEvent baseEvent, List<BuildEvent> events)
        {
            Dictionary<string, CodeDocument> baseDocuments = new Dictionary<string, CodeDocument>();
            foreach (CodeDocument document in baseEvent.Documents.Select(d => d.Document))
            {
                baseDocuments.Add(document.FileName, document);
            }

            //null hypothesis: no interesting events found
            BuildEvent interestingEvent = baseEvent;
            bool interestingEventFound = false;

            foreach (BuildEvent nextBuild in events)
            {
                foreach (CodeDocument document in nextBuild.Documents.Select(d => d.Document))
                {
                    //rule #1: additional file(s)
                    if (baseEvent.Documents.Count != nextBuild.Documents.Count
                        || baseDocuments.ContainsKey(document.FileName) == false)
                    {
                        interestingEventFound = true;
                        interestingEvent = nextBuild;
                    }
                    else
                    {
                        //rule #2: file length differ by more than 10%
                        int originalLength = baseDocuments[document.FileName].Lines.Count;
                        int futureLength = document.Lines.Count;
                        double bound = originalLength * 0.10;

                        //ABS accounts for future length being smaller than original
                        if (Math.Abs(futureLength - originalLength) > bound)
                        {
                            //rule #1 satisfied
                            interestingEventFound = true;
                            interestingEvent = nextBuild;
                        }
                    }
                }

                //short-circut loop if we found something interesting
                if (interestingEventFound == true)
                {
                    break;
                }
            }

            return interestingEvent;
        }
    }
}