namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    ///  Feedback provider helps in fetching and storing information in storage table.
    /// </summary>
    public class FeedbackProvider : IFeedbackProvider
    {
        private const string PartitionKey = "FeedbackInfo";
        private readonly Lazy<Task> initializeTask;
        private CloudTable feedbackCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public FeedbackProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeTableStorageAsync(connectionString));
        }

        /// <summary>
        /// Store or update feedback entity in table storage.
        /// </summary>
        /// <param name="feedback">Represents feedback entity used for storage and retrieval.</param>
        /// <returns>that represents feedback entity is saved or updated.</returns>
        public Task UpsertFeecbackAsync(FeedbackEntity feedback)
        {
            feedback.PartitionKey = PartitionKey;
            feedback.RowKey = feedback.FeedbackId;

            return this.StoreOrUpdateFeedbackEntityAsync(feedback);
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
        /// Create feedback table if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeTableStorageAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.feedbackCloudTable = cloudTableClient.GetTableReference(Constants.FeedbackTableName);

            await this.feedbackCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Store or update feedback entity in table storage.
        /// </summary>
        /// <param name="entity">Represents feedback entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateFeedbackEntityAsync(FeedbackEntity entity)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            return await this.feedbackCloudTable.ExecuteAsync(addOrUpdateOperation).ConfigureAwait(false);
        }
    }
}
