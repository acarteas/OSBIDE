using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Events
{
    public class EventCreatedArgs : EventArgs
    {
        public IOsbideEvent OsbideEvent { get; private set; }

        public EventCreatedArgs(IOsbideEvent osbideEvent)
        {
            OsbideEvent = osbideEvent;
        }
    }
}
