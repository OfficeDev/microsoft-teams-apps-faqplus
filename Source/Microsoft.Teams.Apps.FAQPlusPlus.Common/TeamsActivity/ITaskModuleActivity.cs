// <copyright file="ITaskModuleActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.TeamsActivity
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;

    /// <summary>
    /// Task module activity interface.
    /// </summary>
    public interface ITaskModuleActivity
    {
        /// <summary>
        /// Handles click on edit button on a question in SME team.
        /// </summary>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="appBaseUri">App base uri.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task<TaskModuleResponse> OnFetchAsync(TaskModuleRequest taskModuleRequest, string appBaseUri);

        /// <summary>
        /// Handles submiting a edited question from SME team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="appBaseUri">App base uri.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task<TaskModuleResponse> OnSubmitAsync(ITurnContext<IInvokeActivity> turnContext, string appBaseUri);
    }
}
