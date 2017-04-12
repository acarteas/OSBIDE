using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace OSBIDE.Library.Models.Triggers
{
    public abstract class ModelTrigger
    {
        protected abstract string TriggerString { get; }

        public bool CreateTrigger(DbContext db)
        {
            ObjectContext context = (db as IObjectContextAdapter).ObjectContext;
            var entityConnection = context.Connection as EntityConnection;
            SqlConnection dbConn = entityConnection.StoreConnection as SqlConnection;
            try
            {
                dbConn.Open();
            }
            catch (Exception)
            {
                dbConn.Close();
                return false;
            }

            string query = TriggerString;
            SqlCommand cmd = new SqlCommand(query, dbConn);
            try
            {
                object result = cmd.ExecuteNonQuery();
            }
            catch (Exception)
            {
                dbConn.Close();
                return false;
            }

            dbConn.Close();
            return true;
        }
    }
}
