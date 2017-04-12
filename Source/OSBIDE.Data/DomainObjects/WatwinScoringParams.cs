using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OSBIDE.Data.DomainObjects
{
    public class WatwinScoringParams
    {
        public int? ElineRange { get; set; }
        public decimal? SameErrorPenalty { get; set; }
        public decimal? SameTypePenalty { get; set; }
        public decimal? SameLinePenalty { get; set; }
        public decimal? FastSolvePenalty { get; set; }
        public decimal? MedSolvePenalty { get; set; }
        public decimal? SlowSolvePenalty { get; set; }
        public int ErrorsConsidered { get; set; }

    }
}