// <copyright file="FaqPlusPlusBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.UserData;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using ErrorResponseException = Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models.ErrorResponseException;

    /// <summary>
    /// Class that handles the teams activity of Faq Plus bot and messaging extension.
    /// </summary>
    public class FaqPlusPlusBot : TeamsActivityHandler
    {
        /// <summary>
        ///  Default access cache expiry in days to check if user using the app is a valid SME or not.
        /// </summary>
        private const int DefaultAccessCacheExpiryInDays = 5;

        /// <summary>
        /// Search text parameter name in the manifest file.
        /// </summary>
        private const string SearchTextParameterName = "searchText";

        /// <summary>
        /// Represents the task module height.
        /// </summary>
        private const int TaskModuleHeight = 450;

        /// <summary>
        /// Represents the task module width.
        /// </summary>
        private const int TaskModuleWidth = 500;

        /// <summary>
        /// Represents the conversation type as personal.
        /// </summary>
        private const string ConversationTypePersonal = "personal";

        /// <summary>
        ///  Represents the conversation type as channel.
        /// </summary>
        private const string ConversationTypeChannel = "channel";

        /// <summary>
        /// ChangeStatus - text that triggers change status action by SME.
        /// </summary>
        private const string ChangeStatus = "change status";

        private static readonly string TeamRenamedEventType = "teamRenamed";

        /// <summary>
        /// Represents a set of key/value application configuration properties for FaqPlusPlus bot.
        /// </summary>
        private readonly BotSettings options;

        private readonly IConfigurationDataProvider configurationProvider;
        private readonly MicrosoftAppCredentials microsoftAppCredentials;
        private readonly ITicketsProvider ticketsProvider;
        private readonly IFeedbackProvider feedbackProvider;
        private readonly IUserActionProvider userActionProvider;
        private readonly IConversationProvider conversationProvider;
        private readonly IExpertProvider expertProvider;
        private readonly IActivityStorageProvider activityStorageProvider;
        private readonly ISearchService searchService;
        private readonly string appId;
        private readonly BotFrameworkAdapter botAdapter;
        private readonly IMemoryCache accessCache;
        private readonly int accessCacheExpiryInDays;
        private readonly string appBaseUri;
        private readonly IKnowledgeBaseSearchService knowledgeBaseSearchService;
        private readonly ILogger<FaqPlusPlusBot> logger;
        private readonly IQnaServiceProvider qnaServiceProvider;
        private readonly BotState conversationState;
        private readonly BotState userState;
        private readonly RecommendConfiguration recommendConfiguration;
        private readonly TeamsDataCapture teamsDataCapture;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaqPlusPlusBot"/> class.
        /// </summary>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="microsoftAppCredentials">Microsoft app credentials to use.</param>
        /// <param name="ticketsProvider">Tickets Provider.</param>
        /// <param name="feedbackProvider">Feedback Provider.</param>
        /// <param name="userActionProvider">UserAction Provider.</param>
        /// <param name="conversationProvider">Conversation storage provider.</param>
        /// <param name="expertProvider">Expert storage provider.</param>
        /// <param name="activityStorageProvider">Activity storage provider.</param>
        /// <param name="qnaServiceProvider">Question and answer maker service provider.</param>
        /// <param name="searchService">SearchService dependency injection.</param>
        /// <param name="botAdapter">Bot adapter dependency injection.</param>
        /// <param name="memoryCache">IMemoryCache dependency injection.</param>
        /// <param name="knowledgeBaseSearchService">KnowledgeBaseSearchService dependency injection.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for FaqPlusPlus bot.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="conversationState">conversation sate cache.</param>
        /// <param name="userState">user state cache.</param>
        /// <param name="recommendConfiguration">configuration to decide recommend.</param>
        /// <param name="teamsDataCapture">Teams data capture service.</param>
        public FaqPlusPlusBot(
            Common.Providers.IConfigurationDataProvider configurationProvider,
            MicrosoftAppCredentials microsoftAppCredentials,
            ITicketsProvider ticketsProvider,
            IFeedbackProvider feedbackProvider,
            IUserActionProvider userActionProvider,
            IConversationProvider conversationProvider,
            IExpertProvider expertProvider,
            IQnaServiceProvider qnaServiceProvider,
            IActivityStorageProvider activityStorageProvider,
            ISearchService searchService,
            BotFrameworkAdapter botAdapter,
            IMemoryCache memoryCache,
            IKnowledgeBaseSearchService knowledgeBaseSearchService,
            IOptionsMonitor<BotSettings> optionsAccessor,
            ILogger<FaqPlusPlusBot> logger,
            ConversationState conversationState,
            UserState userState,
            RecommendConfiguration recommendConfiguration,
            TeamsDataCapture teamsDataCapture)
        {
            this.configurationProvider = configurationProvider;
            this.microsoftAppCredentials = microsoftAppCredentials;
            this.ticketsProvider = ticketsProvider;
            this.feedbackProvider = feedbackProvider;
            this.userActionProvider = userActionProvider;
            this.conversationProvider = conversationProvider;
            this.expertProvider = expertProvider;
            this.options = optionsAccessor.CurrentValue;
            this.qnaServiceProvider = qnaServiceProvider;
            this.activityStorageProvider = activityStorageProvider;
            this.searchService = searchService;
            this.appId = this.options.MicrosoftAppId;
            this.botAdapter = botAdapter;
            this.accessCache = memoryCache;
            this.logger = logger;
            this.accessCacheExpiryInDays = this.options.AccessCacheExpiryInDays;

            if (this.accessCacheExpiryInDays <= 0)
            {
                this.accessCacheExpiryInDays = DefaultAccessCacheExpiryInDays;
                this.logger.LogInformation($"Configuration option is not present or out of range for AccessCacheExpiryInDays and the default value is set to: {this.accessCacheExpiryInDays}", SeverityLevel.Information);
            }

            this.appBaseUri = this.options.AppBaseUri;
            this.knowledgeBaseSearchService = knowledgeBaseSearchService;
            this.conversationState = conversationState;
            this.userState = userState;
            this.recommendConfiguration = recommendConfiguration;
            this.teamsDataCapture = teamsDataCapture ?? throw new ArgumentNullException(nameof(teamsDataCapture));
        }

        /// <summary>
        /// Handles an incoming activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.activityhandler.onturnasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        public override async Task OnTurnAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (turnContext != null & !this.IsActivityFromExpectedTenant(turnContext))
                {
                    this.logger.LogWarning($"Unexpected tenant id {turnContext?.Activity.Conversation.TenantId}");
                    return;
                }

                // Get the current culture info to use in resource files
                string locale = turnContext?.Activity.Entities?.FirstOrDefault(entity => entity.Type == "clientInfo")?.Properties["locale"]?.ToString();

                if (!string.IsNullOrEmpty(locale))
                {
                    CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(locale);
                }

                await base.OnTurnAsync(turnContext, cancellationToken);

                // Save any state changes that might have occurred during the turn.
                await this.conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
                await this.userState.SaveChangesAsync(turnContext, false, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error at OnTurnAsync()");
            }
        }

        /// <summary>
        /// Invoked when a message activity is received from the user.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.activityhandler.onmessageactivityasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var message = turnContext?.Activity;
                this.logger.LogInformation($"from: {message.From?.Id}, conversation: {message.Conversation.Id}, replyToId: {message.ReplyToId}");

                await this.SendTypingIndicatorAsync(turnContext).ConfigureAwait(false);

                switch (message.Conversation.ConversationType.ToLower())
                {
                    case ConversationTypePersonal:
                        await this.OnMessageActivityInPersonalChatAsync(
                            message,
                            turnContext,
                            cancellationToken).ConfigureAwait(false);
                        break;

                    case ConversationTypeChannel:
                        await this.OnMessageActivityInChannelAsync(
                            message,
                            turnContext,
                            cancellationToken).ConfigureAwait(false);
                        break;

                    default:
                        this.logger.LogWarning($"Received unexpected conversationType {message.Conversation.ConversationType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync(Strings.ErrorMessage).ConfigureAwait(false);
                this.logger.LogError(ex, $"Error processing message: {ex.Message}", SeverityLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Invoke when a conversation update activity is received from the channel.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onconversationupdateactivityasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task OnConversationUpdateActivityAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var activity = turnContext?.Activity;
                this.logger.LogInformation("Received conversationUpdate activity");
                this.logger.LogInformation($"conversationType: {activity.Conversation.ConversationType}, membersAdded: {activity.MembersAdded?.Count}, membersRemoved: {activity.MembersRemoved?.Count}");

                var isTeamRenamed = this.IsTeamInformationUpdated(activity);
                if (isTeamRenamed)
                {
                    await this.teamsDataCapture.OnTeamInformationUpdatedAsync(activity);
                }

                // members removed
                if (activity.MembersRemoved != null)
                {
                    await this.teamsDataCapture.OnBotRemovedAsync(activity);
                }

                if (activity?.MembersAdded?.Count == 0)
                {
                    this.logger.LogInformation("Ignoring conversationUpdate that was not a membersAdded event");
                    return;
                }

                // members added
                switch (activity.Conversation.ConversationType.ToLower())
                {
                    case ConversationTypePersonal:
                        await this.OnMembersAddedToPersonalChatAsync(activity.MembersAdded, turnContext).ConfigureAwait(false);
                        break;

                    case ConversationTypeChannel:
                        await this.OnMembersAddedToTeamAsync(activity.MembersAdded, turnContext, cancellationToken).ConfigureAwait(false);
                        break;

                    default:
                        this.logger.LogInformation($"Ignoring event from conversation type {activity.Conversation.ConversationType}");
                        break;
                }

                if (activity.MembersAdded != null)
                {
                    await this.teamsDataCapture.OnBotAddedAsync(activity);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error processing conversationUpdate: {ex.Message}", SeverityLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Handle 1:1 chat with members who started chat for the first time.
        /// </summary>
        /// <param name="membersAdded">Channel account information needed to route a message.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task OnMembersAddedToPersonalChatAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext)
        {
            var activity = turnContext.Activity;
            if (membersAdded.Any(channelAccount => channelAccount.Id == activity.Recipient.Id))
            {
                // User started chat with the bot in personal scope, for the first time.
                this.logger.LogInformation($"Bot added to 1:1 chat {activity.Conversation.Id}");
                var welcomeText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText).ConfigureAwait(false);
                var userWelcomeCardAttachment = WelcomeCard.GetCard(welcomeText, this.appBaseUri);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment)).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handle members added conversationUpdate event in team.
        /// </summary>
        /// <param name="membersAdded">Channel account information needed to route a message.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task OnMembersAddedToTeamAsync(
           IList<ChannelAccount> membersAdded,
           ITurnContext<IConversationUpdateActivity> turnContext,
           CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (membersAdded.Any(channelAccount => channelAccount.Id == activity.Recipient.Id))
            {
                // Bot was added to a team
                this.logger.LogInformation($"Bot added to team {activity.Conversation.Id}");

                var teamDetails = ((JObject)turnContext.Activity.ChannelData).ToObject<TeamsChannelData>();
                var botDisplayName = turnContext.Activity.Recipient.Name;
                var teamWelcomeCardAttachment = WelcomeTeamCard.GetCard(this.appBaseUri);
                await this.SendCardToTeamAsync(turnContext, teamWelcomeCardAttachment, teamDetails.Team.Id, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handle message activity in 1:1 chat.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task OnMessageActivityInPersonalChatAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null) && ((JObject)message.Value).HasValues)
            {
                this.logger.LogInformation("Card submit in 1:1 chat");
                await this.OnAdaptiveCardSubmitInPersonalChatAsync(message, turnContext, cancellationToken).ConfigureAwait(false);
                return;
            }

            UserActionEntity userAction = await this.GeneratePersonalActionEntityAsync(turnContext, cancellationToken);

            string text = message.Text?.ToLower()?.Trim() ?? string.Empty;

            switch (text)
            {
                case Constants.AskAnExpert:
                    this.logger.LogInformation("Sending user ask an expert card");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(AskAnExpertCard.GetCard())).ConfigureAwait(false);
                    userAction.Action = nameof(UserActionType.AskExpertReq);
                    break;

                case Constants.ShareFeedback:
                    this.logger.LogInformation("Sending user feedback card");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(ShareFeedbackCard.GetCard(this.appBaseUri))).ConfigureAwait(false);
                    userAction.Action = nameof(UserActionType.ShareFeedbackReq);
                    break;

                case Constants.TakeATour:
                    this.logger.LogInformation("Sending user tour card");
                    var userTourCards = TourCarousel.GetUserTourCards(this.appBaseUri);
                    await turnContext.SendActivityAsync(MessageFactory.Carousel(userTourCards)).ConfigureAwait(false);
                    userAction.Action = nameof(UserActionType.TakeATour);
                    break;
                case "":
                    await turnContext.SendActivityAsync("\U0001F600").ConfigureAwait(false);
                    break;
                default:
                    this.logger.LogInformation("Sending input to QnAMaker");
                    await this.GetQuestionAnswerReplyAsync(turnContext, message, cancellationToken).ConfigureAwait(false);
                    break;
            }

            if (userAction.Action != nameof(UserActionType.NotDefined))
            {
                await this.userActionProvider.UpsertUserActionAsync(userAction).ConfigureAwait(false);
            }

            await this.teamsDataCapture.OnPersonalTurnAsync(turnContext, cancellationToken);
        }

        /// <summary>
        /// Handle message activity in channel.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task OnMessageActivityInChannelAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            string text;

            // Check if the incoming request is from SME for updating the ticket status.
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null) && ((JObject)message.Value).HasValues && !string.IsNullOrEmpty(((JObject)message.Value)["ticketId"]?.ToString()))
            {
                text = ChangeStatus;
            }
            else
            {
                text = message.Text?.ToLower()?.Trim() ?? string.Empty;
            }

            try
            {
                switch (text)
                {
                    case Constants.TeamTour:
                        this.logger.LogInformation("Sending team tour card");
                        var teamTourCards = TourCarousel.GetTeamTourCards(this.appBaseUri);
                        await turnContext.SendActivityAsync(MessageFactory.Carousel(teamTourCards)).ConfigureAwait(false);
                        break;

                    case ChangeStatus:
                        this.logger.LogInformation($"Card submit in channel {message.Value?.ToString()}");
                        await this.OnAdaptiveCardSubmitInChannelAsync(message, turnContext, cancellationToken).ConfigureAwait(false);
                        return;

                    case Constants.DeleteCommand:
                        this.logger.LogInformation($"Delete card submit in channel {message.Value?.ToString()}");
                        await QnaHelper.DeleteQnaPair(turnContext, this.qnaServiceProvider, this.activityStorageProvider, this.logger, cancellationToken).ConfigureAwait(false);
                        break;

                    case Constants.NoCommand:
                        return;

                    default:
                        this.logger.LogInformation("Unrecognized input in channel");
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(UnrecognizedTeamInputCard.GetCard())).ConfigureAwait(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                // Check if expert user is trying to delete the question and knowledge base has not published yet.
                if (((ErrorResponseException)ex).Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var knowledgeBaseId = await this.configurationProvider.GetSavedEntityDetailAsync(Constants.KnowledgeBaseEntityId).ConfigureAwait(false);
                    var hasPublished = await this.qnaServiceProvider.GetInitialPublishedStatusAsync(knowledgeBaseId).ConfigureAwait(false);

                    // Check if knowledge base has not published yet.
                    if (!hasPublished)
                    {
                        var activity = (Activity)turnContext.Activity;
                        var activityValue = ((JObject)activity.Value).ToObject<AdaptiveSubmitActionData>();
                        await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(CultureInfo.InvariantCulture, Strings.WaitMessage, activityValue?.OriginalQuestion))).ConfigureAwait(false);
                        this.logger.LogError(ex, $"Error processing message: {ex.Message}", SeverityLevel.Error);
                        return;
                    }
                }

                // Throw the error at calling place, if there is any generic exception which is not caught by above conditon.
                throw;
            }
        }

        /// <summary>
        /// Handle adaptive card submit in 1:1 chat.
        /// Submits the question or feedback to the SME team.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task OnAdaptiveCardSubmitInPersonalChatAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            Attachment smeTeamCard = null;      // Notification to SME team
            Activity updateCardActivity = null;
            TicketEntity newTicket = null;      // New ticket
            FeedbackEntity newFeedback = null;      // New Feedback
            List<ExpertEntity> experts = null;

            UserActionEntity userAction = await this.GeneratePersonalActionEntityAsync(turnContext, cancellationToken);

            switch (message?.Text)
            {
                case Constants.AskAnExpert:
                    this.logger.LogInformation("Sending user ask an expert card (from answer)");
                    var askAnExpertPayload = ((JObject)message.Value).ToObject<ResponseCardPayload>();
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(AskAnExpertCard.GetCard(askAnExpertPayload))).ConfigureAwait(false);
                    userAction.Action = nameof(UserActionType.AskExpertReq);
                    break;

                case Constants.ShareFeedback:
                    this.logger.LogInformation("Sending user share feedback card (from answer)");
                    var shareFeedbackPayload = ((JObject)message.Value).ToObject<ResponseCardPayload>();
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(ShareFeedbackCard.GetCard(shareFeedbackPayload, this.appBaseUri))).ConfigureAwait(false);
                    userAction.Action = nameof(UserActionType.ShareFeedbackReq);
                    break;

                case AskAnExpertCard.AskAnExpertSubmitText:
                    this.logger.LogInformation("Received question for expert");
                    newTicket = await AdaptiveCardHelper.AskAnExpertSubmitText(message, turnContext, cancellationToken, this.ticketsProvider).ConfigureAwait(false);
                    if (newTicket != null)
                    {
                        experts = await this.expertProvider.GetExpertsAsync().ConfigureAwait(false);
                        smeTeamCard = new SmeTicketCard(newTicket, experts).ToAttachment(message?.LocalTimestamp, this.appBaseUri);
                        Attachment userCard = new UserNotificationCard(newTicket, this.appBaseUri, this.options.AppId).ToAttachment(Strings.NotificationCardContent, message?.LocalTimestamp);

                        updateCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = turnContext.Activity.ReplyToId,
                            Conversation = turnContext.Activity.Conversation,
                            Attachments = new List<Attachment> { userCard },
                        };
                    }

                    userAction.Action = nameof(UserActionType.AskExpert);
                    break;

                case ShareFeedbackCard.ShareFeedbackSubmitText:
                    this.logger.LogInformation("Received app feedback");
                    newFeedback = await AdaptiveCardHelper.ShareFeedbackSubmitText(message, this.appBaseUri, turnContext, cancellationToken, this.feedbackProvider).ConfigureAwait(false);
                    if (newFeedback != null)
                    {
                        smeTeamCard = SmeFeedbackCard.GetCard(newFeedback, this.appBaseUri);
                        Attachment userCard = UserNotificationCard.ToAttachmentString(Strings.ThankYouTextContent);

                        updateCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = turnContext.Activity.ReplyToId,
                            Conversation = turnContext.Activity.Conversation,
                            Attachments = new List<Attachment> { userCard },
                        };
                    }

                    userAction.Action = nameof(UserActionType.ShareFeedback);
                    break;

                case UserNotificationCard.TicketFeedback:
                    this.logger.LogInformation("Received ticket feedback");
                    var ticketFeedbackPayload = ((JObject)message.Value).ToObject<TicketFeedbackPayload>();
                    var ticket = await this.ticketsProvider.GetTicketAsync(ticketFeedbackPayload.TicketId).ConfigureAwait(false);
                    ticket.Feedback = ticketFeedbackPayload.Rating;
                    await this.ticketsProvider.UpsertTicketAsync(ticket).ConfigureAwait(false);

                    await turnContext.SendActivityAsync(MessageFactory.Text(Strings.ThankYouTextContent)).ConfigureAwait(false);

                    // experts = await this.expertProvider.GetExpertsAsync().ConfigureAwait(false);
                    // smeTeamCard = new SmeTicketCard(ticket, experts).ToAttachment(message?.LocalTimestamp, this.appBaseUri);
                    userAction.Action = nameof(UserActionType.ShareTicketFeedback);
                    break;

                default:
                    var payload = ((JObject)message.Value).ToObject<ResponseCardPayload>();

                    if (payload.IsPrompt)
                    {
                        this.logger.LogInformation("Sending input to QnAMaker for prompt");
                        await this.GetQuestionAnswerReplyAsync(turnContext, message, cancellationToken).ConfigureAwait(false);
                    }
                    else
                    {
                        var payloadRecommend = ((JObject)message.Value).ToObject<RecommendCardPayload>();
                        if (payloadRecommend.Question != null)
                        {
                            this.logger.LogInformation("User select from recommend list");
                            await this.GetQuestionAnswerReplyAsync(turnContext, message, cancellationToken).ConfigureAwait(false);
                        }
                        else
                        {
                            this.logger.LogWarning($"Unexpected text in submit payload: {message.Text}");
                        }
                    }

                    break;
            }

            string expertTeamId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId).ConfigureAwait(false);

            // Send message to SME team.
            if (smeTeamCard != null)
            {
                var resourceResponse = await this.SendCardToTeamAsync(turnContext, smeTeamCard, expertTeamId, cancellationToken).ConfigureAwait(false);

                // If a ticket was created, update the ticket with the conversation info.
                if (newTicket != null)
                {
                    newTicket.SmeCardActivityId = resourceResponse.ActivityId;
                    newTicket.SmeThreadConversationId = resourceResponse.Id;
                }
            }

            // Send acknowledgment to the user by updating the original card
            if (updateCardActivity != null)
            {
                var resourceResponse = await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
                if (newTicket != null)
                {
                    newTicket.RequesterCardActivityId = updateCardActivity.Id;
                }
            }

            if (newTicket != null)
            {
                await this.ticketsProvider.UpsertTicketAsync(newTicket).ConfigureAwait(false);
            }

            // Save user action
            if (userAction.Action != nameof(UserActionType.NotDefined))
            {
                await this.userActionProvider.UpsertUserActionAsync(userAction).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Handle adaptive card submit in channel.
        /// Updates the ticket status based on the user submission.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task OnAdaptiveCardSubmitInChannelAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            //await SaveChannelMembers(turnContext,cancellationToken);

            var payload = ((JObject)message.Value).ToObject<ChangeTicketStatusPayload>();
            this.logger.LogInformation($"Received submit: ticketId={payload.TicketId} action={payload.Action}");

            // Get the ticket from the data store.
            var ticket = await this.ticketsProvider.GetTicketAsync(payload.TicketId).ConfigureAwait(false);
            if (ticket == null)
            {
                await turnContext.SendActivityAsync($"Ticket {payload.TicketId} was not found in the data store").ConfigureAwait(false);
                this.logger.LogInformation($"Ticket {payload.TicketId} was not found in the data store");
                return;
            }

            UserActionEntity userAction = this.GenerateChannelAction();
            userAction.Action = nameof(UserActionType.ChangeStatus);
            userAction.Remark += $"from {((TicketState)ticket.Status).ToString()}";

            // Illegal operation, post warning to SME team thread and return
            if (payload.Action == ChangeTicketStatusPayload.PendingAction || payload.Action == ChangeTicketStatusPayload.ResolveAction)
            {
                // if not the owner
                if (ticket.AssignedToName != message.From.Name)
                {
                    var mention = await this.MentionSomeoneByName(turnContext, cancellationToken, message.From.Name).ConfigureAwait(false);
                    if (mention != null)
                    {
                        var replyActivity = MessageFactory.Text(string.Format(CultureInfo.InvariantCulture, Strings.NotTicketOwnerWarning, mention.Text));
                        replyActivity.Entities = new List<Entity> { mention };

                        await turnContext.SendActivityAsync(replyActivity, cancellationToken);
                        return;
                    }
                }

                // if no comment
                if ((payload.Action == ChangeTicketStatusPayload.PendingAction && payload.PendingComment == string.Empty) || (payload.Action == ChangeTicketStatusPayload.ResolveAction && payload.ResolveComment == string.Empty))
                {
                    var mention = await this.MentionSomeoneByName(turnContext, cancellationToken, message.From.Name).ConfigureAwait(false);
                    if (mention != null)
                    {
                        var replyActivity = MessageFactory.Text(string.Format(CultureInfo.InvariantCulture, Strings.CommentEmptyWarning, mention.Text));
                        replyActivity.Entities = new List<Entity> { mention };

                        await turnContext.SendActivityAsync(replyActivity, cancellationToken);
                        return;
                    }
                }
            }

            // Update the ticket based on the payload.
            string smeNotification = null;
            int previousState = ticket.Status;
            switch (payload.Action)
            {
                case ChangeTicketStatusPayload.PendingAction:
                    ticket.Status = (int)TicketState.Pending;
                    ticket.DatePending = DateTime.UtcNow;
                    ticket.PendingComment = payload.PendingComment;

                    userAction.Remark += $" to Pending";
                    break;

                case ChangeTicketStatusPayload.ResolveAction:
                    ticket.Status = (int)TicketState.Resolved;
                    ticket.DateClosed = DateTime.UtcNow;
                    ticket.DatePending = null;
                    ticket.ResolveComment = payload.ResolveComment;

                    userAction.Remark += $" to Resolved";
                    break;

                case ChangeTicketStatusPayload.AssignToOthersAction:
                    var info = payload.OtherAssigneeInfo.Split(':');
                    string assigneeName = null;
                    string assigneeID = null;
                    string assigneeUserPrincilpeName = null;
                    if (info?.Length == 3)
                    {
                        assigneeName = info[0];
                        assigneeID = info?[1];
                        assigneeUserPrincilpeName = info?[2];
                    }
                    else
                    {
                        throw new ArgumentException("assigned infor error");
                    }

                    // if already assigned
                    if (ticket.Status == (int)TicketState.Assigned && ticket.AssignedToName == assigneeName)
                    {
                        var mention = await this.MentionSomeoneByName(turnContext, cancellationToken, message.From.Name).ConfigureAwait(false);
                        if (mention != null)
                        {
                            var replyActivity = MessageFactory.Text(string.Format(CultureInfo.InvariantCulture, Strings.TicketAssignedToSameOneWarning, mention.Text, ticket.AssignedToName));
                            replyActivity.Entities = new List<Entity> { mention };

                            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
                            return;
                        }
                    }

                    ticket.Status = (int)TicketState.Assigned;
                    ticket.DateAssigned = DateTime.UtcNow;

                    if (info?.Length == 3)
                    {
                        ticket.AssignedToName = assigneeName;
                        ticket.AssignedToObjectId = assigneeID;
                        ticket.AssignedToUserPrincipalName = assigneeUserPrincilpeName;
                    }

                    ticket.DateClosed = null;
                    ticket.DatePending = null;

                    userAction.Remark += $" to Assigned";
                    break;

                case ChangeTicketStatusPayload.AssignToSelfAction:
                    // if already assigned
                    if (ticket.Status == (int)TicketState.Assigned && ticket.AssignedToName == message.From.Name)
                    {
                        var mention = await this.MentionSomeoneByName(turnContext, cancellationToken, message.From.Name).ConfigureAwait(false);
                        if (mention != null)
                        {
                            var replyActivity = MessageFactory.Text(string.Format(CultureInfo.InvariantCulture, Strings.TicketAssignedToSameOneWarning, mention.Text, ticket.AssignedToName));
                            replyActivity.Entities = new List<Entity> { mention };

                            await turnContext.SendActivityAsync(replyActivity, cancellationToken);
                            return;
                        }
                    }

                    var member = await TeamsInfo.GetMemberAsync(turnContext, turnContext.Activity.From.Id, cancellationToken);
                    ticket.Status = (int)TicketState.Assigned;
                    ticket.DateAssigned = DateTime.UtcNow;
                    ticket.AssignedToName = message.From.Name;
                    ticket.AssignedToUserPrincipalName = member.UserPrincipalName;
                    ticket.AssignedToObjectId = message.From.AadObjectId;
                    ticket.DateClosed = null;
                    ticket.DatePending = null;
                    ticket.PendingComment = null;
                    ticket.ResolveComment = null;

                    userAction.Remark += $" to Assigned";
                    break;

                default:
                    this.logger.LogWarning($"Unknown status command {payload.Action}");
                    return;
            }

            userAction.UserName += message.From.Name;

            ticket.LastModifiedByName = message.From.Name;
            ticket.LastModifiedByObjectId = message.From.AadObjectId;
            await this.ticketsProvider.UpsertTicketAsync(ticket).ConfigureAwait(false);
            this.logger.LogInformation($"Ticket {ticket.TicketId} updated to status ({ticket.Status}, {ticket.AssignedToObjectId}) in store");

            // Update the card in the SME team.
            var experts = await this.expertProvider.GetExpertsAsync().ConfigureAwait(false);
            var updateCardActivity = new Activity(ActivityTypes.Message)
            {
                Id = ticket.SmeCardActivityId,
                Conversation = new ConversationAccount { Id = ticket.SmeThreadConversationId },
                Attachments = new List<Attachment> { new SmeTicketCard(ticket, experts).ToAttachment(message.LocalTimestamp, this.appBaseUri) },
            };
            var updateResponse = await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
            this.logger.LogInformation($"Card for ticket {ticket.TicketId} updated to status ({ticket.Status}, {ticket.AssignedToObjectId}), activityId = {updateResponse.Id}");

            // Post update to user and SME team thread.
            Activity updateUserCardActivity = null;
            switch (payload.Action)
            {
                case ChangeTicketStatusPayload.PendingAction:
                    smeNotification = string.Format(CultureInfo.InvariantCulture, Strings.SMEPendingStatus, message.From.Name);

                    updateUserCardActivity = new Activity(ActivityTypes.Message)
                    {
                        Id = ticket.RequesterCardActivityId,
                        Conversation = new ConversationAccount { Id = ticket.RequesterConversationId },
                        Attachments = new List<Attachment> { new UserNotificationCard(ticket, this.appBaseUri, this.options.AppId).ToAttachment(Strings.PendingTicketUserNotification, message.LocalTimestamp) },
                    };
                    break;

                case ChangeTicketStatusPayload.ResolveAction:
                    smeNotification = string.Format(CultureInfo.InvariantCulture, Strings.SMEClosedStatus, ticket.LastModifiedByName);

                    updateUserCardActivity = new Activity(ActivityTypes.Message)
                    {
                        Id = ticket.RequesterCardActivityId,
                        Conversation = new ConversationAccount { Id = ticket.RequesterConversationId },
                        Attachments = new List<Attachment> { new UserNotificationCard(ticket, this.appBaseUri, this.options.AppId).ToAttachment(Strings.ClosedTicketUserNotification, message.LocalTimestamp) },
                    };
                    break;

                case ChangeTicketStatusPayload.AssignToSelfAction:
                    smeNotification = string.Format(CultureInfo.InvariantCulture, Strings.SMEAssignedStatus, ticket.AssignedToName);
                    if (previousState == (int)TicketState.Resolved)
                    {
                        updateUserCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = ticket.RequesterCardActivityId,
                            Conversation = new ConversationAccount { Id = ticket.RequesterConversationId },
                            Attachments = new List<Attachment> { new UserNotificationCard(ticket, this.appBaseUri, this.options.AppId).ToAttachment(Strings.ReopenedTicketUserNotification, message.LocalTimestamp) },
                        };
                    }
                    else if (previousState == (int)TicketState.Assigned)
                    {
                        updateUserCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = ticket.RequesterCardActivityId,
                            Conversation = new ConversationAccount { Id = ticket.RequesterConversationId },
                            Attachments = new List<Attachment> { new UserNotificationCard(ticket, this.appBaseUri, this.options.AppId).ToAttachment(Strings.ReAssigneTicketUserNotification, message.LocalTimestamp) },
                        };
                    }
                    else
                    {
                        updateUserCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = ticket.RequesterCardActivityId,
                            Conversation = new ConversationAccount { Id = ticket.RequesterConversationId },
                            Attachments = new List<Attachment> { new UserNotificationCard(ticket, this.appBaseUri, this.options.AppId).ToAttachment(Strings.AssignedTicketUserNotification, message.LocalTimestamp) },
                        };
                    }

                    break;
                case ChangeTicketStatusPayload.AssignToOthersAction:

                    // @ assignee
                    var mention = await this.MentionSomeoneByName(turnContext, cancellationToken, ticket.AssignedToName).ConfigureAwait(false);
                    if (mention != null)
                    {
                        var replyActivity = MessageFactory.Text(string.Format(CultureInfo.InvariantCulture, Strings.SMEAssignedByOthersStatus, mention.Text, message.From.Name));
                        replyActivity.Entities = new List<Entity> { mention };

                        await turnContext.SendActivityAsync(replyActivity, cancellationToken);
                    }

                    if (previousState == (int)TicketState.Resolved)
                    {
                        updateUserCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = ticket.RequesterCardActivityId,
                            Conversation = new ConversationAccount { Id = ticket.RequesterConversationId },
                            Attachments = new List<Attachment> { new UserNotificationCard(ticket, this.appBaseUri, this.options.AppId).ToAttachment(Strings.ReopenedTicketUserNotification, message.LocalTimestamp) },
                        };
                    }
                    else if (previousState == (int)TicketState.Assigned)
                    {
                        updateUserCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = ticket.RequesterCardActivityId,
                            Conversation = new ConversationAccount { Id = ticket.RequesterConversationId },
                            Attachments = new List<Attachment> { new UserNotificationCard(ticket, this.appBaseUri, this.options.AppId).ToAttachment(Strings.ReAssigneTicketUserNotification, message.LocalTimestamp) },
                        };
                    }
                    else
                    {
                        updateUserCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = ticket.RequesterCardActivityId,
                            Conversation = new ConversationAccount { Id = ticket.RequesterConversationId },
                            Attachments = new List<Attachment> { new UserNotificationCard(ticket, this.appBaseUri, this.options.AppId).ToAttachment(Strings.AssignedTicketUserNotification, message.LocalTimestamp) },
                        };
                    }

                    break;
            }
            if (!string.IsNullOrEmpty(smeNotification))
            {
                var smeResponse = await turnContext.SendActivityAsync(smeNotification).ConfigureAwait(false);
                this.logger.LogInformation($"SME team notified of update to ticket {ticket.TicketId}, activityId = {smeResponse.Id}");
            }

            if (updateUserCardActivity != null)
            {
                var userResponse = await turnContext.Adapter.UpdateActivityAsync(turnContext, updateUserCardActivity, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation($"Update user ticket {ticket.TicketId}, activityId = {userResponse?.Id}");

                IMessageActivity userNotification = null;
                userNotification = MessageFactory.Text($"Your request [{ticket.TicketId.Substring(0, 8)}] updated to status: {CardHelper.GetUserTicketDisplayStatus(ticket)} ");
                userNotification.Conversation = updateUserCardActivity.Conversation;
                var userNotificationResponse = await turnContext.Adapter.SendActivitiesAsync(turnContext, new Activity[] { (Activity)userNotification }, cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation($"User notified of update to ticket {ticket.TicketId}, activityId = {userNotificationResponse.FirstOrDefault()?.Id}");

            }

            // record user action
            if (userAction.Action != nameof(UserActionType.NotDefined))
            {
                await this.userActionProvider.UpsertUserActionAsync(userAction).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Send typing indicator to the user.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task SendTypingIndicatorAsync(ITurnContext turnContext)
        {
            try
            {
                var typingActivity = turnContext.Activity.CreateReply();
                typingActivity.Type = ActivityTypes.Typing;
                await turnContext.SendActivityAsync(typingActivity);
            }
            catch (Exception ex)
            {
                // Do not fail on errors sending the typing indicator
                this.logger.LogWarning(ex, "Failed to send a typing indicator");
            }
        }

        /// <summary>
        /// Send the given attachment to the specified team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cardToSend">The card to send.</param>
        /// <param name="teamId">Team id to which the message is being sent.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns><see cref="Task"/>That resolves to a <see cref="ConversationResourceResponse"/>Send a attachemnt.</returns>
        private async Task<ConversationResourceResponse> SendCardToTeamAsync(
            ITurnContext turnContext,
            Attachment cardToSend,
            string teamId,
            CancellationToken cancellationToken)
        {
            var conversationParameters = new ConversationParameters
            {
                Activity = (Activity)MessageFactory.Attachment(cardToSend),
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo(teamId) },
            };

            var taskCompletionSource = new TaskCompletionSource<ConversationResourceResponse>();
            await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                null,       // If we set channel = "msteams", there is an error as preinstalled middleware expects ChannelData to be present.
                turnContext.Activity.ServiceUrl,
                this.microsoftAppCredentials,
                conversationParameters,
                (newTurnContext, newCancellationToken) =>
                {
                    var activity = newTurnContext.Activity;
                    taskCompletionSource.SetResult(new ConversationResourceResponse
                    {
                        Id = activity.Conversation.Id,
                        ActivityId = activity.Id,
                        ServiceUrl = activity.ServiceUrl,
                    });
                    return Task.CompletedTask;
                },
                cancellationToken).ConfigureAwait(false);

            return await taskCompletionSource.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Verify if the tenant Id in the message is the same tenant Id used when application was configured.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <returns>Boolean value where true represent tenant is valid while false represent tenant in not valid.</returns>
        private bool IsActivityFromExpectedTenant(ITurnContext turnContext)
        {
            return turnContext.Activity.Conversation.TenantId == this.options.TenantId;
        }

        /// <summary>
        /// Get the reply to a question asked by end user.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="message">Text message.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task GetQuestionAnswerReplyAsync(
            ITurnContext<IMessageActivity> turnContext,
            IMessageActivity message,
            CancellationToken cancellationToken)
        {
            string text = message.Text?.ToLower()?.Trim() ?? string.Empty;
            ConversationEntity conInfo = new ConversationEntity();
            conInfo.ConversationID = Guid.NewGuid().ToString();
            conInfo.Question = text;

            try
            {
                var queryResult = new QnASearchResultList();

                ResponseCardPayload payload = new ResponseCardPayload();

                if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null))
                {
                    payload = ((JObject)message.Value).ToObject<ResponseCardPayload>();
                }

                queryResult = await this.qnaServiceProvider.GenerateAnswerAsync(question: text, isTestKnowledgeBase: false, payload.PreviousQuestions?.Last().Id.ToString(), payload.PreviousQuestions?.Last().Questions.First(), payload.QnAID).ConfigureAwait(false);

                if (queryResult.Answers.First().Id != -1)
                {
                    var answerData = queryResult.Answers.First();

                    conInfo.QnAID = answerData.Id.ToString();
                    conInfo.Answer = answerData.Answer;
                    conInfo.Score = answerData.Score.ToString();
                    conInfo.Project = (from r in answerData.Metadata where r.Name.Equals("project") select r).FirstOrDefault()?.Value;
                    conInfo.PreviousQnAID = payload.PreviousQuestions?.Last().Id.ToString();

                    StringBuilder sb = new StringBuilder();
                    if (answerData?.Context.Prompts.Count > 0)
                    {
                        foreach (var item in answerData.Context.Prompts)
                        {
                            sb.Append($"[{item.DisplayText}]");
                        }
                    }

                    conInfo.Prompts = sb.ToString();

                    AnswerModel answerModel = new AnswerModel();

                    if (Validators.IsValidJSON(answerData.Answer))
                    {
                        answerModel = JsonConvert.DeserializeObject<AnswerModel>(answerData.Answer);
                    }

                    if (!string.IsNullOrEmpty(answerModel?.Title) || !string.IsNullOrEmpty(answerModel?.Subtitle) || !string.IsNullOrEmpty(answerModel?.ImageUrl) || !string.IsNullOrEmpty(answerModel?.RedirectionUrl))
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(MessagingExtensionQnaCard.GetEndUserRichCard(text, answerData))).ConfigureAwait(false);
                    }
                    else
                    {
                        bool isChitChat = (from r in answerData.Metadata where r.Value == "chitchat" select r).FirstOrDefault() == null ? false : true;
                        if (isChitChat)
                        {
                            await turnContext.SendActivityAsync(answerData.Answer).ConfigureAwait(false);
                        }
                        else
                        {
                            await turnContext.SendActivityAsync(MessageFactory.Attachment(ResponseCard.GetCard(answerData, text, this.appBaseUri, payload))).ConfigureAwait(false);
                        }
                    }
                }
                else
                {
                    var conPro = await this.GetConversationInfoAsync(turnContext, cancellationToken);
                    conPro.ContinousFailureTimes++;

                    bool isRecommended = false;
                    if (conPro.ContinousFailureTimes >= this.recommendConfiguration.RecommendationContinousFailureTimes && DateTime.Now > conPro.LastRecommendTime.AddMinutes(this.recommendConfiguration.RecommendationIntervalInMinutes))
                    {
                        var conList = await this.GetRecommendQuestionsAsync();
                        if (conList.Count > 0)
                        {
                            isRecommended = true;

                            // Send recommend
                            await turnContext.SendActivityAsync(MessageFactory.Attachment(RecommendCard.GetCard(conList, this.options.AppId, this.appBaseUri))).ConfigureAwait(false);

                            // Save user action
                            UserActionEntity userAction = await this.GeneratePersonalActionEntityAsync(turnContext, cancellationToken);
                            userAction.Action = nameof(UserActionType.Recommended);
                            userAction.Remark = text;
                            await this.userActionProvider.UpsertUserActionAsync(userAction).ConfigureAwait(false);

                            conPro.ContinousFailureTimes = 0;
                            conPro.LastRecommendTime = DateTime.Now;
                        }
                    }

                    if (!isRecommended)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInputCard.GetCard(text, this.appBaseUri))).ConfigureAwait(false);
                    }
                }

                var userDetails = await AdaptiveCardHelper.GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken).ConfigureAwait(false);
                conInfo.UserName = userDetails.Name;
                conInfo.UserPrincipalName = userDetails.UserPrincipalName;

                await this.conversationProvider.UpsertConversationAsync(conInfo).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Check if knowledge base is empty and has not published yet when end user is asking a question to bot.
                if (((ErrorResponseException)ex).Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var knowledgeBaseId = await this.configurationProvider.GetSavedEntityDetailAsync(Constants.KnowledgeBaseEntityId).ConfigureAwait(false);
                    var hasPublished = await this.qnaServiceProvider.GetInitialPublishedStatusAsync(knowledgeBaseId).ConfigureAwait(false);

                    // Check if knowledge base has not published yet.
                    if (!hasPublished)
                    {
                        this.logger.LogError(ex, "Error while fetching the qna pair: knowledge base may be empty or it has not published yet.");
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInputCard.GetCard(text, this.appBaseUri))).ConfigureAwait(false);
                        return;
                    }
                }

                // Throw the error at calling place, if there is any generic exception which is not caught.
                throw;
            }
        }

        /// <summary>
        /// Get new personl user action entity.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>user action entity.</returns>
        private async Task<UserActionEntity> GeneratePersonalActionEntityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            UserActionEntity userAction = new UserActionEntity();
            var userDetails = await AdaptiveCardHelper.GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken).ConfigureAwait(false);
            userAction.UserPrincipalName = userDetails.UserPrincipalName;
            userAction.UserName = userDetails.Name;
            userAction.UserActionId = Guid.NewGuid().ToString();
            userAction.Action = nameof(UserActionType.NotDefined);
            return userAction;
        }

        private UserActionEntity GenerateChannelAction()
        {
            UserActionEntity userAction = new UserActionEntity();
            userAction.UserActionId = Guid.NewGuid().ToString();
            userAction.Action = nameof(UserActionType.NotDefined);
            return userAction;
        }

        /// <summary>
        /// Get members information from channel.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns> account list of users.</returns>
        private async Task<List<TeamsChannelAccount>> GetTeamsChannelMembers(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var members = new List<TeamsChannelAccount>();
            string continuationToken = null;

            do
            {
                var currentPage = await TeamsInfo.GetPagedMembersAsync(turnContext, 100, continuationToken, cancellationToken);
                continuationToken = currentPage.ContinuationToken;
                members = members.Concat(currentPage.Members).ToList();
            }
            while (continuationToken != null);
            return members;
        }

        private async Task SaveChannelMembers(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var accounts = await this.GetTeamsChannelMembers(turnContext, cancellationToken).ConfigureAwait(false);
            List<ExpertEntity> experts = new List<ExpertEntity>();
            foreach (TeamsChannelAccount account in accounts)
            {
                experts.Add(new ExpertEntity()
                {
                    ID = account.AadObjectId,
                    Name = account.Name,
                    GivenName = account.GivenName,
                    Surname = account.Surname,
                    Email = account.Email,
                    UserPrincipalName = account.UserPrincipalName,
                    TenantId = account.TenantId,
                    UserRole = account.UserRole,
                });
            }

            await this.expertProvider.UpserExpertsAsync(experts);
        }

        /// <summary>
        /// Create mention by user name.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="name">member name.</param>
        /// <returns>a mention entity.</returns>
        private async Task<Mention> MentionSomeoneByName(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken, string name)
        {
            var members = await this.GetTeamsChannelMembers(turnContext, cancellationToken);
            foreach (TeamsChannelAccount member in members)
            {
                if (member.Name.Equals(name))
                {
                    var mention = new Mention
                    {
                        Mentioned = member,
                        Text = $"<at>{XmlConvert.EncodeName(member.Name)}</at>",
                    };
                    return mention;
                }
            }

            return null;
        }

        /// <summary>
        /// Get current conversation info.
        /// </summary>
        /// <returns>conversation info.</returns>
        private async Task<ConversationProperty> GetConversationInfoAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var conversationStateAccessors = this.conversationState.CreateProperty<ConversationProperty>(nameof(ConversationProperty));
            var conProperty = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationProperty(), cancellationToken);
            return conProperty;
        }

        /// <summary>
        /// Get recommend question list.
        /// </summary>
        /// <returns>recommend 6 question list.</returns>
        private async Task<List<string>> GetRecommendQuestionsAsync()
        {
            var ceList = await this.conversationProvider.GetRecentAskedQnAListAsync(30);
            Dictionary<string, int> idCountDic = new Dictionary<string, int>();
            foreach (ConversationEntity ce in ceList)
            {
                if (!idCountDic.ContainsKey(ce.Question))
                {
                    idCountDic.Add(ce.Question, 1);
                }
                else
                {
                    idCountDic[ce.Question]++;
                }
            }

            List<string> result = new List<string>();

            var list = idCountDic.OrderByDescending(r => r.Value).ToList();
            int suggestionCount = list.Count > 6 ? 6 : list.Count;
            int randomRange = list.Count > 15 ? 15 : list.Count - 1;

            List<int> listNumbers = new List<int>();
            int number;
            Random ran = new Random();
            for (int i = 0; i < suggestionCount; i++)
            {
                do
                {
                    number = ran.Next(0, randomRange);
                }
                while (listNumbers.Contains(number));
                listNumbers.Add(number);
            }

            foreach (int i in listNumbers)
            {
                result.Add(QnaHelper.CapitalizeString(list[i].Key));
            }

            return result;
        }

        /// <summary>
        ///  Is information of Teams where this app installed updated.
        /// </summary>
        /// <param name="activity">activity.</param>
        /// <returns>true or false.</returns>
        private bool IsTeamInformationUpdated(IConversationUpdateActivity activity)
        {
            if (activity == null)
            {
                return false;
            }

            var channelData = activity.GetChannelData<TeamsChannelData>();
            if (channelData == null)
            {
                return false;
            }

            return FaqPlusPlusBot.TeamRenamedEventType.Equals(channelData.EventType, StringComparison.OrdinalIgnoreCase);
        }
    }

}