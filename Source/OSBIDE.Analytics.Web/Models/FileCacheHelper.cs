using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Web;

namespace OSBIDE.Analytics.Web.Models
{
    public class FileCacheHelper
    {

        /// <summary>
        /// Returns a cache instance for global, cross-session information.  You probably
        /// should use <see cref="GetCacheInstance"/> instead.
        /// </summary>
        /// <returns></returns>
        public static FileCache GetGlobalCacheInstance()
        {
            FileCache fc = new FileCache(FileCacheHelper.CachePath, new ObjectBinder());
            fc.DefaultRegion = "global";
            fc.DefaultPolicy = new CacheItemPolicy() { SlidingExpiration = new TimeSpan(7, 0, 0, 0) };
            return fc;
        }

        public static string CachePath
        {
            get
            {
                return Path.Combine(HttpContext.Current.Server.MapPath("~\\App_Data\\"), "Cache");
            }
        }
    }
}