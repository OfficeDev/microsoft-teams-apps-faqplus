// <copyright file="ExpertProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Exceptions;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Ticket provider helps in fetching and storing information in storage table.
    /// </summary>
    public class ExpertProvider : IExpertProvider
    {
        private const string PartitionKey = "ExpertInfo";
        private readonly Lazy<Task> initializeTask;
        private CloudTable expertCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpertProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public ExpertProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeTableStorageAsync(connectionString));
        }

        /// <summary>
        /// Store or update ticket entity in table storage.
        /// </summary>
        /// <param name="expert">Represents expert entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        public Task UpserExpertAsync(ExpertEntity expert)
        {
            expert.PartitionKey = PartitionKey;
            expert.RowKey = expert.ID;

            return this.StoreOrUpdateExpertEntityAsync(expert);
        }

        /// <summary>
        /// Store or update ticket entity in table storage.
        /// </summary>
        /// <param name="experts">Represents expert entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        public Task UpserExpertsAsync(List<ExpertEntity> experts)
        {
            return this.StoreOrUpdateExpertEntityAsync(experts);
        }

        /// <summary>
        /// Get already saved entity detail from storage table.
        /// </summary>
        /// <param name="expertId">expert channelaccount id.</param>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        public async Task<ExpertEntity> GetExpertAsync(string expertId)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false); // When there is no ticket created by end user and messaging extension is open by SME, table initialization is required before creating search index or datasource or indexer.
            if (string.IsNullOrEmpty(expertId))
            {
                return null;
            }

            var searchOperation = TableOperation.Retrieve<TicketEntity>(PartitionKey, expertId);
            var searchResult = await this.expertCloudTable.ExecuteAsync(searchOperation).ConfigureAwait(false);

            return (ExpertEntity)searchResult.Result;
        }

        /// <summary>
        /// Get all already saved entity detail from storage table.
        /// </summary>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        public async Task<List<ExpertEntity>> GetExpertsAsync()
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false); // When there is no expert info, table initialization is required before creating search index or datasource or indexer.

            TableContinuationToken token = null;
            var entities = new List<ExpertEntity>();
            do
            {
                var queryResult = await this.expertCloudTable.ExecuteQuerySegmentedAsync(new TableQuery<ExpertEntity>(), token).ConfigureAwait(false);
                entities.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            }
            while (token != null);

            return entities;
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in creating table.
        /// </summary>
        /// <returns>Represent a task with initialized connection data.</returns>
        private async Task EnsureInitializedAsync()
        {
            await this.initializeTask.Value.ConfigureAwait(false);
        }

        /// <summary>
        /// Create expert table if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeTableStorageAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.expertCloudTable = cloudTableClient.GetTableReference(Constants.ExpertTableName);

            await this.expertCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Store or update ticket entity in table storage.
        /// </summary>
        /// <param name="expert">Represents expert entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateExpertEntityAsync(ExpertEntity expert)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(expert);
            return await this.expertCloudTable.ExecuteAsync(addOrUpdateOperation).ConfigureAwait(false);
        }

        /// <summary>
        /// Store or update ticket entity in table storage.
        /// </summary>
        /// <param name="experts">Represents experts entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<IList<TableResult>> StoreOrUpdateExpertEntityAsync(List<ExpertEntity> experts)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableBatchOperation batch = new TableBatchOperation();
            foreach (var expert in experts)
            {
                expert.PartitionKey = PartitionKey;
                expert.RowKey = expert.ID;
                TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(expert);
                batch.Add(addOrUpdateOperation);
            }

            return await this.expertCloudTable.ExecuteBatchAsync(batch).ConfigureAwait(false);
        }
    }
}
