using OSBIDE.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Plugins.Base
{
    public static class Cache
    {
        public static FileCache CacheInstance
        {
            get
            {
                return new FileCache(StringConstants.LocalCacheDirectory);
            }
        }
    }
}
