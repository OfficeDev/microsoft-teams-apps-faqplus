// <copyright file="RetryMessageService.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// RetryMessageService will manage all message id and check if it is retry message.
    /// </summary>
    public class RetryMessageService : IRetryMessageService
    {
        private readonly Dictionary<string, DateTime> messageIdDic = new Dictionary<string, DateTime>();

        /// <summary>
        /// Identify if a messageId is retry message.
        /// </summary>
        /// <param name="messageId">Id of message.</param>
        /// <returns>true of false.</returns>
        public bool IsRetryMessage(string messageId)
        {
            DateTime now = DateTime.UtcNow;
            foreach (var outdated in this.messageIdDic.Where(kv => (now - kv.Value).TotalSeconds > 60).ToList())
            {
                this.messageIdDic.Remove(outdated.Key);
            }

            if (this.messageIdDic.ContainsKey(messageId))
            {
                return true;
            }
            else
            {
                this.messageIdDic.Add(messageId, now);
                return false;
            }
        }
    }
}
