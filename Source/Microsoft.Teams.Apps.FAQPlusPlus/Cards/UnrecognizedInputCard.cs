// <copyright file="UnrecognizedInputCard.cs" company="Microsoft">
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
    ///  This class handles unrecognized input sent by the user-asking random question to bot.
    /// </summary>
    public static class UnrecognizedInputCard
    {
        /// <summary>
        /// This method will construct the card when unrecognized input is sent by the user.
        /// </summary>
        /// <param name="userQuestion">Actual question asked by the user to the bot.</param>
        /// <returns>UnrecognizedInput Card.</returns>
        public static Attachment GetCard(string userQuestion)
        {
            AdaptiveCard unrecognizedInputCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = Resource.CustomMessage,
                        Wrap = true
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
                                Text = FaqPlusPlusBot.AskAnExpert
                            },
                            UserQuestion = userQuestion
                        },
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = unrecognizedInputCard,
            };
        }
    }
}