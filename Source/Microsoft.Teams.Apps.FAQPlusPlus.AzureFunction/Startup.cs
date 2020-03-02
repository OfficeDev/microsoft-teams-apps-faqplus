// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using System;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
using IConfigurationDataProvider = Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers.IConfigurationDataProvider;

[assembly: WebJobsStartup(typeof(Startup))]

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction
{
    /// <summary>
    /// Azure function Startup Class.
    /// </summary>
    public class Startup : IWebJobsStartup
    {
        /// <summary>
        /// Application startup configuration.
        /// </summary>
        /// <param name="builder">Webjobs builder.</param>
        public void Configure(IWebJobsBuilder builder)
        {
            IQnAMakerClient qnaMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("QnAMakerSubscriptionKey"))) { Endpoint = Environment.GetEnvironmentVariable("QnAMakerApiUrl") };
            builder.Services.AddSingleton<IQnaServiceProvider>((provider) => new QnaServiceProvider(
                provider.GetRequiredService<IConfigurationDataProvider>(), provider.GetRequiredService<IOptionsMonitor<QnAMakerSettings>>(), qnaMakerClient));
            builder.Services.AddSingleton<IConfigurationDataProvider, Common.Providers.ConfigurationDataProvider>();
            builder.Services.AddSingleton<ISearchServiceDataProvider>((provider) => new SearchServiceDataProvider(provider.GetRequiredService<IQnaServiceProvider>(), Environment.GetEnvironmentVariable("StorageConnectionString")));
            builder.Services.AddSingleton<IConfigurationDataProvider>(new Common.Providers.ConfigurationDataProvider(Environment.GetEnvironmentVariable("StorageConnectionString")));
            builder.Services.AddSingleton<IKnowledgeBaseSearchService, KnowledgeBaseSearchService>();
            builder.Services.AddSingleton<IKnowledgeBaseSearchService>((provider) => new KnowledgeBaseSearchService(Environment.GetEnvironmentVariable("SearchServiceName"), Environment.GetEnvironmentVariable("SearchServiceQueryApiKey"), Environment.GetEnvironmentVariable("SearchServiceAdminApiKey"), Environment.GetEnvironmentVariable("StorageConnectionString")));
        }
    }
}
