// <copyright file="PublishFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// Azure Function to publish QnA Maker knowledge base.
    /// </summary>
    public class PublishFunction
    {
        private readonly IQnaServiceProvider qnaServiceProvider;
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly ISearchServiceDataProvider searchServiceDataProvider;
        private readonly IKnowledgeBaseSearchService knowledgeBaseSearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishFunction"/> class.
        /// </summary>
        /// <param name="qnaServiceProvider">Qna service provider.</param>
        /// <param name="configurationProvider">Configuration service provider.</param>
        /// <param name="searchServiceDataProvider">Search service data provider.</param>
        /// <param name="knowledgeBaseSearchService">Knowledgebase search service.</param>
        public PublishFunction(IQnaServiceProvider qnaServiceProvider, IConfigurationDataProvider configurationProvider, ISearchServiceDataProvider searchServiceDataProvider, IKnowledgeBaseSearchService knowledgeBaseSearchService)
        {
            this.qnaServiceProvider = qnaServiceProvider;
            this.configurationProvider = configurationProvider;
            this.searchServiceDataProvider = searchServiceDataProvider;
            this.knowledgeBaseSearchService = knowledgeBaseSearchService;
        }

        /// <summary>
        /// Function to get and publish QnA Maker knowledge base.
        /// </summary>
        /// <param name="myTimer">Duration of publish operations.</param>
        /// <param name="log">Log.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [FunctionName("PublishFunction")]
        public async Task Run([TimerTrigger("0 */15 * * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
                var knowledgeBaseId = await this.configurationProvider.GetSavedEntityDetailAsync(Constants.KnowledgeBaseEntityId).ConfigureAwait(false);
                bool toBePublished = await this.qnaServiceProvider.GetPublishStatusAsync(knowledgeBaseId).ConfigureAwait(false);
                log.LogInformation("To be published - " + toBePublished);
                log.LogInformation("knowledge base id - " + knowledgeBaseId);

                if (toBePublished)
                {
                    log.LogInformation("Publishing knowledge base");
                    await this.qnaServiceProvider.PublishKnowledgebaseAsync(knowledgeBaseId).ConfigureAwait(false);
                }

                log.LogInformation("Setup azure search data");
                await this.searchServiceDataProvider.SetupAzureSearchDataAsync(knowledgeBaseId).ConfigureAwait(false);

                log.LogInformation("Update azure search service");
                await this.knowledgeBaseSearchService.InitializeSearchServiceDependencyAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Exception occured while publishing knowledge base in QnA Maker.", SeverityLevel.Error);
                throw;
            }
        }
    }
}
