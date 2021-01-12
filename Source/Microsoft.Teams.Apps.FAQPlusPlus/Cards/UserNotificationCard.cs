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
        /// <summary>
        /// Text associated with share feedback command.
        /// </summary>
        public const string TicketFeedback = "TicketFeedback";

        private readonly TicketEntity ticket;
        private string appBaseUri;
        private string appId;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserNotificationCard"/> class.
        /// </summary>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <param name="ticket">The ticket to create a card from.</param>
        /// <param name="appId">app id which you provided when configuring the tab.</param>
        public UserNotificationCard(TicketEntity ticket, string appBaseUri, string appId)
        {
            this.ticket = ticket;
            this.appBaseUri = appBaseUri;
            this.appId = appId;
        }

        /// <summary>
        /// Returns a user notification card which contains a single string.
        /// </summary>
        /// <param name="message">The message to add to the card.</param>
        /// <returns>An adaptive card as an attachment.</returns>
        public static Attachment ToAttachmentString(string message)
        {
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = message,
                                        Wrap = true,
                                    },
                                },
                                Width = "stretch",
                            },
                        },
                    },
                },
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Returns a user notification card for the ticket.
        /// </summary>
        /// <param name="message">The status message to add to the card.</param>
        /// <param name="activityLocalTimestamp">Local time stamp of user activity.</param>
        /// <returns>An adaptive card as an attachment.</returns>
        public Attachment ToAttachment(string message, DateTimeOffset? activityLocalTimestamp)
        {
            string urlString = this.appBaseUri + "/content/ticket_reopen.png";

            if (string.Compare(message, Strings.NotificationCardContent) == 0)
            {
                urlString = this.appBaseUri + "/content/ticket_created.png";
            }

            if (string.Compare(message, Strings.PendingTicketUserNotification) == 0)
            {
                urlString = this.appBaseUri + "/content/ticket_pending.png";
            }
            else if (string.Compare(message, Strings.ClosedTicketUserNotification) == 0)
            {
                urlString = this.appBaseUri + "/content/ticket_resolved.png";
            }
            else if (string.Compare(message, Strings.ReopenedTicketUserNotification) == 0 || string.Compare(message, Strings.ReAssigneTicketUserNotification) == 0)
            {
                urlString = this.appBaseUri + "/content/ticket_reopen.png";
            }
            else if (string.Compare(message, Strings.AssignedTicketUserNotification) == 0)
            {
                urlString = this.appBaseUri + "/content/ticket_assigned.png";
            }

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = new List<AdaptiveColumn>
                        {
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveImage
                                    {
                                        Style = AdaptiveImageStyle.Default,
                                        Size = AdaptiveImageSize.Medium,
                                        Url = new Uri(urlString),
                                    },
                                },
                                Width = "auto",
                            },
                            new AdaptiveColumn
                            {
                                Items = new List<AdaptiveElement>
                                {
                                    new AdaptiveTextBlock
                                    {
                                        Text = message,
                                        Wrap = true,
                                    },
                                },
                                Width = "stretch",
                            },
                        },
                    },
                    new AdaptiveFactSet
                    {
                      Facts = this.BuildFactSet(this.ticket, activityLocalTimestamp),
                    },
                },
                Actions = BuildActions(this.ticket, this.appBaseUri, this.appId),
            };

            card.Body.Add(new AdaptiveColumnSet
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
                                    Text = this.ticket.Status == (int)TicketState.Resolved ? Strings.UserNotificationFooterText : Strings.UserNotificationUpdateFooterText,
                                    Wrap = true,
                                },
                            },
                        },
                    },
            });

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
        /// <param name="appId">app id which you provided when configuring the tab.</param>
        /// <returns>A list of adaptive card actions.</returns>
        private static List<AdaptiveAction> BuildActions(TicketEntity ticket, string appBaseUri, string appId)
        {
            if (ticket.Status == (int)TicketState.Resolved)
            {
                return new List<AdaptiveAction>
                {
                    new AdaptiveSubmitAction
                    {
                        Title = " ",
                        Data = new TicketFeedbackPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Text = TicketFeedback,
                            },
                            Rating = nameof(TicketSatisficationRating.Satisfied),
                            TicketId = ticket.TicketId,
                        },
                        IconUrl = appBaseUri + "/content/face_smile.png",
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = " ",
                        Data = new TicketFeedbackPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Text = TicketFeedback,
                            },
                            Rating = nameof(TicketSatisficationRating.Neutral),
                            TicketId = ticket.TicketId,
                        },
                        IconUrl = appBaseUri + "/content/face_straigh.png",
                    },
                    new AdaptiveSubmitAction
                    {
                        Title = " ",
                        Data = new TicketFeedbackPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                Text = TicketFeedback,
                            },
                            TicketId = ticket.TicketId,
                            Rating = nameof(TicketSatisficationRating.Disappointed),
                        },
                        IconUrl = appBaseUri + "/content/face_sad.png",
                    },
                };
            }
            else
            {
                return new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = Strings.MyTicketButtonText,
                        Url = new Uri($"https://teams.microsoft.com/l/entity/" + appId + "/my"),
                        IconUrl = appBaseUri + "/content/my_ticket.png",
                    },
                };
            }
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

            if (!string.IsNullOrEmpty(ticket.TicketId))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.TicketIDFact,
                    Value = ticket.TicketId.Substring(0, 8),
                });
            }

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