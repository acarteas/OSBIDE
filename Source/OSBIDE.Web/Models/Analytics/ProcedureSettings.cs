using System.Collections.Generic;

using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Web.Models.Analytics
{
    public class ProcedureSettings
    {
        public static List<ProcedureTypeLookup> ProcedureTypes
        {
            get
            {
                return new List<ProcedureTypeLookup>
                {
                    new ProcedureTypeLookup{ProcedureTypeId=(int)ProcedureType.ErrorQuotient, DisplayText="Error Quotient"},
                };
            }
        }

        public ProcedureType SelectedProcedureType { get; set; }
        public dynamic ProcedureParams { get; set; }
    }
}