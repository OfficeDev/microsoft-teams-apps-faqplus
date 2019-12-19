// <copyright file="ResponseCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process Response Card- Response by bot when user asks a question to bot.
    /// </summary>
    public static class ResponseCard
    {
        /// <summary>
        /// Construct the response card - when user asks a question to QnA Maker through bot.
        /// </summary>
        /// <param name="question">Knowledgebase question, from QnA Maker service.</param>
        /// <param name="answer">Knowledgebase answer, from QnA Maker service.</param>
        /// <param name="userQuestion">Actual question asked by the user to the bot.</param>
        /// <returns>Response card.</returns>
        public static Attachment GetCard(string question, string answer, string userQuestion)
        {
            AdaptiveCard responseCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Resource.ResponseHeaderText,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = question,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = answer,
                        Wrap = true,
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.AskAnExpertButtonText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Resource.AskAnExpertDisplayText,
                                Text = FaqPlusPlusBot.AskAnExpert,
                            },
                            UserQuestion = userQuestion,
                            KnowledgeBaseAnswer = answer,
                        }
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.ShareFeedbackButtonText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Resource.ShareFeedbackDisplayText,
                                Text = FaqPlusPlusBot.ShareFeedback,
                            },
                            UserQuestion = userQuestion,
                            KnowledgeBaseAnswer = answer,
                        }
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }
    }
}