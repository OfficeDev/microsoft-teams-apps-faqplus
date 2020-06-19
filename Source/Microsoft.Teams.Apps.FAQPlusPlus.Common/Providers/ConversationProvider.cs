﻿namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    ///  Conversation provider helps in fetching and storing information in storage table.
    /// </summary>
    public class ConversationProvider : IConversationProvider
    {
        private const string PartitionKey = "ConversationInfo";
        private readonly Lazy<Task> initializeTask;
        private CloudTable conversationCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public ConversationProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeTableStorageAsync(connectionString));
        }

        /// <summary>
        /// Store or update Conversation entity in table storage.
        /// </summary>
        /// <param name="conversation">Represents Conversation entity used for storage and retrieval.</param>
        /// <returns>that represents Conversation entity is saved or updated.</returns>
        public Task UpsertConversationAsync(ConversationEntity conversation)
        {
            conversation.PartitionKey = PartitionKey;
            conversation.RowKey = conversation.ConversationId;
            return this.StoreOrUpdatConversationEntityAsync(conversation);
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
        /// Create Conversation table if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeTableStorageAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.conversationCloudTable = cloudTableClient.GetTableReference(Constants.ConversationTableName);

            await this.conversationCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Store or update Conversation entity in table storage.
        /// </summary>
        /// <param name="entity">Represents Conversation entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdatConversationEntityAsync(ConversationEntity entity)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            return await this.conversationCloudTable.ExecuteAsync(addOrUpdateOperation).ConfigureAwait(false);
        }
    }
}