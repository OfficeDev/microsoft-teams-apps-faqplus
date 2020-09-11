// <copyright file="UserNotificationCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Creates a user notification card from a ticket.
    /// </summary>
    public class UserNotificationCard
    {
        private readonly TicketEntity ticket;
        private string appBaseUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotificationCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket to create a card from.</param>
        public UserNotificationCard(TicketEntity ticket)
        {
            this.ticket = ticket;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotificationCard"/> class.
        /// </summary>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <param name="ticket">The ticket to create a card from.</param>
        public UserNotificationCard(TicketEntity ticket, string appBaseUri)
        {
            this.ticket = ticket;
            this.appBaseUri = appBaseUri;
        }

        /// <summary>
        /// Returns a user notification card for the ticket.
        /// </summary>
        /// <param name="message">The status message to add to the card.</param>
        /// <param name="activityLocalTimestamp">Local time stamp of user activity.</param>
        /// <returns>An adaptive card as an attachment.</returns>
        public Attachment ToAttachment(string message, DateTimeOffset? activityLocalTimestamp)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Text = message,
                        Wrap = true,
                    },
                    new AdaptiveFactSet
                    {
                      Facts = this.BuildFactSet(this.ticket, activityLocalTimestamp),
                    },
                },
                Actions = BuildActions(this.ticket, this.appBaseUri),
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
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>A list of adaptive card actions.</returns>
        private static List<AdaptiveAction> BuildActions(TicketEntity ticket, string appBaseUri)
        {
            if (ticket.Status == (int)TicketState.Resolved)
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
                                Text = Strings.AskAnExpertDisplayText,
                            },
                        },
                        IconUrl = appBaseUri + "/content/expert.png",
                    },
                };
            }

            return null;
        }

        /// <summary>
        /// Building the fact set to render out the user facing details.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <param name="activityLocalTimestamp">The local timestamp.</param>
        /// <returns>The adaptive facts.</returns>
        private List<AdaptiveFact> BuildFactSet(TicketEntity ticket, DateTimeOffset? activityLocalTimestamp)
        {
            List<AdaptiveFact> factList = new List<AdaptiveFact>();
            factList.Add(new AdaptiveFact
            {
                Title = Strings.StatusFactTitle,
                Value = CardHelper.GetUserTicketDisplayStatus(this.ticket),
            });

            if (ticket.Status != (int)TicketState.UnAssigned)
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.ExpertFact,
                    Value = ticket.AssignedToName,
                });
            }

            if (!string.IsNullOrEmpty(ticket.Subject))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.SubjectFact,
                    Value = ticket.Subject,
                });
            }

            factList.Add(new AdaptiveFact
            {
                Title = Strings.TitleFact,
                Value = CardHelper.TruncateStringIfLonger(this.ticket.Title, CardHelper.TitleMaxDisplayLength),
            });

            if (!string.IsNullOrEmpty(ticket.Description))
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
                Value = CardHelper.GetFormattedDateInUserTimeZone(this.ticket.DateCreated, activityLocalTimestamp),
            });
            if (ticket.Status == (int)TicketState.Pending && this.ticket.PendingComment != null)
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.CommentText,
                    Value = CardHelper.TruncateStringIfLonger(this.ticket.PendingComment, CardHelper.DescriptionMaxDisplayLength),
                });
            }

            if (ticket.Status == (int)TicketState.Resolved)
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.ClosedFactTitle,
                    Value = CardHelper.GetFormattedDateInUserTimeZone(this.ticket.DateClosed.Value, activityLocalTimestamp),
                });
                if (this.ticket.ResolveComment != null)
                {
                    factList.Add(new AdaptiveFact
                    {
                        Title = Strings.CommentText,
                        Value = CardHelper.TruncateStringIfLonger(this.ticket.ResolveComment, CardHelper.DescriptionMaxDisplayLength),
                    });
                }
            }

            return factList;
        }
    }
}