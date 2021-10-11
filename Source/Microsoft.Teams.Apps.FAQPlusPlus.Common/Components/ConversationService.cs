// <copyright file="ConversationService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Components
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class that handles sending welcome card and adaptive card in personal and team chat.
    /// </summary>
    public class ConversationService : IConversationService
    {
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly ITicketsProvider ticketsProvider;
        private readonly INotificationService notificationService;
        private readonly IQnAPairServiceFacade qnaPairServiceFacade;
        private readonly ILogger<ConversationService> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConversationService"/> class.
        /// </summary>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="qnaPairServiceFacade">Instance of QnA pair service class to call add/update/get QnA pair.</param>
        /// <param name="ticketsProvider">Instance of Ticket provider helps in fetching and storing information in storage table.</param>
        /// <param name="notificationService">Notifies in expert's Team chat.</param>
        public ConversationService(
            Common.Providers.IConfigurationDataProvider configurationProvider,
            IQnAPairServiceFacade qnaPairServiceFacade,
            ITicketsProvider ticketsProvider,
            INotificationService notificationService,
            ILogger<ConversationService> logger)
        {
            this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            this.ticketsProvider = ticketsProvider ?? throw new ArgumentNullException(nameof(ticketsProvider));
            this.notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.qnaPairServiceFacade = qnaPairServiceFacade ?? throw new ArgumentNullException(nameof(qnaPairServiceFacade));
        }

        /// <summary>
        /// Sends welcome card in 1:1 chat.
        /// </summary>
        /// <param name="membersAdded">Channel account information needed to route a message.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SendWelcomeCardInPersonalChatAsync(
            IList<ChannelAccount> membersAdded,
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var activity = turnContext.Activity;
            if (membersAdded.Any(channelAccount => channelAccount.Id == activity.Recipient.Id))
            {
                // User started chat with the bot in personal scope, for the first time.
                this.logger.LogInformation($"Bot added to 1:1 chat {activity.Conversation.Id}");
                var welcomeText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText).ConfigureAwait(false);
                var userWelcomeCardAttachment = WelcomeCard.GetCard(welcomeText);
                await turnContext.SendActivityAsync(MessageFactory.Attachment(userWelcomeCardAttachment), cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends welcome card in teams chat.
        /// </summary>
        /// <param name="membersAdded">Channel account information needed to route a message.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task SendWelcomeCardInTeamChatAsync(
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
                var teamWelcomeCardAttachment = WelcomeTeamCard.GetCard();
                await this.notificationService.NotifyInTeamChatAsync(turnContext, teamWelcomeCardAttachment, teamDetails.Team.Id, cancellationToken).ConfigureAwait(false);
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
        public async Task SendAdaptiveCardInPersonalChatAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            Attachment smeTeamCard = null;      // Notification to SME team
            Attachment userCard = null;         // Acknowledgement to the user
            TicketEntity newTicket = null;      // New ticket

            string text = (message.Text ?? string.Empty).Trim();

            switch (text)
            {
                // Sends user ask an expert card from the answer card.
                case Constants.AskAnExpert:
                    this.logger.LogInformation("Sending user ask an expert card (from answer)");
                    var askAnExpertPayload = ((JObject)message.Value).ToObject<ResponseCardPayload>();
                    await this.SendActivityInChatAsync(turnContext, MessageFactory.Attachment(AskAnExpertCard.GetCard(askAnExpertPayload)), cancellationToken);
                    break;

                // Sends user the feedback card from the answer card.
                case Constants.ShareFeedback:
                    this.logger.LogInformation("Sending user share feedback card (from answer)");
                    var shareFeedbackPayload = ((JObject)message.Value).ToObject<ResponseCardPayload>();
                    await this.SendActivityInChatAsync(turnContext, MessageFactory.Attachment(ShareFeedbackCard.GetCard(shareFeedbackPayload)), cancellationToken);
                    break;

                // User submits the ask an expert card.
                case Constants.AskAnExpertSubmitText:
                    this.logger.LogInformation("Received question for expert");
                    newTicket = await AdaptiveCardHelper.AskAnExpertSubmitText(message, turnContext, cancellationToken, this.ticketsProvider).ConfigureAwait(false);
                    if (newTicket != null)
                    {
                        smeTeamCard = new SmeTicketCard(newTicket).ToAttachment();
                        userCard = new UserNotificationCard(newTicket).ToAttachment(Strings.NotificationCardContent);
                    }

                    break;

                // User submits the share feedback card.
                case Constants.ShareFeedbackSubmitText:
                    this.logger.LogInformation("Received app feedback");
                    smeTeamCard = await AdaptiveCardHelper.ShareFeedbackSubmitText(message, turnContext, cancellationToken).ConfigureAwait(false);
                    if (smeTeamCard != null)
                    {
                        await this.SendActivityInChatAsync(turnContext, MessageFactory.Text(Strings.ThankYouTextContent), cancellationToken);
                    }

                    break;

                default:
                    var payload = ((JObject)message.Value).ToObject<ResponseCardPayload>();

                    if (payload.IsPrompt)
                    {
                        this.logger.LogInformation("Sending input to QnAMaker for prompt");
                        await this.qnaPairServiceFacade.GetReplyToQnAAsync(turnContext, message).ConfigureAwait(false);
                    }
                    else
                    {
                        this.logger.LogWarning($"Unexpected text in submit payload: {message.Text}");
                    }

                    break;
            }

            string expertTeamId = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId).ConfigureAwait(false);

            // Send message to SME team.
            if (smeTeamCard != null)
            {
                await this.SendTicketCardToSMETeamAsync(turnContext, smeTeamCard, expertTeamId, cancellationToken, newTicket);
            }

            // Send acknowledgment to the user
            if (userCard != null)
            {
                await this.SendActivityInChatAsync(turnContext, MessageFactory.Attachment(userCard), cancellationToken);
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
        public async Task SendAdaptiveCardInTeamChatAsync(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var payload = ((JObject)message.Value).ToObject<ChangeTicketStatusPayload>();
            this.logger.LogInformation($"Received submit: ticketId={payload.TicketId} action={payload.Action}");

            // Get the ticket from the data store.
            var ticket = await this.ticketsProvider.GetTicketAsync(payload.TicketId).ConfigureAwait(false);
            if (ticket == null)
            {
                await this.SendActivityInChatAsync(turnContext, MessageFactory.Text($"Ticket {payload.TicketId} was not found in the data store"), cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation($"Ticket {payload.TicketId} was not found in the data store");
                return;
            }

            // Update the ticket based on the payload.
            switch (payload.Action)
            {
                // Ticket reopen.
                case ChangeTicketStatusPayload.ReopenAction:
                    ticket.Status = (int)TicketState.Open;
                    ticket.DateAssigned = null;
                    ticket.AssignedToName = null;
                    ticket.AssignedToObjectId = null;
                    ticket.DateClosed = null;
                    break;

                // Ticket close.
                case ChangeTicketStatusPayload.CloseAction:
                    ticket.Status = (int)TicketState.Closed;
                    ticket.DateClosed = DateTime.UtcNow;
                    break;

                // Assign ticket to self.
                case ChangeTicketStatusPayload.AssignToSelfAction:
                    ticket.Status = (int)TicketState.Open;
                    ticket.DateAssigned = DateTime.UtcNow;
                    ticket.AssignedToName = message.From.Name;
                    ticket.AssignedToObjectId = message.From.AadObjectId;
                    ticket.DateClosed = null;
                    break;

                default:
                    this.logger.LogWarning($"Unknown status command {payload.Action}");
                    return;
            }

            ticket.LastModifiedByName = message.From.Name;
            ticket.LastModifiedByObjectId = message.From.AadObjectId;
            await this.ticketsProvider.UpsertTicketAsync(ticket).ConfigureAwait(false);
            this.logger.LogInformation($"Ticket {ticket.TicketId} updated to status ({ticket.Status}, {ticket.AssignedToObjectId}) in store");

            // Update the card in the SME team.
            var updateCardActivity = new Activity(ActivityTypes.Message)
            {
                Id = ticket.SmeCardActivityId,
                Conversation = new ConversationAccount { Id = ticket.SmeThreadConversationId },
                Attachments = new List<Attachment> { new SmeTicketCard(ticket).ToAttachment() },
            };
            var updateResponse = await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
            this.logger.LogInformation($"Card for ticket {ticket.TicketId} updated to status ({ticket.Status}, {ticket.AssignedToObjectId}), activityId = {updateResponse.Id}");

            // Post update to user and SME team thread.
            string smeNotification = null;
            IMessageActivity userNotification = null;
            switch (payload.Action)
            {
                case ChangeTicketStatusPayload.ReopenAction:
                    smeNotification = string.Format(CultureInfo.InvariantCulture, Strings.SMEOpenedStatus, message.From.Name);

                    userNotification = MessageFactory.Attachment(new UserNotificationCard(ticket).ToAttachment(Strings.ReopenedTicketUserNotification));
                    userNotification.Summary = Strings.ReopenedTicketUserNotification;
                    break;

                case ChangeTicketStatusPayload.CloseAction:
                    smeNotification = string.Format(CultureInfo.InvariantCulture, Strings.SMEClosedStatus, ticket.LastModifiedByName);

                    userNotification = MessageFactory.Attachment(new UserNotificationCard(ticket).ToAttachment(Strings.ClosedTicketUserNotification));
                    userNotification.Summary = Strings.ClosedTicketUserNotification;
                    break;

                case ChangeTicketStatusPayload.AssignToSelfAction:
                    smeNotification = string.Format(CultureInfo.InvariantCulture, Strings.SMEAssignedStatus, ticket.AssignedToName);

                    userNotification = MessageFactory.Attachment(new UserNotificationCard(ticket).ToAttachment(Strings.AssignedTicketUserNotification));
                    userNotification.Summary = Strings.AssignedTicketUserNotification;
                    break;
            }

            if (!string.IsNullOrEmpty(smeNotification))
            {
                var smeResponse = await this.SendActivityInChatAsync(turnContext, MessageFactory.Text(smeNotification), cancellationToken).ConfigureAwait(false);
                this.logger.LogInformation($"SME team notified of update to ticket {ticket.TicketId}, activityId = {smeResponse.Id}");
            }

            if (userNotification != null)
            {
                await this.SendAckCardToPersonalChatAsync(turnContext, userNotification, ticket.RequesterConversationId, ticket.TicketId);
            }
        }

        /// <summary>
        /// Sends ticket card in SME team chat.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="smeTeamCard">Card to be sent to SME team.</param>
        /// <param name="expertTeamId">Expert team's id.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="ticket">Ticket that has to be updated with conversation info.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task SendTicketCardToSMETeamAsync(
            ITurnContext<IMessageActivity> turnContext,
            Attachment smeTeamCard,
            string expertTeamId,
            CancellationToken cancellationToken,
            TicketEntity ticket)
        {
            var resourceResponse = await this.notificationService.NotifyInTeamChatAsync(turnContext, smeTeamCard, expertTeamId, cancellationToken).ConfigureAwait(false);

            // If a ticket was created, update the ticket with the conversation info.
            if (ticket != null)
            {
                ticket.SmeCardActivityId = resourceResponse.ActivityId;
                ticket.SmeThreadConversationId = resourceResponse.Id;
                await this.ticketsProvider.UpsertTicketAsync(ticket).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Sends acknowledgement card in 1:1 chat.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="userNotification">Message activity formed by user notification card.</param>
        /// <param name="conversationId">Conversation id of user.</param>
        /// <param name="ticketId">Ticket id.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task SendAckCardToPersonalChatAsync(
            ITurnContext<IMessageActivity> turnContext,
            IMessageActivity userNotification,
            string conversationId,
            string ticketId)
        {
            await this.notificationService.NotifyInPersonalChatAsync(turnContext, userNotification, conversationId);
            this.logger.LogInformation($"User notified of update to ticket {ticketId}");
        }

        /// <summary>
        /// Sends activity in chat.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="activity">Activity to be sent in chat</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private async Task<ResourceResponse> SendActivityInChatAsync(
            ITurnContext<IMessageActivity> turnContext,
            IActivity activity,
            CancellationToken cancellationToken)
        {
            return await turnContext.SendActivityAsync(activity, cancellationToken).ConfigureAwait(false);
        }
    }
}
