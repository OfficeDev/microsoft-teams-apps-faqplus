// <copyright file="TaskModuleActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.TeamsActivity
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Components;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.TeamsActivity;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class that handles the task module fetch/submit activity in expert's team chat.
    /// </summary>
    public class TaskModuleActivity : ITaskModuleActivity
    {
        /// <summary>
        /// Represents the task module height.
        /// </summary>
        private const int TaskModuleHeight = 450;

        /// <summary>
        /// Represents the task module width.
        /// </summary>
        private const int TaskModuleWidth = 500;

        private readonly IConfigurationDataProvider configurationProvider;
        private readonly IQnaServiceProvider qnaServiceProvider;
        private readonly ILogger<TaskModuleActivity> logger;
        private readonly IQnAPairServiceFacade qnaPairServiceFacade;

        /// <summary>
        /// Initializes a new instance of the <see cref="TaskModuleActivity"/> class.
        /// </summary>
        /// <param name="configurationProvider">Configuration Provider.</param>
        /// <param name="qnaServiceProvider">QnA service provider.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="qnaPairServiceFacade">Instance of QnA pair service class to call add/update/get QnA pair.</param>
        public TaskModuleActivity(
            Common.Providers.IConfigurationDataProvider configurationProvider,
            IQnaServiceProvider qnaServiceProvider,
            ILogger<TaskModuleActivity> logger,
            IQnAPairServiceFacade qnaPairServiceFacade)
        {
            this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            this.qnaServiceProvider = qnaServiceProvider ?? throw new ArgumentNullException(nameof(qnaServiceProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.qnaPairServiceFacade = qnaPairServiceFacade ?? throw new ArgumentNullException(nameof(qnaPairServiceFacade));
        }

        /// <summary>
        /// Get TaskModuleResponse object while adding or editing the question and answer pair.
        /// </summary>
        /// <param name="questionAnswerAdaptiveCardEditor">Card as an input.</param>
        /// <param name="titleText">Gets or sets text that appears below the app name and to the right of the app icon.</param>
        /// <returns>Envelope for Task Module Response.</returns>
        public static Task<TaskModuleResponse> GetTaskModuleResponseAsync(Attachment questionAnswerAdaptiveCardEditor, string titleText = "")
        {
            return Task.FromResult(new TaskModuleResponse
            {
                Task = new TaskModuleContinueResponse
                {
                    Value = new TaskModuleTaskInfo
                    {
                        Card = questionAnswerAdaptiveCardEditor ?? throw new ArgumentNullException(nameof(questionAnswerAdaptiveCardEditor)),
                        Height = TaskModuleHeight,
                        Width = TaskModuleWidth,
                        Title = titleText ?? throw new ArgumentNullException(nameof(titleText)),
                    },
                },
            });
        }

        /// <summary>
        /// Handles click on edit button on a question in SME team.
        /// </summary>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="appBaseUri">App base uri.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public Task<TaskModuleResponse> OnFetchAsync(
            TaskModuleRequest taskModuleRequest,
            string appBaseUri)
        {
            try
            {
                var postedValues = JsonConvert.DeserializeObject<AdaptiveSubmitActionData>(JObject.Parse(taskModuleRequest?.Data?.ToString()).ToString());
                var adaptiveCardEditor = MessagingExtensionQnaCard.AddQuestionForm(postedValues, appBaseUri);
                return GetTaskModuleResponseAsync(adaptiveCardEditor, Strings.EditQuestionSubtitle);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while fetch event is received from the user.");
                throw;
            }
        }

        /// <summary>
        /// Handles submiting a edited question from SME team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="appBaseUri">App base uri.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task<TaskModuleResponse> OnSubmitAsync(
            ITurnContext<IInvokeActivity> turnContext,
            string appBaseUri)
        {
            try
            {
                var postedQuestionData = ((JObject)turnContext?.Activity?.Value).GetValue("data", StringComparison.OrdinalIgnoreCase).ToObject<AdaptiveSubmitActionData>();
                if (postedQuestionData == null)
                {
                    await turnContext.SendActivityAsync(Strings.ErrorMessage).ConfigureAwait(false);
                    return default;
                }

                if (postedQuestionData.BackButtonCommandText == Strings.BackButtonCommandText)
                {
                    // Populates the prefilled data on task module for adaptive card form fields on back button click.
                    return await GetTaskModuleResponseAsync(MessagingExtensionQnaCard.AddQuestionForm(postedQuestionData, appBaseUri), Strings.EditQuestionSubtitle).ConfigureAwait(false);
                }

                if (postedQuestionData.PreviewButtonCommandText == Constants.PreviewCardCommandText)
                {
                    // Preview the actual view of the card on preview button click.
                    return await GetTaskModuleResponseAsync(MessagingExtensionQnaCard.PreviewCardResponse(postedQuestionData, appBaseUri)).ConfigureAwait(false);
                }

                return await this.qnaPairServiceFacade.EditQnAPairAsync(postedQuestionData, turnContext).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Check if knowledge base is empty and has not published yet when sme user is trying to edit the qna pair.
                if (((ErrorResponseException)ex?.InnerException)?.Response.StatusCode == HttpStatusCode.BadRequest)
                {
                    var knowledgeBaseId = await this.configurationProvider.GetSavedEntityDetailAsync(Constants.KnowledgeBaseEntityId).ConfigureAwait(false);
                    var hasPublished = await this.qnaServiceProvider.GetInitialPublishedStatusAsync(knowledgeBaseId).ConfigureAwait(false);

                    // Check if knowledge base has not published yet.
                    if (!hasPublished)
                    {
                        this.logger.LogError(ex, "Error while fetching the qna pair: knowledge base may be empty or it has not published yet.");
                        await turnContext.SendActivityAsync("Please wait for some time, updates to this question will be available in short time.").ConfigureAwait(false);
                    }
                }
                else
                {
                    this.logger.LogError(ex, "Error while submit event is received from the user.");
                    await turnContext.SendActivityAsync(Strings.ErrorMessage).ConfigureAwait(false);
                }

                throw ex;
            }
        }
    }
}
