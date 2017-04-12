using OSBIDE.Analytics.Library.Models;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Web;
using System.Web.Mvc;

namespace OSBIDE.Analytics.Web.Controllers
{
    public abstract class ControllerBase : Controller
    {
        protected AnalyticsDbContext Db { get; private set; }
        protected OsbideContext OsbideDb { get; private set; }
        protected FileCache GlobalCache { get; private set; }

        public ControllerBase()
        {
            Db = AnalyticsDbContext.DefaultWebConnection;
            OsbideDb = OsbideContext.DefaultWebConnection;
            GlobalCache = new FileCache(HttpContext.Server.MapPath("~/App_Data/Cache/"));
        }

        public new HttpContextBase HttpContext
        {
            get
            {
                HttpContextWrapper context =
                    new HttpContextWrapper(System.Web.HttpContext.Current);
                return (HttpContextBase)context;
            }
        }
    }
}