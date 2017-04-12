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
    public class CommentAnalyzerController : ControllerBase
    {
        /*Pages:
         * Index: Show list of students by class.  Include Post count & document save count.
         * Student: Shows all comments by student.  Also includes time of previous and next edit
         * Details: Shows comment, code before, and code after
         * */

        // GET: CommentAnalyzer
        public ActionResult Index(int courseId = 3)
        {
            //grab all students in the course
            var studentsQuery = from student in OsbideDb.Users
                                join cur in OsbideDb.CourseUserRelationships on student.Id equals cur.UserId
                                join course in OsbideDb.Courses on cur.CourseId equals course.Id
                                where course.Id == courseId
                                select new { Student = student, Course = course };
            var students = studentsQuery.ToList();

            //grab num posts by student
            var numPosts = from item in
                               (from log in OsbideDb.EventLogs
                                where log.LogType == "FeedPostEvent"
                                select log
                                )
                           group item by item.SenderId into items
                           select new { StudentId = items.Key, Count = items.Count() };
            Dictionary<int, int> postsByStudent = new Dictionary<int, int>();
            foreach (var item in numPosts)
            {
                postsByStudent.Add(item.StudentId, item.Count);
            }

            //grab num replies by student
            var numReplies = from item in
                                 (from log in OsbideDb.EventLogs
                                  where log.LogType == "LogCommentEvent"
                                  select log
                                  )
                             group item by item.SenderId into items
                             select new { StudentId = items.Key, Count = items.Count() };
            Dictionary<int, int> repliesByStudent = new Dictionary<int, int>();
            foreach (var item in numReplies)
            {
                repliesByStudent.Add(item.StudentId, item.Count);
            }

            //grab number of saves by student
            var numSaves = from item in
                               (from log in OsbideDb.EventLogs
                                where log.LogType == "SaveEvent"
                                select log
                                )
                           group item by item.SenderId into items
                           select new { StudentId = items.Key, Count = items.Count() };
            Dictionary<int, int> savesByStudent = new Dictionary<int, int>();
            foreach (var item in numSaves)
            {
                savesByStudent.Add(item.StudentId, item.Count);
            }

            IndexViewModel vm = new IndexViewModel();
            vm.Course = students.FirstOrDefault().Course;
            vm.Users = students.Select(s => s.Student).OrderBy(s => s.LastName).ToList();
            vm.PostsByUser = postsByStudent;
            vm.RepliesByUser = repliesByStudent;
            vm.SavesByUser = savesByStudent;
            return View(vm);
        }

        /// <summary>
        /// Gets comment details for a particular student
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult Student(int id)
        {
            /*
             * to fetch:
             * For each comment:
             *      Comment
             *      Option to view entire thread
             *      Chris, Adam, Carla content coding info
             *      Adam's student EC coding info
             *      size of previous / next save
             *      time of prevous /  last save
             *      NPSM state
             * */
            List<CommentTimelineViewModel> viewModel = new List<CommentTimelineViewModel>();

            //id is nice for MVC, but not very descriptive, switch to studentId in body.
            int studentId = id;
            int[] interestingEventIds =
            {
                  1  //ask for help 
                , 2  //build event
                , 7  //feed post
                , 9  //log comment
                , 10 //save
            };

            //only social events
            int[] socialEventIds = {1, 7, 9};

            //check to see if we have cached results in the DB
            var cachedResults = Db
                .CommentTimelines
                .Include(c => c.ProgrammingState)
                .Include(c => c.QuestionCodings.Select(q => q.QuestionCoding.Post))
                .Include(c => c.AnswerCodings.Select(q => q.AnswerCoding.Answer))
                .Include(c => c.ExpertCoding)
                .Include(c => c.TimelineCodeDocuments)
                .Where(c => c.AuthorId == studentId)
                ;
            if (cachedResults.Count() > 0)
            {
                var authorQuery = from user in OsbideDb.Users
                                  where user.Id == studentId
                                  select user;
                OsbideUser student = authorQuery.FirstOrDefault();
                student = (student == null) ? new OsbideUser() : student;

                var eventLogQuery = from log in OsbideDb.EventLogs
                                    where log.SenderId == studentId
                                    && socialEventIds.Contains(log.EventTypeId)
                                    select log;
                Dictionary<int, EventLog> allEventLogs = new Dictionary<int, EventLog>();
                foreach(EventLog log in eventLogQuery)
                {
                    allEventLogs.Add(log.Id, log);
                }

                foreach(CommentTimeline timeline in cachedResults)
                {
                    CommentTimelineViewModel nextViewModel = new CommentTimelineViewModel(timeline);
                    nextViewModel.Log = allEventLogs[timeline.EventLogId];
                    nextViewModel.Author = student;
                    List<TimelineCodeDocument> beforeDocuments = timeline.TimelineCodeDocuments.Where(t => t.isBeforeComment == true).ToList();
                    List<TimelineCodeDocument> afterDocuments = timeline.TimelineCodeDocuments.Where(t => t.isBeforeComment == false).ToList();
                    nextViewModel.CodeBeforeComment = new Dictionary<string, TimelineCodeDocument>();
                    nextViewModel.CodeAfterComment = new Dictionary<string, TimelineCodeDocument>();
                    foreach(TimelineCodeDocument tcd in beforeDocuments)
                    {
                        nextViewModel.CodeBeforeComment.Add(tcd.DocumentName, tcd);
                    }
                    foreach(TimelineCodeDocument tcd in afterDocuments)
                    {
                        nextViewModel.CodeAfterComment.Add(tcd.DocumentName, tcd);
                    }
                    viewModel.Add(nextViewModel);
                }
            }
            else
            {
                //no cached results, do it the long way...

                //this query pulls most questions (ask for help excluded?) from the analytics DB
                //and should be faster than loading all questions from the OSBIDE DB
                var commentQuery = from comment in Db.Posts
                                   where comment.AuthorId == studentId
                                   select comment;

                //convert into dictionary for lookup by logsQuery
                Dictionary<int, Post> posts = new Dictionary<int, Post>();
                foreach (Post post in commentQuery)
                {
                    posts.Add(post.OsbideId, post);
                }

                //This query will pull down all content coding questions, organized by Osbide user ID
                var contentCodingQuery = from code in Db.ContentCodings
                                         where code.AuthorId == studentId
                                         select code;
                SortedList<DateTime, ContentCoding> expertCodings = new SortedList<DateTime, ContentCoding>();
                foreach (ContentCoding coding in contentCodingQuery)
                {
                    //I was getting key mismatch (probably difference in milliseconds).  My solution was to create
                    //a new date using only coarser measures
                    DateTime dateKey = new DateTime(coding.Date.Year, coding.Date.Month, coding.Date.Day, coding.Date.Hour, coding.Date.Minute, coding.Date.Second, DateTimeKind.Utc);
                    expertCodings.Add(dateKey, coding);
                }

                //This query will pull down information obtained from my crowd-sourced content coding.
                //AnswerCodings have FK reference to the original question as well as the answer.  If a Post
                //is in the AnswerCodings table, it must be an answer
                var answeredQuestionsQuery = from answer in Db.AnswerCodings
                                           .Include(c => c.Answer)
                                           .Include(c => c.Question)
                                             where answer.Answer.AuthorId == studentId || answer.Question.AuthorId == studentId
                                             select answer;

                var allPostsQuery = from question in Db.QuestionCodings
                                        .Include(c => c.Post)
                                    where question.Post.AuthorId == studentId
                                    select question;
                Dictionary<int, PostCoding> crowdCodings = new Dictionary<int, PostCoding>();
                foreach (QuestionCoding coding in allPostsQuery)
                {
                    if (crowdCodings.ContainsKey(coding.Post.OsbideId) == false)
                    {
                        crowdCodings.Add(coding.Post.OsbideId, new PostCoding());
                        crowdCodings[coding.Post.OsbideId].OsbidePostId = coding.Post.OsbideId;
                    }
                    crowdCodings[coding.Post.OsbideId].Codings.Add(coding);
                }
                foreach (AnswerCoding coding in answeredQuestionsQuery)
                {
                    if (crowdCodings.ContainsKey(coding.Question.OsbideId) == false)
                    {
                        crowdCodings.Add(coding.Question.OsbideId, new PostCoding());
                        crowdCodings[coding.Question.OsbideId].OsbidePostId = coding.Question.OsbideId;
                    }
                    crowdCodings[coding.Question.OsbideId].Responses.Add(coding);
                }

                //grab all save and build events for this user
                Dictionary<int, SaveEvent> allSaves = new Dictionary<int, SaveEvent>();
                Dictionary<int, BuildEvent> allBuilds = new Dictionary<int, BuildEvent>();
                var savesQuery = from save in OsbideDb.SaveEvents
                                 .Include(s => s.Document)
                                 join log in OsbideDb.EventLogs on save.EventLogId equals log.Id
                                 where log.SenderId == studentId
                                 select save;
                foreach (SaveEvent saveEvent in savesQuery)
                {
                    allSaves[saveEvent.EventLogId] = saveEvent;
                }
                var buildsQuery = from build in OsbideDb.BuildEvents
                                 .Include(b => b.Documents.Select(d => d.Document))
                                  join log in OsbideDb.EventLogs on build.EventLogId equals log.Id
                                  where log.SenderId == studentId
                                  select build;
                foreach (BuildEvent buildEvent in buildsQuery)
                {
                    allBuilds[buildEvent.EventLogId] = buildEvent;
                }

                //this query pulls data directly from event logs.
                var logsQuery = from log in OsbideDb.EventLogs
                                where log.SenderId == studentId && interestingEventIds.Contains(log.EventTypeId)
                                select log;
                List<EventLog> eventLogs = logsQuery.ToList();

                Stack<EventLog> saveEvents = new Stack<EventLog>();
                List<EventLog> socialEvents = new List<EventLog>();

                foreach (EventLog log in eventLogs)
                {
                    //holds the next entry into the view model
                    CommentTimelineViewModel nextViewModel = new CommentTimelineViewModel();

                    //if we have a document save event, remember for later until we get a social event
                    if (log.LogType == SaveEvent.Name || log.LogType == BuildEvent.Name)
                    {
                        saveEvents.Push(log);
                    }
                    else
                    {
                        //social event detected

                        //1: grab previous edit information
                        string solutionName = "";
                        Dictionary<string, CodeDocument> previousDocuments = new Dictionary<string, CodeDocument>();

                        //Start with saves as they will contain more up-to-date information than last build
                        while (saveEvents.Count > 0 && saveEvents.Peek().LogType != BuildEvent.Name)
                        {
                            EventLog next = saveEvents.Pop();
                            if (allSaves.ContainsKey(next.Id))
                            {
                                SaveEvent save = allSaves[next.Id];
                                if (solutionName.Length == 0)
                                {
                                    solutionName = save.SolutionName;
                                }
                                if (save.SolutionName == solutionName)
                                {
                                    if (previousDocuments.ContainsKey(save.Document.FileName) == false)
                                    {
                                        previousDocuments[save.Document.FileName] = save.Document;
                                    }
                                }
                            }
                        }

                        //at this point, saveEvents should be empty or we should be at a build event.
                        //Finish off the snapshot with documents transferred with last build
                        if (saveEvents.Count > 0)
                        {
                            EventLog top = saveEvents.Pop();
                            if (allBuilds.ContainsKey(top.Id))
                            {
                                BuildEvent build = allBuilds[top.Id];

                                if (solutionName.Length == 0)
                                {
                                    solutionName = build.SolutionName;
                                }
                                if (build.SolutionName == solutionName)
                                {
                                    foreach (BuildDocument doc in build.Documents)
                                    {
                                        if (previousDocuments.ContainsKey(doc.Document.FileName) == false)
                                        {
                                            previousDocuments[doc.Document.FileName] = doc.Document;
                                        }
                                    }
                                }
                            }
                        }


                        //store final result in view model
                        foreach(CodeDocument document in previousDocuments.Values)
                        {
                            TimelineCodeDocument tcd = new TimelineCodeDocument()
                            {
                                CodeDocumentId = document.Id,
                                CommentTimeline = nextViewModel.Timeline,
                                DocumentContent = document.Content,
                                DocumentName = document.FileName,
                                isBeforeComment = true
                            };
                            if(nextViewModel.Timeline.TimelineCodeDocuments == null)
                            {
                                nextViewModel.Timeline.TimelineCodeDocuments = new List<TimelineCodeDocument>();
                            }
                            nextViewModel.Timeline.TimelineCodeDocuments.Add(tcd);
                        }

                        //2: grab next edit information (will have to be done on 2nd pass)

                        //grab expert content coding info
                        DateTime dateKey = new DateTime(log.DateReceived.Year,
                                                        log.DateReceived.Month,
                                                        log.DateReceived.Day,
                                                        log.DateReceived.Hour,
                                                        log.DateReceived.Minute,
                                                        log.DateReceived.Second,
                                                        DateTimeKind.Utc);
                        //I was getting key mismatch (probably difference in milliseconds).  My solution was to create
                        //a new date using only coarser measures
                        if (expertCodings.ContainsKey(dateKey))
                        {
                            nextViewModel.Timeline.ExpertCoding = expertCodings[dateKey];
                            nextViewModel.Timeline.ExpertCodingId = expertCodings[dateKey].Id;
                        }

                        //grab crowd coding info
                        var crowd = crowdCodings.Where(cc => cc.Key == log.Id).Select(k => k.Value).FirstOrDefault();
                        if (crowd != null)
                        {
                            foreach (var question in crowd.Codings)
                            {
                                TimelineQuestionCoding questionCode = new TimelineQuestionCoding()
                                {
                                    CommentTimeline = nextViewModel.Timeline,
                                    QuestionCoding = question,
                                    QuestionCodingId = question.Id
                                };
                                nextViewModel.Timeline.QuestionCodings.Add(questionCode);
                            }
                            foreach (var answer in crowd.Responses)
                            {
                                TimelineAnswerCoding responseCoding = new TimelineAnswerCoding()
                                {
                                    CommentTimeline = nextViewModel.Timeline,
                                    AnswerCoding = answer,
                                    AnswerCodingId = answer.Id
                                };
                                nextViewModel.Timeline.AnswerCodings.Add(responseCoding);
                            }
                        }

                        //grab NPSM state info
                        //we want the most recent NPSM state that occurred before the comment was made
                        var npsmQuery = from npsm in Db.TimelineStates
                                        where npsm.StartTime <= log.DateReceived && npsm.IsSocialEvent == false
                                        && npsm.UserId == log.SenderId
                                        && npsm.State != "--"
                                        orderby npsm.Id ascending
                                        select npsm;
                        TimelineState state = npsmQuery.Take(1).FirstOrDefault();
                        if (state != null)
                        {
                            nextViewModel.Timeline.ProgrammingState = state;
                            nextViewModel.Timeline.ProgrammingStateId = state.Id;
                        }


                        //add in comment information
                        if (posts.ContainsKey(log.Id) == true)
                        {
                            nextViewModel.Timeline.Comment = posts[log.Id].Content;
                        }
                        else
                        {
                            //not found in pre-query.  Pull manually
                            if (log.LogType == FeedPostEvent.Name)
                            {
                                FeedPostEvent feedPost = OsbideDb.FeedPostEvents.Where(fpe => fpe.EventLogId == log.Id).FirstOrDefault();
                                if (feedPost != null)
                                {
                                    nextViewModel.Timeline.Comment = feedPost.Comment;
                                }
                            }
                            else if (log.LogType == LogCommentEvent.Name)
                            {
                                LogCommentEvent logComment = OsbideDb.LogCommentEvents.Where(fpe => fpe.EventLogId == log.Id).FirstOrDefault();
                                if (logComment != null)
                                {
                                    nextViewModel.Timeline.Comment = logComment.Content;
                                }
                            }
                            else if (log.LogType == AskForHelpEvent.Name)
                            {
                                AskForHelpEvent ask = OsbideDb.AskForHelpEvents.Where(fpe => fpe.EventLogId == log.Id).FirstOrDefault();
                                if (ask != null)
                                {
                                    nextViewModel.Timeline.Comment = ask.UserComment + "\n" + ask.Code;
                                }
                            }
                        }
                        nextViewModel.Log = log;
                        nextViewModel.Timeline.EventLogId = log.Id;
                        nextViewModel.Timeline.AuthorId = log.SenderId;
                        nextViewModel.Author = log.Sender;
                        if(nextViewModel.Timeline.ExpertCoding == null)
                        {
                            nextViewModel.Timeline.ExpertCoding = new ContentCoding();
                        }
                        if(nextViewModel.Timeline.ProgrammingState == null)
                        {
                            nextViewModel.Timeline.ProgrammingState = new TimelineState()
                            {
                                EndTime = new DateTime(2016, 01, 01),
                                StartTime = new DateTime(2016, 01, 01),
                                State = "not available"
                            };
                        }
                        viewModel.Add(nextViewModel);
                    }
                }

                //2nd pass: find code modifications made after comment.  
                for (int i = 0; i < viewModel.Count; i++)
                {
                    CommentTimelineViewModel current = viewModel[i] as CommentTimelineViewModel;
                    CommentTimelineViewModel next = new CommentTimelineViewModel();
                    if (i + 1 < viewModel.Count)
                    {
                        next = viewModel[i + 1] as CommentTimelineViewModel;
                    }
                    else
                    {
                        next.Log = eventLogs.Last();
                    }

                    List<EventLog> logsBetween = eventLogs
                        .Where(l => l.DateReceived >= current.Log.DateReceived)
                        .Where(l => l.DateReceived <= next.Log.DateReceived)
                        .ToList();

                    Dictionary<string, CodeDocument> nextDocuments = new Dictionary<string, CodeDocument>();
                    string solutionName = "";
                    foreach (EventLog log in logsBetween)
                    {
                        if (log.LogType == SaveEvent.Name)
                        {
                            if (allSaves.ContainsKey(log.Id))
                            {
                                SaveEvent save = allSaves[log.Id];
                                if (solutionName.Length == 0)
                                {
                                    solutionName = save.SolutionName;
                                }
                                if (save.SolutionName == solutionName)
                                {
                                    if (nextDocuments.ContainsKey(save.Document.FileName) == false)
                                    {
                                        nextDocuments[save.Document.FileName] = save.Document;
                                    }
                                }
                            }
                        }
                        else if (log.LogType == BuildEvent.Name)
                        {
                            if (allBuilds.ContainsKey(log.Id))
                            {
                                BuildEvent build = allBuilds[log.Id];
                                if (solutionName.Length == 0)
                                {
                                    solutionName = build.SolutionName;
                                }
                                if (build.SolutionName == solutionName)
                                {
                                    foreach (BuildDocument doc in build.Documents)
                                    {
                                        if (nextDocuments.ContainsKey(doc.Document.FileName) == false)
                                        {
                                            nextDocuments[doc.Document.FileName] = doc.Document;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    //store after documents
                    foreach (CodeDocument document in nextDocuments.Values)
                    {
                        TimelineCodeDocument tcd = new TimelineCodeDocument()
                        {
                            CodeDocumentId = document.Id,
                            CommentTimeline = current.Timeline,
                            DocumentContent = document.Content,
                            DocumentName = document.FileName,
                            isBeforeComment = false
                        };
                        if(current.Timeline.TimelineCodeDocuments == null)
                        {
                            current.Timeline.TimelineCodeDocuments = new List<TimelineCodeDocument>();
                        }
                        current.Timeline.TimelineCodeDocuments.Add(tcd);
                    }
                }

                //try jamming all this crap into DB
                Db.CommentTimelines.AddRange(viewModel.Select(m => m.Timeline).ToList());
                Db.SaveChanges();
            }

            return View(viewModel);
        }

        public ActionResult QuestionsWithResponses()
        {
            //pull everything that has been coded as a question by an expert
            var questionsQuery = (from comment in Db.CommentTimelines
                                 .Include(c => c.ExpertCoding)
                                 .Include(c => c.AnswerCodings)
                                 .Include(c => c.ProgrammingState)
                                 .Include(c => c.QuestionCodings)
                                 .Include(c => c.TimelineCodeDocuments)
                                 where
                                  (
                                    comment.ExpertCoding.PrimaryModifier == "Question"
                                    || comment.ExpertCoding.SecondaryModifier == "Question"
                                    || comment.ExpertCoding.TertiaryModifier == "Question"
                                    )
                                    &&
                                    (
                                    comment.ExpertCoding.Category == "Code"
                                    )
                                  
                                    orderby comment.ExpertCoding.Date
                                 select comment).ToList();

            List<CommentTimelineViewModel> viewModel = new List<CommentTimelineViewModel>();
            int[] userIds = questionsQuery.Select(q => q.AuthorId).Distinct().ToArray();
            int[] eventIds = questionsQuery.Select(q => q.EventLogId).Distinct().ToArray();
            var users = (from user in OsbideDb.Users
                         where userIds.Contains(user.Id)
                                      select user).ToDictionary(u => u.Id, u => u);
            var logs = (from log in OsbideDb.EventLogs
                        where eventIds.Contains(log.Id)
                        select log).ToDictionary(l => l.Id, l => l);
            foreach(CommentTimeline timeline in questionsQuery)
            {
                CommentTimelineViewModel current = new CommentTimelineViewModel(timeline);
                current.Author = users[timeline.AuthorId];
                current.Log = logs[timeline.EventLogId];
                current.CodeBeforeComment = timeline.TimelineCodeDocuments.Where(d => d.isBeforeComment == true).ToDictionary(d => d.DocumentName, d => d);
                current.CodeAfterComment = timeline.TimelineCodeDocuments.Where(d => d.isBeforeComment == false).ToDictionary(d => d.DocumentName, d => d);
                viewModel.Add(current);
            }

            return View(viewModel);
        }

        /// <summary>
        /// Generates a list of document saves centered around a particular point of interest
        /// </summary>
        /// <param name="id">The id of the <see cref="CommentTimeline"/> to use as an anchor</param>
        /// <returns></returns>
        public ActionResult DocumentSaveTimeline(int id)
        {
            CommentTimeline timeline = Db.CommentTimelines.Where(t => t.Id == id).FirstOrDefault();
            EventLog commentLog = OsbideDb.EventLogs.Include(u => u.Sender).Where(el => el.Id == timeline.EventLogId).FirstOrDefault();

            //get all posts in the conversation
            Post userPost = Db.Posts.Where(p => p.OsbideId == commentLog.Id).FirstOrDefault();
            List<Post> entireDiscussion = new List<Post>(); 
            if(userPost.ParentId > 0)
            {
                entireDiscussion = (from post in Db.Posts
                                    where (post.ParentId == userPost.ParentId || post.Id == userPost.ParentId)
                                    orderby post.OsbideId
                                    select post).ToList();
            }
            else
            {
                entireDiscussion = (from post in Db.Posts
                                    where (post.ParentId == userPost.Id || post.Id == userPost.Id)
                                    orderby post.OsbideId
                                    select post).ToList();
            }

            //for each post in the discussion, grab a snapshot of the user's code
            Dictionary<int, BuildEvent> beforeBuildEvents = new Dictionary<int, BuildEvent>();
            Dictionary<int, BuildEvent> afterBuildEvents = new Dictionary<int, BuildEvent>();
            Dictionary<int, TimelineState> statesBefore = new Dictionary<int, TimelineState>();
            Dictionary<int, TimelineState> statesAfter = new Dictionary<int, TimelineState>();
            foreach(Post post in entireDiscussion)
            {
                //get prior build event
                BuildEvent priorBuildEvent = (from el in OsbideDb.EventLogs
                                              join be in OsbideDb.BuildEvents 
                                              .Include(b => b.Documents.Select(d => d.Document))
                                              .Include(b => b.EventLog)
                                              on el.Id equals be.EventLogId
                                              where el.Id < post.OsbideId
                                              && el.SenderId == commentLog.SenderId
                                              orderby el.Id descending
                                              select be).Take(1).FirstOrDefault();
                //grab next build event
                BuildEvent nextBuildEvent = (from el in OsbideDb.EventLogs
                                             join be in OsbideDb.BuildEvents
                                             .Include(b => b.Documents.Select(d => d.Document))
                                             .Include(b => b.EventLog)
                                             on el.Id equals be.EventLogId
                                             where el.Id > post.OsbideId
                                             && el.SenderId == commentLog.SenderId
                                             orderby el.Id ascending
                                             select be).Take(1).FirstOrDefault();

                if (priorBuildEvent != null)
                {
                    //we want the NPSM state that resulted from this build
                    TimelineState priorBuildState = (from npsm in Db.TimelineStates
                                                     where npsm.StartTime >= priorBuildEvent.EventLog.DateReceived
                                                     && npsm.IsSocialEvent == false
                                                     && npsm.UserId == commentLog.SenderId
                                                     && npsm.State != "--"
                                                     orderby npsm.Id ascending
                                                     select npsm).Take(1).FirstOrDefault();
                    statesBefore.Add(post.Id, priorBuildState);
                    beforeBuildEvents.Add(post.Id, priorBuildEvent);
                }
                if(nextBuildEvent != null)
                {
                    //we want the NPSM state that resulted from this build
                    TimelineState afterBuildState = (from npsm in Db.TimelineStates
                                                     where npsm.StartTime >= nextBuildEvent.EventLog.DateReceived
                                                     && npsm.IsSocialEvent == false
                                                     && npsm.UserId == commentLog.SenderId
                                                     && npsm.State != "--"
                                                     orderby npsm.Id ascending
                                                     select npsm).Take(1).FirstOrDefault();
                    statesAfter.Add(post.Id, afterBuildState);
                    afterBuildEvents.Add(post.Id, nextBuildEvent);
                }
            }

            //construct final VM
            DocumentSaveTimelineViewModel viewModel = new DocumentSaveTimelineViewModel()
            {
                BuildsAfter = afterBuildEvents,
                BuildsBefore = beforeBuildEvents,
                Discussion = entireDiscussion,
                StatesAfter = statesAfter,
                StatesBefore = statesBefore,
                Timeline = timeline,
                TimelineLog = commentLog,
                User = commentLog.Sender
            };
            
            return View(viewModel);
        }
    }
}