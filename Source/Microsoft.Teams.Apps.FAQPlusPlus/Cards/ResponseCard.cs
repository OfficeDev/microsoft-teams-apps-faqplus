// <copyright file="ResponseCard.cs" company="Microsoft">
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
    using Microsoft.Teams.Apps.FAQPlusPlus.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    ///  This class process Response Card- Response by bot when user asks a question to bot.
    /// </summary>
    public static class ResponseCard
    {
        /// <summary>
        /// Represent response card icon width in pixel.
        /// </summary>
        private const uint IconWidth = 32;

        /// <summary>
        /// Represent response card icon height in pixel.
        /// </summary>
        private const uint IconHeight = 32;

        /// <summary>
        /// Construct the response card - when user asks a question to the QnA Maker through the bot.
        /// </summary>
        /// <param name="response">The response model.</param>
        /// <param name="userQuestion">Actual question that the user has asked the bot.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <param name="payload">The response card payload.</param>
        /// <returns>The response card to append to a message as an attachment.</returns>
        public static Attachment GetCard(QnASearchResult response, string userQuestion, string appBaseUri, ResponseCardPayload payload)
        {
            bool isRichCard = false;
            AdaptiveSubmitActionData answerModel = new AdaptiveSubmitActionData();
            if (Validators.IsValidJSON(response.Answer))
            {
                answerModel = JsonConvert.DeserializeObject<AdaptiveSubmitActionData>(response.Answer);
                isRichCard = Validators.IsRichCard(answerModel);
            }

            string answer = isRichCard ? answerModel.Description : response.Answer;
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = BuildResponseCardBody(response, userQuestion, answer, appBaseUri, payload, isRichCard),
                Actions = BuildListOfActions(userQuestion, answer),
            };

            if (!string.IsNullOrEmpty(answerModel.RedirectionUrl))
            {
                responseCard.Actions.Add(
                    new AdaptiveOpenUrlAction
                    {
                        Title = Strings.OpenArticle,
                        Url = new Uri(answerModel.RedirectionUrl),
                    });
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }

        /// <summary>
        /// This method builds the body of the response card, and helps to render the follow up prompts if the response contains any.
        /// </summary>
        /// <param name="response">The QnA response model.</param>
        /// /// <param name="userQuestion">The user question - the actual question asked to the bot.</param>
        /// <param name="answer">The answer string.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <param name="payload">The response card payload.</param>
        /// <param name="isRichCard">Boolean value where true represent it is a rich card while false represent it is a normal card.</param>
        /// <returns>A list of adaptive elements which makes up the body of the adaptive card.</returns>
        private static List<AdaptiveElement> BuildResponseCardBody(QnASearchResult response, string userQuestion, string answer, string appBaseUri, ResponseCardPayload payload, bool isRichCard)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            var answerModel = isRichCard ? JsonConvert.DeserializeObject<AnswerModel>(response?.Answer) : new AnswerModel();

            var cardBodyToConstruct = new List<AdaptiveElement>()
            {
                new AdaptiveTextBlock
                {
                    Weight = AdaptiveTextWeight.Bolder,
                    Text = Strings.ResponseHeaderText,
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Size = AdaptiveTextSize.Default,
                    Wrap = true,
                    Text = response?.Questions[0],
                    IsVisible = isRichCard,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Wrap = true,
                    Text = answerModel.Title ?? string.Empty,
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    HorizontalAlignment = textAlignment,
                },
                new AdaptiveTextBlock
                {
                    Text = answerModel.Subtitle ?? string.Empty,
                    Size = AdaptiveTextSize.Medium,
                    HorizontalAlignment = textAlignment,
                },
            };

            if (!string.IsNullOrWhiteSpace(answerModel?.ImageUrl))
            {
                cardBodyToConstruct.Add(new AdaptiveImage
                {
                    Url = new Uri(answerModel.ImageUrl.Trim()),
                    Size = AdaptiveImageSize.Auto,
                    Style = AdaptiveImageStyle.Default,
                    AltText = answerModel.Title,
                    IsVisible = isRichCard,
                });
            }

            cardBodyToConstruct.Add(new AdaptiveTextBlock
            {
                Text = answer,
                Wrap = true,
                Size = isRichCard ? AdaptiveTextSize.Small : AdaptiveTextSize.Default,
                Spacing = AdaptiveSpacing.Medium,
                HorizontalAlignment = textAlignment,
            });

            // If there follow up prompts, then the follow up prompts will render accordingly.
            if (response?.Context.Prompts.Count > 0)
            {
                List<QnADTO> previousQuestions = BuildListOfPreviousQuestions((int)response.Id, userQuestion, answer, payload);

                foreach (var item in response.Context.Prompts)
                {
                    var container = new AdaptiveContainer
                    {
                        Items = new List<AdaptiveElement>()
                        {
                            new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>()
                                {
                                    // This column will be for the icon.
                                    new AdaptiveColumn
                                    {
                                        Width = AdaptiveColumnWidth.Auto,
                                        VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveImage
                                            {
                                                Url = new Uri(appBaseUri + "/content/Followupicon.png"),
                                                PixelWidth = IconWidth,
                                                PixelHeight = IconHeight,
                                            },
                                        },
                                        Spacing = AdaptiveSpacing.Small,
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = AdaptiveColumnWidth.Stretch,
                                        VerticalContentAlignment = AdaptiveVerticalContentAlignment.Center,
                                        Items = new List<AdaptiveElement>()
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Wrap = true,
                                                Text = string.Format(Strings.SelectActionItemDisplayTextFormatting, item.DisplayText?.Trim()),
                                                HorizontalAlignment = textAlignment,
                                            },
                                        },
                                        Spacing = AdaptiveSpacing.Small,
                                    },
                                },
                            },
                        },
                        SelectAction = new AdaptiveSubmitAction
                        {
                            Title = item.DisplayText,
                            Data = new ResponseCardPayload
                            {
                                MsTeams = new CardAction
                                {
                                    Type = ActionTypes.MessageBack,
                                    DisplayText = item.DisplayText,
                                    Text = item.DisplayText,
                                },
                                PreviousQuestions = new List<QnADTO> { previousQuestions.Last() },
                                IsPrompt = true,
                            },
                        },
                        Separator = true,
                    };

                    cardBodyToConstruct.Add(container);
                }
            }

            return cardBodyToConstruct;
        }

        /// <summary>
        /// This method will build the necessary list of actions.
        /// </summary>
        /// <param name="userQuestion">The user question - the actual question asked to the bot.</param>
        /// <param name="answer">The answer string.</param>
        /// <returns>A list of adaptive actions.</returns>
        private static List<AdaptiveAction> BuildListOfActions(string userQuestion, string answer)
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
                        UserQuestion = userQuestion,
                        KnowledgeBaseAnswer = answer,
                    },
                },

                // Adds the "Share feedback" button.
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
                    },
                },
            };

            return actionsList;
        }

        /// <summary>
        /// This method will build the list of previous questions.
        /// </summary>
        /// <param name="id">The QnA Id of the previous question.</param>
        /// <param name="userQuestion">The question that was asked by the user originally.</param>
        /// <param name="answer">The knowledge base answer.</param>
        /// <param name="payload">The response card payload.</param>
        /// <returns>A list of previous questions.</returns>
        private static List<QnADTO> BuildListOfPreviousQuestions(int id, string userQuestion, string answer, ResponseCardPayload payload)
        {
            var previousQuestions = payload.PreviousQuestions ?? new List<QnADTO>();

            previousQuestions.Add(new QnADTO
            {
                Id = id,
                Questions = new List<string>()
                {
                    userQuestion,
                },
                Answer = answer,
            });

            return previousQuestions;
        }
    }
}
