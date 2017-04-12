using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Web.Models.Analytics
{
    public class ProcedureDataItem
    {
        public bool IsSelected { get; set; }
        public int Id { get; set; }
        public int InstitutionId { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int? Age { get; set; }
        public string Class { get; set; }
        public string Deliverable { get; set; }
        public decimal Grade { get; set; }
        public string Ethnicity { get; set; }
        public decimal Score { get; set; }
    }
}