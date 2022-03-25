// <copyright file="HelpController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// This is a Static tab controller class which will be used to display Help
    /// details in the bot tab.
    /// </summary>
    [Route("/help")]
    public class HelpController : Controller
    {
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly BotSettings options;
        private readonly IUserActionProvider userActionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpController"/> class.
        /// </summary>
        /// <param name="configurationProvider">Configuration provider dependency injection.</param>
        /// <param name="userActionProvider">UserAction Provider.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for FaqPlusPlus bot.</param>
        public HelpController(IConfigurationDataProvider configurationProvider, IUserActionProvider userActionProvider, IOptionsMonitor<BotSettings> optionsAccessor)
        {
            this.configurationProvider = configurationProvider;
            this.userActionProvider = userActionProvider;
            this.options = optionsAccessor.CurrentValue;
        }

        /// <summary>
        /// Display help tab.
        /// </summary>
        /// <returns>Help tab view.</returns>
        public ActionResult Index()
        {
            return this.View(nameof(this.Index));
        }

        /// <summary>
        /// Get unresolved ticket detail.
        /// </summary>
        /// <returns>unresolved tickets list in json format.</returns>
        [HttpPost]
        [Route("/help/appID")]
        public async Task<ActionResult> AppIDAsync([FromBody] Parameter para)
        {
            Parameter p = new Parameter();
            p.APPID = this.options.MicrosoftAppId;

            UserActionEntity userAction = new UserActionEntity();
            userAction.UserPrincipalName = para.UserPrincipleName;
            userAction.UserActionId = Guid.NewGuid().ToString();
            userAction.Action = nameof(UserActionType.ViewHelpTab);
            await this.userActionProvider.UpsertUserActionAsync(userAction).ConfigureAwait(false);

            return this.Json(p);
        }

        public class Parameter
        {
            public string APPID { get; set; }
            public string UserPrincipleName { get; set; }
        }
    }
}