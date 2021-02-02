// <copyright file="TicketNotificationStrings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.CheckSLA
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Ticket Notification template information.
    /// </summary>
    public class TicketNotificationStrings
    {
        /// <summary>
        /// Titile of ticket unassigned notification.
        /// </summary>
        public static readonly string TitleUnassigned = "Reminder for Unassigning";

        /// <summary>
        /// Title of ticket pending notification.
        /// </summary>
        public static readonly string TitlePending = "Reminder of Ticket Updating";

        /// <summary>
        /// Title of ticket unresolved notification.
        /// </summary>
        public static readonly string TitleUnResolved = "Reminder of Ticket Resolving";

        /// <summary>
        /// Description of ticket unassigned notification.
        /// </summary>
        public static readonly string DescriptionUnassigned = "This ticket is still not assigned to certain member. Please delegate this request.";

        /// <summary>
        /// Description of ticket pending notification.
        /// </summary>
        public static readonly string DescriptionPending = "You didn't updat the pending ticket over {0} hours. Please check if it could be resovled";

        /// <summary>
        /// Description of ticket pending CC notification.
        /// </summary>
        public static readonly string DescriptionPendingCC = "{0} didn't update this pending ticket over {1} hours";

        /// <summary>
        /// Description of ticket unresolved notification.
        /// </summary>
        public static readonly string DescriptionUnresolved = "This ticket is assigned over {0} hours. Please resolve or change to pending status if there is any blocker.";

        /// <summary>
        /// Description of ticket unresolved CC notification.
        /// </summary>
        public static readonly string DescriptionUnresolvedCC = "{0}'s ticket is ongoing over {1} hours. A update (resolve or pending) is required.";
    }
}
