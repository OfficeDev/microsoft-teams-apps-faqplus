// <copyright file="MessagingExtensionTicketsCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Implements messaging extension tickets card.
    /// </summary>
    public class MessagingExtensionTicketsCard : SmeTicketCard
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessagingExtensionTicketsCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket model with the latest details.</param>
        public MessagingExtensionTicketsCard(TicketEntity ticket)
            : base(ticket)
        {
        }

        /// <summary>
        /// Return the appropriate set of card actions based on the state and information in the ticket.
        /// </summary>
        /// <returns>Adaptive card actions.</returns>
        protected override List<AdaptiveAction> BuildActions()
        {
            List<AdaptiveAction> actions = new List<AdaptiveAction>();

            actions.Add(this.CreateChatWithUserAction());

            if (!string.IsNullOrEmpty(this.Ticket.SmeThreadConversationId))
            {
                actions.Add(
                    new AdaptiveOpenUrlAction
                    {
                        Title = Strings.GoToOriginalThreadButtonText,
                        Url = new Uri(CreateDeeplinkToThread(this.Ticket.SmeThreadConversationId)),
                    });
            }

            return actions;
        }

        /// <summary>
        /// Returns go to original thread uri which will help in opening the original conversation about the ticket.
        /// </summary>
        /// <param name="threadConversationId">The thread along with message Id stored in storage table.</param>
        /// <returns>Original thread uri.</returns>
        private static string CreateDeeplinkToThread(string threadConversationId)
        {
            string[] threadAndMessageId = threadConversationId.Split(";");
            var threadId = threadAndMessageId[0];
            var messageId = threadAndMessageId[1].Split("=")[1];
            return $"https://teams.microsoft.com/l/message/{threadId}/{messageId}";
        }
    }
}
