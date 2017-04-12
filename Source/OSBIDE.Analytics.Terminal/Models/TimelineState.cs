using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Models
{
    public class TimelineState
    {
        public int UserId { get; set; }
        public bool IsSocialEvent { get; set; }
        public string State { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public TimeSpan TimeInState { 
            get
            {
                //make sure end time is indeed larger
                if(EndTime > StartTime)
                {
                    return EndTime - StartTime;
                }
                else
                {
                    //else, just return minvalue
                    return new TimeSpan(0, 0, 0);
                }
            }
        }

        /// <summary>
        /// Used for representing normalized time in state (out of 100%)
        /// </summary>
        public double NormalizedTimeInState { get; set; }
    }
}
