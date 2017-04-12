using System.Collections.Generic;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase;

namespace OSBIDE.Web.Models.Analytics
{
    public static class CriteriaLookups
    {
        public static List<GenderLookup> Genders
        {
            get
            {
                return CriteriaLookupsProc.GetGenders();
            }
        }
        public static List<AgeLookup> AgeRange
        {
            get
            {
                return CriteriaLookupsProc.GetAges();
            }
        }
        public static List<CourseLookup> Courses
        {
            get
            {
                return CriteriaLookupsProc.GetCourses();
            }
        }
    }
}
