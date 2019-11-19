// <copyright file="FaqPlusPlusBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.AI.QnA;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Services;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Implements the core logic of the FAQ++ bot.
    /// </summary>
    public class FaqPlusPlusBot : ActivityHandler
    {
        // Commands supported by the bot

        /// <summary>
        /// TeamTour - text that triggers team tour action.
        /// </summary>
        public const string TeamTour = "team tour";

        /// <summary>
        /// TakeAtour - text that triggers take a tour action for the user.
        /// </summary>
        public const string TakeATour = "take a tour";

        /// <summary>
        /// AskAnExpert - text that renders the ask an expert card.
        /// </summary>
        public const string AskAnExpert = "ask an expert";

        /// <summary>
        /// Feedback - text that renders share feedback card.
        /// </summary>
        public const string ShareFeedback = "share feedback";

        private readonly TelemetryClient telemetryClient;
        private readonly IConfigurationProvider configurationProvider;
        private readonly MessagingExtension messageExtension;
        private readonly IQnAMakerFactory qnaMakerFactory;
        private readonly string appBaseUri;
        private readonly MicrosoftAppCredentials microsoftAppCredentials;
        private readonly ITicketsProvider ticketsProvider;
        private readonly string expectedTenantId;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaqPlusPlusBot"/> class.
        /// </summary>
        /// <param name="telemetryClient"> Telemetry Client.</param>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="qnaMakerFactory">QnAMaker factory instance</param>
        /// <param name="messageExtension">Messaging extension instance</param>
        /// <param name="appBaseUri">Base URI at which the app is served</param>
        /// <param name="expectedTenantId">The expected Tenant Id (from configuration)</param>
        /// <param name="microsoftAppCredentials">Microsoft app credentials to use</param>
        /// <param name="ticketsProvider">The tickets provider.</param>
        public FaqPlusPlusBot(
            TelemetryClient telemetryClient,
            IConfigurationProvider configurationProvider,
            IQnAMakerFactory qnaMakerFactory,
            MessagingExtension messageExtension,
            string appBaseUri,
            string expectedTenantId,
            MicrosoftAppCredentials microsoftAppCredentials,
            ITicketsProvider ticketsProvider)
        {
            this.telemetryClient = telemetryClient;
            this.configurationProvider = configurationProvider;
            this.qnaMakerFactory = qnaMakerFactory;
            this.messageExtension = messageExtension;
            this.appBaseUri = appBaseUri;
            this.microsoftAppCredentials = microsoftAppCredentials;
            this.ticketsProvider = ticketsProvider;
            this.expectedTenantId = expectedTenantId;
        }

        /// <inheritdoc/>
        public override Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!this.IsActivityFromExpectedTenant(turnContext))
            {
                this.telemetryClient.TrackTrace($"Unexpected tenant id {turnContext.Activity.Conversation.TenantId}", SeverityLevel.Warning);
                return Task.CompletedTask;
            }

            switch (turnContext.Activity.Type)
            {
                case ActivityTypes.Message:
                    return this.OnMessageActivityAsync(new DelegatingTurnContext<IMessageActivity>(turnContext), cancellationToken);

                case ActivityTypes.Invoke:
                    return this.OnInvokeActivityAsync(new DelegatingTurnContext<IInvokeActivity>(turnContext), cancellationToken);

                case ActivityTypes.ConversationUpdate:
                    return this.OnConversationUpdateActivityAsync(new DelegatingTurnContext<IConversationUpdateActivity>(turnContext), cancellationToken);

                default:
                    return base.OnTurnAsync(turnContext, cancellationToken);
            }
        }

        /// <inheritdoc/>
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var message = turnContext.Activity;

                this.telemetryClient.TrackTrace($"Received message activity");
                this.telemetryClient.TrackTrace($"from: {message.From?.Id}, conversation: {message.Conversation.Id}, replyToId: {message.ReplyToId}");

                await this.SendTypingIndicatorAsync(turnContext);

                switch (message.Conversation.ConversationType)
                {
                    case "personal":
                        await this.OnMessageActivityInPersonalChatAsync(message, turnContext, cancellationToken);
                        break;

                    case "channel":
                        await this.OnMessageActivityInChannelAsync(message, turnContext, cancellationToken);
                        break;

                    default:
                        this.telemetryClient.TrackTrace($"Received unexpected conversationType {message.Conversation.ConversationType}", SeverityLevel.Warning);
                        break;
                }
            }
            catch (Exception ex)
            {
                // TODO: Respond to the user with an error message
                this.telemetryClient.TrackTrace($"Error processing message: {ex.Message}", SeverityLevel.Error);
                this.telemetryClient.TrackException(ex);
                throw;
            }
        }

        /// <inheritdoc/>
        protected override async Task OnConversationUpdateActivityAsync(ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            try
            {
                var activity = turnContext.Activity;

                this.telemetryClient.TrackTrace($"Received conversationUpdate activity");
                this.telemetryClient.TrackTrace($"conversationType: {activity.Conversation.ConversationType}, membersAdded: {activity.MembersAdded?.Count()}, membersRemoved: {activity.MembersRemoved?.Count()}");

                if (activity.MembersAdded?.Count() > 0)
                {
                    switch (activity.Conversation.ConversationType)
                    {
                        case "personal":
                            await this.OnMembersAddedToPersonalChatAsync(activity.MembersAdded, turnContext, cancellationToken);
                            break;

                        case "channel":
                            await this.OnMembersAddedToTeamAsync(activity.MembersAdded, turnContext, cancellationToken);
                            break;

                        default:
                            this.telemetryClient.TrackTrace($"Ignoring event from conversation type {activity.Conversation.ConversationType}");
                            break;
                    }
                }
                else
                {
                    this.telemetryClient.TrackTrace($"Ignoring conversationUpdate that was not a membersAdded event");
                }
            }
            catch (Exception ex)
            {
                this.telemetryClient.TrackTrace($"Error processing conversationUpdate: {ex.Message}", SeverityLevel.Error);
                this.telemetryClient.TrackException(ex);
            }
        }

        // Handle members added conversationUpdate event in 1:1 chat
        private async Task OnMembersAddedToPersonalChatAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (membersAdded.Any(m => m.Id == activity.Recipient.Id))
            {
                // User started chat with the bot in personal scope, for the first time
                this.telemetryClient.TrackTrace($"Bot added to 1:1 chat {activity.Conversation.Id}");

                var welcomeText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText);
                var userWelcomeCardAttachment = WelcomeCard.GetCard(welcomeText);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment));
            }
        }

        // Handle members added conversationUpdate event in team
        private async Task OnMembersAddedToTeamAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (membersAdded.Any(m => m.Id == activity.Recipient.Id))
            {
                // Bot was added to a team
                this.telemetryClient.TrackTrace($"Bot added to team {activity.Conversation.Id}");

                var teamDetails = ((JObject)turnContext.Activity.ChannelData).ToObject<TeamsChannelData>();
                var botDisplayName = turnContext.Activity.Recipient.Name;
                var teamWelcomeCardAttachment = WelcomeTeamCard.GetCard();
                await this.SendCardToTeamAsync(turnContext, teamWelcomeCardAttachment, teamDetails.Team.Id, cancellationToken);
            }
        }

        // Handle message activity in 1:1 chat
        private async Task OnMessageActivityInPersonalChatAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null) && ((JObject)message.Value).HasValues)
            {
                this.telemetryClient.TrackTrace("Card submit in 1:1 chat");
                await this.OnAdaptiveCardSubmitInPersonalChatAsync(message, turnContext, cancellationToken);
                return;
            }

            string text = (message.Text ?? string.Empty).Trim().ToLower();

            switch (text)
            {
                case AskAnExpert:
                    this.telemetryClient.TrackTrace("Sending user ask an expert card");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(AskAnExpertCard.GetCard()));
                    break;

                case ShareFeedback:
                    this.telemetryClient.TrackTrace("Sending user feedback card");
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(ShareFeedbackCard.GetCard()));
                    break;

                case TakeATour:
                    this.telemetryClient.TrackTrace("Sending user tour card");
                    var userTourCards = TourCarousel.GetUserTourCards(this.appBaseUri);
                    await turnContext.SendActivityAsync(MessageFactory.Carousel(userTourCards));
                    break;

                default:
                    this.telemetryClient.TrackTrace("Sending input to QnAMaker");
                    var queryResult = await this.GetAnswerFromQnAMakerAsync(text, turnContext, cancellationToken);
                    if (queryResult != null)
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(ResponseCard.GetCard(queryResult.Questions[0], queryResult.Answer, text)));
                    }
                    else
                    {
                        await turnContext.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInputCard.GetCard(text)));
                    }

                    break;
            }
        }

        // Handle message activity in channel
        private async Task OnMessageActivityInChannelAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(message.ReplyToId) && (message.Value != null) && ((JObject)message.Value).HasValues)
            {
                this.telemetryClient.TrackTrace("Card submit in channel");
                await this.OnAdaptiveCardSubmitInChannelAsync(message, turnContext, cancellationToken);
                return;
            }

            string text = (message.Text ?? string.Empty).Trim().ToLower();

            switch (text)
            {
                case TeamTour:
                    this.telemetryClient.TrackTrace("Sending team tour card");
                    var teamTourCards = TourCarousel.GetTeamTourCards(this.appBaseUri);
                    await turnContext.SendActivityAsync(MessageFactory.Carousel(teamTourCards));
                    break;

                default:
                    this.telemetryClient.TrackTrace("Unrecognized input in channel");
                    var unrecognizedInputCard = UnrecognizedTeamInputCard.GetCard();
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(unrecognizedInputCard));
                    break;
            }
        }

        // Handle adaptive card submit in 1:1 chat
        // Submits the question or feedback to the SME team
        private async Task OnAdaptiveCardSubmitInPersonalChatAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            Attachment smeTeamCard = null;      // Notification to SME team
            Attachment userCard = null;         // Acknowledgement to the user
            TicketEntity newTicket = null;      // New ticket

            switch (message.Text)
            {
                case AskAnExpert:
                {
                    this.telemetryClient.TrackTrace("Sending user ask an expert card (from answer)");

                    var responseCardPayload = ((JObject)message.Value).ToObject<ResponseCardPayload>();
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(AskAnExpertCard.GetCard(responseCardPayload)));
                    break;
                }

                case ShareFeedback:
                {
                    this.telemetryClient.TrackTrace("Sending user share feedback card (from answer)");

                    var responseCardPayload = ((JObject)message.Value).ToObject<ResponseCardPayload>();
                    await turnContext.SendActivityAsync(MessageFactory.Attachment(ShareFeedbackCard.GetCard(responseCardPayload)));
                    break;
                }

                case AskAnExpertCard.AskAnExpertSubmitText:
                {
                    this.telemetryClient.TrackTrace($"Received question for expert");

                    var askAnExpertPayload = ((JObject)message.Value).ToObject<AskAnExpertCardPayload>();

                    // Validate required fields
                    if (string.IsNullOrWhiteSpace(askAnExpertPayload.Title))
                    {
                        var updateCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = turnContext.Activity.ReplyToId,
                            Conversation = turnContext.Activity.Conversation,
                            Attachments = new List<Attachment> { AskAnExpertCard.GetCard(askAnExpertPayload) },
                        };
                        await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken);
                        return;
                    }

                    var userDetails = await this.GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken);

                    newTicket = await this.CreateTicketAsync(message, askAnExpertPayload, userDetails);
                    smeTeamCard = new SmeTicketCard(newTicket).ToAttachment(message.LocalTimestamp);
                    userCard = new UserNotificationCard(newTicket).ToAttachment(Resource.NotificationCardContent, message.LocalTimestamp);
                    break;
                }

                case ShareFeedbackCard.ShareFeedbackSubmitText:
                {
                    this.telemetryClient.TrackTrace($"Received app feedback");

                    var shareFeedbackPayload = ((JObject)message.Value).ToObject<ShareFeedbackCardPayload>();

                    // Validate required fields
                    if (!Enum.TryParse(shareFeedbackPayload.Rating, out FeedbackRating rating))
                    {
                        var updateCardActivity = new Activity(ActivityTypes.Message)
                        {
                            Id = turnContext.Activity.ReplyToId,
                            Conversation = turnContext.Activity.Conversation,
                            Attachments = new List<Attachment> { ShareFeedbackCard.GetCard(shareFeedbackPayload) },
                        };
                        await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken);
                        return;
                    }

                    var userDetails = await this.GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken);

                    smeTeamCard = SmeFeedbackCard.GetCard(shareFeedbackPayload, userDetails);
                    await turnContext.SendActivityAsync(MessageFactory.Text(Resource.ThankYouTextContent));
                    break;
                }

                default:
                    this.telemetryClient.TrackTrace($"Unexpected text in submit payload: {message.Text}", SeverityLevel.Warning);
                    break;
            }

            // Send message to SME team
            if (smeTeamCard != null)
            {
                var channelId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId);
                var resourceResponse = await this.SendCardToTeamAsync(turnContext, smeTeamCard, channelId, cancellationToken);

                // If a ticket was created, update the ticket with the conversation info
                if (newTicket != null)
                {
                    newTicket.SmeCardActivityId = resourceResponse.ActivityId;
                    newTicket.SmeThreadConversationId = resourceResponse.Id;
                    await this.ticketsProvider.SaveOrUpdateTicketAsync(newTicket);
                }
            }

            // Send acknowledgment to the user
            if (userCard != null)
            {
                await turnContext.SendActivityAsync(MessageFactory.Attachment(userCard), cancellationToken);
            }
        }

        // Handle adaptive card submit in channel
        // Updates the ticket status based on the user submission
        private async Task OnAdaptiveCardSubmitInChannelAsync(IMessageActivity message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            var payload = ((JObject)message.Value).ToObject<ChangeTicketStatusPayload>();
            this.telemetryClient.TrackTrace($"Received submit: ticketId={payload.TicketId} action={payload.Action}");

            // Get the ticket from the data store
            var ticket = await this.ticketsProvider.GetTicketAsync(payload.TicketId);
            if (ticket == null)
            {
                // TODO: Send error message to the user
                this.telemetryClient.TrackTrace($"Ticket {payload.TicketId} was not found in the data store");
                return;
            }

            // Update the ticket based on the payload
            switch (payload.Action)
            {
                case ChangeTicketStatusPayload.ReopenAction:
                    ticket.Status = (int)TicketState.Open;
                    ticket.DateAssigned = null;
                    ticket.AssignedToName = null;
                    ticket.AssignedToObjectId = null;
                    ticket.DateClosed = null;
                    break;

                case ChangeTicketStatusPayload.CloseAction:
                    ticket.Status = (int)TicketState.Closed;
                    ticket.DateClosed = DateTime.UtcNow;
                    break;

                case ChangeTicketStatusPayload.AssignToSelfAction:
                    ticket.Status = (int)TicketState.Open;
                    ticket.DateAssigned = DateTime.UtcNow;
                    ticket.AssignedToName = message.From.Name;
                    ticket.AssignedToObjectId = message.From.AadObjectId;
                    ticket.DateClosed = null;
                    break;

                default:
                    // TODO: Show error message
                    this.telemetryClient.TrackTrace($"Unknown status command {payload.Action}", SeverityLevel.Warning);
                    return;
            }

            ticket.LastModifiedByName = message.From.Name;
            ticket.LastModifiedByObjectId = message.From.AadObjectId;

            await this.ticketsProvider.SaveOrUpdateTicketAsync(ticket);
            this.telemetryClient.TrackTrace($"Ticket {ticket.TicketId} updated to status ({ticket.Status}, {ticket.AssignedToObjectId}) in store");

            // Update the card in the SME team
            var updateCardActivity = new Activity(ActivityTypes.Message)
            {
                Id = ticket.SmeCardActivityId,
                Conversation = new ConversationAccount { Id = ticket.SmeThreadConversationId },
                Attachments = new List<Attachment> { new SmeTicketCard(ticket).ToAttachment(message.LocalTimestamp) },
            };
            var updateResponse = await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken);
            this.telemetryClient.TrackTrace($"Card for ticket {ticket.TicketId} updated to status ({ticket.Status}, {ticket.AssignedToObjectId}), activityId = {updateResponse.Id}");

            // Post update to user and SME team thread
            string smeNotification = null;
            IMessageActivity userNotification = null;
            switch (payload.Action)
            {
                case ChangeTicketStatusPayload.ReopenAction:
                    smeNotification = string.Format(Resource.SMEOpenedStatus, message.From.Name);

                    userNotification = MessageFactory.Attachment(new UserNotificationCard(ticket).ToAttachment(Resource.ReopenedTicketUserNotification, message.LocalTimestamp));
                    userNotification.Summary = Resource.ReopenedTicketUserNotification;
                    break;

                case ChangeTicketStatusPayload.CloseAction:
                    smeNotification = string.Format(Resource.SMEClosedStatus, ticket.LastModifiedByName);

                    userNotification = MessageFactory.Attachment(new UserNotificationCard(ticket).ToAttachment(Resource.ClosedTicketUserNotification, message.LocalTimestamp));
                    userNotification.Summary = Resource.ClosedTicketUserNotification;
                    break;

                case ChangeTicketStatusPayload.AssignToSelfAction:
                    smeNotification = string.Format(Resource.SMEAssignedStatus, ticket.AssignedToName);

                    userNotification = MessageFactory.Attachment(new UserNotificationCard(ticket).ToAttachment(Resource.AssignedTicketUserNotification, message.LocalTimestamp));
                    userNotification.Summary = Resource.AssignedTicketUserNotification;
                    break;
            }

            if (smeNotification != null)
            {
                var smeResponse = await turnContext.SendActivityAsync(smeNotification);
                this.telemetryClient.TrackTrace($"SME team notified of update to ticket {ticket.TicketId}, activityId = {smeResponse.Id}");
            }

            if (userNotification != null)
            {
                userNotification.Conversation = new ConversationAccount { Id = ticket.RequesterConversationId };
                var userResponse = await turnContext.Adapter.SendActivitiesAsync(turnContext, new Activity[] { (Activity)userNotification }, cancellationToken);
                this.telemetryClient.TrackTrace($"User notified of update to ticket {ticket.TicketId}, activityId = {userResponse.FirstOrDefault()?.Id}");
            }
        }

        // Get an answer from QnAMaker
        private async Task<QueryResult> GetAnswerFromQnAMakerAsync(string message, ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return null;
            }

            try
            {
                var kbId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.KnowledgeBaseId);
                if (string.IsNullOrEmpty(kbId))
                {
                    this.telemetryClient.TrackTrace("Knowledge base ID was not found in configuration table", SeverityLevel.Warning);
                    return null;
                }

                var endpointKey = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.QnAMakerEndpointKey);
                if (string.IsNullOrEmpty(endpointKey))
                {
                    this.telemetryClient.TrackTrace("QnAMaker endpoint key was not found in configuration table", SeverityLevel.Warning);
                    return null;
                }

                var qnaMaker = this.qnaMakerFactory.GetQnAMaker(kbId, endpointKey);

                var response = await qnaMaker.GetAnswersAsync(turnContext);
                this.telemetryClient.TrackTrace($"Received {response?.Count() ?? 0} answers from QnAMaker, with top score {response?.FirstOrDefault()?.Score ?? 0}");

                return response?.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // Per spec, treat errors getting a response from QnAMaker as if we got no results
                this.telemetryClient.TrackTrace($"Error getting answer from QnAMaker, will convert to no result: {ex.Message}");
                this.telemetryClient.TrackException(ex);
                return null;
            }
        }

        // Handle invoke activities received by the bot
        private async Task OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            var responseActivity = new Activity(ActivityTypesEx.InvokeResponse);

            switch (turnContext.Activity.Name)
            {
                case "composeExtension/query":
                    var invokeResponse = await this.messageExtension.HandleMessagingExtensionQueryAsync(turnContext).ConfigureAwait(false);
                    responseActivity.Value = invokeResponse;
                    break;

                default:
                    this.telemetryClient.TrackTrace($"Received invoke activity with unknown name {turnContext.Activity.Name}");
                    responseActivity.Value = new InvokeResponse { Status = 200 };
                    break;
            }

            await turnContext.SendActivityAsync(responseActivity).ConfigureAwait(false);
        }

        // Get the account details of the user in a 1:1 chat with the bot.
        private async Task<TeamsChannelAccount> GetUserDetailsInPersonalChatAsync(
          ITurnContext<IMessageActivity> turnContext,
          CancellationToken cancellationToken)
        {
            var members = await ((BotFrameworkAdapter)turnContext.Adapter).GetConversationMembersAsync(turnContext, cancellationToken);
            return JsonConvert.DeserializeObject<TeamsChannelAccount>(JsonConvert.SerializeObject(members[0]));
        }

        // Send typing indicator to the user.
        private Task SendTypingIndicatorAsync(ITurnContext turnContext)
        {
            var typingActivity = turnContext.Activity.CreateReply();
            typingActivity.Type = ActivityTypes.Typing;
            return turnContext.SendActivityAsync(typingActivity);
        }

        /// <summary>
        /// Send the given attachment to the specified team.
        /// </summary>
        /// <param name="turnContext">The current turn/execution flow.</param>
        /// <param name="cardToSend">The card to send.</param>
        /// <param name="teamId">Team id to which the message is being sent.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns><see cref="Task"/> that resolves to a <see cref="ConversationResourceResponse"/></returns>
        private async Task<ConversationResourceResponse> SendCardToTeamAsync(ITurnContext turnContext, Attachment cardToSend, string teamId, CancellationToken cancellationToken)
        {
            var conversationParameters = new ConversationParameters
            {
                Activity = (Activity)MessageFactory.Attachment(cardToSend),
                ChannelData = new TeamsChannelData { Channel = new ChannelInfo(teamId) },
            };

            var tcs = new TaskCompletionSource<ConversationResourceResponse>();
            await ((BotFrameworkAdapter)turnContext.Adapter).CreateConversationAsync(
                null,       // If we set channel = "msteams", there is an error as preinstalled middleware expects ChannelData to be present
                turnContext.Activity.ServiceUrl,
                this.microsoftAppCredentials,
                conversationParameters,
                (newTurnContext, newCancellationToken) =>
                {
                    var activity = newTurnContext.Activity;
                    tcs.SetResult(new ConversationResourceResponse
                    {
                        Id = activity.Conversation.Id,
                        ActivityId = activity.Id,
                        ServiceUrl = activity.ServiceUrl,
                    });
                    return Task.CompletedTask;
                },
                cancellationToken);

            return await tcs.Task;
        }

        // Create a new ticket from the input
        private async Task<TicketEntity> CreateTicketAsync(IMessageActivity message, AskAnExpertCardPayload data, TeamsChannelAccount member)
        {
            TicketEntity ticketEntity = new TicketEntity
            {
                TicketId = Guid.NewGuid().ToString(),
                Status = (int)TicketState.Open,
                DateCreated = DateTime.UtcNow,
                Title = data.Title,
                Description = data.Description,
                RequesterName = member.Name,
                RequesterUserPrincipalName = member.UserPrincipalName,
                RequesterGivenName = member.GivenName,
                RequesterConversationId = message.Conversation.Id,
                LastModifiedByName = message.From.Name,
                LastModifiedByObjectId = message.From.AadObjectId,
                UserQuestion = data.UserQuestion,
                KnowledgeBaseAnswer = data.KnowledgeBaseAnswer
            };

            await this.ticketsProvider.SaveOrUpdateTicketAsync(ticketEntity);

            return ticketEntity;
        }

        // Verify if the tenant Id in the message is the same tenant Id used when application was configured
        private bool IsActivityFromExpectedTenant(ITurnContext turnContext)
        {
            return turnContext.Activity.Conversation.TenantId == this.expectedTenantId;
        }
    }
}