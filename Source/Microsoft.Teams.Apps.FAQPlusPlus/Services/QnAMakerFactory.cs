// <copyright file="QnAMakerFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Services
{
    using System.Collections.Concurrent;
    using System.Globalization;
    using System.Net.Http;
    using Microsoft.Bot.Builder.AI.QnA;
    using Microsoft.Bot.Configuration;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Produces the right <see cref="QnAMaker"/> instance for a knowledge base.
    /// </summary>
    public class QnAMakerFactory : IQnAMakerFactory
    {
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;
        private readonly ConcurrentDictionary<string, QnAMaker> qnaMakerInstances;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerFactory"/> class.
        /// </summary>
        /// <param name="configuration">App configuration</param>
        public QnAMakerFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
            this.httpClient = new HttpClient();
            this.qnaMakerInstances = new ConcurrentDictionary<string, QnAMaker>();
        }

        /// <inheritdoc/>
        public QnAMaker GetQnAMaker(string knowledgeBaseId, string endpointKey)
        {
            return this.qnaMakerInstances.GetOrAdd(knowledgeBaseId, (kbId) =>
            {
                var serviceConfig = new QnAMakerService
                {
                    KbId = kbId,
                    EndpointKey = endpointKey,
                    Hostname = this.configuration["QnAMakerHostUrl"]
                };
                var options = new QnAMakerOptions
                {
                    Top = 1,
                    ScoreThreshold = float.Parse(this.configuration["ScoreThreshold"], CultureInfo.InvariantCulture),
                };
                return new QnAMaker(serviceConfig, options, this.httpClient);
            });
        }
    }
}
