﻿// <copyright file="SendQueueMessageContentExtension.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Extensions
{
    using System;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.SendQueue;

    /// <summary>
    /// Extension class for <see cref="SendQueueMessageContent"/>.
    /// </summary>
    public static class SendQueueMessageContentExtension
    {
        /// <summary>
        /// Get service url.
        /// </summary>
        /// <param name="message">Send Queue message.</param>
        /// <returns>Service url.</returns>
        public static string GetServiceUrl(this SendQueueMessageContent message)
        {
            var recipient = message.RecipientData;
            switch (recipient.RecipientType)
            {
                case RecipientDataType.User:
                    return recipient.UserData.ServiceUrl;
                case RecipientDataType.Team:
                    return recipient.TeamData.ServiceUrl;
                default:
                    throw new ArgumentException("Invalid recipient type");
            }
        }

        /// <summary>
        /// Get conversationId.
        /// </summary>
        /// <param name="message">Send Queue message.</param>
        /// <returns>Conversation Id.</returns>
        public static string GetConversationId(this SendQueueMessageContent message)
        {
            var recipient = message.RecipientData;

            switch (recipient.RecipientType)
            {
                case RecipientDataType.User:
                    return recipient.UserData.ConversationId;
                case RecipientDataType.Team:
                    return recipient.TeamData.TeamId;
                default:
                    throw new ArgumentException("Invalid recipient type");
            }
        }
    }
}
