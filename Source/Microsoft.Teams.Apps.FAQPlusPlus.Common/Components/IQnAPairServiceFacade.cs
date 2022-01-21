// <copyright file="IQnAPairServiceFacade.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Components
{
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// QnA pair facade interface.
    /// </summary>
    public interface IQnAPairServiceFacade
    {
        /// <summary>
        /// Get the reply to a question asked by end user.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="message">Text message.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        Task GetReplyToQnAAsync(ITurnContext<IMessageActivity> turnContext, IMessageActivity message);

        /// <summary>
        /// Method perform update operation of question and answer pair.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="answer">Answer of the given question.</param>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents question and answer pair updated successfully while false indicates failure in updating the question and answer pair.</returns>
        Task<bool> SaveQnAPairAsync(ITurnContext turnContext, string answer, AdaptiveSubmitActionData qnaPairEntity);

        /// <summary>
        /// Validate the adaptive card fields while editing the question and answer pair.
        /// </summary>
        /// <param name="postedQnaPairEntity">Qna pair entity contains submitted card data.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <returns>Envelope for Task Module Response.</returns>
        Task<TaskModuleResponse> EditQnAPairAsync(AdaptiveSubmitActionData postedQnaPairEntity, ITurnContext<IInvokeActivity> turnContext);
    }
}
