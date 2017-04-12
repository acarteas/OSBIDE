using System;
using System.Linq;
using System.Web.Mvc;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase;
using OSBIDE.Library.Models;
using OSBIDE.Web.Helpers;
using OSBIDE.Web.Models.Analytics;
using OSBIDE.Web.Models.Attributes;

namespace OSBIDE.Web.Controllers
{
    [AllowAccess(SystemRole.Instructor, SystemRole.Admin)]
    public class CalendarVisualizationController : ControllerBase
    {
        public ActionResult Index()
        {
            var analytics = Analytics.FromSession();
            if (analytics.CalendarSettings == null)
            {
                analytics.CalendarSettings = new CalendarSettings();
            }

            if (analytics.Criteria != null)
            {
                if (analytics.Criteria.DateFrom.HasValue)
                {
                    analytics.CalendarSettings.Year = analytics.Criteria.DateFrom.Value.Year;
                    analytics.CalendarSettings.Month = analytics.Criteria.DateFrom.Value.Month;
                }
                analytics.CalendarSettings.CourseId = analytics.Criteria.CourseId;
            }

            return View("~/Views/Analytics/CalendarVisualization.cshtml", analytics.CalendarSettings);
        }

        public ActionResult GetMeasures(AggregateFunction a, int? c, int o, string m)
        {
            var analytics = Analytics.FromSession();

            var startDate = new DateTime(analytics.CalendarSettings.Year, analytics.CalendarSettings.Month, 1).AddMonths(o);

            if (c.HasValue) analytics.CalendarSettings.CourseId = c.Value;
            analytics.CalendarSettings.AggregateFunctionId = a;
            analytics.CalendarSettings.SelectedMeasures = m;
            analytics.CalendarSettings.DailyAggregations = CalendarDataProc.GetDailyAggregates
                                                                    (
                                                                        startDate,
                                                                        startDate.AddMonths(2),
                                                                        analytics.ProcedureData.Where(x => x.IsSelected).Select(x => x.Id).ToList(),
                                                                        analytics.CalendarSettings.CourseId,
                                                                        analytics.CalendarSettings.SelectedMeasures,
                                                                        analytics.CalendarSettings.AggregateFunctionId == AggregateFunction.Avg
                                                                    );

            analytics.CalendarSettings.Year = startDate.Year;
            analytics.CalendarSettings.Month = startDate.Month;

            return Json(analytics.CalendarSettings.ToJson(), JsonRequestBehavior.AllowGet);
        }
    }
}
