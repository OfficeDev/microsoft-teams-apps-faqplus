// <copyright file="SOSRequest.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// SOSRequest in Json format.
    /// </summary>
    public class SOSRequest
    {
        /// <summary>
        /// Gets or sets name of email of ticket owner.
        /// </summary>
        [JsonProperty("requested_for")]
        public string RequestFor { get; set; }

        /// <summary>
        ///  Gets or sets topic of ticket.
        /// </summary>
        [JsonProperty("u_service_topic")]
        public string Topic { get; set; }

        /// <summary>
        ///  Gets or sets short description of ticket.
        /// </summary>
        [JsonProperty("short_description")]
        public string ShortDescription { get; set; }

        /// <summary>
        ///  Gets or sets description of ticket.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }

        /// <summary>
        ///  Gets or sets the default assignment group.
        /// </summary>
        [JsonProperty("assignment_group")]
        public string AssignmentGroup { get; set; } = "Enterprise Infra Service Center";

        /// <summary>
        ///  Gets or sets the watch list.
        /// </summary>
        [JsonProperty("watch_list")]
        public string WatchList { get; set; }
    }
}
