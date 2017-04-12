using OSBIDE.Library.Events;
using OSBIDE.Library.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models
{
    public class FeedItem
    {
        public int LogId { get; set; }
        public EventLog Log { get; set; }

        public int EventId { get; set; }
        public IOsbideEvent Event { get; set; }

        public List<LogCommentEvent> Comments { get; set; }

        public int HelpfulComments { get; set; }

        public OsbideUser Creator
        {
            get
            {
                if (Log != null)
                {
                    return Log.Sender;
                }
                return new OsbideUser();
            }
        }
        public DateTime ItemDate
        {
            get
            {
                if (Log != null)
                {
                    return Log.DateReceived;
                }
                return DateTime.UtcNow;
            }
        }
        public FeedItem()
        {
            Comments = new List<LogCommentEvent>();
        }
    }
}