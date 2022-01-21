// <copyright file="MigrateTicketCardPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// Represents the data payload of Action.Submit to migrate of a ticket from ols bot to new bot.
    /// </summary>
    public class MigrateTicketCardPayload
    {
        /// <summary>
        /// Gets or sets the ticket id.
        /// </summary>
        public string TicketId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the ticket is to be migrated.
        /// </summary>
        public bool ToBeMigrated { get; set; }
    }
}
