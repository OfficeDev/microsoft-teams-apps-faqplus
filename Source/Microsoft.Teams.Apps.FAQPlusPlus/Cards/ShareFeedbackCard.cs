// <copyright file="ShareFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
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
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(string appBaseUri)
        {
            return GetCard(new ShareFeedbackCardPayload(), showValidationErrors: false, appBaseUri);
        }

        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the response card.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(ResponseCardPayload payload, string appBaseUri)
        {
            var cardPayload = new ShareFeedbackCardPayload
            {
                DescriptionHelpful = payload.UserQuestion,             // Pre-populate the description with the user's question
                DescriptionNeedsImprovement = payload.UserQuestion,    // Pre-populate the description with the user's question
                DescriptionNotHelpful = payload.UserQuestion,          // Pre-populate the description with the user's question
                UserQuestion = payload.UserQuestion,
                KnowledgeBaseAnswer = payload?.KnowledgeBaseAnswer,
                Project = payload.Project,
            };

            return GetCard(cardPayload, showValidationErrors: false, appBaseUri);
        }

        /// <summary>
        /// This method will construct the card for share feedback, when invoked from the feedback card submit.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(ShareFeedbackCardPayload payload, string appBaseUri)
        {
            if (payload == null)
            {
                return null;
            }
            else
            {
                return GetCard(payload, showValidationErrors: true, appBaseUri);
            }
        }

        /// <summary>
        /// This method will construct the card  for share feedback bot menu.
        /// </summary>
        /// <param name="data">Data from the share feedback card.</param>
        /// <param name="showValidationErrors">Flag to determine rating value.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Share feedback card.</returns>
        private static Attachment GetCard(ShareFeedbackCardPayload data, bool showValidationErrors, string appBaseUri)
        {
            AdaptiveCard shareFeedbackCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = !string.IsNullOrWhiteSpace(data.UserQuestion) ? string.Format(CultureInfo.InvariantCulture, Strings.ResultsFeedbackText, data.UserQuestion) : Strings.ShareFeedbackTitleText,
                        Size = AdaptiveTextSize.Large,
                        Wrap = true,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveShowCardAction
                    {
                        IconUrl = appBaseUri + "/content/face_smile.png",
                        Title = " ",
                        Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                        {
                           Body = new List<AdaptiveElement>
                            {
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
                                                    Text = Strings.DescriptionText,
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
                                                    Text = (showValidationErrors && data?.DescriptionHelpful?.Length > 500) ? Strings.MaxCharactersText : string.Empty,
                                                    Color = AdaptiveTextColor.Attention,
                                                    HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                                    Wrap = true,
                                                },
                                            },
                                         },
                                    },
                                },
                                new AdaptiveTextInput
                                {
                                    Spacing = AdaptiveSpacing.Small,
                                    Id = nameof(ShareFeedbackCardPayload.DescriptionHelpful),
                                    Placeholder = Strings.FeedbackDescriptionPlaceholderText,
                                    IsMultiline = true,
                                    Value = data?.DescriptionHelpful,
                                },
                            },
                           Actions = new List<AdaptiveAction>
                           {
                                new AdaptiveSubmitAction
                                {
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
                                        Rating = nameof(FeedbackRating.Helpful),
                                        Project = data.Project,
                                    },
                                },
                           },
                        },
                    },
                    new AdaptiveShowCardAction
                    {
                        IconUrl =  appBaseUri + "/content/face_straigh.png",
                        Title = " ",
                        Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                        {
                           Body = new List<AdaptiveElement>
                           {
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
                                                   Text = Strings.DescriptionText,
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
                                                   Text = (showValidationErrors && data?.DescriptionNeedsImprovement?.Length > 500) ? Strings.MaxCharactersText : string.Empty,
                                                   Color = AdaptiveTextColor.Attention,
                                                   HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                                   Wrap = true,
                                               },
                                           },
                                        },
                                   },
                               },
                               new AdaptiveTextInput
                               {
                                   Spacing = AdaptiveSpacing.Small,
                                   Id = nameof(ShareFeedbackCardPayload.DescriptionNeedsImprovement),
                                   Placeholder = Strings.FeedbackDescriptionPlaceholderText,
                                   IsMultiline = true,
                                   Value = data?.DescriptionNeedsImprovement,
                               },
                           },
                           Actions = new List<AdaptiveAction>
                           {
                               new AdaptiveSubmitAction
                               {
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
                                       Rating = nameof(FeedbackRating.NeedsImprovement),
                                       Project = data.Project,
                                   },
                               },
                           },
                        },
                    },
                    new AdaptiveShowCardAction
                    {
                        IconUrl = appBaseUri + "/content/face_sad.png",
                        Title = " ",
                        Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                        {
                           Body = new List<AdaptiveElement>
                           {
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
                                                   Text = Strings.DescriptionText,
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
                                                   Text = (showValidationErrors && data?.DescriptionNotHelpful?.Length > 500) ? Strings.MaxCharactersText : string.Empty,
                                                   Color = AdaptiveTextColor.Attention,
                                                   HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                                                   Wrap = true,
                                               },
                                           },
                                        },
                                   },
                               },
                               new AdaptiveTextInput
                               {
                                   Spacing = AdaptiveSpacing.Small,
                                   Id = nameof(ShareFeedbackCardPayload.DescriptionNotHelpful),
                                   Placeholder = Strings.FeedbackDescriptionPlaceholderText,
                                   IsMultiline = true,
                                   Value = data?.DescriptionNotHelpful,
                               },
                           },
                           Actions = new List<AdaptiveAction>
                            {
                                new AdaptiveSubmitAction
                                {
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
                                        Rating = nameof(FeedbackRating.NotHelpful),
                                        Project = data.Project,
                                    },
                                },
                            },
                        },
                    },
                },
            };

            //shareFeedbackCard.Actions[0].AdditionalProperties.Add("iconUrl", appBaseUri + "/content/face_smile.png");
            //shareFeedbackCard.Actions[1].AdditionalProperties.Add("iconUrl", appBaseUri + "/content/face_straigh.png");
            //shareFeedbackCard.Actions[2].AdditionalProperties.Add("iconUrl", appBaseUri + "/content/face_sad.png");

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = shareFeedbackCard,
            };
        }
    }
}