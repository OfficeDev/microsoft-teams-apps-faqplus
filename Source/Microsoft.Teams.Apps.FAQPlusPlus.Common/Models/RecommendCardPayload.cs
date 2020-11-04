// <copyright file="RecommendCardPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System.Collections.Generic;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the payload of a response card.
    /// </summary>
    public class RecommendCardPayload : TeamsAdaptiveSubmitActionData
    {
        /// <summary>
        /// Gets or sets the Question of asked by the user.
        /// </summary>
        [JsonProperty("Question")]
        public string Question { get; set; }
    }
}
