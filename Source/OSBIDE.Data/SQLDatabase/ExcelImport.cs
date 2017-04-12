using System;
using System.Collections.Generic;

namespace OSBIDE.Data.SQLDatabase
{
    public class ExcelImport
    {
        public static void UploadGrades(string fileLocation, string fileExtension, int courseId, string deliverable, int createdBy)
        {
            ExcelToSQL.Upsert(fileLocation,
                                fileExtension,
                                SQLTemplateGrades.Upsert,
                                new Dictionary<string, string>
                                {
                                    {"K1", courseId.ToString()},
                                    {"K2", deliverable},
                                    {"K3", DateTime.Now.ToString()},
                                    {"K4", createdBy.ToString()},
                                },
                                new int[] { 1, 2 });
        }
        public static void UploadSurveys(string fileLocation, string fileExtension, int courseId, int createdBy)
        {
            ExcelToSQL.Insert(fileLocation,
                                fileExtension,
                                SQLTemplateSurveys.Insert, "{0},{1},'{2}'",
                                new string[] { courseId.ToString(), createdBy.ToString(), DateTime.Now.ToString() },
                                new int[] { 1, 3, 7, 8 });
        }
    }
}