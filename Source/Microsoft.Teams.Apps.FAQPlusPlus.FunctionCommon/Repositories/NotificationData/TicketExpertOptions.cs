// <copyright file="TicketExpertOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData
{
    /// <summary>
    /// Options used for creating service bus message queues.
    /// </summary>
    public class TicketExpertOptions
    {
        /// <summary>
        /// Gets or sets the Team Id.
        /// </summary>
        public string TeamId { get; set; }
        /// <summary>
        /// Gets or sets the Tenant Id.
        /// </summary>
        public string TenantId { get; set; }
        /// <summary>
        /// Gets or sets the Group Id.
        /// </summary>
        public string GroupId { get; set; }
        /// <summary>
        /// Gets or sets the Team Name.
        /// </summary>
        public string TeamName { get; set; }
        /// <summary>
        /// Gets or sets the Channel Name.
        /// </summary>
        public string ChannelName { get; set; }
    }
}
