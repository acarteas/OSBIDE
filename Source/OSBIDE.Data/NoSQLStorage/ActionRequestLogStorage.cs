using System;
using System.Collections.Generic;
using System.Configuration;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;

using OSBIDE.Data.SQLDatabase.Edmx;
using OSBIDE.Library.Models;

namespace OSBIDE.Data.NoSQLStorage
{
    internal class ActionRequestLogStorage : IDisposable
    {
        public void Create()
        {
            GetTableReference().CreateIfNotExists();
        }

        public void Delete()
        {
            GetTableReference().DeleteIfExists();
        }

        /// <summary>
        /// get all url request history for the whole school
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public IEnumerable<ActionRequestLogEntry> Select(string partitionKey)
        {
            var query = new TableQuery<ActionRequestLogEntry>()
                            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            return GetTableReference().ExecuteQuery(query);
        }

        /// <summary>
        /// get all url request history for the student
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="partitionKey"></param>
        /// <returns></returns>
        public IEnumerable<ActionRequestLogEntry> Select(string tableName, string partitionKey)
        {
            var query = new TableQuery<ActionRequestLogEntry>()
                            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            return GetTableReference(tableName).ExecuteQuery(query);
        }

        /// <summary>
        /// get one url request history for the school student
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public ActionRequestLogEntry SelectOne(string partitionKey, string rowKey)
        {
            var result = GetTableReference().Execute(TableOperation.Retrieve<ActionRequestLogEntry>(partitionKey, rowKey));

            return result == null ? null : (ActionRequestLogEntry) result.Result;
        }

        /// <summary>
        /// query a portion of the urls
        /// </summary>
        /// <param name="propertyName"></param>
        public void SelectProperty(string propertyName)
        {
            var projectionQuery = new TableQuery<DynamicTableEntity>().Select(new string[] { propertyName });

            EntityResolver<string> resolver = (pk, rk, ts, props, etag) => props.ContainsKey(propertyName) ? props[propertyName].StringValue : null;

            GetTableReference().ExecuteQuery(projectionQuery, resolver, null, null);
        }

        /// <summary>
        /// insert an entity regardless
        /// </summary>
        /// <param name="entity"></param>
        public void Insert(ActionRequestLogEntry entity)
        {
            try
            {
                GetTableReference().Execute(TableOperation.Insert(entity));
            }
            catch (Exception ex)
            {
                LogErrorMessage(entity, ex);
            }
        }

        /// <summary>
        /// batch insert entities
        /// </summary>
        /// <param name="entities"></param>
        public void Insert(IEnumerable<ActionRequestLogEntry> entities)
        {
            var batchOperation = new TableBatchOperation();

            foreach (var entity in entities)
            {
                batchOperation.Insert(entity);
            }

            GetTableReference().ExecuteBatch(batchOperation);
        }

        /// <summary>
        /// update an entity if it exists
        /// </summary>
        /// <param name="entity"></param>
        public void Update(ActionRequestLogEntry entity)
        {
            var updateEntity = this.SelectOne(entity.PartitionKey, entity.RowKey);
            if (updateEntity == null) return;

            // not allow to change the original creator id
            updateEntity.AccessDate = entity.AccessDate;
            updateEntity.ActionParameters = entity.ActionParameters;
            updateEntity.IpAddress = entity.IpAddress;
            GetTableReference().Execute(TableOperation.Replace(updateEntity));
        }

        /// <summary>
        /// update an entity if it exists, otherwise insert a new record
        /// </summary>
        /// <param name="entity"></param>
        public void Upsert(ActionRequestLogEntry entity)
        {
            var updateEntity = this.SelectOne(entity.PartitionKey, entity.RowKey);
            if (updateEntity == null)
            {
                this.Insert(entity);
            }
            else
            {
                this.Update(entity);
            }
        }

        /// <summary>
        /// delete an entity if it exists
        /// </summary>
        /// <param name="entity"></param>
        public void Delete(ActionRequestLogEntry entity)
        {
            var deleteEntity = this.SelectOne(entity.PartitionKey, entity.RowKey);
            if (deleteEntity != null)
            {
                GetTableReference().Execute(TableOperation.Delete(deleteEntity));
            }
        }

        public static CloudTable GetTableReference()
        {
            return GetTableReference(GetTableName(DateTime.Today));
        }

        public static CloudTable GetTableReference(string tableName)
        {
            try
            {
                var storageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings["StorageConnectionString"]);
                var tableClient = storageAccount.CreateCloudTableClient();
                var table = tableClient.GetTableReference(tableName);
                if (table.CreateIfNotExists())
                {
                    // log a record in PassiveSocialEventProcessLog table
                    using (var context = new OsbideProcs())
                    {
                        context.InsertPassiveSocialEventProcessLog(table.Name);
                    }
                }

                return table;
            }
            catch (Exception ex)
            {
                // need to do this due to the unstability of Azure
                var msg = ex.Message;

                throw;
            }
        }

        public static string GetTableName(DateTime dateTime)
        {
            return string.Format("ActionRequestLogEntity{0}{1}", dateTime.Year, dateTime.Month);
        }

        public void LogErrorMessage(ActionRequestLogEntry entity, Exception ex)
        {
            var msg = string.Format("message: {0}, source: {1}, stack trace: {2}, TargetSite: {3}", ex.Message, ex.Source, ex.StackTrace, ex.TargetSite);

            if (ex.Data != null && ex.Data.Keys != null)
            {
                foreach (var key in ex.Data.Keys)
                {
                    msg = string.Format("{0}, {1}|{2},", msg, key, ex.Data[key]);
                }
            }
                using (var context= OsbideContext.DefaultWebConnection)
                {
                    context.LocalErrorLogs.Add(new LocalErrorLog
                    {
                        SenderId = Convert.ToInt32(entity.RowKey.Split('_')[0]),
                        LogDate = DateTime.Now,
                        Content = msg
                    });
                    context.SaveChanges();
                }
        }

        #region IDispose

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
