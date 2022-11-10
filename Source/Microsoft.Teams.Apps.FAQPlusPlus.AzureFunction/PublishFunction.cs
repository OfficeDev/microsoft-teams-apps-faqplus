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
    /// Azure Function to publish Question Answering knowledge base.
    /// </summary>
    public class PublishFunction
    {
        private readonly IQuestionAnswerServiceProvider questionAnswerServiceProvider;
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly ISearchServiceDataProvider searchServiceDataProvider;
        private readonly IKnowledgeBaseSearchService knowledgeBaseSearchService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishFunction"/> class.
        /// </summary>
        /// <param name="questionAnswerServiceProvider">Question Answering service provider.</param>
        /// <param name="configurationProvider">Configuration service provider.</param>
        /// <param name="searchServiceDataProvider">Search service data provider.</param>
        /// <param name="knowledgeBaseSearchService">Knowledgebase search service.</param>
        public PublishFunction(IQuestionAnswerServiceProvider questionAnswerServiceProvider, IConfigurationDataProvider configurationProvider, ISearchServiceDataProvider searchServiceDataProvider, IKnowledgeBaseSearchService knowledgeBaseSearchService)
        {
            this.questionAnswerServiceProvider = questionAnswerServiceProvider;
            this.configurationProvider = configurationProvider;
            this.searchServiceDataProvider = searchServiceDataProvider;
            this.knowledgeBaseSearchService = knowledgeBaseSearchService;
        }

        /// <summary>
        /// Function to get and publish Question Answering knowledge base.
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
                bool toBePublished = await this.questionAnswerServiceProvider.GetPublishStatusAsync().ConfigureAwait(false);
                log.LogInformation("To be published - " + toBePublished);
                log.LogInformation("knowledge base id - " + knowledgeBaseId);

                if (toBePublished)
                {
                    log.LogInformation("Publishing knowledge base");
                    await this.questionAnswerServiceProvider.PublishKnowledgebaseAsync().ConfigureAwait(false);
                    log.LogInformation("Successfully published the knowledge base" + knowledgeBaseId);
                }

                log.LogInformation("Setup azure search data");
                await this.searchServiceDataProvider.SetupAzureSearchDataAsync(knowledgeBaseId).ConfigureAwait(false);
                log.LogInformation("Successfully setup the azure search data");

                log.LogInformation("Update azure search service");
                await this.knowledgeBaseSearchService.InitializeSearchServiceDependencyAsync().ConfigureAwait(false);
                log.LogInformation("Successfully updated azure search service");

            }
            catch (Exception ex)
            {
                log.LogError(ex, "Exception occured while publishing knowledge base.", SeverityLevel.Error);
                throw;
            }
        }
    }
}
