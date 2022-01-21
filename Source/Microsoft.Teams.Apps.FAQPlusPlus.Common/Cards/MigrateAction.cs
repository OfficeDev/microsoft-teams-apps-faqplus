// <copyright file="MigrateAction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;

    /// <summary>
    ///  This class process Migrate Card- Card displayed when Migrate action command is triggered.
    /// </summary>
    public class MigrateAction
    {
        private readonly TicketEntity ticket;

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrateAction"/> class.
        /// </summary>
        /// <param name="ticket">The ticket model with the latest details.</param>
        public MigrateAction(TicketEntity ticket)
        {
            this.ticket = ticket;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MigrateAction"/> class.
        /// </summary>
        public MigrateAction()
        {
        }

        /// <summary>
        /// Gets the ticket that is the basis for the information in this card.
        /// </summary>
        protected TicketEntity Ticket => this.ticket;

        /// <summary>
        /// Returns an attachment based on the state and information of the ticket.
        /// </summary>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment GetCard()
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = this.Ticket.Title,
                        Size = AdaptiveTextSize.Large,
                        Weight = AdaptiveTextWeight.Bolder,
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = string.Format(CultureInfo.InvariantCulture, Strings.QuestionForExpertSubHeaderText, this.Ticket.RequesterName),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                    new AdaptiveFactSet
                    {
                        Facts = new List<AdaptiveFact>(this.BuildFactSet()),
                    },
                },
                Actions = new List<AdaptiveAction>(this.BuildActions()),
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Returns an attachment for migrate action on wrong card. 
        /// This card is displayed if migrate action is invoked on the ticket posted by same bot.
        /// </summary>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment GetErrorCard()
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            var actionsList = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = Strings.BackButtonText,
                },
            };

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = string.Format(CultureInfo.InvariantCulture, Strings.MigrateActionErrorText),
                        Wrap = true,
                        HorizontalAlignment = textAlignment,
                    },
                },
                Actions = actionsList,
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Return the appropriate set of card actions based on the state and information in the ticket.
        /// </summary>
        /// <returns>Adaptive card actions.</returns>
        private IEnumerable<AdaptiveAction> BuildActions()
        {
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>
            {
                new AdaptiveSubmitAction
                {
                    Title = Strings.BackButtonText,
                    Data = new MigrateTicketCardPayload { TicketId = this.Ticket.TicketId, ToBeMigrated = false },
                },

                new AdaptiveSubmitAction
                {
                    Title = Strings.SubmitButtonText,
                    Data = new MigrateTicketCardPayload { TicketId = this.Ticket.TicketId, ToBeMigrated = true },
                },
            };

            return actionsList;
        }

        /// <summary>
        /// Return the appropriate fact set based on the state and information in the ticket.
        /// </summary>
        /// <returns>The fact set showing the necessary details.</returns>
        private IEnumerable<AdaptiveFact> BuildFactSet()
        {
            List<AdaptiveFact> factList = new List<AdaptiveFact>();

            if (!string.IsNullOrEmpty(this.Ticket.Description))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.DescriptionFact,
                    Value = this.Ticket.Description,
                });
            }

            if (!string.IsNullOrEmpty(this.Ticket.UserQuestion))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.QuestionAskedFactTitle,
                    Value = this.Ticket.UserQuestion,
                });
            }

            factList.Add(new AdaptiveFact
            {
                Title = Strings.StatusFactTitle,
                Value = CardHelper.GetTicketDisplayStatusForSme(this.Ticket),
            });

            if (this.Ticket.Status == (int)TicketState.Closed)
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.ClosedFactTitle,
                    Value = CardHelper.GetFormattedDateForAdaptiveCard(this.Ticket.DateClosed.Value),
                });
            }

            return factList;
        }
    }
}
