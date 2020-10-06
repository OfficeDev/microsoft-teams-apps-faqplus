// <copyright file="WelcomeCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using AdaptiveCards.Templating;
    using System.IO;

    /// <summary>
    ///  This class process Welcome Card, when bot is installed by the user in personal scope.
    /// </summary>
    public static class WelcomeCard
    {
        /// <summary>
        /// This method will construct the user welcome card when bot is added in personal scope.
        /// </summary>
        /// <param name="welcomeText">Gets welcome text.</param>
        /// <returns>User welcome card.</returns>
        public static Attachment GetCard(string welcomeText)
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            // create template instance from the template payload
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(File.ReadAllText(@".\Cards\json\WelcomeCard.json"));
            var welcomeData = new
            {
                text = welcomeText,
                displytext = Strings.TakeATourButtonText,
                submitActionText = Constants.TakeATour,
            };

            // "Expand" the template - this generates the final Adaptive Card payload
            var cardJson = template.Expand(welcomeData);
            var result = AdaptiveCard.FromJson(cardJson);
            responseCard = result.Card;
            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = responseCard,
            };
        }
    }
}