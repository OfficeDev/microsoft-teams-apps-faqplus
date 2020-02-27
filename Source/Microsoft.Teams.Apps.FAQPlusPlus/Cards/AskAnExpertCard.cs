// <copyright file="AskAnExpertCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Ask an expert function : A feature available in bot menu commands in 1:1 scope.
    /// </summary>
    public static class AskAnExpertCard
    {
        /// <summary>
        /// Text associated with ask an expert command.
        /// </summary>
        public const string AskAnExpertSubmitText = "QuestionForExpert";

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
            AdaptiveCard askAnExpertCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Strings.AskAnExpertTitleText,
                        Size = AdaptiveTextSize.Large,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Strings.AskAnExpertSubheaderText,
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
                                        HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
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
                                Text = AskAnExpertSubmitText,
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