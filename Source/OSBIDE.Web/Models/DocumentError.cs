using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Web.Models
{
    public enum DocumentErrorSource { Build, Debug, Other, None };
    public class DocumentError
    {
        public string FileName { get; set; }
        public int LineNumber { get; set; }
        public DocumentErrorSource Source { get; set; }
        public string ErrorMessage { get; set; }

        public DocumentError()
        {
        }

        public DocumentError(DocumentError other)
        {
            FileName = other.FileName;
            LineNumber = other.LineNumber;
            Source = other.Source;
            ErrorMessage = other.ErrorMessage;
        }
    }

    public class DocumentErrorComparer : IEqualityComparer<DocumentError>
    {

        public bool Equals(DocumentError x, DocumentError y)
        {
            bool retVal = false;
            if (x.FileName.CompareTo(y.FileName) == 0)
            {
                if (x.LineNumber == y.LineNumber)
                {
                    if (x.Source == y.Source)
                    {
                        retVal = true;
                    }
                }
            }
            return retVal;
        }

        public int GetHashCode(DocumentError obj)
        {
            return obj.FileName.GetHashCode() + obj.LineNumber.GetHashCode() + obj.Source.GetHashCode();
        }
    }
}