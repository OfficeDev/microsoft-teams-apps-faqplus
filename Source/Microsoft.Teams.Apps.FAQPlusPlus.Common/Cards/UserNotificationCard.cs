// <copyright file="UserNotificationCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards
{
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;

    /// <summary>
    /// Creates a user notification card from a ticket.
    /// </summary>
    public class UserNotificationCard
    {
        private readonly TicketEntity ticket;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotificationCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket to create a card from.</param>
        public UserNotificationCard(TicketEntity ticket)
        {
            this.ticket = ticket;
        }

        /// <summary>
        /// Returns a user notification card for the ticket.
        /// </summary>
        /// <param name="message">The status message to add to the card.</param>
        /// <returns>An adaptive card as an attachment.</returns>
        public Attachment ToAttachment(string message)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = message,
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveFactSet
                    {
                      Facts = this.BuildFactSet(),
                    },
                },
                Actions = BuildActions(this.ticket),
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Having the necessary adaptive actions built.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>A list of adaptive card actions.</returns>
        private static List<AdaptiveAction> BuildActions(TicketEntity ticket)
        {
            if (ticket.Status == (int)TicketState.Closed)
            {
                return new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = Strings.AskAnExpertButtonText,
                        Data = new TeamsAdaptiveSubmitActionData
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = Strings.AskAnExpertDisplayText,
                                Text = Constants.AskAnExpert,
                            },
                        },
                    },
                };
            }

            return null;
        }

        /// <summary>
        /// Building the fact set to render out the user facing details.
        /// </summary>
        /// <returns>The adaptive facts.</returns>
        private List<AdaptiveFact> BuildFactSet()
        {
            List<AdaptiveFact> factList = new List<AdaptiveFact>();
            factList.Add(new AdaptiveFact
            {
                Title = Strings.StatusFactTitle,
                Value = CardHelper.GetUserTicketDisplayStatus(this.ticket),
            });

            factList.Add(new AdaptiveFact
            {
                Title = Strings.TitleFact,
                Value = CardHelper.TruncateStringIfLonger(this.ticket.Title, CardHelper.TitleMaxDisplayLength),
            });

            if (!string.IsNullOrEmpty(this.ticket.Description))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.DescriptionFact,
                    Value = CardHelper.TruncateStringIfLonger(this.ticket.Description, CardHelper.DescriptionMaxDisplayLength),
                });
            }

            factList.Add(new AdaptiveFact
            {
                Title = Strings.DateCreatedDisplayFactTitle,
                Value = CardHelper.GetFormattedDateForAdaptiveCard(this.ticket.DateCreated),
            });

            if (this.ticket.Status == (int)TicketState.Closed)
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.ClosedFactTitle,
                    Value = CardHelper.GetFormattedDateForAdaptiveCard(this.ticket.DateClosed.Value),
                });
            }

            return factList;
        }
    }
}