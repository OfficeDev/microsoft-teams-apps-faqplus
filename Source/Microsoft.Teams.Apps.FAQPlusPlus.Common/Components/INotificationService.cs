// <copyright file="INotificationService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Components
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;

    /// <summary>
    /// Notification interface.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Send notification in personal chat.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="userNotification">The card to send.</param>
        /// <param name="conversationId">Team id to which the message is being sent.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        Task NotifyInPersonalChatAsync(ITurnContext turnContext, IMessageActivity userNotification, string conversationId);

        /// <summary>
        /// Send notification to specified team. Send the given attachment to the specified team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cardToSend">The card to send.</param>
        /// <param name="teamId">Team id to which the message is being sent.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns><see cref="Task"/>That resolves to a <see cref="ConversationResourceResponse"/>Send a attachemnt.</returns>
        Task<ConversationResourceResponse> NotifyInTeamChatAsync(ITurnContext turnContext, Attachment cardToSend, string teamId, CancellationToken cancellationToken);
    }
}
