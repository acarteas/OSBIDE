using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OSBIDE.Library.Logging
{
    public class LocalErrorLogger : ILogger
    {
        string filePath = "";
        public LogPriority MinimumPriority { get; set; }
        public string FilePath
        {
            get
            {
                return filePath;
            }
        }

        public LocalErrorLogger()
        {
            filePath = StringConstants.LocalErrorLogPath;
            MinimumPriority = LogPriority.MediumPriority;
        }

        public LocalErrorLogger(string filePath)
            : this()
        {
            this.filePath = filePath;
        }

        public void WriteToLog(LogMessage message)
        {
            //ignore events below our threshold.
            if (message.Priority < MinimumPriority)
            {
                return;
            }
            lock (this)
            {
                try
                {
                    using (StreamWriter writer = File.AppendText(filePath))
                    {
                        string text = string.Format("{0},{1},{2}",
                            message.Priority,
                            DateTime.UtcNow.ToString("HH:mm:ss"),
                            message.Message
                            );
                        writer.WriteLine(text);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        public void WriteToLog(string content)
        {
            LogMessage message = new LogMessage()
            {
                Message = content,
                Priority = LogPriority.LowPriority
            };
            WriteToLog(message);
        }

        public void WriteToLog(string content, LogPriority priority)
        {
            LogMessage message = new LogMessage()
            {
                Message = content,
                Priority = priority
            };
            WriteToLog(message);
        }
    }
}
