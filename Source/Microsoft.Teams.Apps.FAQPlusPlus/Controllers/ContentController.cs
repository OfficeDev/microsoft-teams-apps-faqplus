// <copyright file="ContentController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Controllers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// This is a content controller class which will be displayed in expert channel tab
    /// details in the bot tab.
    /// </summary>
    public class ContentController : Controller
    {
        private readonly ITicketsProvider ticketsProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentController"/> class.
        /// </summary>
        /// <param name="ticketsProvider">Tickets Provider.</param>
        public ContentController(ITicketsProvider ticketsProvider)
        {
            this.ticketsProvider = ticketsProvider;
        }

        /// <summary>
        /// Display content tab.
        /// </summary>
        /// <returns>confgiuration tab view.</returns>
        [Route("/content")]
        public IActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Get unresolved ticket detail.
        /// </summary>
        /// <param name="para">is ticket resolved.</param>
        /// <returns>unresolved tickets list in json format.</returns>
        [HttpPost]
        [Route("/content/tickets")]
        public async Task<ActionResult> TicketsAsync([FromBody] Parameters para)
        {
            var tickets = await this.ticketsProvider.GetTicketsAsync(para.isResolved);
            foreach (TicketEntity ticket in tickets)
            {
                ticket.RequesterUserPrincipalName = $"https://teams.microsoft.com/l/chat/0/0?users=" + Uri.EscapeDataString(ticket.RequesterUserPrincipalName);
                if (ticket.SmeThreadConversationId != null)
                {
                    var index = ticket.SmeThreadConversationId.IndexOf('=');
                    ticket.SmeThreadConversationId = ticket.SmeThreadConversationId.Substring(index + 1, ticket.SmeThreadConversationId.Length - index - 1);
                }
            }

            return this.Json(tickets);
        }

        public class Parameters
        {
            public bool isResolved { get; set; }
        }
    }
}