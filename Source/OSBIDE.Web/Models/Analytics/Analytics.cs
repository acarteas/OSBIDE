using System;
using System.Collections.Generic;
using System.Web;

using OSBIDE.Data.DomainObjects;

namespace OSBIDE.Web.Models.Analytics
{
    [Serializable]
    class Analytics
    {
        private const string AnalyticsSession = "ANALYTICS_SESSION_KEY";

        public Criteria Criteria { get; set; }
        public List<ProcedureDataItem> ProcedureData { get; set; }
        public List<int> SelectedDataItems { get; set; }
        public ProcedureSettings ProcedureSettings { get; set; }
        public ProcedureResults ProcedureResults { get; set; }
        public VisualizationParams VisualizationParams {get;set;}
        public CalendarSettings CalendarSettings { get; set; }

        public static Analytics FromSession()
        {
            if (HttpContext.Current.Session[AnalyticsSession] == null)
            {
                HttpContext.Current.Session[AnalyticsSession] = new Analytics();
            }

            return (Analytics)HttpContext.Current.Session[AnalyticsSession];
        }
    }
}