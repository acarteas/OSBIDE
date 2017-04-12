using System.Collections.Generic;

namespace OSBIDE.Data.DomainObjects
{
    public class HourlyAggregate
    {
        public int Hour { get; set; }
        public int Value { get; set; }
    }

    public class HourlyAggregations
    {
        public string Title { get;set; }
        public string ColorCode { get;set; }
        public List<HourlyAggregate> Aggregates { get; set; }
        public double Max { get; set; }
        public double Min { get; set; }
        public double Avg { get; set; }
    }
}