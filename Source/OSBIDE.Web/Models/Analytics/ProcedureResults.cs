using OSBIDE.Data.DomainObjects;
using System.Collections.Generic;

namespace OSBIDE.Web.Models.Analytics
{
    public class ProcedureResults
    {
        public ResultViewType ViewType { get; set; }
        public dynamic Results { get; set; }
    }
}