// <copyright file="ResponseCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
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
        /// <param name="project"> the value of project metadata </param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Response card.</returns>
        public static Attachment GetCard(string question, string answer, string userQuestion, string project, string appBaseUri)
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = answer,
                        Wrap = true,
                    },
                    new AdaptiveColumnSet
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
                    },

                },
                Actions = new List<AdaptiveAction>
                {
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
                            UserQuestion = userQuestion,
                            KnowledgeBaseAnswer = answer,
                            Project = project,
                        },
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.ShareFeedbackButtonText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.ShareFeedbackDisplayText,
                                Text = Constants.ShareFeedback,
                            },
                            UserQuestion = userQuestion,
                            KnowledgeBaseAnswer = answer,
                            Project = project,
                        },
                    },
                },
            };
            responseCard.Actions[0].AdditionalProperties.Add("iconUrl", appBaseUri + "/content/expert.png");
            responseCard.Actions[1].AdditionalProperties.Add("iconUrl", appBaseUri + "/content/feedback.png");

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        /// <summary>
        /// Construct the response card - when user asks a question to QnA Maker through bot.
        /// </summary>
        /// <param name="question">Knowledgebase question, from QnA Maker service.</param>
        /// <param name="answer">Knowledgebase answer, from QnA Maker service.</param>
        /// <param name="promts">multiturn prompts</param>
        /// <returns>Response card.</returns>
        public static List<Attachment> GetMultiturnCard(string question, string answer, IList<PromptDTO> promts)
        {
            double cardNum = System.Math.Ceiling((double)promts.Count / 6);

            var attachments = new List<Attachment>();

            for (int cardIndex = 0; cardIndex < cardNum; cardIndex++)
            {
                List<AdaptiveAction> actions = new List<AdaptiveAction>();
                for (int index = 0 + (cardIndex * 6); index < (cardIndex + 1) * 6 && index < promts.Count; index++)
                {
                    actions.Add(new AdaptiveSubmitAction
                    {
                        Title = promts[index].DisplayText,
                        Data = new ResponseCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = promts[index].DisplayText,
                                Text = promts[index].DisplayText,
                            },
                            UserQuestion = question,
                            KnowledgeBaseAnswer = answer,
                            IsMultiturn = true,
                        },
                    });
                }

                AdaptiveCard responseCard = new AdaptiveCard
                {
                    Body = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = cardIndex == 0 ? answer : null,
                            Wrap = true,
                        },
                    },
                    Actions = actions,
                };

                attachments.Add(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    Content = responseCard,
                });
            }

            return attachments;
        }
    }
}