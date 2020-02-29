// <copyright file="QnaHelper.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Helpers
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Qna helper class for qna pair data.
    /// </summary>
    public static class QnaHelper
    {
        /// <summary>
        /// Get combined description for rich card.
        /// </summary>
        /// <param name="questionData">Question data object.</param>
        /// <returns>Combined description for rich card.</returns>
        public static string BuildCombinedDescriptionAsync(AdaptiveSubmitActionData questionData)
        {
            if (!string.IsNullOrWhiteSpace(questionData?.Subtitle?.Trim())
                || !string.IsNullOrWhiteSpace(questionData?.Title?.Trim())
                || !string.IsNullOrWhiteSpace(questionData?.ImageUrl?.Trim())
                || !string.IsNullOrWhiteSpace(questionData?.RedirectionUrl?.Trim()))
            {
                var answerModel = new AnswerModel
                {
                    Description = questionData?.Description.Trim(),
                    Title = questionData?.Title?.Trim(),
                    Subtitle = questionData?.Subtitle?.Trim(),
                    ImageUrl = questionData?.ImageUrl?.Trim(),
                    RedirectionUrl = questionData?.RedirectionUrl?.Trim(),
                };

                return JsonConvert.SerializeObject(answerModel);
            }
            else
            {
                return questionData.Description.Trim();
            }
        }

        /// <summary>
        /// Delete qna pair.
        /// </summary>
        /// <param name="turnContext">Turn context.</param>
        /// <param name="qnaServiceProvider">Qna Service provider.</param>
        /// <param name="activityStorageProvider">Activity Storage Provider.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task DeleteQnaPair(
            ITurnContext<IMessageActivity> turnContext,
            IQnaServiceProvider qnaServiceProvider,
            IActivityStorageProvider activityStorageProvider,
            ILogger logger,
            CancellationToken cancellationToken)
        {
            QnASearchResult searchResult;
            Attachment attachment;

            var activity = (Activity)turnContext.Activity;
            var activityValue = ((JObject)activity.Value).ToObject<AdaptiveSubmitActionData>();
            QnASearchResultList qnaAnswerResponse = await qnaServiceProvider.GenerateAnswerAsync(activityValue?.OriginalQuestion, isTestKnowledgeBase: false).ConfigureAwait(false);

            bool isSameQuestion = false;
            searchResult = qnaAnswerResponse.Answers.First();

            // Check if question exist in the knowledgebase.
            if (searchResult != null && searchResult.Questions.Count > 0)
            {
                // Check if the deleted question & result returned from the knowledgebase are same.
                isSameQuestion = searchResult.Questions.First().ToUpperInvariant() == activityValue?.OriginalQuestion.ToUpperInvariant().Trim();
            }

            // Delete the QnA pair if question exist in the knowledgebase & exactly the same question user wants to delete.
            if (searchResult.Id != -1 && isSameQuestion)
            {
                await qnaServiceProvider.DeleteQnaAsync(searchResult.Id.Value).ConfigureAwait(false);
                logger.LogInformation($"Question deleted by: {activity.Conversation.AadObjectId}");
                attachment = MessagingExtensionQnaCard.DeletedEntry(activityValue?.OriginalQuestion, searchResult.Answer, activity.From.Name, activityValue?.UpdateHistoryData);
                ActivityEntity activityEntity = new ActivityEntity { ActivityReferenceId = searchResult.Metadata.FirstOrDefault(x => x.Name == Constants.MetadataActivityReferenceId)?.Value };

                bool operationStatus = await activityStorageProvider.DeleteActivityEntityAsync(activityEntity).ConfigureAwait(false);
                if (!operationStatus)
                {
                    logger.LogInformation($"Unable to delete the activity data from table storage.");
                }

                var updateCardActivity = new Activity(ActivityTypes.Message)
                {
                    Id = turnContext.Activity.ReplyToId,
                    Conversation = turnContext.Activity.Conversation,
                    Attachments = new List<Attachment> { attachment },
                };

                // Send deleted question and answer card as response.
                await turnContext.UpdateActivityAsync(updateCardActivity, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // check if question and answer is present in unpublished version.
                qnaAnswerResponse = await qnaServiceProvider.GenerateAnswerAsync(activityValue?.OriginalQuestion, isTestKnowledgeBase: true).ConfigureAwait(false);

                if (qnaAnswerResponse?.Answers?.First().Id != -1)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(string.Format(CultureInfo.InvariantCulture, Strings.WaitMessage, activityValue?.OriginalQuestion))).ConfigureAwait(false);
                }
            }

            return;
        }

        /// <summary>
        /// Checks whether question exist in production/test knowledgebase.
        /// </summary>
        /// <param name="provider">Qna service provider.</param>
        /// <param name="question">Question.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents question is already exist in knowledgebase while false indicates the question does not exist in knowledgebase.</returns>
        public static async Task<bool> QuestionExistsInKbAsync(this IQnaServiceProvider provider, string question)
        {
            var prodHasQuestion = await provider.HasQuestionAsync(question, isTestKnowledgeBase: false).ConfigureAwait(false);
            if (prodHasQuestion)
            {
                return true;
            }

            return await provider.HasQuestionAsync(question, isTestKnowledgeBase: true).ConfigureAwait(false);
        }

        /// <summary>
        /// Checks whether question exist in knowledgebase.
        /// </summary>
        /// <param name="provider">Qna service provider.</param>
        /// <param name="question">Question.</param>
        /// <param name="isTestKnowledgeBase">Knowledgebase.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents question is already exist in knowledgebase while false indicates the question does not exist in knowledgebase.</returns>
        private static async Task<bool> HasQuestionAsync(this IQnaServiceProvider provider, string question, bool isTestKnowledgeBase)
        {
            var qnaPreviewAnswerResponse = await provider.GenerateAnswerAsync(question, isTestKnowledgeBase).ConfigureAwait(false);
            var questionAnswerResponse = qnaPreviewAnswerResponse.Answers.FirstOrDefault();

            if (questionAnswerResponse == null || questionAnswerResponse.Questions.Count == 0)
            {
                return false;
            }

            // Check if question asked and result returned from the knowledgebase are same.
            return questionAnswerResponse.Questions.First().ToUpperInvariant() == question?.ToUpperInvariant().Trim();
        }
    }
}
