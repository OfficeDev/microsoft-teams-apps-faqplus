// <copyright file="Startup.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction;

[assembly: WebJobsStartup(typeof(Startup))]

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction
{
    extern alias BetaLib;

    using System;
    using System.Globalization;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Options;
    using Microsoft.Graph;
    using Microsoft.Identity.Client;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.NotificationData.NotificationDataServices;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.NotificationPrepareToSend;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.NotificationSend;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.NotificationSend.Services;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.TeamData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.UserData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.AdaptiveCard;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.CommonBot;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.DataQueue;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.PrepareToSendQueue;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.SendQueue;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.Teams;
    using Beta = BetaLib::Microsoft.Graph;

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

            #region Archived
            //IQnAMakerClient qnaMakerClient = new QnAMakerClient(new ApiKeyServiceClientCredentials(Environment.GetEnvironmentVariable("QnAMakerSubscriptionKey"))) { Endpoint = Environment.GetEnvironmentVariable("QnAMakerApiUrl") };
            //builder.Services.AddSingleton<IQnaServiceProvider>((provider) => new QnaServiceProvider(
            //    provider.GetRequiredService<IConfigurationDataProvider>(), provider.GetRequiredService<IOptionsMonitor<QnAMakerSettings>>(), qnaMakerClient));
            //builder.Services.AddSingleton<IConfigurationDataProvider, Common.Providers.ConfigurationDataProvider>();
            //builder.Services.AddSingleton<ISearchServiceDataProvider>((provider) => new SearchServiceDataProvider(provider.GetRequiredService<IQnaServiceProvider>(), Environment.GetEnvironmentVariable("StorageConnectionString")));
            //builder.Services.AddSingleton<IConfigurationDataProvider>(new Common.Providers.ConfigurationDataProvider(Environment.GetEnvironmentVariable("StorageConnectionString")));
            //builder.Services.AddSingleton<IKnowledgeBaseSearchService, KnowledgeBaseSearchService>();
            //builder.Services.AddSingleton<IKnowledgeBaseSearchService>((provider) => new KnowledgeBaseSearchService(Environment.GetEnvironmentVariable("SearchServiceName"), Environment.GetEnvironmentVariable("SearchServiceQueryApiKey"), Environment.GetEnvironmentVariable("SearchServiceAdminApiKey"), Environment.GetEnvironmentVariable("StorageConnectionString")));
            #endregion

            //#region PrepareToSend
            // Add all options set from configuration values.
            builder.Services.AddOptions<RepositoryOptions>()
                .Configure<IConfiguration>((repositoryOptions, configuration) =>
                {
                    repositoryOptions.StorageAccountConnectionString =
                        configuration.GetValue<string>("StorageAccountConnectionString");

                    // Defaulting this value to true because the main app should ensure all
                    // tables exist. It is here as a possible configuration setting in
                    // case it needs to be set differently.
                    repositoryOptions.EnsureTableExists =
                        !configuration.GetValue<bool>("IsItExpectedThatTableAlreadyExists", true);
                });
            builder.Services.AddOptions<MessageQueueOptions>()
                .Configure<IConfiguration>((messageQueueOptions, configuration) =>
                {
                    messageQueueOptions.ServiceBusConnection =
                        configuration.GetValue<string>("ServiceBusConnection");
                });

            builder.Services.AddSingleton<PrepareToSendQueue>();
            //builder.Services.AddOptions<BotOptions>()
            //    .Configure<IConfiguration>((botOptions, configuration) =>
            //    {
            //        botOptions.MicrosoftAppId =
            //            configuration.GetValue<string>("MicrosoftAppId");
            //        botOptions.MicrosoftAppPassword =
            //            configuration.GetValue<string>("MicrosoftAppPassword");
            //    });
            //builder.Services.AddOptions<DataQueueMessageOptions>()
            //    .Configure<IConfiguration>((dataQueueMessageOptions, configuration) =>
            //    {
            //        dataQueueMessageOptions.MessageDelayInSeconds =
            //            configuration.GetValue<double>("DataQueueMessageDelayInSeconds", 5);
            //    });

            //builder.Services.AddOptions<TeamsConversationOptions>()
            //    .Configure<IConfiguration>((options, configuration) =>
            //    {
            //        options.ProactivelyInstallUserApp =
            //            configuration.GetValue<bool>("ProactivelyInstallUserApp", true);

            //        options.MaxAttemptsToCreateConversation =
            //            configuration.GetValue<int>("MaxAttemptsToCreateConversation", 2);
            //    });

            //builder.Services.AddOptions<ConfidentialClientApplicationOptions>().
            //    Configure<IConfiguration>((confidentialClientApplicationOptions, configuration) =>
            //    {
            //        confidentialClientApplicationOptions.ClientId = configuration.GetValue<string>("MicrosoftAppId");
            //        confidentialClientApplicationOptions.ClientSecret = configuration.GetValue<string>("MicrosoftAppPassword");
            //        confidentialClientApplicationOptions.TenantId = configuration.GetValue<string>("TenantId");
            //    });

            //builder.Services.AddLocalization();

            //// Set current culture.
            //var culture = Environment.GetEnvironmentVariable("i18n:DefaultCulture");
            //CultureInfo.DefaultThreadCurrentCulture = new CultureInfo(culture);
            //CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo(culture);

            //// Add orchestration.
            ////builder.Services.AddTransient<ExportOrchestration>();

            //// Add activities.
            ////builder.Services.AddTransient<UpdateExportDataActivity>();
            ////builder.Services.AddTransient<GetMetadataActivity>();
            ////builder.Services.AddTransient<UploadActivity>();
            ////builder.Services.AddTransient<SendFileCardActivity>();
            ////builder.Services.AddTransient<HandleExportFailureActivity>();

            //// Add bot services.
            //builder.Services.AddSingleton<CommonMicrosoftAppCredentials>();
            //builder.Services.AddSingleton<ICredentialProvider, CommonBotCredentialProvider>();
            //builder.Services.AddSingleton<BotFrameworkHttpAdapter>();

            //// Add repositories.
            builder.Services.AddSingleton<NotificationDataRepository>();
            ////builder.Services.AddSingleton<SendingNotificationDataRepository>();
            builder.Services.AddSingleton<SentNotificationDataRepository>();
            //builder.Services.AddSingleton<UserDataRepository>();
            //builder.Services.AddSingleton<TeamDataRepository>();
            ////builder.Services.AddSingleton<ExportDataRepository>();
            //builder.Services.AddSingleton<AppConfigRepository>();

            //// Add service bus message queues.
            //builder.Services.AddSingleton<SendQueue>();
            //builder.Services.AddSingleton<DataQueue>();
            ////builder.Services.AddSingleton<ExportQueue>();

            //// Add miscellaneous dependencies.
            //builder.Services.AddTransient<TableRowKeyGenerator>();
            //builder.Services.AddTransient<AdaptiveCardCreator>();
            //builder.Services.AddSingleton<IAppSettingsService, AppSettingsService>();

            //// Add Teams services.
            //builder.Services.AddTransient<ITeamMembersService, TeamMembersService>();
            //builder.Services.AddTransient<IConversationService, ConversationService>();

            //// Add graph services.
            //this.AddGraphServices(builder);

            ////builder.Services.AddTransient<IDataStreamFacade, DataStreamFacade>();
            //#endregion

            //#region NotificationSend
            //// Add all options set from configuration values.
            //builder.Services.AddOptions<SendFunctionOptions>()
            //    .Configure<IConfiguration>((companyCommunicatorSendFunctionOptions, configuration) =>
            //    {
            //        companyCommunicatorSendFunctionOptions.MaxNumberOfAttempts =
            //            configuration.GetValue<int>("MaxNumberOfAttempts", 1);

            //        companyCommunicatorSendFunctionOptions.SendRetryDelayNumberOfSeconds =
            //            configuration.GetValue<double>("SendRetryDelayNumberOfSeconds", 660);
            //    });
            //builder.Services.AddOptions<BotOptions>()
            //    .Configure<IConfiguration>((botOptions, configuration) =>
            //    {
            //        botOptions.MicrosoftAppId =
            //            configuration.GetValue<string>("MicrosoftAppId");

            //        botOptions.MicrosoftAppPassword =
            //            configuration.GetValue<string>("MicrosoftAppPassword");
            //    });
            //builder.Services.AddOptions<RepositoryOptions>()
            //    .Configure<IConfiguration>((repositoryOptions, configuration) =>
            //    {
            //        repositoryOptions.StorageAccountConnectionString =
            //            configuration.GetValue<string>("StorageAccountConnectionString");

            //        // Defaulting this value to true because the main app should ensure all
            //        // tables exist. It is here as a possible configuration setting in
            //        // case it needs to be set differently.
            //        repositoryOptions.EnsureTableExists =
            //            !configuration.GetValue<bool>("IsItExpectedThatTableAlreadyExists", true);
            //    });
            //builder.Services.AddOptions<MessageQueueOptions>()
            //    .Configure<IConfiguration>((messageQueueOptions, configuration) =>
            //    {
            //        messageQueueOptions.ServiceBusConnection =
            //            configuration.GetValue<string>("ServiceBusConnection");
            //    });

            //builder.Services.AddLocalization();

            //// Add bot services.
            //builder.Services.AddSingleton<CommonMicrosoftAppCredentials>();
            //builder.Services.AddSingleton<ICredentialProvider, CommonBotCredentialProvider>();
            //builder.Services.AddSingleton<BotFrameworkHttpAdapter>();

            //// Add teams services.
            //builder.Services.AddTransient<IMessageService, MessageService>();

            //// Add repositories.
            //builder.Services.AddSingleton<SendingNotificationDataRepository>();
            //builder.Services.AddSingleton<GlobalSendingNotificationDataRepository>();
            //builder.Services.AddSingleton<SentNotificationDataRepository>();

            //// Add service bus message queues.
            //builder.Services.AddSingleton<SendQueue>();

            //// Add the Notification service.
            //builder.Services.AddTransient<INotificationService, NotificationService>();
            //#endregion

            //#region NotificationData
            //// Add all options set from configuration values.
            //builder.Services.AddOptions<RepositoryOptions>()
            //    .Configure<IConfiguration>((repositoryOptions, configuration) =>
            //    {
            //        repositoryOptions.StorageAccountConnectionString =
            //            configuration.GetValue<string>("StorageAccountConnectionString");

            //        // Defaulting this value to true because the main app should ensure all
            //        // tables exist. It is here as a possible configuration setting in
            //        // case it needs to be set differently.
            //        repositoryOptions.EnsureTableExists =
            //            !configuration.GetValue<bool>("IsItExpectedThatTableAlreadyExists", true);
            //    });
            //builder.Services.AddOptions<MessageQueueOptions>()
            //    .Configure<IConfiguration>((messageQueueOptions, configuration) =>
            //    {
            //        messageQueueOptions.ServiceBusConnection =
            //            configuration.GetValue<string>("ServiceBusConnection");
            //    });
            //builder.Services.AddOptions<BotOptions>()
            //   .Configure<IConfiguration>((botOptions, configuration) =>
            //   {
            //       botOptions.MicrosoftAppId =
            //           configuration.GetValue<string>("MicrosoftAppId");

            //       botOptions.MicrosoftAppPassword =
            //           configuration.GetValue<string>("MicrosoftAppPassword");
            //   });

            //builder.Services.AddOptions<NotificationData.DataQueueMessageOptions>()
            //    .Configure<IConfiguration>((dataQueueMessageOptions, configuration) =>
            //    {
            //        dataQueueMessageOptions.FirstTenMinutesRequeueMessageDelayInSeconds =
            //            configuration.GetValue<double>("FirstTenMinutesRequeueMessageDelayInSeconds", 20);

            //        dataQueueMessageOptions.RequeueMessageDelayInSeconds =
            //            configuration.GetValue<double>("RequeueMessageDelayInSeconds", 120);
            //    });

            //builder.Services.AddLocalization();

            //// Add blob client.
            ////builder.Services.AddSingleton(sp => new BlobContainerClient(
            ////    sp.GetService<IConfiguration>().GetValue<string>("StorageAccountConnectionString"),
            ////    Common.Constants.BlobContainerName));

            //// Add bot services.
            //builder.Services.AddSingleton<CommonMicrosoftAppCredentials>();
            //builder.Services.AddSingleton<ICredentialProvider, CommonBotCredentialProvider>();
            //builder.Services.AddSingleton<BotFrameworkHttpAdapter>();

            //// Add services.
            ////builder.Services.AddSingleton<IFileCardService, FileCardService>();

            //// Add notification data services.
            //builder.Services.AddTransient<AggregateSentNotificationDataService>();
            //builder.Services.AddTransient<UpdateNotificationDataService>();

            //// Add repositories.
            //builder.Services.AddSingleton<NotificationDataRepository>();
            //builder.Services.AddSingleton<SentNotificationDataRepository>();
            //builder.Services.AddSingleton<UserDataRepository>();

            //// Add service bus message queues.
            //builder.Services.AddSingleton<DataQueue>();
            //#endregion
        }

        /// <summary>
        /// Adds Graph Services and related dependencies.
        /// </summary>
        /// <param name="builder">Builder.</param>
        private void AddGraphServices(IWebJobsBuilder builder)
        {
            // Options
            builder.Services.AddOptions<ConfidentialClientApplicationOptions>().
                Configure<IConfiguration>((confidentialClientApplicationOptions, configuration) =>
                {
                    confidentialClientApplicationOptions.ClientId = configuration.GetValue<string>("MicrosoftAppId");
                    confidentialClientApplicationOptions.ClientSecret = configuration.GetValue<string>("MicrosoftAppPassword");
                    confidentialClientApplicationOptions.TenantId = configuration.GetValue<string>("TenantId");
                });

            // Graph Token Services
            builder.Services.AddSingleton<IConfidentialClientApplication>(provider =>
            {
                var options = provider.GetRequiredService<IOptions<ConfidentialClientApplicationOptions>>();
                return ConfidentialClientApplicationBuilder
                    .Create(options.Value.ClientId)
                    .WithClientSecret(options.Value.ClientSecret)
                    .WithAuthority(new Uri($"https://login.microsoftonline.com/{options.Value.TenantId}"))
                    .Build();
            });

            builder.Services.AddSingleton<IAuthenticationProvider, MsalAuthenticationProvider>();

            // Add Graph Clients.
            builder.Services.AddSingleton<IGraphServiceClient>(
                serviceProvider =>
                new GraphServiceClient(serviceProvider.GetRequiredService<IAuthenticationProvider>()));
            builder.Services.AddSingleton<Beta.IGraphServiceClient>(
                sp => new Beta.GraphServiceClient(sp.GetRequiredService<IAuthenticationProvider>()));

            // Add Service Factory
            builder.Services.AddSingleton<IGraphServiceFactory, GraphServiceFactory>();

            // Add Graph Services
            builder.Services.AddScoped<IUsersService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetUsersService());
            builder.Services.AddScoped<IGroupMembersService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetGroupMembersService());
            builder.Services.AddScoped<IAppManagerService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetAppManagerService());
            builder.Services.AddScoped<IChatsService>(sp => sp.GetRequiredService<IGraphServiceFactory>().GetChatsService());
        }
    }
}
