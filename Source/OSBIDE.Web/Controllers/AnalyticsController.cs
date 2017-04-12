using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase;
using OSBIDE.Data.SQLDatabase.DataAnalytics;
using OSBIDE.Library.Models;
using OSBIDE.Web.Helpers;
using OSBIDE.Web.Models.Analytics;
using OSBIDE.Web.Models.Attributes;

namespace OSBIDE.Web.Controllers
{
    [AllowAccess(SystemRole.Instructor, SystemRole.Admin)]
    public class AnalyticsController : ControllerBase
    {
        public ActionResult Criteria()
        {
            var analytics = Analytics.FromSession();
            if (analytics.Criteria == null)
            {
                analytics.Criteria = new Criteria();
            }
            return View("Criteria", analytics.Criteria);
        }

        public ActionResult GetCourseDeliverables(int courseId, DateTime? dateFrom, DateTime? dateTo)
        {
            return Json(CriteriaLookupsProc.GetDeliverables(courseId, dateFrom, dateTo), JsonRequestBehavior.AllowGet);
        }

        public ActionResult RunDocUtils()
        {
            return Json(DocUtil.Run(CurrentUser.Id), JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Refine(Criteria criteria)
        {
            var analytics = Analytics.FromSession();

            analytics.Criteria = criteria;
            analytics.ProcedureData = ProcedureDataProc.Get(criteria)
                                                       .Select(x => new ProcedureDataItem
                                                       {
                                                           IsSelected = true,
                                                           Id = x.Id,
                                                           InstitutionId = x.InstitutionId,
                                                           Name = x.Name,
                                                           Gender = ((Gender)x.Gender).ToString(),
                                                           Age = x.Age,
                                                           Class = x.Class,
                                                           Deliverable = x.Deliverable,
                                                           Grade = x.Grade,
                                                           Ethnicity = x.Ethnicity
                                                       })
                                                       .ToList();
            analytics.SelectedDataItems = null;

            return View("Refine", analytics.ProcedureData);
        }

        public ActionResult Refine()
        {
            return View("Refine", Analytics.FromSession().ProcedureData);
        }

        [HttpPost]
        public ActionResult Procedure(List<int> selectedDataItems)
        {
            var analytics = Analytics.FromSession();
            analytics.SelectedDataItems = selectedDataItems;

            if (analytics.ProcedureSettings == null)
            {
                analytics.ProcedureSettings = new ProcedureSettings
                                                    {
                                                        SelectedProcedureType = ProcedureType.ErrorQuotient,
                                                        ProcedureParams = new ErrorQuotientParams()
                                                    };
            }
            return View("Procedure", analytics.ProcedureSettings);
        }

        public ActionResult Procedure()
        {
            return View("Procedure", Analytics.FromSession().ProcedureSettings);
        }

        [HttpPost]
        [ActionName("ProcedureHandler")]
        [ActionSelector(Name = "ChangeProcedure")]
        public ActionResult ChangeProcedure(FormCollection form)
        {
            var analytics = Analytics.FromSession();
            analytics.ProcedureSettings.SelectedProcedureType = (ProcedureType)Enum.Parse(typeof(ProcedureType), form["SelectedProcedureType"]);

            if (analytics.ProcedureSettings.SelectedProcedureType == ProcedureType.WatwinScoring)
            {
                var procedureParams = analytics.ProcedureSettings.ProcedureParams as WatwinScoringParams;
                if (procedureParams == null)
                {
                    analytics.ProcedureSettings.ProcedureParams = new WatwinScoringParams();
                }
            }
            else
            {
                var procedureParams = analytics.ProcedureSettings.ProcedureParams as ErrorQuotientParams;
                if (procedureParams == null)
                {
                    analytics.ProcedureSettings.ProcedureParams = new ErrorQuotientParams();
                }
            }
            return View("Procedure", analytics.ProcedureSettings);
        }

        [HttpPost]
        [ActionName("ProcedureHandler")]
        [ActionSelector(Name = "ErrorQuotient")]
        public ActionResult CalcErrorQuotient(ErrorQuotientParams procedureParams)
        {
            var analytics = Analytics.FromSession();

            analytics.ProcedureSettings.ProcedureParams = procedureParams;
            analytics.ProcedureResults = new ProcedureResults
            {
                ViewType = ResultViewType.Tabular,
            };

            var resultlist = new List<ProcedureDataItem>();
            foreach (var u in ErrorQuotientAnalytics.GetResults(procedureParams, analytics.Criteria.DateFrom, analytics.Criteria.DateTo, analytics.SelectedDataItems))
            {
                var user = analytics.ProcedureData.Where(r => r.IsSelected && r.Id == u.UserId).First();
                user.Score = u.Score;
                resultlist.Add(user);
            }

            analytics.ProcedureResults.Results = resultlist;
            ViewBag.ScoreTitle = "EQ Score";

            return View("Results", analytics.ProcedureResults);
        }

        [HttpPost]
        [ActionName("ProcedureHandler")]
        [ActionSelector(Name = "WatwinScoring")]
        public ActionResult CalcWatwinScoring(WatwinScoringParams procedureParams)
        {
            var analytics = Analytics.FromSession();

            analytics.ProcedureSettings.ProcedureParams = procedureParams;
            analytics.ProcedureResults = new ProcedureResults
            {
                ViewType = ResultViewType.Tabular,
            };

            var resultlist = new List<ProcedureDataItem>();
            foreach (var u in WatwinScoringAnalytics.GetResults(procedureParams, analytics.Criteria.DateFrom, analytics.Criteria.DateTo, analytics.SelectedDataItems))
            {
                var user = analytics.ProcedureData.Where(r => r.IsSelected && r.Id == u.UserId).First();
                user.Score = u.Score;
                resultlist.Add(user);
            }

            analytics.ProcedureResults.Results = resultlist;
            ViewBag.ScoreTitle = "WS Score";

            return View("Results", analytics.ProcedureResults);
        }

        [HttpPost]
        public ActionResult Charts(ProcedureResults procedureResults)
        {
            var analytics = Analytics.FromSession();

            analytics.ProcedureResults.ViewType = procedureResults.ViewType;

            return View("Results", analytics.ProcedureResults);
        }

        public ActionResult GetScoreFor(CategoryColumn x)
        {
            var analytics = Analytics.FromSession();

            switch (x)
            {
                case CategoryColumn.Age:
                    return Json(((List<ProcedureDataItem>)analytics.ProcedureResults.Results)
                                                                   .Where(r => r.Age.HasValue)
                                                                   .GroupBy(r => r.Age.Value)
                                                                   .Select(r => new { x = r.Key, y = Math.Truncate(100 * r.Average(s => s.Score)) / 100 })
                                                                   .OrderBy(r => r.x)
                                                                   .ToArray(), JsonRequestBehavior.AllowGet);
                case CategoryColumn.Class:
                    return Json(((List<ProcedureDataItem>)analytics.ProcedureResults.Results)
                                                                   .Where(r => !string.IsNullOrWhiteSpace(r.Class))
                                                                   .GroupBy(r => r.Class)
                                                                   .Select(r => new { x = r.Key, y = Math.Truncate(100 * r.Average(s => s.Score)) / 100 })
                                                                   .OrderBy(r => r.x)
                                                                   .ToArray(), JsonRequestBehavior.AllowGet);
                case CategoryColumn.Ethnicity:
                    return Json(((List<ProcedureDataItem>)analytics.ProcedureResults.Results)
                                                                   .Where(r => !string.IsNullOrWhiteSpace(r.Ethnicity))
                                                                   .GroupBy(r => r.Ethnicity)
                                                                   .Select(r => new { x = r.Key, y = Math.Truncate(100 * r.Average(s => s.Score)) / 100 })
                                                                   .OrderBy(r => r.x)
                                                                   .ToArray(), JsonRequestBehavior.AllowGet);
                case CategoryColumn.Gender:
                    return Json(((List<ProcedureDataItem>)analytics.ProcedureResults.Results)
                                                                   .Where(r => !string.IsNullOrWhiteSpace(r.Gender))
                                                                   .GroupBy(r => r.Gender)
                                                                   .Select(r => new { x = r.Key, y = Math.Truncate(100 * r.Average(s => s.Score)) / 100 })
                                                                   .OrderBy(r => r.x)
                                                                   .ToArray(), JsonRequestBehavior.AllowGet);
                case CategoryColumn.InstitutionId:
                    return Json(((List<ProcedureDataItem>)analytics.ProcedureResults.Results)
                                                                   .GroupBy(r => r.InstitutionId)
                                                                   .Select(r => new { x = r.Key, y = Math.Truncate(100 * r.Average(s => s.Score)) / 100 })
                                                                   .OrderBy(r => r.x)
                                                                   .ToArray(), JsonRequestBehavior.AllowGet);
                default:
                    return Json(((List<ProcedureDataItem>)analytics.ProcedureResults.Results)
                                                                   .GroupBy(r => r.Name)
                                                                   .Select(r => new { x = r.Key, y = Math.Truncate(100 * r.Average(s => s.Score)) / 100 })
                                                                   .OrderBy(r => r.x)
                                                                   .ToArray(), JsonRequestBehavior.AllowGet);
            }
        }
    }
}