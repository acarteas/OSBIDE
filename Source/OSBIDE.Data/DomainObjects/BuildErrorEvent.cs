using System;
using System.Collections.Generic;

namespace OSBIDE.Data.DomainObjects
{
    /// <summary>
    /// The collated compilation event for error quotient procedure
    /// </summary>
    public class BuildErrorEvent
    {
        public int BuildId { get; set; }
        public int LogId { get; set; }
        public int UserId { get; set; }
        public DateTime EventDate { get; set; }
        public List<ErrorDocumentInfo> Documents { get; set; }
        public List<ErrorTypeDetails> ErrorTypes { get; set; }
        public List<string> ErrorMessages { get; set; }
    }

    public class ErrorDocumentInfo
    {
        public int DocumentId { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
        public string FileName { get; set; }
        public int NumberOfModified { get; set; }
        public List<int> ModifiedLines { get; set; }
        public ErrorDocumentInfo()
        {
            NumberOfModified = 0;
            ModifiedLines = new List<int>();
        }
    }
    public class ErrorTypeDetails
    {
        public int LogId { get; set; }
        public int ErrorTypeId { get; set; }
        public string ErrorType { get; set; }
        public long FixingTime { get; set; }
    }
}
