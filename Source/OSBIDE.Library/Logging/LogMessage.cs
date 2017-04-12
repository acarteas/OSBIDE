using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Logging
{
    public enum LogPriority : byte
    {
        LowPriority = 0,
        MediumPriority = 1,
        HighPriority = 2
    };

    public class LogMessage
    {
        public LogPriority Priority { get; set; }
        public string Message { get; set; }
    }
}
