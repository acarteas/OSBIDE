using System;
using System.Collections.Generic;
using System.Configuration;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Auth;
using OSBIDE.Library.Models;

namespace OSBIDE.Data.NoSQLStorage
{
    internal class ActionRequestLogTable : IDisposable
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
        public IEnumerable<ActionRequestLogEntity> Select(string partitionKey)
        {
            var query = new TableQuery<ActionRequestLogEntity>()
                            .Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey));

            return GetTableReference().ExecuteQuery(query);
        }

        /// <summary>
        /// get all url request history for the student
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public IEnumerable<ActionRequestLogEntity> Select(string partitionKey, string rowKey)
        {
            var query = new TableQuery<ActionRequestLogEntity>()
                            .Where(TableQuery.CombineFilters(
                                            TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                                            TableOperators.And,
                                            TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, rowKey)));

            return GetTableReference().ExecuteQuery(query);
        }

        /// <summary>
        /// get one url request history for the school student
        /// </summary>
        /// <param name="partitionKey"></param>
        /// <param name="rowKey"></param>
        /// <returns></returns>
        public ActionRequestLogEntity SelectOne(string partitionKey, string rowKey)
        {
            var result = GetTableReference().Execute(TableOperation.Retrieve<ActionRequestLogEntity>(partitionKey, rowKey));

            return result == null ? null : (ActionRequestLogEntity)result.Result;
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
        public void Insert(ActionRequestLogEntity entity)
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
        public void Insert(IEnumerable<ActionRequestLogEntity> entities)
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
        public void Update(ActionRequestLogEntity entity)
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
        public void Upsert(ActionRequestLogEntity entity)
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
        public void Delete(ActionRequestLogEntity entity)
        {
            var deleteEntity = this.SelectOne(entity.PartitionKey, entity.RowKey);
            if (deleteEntity != null)
            {
                GetTableReference().Execute(TableOperation.Delete(deleteEntity));
            }
        }

        public static CloudTable GetTableReference()
        {
            try
            {
                var configKey = ConfigurationManager.AppSettings["CloudTable"].Split('|');
                var accountName = configKey[0];
                var accountKey = configKey[1];
                var creds = new StorageCredentials(accountName, accountKey);
                var account = new CloudStorageAccount(creds, useHttps: true);
                var client = account.CreateCloudTableClient();
                var table = client.GetTableReference("ActionRequestLogEntity");
                table.CreateIfNotExists();

                return table;
            }
            catch (Exception ex)
            {
                // need to do this due to the unstability of Azure
                var msg = ex.Message;

                throw;
            }
        }
        public void LogErrorMessage(ActionRequestLogEntity entity, Exception ex)
        {
            var msg = string.Format("message: {0}, source: {1}, stack trace: {2}, TargetSite: {3}", ex.Message, ex.Source, ex.StackTrace, ex.TargetSite);

            if (ex.Data != null && ex.Data.Keys != null)
            {
                foreach (var key in ex.Data.Keys)
                {
                    msg = string.Format("{0}, {1}|{2},", msg, key, ex.Data[key]);
                }
            }
            using (var context = OsbideContext.DefaultWebConnection)
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
