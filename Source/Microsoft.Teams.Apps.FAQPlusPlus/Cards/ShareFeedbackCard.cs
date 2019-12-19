// <copyright file="ShareFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process a Share feedback function - A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public static class ShareFeedbackCard
    {
        /// <summary>
        /// Text associated with share feedback command
        /// </summary>
        public const string ShareFeedbackSubmitText = "ShareFeedback";

        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the bot menu.
        /// </summary>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard()
        {
            return GetCard(false, new ShareFeedbackCardPayload());
        }

        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the response card.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(ResponseCardPayload payload)
        {
            var data = new ShareFeedbackCardPayload
            {
                Description = payload.UserQuestion,     // Pre-populate the description with the user's question
                UserQuestion = payload.UserQuestion,
                KnowledgeBaseAnswer = payload.KnowledgeBaseAnswer,
            };
            return GetCard(false, data);
        }

        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the feedback card submit.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(ShareFeedbackCardPayload payload)
        {
            return GetCard(true, payload);
        }

        /// <summary>
        /// This method will construct the card  for share feedback bot menu.
        /// </summary>
        /// <param name="showValidationErrors">Flag to determine rating value.</param>
        /// <param name="data">Data from the share feedback card.</param>
        /// <returns>Share feedback card.</returns>
        private static Attachment GetCard(bool showValidationErrors, ShareFeedbackCardPayload data)
        {
            AdaptiveCard shareFeedbackCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = !string.IsNullOrWhiteSpace(data.UserQuestion) ? Resource.ResultsFeedbackText : Resource.ShareFeedbackTitleText,
                        Size = AdaptiveTextSize.Large,
                        Wrap = true
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
                                        Text = Resource.FeedbackRatingRequired,
                                        Wrap = true
                                    }
                                }
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = (showValidationErrors && !Enum.TryParse(data.Rating, out FeedbackRating rating)) ? Resource.RatingMandatoryText : string.Empty,
                                        Color = AdaptiveTextColor.Attention,
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                        Wrap = true
                                    }
                                }
                            }
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
                                Title = Resource.HelpfulRatingText,
                                Value = nameof(FeedbackRating.Helpful),
                            },
                            new AdaptiveChoice
                            {
                                Title = Resource.NeedsImprovementRatingText,
                                Value = nameof(FeedbackRating.NeedsImprovement),
                            },
                            new AdaptiveChoice
                            {
                                Title = Resource.NotHelpfulRatingText,
                                Value = nameof(FeedbackRating.NotHelpful),
                            },
                        }
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Resource.DescriptionText,
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Spacing = AdaptiveSpacing.Small,
                        Id = nameof(ShareFeedbackCardPayload.Description),
                        Placeholder = Resource.FeedbackDescriptionPlaceholderText,
                        IsMultiline = true,
                        Value = data.Description,
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.ShareFeedbackButtonText,
                        Data = new ShareFeedbackCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Resource.ShareFeedbackDisplayText,
                                Text = ShareFeedbackSubmitText,
                            },
                            UserQuestion = data.UserQuestion,
                            KnowledgeBaseAnswer = data.KnowledgeBaseAnswer,
                        },
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = shareFeedbackCard,
            };
        }
    }
}