// <copyright file="WelcomeTeamCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using System.IO;
    using AdaptiveCards;
    using AdaptiveCards.Templating;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process  Welcome Card when installed in Team scope.
    /// </summary>
    public static class WelcomeTeamCard
    {
        /// <summary>
        /// This method will construct the welcome team card when bot is added to the team.
        /// </summary>
        /// <returns>Team welcome card.</returns>
        public static Attachment GetCard()
        {
            AdaptiveCard responseCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            // create template instance from the template payload
            AdaptiveCardTemplate template = new AdaptiveCardTemplate(File.ReadAllText(@".\Cards\json\WelcomeCard.json"));
            var welcomeData = new
            {
                text = Strings.WelcomeTeamCardContent,
                displytext = Strings.TakeATeamTourButtonText,
                submitActionText = Constants.TeamTour,
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