// <copyright file="IRetryMessageService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of service now provider.
    /// </summary>
    public interface IRetryMessageService
    {
        /// <summary>
        /// If this message is retry message.
        /// </summary>
        /// <param name="messageId">The id of message.</param>
        /// <returns>true or false.</returns>
        bool IsRetryMessage(string messageId);
    }
}
