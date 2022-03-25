// <copyright file="Subject.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Json fromat subjects get from Storage table.
    /// </summary>
    public class Subject
    {
        /// <summary>
        ///  Gets or Sets Projects separated by ','.
        /// </summary>
        [JsonProperty("Project")]
        public string Project { get; set; }

        /// <summary>
        /// Gets or Sets Other subject speparated by ','.
        /// </summary>
        [JsonProperty("Other")]
        public string Other { get; set; }
    }
}
