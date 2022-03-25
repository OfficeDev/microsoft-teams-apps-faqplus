// <copyright file="RecommendCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AdaptiveCards;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Recommend Card- recommend by bot when user asks a question with no answer for some times to bot.
    /// </summary>
    public static class RecommendCard
    {
        /// <summary>
        /// Construct the response card - when user asks a question to the QnA Maker through the bot.
        /// </summary>
        /// <param name="questionsList">Question list to build the card.</param>
        /// <param name="appId">app id which you provided when configuring the tab.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>The recommend card to append to a message as an attachment.</returns>
        public static Attachment GetCard(List<string> questionsList, string appId, string appBaseUri)
        {
            List<AdaptiveAction> actions = BuildActions(appBaseUri, appId);

            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = BuildRecommendCardBody(questionsList, appBaseUri),
                Actions = actions,
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        /// <summary>
        /// This method builds the body of the response card, and helps to render the follow up prompts if the response contains any.
        /// </summary>
        /// <param name="questionsList">Question list to build the card.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>A list of adaptive elements which makes up the body of the adaptive card.</returns>
        private static List<AdaptiveElement> BuildRecommendCardBody(List<string> questionsList, string appBaseUri)
        {
            var cardBodyToConstruct = new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock
                {
                    Text = Strings.RecommendMessage,
                    Wrap = true,
                    Spacing = AdaptiveSpacing.Medium,
                },
            };

            // If there follow up prompts, then the follow up prompts will render accordingly.
            if (questionsList.Count > 0)
            {
                foreach (var item in questionsList)
                {
                    var container = new AdaptiveContainer
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = AdaptiveColumnWidth.Stretch,
                                        VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Wrap = true,
                                                Text = string.Format(Strings.SelectActionItemDisplayTextFormatting, item),
                                                HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                                            },
                                        },
                                        Spacing = AdaptiveSpacing.Padding,
                                        BackgroundImage = new AdaptiveBackgroundImage
                                        {
                                            Url = new Uri(appBaseUri + "/content/Followupicon3.3.png"),
                                            HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                                            VerticalAlignment = AdaptiveVerticalAlignment.Center,
                                        },
                                    },
                                },
                            },
                        },
                        SelectAction = new AdaptiveSubmitAction
                        {
                            Title = item,
                            Data = new RecommendCardPayload
                            {
                                MsTeams = new CardAction
                                {
                                    Type = ActionTypes.MessageBack,
                                    DisplayText = item,
                                    Text = item,
                                },
                                Question = item,
                            },
                        },
                        Separator = true,
                    };

                    cardBodyToConstruct.Add(container);
                }
            }

            cardBodyToConstruct.Add(new AdaptiveColumnSet
            {
                Separator = true,
                Columns = new List<AdaptiveColumn>
                    {
                        new AdaptiveColumn
                        {
                            Items = new List<AdaptiveElement>
                            {
                                new AdaptiveTextBlock
                                {
                                    Weight = AdaptiveTextWeight.Lighter,
                                    Text = Strings.ResponseFooterText,
                                    Wrap = true,
                                },
                            },
                        },
                    },
            });

            return cardBodyToConstruct;
        }

        /// <summary>
        /// This method will build the necessary list of actions.
        /// </summary>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <param name="appId">app id which you provided when configuring the tab.</param>
        /// <returns>A list of adaptive actions.</returns>
        private static List<AdaptiveAction> BuildActions(string appBaseUri, string appId)
        {
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>
            {
                // Adds the "Ask an expert" button.
                new AdaptiveSubmitAction
                {
                    Title = Strings.AskAnExpertButtonText,
                    Data = new ResponseCardPayload
                    {
                        MsTeams = new CardAction
                        {
                            Type = ActionTypes.MessageBack,
                            DisplayText = Strings.AskAnExpertDisplayText,
                            Text = Constants.AskAnExpert,
                        },
                    },
                    IconUrl = appBaseUri + "/content/expert.png",
                },

                // Adds the "User Guide" button.
                new AdaptiveOpenUrlAction
                {
                   Title = Strings.UserGuideButtonText,
                   Url = new Uri($"https://teams.microsoft.com/l/entity/" + appId + "/help"),
                   IconUrl = appBaseUri + "/content/guide.png",
                },
            };

            return actionsList;
        }
    }
}