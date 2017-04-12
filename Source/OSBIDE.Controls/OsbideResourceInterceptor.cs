using Awesomium.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OSBIDE.Library;

namespace OSBIDE.Controls
{
    public class OsbideResourceInterceptor : IResourceInterceptor
    {
        public class ResourceInterceptorEventArgs : EventArgs
        {
            public OsbideVsComponent Component { get; set; }
            public string Url { get; set; }
        }

        public event EventHandler<ResourceInterceptorEventArgs> NavigationRequested = delegate { };
        private static OsbideResourceInterceptor _instance = null;

        private OsbideResourceInterceptor()
        {
        }

        public static OsbideResourceInterceptor Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OsbideResourceInterceptor();
                }
                return _instance;
            }
        }

        public bool OnFilterNavigation(NavigationRequest request)
        {
            string pattern = @"component=([\w]+)";
            string subject = request.Url.Query;
            Match match = Regex.Match(subject, pattern);
                
                //ignore bad matches
                if (match.Groups.Count == 2)
                {
                    OsbideVsComponent component = OsbideVsComponent.None;
                    if(Enum.TryParse(match.Groups[1].Value, out component) == true)
                    {
                        if(NavigationRequested != null)
                        {
                            string url = Regex.Replace(request.Url.ToString(), pattern, "");
                            ResourceInterceptorEventArgs args = new ResourceInterceptorEventArgs()
                            {
                                Component = component,
                                Url = url
                            };
                            NavigationRequested(this, args);
                            return true;
                        }
                    }
                }
            return false;
        }

        public ResourceResponse OnRequest(ResourceRequest request)
        {
            //returning null results in normal behavior
            return null;
        }
    }
}
