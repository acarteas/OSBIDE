using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Library.Models
{
    public class TimelineState
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsSocialEvent { get; set; }
        public string State { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        [NotMapped]
        public TimeSpan TimeInState
        {
            get
            {
                //make sure end time is indeed larger
                if (EndTime > StartTime)
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
        [NotMapped]
        public double NormalizedTimeInState { get; set; }
    }
}
