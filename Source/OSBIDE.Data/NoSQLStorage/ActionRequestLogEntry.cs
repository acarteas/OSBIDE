using System;
using System.Globalization;

using Microsoft.WindowsAzure.Storage.Table;

namespace OSBIDE.Data.NoSQLStorage
{
    internal class ActionRequestLogEntry : TableEntity
    {
        public DateTime AccessDate { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string ActionParameters { get; set; }
        public string IpAddress { get; set; }
        public int CreatorId { get; set; }
    }
}
