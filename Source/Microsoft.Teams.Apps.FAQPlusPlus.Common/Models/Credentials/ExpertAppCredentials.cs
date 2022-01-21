// <copyright file="ExpertAppCredentials.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Credentials
{
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    /// <summary>
    /// An Expert Microsoft app credentials object.
    /// </summary>
    public class ExpertAppCredentials : MicrosoftAppCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpertAppCredentials"/> class.
        /// </summary>
        /// <param name="botSettings">The bot settings.</param>
        public ExpertAppCredentials(IOptionsMonitor<BotSettings> botSettings)
            : base(appId: botSettings.CurrentValue.ExpertAppId, password: botSettings.CurrentValue.ExpertAppPassword)
        {
        }
    }
}
