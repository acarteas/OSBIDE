using OSBIDE.Analytics.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OSBIDE.Analytics.Terminal.Models
{
    public class PatternParserResult
    {
        public List<TimelineState> StateSequence { get; set; }

        /// <summary>
        /// Allows us to know why this particular state was terminated
        /// </summary>
        public TimelineState TerminatingState { get; set; }
    }
}
