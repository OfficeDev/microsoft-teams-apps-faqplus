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
    using Microsoft.Bot.Builder.Azure;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.TeamData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.UserData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
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
                botSettings.AppId = this.Configuration["APPId"];
                botSettings.SessionExpiryInMinutes = Convert.ToInt32(this.Configuration["SessionExpiryInMinutes"]);
            });
            services.AddOptions<RepositoryOptions>().Configure<IConfiguration>((repositoryOptions, configuration) =>
            {
                repositoryOptions.StorageAccountConnectionString =
                    configuration.GetValue<string>("StorageConnectionString");

                // Setting this to true because the main application should ensure that all
                // tables exist.
                repositoryOptions.EnsureTableExists = true;
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<Common.Providers.IConfigurationDataProvider>(new Common.Providers.ConfigurationDataProvider(this.Configuration["StorageConnectionString"]));
            services.AddHttpClient();
            services.AddSingleton<ICredentialProvider, ConfigurationCredentialProvider>();
            services.AddSingleton<ITicketsProvider>(new TicketsProvider(this.Configuration["StorageConnectionString"]));
            services.AddSingleton<IFeedbackProvider>(new FeedbackProvider(this.Configuration["StorageConnectionString"]));
            services.AddSingleton<IConversationProvider>(new ConversationProvider(this.Configuration["StorageConnectionString"]));
            services.AddSingleton<IUserActionProvider>(new UserActionProvider(this.Configuration["StorageConnectionString"]));
            services.AddSingleton<IExpertProvider>(new ExpertProvider(this.Configuration["StorageConnectionString"]));
            services.AddSingleton<IBotFrameworkHttpAdapter, BotFrameworkHttpAdapter>();
            services.AddSingleton(new MicrosoftAppCredentials(this.Configuration["MicrosoftAppId"], this.Configuration["MicrosoftAppPassword"]));

            // Add SOS service
            services.AddSingleton<ISOSClient>(new SOSClient(this.Configuration["SOSBaseUrl"], new SOSBasicAuth(this.Configuration["SOSUserName"], this.Configuration["SOSPassword"])));

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

            AzureBlobStorage storage = new AzureBlobStorage(this.Configuration["StorageConnectionString"], Common.Constants.ConversationStorageContainer);
            services.AddSingleton<IStorage>(storage);

            // You can inspect the UserState
            services.AddSingleton<UserState>();

            // Add TeamDataCapture services
            services.AddTransient<TeamsDataCapture>();

            // Add repositories
            services.AddSingleton<TeamDataRepository>();
            services.AddSingleton<UserDataRepository>();
            services.AddSingleton<AppConfigRepository>();

            // You can inspect the ConversationState
            services.AddSingleton<ConversationState>();
            services.AddTransient<IBot, FaqPlusPlusBot>();

            // Recommend configuration
            RecommendConfiguration con = new RecommendConfiguration();
            con.RecommendationContinousFailureTimes = Convert.ToInt32(this.Configuration["RecommendationContinousFailureTimes"]);
            con.RecommendationIntervalInMinutes = Convert.ToInt32(this.Configuration["RecommendationIntervalInMinutes"]);
            services.AddSingleton<RecommendConfiguration>(con);

            // Create the telemetry middleware(used by the telemetry initializer) to track conversation events
            services.AddSingleton<TelemetryLoggerMiddleware>();
            services.AddMemoryCache();

            // Add miscellaneous dependencies
            services.AddSingleton<IAppSettingsService, AppSettingsService>();
        }
    }
}