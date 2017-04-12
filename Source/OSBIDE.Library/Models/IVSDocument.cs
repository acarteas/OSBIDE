using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models
{
    public interface IVSDocument
    {
        int Id { get; set; }
        string FileName { get; set; }
        string Content { get; set; }
        List<CodeDocumentBreakPoint> BreakPoints { get; set; }
        List<CodeDocumentErrorListItem> ErrorItems { get; set; }
    }
}
