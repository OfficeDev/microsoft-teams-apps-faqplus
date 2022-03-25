// <copyright file="SOSQueryReslutContent.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// The content of SOS query result.
    /// </summary>
    public class SOSQueryReslutContent
    {
        /// <summary>
        /// Gets or Sets the ticket Number.
        /// </summary>
        [JsonProperty("number")]
        public string Number { get; set; }

        /// <summary>
        /// Gets or Sets the ticket State.
        /// </summary>
        [JsonProperty("state")]
        public string State { get; set; }
    }
}
