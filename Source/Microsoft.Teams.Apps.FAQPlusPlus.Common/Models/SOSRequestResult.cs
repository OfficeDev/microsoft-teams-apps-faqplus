// <copyright file="SOSRequestResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// the response of request.
    /// </summary>
    public class SOSRequestResult
    {
        /// <summary>
        /// Gets or sets Result.
        /// </summary>
        [JsonProperty("result")]
        public SOSRequestResultContent Result { get; set; }
    }
}
