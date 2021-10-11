// <copyright file="AskAnExpertCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards
{
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;

    /// <summary>
    ///  This class process Ask an expert function : A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public static class AskAnExpertCard
    {
        /// <summary>
        /// This method will construct the card for ask an expert, when invoked from the bot menu.
        /// </summary>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard()
        {
            return GetCard(new AskAnExpertCardPayload(), showValidationErrors: false);
        }

        /// <summary>
        /// This method will construct the card for ask an expert, when invoked from the response card.
        /// </summary>
        /// <param name="payload">Payload from the response card.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(ResponseCardPayload payload)
        {
            var cardPayload = new AskAnExpertCardPayload
            {
                Description = payload.UserQuestion,     // Pre-populate the description with the user's question.
                UserQuestion = payload.UserQuestion,
                KnowledgeBaseAnswer = payload?.KnowledgeBaseAnswer,
            };

            return GetCard(cardPayload, showValidationErrors: false);
        }

        /// <summary>
        /// This method will construct the card for ask an expert, when invoked from the ask an expert card submit.
        /// </summary>
        /// <param name="payload">Payload from the ask an expert card.</param>
        /// <returns>Ask an expert card.</returns>
        public static Attachment GetCard(AskAnExpertCardPayload payload)
        {
            return GetCard(payload, showValidationErrors: true);
        }

        /// <summary>
        /// This method will construct the card for ask an expert bot menu.
        /// </summary>
        /// <param name="cardPayload">Data from the ask an expert card.</param>
        /// <param name="showValidationErrors">Determines whether we show validation errors.</param>
        /// <returns>Ask an expert card.</returns>
        private static Attachment GetCard(AskAnExpertCardPayload cardPayload, bool showValidationErrors)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            var errorAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Left : AdaptiveHorizontalAlignment.Right;

            AdaptiveCard askAnExpertCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Strings.AskAnExpertTitleText,
                        HorizontalAlignment = textAlignment,
                        Size = AdaptiveTextSize.Large,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Strings.AskAnExpertSubheaderText,
                        HorizontalAlignment = textAlignment,
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
                                        Text = Strings.TitleRequiredText,
                                        Wrap = true,
                                        HorizontalAlignment = textAlignment,
                                    },
                                },
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = (showValidationErrors && string.IsNullOrWhiteSpace(cardPayload?.Title)) ? Strings.MandatoryTitleFieldText : string.Empty,
                                        Color = AdaptiveTextColor.Attention,
                                        HorizontalAlignment = errorAlignment,
                                        Wrap = true,
                                    },
                                },
                            },
                        },
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(AskAnExpertCardPayload.Title),
                        Placeholder = Strings.ShowCardTitleText,
                        IsMultiline = false,
                        Spacing = AdaptiveSpacing.Small,
                        Value = cardPayload?.Title,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Strings.DescriptionText,
                        HorizontalAlignment = textAlignment,
                        Wrap = true,
                    },
                    new AdaptiveTextInput
                    {
                        Id = nameof(AskAnExpertCardPayload.Description),
                        Placeholder = Strings.AskAnExpertPlaceholderText,
                        IsMultiline = true,
                        Spacing = AdaptiveSpacing.Small,
                        Value = cardPayload?.Description,
                    },
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.AskAnExpertButtonText,
                        Data = new AskAnExpertCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.AskAnExpertDisplayText,
                                Text = Constants.AskAnExpertSubmitText,
                            },
                            UserQuestion = cardPayload?.UserQuestion,
                            KnowledgeBaseAnswer = cardPayload?.KnowledgeBaseAnswer,
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = askAnExpertCard,
            };
        }
    }
}