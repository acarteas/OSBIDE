using System;
using System.Collections.Generic;

using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Web.Models.Analytics
{
    public class CalendarSettings
    {
        // bi-directional
        public int? CourseId { get; set; }
        public AggregateFunction AggregateFunctionId { get; set; }
        public string SelectedMeasures { get; set; }

        // to client
        public int Year { get; set; }
        public int Month { get; set; }

        // the data area of the calendar
        public DailyAggregations DailyAggregations { get; set; }
        public List<HourlyAggregations> HourlyAggregations { get; set; }

        // from client
        public int MonthOffset { get; set; }
    }
}