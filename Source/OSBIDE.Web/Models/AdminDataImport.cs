using System.Web;

using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Web.Models
{
    public class AdminDataImport
    {
        public HttpPostedFileBase File { get; set; }
        public int CourseId { get; set; }
        public string Deliverable { get; set; }
        public string Schema { get; set; }
    }
}