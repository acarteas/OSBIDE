
namespace OSBIDE.Data.DomainObjects
{
    public class ErrorFixTimeStats
    {
        public int ErrorTypeId { get; set; }
        public decimal Mean { get; set; }
        public decimal SD { get; set; }
        public decimal SDP { get; set; }
    }
}
