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
namespace OSBIDE.Plugins.Base
{
    [Guid("eee1c7ba-00ea-4b22-88d7-6cb17837c3da")]
    public class AskTheProfessorToolWindow : ToolWindowPane
    {
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public AskTheProfessorToolWindow() :
            base(null)
        {
            // Set the window title reading it from the resources.
            this.Caption = "Ask the Professor";
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            BrowserView view = new BrowserView();
            FileCache cache = Cache.CacheInstance;
            string authKey = "";
            try
            {
                authKey = cache[StringConstants.AuthenticationCacheKey].ToString();
            }
            catch (Exception)
            {
                authKey = "";
            }
            view.ViewModel = new Controls.ViewModels.BrowserViewModel()
            {
                Url = StringConstants.AskTheProfessorUrl,
                AuthKey = authKey
            };
            base.Content = view;
        }
    }
}
