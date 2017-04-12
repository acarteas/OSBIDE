using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using OSBIDE.Library.Models;
using System.IO;
using Ionic.Zip;
using EnvDTE80;
using EnvDTE90a;

namespace OSBIDE.Library.Events
{
    public class OsbideEventHandler : EventHandlerBase
    {
        /// <summary>
        /// These events constantly fire and are of no use to us.
        /// </summary>
        private List<string> boringCommands =
            (new string[] 
                {
                    "Build.SolutionConfigurations",
                    "Edit.GoToFindCombo",
                    ""
                }
            ).ToList();
        private DateTime LastEditorActivityEvent = DateTime.MinValue;
        public enum BreakpointIDs
        {
            ToggleBreakpoint = 255,
            BreakAtFunction = 311,
            EditorClick = 769
        };

        public OsbideEventHandler(IServiceProvider serviceProvider, IOsbideEventGenerator osbideEvents)
            : base(serviceProvider, osbideEvents)
        {

        }

        private Command GetCommand(string guid, int id)
        {
            Command cmd = null;
            try
            {
                cmd = dte.Commands.Item(guid, id);
            }
            catch (Exception)
            {
                //do nothing
            }
            return cmd;
        }

        #region EventHandlerBase Overrides

        public override void SubmitEventRequested(object sender, SubmitEventArgs e)
        {
            base.SubmitEventRequested(sender, e);
            IOsbideEvent evt = e.Event;
            evt.EventDate = DateTime.UtcNow;
            evt.SolutionName = dte.Solution.FullName;

            //send off to the service client
            NotifyEventCreated(this, new EventCreatedArgs(evt));
        }

        public override void OsbideSolutionSubmitted(object sender, SubmitAssignmentArgs e)
        {
            base.OsbideSolutionSubmitted(sender, e);

            SubmitEvent submit = new SubmitEvent(dte);
            submit.AssignmentId = e.AssignmentId;
            submit.CreateSolutionBinary();

            //let others know that we have a new event
            NotifyEventCreated(this, new EventCreatedArgs(submit));
        }

        public override void OsbideSolutionDownloaded(object sender, SolutionDownloadedEventArgs e)
        {
            base.OsbideSolutionDownloaded(sender, e);
            SolutionDownloadEvent download = new SolutionDownloadEvent()
            {
                AssignmentId = e.DownloadedSubmission.AssignmentId,
                AuthorId = e.DownloadedSubmission.EventLog.SenderId,
                DownloadingUserId = e.DownloadingUser.Id,
                SolutionName = e.DownloadedSubmission.SolutionName
            };

            //let others know that we have a new event
            NotifyEventCreated(this, new EventCreatedArgs(download));
        }

        public override void DocumentSaved(Document document)
        {
            base.DocumentSaved(document);
            SaveEvent save = new SaveEvent();
            save.EventDate = DateTime.UtcNow;
            save.SolutionName = dte.Solution.FullName;
            save.Document = (CodeDocument)DocumentFactory.FromDteDocument(document);

            //let others know that we have a new event
            NotifyEventCreated(this, new EventCreatedArgs(save));
        }

        public override void OnBuildDone(vsBuildScope Scope, vsBuildAction Action)
        {
            
            base.OnBuildDone(Scope, Action);

            //this might take a while, so throw it in its own thread
            //System.Threading.Tasks.Task.Factory.StartNew(
            //    () =>
            //    {
                    BuildEvent build = new BuildEvent();
                    List<string> filesWithErrors = new List<string>();
                    build.SolutionName = dte.Solution.FullName;
                    build.EventDate = DateTime.UtcNow;

                    //start at 1 when iterating through Error List
                    for (int i = 1; i <= dte.ToolWindows.ErrorList.ErrorItems.Count; i++)
                    {
                        ErrorItem item = dte.ToolWindows.ErrorList.ErrorItems.Item(i);
                        BuildEventErrorListItem beli = new BuildEventErrorListItem();
                        beli.BuildEvent = build;
                        beli.ErrorListItem = ErrorListItem.FromErrorItem(item);

                        //only worry about critical errors
                        if (beli.ErrorListItem.CriticalErrorName.Length > 0)
                        {
                            build.ErrorItems.Add(beli);

                            //add the file with the error to our list of items that have errors
                            if (filesWithErrors.Contains(beli.ErrorListItem.File.ToLower()) == false)
                            {
                                filesWithErrors.Add(beli.ErrorListItem.File.ToLower());
                            }
                        }
                    }

                    //add in breakpoint information
                    for (int i = 1; i <= dte.Debugger.Breakpoints.Count; i++)
                    {
                        BreakPoint bp = new BreakPoint(dte.Debugger.Breakpoints.Item(i));
                        BuildEventBreakPoint bebp = new BuildEventBreakPoint();
                        bebp.BreakPoint = bp;
                        bebp.BuildEvent = build;
                        build.Breakpoints.Add(bebp);
                    }

                    //get all files in the solution
                    List<CodeDocument> files = build.GetSolutionFiles(dte.Solution);

                    //add in associated documents
                    foreach (CodeDocument file in files)
                    {
                        BuildDocument bd = new BuildDocument();
                        bd.Build = build;
                        bd.Document = file;
                        build.Documents.Add(bd);
                    }

                    byte[] data = EventFactory.ToZippedBinary(build);

                    //let others know that we have created a new event
                    NotifyEventCreated(this, new EventCreatedArgs(build));
                //}
                //);
        }

        public override void GenericCommand_AfterCommandExecute(string Guid, int ID, object CustomIn, object CustomOut)
        {
            base.GenericCommand_AfterCommandExecute(Guid, ID, CustomIn, CustomOut);
            Command cmd = GetCommand(Guid, ID);
            string commandName = "";
            if (cmd != null)
            {
                commandName = cmd.Name;

                //Speed up the process by always ignoring boring commands
                if (boringCommands.Contains(commandName) == false)
                {
                    IOsbideEvent oEvent = EventFactory.FromCommand(commandName, dte);

                    //protect against the off-chance that we'll get a null return value
                    if (oEvent != null)
                    {
                        //let others know that we have created a new event
                        NotifyEventCreated(this, new EventCreatedArgs(oEvent));
                    }
                }
            }
        }

        /// <summary>
        /// Called whenever the current line gets modified (text added / deleted).  Only raised at a maximum of
        /// once per minute in order to undercut the potential flood of event notifications.
        /// </summary>
        /// <param name="StartPoint"></param>
        /// <param name="EndPoint"></param>
        /// <param name="Hint"></param>
        public override void EditorLineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint)
        {
            base.EditorLineChanged(StartPoint, EndPoint, Hint);
            if (LastEditorActivityEvent < DateTime.UtcNow.Subtract(new TimeSpan(0, 1, 0)))
            {
                LastEditorActivityEvent = DateTime.UtcNow;
                EditorActivityEvent activity = new EditorActivityEvent();
                activity.EventDate = DateTime.UtcNow;
                activity.SolutionName = Path.GetFileName(dte.Solution.FullName);
                NotifyEventCreated(this, new EventCreatedArgs(activity));
            }
        }

        public override void OnExceptionThrown(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            base.OnExceptionThrown(ExceptionType, Name, Code, Description, ref ExceptionAction);
            HandleException(ExceptionType, Name, Code, Description, ref ExceptionAction);
        }

        public override void OnExceptionNotHandled(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            base.OnExceptionNotHandled(ExceptionType, Name, Code, Description, ref ExceptionAction);
            HandleException(ExceptionType, Name, Code, Description, ref ExceptionAction);
        }

        private void HandleException(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction)
        {
            ExceptionEvent ex = new ExceptionEvent();
            int depthCounter = 0;

            EnvDTE90a.Debugger4 debugger = dte.Debugger as EnvDTE90a.Debugger4;

            if (debugger != null)
            {

                //not sure when the current thread could be NULL, but you never know with
                //the DTE.
                if (debugger.CurrentThread != null)
                {
                    foreach (EnvDTE.StackFrame dteFrame in debugger.CurrentThread.StackFrames)
                    {
                        EnvDTE90a.StackFrame2 frame = (StackFrame2)dteFrame;
                        Models.StackFrame modelFrame = new Models.StackFrame(frame);
                        modelFrame.Depth = depthCounter;
                        ex.StackFrames.Add(modelFrame);
                        depthCounter++;
                    }
                }
            }

            //the stuff inside this try will be null if there isn't an open document
            //window (rare, but possible)
            try
            {
                TextSelection debugSelection = dte.ActiveDocument.Selection;
                debugSelection.SelectLine();
                ex.LineContent = debugSelection.Text;
                ex.LineNumber = debugSelection.CurrentLine;
                ex.DocumentName = dte.ActiveDocument.Name;
            }
            catch (Exception)
            {
                ex.LineContent = "";
                ex.LineNumber = 0;
                ex.DocumentName = dte.Solution.FullName;
            }

            ex.EventDate = DateTime.UtcNow;
            ex.ExceptionAction = (int)ExceptionAction;
            ex.ExceptionCode = Code;
            ex.ExceptionDescription = Description;
            ex.ExceptionName = Name;
            ex.ExceptionType = ExceptionType;
            ex.SolutionName = dte.Solution.FullName;
            NotifyEventCreated(this, new EventCreatedArgs(ex));
        }

        #endregion
    }
}
