// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Language.QuestionAnswering.Projects;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
using IConfigurationDataProvider = Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers.IConfigurationDataProvider;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction
{
    /// <summary>
    /// Azure function Startup Class.
    /// </summary>
    public class Startup : FunctionsStartup
    {
        private Uri endpoint = new Uri(Environment.GetEnvironmentVariable("QnAMakerApiUrl"));
        private AzureKeyCredential credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("QnAMakerSubscriptionKey"));
        private string projectName = Environment.GetEnvironmentVariable("ProjectName");
        private string deploymentName = Environment.GetEnvironmentVariable("DeploymentName");

        /// <summary>
        /// Application startup configuration.
        /// </summary>
        /// <param name="builder">Webjobs builder.</param>
        public override void Configure(IFunctionsHostBuilder builder)
        {
            this.endpoint = new Uri(Environment.GetEnvironmentVariable("QnAMakerApiUrl"));
            this.credential = new AzureKeyCredential(Environment.GetEnvironmentVariable("QnAMakerSubscriptionKey"));
            this.projectName = Environment.GetEnvironmentVariable("ProjectName");
            this.deploymentName = Environment.GetEnvironmentVariable("DeploymentName");

            builder.Services.AddSingleton<IQuestionAnswerServiceProvider>((provider) => new QuestionAnswerServiceProvider(
                provider.GetRequiredService<IConfigurationDataProvider>(), provider.GetRequiredService<IOptionsMonitor<QnAMakerSettings>>(), this.endpoint, this.credential, this.projectName, this.deploymentName, Environment.GetEnvironmentVariable("QnAMakerSubscriptionKey")));
            builder.Services.AddSingleton<IConfigurationDataProvider, Common.Providers.ConfigurationDataProvider>();
            builder.Services.AddSingleton<ISearchServiceDataProvider>((provider) => new SearchServiceDataProvider(provider.GetRequiredService<IQuestionAnswerServiceProvider>(), Environment.GetEnvironmentVariable("StorageConnectionString")));
            builder.Services.AddSingleton<IConfigurationDataProvider>(new Common.Providers.ConfigurationDataProvider(Environment.GetEnvironmentVariable("StorageConnectionString")));
            builder.Services.AddSingleton<IKnowledgeBaseSearchService, KnowledgeBaseSearchService>();
            var isGCCHybridDeployment = Convert.ToBoolean(Environment.GetEnvironmentVariable("IsGCCHybridDeployment"));
            builder.Services.AddSingleton<IKnowledgeBaseSearchService>((provider) => new KnowledgeBaseSearchService(Environment.GetEnvironmentVariable("SearchServiceName"), Environment.GetEnvironmentVariable("SearchServiceQueryApiKey"), Environment.GetEnvironmentVariable("SearchServiceAdminApiKey"), Environment.GetEnvironmentVariable("StorageConnectionString"), isGCCHybridDeployment));
        }
    }
}
