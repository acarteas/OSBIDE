using System;

namespace OSBIDE.Data.DomainObjects
{
    public class DocUtilDetails
    {
        public int LogId{get;set;}
        public int BuildId { get; set; }
        public int DocumentId { get; set; }
        public int UserId { get; set; }
        public DateTime EventDate{get;set;}
        public string SolutionName{get;set;}
        public string FileName{get;set;}
        public string Document{get;set;}
    }
}
