using System;
//using OSBIDE.Library.Models;

namespace OSBIDE.Data.DomainObjects
{
    public class ActionRequestLog
    {
        public int SchoolId { get; set; }
        public int CreatorId { get; set; }
        //public OsbideUser Creator { get; set; }
        public DateTime AccessDate { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string ActionParameters { get; set; }
        public string IpAddress { get; set; }
    }
}
