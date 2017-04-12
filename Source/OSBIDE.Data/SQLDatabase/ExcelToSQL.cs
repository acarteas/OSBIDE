using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using OfficeOpenXml;

namespace OSBIDE.Data.SQLDatabase
{
    public static class ExcelToSQL
    {
        private const int BATCH_SIZE = 500;
        public static void Insert(string filePath, string fileExtension, string sqlTemplate, string paramTemplate, string[] paramList, int[] digitalColumns)
        {
            using (var xlPackage = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = xlPackage.Workbook.Worksheets[1];

                var query = new StringBuilder(sqlTemplate);
                var batches = worksheet.Dimension.End.Row / BATCH_SIZE;
                for (var b = 0; b < batches + 1; b++)
                {
                    var firstRow = b == 0 ? 2 : b * BATCH_SIZE;
                    var lastRow = (b + 1) * BATCH_SIZE > worksheet.Dimension.End.Row ? worksheet.Dimension.End.Row + 1 : (b + 1) * BATCH_SIZE;
                    for (var i = firstRow; i < lastRow; i++)
                    {
                        // compose a row
                        query.Append("(");

                        for (var j = 1; j < worksheet.Dimension.End.Column + 1; j++)
                        {
                            // compose a column
                            var column = worksheet.Cells[i, j].Value;

                            if (column == null)
                            {
                                query.Append("Null,");
                            }
                            else if (digitalColumns.Any(c => c == j))
                            {
                                try
                                {
                                    query.AppendFormat("{0},", Convert.ToInt32(column));
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(string.Format("{0} at row {1} column {2}.", ex.Message.TrimEnd('.'), i, j));
                                }
                            }
                            else
                            {
                                query.AppendFormat("'{0}',", column.ToString().Replace("'", "''"));
                            }
                        }
                        query.AppendFormat(paramTemplate, paramList);

                        query.Append("),");
                    }
                }

                DynamicSQLExecutor.Execute(query.ToString().TrimEnd(','));
            }
        }
        public static void Upsert(string filePath, string fileExtension, string sqlTemplate, Dictionary<string, string> keyVals, int[] digitalColumns)
        {
            using (var xlPackage = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = xlPackage.Workbook.Worksheets[1];

                var query = new StringBuilder();
                var batches = worksheet.Dimension.End.Row / BATCH_SIZE;
                for (var b = 0; b < batches + 1; b++)
                {
                    var firstRow = b == 0 ? 2 : b * BATCH_SIZE;
                    var lastRow = (b + 1) * BATCH_SIZE > worksheet.Dimension.End.Row ? worksheet.Dimension.End.Row + 1 : (b + 1) * BATCH_SIZE;
                    for (var i = firstRow; i < lastRow; i++)
                    {
                        // compose a row
                        var sqlRow = sqlTemplate;

                        for (var j = 1; j < worksheet.Dimension.End.Column + 1; j++)
                        {
                            // compose a column
                            var column = worksheet.Cells[i, j].Value;

                            if (column == null)
                            {
                                sqlRow = sqlRow.Replace(string.Format("C_{0}", j), "Null");
                            }
                            else if (digitalColumns.Any(c => c == j))
                            {
                                try
                                {
                                    sqlRow = sqlRow.Replace(string.Format("C_{0}", j), Convert.ToInt32(column).ToString());
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(string.Format("{0} at row {1} column {2}.", ex.Message.TrimEnd('.'), i, j));
                                }
                            }
                            else
                            {
                                sqlRow = sqlRow.Replace(string.Format("C_{0}", j), column.ToString().Replace("'", "''"));
                            }
                        }

                        foreach (var key in keyVals.Keys)
                        {
                            sqlRow = sqlRow.Replace(key, keyVals[key]);
                        }

                        query.Append(sqlRow.Replace("NEW_LINE", Environment.NewLine));
                    }
                }

                DynamicSQLExecutor.Execute(query.ToString());
            }
        }
    }
}
