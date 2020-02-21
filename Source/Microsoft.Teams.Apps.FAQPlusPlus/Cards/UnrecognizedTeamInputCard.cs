// <copyright file="UnrecognizedTeamInputCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class handles unrecognized input sent by the team member-sending random text to bot.
    /// </summary>
    public static class UnrecognizedTeamInputCard
    {
        /// <summary>
        /// Construct the card to render when there's an unrecognized input in a channel.
        /// </summary>
        /// <returns>Card attachment.</returns>
        public static Attachment GetCard()
        {
            var card = new HeroCard
            {
                Text = Strings.TeamCustomMessage,
                Buttons = new List<CardAction>
                {
                    new CardAction(ActionTypes.MessageBack)
                    {
                        Title = Strings.TakeATeamTourButtonText,
                        DisplayText = Strings.TakeATeamTourButtonText,
                        Text = Constants.TeamTour,
                    },
                },
            };

            return card.ToAttachment();
        }
    }
}
