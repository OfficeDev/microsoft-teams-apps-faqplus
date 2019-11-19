// <copyright file="SearchService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.Azure.Search;
    using Microsoft.Azure.Search.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;

    /// <summary>
    /// SearchService which will help in creating index, indexer and datasource if it doesn't exists
    /// for indexing table which will be used for search by message extension.
    /// </summary>
    public class SearchService : ISearchService
    {
        private const string TicketsIndexName = "tickets-index";
        private const string TicketsIndexerName = "tickets-indexer";
        private const string TicketsDataSourceName = "tickets-storage";

        // Default to 25 results, same as page size of a messaging extension query
        private const int DefaultSearchResultCount = 25;

        private readonly Lazy<Task> initializeTask;
        private readonly TelemetryClient telemetryClient;
        private readonly SearchServiceClient searchServiceClient;
        private readonly SearchIndexClient searchIndexClient;
        private readonly int searchIndexingIntervalInMinutes;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchService"/> class.
        /// </summary>
        /// <param name="configuration">IConfiguration provided by DI</param>
        /// <param name="telemetryClient">TelemetryClient provided by DI</param>
        public SearchService(IConfiguration configuration, TelemetryClient telemetryClient)
        {
            this.telemetryClient = telemetryClient;

            this.searchServiceClient = new SearchServiceClient(
                configuration["SearchServiceName"],
                new SearchCredentials(configuration["SearchServiceAdminApiKey"]));
            this.searchIndexClient = new SearchIndexClient(
                configuration["SearchServiceName"],
                TicketsIndexName,
                new SearchCredentials(configuration["SearchServiceQueryApiKey"]));
            this.searchIndexingIntervalInMinutes = Convert.ToInt32(configuration["SearchIndexingIntervalInMinutes"]);

            this.initializeTask = new Lazy<Task>(() => this.InitializeAsync(configuration["StorageConnectionString"]));
        }

        /// <inheritdoc/>
        public async Task<IList<TicketEntity>> SearchTicketsAsync(TicketSearchScope searchScope, string searchQuery, int? count = null, int? skip = null)
        {
            await this.EnsureInitializedAsync();

            IList<TicketEntity> tickets = new List<TicketEntity>();

            SearchParameters searchParam = new SearchParameters();
            switch (searchScope)
            {
                case TicketSearchScope.RecentTickets:
                    searchParam.OrderBy = new[] { "Timestamp desc" };
                    break;

                case TicketSearchScope.OpenTickets:
                    searchParam.Filter = "Status eq " + (int)TicketState.Open + " and AssignedToName eq null";
                    searchParam.OrderBy = new[] { "Timestamp desc" };
                    break;

                case TicketSearchScope.AssignedTickets:
                    searchParam.Filter = "Status eq " + (int)TicketState.Open + " and AssignedToName ne null";
                    searchParam.OrderBy = new[] { "Timestamp desc" };
                    break;

                default:
                    break;
            }

            searchParam.Top = count ?? DefaultSearchResultCount;
            searchParam.Skip = skip ?? 0;
            searchParam.IncludeTotalResultCount = false;
            searchParam.Select = new[] { "Timestamp", "Title", "Status", "AssignedToName", "AssignedToObjectId", "DateCreated", "RequesterName", "RequesterUserPrincipalName", "Description", "RequesterGivenName", "SmeThreadConversationId", "DateAssigned", "DateClosed", "LastModifiedByName", "UserQuestion", "KnowledgeBaseAnswer" };

            var docs = await this.searchIndexClient.Documents.SearchAsync<TicketEntity>(searchQuery, searchParam);
            if (docs != null)
            {
                foreach (SearchResult<TicketEntity> doc in docs.Results)
                {
                    tickets.Add(doc.Document);
                }
            }

            return tickets;
        }

        /// <summary>
        /// Create index, indexer and data source it doesnt exists
        /// </summary>
        /// <param name="storageConnectionString">Connection string to the data store</param>
        /// <returns>Tracking task</returns>
        private async Task InitializeAsync(string storageConnectionString)
        {
            try
            {
                await this.CreateIndexAsync();
                await this.CreateDataSourceAsync(storageConnectionString);
                await this.CreateIndexerAsync();
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackTrace($"Failed to initialize Azure Search Service: {ex.Message}", ApplicationInsights.DataContracts.SeverityLevel.Error);
                this.telemetryClient.TrackException(ex);
                throw;
            }
        }

        /// <summary>
        /// Create index in Azure search service if it doesn't exists
        /// </summary>
        /// <returns><see cref="Task"/> that represents index is created if it is not created.</returns>
        private async Task CreateIndexAsync()
        {
            if (!this.searchServiceClient.Indexes.Exists(TicketsIndexName))
            {
                var tableIndex = new Index()
                {
                    Name = TicketsIndexName,
                    Fields = FieldBuilder.BuildForType<TicketEntity>()
                };
                await this.searchServiceClient.Indexes.CreateAsync(tableIndex);
            }
        }

        /// <summary>
        /// Add data source if it doesn't exists in Azure search service
        /// </summary>
        /// <param name="connectionString">connectionString.</param>
        /// <returns><see cref="Task"/> that represents data source is added to Azure search service.</returns>
        private async Task CreateDataSourceAsync(string connectionString)
        {
            if (!this.searchServiceClient.DataSources.Exists(TicketsDataSourceName))
            {
                var dataSource = DataSource.AzureTableStorage(
                                  name: TicketsDataSourceName,
                                  storageConnectionString: connectionString,
                                  tableName: StorageInfo.TicketTableName);

                await this.searchServiceClient.DataSources.CreateAsync(dataSource);
            }
        }

        /// <summary>
        /// Create indexer if it doesnt exists in Azure search service
        /// </summary>
        /// <returns><see cref="Task"/> that represents indexer is created if not available in Azure search service.</returns>
        private async Task CreateIndexerAsync()
        {
            if (!this.searchServiceClient.Indexers.Exists(TicketsIndexerName))
            {
                var indexer =
                new Indexer()
                {
                    Name = TicketsIndexerName,
                    DataSourceName = TicketsDataSourceName,
                    TargetIndexName = TicketsIndexName,
                    Schedule = new IndexingSchedule(TimeSpan.FromMinutes(this.searchIndexingIntervalInMinutes))
                };

                await this.searchServiceClient.Indexers.CreateAsync(indexer);
            }
        }

        /// <summary>
        /// Initialization of InitializeAsync method which will help in indexing
        /// </summary>
        /// <returns>Task</returns>
        private Task EnsureInitializedAsync()
        {
            return this.initializeTask.Value;
        }
    }
}