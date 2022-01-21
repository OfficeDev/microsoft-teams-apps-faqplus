// <copyright file="MessagingExtensionActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.TeamsActivity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Components;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ErrorResponseException = Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models.ErrorResponseException;

    /// <summary>
    /// Class that handles messaging extension query, fetch, submit activity in expert's team chat.
    /// </summary>
    public class MessagingExtensionActivity : IMessagingExtensionActivity
    {
        /// <summary>
        ///  Default access cache expiry in days to check if user using the app is a valid SME or not.
        /// </summary>
        private const int DefaultAccessCacheExpiryInDays = 5;

        /// <summary>
        /// Represents the task module height for Add a new question card.
        /// </summary>
        private const int TaskModuleHeightForAddQuestion = 450;

        /// <summary>
        /// Represents the task module height for Migrate ticket card.
        /// </summary>
        private const int TaskModuleHeightForMigrateTicket = 200;

        /// <summary>
        /// Represents the task module height for Migrate ticket error card.
        /// </summary>
        private const int TaskModuleHeightForMigrateTicketError = 120;

        /// <summary>
        /// Represents the task module width.
        /// </summary>
        private const int TaskModuleWidth = 500;

        /// <summary>
        /// Search text parameter name in the manifest file.
        /// </summary>
        private const string SearchTextParameterName = "searchText";

        /// <summary>
        /// Migrate action command in the manifest file.
        /// </summary>
        private const string MigrateTicketAction = "migrateticket";

        /// <summary>
        /// Represents a set of key/value application configuration properties for FaqPlusPlus bot.
        /// </summary>
        private readonly BotSettings options;

        private readonly string appBaseUri;
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly IQnaServiceProvider qnaServiceProvider;
        private readonly ILogger<MessagingExtensionActivity> logger;
        private readonly IActivityStorageProvider activityStorageProvider;
        private readonly ISearchService searchService;
        private readonly string appId;
        private readonly BotFrameworkAdapter botAdapter;
        private readonly IMemoryCache accessCache;
        private readonly IKnowledgeBaseSearchService knowledgeBaseSearchService;
        private readonly int accessCacheExpiryInDays;
        private readonly ITicketsProvider ticketsProvider;
        private readonly INotificationService notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionActivity"/> class.
        /// </summary>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="activityStorageProvider">Activity storage provider.</param>
        /// <param name="qnaServiceProvider">Question and answer maker service provider.</param>
        /// <param name="searchService">SearchService dependency injection.</param>
        /// <param name="botAdapter">Bot adapter dependency injection.</param>
        /// <param name="memoryCache">IMemoryCache dependency injection.</param>
        /// <param name="knowledgeBaseSearchService">KnowledgeBaseSearchService dependency injection.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for FaqPlusPlus bot.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="ticketsProvider">Instance of Ticket provider helps in fetching and storing information in storage table.</param>
        /// <param name="notificationService">Notifies in expert's Team chat.</param>
        public MessagingExtensionActivity(
            Common.Providers.IConfigurationDataProvider configurationProvider,
            IQnaServiceProvider qnaServiceProvider,
            IActivityStorageProvider activityStorageProvider,
            ISearchService searchService,
            BotFrameworkAdapter botAdapter,
            IMemoryCache memoryCache,
            IKnowledgeBaseSearchService knowledgeBaseSearchService,
            IOptionsMonitor<BotSettings> optionsAccessor,
            ILogger<MessagingExtensionActivity> logger,
            ITicketsProvider ticketsProvider,
            INotificationService notificationService)
        {
            this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            this.qnaServiceProvider = qnaServiceProvider ?? throw new ArgumentNullException(nameof(qnaServiceProvider));
            this.activityStorageProvider = activityStorageProvider ?? throw new ArgumentNullException(nameof(activityStorageProvider));
            this.searchService = searchService ?? throw new ArgumentNullException(nameof(searchService));
            this.botAdapter = botAdapter ?? throw new ArgumentNullException(nameof(botAdapter));
            this.accessCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.knowledgeBaseSearchService = knowledgeBaseSearchService ?? throw new ArgumentNullException(nameof(knowledgeBaseSearchService));
            this.ticketsProvider = ticketsProvider ?? throw new ArgumentNullException(nameof(ticketsProvider));
            this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            this.options = optionsAccessor.CurrentValue;
            this.appId = this.options.ExpertAppId;
            this.appBaseUri = this.options.AppBaseUri;
            this.accessCacheExpiryInDays = this.options.AccessCacheExpiryInDays;
            if (this.accessCacheExpiryInDays <= 0)
            {
                this.accessCacheExpiryInDays = DefaultAccessCacheExpiryInDays;
                this.logger.LogInformation($"Configuration option is not present or out of range for AccessCacheExpiryInDays and the default value is set to: {this.accessCacheExpiryInDays}", SeverityLevel.Information);
            }
        }

        /// <summary>
        /// Handles query to messaging extension.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <returns>Messaging extension response object to fill compose extension section.</returns>
        public async Task<MessagingExtensionResponse> QueryAsync(
            ITurnContext<IInvokeActivity> turnContext)
        {
            var turnContextActivity = turnContext?.Activity;
            try
            {
                turnContextActivity.TryGetChannelData<TeamsChannelData>(out var teamsChannelData);
                string expertTeamId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId).ConfigureAwait(false);

                if (turnContext != null && teamsChannelData?.Team?.Id == expertTeamId && await this.IsMemberOfSmeTeamAsync(turnContext).ConfigureAwait(false))
                {
                    var messageExtensionQuery = JsonConvert.DeserializeObject<MessagingExtensionQuery>(turnContextActivity.Value.ToString());
                    var searchQuery = this.GetSearchQueryString(messageExtensionQuery);

                    return new MessagingExtensionResponse
                    {
                        ComposeExtension = await SearchHelper.GetSearchResultAsync(searchQuery, messageExtensionQuery.CommandId, messageExtensionQuery.QueryOptions.Count, messageExtensionQuery.QueryOptions.Skip, turnContextActivity.LocalTimestamp, this.searchService, this.knowledgeBaseSearchService, this.activityStorageProvider).ConfigureAwait(false),
                    };
                }

                return new MessagingExtensionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Text = Strings.NonSMEErrorText,
                        Type = "message",
                    },
                };
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error while invoking messaging extension: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Handles "Add new question" button via messaging extension.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Action to be performed.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Response of messaging extension action.</returns>
        public async Task<MessagingExtensionActionResponse> FetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            if (turnContext == null)
            {
                throw new ArgumentNullException(nameof(turnContext));
            }
            else if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            try
            {
                turnContext.Activity.TryGetChannelData<TeamsChannelData>(out var teamsChannelData);
                string expertTeamId = this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId).GetAwaiter().GetResult();

                if (teamsChannelData?.Team?.Id != expertTeamId)
                {
                    var unauthorizedUserCard = MessagingExtensionQnaCard.UnauthorizedUserActionCard();
                    return new MessagingExtensionActionResponse
                    {
                        Task = new TaskModuleContinueResponse
                        {
                            Value = new TaskModuleTaskInfo
                            {
                                Card = unauthorizedUserCard ?? throw new ArgumentNullException(nameof(unauthorizedUserCard)),
                                Height = 250,
                                Width = 300,
                                Title = Strings.AddQuestionSubtitle,
                            },
                        },
                    };
                }

                if (action.CommandId.Equals(MigrateTicketAction, StringComparison.OrdinalIgnoreCase))
                {
                    return await this.MigrateTicket(turnContext, action);
                }
                else
                {
                    // Add a question form.
                    var adaptiveCardEditor = MessagingExtensionQnaCard.AddQuestionForm(new AdaptiveSubmitActionData(), this.appBaseUri);
                    return await GetResponseForAddQuestionAsync(adaptiveCardEditor, Strings.AddQuestionSubtitle);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while adding new question from messaging extension");
                throw ex;
            }
        }

        /// <summary>
        /// Handles submit new question.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Action to be performed.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Response of messaging extension action.</returns>
        public async Task<MessagingExtensionActionResponse> SubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            try
            {
                // Migrate ticket action.
                if (action.CommandId.Equals(MigrateTicketAction))
                {
                    var data = JsonConvert.DeserializeObject<MigrateTicketCardPayload>(action.Data.ToString());
                    if (data.ToBeMigrated)
                    {
                        // Get the ticket from the data store.
                        var ticket = await this.ticketsProvider.GetTicketAsync(data.TicketId).ConfigureAwait(false);

                        turnContext.Activity.TryGetChannelData<TeamsChannelData>(out var teamsChannelData);
                        string expertTeamId = this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId).GetAwaiter().GetResult();

                        if (teamsChannelData?.Team?.Id == expertTeamId)
                        {
                            // Notify on the legacy ticket that it is being migrated.
                            await turnContext.SendActivityAsync(MessageFactory.Text(Strings.MigrateTicketText), cancellationToken).ConfigureAwait(false);

                            // Send the card in the SME team.
                            var resourceResponse = await this.notificationService.NotifyInTeamChatAsync(turnContext, new SmeTicketCard(ticket).ToAttachment(), expertTeamId, cancellationToken);
                            this.logger.LogInformation($"Migrated the ticket {ticket.TicketId} from legacy bot to new expert bot.");

                            // Update ticket into in table storage.
                            ticket.SmeCardActivityId = resourceResponse.ActivityId;
                            ticket.SmeThreadConversationId = resourceResponse.Id;
                            await this.ticketsProvider.UpsertTicketAsync(ticket).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    // Add a question action.
                    var postedQuestionObject = ((JObject)turnContext.Activity.Value).GetValue("data", StringComparison.OrdinalIgnoreCase).ToObject<AdaptiveSubmitActionData>();
                    if (postedQuestionObject == null)
                    {
                        return default;
                    }

                    if (postedQuestionObject.BackButtonCommandText == Strings.BackButtonCommandText)
                    {
                        // Populates the prefilled data on task module for adaptive card form fields on back button click.
                        return await GetResponseForAddQuestionAsync(MessagingExtensionQnaCard.AddQuestionForm(postedQuestionObject, this.appBaseUri), Strings.AddQuestionSubtitle).ConfigureAwait(false);
                    }

                    if (postedQuestionObject.PreviewButtonCommandText == Constants.PreviewCardCommandText)
                    {
                        // Preview the actual view of the card on preview button click.
                        return await GetResponseForAddQuestionAsync(MessagingExtensionQnaCard.PreviewCardResponse(postedQuestionObject, this.appBaseUri)).ConfigureAwait(false);
                    }

                    // Response of messaging extension action.
                    return await this.RespondToQuestionMessagingExtensionAsync(postedQuestionObject, turnContext, cancellationToken).ConfigureAwait(false);
                }

                return default;
            }
            catch (Exception ex)
            {
                if (((ErrorResponseException)ex).Body?.Error?.Code == ErrorCodeType.QuotaExceeded)
                {
                    this.logger.LogError(ex, "QnA storage limit exceeded and is not able to save the qna pair. Please contact your system administrator to provision additional storage space.");
                    await turnContext.SendActivityAsync("QnA storage limit exceeded and is not able to save the qna pair. Please contact your system administrator to provision additional storage space.").ConfigureAwait(false);
                    return null;
                }

                this.logger.LogError(ex, "Error while submitting new question via messaging extension");
                await turnContext.SendActivityAsync(Strings.ErrorMessage).ConfigureAwait(false);
                throw ex;
            }
        }

        /// <summary>
        /// Sends error card when migrate fails.
        /// </summary>
        /// <returns>Response of messaging extension action object.</returns>
        private static async Task<MessagingExtensionActionResponse> SendErrorCardForMigrateTicket()
        {
            var attachment = new MigrateAction().GetErrorCard();
            return await GetResponseForMigrateTicketAsync(attachment, false);
        }

        /// <summary>
        /// Get messaging extension response object.
        /// </summary>
        /// <param name="questionAnswerAdaptiveCardEditor">Card as an input.</param>
        /// <param name="titleText">Gets or sets text that appears below the app name and to the right of the app icon.</param>
        /// <returns>Response of messaging extension action object.</returns>
        private static Task<MessagingExtensionActionResponse> GetResponseForAddQuestionAsync(
            Attachment questionAnswerAdaptiveCardEditor,
            string titleText = "")
        {
            return Task.FromResult(new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = questionAnswerAdaptiveCardEditor ?? throw new ArgumentNullException(nameof(questionAnswerAdaptiveCardEditor)),
                        Height = TaskModuleHeightForAddQuestion,
                        Width = TaskModuleWidth,
                        Title = titleText ?? throw new ArgumentNullException(nameof(titleText)),
                    },
                },
            });
        }

        private static Task<MessagingExtensionActionResponse> GetResponseForMigrateTicketAsync(
            Attachment questionAnswerAdaptiveCardEditor,
            bool canMigrate)
        {
            return Task.FromResult(new MessagingExtensionActionResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = questionAnswerAdaptiveCardEditor ?? throw new ArgumentNullException(nameof(questionAnswerAdaptiveCardEditor)),
                        Height = canMigrate ? TaskModuleHeightForMigrateTicket : TaskModuleHeightForMigrateTicketError,
                        Width = 500,
                    },
                },
            });
        }

        /// <summary>
        /// Migrates the ticket.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Action to be performed.</param>
        /// <returns>Response of messaging extension action.</returns>
        private async Task<MessagingExtensionActionResponse> MigrateTicket(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action)
        {
            var expertBotId = turnContext.Activity.Recipient?.Id.Split(':')[1];

            // If bot id in ticket payload is not same as the expert bot Id, then the ticket can be migrated from legacy bot to new bot.
            if (action.MessagePayload?.From?.Application?.Id != expertBotId)
            {
                // Get the ticket id from message payload.
                string cardString = action.MessagePayload.Attachments[0].Content.ToString();
                string[] arr = Regex.Split(cardString, "ticketId");
                if (arr.Length <= 1)
                {
                    this.logger.LogInformation($"Couldn't find ticketId in message payload.");
                    return await SendErrorCardForMigrateTicket();
                }

                string ticketId = Regex.Replace(Regex.Split(arr[1], "title")[0], @"[^0-9a-zA-Z-]+", string.Empty);

                // Get ticket details from table storage for above ticket id and create an attachment.
                if (ticketId != null)
                {
                    var ticket = await this.ticketsProvider.GetTicketAsync(ticketId).ConfigureAwait(false);
                    if (ticket != null)
                    {
                        var attachment = new MigrateAction(ticket).GetCard();
                        return await GetResponseForMigrateTicketAsync(attachment, true);
                    }
                    else
                    {
                        this.logger.LogError($"Couldn't find ticket for ticketId {ticketId} in table storage.");
                        return await SendErrorCardForMigrateTicket();
                    }
                }
                else
                {
                    this.logger.LogInformation($"Couldn't find ticketId in message payload.");
                    return await SendErrorCardForMigrateTicket();
                }
            }
            else
            {
                this.logger.LogInformation($"Couldn't migrate the ticket. Expert bot Id is same in message payload.");
                return await SendErrorCardForMigrateTicket();
            }
        }

        /// <summary>
        /// Get the value of the searchText parameter in the messaging extension query.
        /// </summary>
        /// <param name="query">Contains messaging extension query keywords.</param>
        /// <returns>A value of the searchText parameter.</returns>
        private string GetSearchQueryString(MessagingExtensionQuery query)
        {
            var messageExtensionInputText = query.Parameters.FirstOrDefault(parameter => parameter.Name.Equals(SearchTextParameterName, StringComparison.OrdinalIgnoreCase));
            return messageExtensionInputText?.Value?.ToString();
        }

        /// <summary>
        /// Check if user using the app is a valid SME or not.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents that user using the app is a valid SME while false indicates that user using the app is not a valid SME.</returns>
        private async Task<bool> IsMemberOfSmeTeamAsync(ITurnContext<IInvokeActivity> turnContext)
        {
            bool isUserPartOfRoster = false;
            string expertTeamId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId).ConfigureAwait(false);
            try
            {
                ConversationAccount conversationAccount = new ConversationAccount()
                {
                    Id = expertTeamId ?? throw new ArgumentNullException(nameof(expertTeamId)),
                };

                ConversationReference conversationReference = new ConversationReference()
                {
                    ServiceUrl = turnContext.Activity.ServiceUrl,
                    Conversation = conversationAccount ?? throw new ArgumentNullException(nameof(conversationAccount)),
                };

                string currentUserId = turnContext.Activity.From.Id;

                // Check for current user id in cache and add id of current user to cache if they are not added before
                // once they are validated against SME roster.
                if (!this.accessCache.TryGetValue(currentUserId, out string membersCacheEntry))
                {
                    await this.botAdapter.ContinueConversationAsync(
                        this.appId,
                        conversationReference,
                        async (newTurnContext, newCancellationToken) =>
                        {
                            var members = await this.botAdapter.GetConversationMembersAsync(newTurnContext, default(CancellationToken)).ConfigureAwait(false);
                            foreach (var member in members)
                            {
                                if (member.Id == currentUserId)
                                {
                                    membersCacheEntry = member.Id;
                                    isUserPartOfRoster = true;
                                    var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(this.accessCacheExpiryInDays));
                                    this.accessCache.Set(currentUserId, membersCacheEntry, cacheEntryOptions);
                                    break;
                                }
                            }
                        },
                        default(CancellationToken)).ConfigureAwait(false);
                }
                else
                {
                    isUserPartOfRoster = true;
                }
            }
            catch (Exception error)
            {
                this.logger.LogError(error, $"Failed to get members of team {expertTeamId}: {error.Message}", SeverityLevel.Error);
                isUserPartOfRoster = false;
                throw;
            }

            return isUserPartOfRoster;
        }

        /// <summary>
        ///  Validate the adaptiver card fields while adding the question and answer pair.
        /// </summary>
        /// <param name="postedQnaPairEntity">Qna pair entity contains submitted card data.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Response of messaging extension action.</returns>
        private async Task<MessagingExtensionActionResponse> RespondToQuestionMessagingExtensionAsync(
            AdaptiveSubmitActionData postedQnaPairEntity,
            ITurnContext<IInvokeActivity> turnContext,
            CancellationToken cancellationToken)
        {
            // Check if fields contains Html tags or Question and answer empty then return response with error message.
            if (Validators.IsContainsHtml(postedQnaPairEntity) || Validators.IsQnaFieldsNullOrEmpty(postedQnaPairEntity))
            {
                // Returns the card with validation errors on add QnA task module.
                return await GetResponseForAddQuestionAsync(MessagingExtensionQnaCard.AddQuestionForm(Validators.HtmlAndQnaEmptyValidation(postedQnaPairEntity), this.appBaseUri)).ConfigureAwait(false);
            }

            if (Validators.IsRichCard(postedQnaPairEntity))
            {
                // While adding the new entry in knowledgebase,if user has entered invalid Image URL or Redirect URL then show the error message to user.
                if (Validators.IsImageUrlInvalid(postedQnaPairEntity) || Validators.IsRedirectionUrlInvalid(postedQnaPairEntity))
                {
                    return await GetResponseForAddQuestionAsync(MessagingExtensionQnaCard.AddQuestionForm(Validators.ValidateImageAndRedirectionUrls(postedQnaPairEntity), this.appBaseUri)).ConfigureAwait(false);
                }

                // Return the rich card as response to user if he has filled title & image URL while adding the new entry in knowledgebase.
                return await this.AddQuestionCardResponseAsync(turnContext, postedQnaPairEntity, isRichCard: true, cancellationToken).ConfigureAwait(false);
            }

            // Normal card as response if only question and answer fields are filled while adding the QnA pair in the knowledgebase.
            return await this.AddQuestionCardResponseAsync(turnContext, postedQnaPairEntity, isRichCard: false, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Return normal card as response if only question and answer fields are filled while adding the QnA pair in the knowledgebase.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <param name="isRichCard">Indicate whether it's a rich card or normal. While adding the qna pair,
        /// if sme user is providing the value for fields like: image url or title or subtitle or redirection url then it's a rich card otherwise it will be a normal card containing only question and answer. </param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Response of messaging extension action object.</returns>
        private async Task<MessagingExtensionActionResponse> AddQuestionCardResponseAsync(
        ITurnContext<IInvokeActivity> turnContext,
        AdaptiveSubmitActionData qnaPairEntity,
        bool isRichCard,
        CancellationToken cancellationToken)
        {
            string combinedDescription = QnaHelper.BuildCombinedDescriptionAsync(qnaPairEntity);

            try
            {
                // Check if question exist in the production/test knowledgebase & exactly the same question.
                var hasQuestionExist = await this.qnaServiceProvider.QuestionExistsInKbAsync(qnaPairEntity.UpdatedQuestion).ConfigureAwait(false);

                // Question already exist in knowledgebase.
                if (hasQuestionExist)
                {
                    // Response with question already exist(in test knowledgebase).
                    // If edited question text is already exist in the test knowledgebase.
                    qnaPairEntity.IsQuestionAlreadyExists = true;

                    // Messaging extension response object.
                    return await GetResponseForAddQuestionAsync(MessagingExtensionQnaCard.AddQuestionForm(qnaPairEntity, this.appBaseUri)).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                // Check if exception is not related to empty kb then add the qna pair otherwise throw it.
                if (((ErrorResponseException)ex).Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var knowledgeBaseId = await this.configurationProvider.GetSavedEntityDetailAsync(Constants.KnowledgeBaseEntityId).ConfigureAwait(false);
                    var hasPublished = await this.qnaServiceProvider.GetInitialPublishedStatusAsync(knowledgeBaseId).ConfigureAwait(false);

                    // Check if knowledge base has not published yet.
                    // If knowledge base has published then throw the error otherwise contiue to add the question & answer pair.
                    if (hasPublished)
                    {
                        this.logger.LogError(ex, "Error while checking if the question exists in knowledge base.");
                        throw;
                    }
                }
            }

            // Save the question in the knowledgebase.
            var activityReferenceId = Guid.NewGuid().ToString();
            await this.qnaServiceProvider.AddQnaAsync(qnaPairEntity.UpdatedQuestion?.Trim(), combinedDescription, turnContext.Activity.From.AadObjectId, turnContext.Activity.Conversation.Id, activityReferenceId).ConfigureAwait(false);
            qnaPairEntity.IsTestKnowledgeBase = true;
            ResourceResponse activityResponse;

            // Rich card as response.
            if (isRichCard)
            {
                qnaPairEntity.IsPreviewCard = false;
                activityResponse = await turnContext.SendActivityAsync(MessageFactory.Attachment(MessagingExtensionQnaCard.ShowRichCard(qnaPairEntity, turnContext.Activity.From.Name, Strings.EntryCreatedByText)), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // Normal card as response.
                activityResponse = await turnContext.SendActivityAsync(MessageFactory.Attachment(MessagingExtensionQnaCard.ShowNormalCard(qnaPairEntity, turnContext.Activity.From.Name, actionPerformed: Strings.EntryCreatedByText)), cancellationToken).ConfigureAwait(false);
            }

            this.logger.LogInformation($"Question added by: {turnContext.Activity.From.AadObjectId}");
            ActivityEntity activityEntity = new ActivityEntity { ActivityId = activityResponse.Id, ActivityReferenceId = activityReferenceId ?? throw new ArgumentNullException(nameof(activityReferenceId)) };
            bool operationStatus = await this.activityStorageProvider.AddActivityEntityAsync(activityEntity).ConfigureAwait(false);
            if (!operationStatus)
            {
                this.logger.LogInformation($"Unable to add activity data in table storage.");
            }

            return default;
        }
    }
}
