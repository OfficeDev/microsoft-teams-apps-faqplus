﻿// <copyright file="SendQueueMessageContent.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.SendQueue
{
    /// <summary>
    /// Azure service bus send queue message content class.
    /// </summary>
    public class SendQueueMessageContent
    {
        /// <summary>
        /// Gets or sets the notification id value.
        /// </summary>
        public string NotificationId { get; set; }

        /// <summary>
        /// Gets or sets the information about the recipient. This
        /// holds enough information for the Azure Function to send this
        /// recipient a notification.
        /// </summary>
        public RecipientData RecipientData { get; set; }
    }
}
