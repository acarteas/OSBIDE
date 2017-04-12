using System;
using System.Collections.Generic;
using System.Linq;

using OSBIDE.Data.DomainObjects;
using OSBIDE.Data.SQLDatabase.Edmx;

namespace OSBIDE.Data.SQLDatabase
{
    public class StudentDemographicInfoProc
    {
        public static List<StudentDemographicInfo> Get(Criteria criteria)
        {
            using (var context = new OsbideProcs())
            {
                var minDate = new DateTime(2000, 1,1);
                return (from p in context.GetProcedureData(criteria.DateFrom < minDate ? minDate : criteria.DateFrom,
                            criteria.DateTo < minDate ? DateTime.Today : criteria.DateTo,
                            criteria.NameToken,
                            criteria.GenderId,
                            criteria.AgeFrom,
                            criteria.AgeTo,
                            criteria.CourseId,
                            criteria.Year,
                            criteria.GradeFrom,
                            criteria.GradeTo,
                            criteria.OverallGradeFrom,
                            criteria.OverallGradeTo)
                        select new StudentDemographicInfo
                        {
                            Name = p.Name,
                            Age = p.Age,
                            Class = p.Class_Class,
                            Gender = p.Gender_Gend,
                            Year = p.Year,
                            Quarter = p.Semester,
                            Ethnicity = p.Ethnicity_Ethn,
                            Id = p.Id,
                            InstitutionId = p.InstitutionId,
                            Grade = 100,
                            OverallGrade = 100,
                        }).ToList();
            }
        }
    }
}
