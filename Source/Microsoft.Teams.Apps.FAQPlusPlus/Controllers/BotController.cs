// <copyright file="BotController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Teams.Apps.FAQPlusPlus.Bots;

    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.

    /// <summary>
    /// This is a Bot controller class includes all API's related to this Bot.
    /// </summary>
    [Route("api/messages")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotFrameworkHttpAdapter adapter;
        private readonly IBot expertBot;
        private readonly IBot userBot;

        /// <summary>
        /// Initializes a new instance of the <see cref="BotController"/> class.
        /// </summary>
        /// <param name="adapter">Bot adapter.</param>
        /// <param name="expertBot"> Instance of Expert bot.</param>
        /// <param name="userBot"> Instance of User bot.</param>
        public BotController(IBotFrameworkHttpAdapter adapter, FaqPlusExpertBot expertBot, FaqPlusUserBot userBot)
        {
            this.adapter = adapter;
            this.expertBot = expertBot;
            this.userBot = userBot;
        }

        /// <summary>
        /// Executing the Post Async method for Expert bot.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Route("expert")]
        public async Task PostExpertAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await this.adapter.ProcessAsync(this.Request, this.Response, this.expertBot).ConfigureAwait(false);
        }

        /// <summary>
        /// Executing the Post Async method for User bot.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [HttpPost]
        [Route("user")]
        public async Task PostUserAsync()
        {
            // Delegate the processing of the HTTP POST to the adapter.
            // The adapter will invoke the bot.
            await this.adapter.ProcessAsync(this.Request, this.Response, this.userBot).ConfigureAwait(false);
        }
    }
}
