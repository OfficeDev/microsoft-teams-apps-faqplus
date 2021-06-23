// <copyright file="SOSRequestResultContent.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// The content of SOS request result.
    /// </summary>
    public class SOSRequestResultContent
    {
        /// <summary>
        /// Gets or sets Response Code.
        /// </summary>
        [JsonProperty("Code")]
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets Result.
        /// </summary>
        [JsonProperty("result")]
        public string Result { get; set; }

        /// <summary>
        /// Gets or sets the ticket ID.
        /// </summary>
        [JsonProperty("reference")]
        public string Reference { get; set; }

        /// <summary>
        /// Gets or sets the ticket system ID.
        /// </summary>
        [JsonProperty("sys_id")]
        public string SysId { get; set; }

        /// <summary>
        /// Gets or sets the ticket URL.
        /// </summary>
        [JsonProperty("url")]
        public string URL { get; set; }
    }
}
