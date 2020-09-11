// <copyright file="TicketState.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// Represents the current status of a ticket.
    /// </summary>
    public enum TicketState
    {
        /// <summary>
        /// Represents an active ticket.
        /// </summary>
        UnAssigned = 0,

        /// <summary>
        /// Represents an assigned ticket.
        /// </summary>
        Assigned = 1,

        /// <summary>
        /// Represents a pending ticket.
        /// </summary>
        Pending = 2,

        /// <summary>
        /// Represents a ticket that requires no further action.
        /// </summary>
        Resolved = 3,

        /// <summary>
        /// Sentinel value.
        /// </summary>
        MaxValue = Resolved,
    }
}
