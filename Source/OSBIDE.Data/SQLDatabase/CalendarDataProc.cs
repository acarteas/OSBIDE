using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class CalendarDataProc
    {
        public static DailyAggregations GetDailyAggregates(DateTime? startDate, DateTime? endDate, List<int> users, int? courseId, string selectedMeasures, bool isAvg)
        {
            using (var context = new OsbideProcs())
            {
                var selectedMeasureNames = string.IsNullOrWhiteSpace(selectedMeasures)
                                         ? string.Empty
                                         : string.Join(",", selectedMeasures.Split(',').Select(x => (MeasureType)Enum.Parse(typeof(MeasureType), x)).ToArray());
                var dailyAggregatesRaw = context.GetCalendarMeasuresByDay(startDate, endDate, users == null? string.Empty : string.Join(",", users), courseId, selectedMeasureNames, isAvg)
                                                .GroupBy(x=>x.Measure)
                                                .ToDictionary(x=>x.Key, x=>x.ToList());

                if (dailyAggregatesRaw.Count > 0)
                {
                    var measureDictionary = MeasureDefinitions.All.Values.SelectMany(x => x).ToList();

                    var activities = dailyAggregatesRaw.Values.SelectMany(x => x).ToList()
                                                       .Where(x => x.Value < 0)
                                                        // month starts from 0 to 11 in JavaScript!!!
                                                       .Select(x => new Activity { Day = x.EventDay.Value.Day, Month = x.EventDay.Value.Month - 1, Name = x.Measure })
                                                       .ToList();

                    var measures = new List<Measure>();
                    foreach (var key in dailyAggregatesRaw.Keys)
                    {
                        if (dailyAggregatesRaw[key].Any(x=>x.Value.HasValue && x.Value.Value > 0))
                        {
                            var ms = measureDictionary.Where(x => x.MeasureType.ToString() == key).Single();
                            measures.Add(
                            new Measure
                            {
                                Title = key.ToDisplayText(),
                                DataPointShape = ms.DataPointShape,
                                Color = ms.Color,
                                // month starts from 0 to 11 in JavaScript!!!
                                Aggregates = dailyAggregatesRaw[key].Select(x => new Aggregate { Day = x.EventDay.Value.Day, Month = x.EventDay.Value.Month - 1, Value = x.Value.Value }).ToList(),
                                FirstDataPointMonth = dailyAggregatesRaw[key].Min(x=>x.EventDay.Value).Month - 1,
                                FirstDataPointDay = dailyAggregatesRaw[key].Min(x => x.EventDay.Value).Day,
                                LastDataPointMonth = dailyAggregatesRaw[key].Max(x => x.EventDay.Value).Month - 1,
                                LastDataPointDay = dailyAggregatesRaw[key].Max(x => x.EventDay.Value).Day,
                                Avg = dailyAggregatesRaw[key].Average(x => x.Value.Value),
                                Max = dailyAggregatesRaw[key].Max(x => x.Value.Value),
                                Min = dailyAggregatesRaw[key].Min(x => x.Value.Value),
                            });
                        }
                    }

                    return new DailyAggregations { Activities = activities, Measures = measures };
                }

                return null;
            }
        }
    }
}
