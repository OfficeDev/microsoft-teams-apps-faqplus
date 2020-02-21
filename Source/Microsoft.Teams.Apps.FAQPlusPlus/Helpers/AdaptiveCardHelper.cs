// <copyright file="AdaptiveCardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Adaptive card helper class for tickets.
    /// </summary>
    public static class AdaptiveCardHelper
    {
        /// <summary>
        /// Helps to get the expert submit card.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="ticketsProvider">Tickets Provider.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task<TicketEntity> AskAnExpertSubmitText(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken,
            ITicketsProvider ticketsProvider)
        {
            var askAnExpertSubmitTextPayload = ((JObject)message.Value).ToObject<AskAnExpertCardPayload>();

            // Validate required fields.
            if (string.IsNullOrWhiteSpace(askAnExpertSubmitTextPayload?.Title))
            {
                var updateCardActivity = new Activity(ActivityTypes.Message)
                {
                    Id = turnContext.Activity.ReplyToId,
                    Conversation = turnContext.Activity.Conversation,
                    Attachments = new List<Attachment> { AskAnExpertCard.GetCard(askAnExpertSubmitTextPayload) },
                };
                await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
                return null;
            }

            var userDetails = await GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return await CreateTicketAsync(message, askAnExpertSubmitTextPayload, userDetails, ticketsProvider).ConfigureAwait(false);
        }

        /// <summary>
        /// Helps to get the expert submit card.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task<Attachment> ShareFeedbackSubmitText(
            IMessageActivity message,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            var shareFeedbackSubmitTextPayload = ((JObject)message.Value).ToObject<ShareFeedbackCardPayload>();

            // Validate required fields.
            if (!Enum.TryParse(shareFeedbackSubmitTextPayload?.Rating, out FeedbackRating rating))
            {
                var updateCardActivity = new Activity(ActivityTypes.Message)
                {
                    Id = turnContext.Activity.ReplyToId,
                    Conversation = turnContext.Activity.Conversation,
                    Attachments = new List<Attachment> { ShareFeedbackCard.GetCard(shareFeedbackSubmitTextPayload) },
                };
                await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
                return null;
            }

            var teamsUserDetails = await GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return SmeFeedbackCard.GetCard(shareFeedbackSubmitTextPayload, teamsUserDetails);
        }

        /// <summary>
        /// Create a new ticket from the input.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="data">Represents the submit data associated with the Ask An Expert card.</param>
        /// <param name="member">Teams channel account detailing user Azure Active Directory details.</param>
        /// <param name="ticketsProvider">Tickets Provider.</param>
        /// <returns>TicketEntity object.</returns>
        private static async Task<TicketEntity> CreateTicketAsync(
            IMessageActivity message,
            AskAnExpertCardPayload data,
            TeamsChannelAccount member,
            ITicketsProvider ticketsProvider)
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
                KnowledgeBaseAnswer = data.KnowledgeBaseAnswer,
            };

            await ticketsProvider.UpsertTicketAsync(ticketEntity).ConfigureAwait(false);

            return ticketEntity;
        }

        /// <summary>
        /// Get the account details of the user in a 1:1 chat with the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        private static async Task<TeamsChannelAccount> GetUserDetailsInPersonalChatAsync(
          ITurnContext<IMessageActivity> turnContext,
          CancellationToken cancellationToken)
        {
            var members = await ((BotFrameworkAdapter)turnContext.Adapter).GetConversationMembersAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TeamsChannelAccount>(JsonConvert.SerializeObject(members[0]));
        }
    }
}
