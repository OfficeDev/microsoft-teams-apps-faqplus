// <copyright file="ConfigurationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// ConfigurationProvider which will help in fetching and storing information in storage table.
    /// </summary>
    public class ConfigurationProvider : IConfigurationProvider
    {
        private const string PartitionKey = "ConfigurationInfo";

        private readonly Lazy<Task> initializeTask;
        private CloudTable configurationCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by DI</param>
        public ConfigurationProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(connectionString));
        }

        /// <inheritdoc/>
        public async Task<bool> SaveOrUpdateEntityAsync(string updatedData, string entityType)
        {
            try
            {
                var entity = new ConfigurationEntity()
                {
                    PartitionKey = PartitionKey,
                    RowKey = entityType,
                    Data = updatedData,
                };
                var result = await this.StoreOrUpdateEntityAsync(entity);
                return result.HttpStatusCode == (int)HttpStatusCode.NoContent;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetSavedEntityDetailAsync(string entityType)
        {
            try
            {
                await this.EnsureInitializedAsync();

                var searchOperation = TableOperation.Retrieve<ConfigurationEntity>(PartitionKey, entityType);
                TableResult searchResult = await this.configurationCloudTable.ExecuteAsync(searchOperation);
                var result = (ConfigurationEntity)searchResult.Result;

                return result?.Data ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Store or update configuration entity in table storage
        /// </summary>
        /// <param name="entity">entity.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateEntityAsync(ConfigurationEntity entity)
        {
            await this.EnsureInitializedAsync();

            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);

            return await this.configurationCloudTable.ExecuteAsync(addOrUpdateOperation);
        }

        /// <summary>
        /// Create teams table if it doesnt exists
        /// </summary>
        /// <param name="connectionString">storage account connection string</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.configurationCloudTable = cloudTableClient.GetTableReference(StorageInfo.ConfigurationTableName);
            await this.configurationCloudTable.CreateIfNotExistsAsync();
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in creating table
        /// </summary>
        /// <returns>Task</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value;
        }
    }
}
