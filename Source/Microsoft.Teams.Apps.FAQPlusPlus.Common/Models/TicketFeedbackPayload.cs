// <copyright file="ShareFeedbackCardPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// This model class is responsible to model user activity with bot-
    /// asking a question or providing feedback on app or on results given by the bot to the user.
    /// </summary>
    public class TicketFeedbackPayload : TeamsAdaptiveSubmitActionData
    {
        /// <summary>
        /// Gets or sets the unique ticket id.
        /// </summary>
        public string TicketId { get; set; }

        /// <summary>
        /// Gets or sets the action when user submits feedback rating.
        /// </summary>
        public string Rating { get; set; }
    }
}