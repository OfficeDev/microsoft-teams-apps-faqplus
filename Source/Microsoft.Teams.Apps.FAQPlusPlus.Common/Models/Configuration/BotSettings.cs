// <copyright file="BotSettings.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration
{
    /// <summary>
   /// Provides app settings related to FaqPlusPlus bot.
   /// </summary>
    public class BotSettings
    {
        /// <summary>
        /// Gets or sets access cache expiry in days.
        /// </summary>
        public int AccessCacheExpiryInDays { get; set; }

        /// <summary>
        /// Gets or sets application base uri.
        /// </summary>
        public string AppBaseUri { get; set; }

        /// <summary>
        /// Gets or sets user app id.
        /// </summary>
        public string UserAppId { get; set; }

        /// <summary>
        /// Gets or sets user app password.
        /// </summary>
        public string UserAppPassword { get; set; }

        /// <summary>
        /// Gets or sets expert app id.
        /// </summary>
        public string ExpertAppId { get; set; }

        /// <summary>
        /// Gets or sets expert app password.
        /// </summary>
        public string ExpertAppPassword { get; set; }

        /// <summary>
        /// Gets or sets access tenant id string.
        /// </summary>
        public string TenantId { get; set; }
    }
}