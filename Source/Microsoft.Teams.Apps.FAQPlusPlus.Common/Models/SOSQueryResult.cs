// <copyright file="SOSQueryResult.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// the response of request.
    /// </summary>
    public class SOSQueryResult
    {
        /// <summary>
        /// Gets or sets Result.
        /// </summary>
        [JsonProperty("result")]
        public SOSQueryReslutContent Result { get; set; }
    }
}
