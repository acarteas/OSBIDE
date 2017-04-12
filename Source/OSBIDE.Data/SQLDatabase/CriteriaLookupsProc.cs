using System;
using System.Linq;
using System.Collections.Generic;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class CriteriaLookupsProc
    {
        public static List<AgeLookup> GetAges()
        {
            using (var context = new OsbideProcs())
            {
                var ages = (from a in context.GetAgeLookup()
                            select new AgeLookup { Age = a.Age.Value, DisplayText = a.Age.Value.ToString() }).ToList();

                ages.Insert(0, new AgeLookup { Age = -1, DisplayText = "Any" });

                return ages;
            }
        }

        public static List<CourseLookup> GetCourses()
        {
            using (var context = new OsbideProcs())
            {
                var courses = (from c in context.GetCourseLookup()
                               select new CourseLookup { CourseId = c.CourseId, DisplayText = c.CourseName }).ToList();

                courses.Insert(0, new CourseLookup { CourseId = -1, DisplayText = "Any" });

                return courses;
            }
        }
        public static List<string> GetDeliverables(int courseId, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            using (var context = new OsbideProcs())
            {
                if (!dateFrom.HasValue) dateFrom = new DateTime(2010, 1, 1);
                if (!dateTo.HasValue) dateTo = DateTime.Today.AddDays(1);
                return (from d in context.GetDeliverableLookup(courseId, dateFrom, dateTo) select d).ToList();
            }
        }
        public static List<GenderLookup> GetGenders()
        {
            using (var context = new OsbideProcs())
            {
                return new List<GenderLookup>
                {
                    new GenderLookup{GenderId=(int)OSBIDE.Library.Models.Gender.Unknown, DisplayText="Any"},
                    new GenderLookup{GenderId=(int)OSBIDE.Library.Models.Gender.Female, DisplayText="Female"},
                    new GenderLookup{GenderId=(int)OSBIDE.Library.Models.Gender.Male, DisplayText="Male"},
                };
            }
        }
    }
}
