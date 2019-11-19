// <copyright file="WelcomeCard.cs" company="Microsoft">
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
    ///  This class process Welcome Card, when bot is installed by the user in personal scope.
    /// </summary>
    public class WelcomeCard
    {
        /// <summary>
        /// This method will construct the user welcome card when bot is added in personal scope.
        /// </summary>
        /// <param name="welcomeText">Gets welcome text.</param>
        /// <returns>User welcome card.</returns>
        public static Attachment GetCard(string welcomeText)
        {
            AdaptiveCard userWelcomeCard = new AdaptiveCard("1.0")
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        HorizontalAlignment = AdaptiveHorizontalAlignment.Left,
                        Text = welcomeText,
                        Wrap = true
                    }
                },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Resource.TakeATourButtonText,
                        Data = new TeamsAdaptiveSubmitActionData
                        {
                            MsTeams = new CardAction
                            {
                              Type = ActionTypes.MessageBack,
                              DisplayText = Resource.TakeATourButtonText,
                              Text = FaqPlusPlusBot.TakeATour
                            }
                        },
                    }
                }
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = userWelcomeCard,
            };
        }
    }
}