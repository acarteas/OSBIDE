using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Library.Extensions
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Converts the current DateTime to local time.  Note that this assumes that the
        /// supplied date is already in UTC time!
        /// </summary>
        /// <param name="utcTime"></param>
        /// <returns></returns>
        public static DateTime LocalFromUtc(this DateTime utcTime)
        {
            DateTime utc = new DateTime(utcTime.Ticks, DateTimeKind.Utc);
            DateTime local = utc.ToLocalTime();
            return local;
        }
    }
}
