// <copyright file="IMessagingExtensionActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.TeamsActivity
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Messaging extension activity interface.
    /// </summary>
    public interface IMessagingExtensionActivity
    {
        /// <summary>
        /// Handles query to messaging extension.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <returns>Messaging extension response object to fill compose extension section.</returns>
        Task<MessagingExtensionResponse> QueryAsync(ITurnContext<IInvokeActivity> turnContext);

        /// <summary>
        /// Handles "Add new question" button via messaging extension.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Action to be performed.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Response of messaging extension action.</returns>
        Task<MessagingExtensionActionResponse> FetchTaskAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken);

        /// <summary>
        /// Handles submit new question.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Action to be performed.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Response of messaging extension action.</returns>
        Task<MessagingExtensionActionResponse> SubmitActionAsync(ITurnContext<IInvokeActivity> turnContext, MessagingExtensionAction action, CancellationToken cancellationToken);
    }
}
