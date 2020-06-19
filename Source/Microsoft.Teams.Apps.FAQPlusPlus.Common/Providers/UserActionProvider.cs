namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    ///  User action provider helps in fetching and storing information in storage table.
    /// </summary>
    public class UserActionProvider : IUserActionProvider
    {
        private const string PartitionKey = "UserAction";
        private readonly Lazy<Task> initializeTask;
        private CloudTable userActionCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserActionProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by dependency injection.</param>
        public UserActionProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeTableStorageAsync(connectionString));
        }

        /// <summary>
        /// Store or update user action entity in table storage.
        /// </summary>
        /// <param name="userAction">Represents user action entity used for storage and retrieval.</param>
        /// <returns>that represents user action entity is saved or updated.</returns>
        public Task UpsertUserActionAsync(UserActionEntity userAction)
        {
            userAction.PartitionKey = PartitionKey;
            userAction.RowKey = userAction.UserActionId;

            return this.StoreOrUpdateFeedbackEntityAsync(userAction);
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
        /// Create user action table if it doesn't exist.
        /// </summary>
        /// <param name="connectionString">storage account connection string.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeTableStorageAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.userActionCloudTable = cloudTableClient.GetTableReference(Constants.UserActionTableName);

            await this.userActionCloudTable.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Store or update user action entity in table storage.
        /// </summary>
        /// <param name="entity">Represents user action entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateFeedbackEntityAsync(UserActionEntity entity)
        {
            await this.EnsureInitializedAsync().ConfigureAwait(false);
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            return await this.userActionCloudTable.ExecuteAsync(addOrUpdateOperation).ConfigureAwait(false);
        }
    }
}
