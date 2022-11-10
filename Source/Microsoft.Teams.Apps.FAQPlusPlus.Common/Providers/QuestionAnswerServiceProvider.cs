// <copyright file="QuestionAnswerServiceProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text.Json;
    using System.Threading.Tasks;
    using System.Web;
    using global::Azure;
    using global::Azure.AI.Language.QuestionAnswering;
    using global::Azure.AI.Language.QuestionAnswering.Authoring;
    using global::Azure.Core;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Question Answering service provider class.
    /// </summary>
    public class QuestionAnswerServiceProvider : IQuestionAnswerServiceProvider
    {
        /// <summary>
        /// Maximum number of answers to be returned by the Question Answering for a given question.
        /// </summary>
        private const int MaxNumberOfAnswersToFetch = 3;

        private readonly string projectName;
        private readonly string deploymentName = "production";
        private readonly string testDeploymentName = "test";

        private readonly Uri endpoint;
        private readonly AzureKeyCredential credential;
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly string qnaServiceSubscriptionKey;
        private readonly QuestionAnsweringClient client;
        private readonly QuestionAnsweringProject project;
        private readonly QuestionAnsweringProject unPublishedProject;

        private readonly QuestionAnsweringAuthoringClient questionAnsweringAuthoringClient;

        /// <summary>
        /// Represents a set of key/value application configuration properties.
        /// </summary>
        private readonly QuestionAnswerSettings options;

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionAnswerServiceProvider"/> class.
        /// </summary>
        /// <param name="configurationProvider">ConfigurationProvider fetch and store information in storage table.</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties.</param>
        /// <param name="endpoint">The end point.</param>
        /// <param name="credential">The credential.</param>
        /// <param name="projectName">The project name.</param>
        /// <param name="deploymentName">The deployment name.</param>
        /// <param name="qnaServiceSubscriptionKey">The qna service subscription key.</param>
        public QuestionAnswerServiceProvider(
            IConfigurationDataProvider configurationProvider,
            IOptionsMonitor<QuestionAnswerSettings> optionsAccessor,
            Uri endpoint,
            AzureKeyCredential credential,
            string projectName,
            string deploymentName,
            string qnaServiceSubscriptionKey)
        {
            this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            this.credential = credential ?? throw new ArgumentNullException(nameof(credential));
            this.projectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
            this.deploymentName = deploymentName ?? throw new ArgumentNullException(nameof(deploymentName));
            this.configurationProvider = configurationProvider ?? throw new ArgumentNullException(nameof(configurationProvider));
            this.options = optionsAccessor.CurrentValue ?? throw new ArgumentNullException(nameof(optionsAccessor));
            this.qnaServiceSubscriptionKey = qnaServiceSubscriptionKey ?? throw new ArgumentNullException(nameof(qnaServiceSubscriptionKey)); ;

            this.endpoint = endpoint;
            this.credential = credential;
            this.projectName = projectName;
            this.deploymentName = deploymentName;
            this.qnaServiceSubscriptionKey = qnaServiceSubscriptionKey;

            this.configurationProvider = configurationProvider;
            this.options = optionsAccessor.CurrentValue;

            this.client = new QuestionAnsweringClient(this.endpoint, this.credential);
            this.project = new QuestionAnsweringProject(this.projectName, this.deploymentName);
            this.unPublishedProject = new QuestionAnsweringProject(this.projectName, this.testDeploymentName);
            this.questionAnsweringAuthoringClient = new QuestionAnsweringAuthoringClient(this.endpoint, this.credential);
        }

        /// <summary>
        /// This method is used to add QnA pair in Kb.
        /// </summary>
        /// <param name="question">Question text.</param>
        /// <param name="combinedDescription">Answer text.</param>
        /// <param name="createdBy">Created by user.</param>
        /// <param name="conversationId">Conversation id.</param>
        /// <param name="activityReferenceId">Activity reference id refer to activityid in storage table.</param>
        /// <returns>Operation state as task.</returns>
        public async Task<Operation<AsyncPageable<BinaryData>>> AddQnaAsync(string question, string combinedDescription, string createdBy, string conversationId, string activityReferenceId)
        {
            RequestContent updateQnasRequestContent = RequestContent.Create(
                                                    new[]
                                                {
                                                    new
                                                    {
                                                            op = "add",
                                                            value = new
                                                            {
                                                                questions = new[] {question },
                                                                answer = combinedDescription?.Trim(),
                                                                metadata = new
                                                                        {
                                                                            createdat = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture),
                                                                            createdby = createdBy,
                                                                            conversationid = HttpUtility.UrlEncode(conversationId),
                                                                            activityreferenceid = activityReferenceId,
                                                                        },
                                                            },
                                                    },
                                                });

            return await this.questionAnsweringAuthoringClient.UpdateQnasAsync(WaitUntil.Started, this.projectName, updateQnasRequestContent);
        }

        /// <summary>
        /// Update Qna pair in knowledge base.
        /// </summary>
        /// <param name="questionId">Question id.</param>
        /// <param name="answer">Answer text.</param>
        /// <param name="updatedBy">Updated by user.</param>
        /// <param name="updatedQuestion">Updated question text.</param>
        /// <param name="question">Original question text.</param>
        /// <param name="metadata">The metadata.</param>
        /// <returns>Perfomed action task.</returns>
        public async Task<Operation<AsyncPageable<BinaryData>>> UpdateQnaAsync(int questionId, string answer, string updatedBy, string updatedQuestion, string question, IReadOnlyDictionary<string, string> metadata)
        {
            string[] questions = null;
            if (!string.IsNullOrEmpty(updatedQuestion?.Trim()))
            {
                questions = (updatedQuestion?.ToUpperInvariant().Trim() == question?.ToUpperInvariant().Trim()) ? new[] { question }
                                    : new[] { updatedQuestion };
            }

            RequestContent updateQnasRequestContent = RequestContent.Create(
                                                    new[]
                                                    {
                                                        new
                                                        {
                                                            op = "replace",
                                                            value = new
                                                            {
                                                                id = questionId,
                                                                Source = Constants.Source,
                                                                questions = questions,
                                                                answer = answer?.Trim(),
                                                                metadata = new
                                                                    {
                                                                      createdat = metadata.TryGetValue("createdat", out string createdat) ? createdat : DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture),
                                                                      createdby = metadata.TryGetValue("createdby", out string createdby) ? createdby : string.Empty,
                                                                      conversationid = metadata.TryGetValue("conversationid", out string conversationid) ? conversationid : string.Empty,
                                                                      activityreferenceid = metadata.TryGetValue("activityreferenceid", out string activityreferenceid) ? activityreferenceid : string.Empty,
                                                                      updatedat = DateTime.UtcNow.Ticks.ToString(CultureInfo.InvariantCulture),
                                                                      updatedby = updatedBy,
                                                                    },
                                                            },
                                                        },
                                                    });

            return await this.questionAnsweringAuthoringClient.UpdateQnasAsync(WaitUntil.Started, this.projectName, updateQnasRequestContent);
        }

        /// <summary>
        /// This method is used to delete Qna pair from KB.
        /// </summary>
        /// <param name="questionId">Question id.</param>
        /// <returns>Perfomed action task.</returns>
        public async Task<Operation<AsyncPageable<BinaryData>>> DeleteQnaAsync(int questionId)
        {
            RequestContent updateQnasRequestContent = RequestContent.Create(
                                                                        new[]
                                                                    {
                                                                        new
                                                                        {
                                                                                op = "delete",
                                                                                value = new
                                                                                {
                                                                                    id = questionId,
                                                                                },
                                                                        },
                                                                    });

            return await this.questionAnsweringAuthoringClient.UpdateQnasAsync(WaitUntil.Started, this.projectName, updateQnasRequestContent).ConfigureAwait(false); ;
        }

        /// <summary>
        /// Get answer from knowledgebase for a given question.
        /// </summary>
        /// <param name="question">Question text.</param>
        /// <param name="isTestKnowledgeBase">Prod or test.</param>
        /// <param name="previousQnAId">Id of previous question.</param>
        /// <param name="previousUserQuery">Previous question information.</param>
        /// <returns>QnaSearchResultList result as response.</returns>
        public async Task<AnswersResult> GenerateAnswerAsync(string question, bool isTestKnowledgeBase, string previousQnAId = null, string previousUserQuery = null)
        {
            string questions = question?.Trim();
            int previousQnAIds = Convert.ToInt32(previousQnAId);
            var context = new KnowledgeBaseAnswerContext(previousQnAIds);
            var project = isTestKnowledgeBase ? this.unPublishedProject : this.project;
            context.PreviousQuestion = previousUserQuery;

            AnswersOptions options = new AnswersOptions
            {
                ConfidenceThreshold = Convert.ToDouble(this.options.ScoreThreshold, CultureInfo.InvariantCulture),
                Size = MaxNumberOfAnswersToFetch,
                AnswerContext = context,
            };

            Response<AnswersResult> responseFollowUp = await this.client.GetAnswersAsync(questions, project, options).ConfigureAwait(false);

            return responseFollowUp.Value;

        }

        /// <summary>
        /// This method returns the downloaded knowledgebase documents.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledgebase Id.</param>
        /// <returns>List of question and answer document object.</returns>
        public async Task<IEnumerable<KnowledgeBaseAnswerDTO>> DownloadKnowledgebaseAsync(string knowledgeBaseId)
        {
            IEnumerable<KnowledgeBaseAnswerDTO> knowledgebaseList = null;

            var qnaDocuments = await this.questionAnsweringAuthoringClient.ExportAsync(WaitUntil.Completed, this.projectName, "json").ConfigureAwait(false);
            JsonDocument operationValueJson = JsonDocument.Parse(qnaDocuments.Value);
            string exportedFileUrl = operationValueJson.RootElement.GetProperty("resultUrl").ToString();

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(exportedFileUrl);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", this.qnaServiceSubscriptionKey);
                HttpResponseMessage response = client.GetAsync(exportedFileUrl).Result;
                if (response.IsSuccessStatusCode)
                {
                    JsonDocument exportedFileResult = JsonDocument.Parse(response.Content.ReadAsStringAsync().Result);
                    var exportedAssests = exportedFileResult.RootElement.GetProperty("Assets").GetProperty("Qnas").ToString();

                    knowledgebaseList = JsonConvert.DeserializeObject<IEnumerable<KnowledgeBaseAnswerDTO>>(exportedAssests.ToString());
                }
            }

            return knowledgebaseList;
        }

        /// <summary>
        /// Checks whether knowledgebase need to be published.
        /// </summary>
        /// <returns>A <see cref="Task"/> of type bool where true represents knowledgebase need to be published while false indicates knowledgebase not need to be published.</returns>
        public async Task<bool> GetPublishStatusAsync()
        {
            var qnaDocuments = await this.questionAnsweringAuthoringClient.GetProjectDetailsAsync(this.projectName).ConfigureAwait(false);
            var formatter = new BinaryData(qnaDocuments.Content);
            var responseJson = JObject.Parse(formatter.ToString());

            if (qnaDocuments != null && qnaDocuments != null && responseJson["lastDeployedDateTime"] != null)
            {
                return Convert.ToDateTime(responseJson["lastModifiedDateTime"]) > Convert.ToDateTime(responseJson["lastDeployedDateTime"]);
            }

            return true;
        }

        /// <summary>
        /// Method is used to publish knowledgebase.
        /// </summary>
        /// <returns>Task for published data.</returns>
        public async Task<Operation<BinaryData>> PublishKnowledgebaseAsync()
        {
            return await this.questionAnsweringAuthoringClient.DeployProjectAsync(WaitUntil.Completed, this.projectName, this.deploymentName).ConfigureAwait(false);
        }

        /// <summary>
        /// Get knowledgebase published information.
        /// </summary>
        /// <param name="knowledgeBaseId">Knowledgebase id.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents knowledgebase has published atleast once while false indicates that knowledgebase has not published yet.</returns>
        public async Task<bool> GetInitialPublishedStatusAsync(string knowledgeBaseId)
        {
            var qnaDocuments = await this.questionAnsweringAuthoringClient.GetProjectDetailsAsync(this.projectName).ConfigureAwait(false);
            var formatter = new BinaryData(qnaDocuments.Content);
            var responseJson = JObject.Parse(formatter.ToString());

            return !string.IsNullOrEmpty(responseJson["lastDeployedDateTime"].ToString());
        }
    }
}
