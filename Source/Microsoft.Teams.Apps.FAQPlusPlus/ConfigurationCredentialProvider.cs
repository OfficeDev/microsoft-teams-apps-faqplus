// <copyright file="ConfigurationCredentialProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Implementation of <see cref="ICredentialProvider"/> that gets the app credentials from configuration.
    /// </summary>
    public class ConfigurationCredentialProvider : SimpleCredentialProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationCredentialProvider"/> class.
        /// </summary>
        /// <param name="configuration">Configuration object to fetch the configuration information.</param>
        public ConfigurationCredentialProvider(IConfiguration configuration)
            : base(configuration != null ? configuration["MicrosoftAppId"] : string.Empty, configuration != null ? configuration["MicrosoftAppPassword"] : string.Empty)
        {
        }
    }
}