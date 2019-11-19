// <copyright file="TicketsProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Exceptions;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// TicketProviders which will help in fetching and storing information in storage table.
    /// </summary>
    public class TicketsProvider : ITicketsProvider
    {
        private const string PartitionKey = "TicketInfo";

        private readonly Lazy<Task> initializeTask;
        private CloudTable ticketCloudTable;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketsProvider"/> class.
        /// </summary>
        /// <param name="connectionString">connection string of storage provided by DI</param>
        public TicketsProvider(string connectionString)
        {
            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(connectionString));
        }

        /// <summary>
        /// Store or update ticket entity in table storage
        /// </summary>
        /// <param name="ticket">ticketEntity.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        public Task SaveOrUpdateTicketAsync(TicketEntity ticket)
        {
            ticket.PartitionKey = PartitionKey;
            ticket.RowKey = ticket.TicketId;

            if (ticket.Status > (int)TicketState.MaxValue)
            {
                throw new TicketValidationException($"The ticket status ({ticket.Status}) is not valid.");
            }

            return this.StoreOrUpdateTicketEntityAsync(ticket);
        }

        /// <inheritdoc/>
        public async Task<TicketEntity> GetTicketAsync(string ticketId)
        {
            await this.EnsureInitializedAsync();

            var searchOperation = TableOperation.Retrieve<TicketEntity>(PartitionKey, ticketId);
            var searchResult = await this.ticketCloudTable.ExecuteAsync(searchOperation);

            return (TicketEntity)searchResult.Result;
        }

        /// <summary>
        /// Store or update ticket entity in table storage
        /// </summary>
        /// <param name="entity">entity.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        private async Task<TableResult> StoreOrUpdateTicketEntityAsync(TicketEntity entity)
        {
            await this.EnsureInitializedAsync();
            TableOperation addOrUpdateOperation = TableOperation.InsertOrReplace(entity);
            return await this.ticketCloudTable.ExecuteAsync(addOrUpdateOperation);
        }

        /// <summary>
        /// Create tickets table if it doesnt exists
        /// </summary>
        /// <param name="connectionString">storage account connection string</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation task which represents table is created if its not existing.</returns>
        private async Task InitializeAsync(string connectionString)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
            CloudTableClient cloudTableClient = storageAccount.CreateCloudTableClient();
            this.ticketCloudTable = cloudTableClient.GetTableReference(StorageInfo.TicketTableName);

            await this.ticketCloudTable.CreateIfNotExistsAsync();
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
