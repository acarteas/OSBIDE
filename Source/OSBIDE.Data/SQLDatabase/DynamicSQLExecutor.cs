using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;

using OSBIDE.Library;

namespace OSBIDE.Data.SQLDatabase
{
    public class DynamicSQLExecutor
    {
        private const int MAX_RETRIES = 5;
        private const int RETRY_INTERVAL = 1000;

        public static void Execute(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql)) return;

            using (var sqlConnection = new SqlConnection(StringConstants.WebConnectionString))
            {
                sqlConnection.Open();
                var sqlCommand = new SqlCommand(sql, sqlConnection);
                for (var retries = 0; retries < MAX_RETRIES; )
                {
                    try
                    {
                        sqlCommand.ExecuteNonQuery();
                        return;
                    }
                    catch (SqlException ex)
                    {
                        if (++retries == MAX_RETRIES)
                        {
                            throw new Exception(string.Format("Error updating database, please check schema. Error Code: {0}{1}", ex.ErrorCode, ex.InnerException == null ? string.Empty : string.Format(". {0}", ex.InnerException.Message)));
                        }
                        else
                        {
                            Thread.Sleep(RETRY_INTERVAL);
                        }
                    }
                }
            }
        }
    }
}
