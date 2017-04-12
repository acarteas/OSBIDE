using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using OSBIDE.Plugins.Base;
using OSBIDE.Controls.WebServices;
using OSBIDE.Library.Events;
using OSBIDE.Library.Logging;
using OSBIDE.Library.ServiceClient;
using OSBIDE.Library.Models;
using System.Reflection;
using System.Runtime.Caching;
using OSBIDE.Library;
using System.Windows;
using OSBIDE.Controls.ViewModels;
using EnvDTE80;
using System.Windows.Documents;
using OSBIDE.Controls.Views;
using System.IO;
using OSBIDE.Controls;
using Microsoft.VisualStudio.CommandBars;
using System.Net;
using System.Security.Cryptography;

namespace OSBIDE.Plugins.VS2012
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the information needed to show this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(ActivityFeedToolWindow), Style = VsDockStyle.Tabbed, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
    [ProvideToolWindow(typeof(ActivityFeedDetailsToolWindow), Style = VsDockStyle.MDI, MultiInstances = true)]
    [ProvideToolWindow(typeof(ChatToolWindow), Style = VsDockStyle.MDI)]
    [ProvideToolWindow(typeof(UserProfileToolWindow), Style = VsDockStyle.MDI)]
    [ProvideToolWindow(typeof(CreateAccountToolWindow), Style = VsDockStyle.MDI)]
    [ProvideToolWindow(typeof(GenericOsbideToolWindow), Style = VsDockStyle.MDI)]
    //[ProvideToolWindow(typeof(AskTheProfessorToolWindow))]
    [ProvideAutoLoad(UIContextGuids80.NoSolution)]
    [Guid(CommonGuidList.guidOSBIDE_VSPackagePkgString)]
    public sealed class OSBIDE_Plugins_VS2012Package : Package, IDisposable, IVsShellPropertyEvents
    {
        private OsbideWebServiceClient _webServiceClient = null;
        private OsbideEventHandler _eventHandler = null;
        private ILogger _errorLogger = new LocalErrorLogger();
        private ServiceClient _client;
        private FileCache _cache = Cache.CacheInstance;
        private string _userName = null;
        private string _userPassword = null;
        private OsbideToolWindowManager _manager = null;
        private uint _EventSinkCookie;
        private EnvDTE.CommandBarEvents _osbideErrorListEvent;
        private RijndaelManaged _encoder = new RijndaelManaged();

        //If OSBIDE isn't up to date, don't allow logging as it means that we've potentially 
        //changed the way the web service operates
        private bool _isOsbideUpToDate = true;

        private bool _hasStartupErrors = false;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public OSBIDE_Plugins_VS2012Package()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overridden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            //AC: Explicity load in assemblies.  Necessary for serialization (why?)
            Assembly.Load("OSBIDE.Library");
            Assembly.Load("OSBIDE.Controls");

            //force load awesomium dlls
            /*
            try
            {
                Assembly.Load("Awesomium.Core, Version=1.7.1.0, Culture=neutral, PublicKeyToken=e1a0d7c8071a5214");
                Assembly.Load("Awesomium.Windows.Controls, Version=1.7.1.0, Culture=neutral, PublicKeyToken=7a34e179b8b61c39");
            }
            catch (Exception ex)
            {
                _errorLogger.WriteToLog("Error loading awesomium DLLs: " + ex.Message, LogPriority.HighPriority);
            }
             * */

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                //login toolbar item.
                CommandID menuCommandID = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideCommand);
                MenuCommand menuItem = new MenuCommand(OpenLoginScreen, menuCommandID);
                mcs.AddCommand(menuItem);

                //login toolbar menu option.
                CommandID loginMenuOption = new CommandID(CommonGuidList.guidOSBIDE_OsbideToolsMenuCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideLoginToolWin);
                MenuCommand menuLoginMenuOption = new MenuCommand(OpenLoginScreen, loginMenuOption);
                mcs.AddCommand(menuLoginMenuOption);

                //activity feed
                CommandID activityFeedId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideActivityFeedTool);
                MenuCommand menuActivityWin = new MenuCommand(ShowActivityFeedTool, activityFeedId);
                mcs.AddCommand(menuActivityWin);

                //activity feed details
                CommandID activityFeedDetailsId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideActivityFeedDetailsTool);
                MenuCommand menuActivityDetailsWin = new MenuCommand(ShowActivityFeedDetailsTool, activityFeedDetailsId);
                mcs.AddCommand(menuActivityDetailsWin);

                //chat window
                CommandID chatWindowId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideChatTool);
                MenuCommand menuChatWin = new MenuCommand(ShowChatTool, chatWindowId);
                mcs.AddCommand(menuChatWin);

                //profile page
                CommandID profileWindowId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideUserProfileTool);
                MenuCommand menuProfileWin = new MenuCommand(ShowProfileTool, profileWindowId);
                mcs.AddCommand(menuProfileWin);

                //"ask for help context" menu
                CommandID askForHelpId = new CommandID(CommonGuidList.guidOSBIDE_ContextMenuCmdSet, (int)CommonPkgCmdIDList.cmdOsbideAskForHelp);
                OleMenuCommand askForHelpWin = new OleMenuCommand(ShowAskForHelp, askForHelpId);
                askForHelpWin.BeforeQueryStatus += AskForHelpCheckActive;
                mcs.AddCommand(askForHelpWin);

                //create account window
                CommandID createAccountWindowId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideCreateAccountTool);
                MenuCommand menuAccountWin = new MenuCommand(ShowCreateAccountTool, createAccountWindowId);
                mcs.AddCommand(menuAccountWin);

                //OSBIDE documentation link
                CommandID documentationId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideDocumentationTool);
                MenuCommand documentationWin = new MenuCommand(OpenDocumentation, documentationId);
                mcs.AddCommand(documentationWin);

                //OSBIDE web link
                CommandID webLinkId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideWebLinkTool);
                MenuCommand webLinkWin = new MenuCommand(OpenOsbideWebLink, webLinkId);
                mcs.AddCommand(webLinkWin);

                //generic OSBIDE window
                CommandID genericId = new CommandID(CommonGuidList.guidOSBIDE_VSPackageCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideGenericToolWindow);
                MenuCommand genericWindow = new MenuCommand(ShowGenericToolWindow, genericId);
                mcs.AddCommand(genericWindow);

                //submit assignment command
                CommandID submitCommand = new CommandID(CommonGuidList.guidOSBIDE_OsbideToolsMenuCmdSet, (int)CommonPkgCmdIDList.cmdidOsbideSubmitAssignmentCommand);
                MenuCommand submitMenuItem = new MenuCommand(SubmitAssignmentCallback, submitCommand);
                mcs.AddCommand(submitMenuItem);

                //ask the professor window 
                //(commented out for Fall 2013 release at instructor request)
                /*
                CommandID askProfessorWindowId = new CommandID(GuidList.guidOSBIDE_VSPackageCmdSet, (int)PkgCmdIDList.cmdidOsbideAskTheProfessor);
                MenuCommand menuAskProfessorWin = new MenuCommand(ShowAskProfessorTool, askProfessorWindowId);
                mcs.AddCommand(menuAskProfessorWin);
                 * */

                // -- Set an event listener for shell property changes
                var shellService = GetService(typeof(SVsShell)) as IVsShell;
                if (shellService != null)
                {
                    ErrorHandler.ThrowOnFailure(shellService.
                      AdviseShellPropertyChanges(this, out _EventSinkCookie));
                }

            }

            //add right-click context menu to the VS Error List
            DTE2 dte = (DTE2)this.GetService(typeof(SDTE));
            if (dte != null)
            {
                CommandBars cmdBars = (CommandBars)dte.CommandBars;
                CommandBar errorListBar = cmdBars[10];

                CommandBarControl osbideControl = errorListBar.Controls.Add(MsoControlType.msoControlButton,
                                                                      System.Reflection.Missing.Value,
                                                                      System.Reflection.Missing.Value, 1, true);
                // Set the caption of the submenuitem
                osbideControl.Caption = "View Error in OSBIDE";
                _osbideErrorListEvent = (EnvDTE.CommandBarEvents)dte.Events.get_CommandBarEvents(osbideControl);
                _osbideErrorListEvent.Click += osbideCommandBarEvent_Click;
            }

            //create our web service
            _webServiceClient = new OsbideWebServiceClient(ServiceBindings.OsbideServiceBinding, ServiceBindings.OsbideServiceEndpoint);
            _webServiceClient.LibraryVersionNumberCompleted += InitStepThree_CheckServiceVersionComplete;
            _webServiceClient.LoginCompleted += InitStepTwo_LoginCompleted;
            _webServiceClient.GetMostRecentWhatsNewItemCompleted += GetRecentNewsItemDateComplete;

            //set up local encryption
            if (_cache.Contains(StringConstants.AesKeyCacheKey) == false)
            {
                _encoder.GenerateKey();
                _encoder.GenerateIV();
                _cache[StringConstants.AesKeyCacheKey] = _encoder.Key;
                _cache[StringConstants.AesVectorCacheKey] = _encoder.IV;
            }
            else
            {
                _encoder.Key = _cache[StringConstants.AesKeyCacheKey] as byte[];
                _encoder.IV = _cache[StringConstants.AesVectorCacheKey] as byte[];
            }

            //set up user name and password
            _userName = _cache[StringConstants.UserNameCacheKey] as string;
            byte[] passwordBytes = _cache[StringConstants.PasswordCacheKey] as byte[];
            try
            {
                _userPassword = AesEncryption.DecryptStringFromBytes_Aes(passwordBytes, _encoder.Key, _encoder.IV);
            }
            catch (Exception)
            {
            }
            

            //set up tool window manager
            _manager = new OsbideToolWindowManager(_cache as FileCache, this);

            //set up service logger
            _eventHandler = new OsbideEventHandler(this as System.IServiceProvider, EventGenerator.GetInstance());
            _client = ServiceClient.GetInstance(_eventHandler, _errorLogger);
            _client.PropertyChanged += ServiceClientPropertyChanged;
            _client.ReceivedNewSocialActivity += ServiceClientReceivedSocialUpdate;
            UpdateSendStatus();

            //display a user notification if we don't have any user on file
            if (_userName == null || _userPassword == null)
            {
                _hasStartupErrors = true;
                MessageBoxResult result = MessageBox.Show("Thank you for installing OSBIDE.  To complete the installation, you must enter your user information, which can be done by clicking on the \"Tools\" menu and selecting \"Log into OSBIDE\".", "Welcome to OSBIDE", MessageBoxButton.OK);
            }
            else
            {
                //step #1: attempt login
                string hashedPassword = UserPassword.EncryptPassword(_userPassword, _userName);

                try
                {
                    _webServiceClient.LoginAsync(_userName, hashedPassword);
                }
                catch (Exception ex)
                {
                    _errorLogger.WriteToLog("Web service error: " + ex.Message, LogPriority.HighPriority);
                    _hasStartupErrors = true;
                }
            }

        }
        #endregion



        #region AC Code

        /// <summary>
        /// The function continues the initialization process, picking up where Initialize() left off
        /// having called OSBIDE's login service
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InitStepTwo_LoginCompleted(object sender, LoginCompletedEventArgs e)
        {
            string authKey = "";
            try
            {
                if (e != null)
                {
                    if (e.Result != null)
                    {
                        authKey = e.Result;
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLogger.WriteToLog("Web service error: " + ex.Message, LogPriority.HighPriority);
                _hasStartupErrors = true;
                return;
            }
            if (authKey.Length <= 0)
            {
                MessageBoxResult result = MessageBox.Show("It appears as though your OSBIDE user name or password has changed since the last time you opened Visual Studio.  Would you like to log back into OSBIDE?", "Log Into OSBIDE", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    OpenLoginScreen(this, EventArgs.Empty);
                }
            }
            else
            {
                _cache[StringConstants.AuthenticationCacheKey] = authKey;
            }

            //having logged in, we can now check to make sure we're up to date
            try
            {
                _webServiceClient.LibraryVersionNumberAsync();
            }
            catch (Exception ex)
            {
                //write to the log file
                _errorLogger.WriteToLog(string.Format("LibraryVersionNumberAsync error: {0}", ex.Message), LogPriority.HighPriority);
                _hasStartupErrors = true;
            }
        }

        /// <summary>
        /// This function continues the initialization process by picking up where InitStepTwo_LoginCompleted() left off.
        /// Namely, we assume that we've gone through the login process and have just received word if the local copy of
        /// OSBIDE is up to date.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void InitStepThree_CheckServiceVersionComplete(object sender, LibraryVersionNumberCompletedEventArgs e)
        {

            string remoteVersionNumber = "";

            try
            {
                if (e != null)
                {
                    if (e.Result != null)
                    {
                        remoteVersionNumber = e.Result;
                    }
                }
            }
            catch (Exception ex)
            {
                _errorLogger.WriteToLog("Web service error: " + ex.Message, LogPriority.HighPriority);
                _hasStartupErrors = true;
                return;
            }

            //if we have a version mismatch, stop sending data to the server & delete localDb
            if (StringConstants.LibraryVersion.CompareTo(remoteVersionNumber) != 0)
            {
                _isOsbideUpToDate = false;

                //download updated library version
                WebClient web = new WebClient();
                web.DownloadFileCompleted += web_DownloadFileCompleted;
                web.Headers.Add(HttpRequestHeader.UserAgent, "OSBIDE");
                if (File.Exists(StringConstants.LocalUpdatePath) == true)
                {
                    try
                    {
                        File.Delete(StringConstants.LocalUpdatePath);
                    }
                    catch (Exception)
                    {
                    }
                }
                try
                {
                    web.DownloadFileAsync(new Uri(StringConstants.UpdateUrl), StringConstants.LocalUpdatePath);
                }
                catch (Exception)
                {
                }
            }

            //if we're all up to date and had no startup errors, then we can start sending logs to the server
            if (_isOsbideUpToDate == true && _hasStartupErrors == false)
            {
                _client.StartSending();
                ShowActivityFeedTool(this, EventArgs.Empty);
                _webServiceClient.GetMostRecentWhatsNewItemAsync();
            }
        }

        /// <summary>
        /// Updates our most recent knowledge of any new news posts.  If a new news post exists, then we'll
        /// open a window for the user to browse the latest OSBIDE happenings.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void GetRecentNewsItemDateComplete(object sender, GetMostRecentWhatsNewItemCompletedEventArgs e)
        {
            string newsKey = "MostRecentNewsDate";
            DateTime cachedDate;
            DateTime webDate = DateTime.MinValue;

            //pull local cache value (note I'm being very careful here)
            if (_cache.Contains(newsKey) == false)
            {
                _cache[newsKey] = DateTime.MinValue;
            }
            try
            {
                cachedDate = (DateTime)_cache[newsKey];
            }
            catch (Exception)
            {
                cachedDate = DateTime.MinValue;
                _cache.Remove(newsKey);
            }

            //pull latest date from web
            try
            {
                if (e != null)
                {
                    if (e.Result != null)
                    {
                        webDate = e.Result;
                    }
                }
            }
            catch (Exception)
            {
                webDate = DateTime.MinValue;
            }

            if (webDate > cachedDate)
            {
                //store more recent value
                _cache[newsKey] = webDate;

                //open news page
                OpenGenericToolWindow(StringConstants.WhatsNewUrl);
            }

        }

        /// <summary>
        /// Called when we have a new version of OSBIDE to install
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void web_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            MessageBox.Show("Your version of OSBIDE is out of date.  Installation of the latest version will now begin.");
            try
            {
                System.Diagnostics.Process.Start(StringConstants.LocalUpdatePath);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Used to determine client send status
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ServiceClientPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {

            UpdateSendStatus();
        }


        void ServiceClientReceivedSocialUpdate(object sender, EventArgs e)
        {
            ToggleProfileImage(true);
        }

        void ToggleProfileImage(bool hasSocial)
        {
            var dte = GetService(typeof(SDTE)) as DTE2;
            var cbs = ((Microsoft.VisualStudio.CommandBars.CommandBars)dte.CommandBars);
            Microsoft.VisualStudio.CommandBars.CommandBar cb = cbs["OSBIDE Toolbar"];
            Microsoft.VisualStudio.CommandBars.CommandBarControl toolsControl = cb.Controls["My OSBIDE Profile"];
            Microsoft.VisualStudio.CommandBars.CommandBarButton profileButton = toolsControl as Microsoft.VisualStudio.CommandBars.CommandBarButton;

            if (hasSocial == true)
            {
                profileButton.Picture = (stdole.StdPicture)IconConverter.GetIPictureDispFromImage(Resources.profile_new_social);
                profileButton.TooltipText = "New social activity detected";
            }
            else
            {
                profileButton.Picture = (stdole.StdPicture)IconConverter.GetIPictureDispFromImage(Resources.profile);
                profileButton.TooltipText = "View your profile";
            }
        }

        private void osbideCommandBarEvent_Click(object CommandBarControl, ref bool Handled, ref bool CancelDefault)
        {
            ErrorListItem listItem = new ErrorListItem();
            DTE2 dte = (DTE2)this.GetService(typeof(SDTE));
            if (dte != null)
            {
                Array selectedItems = (Array)dte.ToolWindows.ErrorList.SelectedItems;
                if (selectedItems != null)
                {
                    foreach (ErrorItem item in selectedItems)
                    {
                        listItem = ErrorListItem.FromErrorItem(item);
                    }
                }
            }

            if (string.IsNullOrEmpty(listItem.CriticalErrorName) == false)
            {
                string url = string.Format("{0}?errorTypeStr={1}&component={2}", StringConstants.ActivityFeedUrl, listItem.CriticalErrorName, OsbideVsComponent.FeedOverview);
                OpenActivityFeedWindow(url);
            }
            else
            {
                MessageBox.Show("OSBIDE only supports search for errors");
            }

        }

        private void UpdateSendStatus()
        {
            var dte = GetService(typeof(SDTE)) as DTE2;
            var cbs = ((Microsoft.VisualStudio.CommandBars.CommandBars)dte.CommandBars);
            Microsoft.VisualStudio.CommandBars.CommandBar cb = cbs["OSBIDE Toolbar"];
            Microsoft.VisualStudio.CommandBars.CommandBarControl toolsControl = cb.Controls["Log into OSBIDE"];
            Microsoft.VisualStudio.CommandBars.CommandBarButton loginButton = toolsControl as Microsoft.VisualStudio.CommandBars.CommandBarButton;

            if (_client.IsSendingData == true)
            {
                loginButton.Picture = (stdole.StdPicture)IconConverter.GetIPictureDispFromImage(Resources.login_active);
                loginButton.TooltipText = "Connected to OSBIDE";
            }
            else
            {
                loginButton.Picture = (stdole.StdPicture)IconConverter.GetIPictureDispFromImage(Resources.login);
                loginButton.TooltipText = "Not connected to OSBIDE.  Click to log in.";
            }
        }

        public int OnShellPropertyChange(int propid, object propValue)
        {
            // --- We handle the event if zombie state changes from true to false
            if ((int)__VSSPROPID.VSSPROPID_Zombie == propid)
            {
                if ((bool)propValue == false)
                {
                    // --- Show the commandbar
                    var dte = GetService(typeof(SDTE)) as DTE2;
                    var cbs = ((Microsoft.VisualStudio.CommandBars.CommandBars)dte.CommandBars);
                    Microsoft.VisualStudio.CommandBars.CommandBar cb = cbs["OSBIDE Toolbar"];
                    cb.Visible = true;

                    // --- Unsubscribe from events
                    var shellService = GetService(typeof(SVsShell)) as IVsShell;
                    if (shellService != null)
                    {
                        ErrorHandler.ThrowOnFailure(shellService.
                          UnadviseShellPropertyChanges(_EventSinkCookie));
                    }
                    _EventSinkCookie = 0;
                }
            }
            return VSConstants.S_OK;
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            /*
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(OsbideStatusToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }
            if (_db != null)
            {
                AssignmentSubmissionsViewModel avm = new AssignmentSubmissionsViewModel(_db);
                OsbideStatusViewModel vm = new OsbideStatusViewModel();
                vm.SubmissionViewModel = avm;
                vm.StatusViewModel = new TransmissionStatusViewModel();
                (window.Content as OsbideStatus).ViewModel = vm;
            }
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
             * */
        }

        private void ShowAwesomiumError(Exception ex)
        {
            if (ex != null)
            {
                _errorLogger.WriteToLog("Awesomium Error: " + ex.Message, LogPriority.HighPriority);
            }
            MessageBox.Show("It appears as though your system is missing prerequisite components necessary for OSBIDE to operate properly.  Until this is resolved, you will not be able to access certain OSBIDE components within Visual Studio.  You can download the prerequisite files and obtain support by visiting http://osbide.codeplex.com.", "OSBIDE", MessageBoxButton.OK);
        }

        private void OpenDocumentation(object sender, EventArgs e)
        {
            Process.Start(new ProcessStartInfo(StringConstants.DocumentationUrl));
        }

        private void ShowGenericToolWindow(object sender, EventArgs e)
        {
            OpenGenericToolWindow();
        }

        private void OpenGenericToolWindow(string url = "")
        {
            _manager.OpenGenericToolWindow(null, url);
        }

        private void OpenOsbideWebLink(object sender, EventArgs e)
        {
            string authKey = _cache[StringConstants.AuthenticationCacheKey] as string;
            string url = string.Format("{0}?auth={1}", StringConstants.ActivityFeedUrl, authKey);
            Process.Start(new ProcessStartInfo(url));
        }

        private void OpenActivityFeedWindow(string url = "")
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenActivityFeedWindow(null, url);
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowActivityFeedTool(object sender, EventArgs e)
        {
            OpenActivityFeedWindow();
        }

        private void ShowActivityFeedDetailsTool(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenActivityFeedDetailsWindow();
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowChatTool(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenChatWindow();
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowProfileTool(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenProfileWindow();
                    ToggleProfileImage(false);
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        private void ShowCreateAccountTool(object sender, EventArgs e)
        {
            try
            {
                _manager.OpenCreateAccountWindow();
            }
            catch (Exception ex)
            {
                ShowAwesomiumError(ex);
            }

        }

        private void ShowAskProfessorTool(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                try
                {
                    _manager.OpenAskTheProfessorWindow();
                }
                catch (Exception ex)
                {
                    ShowAwesomiumError(ex);
                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
            }
        }

        public void ShowAskForHelp(object sender, EventArgs e)
        {
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == true)
            {
                MessageBox.Show("You must be logged into OSBIDE in order to access this window.");
                return;
            }

            AskForHelpViewModel vm = new AskForHelpViewModel();

            //find current text selection
            DTE2 dte = (DTE2)this.GetService(typeof(SDTE));
            if (dte != null)
            {
                dynamic selection = dte.ActiveDocument.Selection;
                if (selection != null)
                {
                    try
                    {
                        vm.Code = selection.Text;
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            //AC: Restrict "ask for help" to approx 20 lines
            if (vm.Code.Length > 750)
            {
                vm.Code = vm.Code.Substring(0, 750);
            }

            //show message dialog
            MessageBoxResult result = AskForHelpForm.ShowModalDialog(vm);
            if (result == MessageBoxResult.OK)
            {
                EventGenerator generator = EventGenerator.GetInstance();
                AskForHelpEvent evt = new AskForHelpEvent();
                evt.Code = vm.Code;
                evt.UserComment = vm.UserText;
                generator.SubmitEvent(evt);
                MessageBox.Show("Your question has been logged and will show up in the activity stream shortly.");
            }
        }

        private void AskForHelpCheckActive(object sender, EventArgs e)
        {
            var cmd = sender as OleMenuCommand;
            DTE2 dte = (DTE2)this.GetService(typeof(SDTE));
            if (dte != null)
            {
                if (dte.ActiveDocument != null)
                {
                    TextSelection selection = dte.ActiveDocument.Selection as TextSelection;
                    if (selection != null)
                    {
                        string text = selection.Text;
                        if (string.IsNullOrEmpty(text) == true)
                        {
                            cmd.Enabled = false;
                        }
                        else
                        {
                            cmd.Enabled = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// </summary>
        private void OpenLoginScreen(object sender, EventArgs e)
        {
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            OsbideLoginViewModel vm = new OsbideLoginViewModel();
            vm.RequestCreateAccount += ShowCreateAccountTool;

            //attempt to store previously cached values if possible
            vm.Password = _userPassword;
            vm.Email = _userName;
            vm.IsLoggedIn = _client.IsSendingData;

            MessageBoxResult result = OsbideLoginControl.ShowModalDialog(vm);

            //assume that data was changed and needs to be saved
            if (result == MessageBoxResult.OK)
            {
                try
                {

                    _cache[StringConstants.UserNameCacheKey] = vm.Email;
                    _userName = vm.Email;
                    _userPassword = vm.Password;
                    _cache[StringConstants.PasswordCacheKey] = AesEncryption.EncryptStringToBytes_Aes(vm.Password, _encoder.Key, _encoder.IV);
                    _cache[StringConstants.AuthenticationCacheKey] = vm.AuthenticationHash;

                }
                catch (Exception ex)
                {
                    //write to the log file
                    _errorLogger.WriteToLog(string.Format("SaveUser error: {0}", ex.Message), LogPriority.HighPriority);

                    //turn off client sending if we run into an error
                    if (_client != null)
                    {
                        _client.StopSending();
                    }
                }

                //If we got back a valid user, turn on log saving
                if (_userName != null && _userPassword != null)
                {
                    //turn on client sending
                    if (_client != null)
                    {
                        _client.IsCollectingData = true;
                        _client.StartSending();
                    }
                    MessageBox.Show("Welcome to OSBIDE!");
                }
                else
                {
                    //turn off client sending if the user didn't log in.
                    if (_client != null)
                    {
                        _client.StopSending();
                    }
                }
            }
            else if (result == MessageBoxResult.No)
            {
                //In this case, I'm using MessageBoxResult.No to represent a log out request.  We can
                //fake that by just turning off client collection and sending.
                _client.IsCollectingData = false;
                _client.StopSending();
                _cache[StringConstants.AuthenticationCacheKey] = "";
                MessageBox.Show("You have been logged out of OSBIDE.");
            }
        }

        /// <summary>
        /// Called when the user selects the "submit assignment" menu option
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SubmitAssignmentCallback(object sender, EventArgs e)
        {
            IVsUIShell uiShell = (IVsUIShell)GetService(typeof(SVsUIShell));
            Guid clsid = Guid.Empty;
            SubmitEvent evt = new SubmitEvent();
            DTE2 dte = (DTE2)this.GetService(typeof(SDTE));

            if (dte.Solution.FullName.Length == 0)
            {
                MessageBox.Show("No solution is currently open.");
                return;
            }
            object cacheItem = _cache[StringConstants.AuthenticationCacheKey];
            if (cacheItem != null && string.IsNullOrEmpty(cacheItem.ToString()) == false)
            {
                evt.SolutionName = dte.Solution.FullName;

                SubmitAssignmentViewModel vm = new SubmitAssignmentViewModel(
                    _cache[StringConstants.UserNameCacheKey] as string,
                    _cache[StringConstants.AuthenticationCacheKey] as string,
                    evt
                    );
                MessageBoxResult result = SubmitAssignmentWindow.ShowModalDialog(vm);

                //assume that data was changed and needs to be saved
                if (result == MessageBoxResult.OK)
                {

                }
            }
            else
            {
                MessageBox.Show("You must be logged into OSBIDE in order to submit an assignment.");
            }
        }
        #endregion

        public void Dispose()
        {
            _webServiceClient.Close();
            _encoder.Dispose();
        }
    }
}
