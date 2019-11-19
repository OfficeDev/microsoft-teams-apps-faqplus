// <copyright file="HelpController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// This is a Static tab controller class which will be used to display Help
    /// details in the bot tab.
    /// </summary>
    [Route("/help")]
    public class HelpController : Controller
    {
        private readonly IConfigurationProvider configurationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpController"/> class.
        /// </summary>
        /// <param name="configurationProvider">configurationProvider DI</param>
        public HelpController(IConfigurationProvider configurationProvider)
        {
            this.configurationProvider = configurationProvider;
        }

        /// <summary>
        /// Display help tab.
        /// </summary>
        /// <returns>Help tab view</returns>
        public async Task<ActionResult> Index()
        {
            string helpTabText = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.HelpTabText);

            var marked = new MarkedNet.Marked();
            var helpTabHtml = marked.Parse(helpTabText);

            return this.View(nameof(this.Index), helpTabHtml);
        }
    }
}