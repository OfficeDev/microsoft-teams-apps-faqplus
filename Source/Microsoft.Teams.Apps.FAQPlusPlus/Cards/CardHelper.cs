// <copyright file="CardHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Globalization;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Utility functions for constructing cards used in this project.
    /// </summary>
    public static class CardHelper
    {
        /// <summary>
        /// Maximum length of the knowledge base answer to show.
        /// </summary>
        public const int KnowledgeBaseAnswerMaxDisplayLength = 500;

        /// <summary>
        /// Maximum length of the user title.
        /// </summary>
        public const int TitleMaxDisplayLength = 50;

        /// <summary>
        /// Maximum length of the user description.
        /// </summary>
        public const int DescriptionMaxDisplayLength = 500;

        private const string Ellipsis = "...";

        /// <summary>
        /// Truncate the provided string to a given maximum length.
        /// </summary>
        /// <param name="text">Text to be truncated.</param>
        /// <param name="maxLength">The maximum length in characters of the text.</param>
        /// <returns>Truncated string.</returns>
        public static string TruncateStringIfLonger(string text, int maxLength)
        {
            if ((!string.IsNullOrEmpty(text)) && (text.Length > maxLength))
            {
                text = text.Substring(0, maxLength) + Ellipsis;
            }

            return text;
        }

        /// <summary>
        /// Gets the ticket status for the user notifications.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>A status string.</returns>
        public static string GetUserTicketDisplayStatus(TicketEntity ticket)
        {
            if (ticket?.Status == (int)TicketState.Open)
            {
                return ticket.IsAssigned() ?
                    Strings.AssignedUserNotificationStatus :
                    Strings.UnassignedUserNotificationStatus;
            }
            else
            {
                return Strings.ClosedUserNotificationStatus;
            }
        }

        /// <summary>
        /// Gets the current status of the ticket to display in the SME team.
        /// </summary>
        /// <param name="ticket">The current ticket information.</param>
        /// <returns>A status string.</returns>
        public static string GetTicketDisplayStatusForSme(TicketEntity ticket)
        {
            if (ticket?.Status == (int)TicketState.Open)
            {
                return ticket.IsAssigned() ?
                    string.Format(CultureInfo.InvariantCulture, Strings.SMETicketAssignedStatus, ticket?.AssignedToName) :
                    Strings.SMETicketUnassignedStatus;
            }
            else
            {
                return Strings.SMETicketClosedStatus;
            }
        }

        /// <summary>
        /// Returns a string that will display the given date and time in the user's local time zone, when placed in an adaptive card.
        /// </summary>
        /// <param name="dateTime">The date and time to format.</param>
        /// <param name="userLocalTime">The sender's local time, as determined by the local timestamp of the activity.</param>
        /// <returns>A datetime string.</returns>
        public static string GetFormattedDateInUserTimeZone(DateTime dateTime, DateTimeOffset? userLocalTime)
        {
            // Adaptive card on mobile has a bug where it does not support DATE and TIME, so for now we convert the date and time manually.
            return dateTime.Add(userLocalTime?.Offset ?? TimeSpan.FromMinutes(0)).ToShortDateString();
        }
    }
}