// <copyright file="MessagingExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using Microsoft.ApplicationInsights;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Services;
    using Newtonsoft.Json;

    /// <summary>
    /// Implements the logic of the messaging extension for FAQ++
    /// </summary>
    public class MessagingExtension
    {
        private const string SearchTextParameterName = "searchText";        // parameter name in the manifest file
        private const string RecentCommandId = "recents";
        private const string OpenCommandId = "openrequests";
        private const string AssignedCommandId = "assignedrequests";
        private const int DefaultAccessCacheExpiryInDays = 5;

        private readonly ISearchService searchService;
        private readonly TelemetryClient telemetryClient;
        private readonly IConfiguration configuration;
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly Common.Providers.IConfigurationProvider configurationProvider;
        private readonly string appID;
        private readonly BotFrameworkAdapter botAdapter;
        private readonly IMemoryCache accessCache;
        private readonly int accessCacheExpiryInDays;
        private readonly ITicketsProvider ticketsProvider;
        private readonly Lazy<Task> initializeTicketsProviderTask;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtension"/> class.
        /// </summary>
        /// <param name="searchService">searchService DI.</param>
        /// <param name="telemetryClient">telemetryClient DI.</param>
        /// <param name="configuration">configuration DI.</param>
        /// <param name="adapter">adapter DI.</param>
        /// <param name="configurationProvider">configurationProvider DI.</param>
        /// <param name="memoryCache">IMemoryCache DI.</param>
        /// <param name="ticketsProvider">ITicketsProvider DI.</param>
        public MessagingExtension(
            ISearchService searchService,
            TelemetryClient telemetryClient,
            IConfiguration configuration,
            IBotFrameworkHttpAdapter adapter,
            Common.Providers.IConfigurationProvider configurationProvider,
            IMemoryCache memoryCache,
            ITicketsProvider ticketsProvider)
        {
            this.searchService = searchService;
            this.telemetryClient = telemetryClient;
            this.configuration = configuration;
            this.adapter = adapter;
            this.configurationProvider = configurationProvider;
            this.appID = this.configuration["MicrosoftAppId"];
            this.botAdapter = (BotFrameworkAdapter)this.adapter;
            this.accessCache = memoryCache;
            this.ticketsProvider = ticketsProvider;

            this.accessCacheExpiryInDays = Convert.ToInt32(this.configuration["AccessCacheExpiryInDays"]);
            if (this.accessCacheExpiryInDays <= 0)
            {
                this.accessCacheExpiryInDays = DefaultAccessCacheExpiryInDays;
            }

            // Ensure that the tables for the tickets are created by running a dummy query against the provider
            this.initializeTicketsProviderTask = new Lazy<Task>(() => this.ticketsProvider.GetTicketAsync(string.Empty));
        }

        /// <summary>
        /// Based on type of activity return the search results or error result.
        /// </summary>
        /// <param name="turnContext">turnContext for messaging extension.</param>
        /// <returns><see cref="Task"/> that returns an <see cref="InvokeResponse"/> with search results, or null to ignore the activity.</returns>
        public async Task<InvokeResponse> HandleMessagingExtensionQueryAsync(ITurnContext<IInvokeActivity> turnContext)
        {
            try
            {
                if (turnContext.Activity.Name == "composeExtension/query")
                {
                    if (await this.IsMemberOfSmeTeamAsync(turnContext))
                    {
                        var messageExtensionQuery = JsonConvert.DeserializeObject<MessagingExtensionQuery>(turnContext.Activity.Value.ToString());
                        var searchQuery = this.GetSearchQueryString(messageExtensionQuery);

                        await this.EnsureTicketsProviderInitializedAsync();

                        return new InvokeResponse
                        {
                            Body = new MessagingExtensionResponse
                            {
                                ComposeExtension = await this.GetSearchResultAsync(searchQuery, messageExtensionQuery.CommandId, messageExtensionQuery.QueryOptions.Count, messageExtensionQuery.QueryOptions.Skip, turnContext.Activity.LocalTimestamp),
                            },
                            Status = 200,
                        };
                    }
                    else
                    {
                        return new InvokeResponse
                        {
                            Body = new MessagingExtensionResponse
                            {
                                ComposeExtension = new MessagingExtensionResult
                                {
                                    Text = Resource.NonSMEErrorText,
                                    Type = "message"
                                },
                            },
                            Status = 200,
                        };
                    }
                }
                else
                {
                    InvokeResponse response = null;
                    return response;
                }
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackTrace($"Failed to handle the ME command {turnContext.Activity.Name}: {ex.Message}", ApplicationInsights.DataContracts.SeverityLevel.Error);
                this.telemetryClient.TrackException(ex);
                throw;
            }
        }

        /// <summary>
        /// Get the results from Azure search service and populate the result (card + preview).
        /// </summary>
        /// <param name="query">query which the user had typed in message extension search.</param>
        /// <param name="commandId">commandId to determine which tab in message extension has been invoked.</param>
        /// <param name="count">count for pagination.</param>
        /// <param name="skip">skip for pagination.</param>
        /// <param name="localTimestamp">Local timestamp of the user activity.</param>
        /// <returns><see cref="Task"/> returns MessagingExtensionResult which will be used for providing the card.</returns>
        public async Task<MessagingExtensionResult> GetSearchResultAsync(string query, string commandId, int? count, int? skip, DateTimeOffset? localTimestamp)
        {
            MessagingExtensionResult composeExtensionResult = new MessagingExtensionResult
            {
                Type = "result",
                AttachmentLayout = "list",
                Attachments = new List<MessagingExtensionAttachment>(),
            };

            IList<TicketEntity> searchServiceResults = new List<TicketEntity>();

            // Enable prefix matches
            query = (query ?? string.Empty) + "*";

            // commandId should be equal to Id mentioned in Manifest file under composeExtensions section
            switch (commandId)
            {
                case RecentCommandId:
                    searchServiceResults = await this.searchService.SearchTicketsAsync(TicketSearchScope.RecentTickets, query, count, skip);
                    break;

                case OpenCommandId:
                    searchServiceResults = await this.searchService.SearchTicketsAsync(TicketSearchScope.OpenTickets, query, count, skip);
                    break;

                case AssignedCommandId:
                    searchServiceResults = await this.searchService.SearchTicketsAsync(TicketSearchScope.AssignedTickets, query, count, skip);
                    break;
            }

            foreach (var ticket in searchServiceResults)
            {
                ThumbnailCard previewCard = new ThumbnailCard
                {
                    Title = ticket.Title,
                    Text = this.GetPreviewCardText(ticket, commandId, localTimestamp),
                };

                var selectedTicketAdaptiveCard = new MessagingExtensionTicketsCard(ticket);
                composeExtensionResult.Attachments.Add(selectedTicketAdaptiveCard.ToAttachment(localTimestamp).ToMessagingExtensionAttachment(previewCard.ToAttachment()));
            }

            return composeExtensionResult;
        }

        // Get the text for the preview card for the result
        private string GetPreviewCardText(TicketEntity ticket, string commandId, DateTimeOffset? localTimestamp)
        {
            var line2 = !commandId.Equals(OpenCommandId) ?
                $"<div style='white-space:nowrap'>{HttpUtility.HtmlEncode(CardHelper.GetTicketDisplayStatusForSme(ticket))}</div>" :
                string.Empty;

            var text = $@"
<div>
    <div style='white-space:nowrap'>{HttpUtility.HtmlEncode(CardHelper.GetFormattedDateInUserTimeZone(ticket.DateCreated, localTimestamp))} | {HttpUtility.HtmlEncode(ticket.RequesterName)}</div>
    {line2}
</div>";
            return text.Trim();
        }

        // Get the value of the searchText parameter in the ME query
        private string GetSearchQueryString(MessagingExtensionQuery query)
        {
            string messageExtensionInputText = string.Empty;
            foreach (var parameter in query.Parameters)
            {
                if (parameter.Name.Equals(SearchTextParameterName, StringComparison.OrdinalIgnoreCase))
                {
                    messageExtensionInputText = parameter.Value.ToString();
                    break;
                }
            }

            return messageExtensionInputText;
        }

        // Check if user using the app is a valid SME or not
        private async Task<bool> IsMemberOfSmeTeamAsync(ITurnContext<IInvokeActivity> turnContext)
        {
            var teamId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId);
            bool isUserPartOfRoster = false;
            try
            {
                ConversationAccount conversationAccount = new ConversationAccount();
                conversationAccount.Id = teamId;

                ConversationReference conversationReference = new ConversationReference();
                conversationReference.ServiceUrl = turnContext.Activity.ServiceUrl;
                conversationReference.Conversation = conversationAccount;

                string currentUserId = turnContext.Activity.From.Id;

                // Check for current user id in cache and add id of current user to cache if they are not added before
                // once they are validated againt SME roster
                if (!this.accessCache.TryGetValue(currentUserId, out string membersCacheEntry))
                {
                    await this.botAdapter.ContinueConversationAsync(
                        this.appID,
                        conversationReference,
                        async (newTurnContext, newCancellationToken) =>
                        {
                            var members = await this.botAdapter.GetConversationMembersAsync(newTurnContext, default(CancellationToken));
                            foreach (var member in members)
                            {
                                if (member.Id.Equals(currentUserId))
                                {
                                    membersCacheEntry = member.Id;
                                    isUserPartOfRoster = true;

                                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(this.accessCacheExpiryInDays));
                                    this.accessCache.Set(currentUserId, membersCacheEntry, cacheEntryOptions);
                                    break;
                                }
                            }
                        },
                        default(CancellationToken));
                }
                else
                {
                    isUserPartOfRoster = true;
                }
            }
            catch (Exception error)
            {
                this.telemetryClient.TrackTrace($"Failed to get members of team {teamId}: {error.Message}", ApplicationInsights.DataContracts.SeverityLevel.Error);
                this.telemetryClient.TrackException(error);
                isUserPartOfRoster = false;
            }

            return isUserPartOfRoster;
        }

        /// <summary>
        /// Ensures that tickets table is created by running a dummy query against it
        /// </summary>
        /// <returns>Task</returns>
        private Task EnsureTicketsProviderInitializedAsync()
        {
            return this.initializeTicketsProviderTask.Value;
        }
    }
}
