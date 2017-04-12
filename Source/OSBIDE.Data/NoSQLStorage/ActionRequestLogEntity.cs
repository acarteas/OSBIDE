using System;
using System.Globalization;

using Microsoft.WindowsAzure.Storage.Table;

namespace OSBIDE.Data.NoSQLStorage
{
    /// <summary>
    /// this is one entry or document of the table storage
    /// the partition key is school id
    /// the row key is student id
    /// </summary>
    internal class ActionRequestLogEntity : TableEntity
    {
        public DateTime AccessDate { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string ActionParameters { get; set; }
        public string IpAddress { get; set; }
        public int CreatorId { get; set; }
    }
}
