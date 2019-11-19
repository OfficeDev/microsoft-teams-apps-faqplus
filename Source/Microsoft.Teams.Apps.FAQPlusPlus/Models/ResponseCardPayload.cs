// <copyright file="ResponseCardPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Models
{
    /// <summary>
    /// Represents the payload of a response card.
    /// </summary>
    public class ResponseCardPayload : TeamsAdaptiveSubmitActionData
    {
        /// <summary>
        /// Gets or sets the question that was asked originally asked by the user.
        /// </summary>
        public string UserQuestion { get; set; }

        /// <summary>
        /// Gets or sets the response given by the bot to the user.
        /// </summary>
        public string KnowledgeBaseAnswer { get; set; }
    }
}