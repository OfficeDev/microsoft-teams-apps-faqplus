// <copyright file="MessageQueueOptions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues
{
    /// <summary>
    /// Options used for creating service bus message queues.
    /// </summary>
    public class MessageQueueOptions
    {
        /// <summary>
        /// Gets or sets the service bus connection.
        /// </summary>
        public string ServiceBusConnection { get; set; }
    }
}
