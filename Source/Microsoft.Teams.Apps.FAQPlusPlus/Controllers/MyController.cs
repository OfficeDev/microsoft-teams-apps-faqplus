// <copyright file="MyController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// This is a Static tab controller class which will be used to display my
    /// details in the bot tab.
    /// </summary>
    [Route("/my")]
    public class MyController : Controller
    {
        private readonly ITicketsProvider ticketsProvider;
        private readonly IConversationProvider conversationProvider;
        private readonly IUserActionProvider userActionProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MyController"/> class.
        /// </summary>
        /// <param name="ticketsProvider">Tickets Provider.</param>
        /// <param name="conversationProvider">Conversation storage provider.</param>
        /// <param name="userActionProvider">UserAction Provider.</param>
        public MyController(ITicketsProvider ticketsProvider, IConversationProvider conversationProvider, IUserActionProvider userActionProvider)
        {
            this.ticketsProvider = ticketsProvider;
            this.conversationProvider = conversationProvider;
            this.userActionProvider = userActionProvider;
        }

        /// <summary>
        /// Display my tab.
        /// </summary>
        /// <returns>My tab view.</returns>
        public ActionResult Index()
        {
            return this.View(nameof(this.Index));
        }

        /// <summary>
        /// Get unresolved ticket detail.
        /// </summary>
        /// <returns>unresolved tickets list in json format.</returns>
        [HttpPost]
        [Route("/my/tickets")]
        public async Task<ActionResult> TicketsAsync([FromBody] Parameter para)
        {
            var tickets = await this.ticketsProvider.GetUserTicketsAsync(para.UserPrincipleName);

            UserActionEntity userAction = new UserActionEntity();
            userAction.UserPrincipalName = para.UserPrincipleName;
            userAction.UserActionId = Guid.NewGuid().ToString();
            userAction.Action = nameof(UserActionType.ViewMyTab);
            await this.userActionProvider.UpsertUserActionAsync(userAction).ConfigureAwait(false);

            return this.Json(tickets);
        }

        /// <summary>
        /// Get unresolved ticket detail.
        /// </summary>
        /// <returns>unresolved tickets list in json format.</returns>
        [HttpPost]
        [Route("/my/FAQ")]
        public async Task<ActionResult> FAQAsync()
        {
            var ceList = await this.conversationProvider.GetRecentAskedQnAListAsync(30);
            Dictionary<string, FAQ> idCountDic = new Dictionary<string, FAQ>();
            foreach (ConversationEntity ce in ceList)
            {
                if (!idCountDic.ContainsKey(ce.Question))
                {
                    idCountDic.Add(ce.Question, new FAQ { Catagory = ce.Project, Count = 1 });
                }
                else
                {
                    idCountDic[ce.Question].Count++;
                }
            }

            var list = idCountDic.OrderByDescending(r => r.Value.Count).ToList();
            return this.Json(list);
        }

        public class Parameter
        {
            public string UserPrincipleName { get; set; }
        }

        public class FAQ
        {
            public string Catagory { get; set; }
            public int Count { get; set; }
        }
    }
}