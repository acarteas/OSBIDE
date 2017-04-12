using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;

namespace OSBIDE.Library.Events
{

    /// <summary>
    /// The EventHandlerBase class consolidates all of the various event handler types into a single class for
    /// easy inheritance.  By default, each event handler does nothing.  
    /// </summary>
    public abstract class EventHandlerBase
    {
        /// <summary>
        /// This event is raised whenever a new event log has been created and is ready for consumption
        /// </summary>
        public event EventHandler<EventCreatedArgs> EventCreated = delegate { };

        /// <summary>
        /// The GUID that contains menu event actions
        /// </summary>
        public static string MenuEventGuid = "{5EFC7975-14BC-11CF-9B2B-00AA00573819}";

        /// <summary>
        /// GUID for physical files and folders
        /// </summary>
        public static string PhysicalFileGuid = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";

        protected DTE2 dte
        {
            get
            {
                DTE2 dteRef = null;
                if (ServiceProvider != null)
                {
                    dteRef = (DTE2)ServiceProvider.GetService(typeof(SDTE));
                }
                return dteRef;
            }
        }
        public IServiceProvider ServiceProvider { get; set; }

        private IOsbideEventGenerator _osbideEvents;

        private BuildEvents buildEvents = null;
        private CommandEvents genericCommandEvents = null;
        private CommandEvents menuCommandEvents = null;
        private DebuggerEvents debuggerEvents = null;
        private DocumentEvents documentEvents = null;
        private FindEvents findEvents = null;
        private ProjectItemsEvents miscFileEvents = null;
        private OutputWindowEvents outputWindowEvents = null;
        private SelectionEvents selectionEvents = null;
        private SolutionEvents solutionEvents = null;
        private ProjectItemsEvents solutionItemsEvents = null;
        private TextEditorEvents textEditorEvents = null;

        public EventHandlerBase(IServiceProvider serviceProvider, IOsbideEventGenerator osbideEvents)
        {
            if (serviceProvider == null)
            {
                throw new Exception("Service provider is null");
            }

            ServiceProvider = serviceProvider;

            //save references to dte events
            buildEvents = dte.Events.BuildEvents;
            genericCommandEvents = dte.Events.CommandEvents;
            menuCommandEvents = dte.Events.get_CommandEvents(MenuEventGuid);
            debuggerEvents = dte.Events.DebuggerEvents;
            documentEvents = dte.Events.DocumentEvents;
            findEvents = dte.Events.FindEvents;
            miscFileEvents = dte.Events.MiscFilesEvents;
            outputWindowEvents = dte.Events.OutputWindowEvents;
            selectionEvents = dte.Events.SelectionEvents;
            solutionEvents = dte.Events.SolutionEvents;
            solutionItemsEvents = dte.Events.SolutionItemsEvents;
            textEditorEvents = dte.Events.TextEditorEvents;

            //attach osbide requests
            _osbideEvents = osbideEvents;
            _osbideEvents.SolutionSubmitRequest += new EventHandler<SubmitAssignmentArgs>(OsbideSolutionSubmitted);
            _osbideEvents.SolutionDownloaded += new EventHandler<SolutionDownloadedEventArgs>(OsbideSolutionDownloaded);
            _osbideEvents.SubmitEventRequested += new EventHandler<SubmitEventArgs>(SubmitEventRequested);

            //attach listeners for dte events
            //build events
            buildEvents.OnBuildBegin += new _dispBuildEvents_OnBuildBeginEventHandler(OnBuildBegin);
            buildEvents.OnBuildDone += new _dispBuildEvents_OnBuildDoneEventHandler(OnBuildDone);

            //generic command events
            genericCommandEvents.AfterExecute += new _dispCommandEvents_AfterExecuteEventHandler(GenericCommand_AfterCommandExecute);
            genericCommandEvents.BeforeExecute += new _dispCommandEvents_BeforeExecuteEventHandler(GenericCommand_BeforeCommandExecute);

            //menu-related command command
            menuCommandEvents.AfterExecute += new _dispCommandEvents_AfterExecuteEventHandler(MenuCommand_AfterExecute);
            menuCommandEvents.BeforeExecute += new _dispCommandEvents_BeforeExecuteEventHandler(MenuCommand_BeforeExecute);

            //debugger events
            debuggerEvents.OnContextChanged += new _dispDebuggerEvents_OnContextChangedEventHandler(OnContextChanged);
            debuggerEvents.OnEnterBreakMode += new _dispDebuggerEvents_OnEnterBreakModeEventHandler(OnEnterBreakMode);
            debuggerEvents.OnEnterDesignMode += new _dispDebuggerEvents_OnEnterDesignModeEventHandler(OnEnterDesignMode);
            debuggerEvents.OnEnterRunMode += new _dispDebuggerEvents_OnEnterRunModeEventHandler(OnEnterRunMode);
            debuggerEvents.OnExceptionNotHandled += new _dispDebuggerEvents_OnExceptionNotHandledEventHandler(OnExceptionNotHandled);
            debuggerEvents.OnExceptionThrown += new _dispDebuggerEvents_OnExceptionThrownEventHandler(OnExceptionThrown);

            //document events
            documentEvents.DocumentClosing += new _dispDocumentEvents_DocumentClosingEventHandler(DocumentClosing);
            documentEvents.DocumentOpened += new _dispDocumentEvents_DocumentOpenedEventHandler(DocumentOpened);
            documentEvents.DocumentSaved += new _dispDocumentEvents_DocumentSavedEventHandler(DocumentSaved);

            //find events
            findEvents.FindDone += new _dispFindEvents_FindDoneEventHandler(FindDone);

            //misc file events
            miscFileEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(ProjectItemAdded);
            miscFileEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(ProjectItemRemoved);
            miscFileEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(ProjectItemRenamed);

            //output window events
            outputWindowEvents.PaneUpdated += new _dispOutputWindowEvents_PaneUpdatedEventHandler(OutputPaneUpdated);

            //selection events
            selectionEvents.OnChange += new _dispSelectionEvents_OnChangeEventHandler(SelectionChange);

            //solution events
            solutionEvents.BeforeClosing += new _dispSolutionEvents_BeforeClosingEventHandler(SolutionBeforeClosing);
            solutionEvents.Opened += new _dispSolutionEvents_OpenedEventHandler(SolutionOpened);
            solutionEvents.ProjectAdded += new _dispSolutionEvents_ProjectAddedEventHandler(ProjectAdded);
            solutionEvents.Renamed += new _dispSolutionEvents_RenamedEventHandler(SolutionRenamed);

            //solution item events
            solutionItemsEvents.ItemAdded += new _dispProjectItemsEvents_ItemAddedEventHandler(SolutionItemAdded);
            solutionItemsEvents.ItemRemoved += new _dispProjectItemsEvents_ItemRemovedEventHandler(SolutionItemRemoved);
            solutionItemsEvents.ItemRenamed += new _dispProjectItemsEvents_ItemRenamedEventHandler(SolutionItemRenamed);

            //text editor events
            textEditorEvents.LineChanged += new _dispTextEditorEvents_LineChangedEventHandler(EditorLineChanged);
        }

        protected void NotifyEventCreated(object sender, EventCreatedArgs eventArgs)
        {
            EventCreated(sender, eventArgs);
        }

        //OSBIDE-specific event handlers 
        public virtual void OsbideSolutionSubmitted(object sender, SubmitAssignmentArgs e) { }
        public virtual void OsbideSolutionDownloaded(object sender, SolutionDownloadedEventArgs e) { }
        public virtual void SubmitEventRequested(object sender, SubmitEventArgs e) { }

        //build event handlers
        public virtual void OnBuildBegin(vsBuildScope Scope, vsBuildAction Action) { }
        public virtual void OnBuildDone(vsBuildScope Scope, vsBuildAction Action) { }

        //command event handlers
        public virtual void GenericCommand_AfterCommandExecute(string Guid, int ID, object CustomIn, object CustomOut) { }
        public virtual void GenericCommand_BeforeCommandExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault) { }

        //generic command event handlers
        public virtual void MenuCommand_BeforeExecute(string Guid, int ID, object CustomIn, object CustomOut, ref bool CancelDefault) { }
        public virtual void MenuCommand_AfterExecute(string Guid, int ID, object CustomIn, object CustomOut) { }

        //debugger event handlers
        public virtual void OnContextChanged(Process NewProcess, Program NewProgram, Thread NewThread, StackFrame NewStackFrame) { }
        public virtual void OnEnterBreakMode(dbgEventReason Reason, ref dbgExecutionAction ExecutionAction) { }
        public virtual void OnEnterDesignMode(dbgEventReason Reason) { }
        public virtual void OnEnterRunMode(dbgEventReason Reason) { }
        public virtual void OnExceptionNotHandled(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction) { }
        public virtual void OnExceptionThrown(string ExceptionType, string Name, int Code, string Description, ref dbgExceptionAction ExceptionAction) { }

        //document event handlers
        public virtual void DocumentClosing(Document Document) { }
        public virtual void DocumentOpened(Document Document) { }
        public virtual void DocumentSaved(Document Document) { }

        //find event handlers
        public virtual void FindDone(vsFindResult Result, bool Cancelled) { }

        //misc file event handlers
        public virtual void ProjectItemAdded(ProjectItem ProjectItem) { }
        public virtual void ProjectItemRemoved(ProjectItem ProjectItem) { }
        public virtual void ProjectItemRenamed(ProjectItem ProjectItem, string OldName) { }

        //output window event handlers
        public virtual void OutputPaneUpdated(OutputWindowPane pPane) { }

        //selection event handlers
        public virtual void SelectionChange() { }

        //solution event handlers
        public virtual void SolutionBeforeClosing() { }
        public virtual void SolutionOpened()
        {
            //Load exception handling on each project open.  Note that I'm only
            //loading C related groups as loading the entire collection takes
            //a very (10+ minute) long time to load.
            EnvDTE90.Debugger3 debugger = (EnvDTE90.Debugger3)dte.Debugger;
            string[] exceptionGroups = { "C++ Exceptions", "Win32 Exceptions", "Native Run-Time Checks" };

            if (debugger != null)
            {
                if (debugger.ExceptionGroups != null)
                {
                    foreach (EnvDTE90.ExceptionSettings settings in debugger.ExceptionGroups)
                    {
                        string settingsName = settings.Name;
                        if (exceptionGroups.Contains(settingsName))
                        {
                            foreach (EnvDTE90.ExceptionSetting setting in settings)
                            {
                                string name = setting.Name;
                                settings.SetBreakWhenThrown(true, setting);
                            }
                        }
                    }
                }
            }
        }
        public virtual void ProjectAdded(Project Project) { }
        public virtual void SolutionRenamed(string OldName) { }

        //solution item event handlers
        public virtual void SolutionItemAdded(ProjectItem ProjectItem) { }
        public virtual void SolutionItemRemoved(ProjectItem ProjectItem) { }
        public virtual void SolutionItemRenamed(ProjectItem ProjectItem, string OldName) { }

        //text editor event handlers
        public virtual void EditorLineChanged(TextPoint StartPoint, TextPoint EndPoint, int Hint) { }
    }
}
