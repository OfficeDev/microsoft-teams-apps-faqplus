// <copyright file="UserAppCredentials.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Credentials
{
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;

    /// <summary>
    /// An User Microsoft app credentials object.
    /// </summary>
    public class UserAppCredentials : MicrosoftAppCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserAppCredentials"/> class.
        /// </summary>
        /// <param name="botSettings">The bot settings.</param>
        public UserAppCredentials(IOptionsMonitor<BotSettings> botSettings)
            : base(appId: botSettings.CurrentValue.UserAppId, password: botSettings.CurrentValue.UserAppPassword)
        {
        }
    }
}
