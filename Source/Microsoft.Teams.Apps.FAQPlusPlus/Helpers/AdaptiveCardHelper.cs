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
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
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
            if (string.IsNullOrWhiteSpace(askAnExpertSubmitTextPayload?.Title) || askAnExpertSubmitTextPayload.Description?.Length > 500)
            {
                var updateCardActivity = new Activity(ActivityTypes.Message)
                {
                    Id = turnContext.Activity.ReplyToId,
                    Conversation = turnContext.Activity.Conversation,
                    Attachments = new List<Attachment> { AskAnExpertCard.GetCard(askAnExpertSubmitTextPayload) },
                };
                try
                {
                    await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
                }
                catch (ErrorResponseException)
                {
                    await turnContext.SendActivityAsync(Strings.InputTooLongWarning).ConfigureAwait(false);
                }

                return null;
            }

            var userDetails = await GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return await CreateTicketAsync(message, askAnExpertSubmitTextPayload, userDetails, ticketsProvider).ConfigureAwait(false);
        }

        /// <summary>
        /// Helps to get the expert submit card.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <param name="feedbackProvider">Feedback Provider.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task<FeedbackEntity> ShareFeedbackSubmitText(
            IMessageActivity message,
            string appBaseUri,
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken,
            IFeedbackProvider feedbackProvider)
        {
            var shareFeedbackSubmitTextPayload = ((JObject)message.Value).ToObject<ShareFeedbackCardPayload>();

            // Validate required fields.
            if (!Enum.TryParse(shareFeedbackSubmitTextPayload?.Rating, out FeedbackRating rating) || shareFeedbackSubmitTextPayload.DescriptionNotHelpful?.Length > 500)
            {
                var updateCardActivity = new Activity(ActivityTypes.Message)
                {
                    Id = turnContext.Activity.ReplyToId,
                    Conversation = turnContext.Activity.Conversation,
                    Attachments = new List<Attachment> { ShareFeedbackCard.GetCard(shareFeedbackSubmitTextPayload, appBaseUri) },
                };
                try
                {
                    await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
                }
                catch (ErrorResponseException)
                {
                    await turnContext.SendActivityAsync(Strings.InputTooLongWarning).ConfigureAwait(false);
                }

                return null;
            }

            var teamsUserDetails = await GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return await CreateFeedbackAsync(message, shareFeedbackSubmitTextPayload, teamsUserDetails, feedbackProvider).ConfigureAwait(false);
        }

        /// <summary>
        /// Get the account details of the user in a 1:1 chat with the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task<TeamsChannelAccount> GetUserDetailsInPersonalChatAsync(
          ITurnContext<IMessageActivity> turnContext,
          CancellationToken cancellationToken)
        {
            var members = await ((BotFrameworkAdapter)turnContext.Adapter).GetConversationMembersAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TeamsChannelAccount>(JsonConvert.SerializeObject(members[0]));
        }

        /// <summary>
        /// Get the account details of the user in a 1:1 chat with the bot.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task<TeamsChannelAccount> GetUserDetailsInPersonalChatAsync(
          ITurnContext turnContext,
          CancellationToken cancellationToken)
        {
            var members = await ((BotFrameworkAdapter)turnContext.Adapter).GetConversationMembersAsync(turnContext, cancellationToken).ConfigureAwait(false);
            return JsonConvert.DeserializeObject<TeamsChannelAccount>(JsonConvert.SerializeObject(members[0]));
        }

        /// <summary>
        /// Create a new Feedback entity from the input.
        /// </summary>
        /// <param name="message">A message in a conversation.</param>
        /// <param name="data">Represents the submit data associated with the Share feedback card.</param>
        /// <param name="member">Teams channel account detailing user Azure Active Directory details.</param>
        /// <param name="feedbackProvider">Tickets Provider.</param>
        /// <returns>TicketEntity object.</returns>
        private static async Task<FeedbackEntity> CreateFeedbackAsync(
            IMessageActivity message,
            ShareFeedbackCardPayload data,
            TeamsChannelAccount member,
            IFeedbackProvider feedbackProvider)
        {
            string description = null;
            string rating = data.Rating;

            if (data.Rating == nameof(FeedbackRating.NotHelpful))
            {
                description = data.DescriptionNotHelpful;
            }

            // if is ticket feedback
            if (!string.IsNullOrEmpty(data.TicketId))
            {
                switch (data.Rating)
                {
                    case nameof(FeedbackRating.Helpful):
                        rating = nameof(TicketSatisficationRating.Satisfied);
                        break;
                    case nameof(FeedbackRating.NotHelpful):
                        rating = nameof(TicketSatisficationRating.Disappointed);
                        break;
                }
            }

            FeedbackEntity feedbackEntity = new FeedbackEntity
            {
                FeedbackId = Guid.NewGuid().ToString(),
                UserPrincipalName = member.UserPrincipalName,
                UserName = member.Name,
                UserGivenName = member.GivenName,
                Rating = rating,
                Description = description,
                UserQuestion = data.UserQuestion,
                KnowledgeBaseAnswer = data.KnowledgeBaseAnswer,
                Subject = data.Project,
            };

            await feedbackProvider.UpsertFeecbackAsync(feedbackEntity).ConfigureAwait(false);

            return feedbackEntity;
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
                Status = (int)TicketState.UnAssigned,
                DateCreated = DateTime.Now,
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
                Subject = data.Project,
            };

            await ticketsProvider.UpsertTicketAsync(ticketEntity).ConfigureAwait(false);

            return ticketEntity;
        }
    }
}
