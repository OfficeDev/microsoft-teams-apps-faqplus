// <copyright file="NotificationService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Components
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Credentials;

    /// <summary>
    /// Class to send notification in 1:1 and team chat.
    /// </summary>
    public class NotificationService : INotificationService
    {
        private readonly ExpertAppCredentials expertAppCredentials;
        private readonly UserAppCredentials userAppCredentials;

        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationService"/> class.
        /// </summary>
        /// <param name="expertAppCredentials">Instance of class having expert bot credentials.</param>
        /// <param name="userAppCredentials">Instance of class having user bot credentials.</param>
        public NotificationService(
            ExpertAppCredentials expertAppCredentials,
            UserAppCredentials userAppCredentials)
        {
            this.expertAppCredentials = expertAppCredentials ?? throw new ArgumentNullException(nameof(expertAppCredentials));
            this.userAppCredentials = userAppCredentials ?? throw new ArgumentNullException(nameof(userAppCredentials));
        }

        /// <summary>
        /// Send notification in 1:1 chat.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="userNotification">The card to send.</param>
        /// <param name="conversationId">Team id to which the message is being sent.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task NotifyInPersonalChatAsync(ITurnContext turnContext, IMessageActivity userNotification, string conversationId)
        {
            userNotification.Conversation = new ConversationAccount { Id = conversationId };
            var connectorClient = new ConnectorClient(new Uri(turnContext.Activity.ServiceUrl), this.userAppCredentials.MicrosoftAppId, this.userAppCredentials.MicrosoftAppPassword);
            await connectorClient.Conversations.SendToConversationAsync((Activity)userNotification).ConfigureAwait(false);
        }

        /// <summary>
        /// Send the given attachment to the specified team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cardToSend">The card to send.</param>
        /// <param name="teamId">Team id to which the message is being sent.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns><see cref="Task"/>That resolves to a <see cref="ConversationResourceResponse"/>Send a attachemnt.</returns>
        public async Task<ConversationResourceResponse> NotifyInTeamChatAsync(ITurnContext turnContext, Attachment cardToSend, string teamId, CancellationToken cancellationToken)
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
                this.expertAppCredentials,
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
    }
}
