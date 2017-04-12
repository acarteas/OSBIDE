using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using OSBIDE.Controls.Views;
using System.Runtime.Caching;
using OSBIDE.Library;
using Microsoft.VisualStudio.Shell.Interop;
using OSBIDE.Controls;
using OSBIDE.Controls.ViewModels;
namespace OSBIDE.Plugins.Base
{
    public class OsbideToolWindowManager
    {
        private OsbideResourceInterceptor _interceptor = OsbideResourceInterceptor.Instance;
        private FileCache _cache = null;
        private Package _vsPackage = null;
        private BrowserViewModel _chatVm = new BrowserViewModel();
        private BrowserViewModel _profileVm = new BrowserViewModel();
        private BrowserViewModel _activityFeedVm = new BrowserViewModel();
        private BrowserViewModel _activityFeedDetailsVm = new BrowserViewModel();
        private BrowserViewModel _createAccountVm = new BrowserViewModel();
        private BrowserViewModel _askTheProfessorVm = new BrowserViewModel();
        private BrowserViewModel _genericWindowVm = new BrowserViewModel();
        private static int _detailsToolWindowId = 0;

        public OsbideToolWindowManager(FileCache cache, Package vsPackage)
        {
            _cache = cache;
            _vsPackage = vsPackage;
            _interceptor.NavigationRequested += NavigationRequested;
            _chatVm.Url = StringConstants.ChatUrl;
            _profileVm.Url = StringConstants.ProfileUrl;
            _activityFeedVm.Url = StringConstants.ActivityFeedUrl;
            _activityFeedDetailsVm.Url = StringConstants.ActivityFeedUrl;
            _createAccountVm.Url = StringConstants.CreateAccountUrl;
            _askTheProfessorVm.Url = StringConstants.AskTheProfessorUrl;
            _genericWindowVm.Url = StringConstants.ProfileUrl;
        }

        public void OpenChatWindow(Package vsPackage = null)
        {
            _chatVm.AuthKey = _cache[StringConstants.AuthenticationCacheKey] as string;
            OpenToolWindow(new ChatToolWindow(), _chatVm, vsPackage);
        }

        public void OpenProfileWindow(Package vsPackage = null)
        {
            _profileVm.AuthKey = _cache[StringConstants.AuthenticationCacheKey] as string;
            OpenToolWindow(new UserProfileToolWindow(), _profileVm, vsPackage);
        }

        public void OpenGenericToolWindow(Package vsPackage = null, string url = "")
        {
            _genericWindowVm.AuthKey = _cache[StringConstants.AuthenticationCacheKey] as string;
            if (string.IsNullOrEmpty(url) == false)
            {
                _genericWindowVm.Url = url;
            }
            OpenToolWindow(new GenericOsbideToolWindow(), _genericWindowVm, vsPackage);
        }

        public void OpenActivityFeedDetailsWindow(Package vsPackage = null)
        {
            _activityFeedDetailsVm.AuthKey = _cache[StringConstants.AuthenticationCacheKey] as string;
            OpenToolWindow(new ActivityFeedDetailsToolWindow(), _activityFeedDetailsVm, vsPackage, _detailsToolWindowId);

            //ensures that we get a new tool window with each click
            _detailsToolWindowId++;
        }

        public void OpenActivityFeedWindow(Package vsPackage = null, string url = "")
        {
            _activityFeedVm.AuthKey = _cache[StringConstants.AuthenticationCacheKey] as string;
            if (string.IsNullOrEmpty(url) == false)
            {
                _activityFeedVm.Url = url;
            }
            OpenToolWindow(new ActivityFeedToolWindow(), _activityFeedVm, vsPackage);
        }

        public void OpenCreateAccountWindow(Package vsPackage = null)
        {
            _createAccountVm.AuthKey = "";
            OpenToolWindow(new CreateAccountToolWindow(), _createAccountVm, vsPackage);
        }

        public void OpenAskTheProfessorWindow(Package vsPackage = null)
        {
            _askTheProfessorVm.AuthKey = _cache[StringConstants.AuthenticationCacheKey] as string;
            OpenToolWindow(new AskTheProfessorToolWindow(), _askTheProfessorVm, vsPackage);
        }

        private void OpenToolWindow(ToolWindowPane pane, BrowserViewModel vm, Package vsPackage, int toolId = 0)
        {
            if (vsPackage == null)
            {
                vsPackage = _vsPackage;
            }
            ToolWindowPane window = vsPackage.FindToolWindow(pane.GetType(), toolId, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("oops!");
            }
            (window.Content as BrowserView).Dispatcher.BeginInvoke(
                (Action)delegate
                {
                    (window.Content as BrowserView).ViewModel = vm;
                }
                );
            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());
        }

        private void NavigationRequested(object sender, OsbideResourceInterceptor.ResourceInterceptorEventArgs e)
        {
            IServiceProvider provider = (_vsPackage as System.IServiceProvider);
            IVsUIShell uiShell = provider.GetService(typeof(SVsUIShell)) as IVsUIShell;

            Guid commandSet = CommonGuidList.guidOSBIDE_VSPackageCmdSet;
            object inputParameters = null;
            if (uiShell == null)
            {
                return;
            }
            _cache[e.Component.ToString()] = e.Url;
            switch (e.Component)
            {
                case OsbideVsComponent.AskTheProfessor:
                    _askTheProfessorVm.Url = e.Url;
                    uiShell.PostExecCommand(ref commandSet, CommonPkgCmdIDList.cmdidOsbideAskTheProfessor, 0, ref inputParameters);
                    break;
                case OsbideVsComponent.CreateAccount:
                    _createAccountVm.Url = e.Url;
                    uiShell.PostExecCommand(ref commandSet, CommonPkgCmdIDList.cmdidOsbideCreateAccountTool, 0, ref inputParameters);
                    break;
                case OsbideVsComponent.Chat:
                    _chatVm.Url = e.Url;
                    uiShell.PostExecCommand(ref commandSet, CommonPkgCmdIDList.cmdidOsbideChatTool, 0, ref inputParameters);
                    break;
                case OsbideVsComponent.FeedDetails:
                    _activityFeedDetailsVm.Url = e.Url;
                    uiShell.PostExecCommand(ref commandSet, CommonPkgCmdIDList.cmdidOsbideActivityFeedDetailsTool, 0, ref inputParameters);
                    break;
                case OsbideVsComponent.FeedOverview:
                    _activityFeedVm.Url = e.Url;
                    uiShell.PostExecCommand(ref commandSet, CommonPkgCmdIDList.cmdidOsbideActivityFeedTool, 0, ref inputParameters);
                    break;
                case OsbideVsComponent.UserProfile:
                    _profileVm.Url = e.Url;
                    uiShell.PostExecCommand(ref commandSet, CommonPkgCmdIDList.cmdidOsbideUserProfileTool, 0, ref inputParameters);
                    break;
                case OsbideVsComponent.GenericComponent:
                    _genericWindowVm.Url = e.Url;
                    uiShell.PostExecCommand(ref commandSet, CommonPkgCmdIDList.cmdidOsbideGenericToolWindow, 0, ref inputParameters);
                    break;
            }
        }
    }
}
