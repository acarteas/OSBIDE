using System;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace OSBIDE.Data.SQLDatabase
{
    public class ImportSurveys
    {
        private const int BATCH_SIZE = 500;
        public static void Upload(string fileLocation, string fileExtension, int surveyYear, string surveySemester, int createdBy)
        {
            var excelConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source="
                                      + fileLocation
                                      + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";

            if (fileExtension == ".xls")
            {
                excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source="
                                      + fileLocation
                                      + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
            }

            // get excel schema
            var excelConnection = new OleDbConnection(excelConnectionString);
            excelConnection.Open();
            DataTable dt = new DataTable();

            dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
            if (dt == null || dt.Rows.Count == 0) return;

            // get the single sheet contents
            var ds = new DataSet();
            using (var dataAdapter = new OleDbDataAdapter(string.Format("Select * from [{0}]", dt.Rows[0]["TABLE_NAME"].ToString()), excelConnection))
            {
                dataAdapter.Fill(ds);
            }

            // compose sql
            var now = DateTime.Now;
            var query = new StringBuilder(ImportSurveysSQL.Template);
            var batches = ds.Tables[0].Rows.Count / BATCH_SIZE;
            for (var b = 0; b < batches + 1; b++)
            {
                var upperBound = (b + 1) * BATCH_SIZE > ds.Tables[0].Rows.Count ? ds.Tables[0].Rows.Count : (b + 1) * BATCH_SIZE;
                for (var i = b * BATCH_SIZE; i < upperBound; i++)
                {
                    // compose a row
                    query.Append("(");
                    for (var j = 0; j < ds.Tables[0].Columns.Count; j++)
                    {
                        // compose a column
                        var column = ds.Tables[0].Rows[i][j];

                        if (column is DBNull)
                        {
                            query.Append("Null,");
                        }
                        else if (column is string || column is DateTime)
                        {
                            query.AppendFormat("'{0}',", column.ToString().Replace("'", "''"));
                        }
                        else
                        {
                            query.AppendFormat("{0},", Convert.ToInt32(column));
                        }
                    }
                    query.AppendFormat("{0},'{1}',{2},'{3}'),", surveyYear, surveySemester, createdBy, now.ToString());
                }
            }

            using (var sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["OsbideAdmin"].ConnectionString))
            {
                sqlConnection.Open();
                (new SqlCommand(query.ToString().TrimEnd(','), sqlConnection)).ExecuteNonQuery();
                sqlConnection.Close();
            }
        }
    }
}