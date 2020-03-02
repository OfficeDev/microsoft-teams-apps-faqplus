// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// This a Startup class for this Bot.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Startup Configuration.</param>
        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        /// <summary>
        /// Gets Configurations Interfaces.
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">Application Builder.</param>
        /// <param name="env">Hosting Environment.</param>
        public static void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseMvc();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services"> Service Collection Interface.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry();
            services.Configure<KnowledgeBaseSettings>(knowledgeBaseSettings =>
            {
                knowledgeBaseSettings.SearchServiceName = this.Configuration["SearchServiceName"];
                knowledgeBaseSettings.SearchServiceQueryApiKey = this.Configuration["SearchServiceQueryApiKey"];
                knowledgeBaseSettings.SearchServiceAdminApiKey = this.Configuration["SearchServiceAdminApiKey"];
                knowledgeBaseSettings.SearchIndexingIntervalInMinutes = this.Configuration["SearchIndexingIntervalInMinutes"];
                knowledgeBaseSettings.StorageConnectionString = this.Configuration["StorageConnectionString"];
            });

            services.Configure<QnAMakerSettings>(qnAMakerSettings =>
            {
                qnAMakerSettings.ScoreThreshold = this.Configuration["ScoreThreshold"];
            });

            services.Configure<BotSettings>(botSettings =>
            {
                botSettings.AccessCacheExpiryInDays = Convert.ToInt32(this.Configuration["AccessCacheExpiryInDays"]);
                botSettings.AppBaseUri = this.Configuration["AppBaseUri"];
                botSettings.MicrosoftAppId = this.Configuration["MicrosoftAppId"];
                botSettings.TenantId = this.Configuration["TenantId"];
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<Common.Providers.IConfigurationDataProvider>(new Common.Providers.ConfigurationDataProvider(this.Configuration["StorageConnectionString"]));
            services.AddHttpClient();
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<ITicketsProvider>(new TicketsProvider(this.Configuration["StorageConnectionString"]));
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();
            services.AddSingleton(new MicrosoftAppCredentials(this.Configuration["MicrosoftAppId"], this.Configuration["MicrosoftAppPassword"]));

            IQnAMakerClient qnaMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(this.Configuration["QnAMakerSubscriptionKey"])) { Endpoint = this.Configuration["QnAMakerApiEndpointUrl"] };
            string endpointKey = Task.Run(() => qnaMakerClient.EndpointKeys.GetKeysAsync()).Result.PrimaryEndpointKey;

            services.AddSingleton<IQnaServiceProvider>((provider) => new QnaServiceProvider(
                provider.GetRequiredService<Common.Providers.IConfigurationDataProvider>(),
                provider.GetRequiredService<IOptionsMonitor<QnAMakerSettings>>(),
                qnaMakerClient,
                new QnAMakerRuntimeClient(new EndpointKeyServiceClientCredentials(endpointKey)) { RuntimeEndpoint = this.Configuration["QnAMakerHostUrl"] }));
            services.AddSingleton<IActivityStorageProvider>((provider) => new ActivityStorageProvider(provider.GetRequiredService<IOptionsMonitor<KnowledgeBaseSettings>>()));
            services.AddSingleton<IKnowledgeBaseSearchService>((provider) => new KnowledgeBaseSearchService(this.Configuration["SearchServiceName"], this.Configuration["SearchServiceQueryApiKey"], this.Configuration["SearchServiceAdminApiKey"], this.Configuration["StorageConnectionString"]));

            services.AddSingleton<ISearchService, SearchService>();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.AddTransient(sp => (BotFrameworkAdapter)sp.GetRequiredService<IBotFrameworkHttpAdapter>());
            services.AddTransient<IBot, FaqPlusPlusBot>();

            // Create the telemetry middleware(used by the telemetry initializer) to track conversation events
            services.AddSingleton<TelemetryLoggerMiddleware>();
            services.AddMemoryCache();
        }
    }
}