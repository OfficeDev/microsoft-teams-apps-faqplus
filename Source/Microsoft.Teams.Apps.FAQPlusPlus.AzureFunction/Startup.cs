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
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Connector.Authentication;
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
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.Holiday;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.DataQueue;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.PrepareToSendQueue;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.SendQueue;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MicrosoftGraph;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
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

            builder.Services.AddOptions<TicketExpertOptions>()
                .Configure<IConfiguration>((ticketExpertOptions, configuration) =>
                {
                    ticketExpertOptions.TeamId =
                        configuration.GetValue<string>("ExpertTeamID");
                    ticketExpertOptions.TenantId =
                         configuration.GetValue<string>("ExpertTenantID");
                    ticketExpertOptions.GroupId =
                         configuration.GetValue<string>("ExpertGroupID");
                    ticketExpertOptions.TeamName =
                         configuration.GetValue<string>("ExpertTeamName");
                    ticketExpertOptions.ChannelName =
                         configuration.GetValue<string>("ExpertChannelName");
                });

            builder.Services.AddOptions<MessageQueueOptions>()
                .Configure<IConfiguration>((messageQueueOptions, configuration) =>
                {
                    messageQueueOptions.ServiceBusConnection =
                        configuration.GetValue<string>("ServiceBusConnection");
                });

            builder.Services.AddOptions<BotOptions>()
               .Configure<IConfiguration>((botOptions, configuration) =>
               {
                   botOptions.MicrosoftAppId =
                       configuration.GetValue<string>("MicrosoftAppId");

                   botOptions.MicrosoftAppPassword =
                       configuration.GetValue<string>("MicrosoftAppPassword");
               });

            builder.Services.AddOptions<NotificationData.DataQueueMessageOptions>()
                .Configure<IConfiguration>((dataQueueMessageOptions, configuration) =>
                {
                    dataQueueMessageOptions.MessageDelayInSeconds =
                       configuration.GetValue<double>("DataQueueMessageDelayInSeconds", 5);

                    dataQueueMessageOptions.FirstTenMinutesRequeueMessageDelayInSeconds =
                        configuration.GetValue<double>("FirstTenMinutesRequeueMessageDelayInSeconds", 20);

                    dataQueueMessageOptions.RequeueMessageDelayInSeconds =
                        configuration.GetValue<double>("RequeueMessageDelayInSeconds", 120);
                });

            builder.Services.AddSingleton<ITicketsProvider>(new TicketsProvider(Environment.GetEnvironmentVariable("StorageAccountConnectionString")));
            builder.Services.AddSingleton<IConfigurationDataProvider>(new ConfigurationDataProvider(Environment.GetEnvironmentVariable("StorageAccountConnectionString")));

            builder.Services.AddLocalization();

            //// Add repositories.
            builder.Services.AddSingleton<NotificationDataRepository>();
            builder.Services.AddSingleton<SendingNotificationDataRepository>();
            builder.Services.AddSingleton<SentNotificationDataRepository>();
            builder.Services.AddSingleton<UserDataRepository>();
            builder.Services.AddSingleton<GlobalSendingNotificationDataRepository>();

            //// Add service bus message queues.
            builder.Services.AddSingleton<PrepareToSendQueue>();
            builder.Services.AddSingleton<SendQueue>();
            builder.Services.AddSingleton<DataQueue>();

            //// Add miscellaneous dependencies.
            builder.Services.AddTransient<TableRowKeyGenerator>();
            builder.Services.AddTransient<AdaptiveCardCreator>();

            //// Add bot services.
            builder.Services.AddSingleton<CommonMicrosoftAppCredentials>();
            builder.Services.AddSingleton<ICredentialProvider, CommonBotCredentialProvider>();
            builder.Services.AddSingleton<BotFrameworkHttpAdapter>();

            //// Add teams services.
            builder.Services.AddTransient<IMessageService, MessageService>();

            //// Add the Notification service.
            builder.Services.AddTransient<INotificationService, NotificationService>();
            builder.Services.AddTransient<AggregateSentNotificationDataService>();
            builder.Services.AddTransient<UpdateNotificationDataService>();

            //// Add the Holiday service.
            builder.Services.AddSingleton<HolidayService>();
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
