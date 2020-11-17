// <copyright file="ConfigurationController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    /// <summary>
    /// This is a configure controller class which will be displayed when add to teams channel
    /// details in the bot tab.
    /// </summary>
    [Route("/configuration")]
    public class ConfigurationController : Controller
    {
        /// <summary>
        /// Display configuration tab.
        /// </summary>
        /// <returns>confgiuration tab view.</returns>
        public IActionResult Index()
        {
            return this.View();
        }
    }
}