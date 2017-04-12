
namespace OSBIDE.Data.DomainObjects
{
    public class TrendAndNotification
    {
        public int? UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? EventLogId { get; set; }
        public bool? Viewed { get; set; }
        public int? HashtagId { get; set; }
        public string Hashtag { get; set; }
        public int? HashtagCounts { get; set; }
    }
}
