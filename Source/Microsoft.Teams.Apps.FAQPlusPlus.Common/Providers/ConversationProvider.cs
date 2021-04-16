// <copyright file="ConversationProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
            conversation.RowKey = conversation.ConversationID;
            return this.StoreOrUpdatConversationEntityAsync(conversation);
        }

        /// <summary>
        /// Get already saved entity detail from storage table.
        /// </summary>
        /// <param name="conversationID">conversation id received from bot based on which appropriate row data will be fetched.</param>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        public async Task<ConversationEntity> GetConversationAsync(string conversationID)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false); // When there is no ticket created by end user and messaging extension is open by SME, table initialization is required before creating search index or datasource or indexer.
            if (string.IsNullOrEmpty(conversationID))
            {
                return null;
            }

            var searchOperation = TableOperation.Retrieve<ConversationEntity>(PartitionKey, conversationID);
            var searchResult = await this.conversationCloudTable.ExecuteAsync(searchOperation).ConfigureAwait(false);

            return (ConversationEntity)searchResult.Result;
        }

        /// <summary>
        /// get recently asked questions with answers.
        /// </summary>
        /// <param name="days">recent n days.</param>
        /// <returns>list of conversation entity.</returns>
        public async Task<List<ConversationEntity>> GetRecentAskedQnAListAsync(int days)
        {
            List<ConversationEntity> activities = new List<ConversationEntity>();
            string filterTime = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, DateTimeOffset.Now.AddDays(-days).Date);
            string filterAnswer = TableQuery.GenerateFilterCondition("Answer", QueryComparisons.NotEqual, "null");
            string filterProject = TableQuery.GenerateFilterCondition("Project", QueryComparisons.NotEqual, "common");

            string finalFilter = TableQuery.CombineFilters(TableQuery.CombineFilters(filterTime, TableOperators.And, filterAnswer), TableOperators.And, filterProject);

            TableContinuationToken continuationToken = null;

            await this.EnsureInitializedAsync().ConfigureAwait(false);
            do
            {
                var result = await this.conversationCloudTable.ExecuteQuerySegmentedAsync(new TableQuery<ConversationEntity>().Where(finalFilter), continuationToken);
                continuationToken = result.ContinuationToken;
                int index = 0;
                if (result.Results != null)
                {
                    foreach (ConversationEntity entity in result.Results)
                    {
                        activities.Add(entity);
                        index++;
                        if (index == 500)
                        {
                            break;
                        }
                    }
                }
            }
            while (continuationToken != null);

            return activities;
        }

        /// <summary>
        /// Get conversation Id.
        /// </summary>
        /// <param name="userPrincipalName">user email.</param>
        /// <param name="expiryMinute">session expriy in minutes.</param>
        /// <returns>sessionId of current conversation.</returns>
        public async Task<string> GetSessionIdAsync(string userPrincipalName, int expiryMinute)
        {
            List<ConversationEntity> activities = new List<ConversationEntity>();
            string filterTime = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, DateTimeOffset.UtcNow.AddMinutes(-expiryMinute));
            string filterUserPrinciple = TableQuery.GenerateFilterCondition("UserPrincipalName", QueryComparisons.Equal, userPrincipalName);
            string finalFilter = TableQuery.CombineFilters(filterTime, TableOperators.And, filterUserPrinciple);
            TableContinuationToken continuationToken = null;

            await this.EnsureInitializedAsync().ConfigureAwait(false);
            do
            {
                var result = await this.conversationCloudTable.ExecuteQuerySegmentedAsync(new TableQuery<ConversationEntity>().Where(finalFilter), continuationToken);
                continuationToken = result.ContinuationToken;
                int index = 0;
                if (result.Results != null)
                {
                    foreach (ConversationEntity entity in result.Results)
                    {
                        activities.Add(entity);
                        index++;
                        if (index == 500)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    return Guid.NewGuid().ToString();
                }
            }
            while (continuationToken != null);
            ConversationEntity con = activities.OrderByDescending(r => r.Timestamp).FirstOrDefault();
            return con?.SessionId == null ? Guid.NewGuid().ToString() : con.SessionId;
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
