
namespace OSBIDE.Data.DomainObjects
{
    public class StudentDemographicInfo
    {
        public int Id { get; set; }
        public int InstitutionId { get; set; }
        public string Name { get; set; }
        public int Gender { get; set; }
        public int? Age { get; set; }
        public string Class { get; set; }
        public string Deliverable { get; set; }
        public decimal Grade { get; set; }
        public string Ethnicity { get; set; }
    }
}
