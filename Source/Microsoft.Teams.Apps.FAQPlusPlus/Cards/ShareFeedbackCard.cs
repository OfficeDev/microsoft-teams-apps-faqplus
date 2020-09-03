// <copyright file="ShareFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Streaming.Payloads;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process a Share feedback function - A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public static class ShareFeedbackCard
    {
        /// <summary>
        /// Text associated with share feedback command.
        /// </summary>
        public const string ShareFeedbackSubmitText = "ShareFeedback";

        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the bot menu.
        /// </summary>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard()
        {
            return GetCard(new ShareFeedbackCardPayload(), showValidationErrors: false);
        }

        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the response card.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(ResponseCardPayload payload)
        {
            var cardPayload = new ShareFeedbackCardPayload
            {
                Description = payload.UserQuestion,     // Pre-populate the description with the user's question
                UserQuestion = payload.UserQuestion,
                KnowledgeBaseAnswer = payload?.KnowledgeBaseAnswer,
            };

            return GetCard(cardPayload, showValidationErrors: false);
        }

        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the feedback card submit.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(ShareFeedbackCardPayload payload)
        {
            if (payload == null)
            {
                return null;
            }
            else
            {
                return GetCard(payload, showValidationErrors: true);
            }
        }

        /// <summary>
        /// This method will construct the card  for share feedback bot menu.
        /// </summary>
        /// <param name="data">Data from the share feedback card.</param>
        /// <param name="showValidationErrors">Flag to determine rating value.</param>
        /// <returns>Share feedback card.</returns>
        private static Attachment GetCard(ShareFeedbackCardPayload data, bool showValidationErrors)
        {
            AdaptiveCard shareFeedbackCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = !string.IsNullOrWhiteSpace(data.UserQuestion) ? Strings.ResultsFeedbackText : Strings.ShareFeedbackTitleText,
                        Size = AdaptiveTextSize.Large,
                        Wrap = true,
                    },
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Width = AdaptiveColumnWidth.Auto,
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = !string.IsNullOrWhiteSpace(data.UserQuestion) ? Strings.FeedbackRatingRequired : Strings.ShareAppFeedbackRating,
                                        Wrap = true,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = (showValidationErrors && !Enum.TryParse(data.Rating, out FeedbackRating rating)) ? Strings.RatingMandatoryText : string.Empty,
                                        Color = AdaptiveTextColor.Attention,
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveChoiceSetInput
                    {
                        Id = nameof(ShareFeedbackCardPayload.Rating),
                        IsMultiSelect = false,
                        Style = AdaptiveChoiceInputStyle.Compact,
                        Choices = new List<AdaptiveChoice>
                        {
                            new AdaptiveChoice
                            {
                                Title = Strings.HelpfulRatingText,
                                Value = nameof(FeedbackRating.Helpful),
                            },
                            new AdaptiveChoice
                            {
                                Title = Strings.NeedsImprovementRatingText,
                                Value = nameof(FeedbackRating.NeedsImprovement),
                            },
                            new AdaptiveChoice
                            {
                                Title = Strings.NotHelpfulRatingText,
                                Value = nameof(FeedbackRating.NotHelpful),
                            },
                        },
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Strings.DescriptionText,
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Spacing = AdaptiveSpacing.Small,
                        Id = nameof(ShareFeedbackCardPayload.Description),
                        Placeholder = !string.IsNullOrWhiteSpace(data.UserQuestion) ? Strings.FeedbackDescriptionPlaceholderText : Strings.AppFeedbackDescriptionPlaceholderText,
                        IsMultiline = true,
                        Value = data.Description,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.ShareFeedbackButtonText,
                        Data = new ShareFeedbackCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.ShareFeedbackDisplayText,
                                Text = ShareFeedbackSubmitText,
                            },
                            UserQuestion = data.UserQuestion,
                            KnowledgeBaseAnswer = data.KnowledgeBaseAnswer,
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = shareFeedbackCard,
            };
        }
    }
}