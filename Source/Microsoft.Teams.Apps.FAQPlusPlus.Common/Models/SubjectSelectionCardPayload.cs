// <copyright file="ResponseCardPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the payload of a response card.
    /// </summary>
    public class SubjectSelectionCardPayload : TeamsAdaptiveSubmitActionData
    {
        /// <summary>
        /// Gets or sets the subject to narrow down user question.
        /// </summary>
        [JsonProperty("Subject")]
        public string Subject { get; set; }
    }
}
