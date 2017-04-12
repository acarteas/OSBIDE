namespace OSBIDE.Data.DomainObjects
{
    public class ErrorQuotientParams
    {
        public decimal? TouchedMultiplier { get; set; }
        public int? TlineRange { get; set; }
        public int? ElineRange { get; set; }
        public decimal? ElinePenalty { get; set; }
        public decimal? EtypeSamePenalty { get; set; }
        public decimal? EtypeDiffPenalty { get; set; }
    }
}
