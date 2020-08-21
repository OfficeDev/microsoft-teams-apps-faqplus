
namespace Microsoft.Teams.Apps.FAQPlusPlus.Dialogs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.AI.QnA;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Schema;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Helpers;
    using Newtonsoft.Json;

    /// <summary>
    /// this is used for QnA multiturn support
    /// </summary>
    public class QnAMakerMultiturnDialog : ComponentDialog
    {
        private const string QnAMakerDialogName = "qnamaker-multiturn-dialog";
        private const string CurrentQuery = "currentQuery";
        private const string QnAContextData = "qnaContextData";
        private const string QnAPromptsData = "qnaPromptsData";
        private const string PreviousQnAId = "prevQnAId";
        private readonly string appBaseUri;

        private readonly IQnaServiceProvider qnaServiceProvider;
        private readonly BotState conversationState;
        private readonly IConversationProvider conversationProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerMultiturnDialog"/> class.
        /// </summary>
        /// <param name="qSP">Instantce of IQnaServiceProvider</param>
        /// <param name="conversationState"> conversation state</param>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for FaqPlusPlus bot.</param>
        /// <param name="conversationProvider">QnA conversation provider</param>
        public QnAMakerMultiturnDialog(IQnaServiceProvider qSP, ConversationState conversationState, IOptionsMonitor<BotSettings> optionsAccessor, IConversationProvider conversationProvider)
            : base(nameof(QnAMakerMultiturnDialog))
        {
            this.AddDialog(new WaterfallDialog(QnAMakerDialogName)
                .AddStep(this.CallGenerateAnswerAsync)
                .AddStep(this.CheckForMultiTurnPrompt)
                .AddStep(this.DisplayQnAResult));
            this.qnaServiceProvider = qSP;
            this.conversationState = conversationState;
            this.appBaseUri = optionsAccessor.CurrentValue.AppBaseUri;
            this.conversationProvider = conversationProvider;
        }

        /// <summary>
        /// Get context information cached for signal step
        /// </summary>
        /// <param name="dialogContext"> Context object containing information cached for a single step of conversation dialog with a user.</param>
        /// <returns>A dictionary stored in dialogcontext</returns>
        private static Dictionary<string, object> GetDialogOptionsValue(DialogContext dialogContext)
        {
            var dialogOptions = new Dictionary<string, object>();

            if (dialogContext.ActiveDialog.State["options"] != null)
            {
                dialogOptions = dialogContext.ActiveDialog.State["options"] as Dictionary<string, object>;
            }

            return dialogOptions;
        }

        /// <summary>
        /// Call QnA maker
        /// </summary>
        /// <param name="stepContext">Context object containing information cached for a single step of conversation dialog with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>4</returns>
        private async Task<DialogTurnResult> CallGenerateAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            List<MetadataDTO> metadata = new List<MetadataDTO>();

            stepContext.Values[CurrentQuery] = stepContext.Context.Activity.Text;

            var dialogOptions = GetDialogOptionsValue(stepContext);

            QueryDTOContext context = dialogOptions.ContainsKey(QnAContextData) ? dialogOptions[QnAContextData] as QueryDTOContext : null;

            int? qnaId = 0;
            if (dialogOptions.ContainsKey(QnAPromptsData))
            {
                var promptsData = dialogOptions[QnAPromptsData] as Dictionary<string, int?>;
                if (promptsData.TryGetValue(stepContext.Context.Activity.Text.ToLower(), out var currentQnAID))
                {
                    qnaId = currentQnAID;
                }
            }

            // A new question & answer flow
            ConversationEntity conInfo = await this.GetConversationInfoAsync(stepContext.Context, cancellationToken);
            if (qnaId == 0)
            {
                conInfo.Question = stepContext.Context.Activity.Text;
                conInfo.Turns = null;
                conInfo.FinalAnswer = null;
                conInfo.TempAnswer = null;
                var userDetails = await AdaptiveCardHelper.GetUserDetailsInPersonalChatAsync(stepContext.Context, cancellationToken).ConfigureAwait(false);
                conInfo.UserPrincipalName = userDetails.UserPrincipalName;
                conInfo.UserName = userDetails.Name;
                conInfo.ConversationId = Guid.NewGuid().ToString();
                context = null;
            }
            else
            {
                conInfo.Turns += stepContext.Context.Activity.Text + ";";
            }

            // Calling QnAMaker to get response.
            var response = await this.qnaServiceProvider.GenerateAnswerAsync(question: stepContext.Context.Activity.Text, isTestKnowledgeBase: false, qnaId, context, metadata).ConfigureAwait(false);

            // Resetting previous query.
            dialogOptions[PreviousQnAId] = -1;
            stepContext.ActiveDialog.State["options"] = dialogOptions;

            // move to next step with top qna response.
            return await stepContext.NextAsync(response, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// check if there is multiturn prompt
        /// </summary>
        /// <param name="stepContext">Context object containing information cached for a single step of conversation dialog with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>8</returns>
        private async Task<DialogTurnResult> CheckForMultiTurnPrompt(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is QnASearchResultList response && response.Answers.Count > 0)
            {
                var answer = response.Answers.First();
                if (answer.Context != null && answer.Context.Prompts != null && answer.Context.Prompts.Count() > 0)
                {
                    var dialogOptions = GetDialogOptionsValue(stepContext);

                    var previousContextData = dialogOptions.ContainsKey(QnAContextData) ? dialogOptions[QnAContextData] as QueryDTOContext : new QueryDTOContext();
                    previousContextData.PreviousQnaId = answer.Id.ToString();
                    previousContextData.PreviousUserQuery = stepContext.Values[CurrentQuery].ToString();
                    dialogOptions[QnAContextData] = previousContextData;

                    var promptsData = new Dictionary<string, int?>();
                    foreach (var prompt in answer.Context.Prompts)
                    {
                        promptsData[prompt.DisplayText.ToLower()] = prompt.QnaId;
                    }

                    dialogOptions[QnAPromptsData] = promptsData;
                    dialogOptions[PreviousQnAId] = answer.Id;

                    stepContext.ActiveDialog.State["options"] = dialogOptions;

                    // Get multi-turn prompts card activity.
                    var reply = MessageFactory.Attachment(ResponseCard.GetMultiturnCard(answer.Questions.First(), answer.Answer, answer.Context.Prompts));
                    reply.AttachmentLayout = AttachmentLayoutTypes.List;
                    await stepContext.Context.SendActivityAsync(reply);

                    // Save conversation data
                    ConversationEntity conInfo = await this.GetConversationInfoAsync(stepContext.Context, cancellationToken);
                    conInfo.TempAnswer += string.Format("{0}{1}{2}", "<Start>", answer.Answer, "<End>");
                    await this.conversationProvider.UpsertConversationAsync(conInfo).ConfigureAwait(false);

                    return new DialogTurnResult(DialogTurnStatus.Waiting);
                }
            }

            return await stepContext.NextAsync(stepContext.Result, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Display the result of QnA search
        /// </summary>
        /// <param name="stepContext">Context object containing information cached for a single step of conversation dialog with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>12</returns>
        private async Task<DialogTurnResult> DisplayQnAResult(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var queryResult = stepContext.Result as QnASearchResultList;
            string reply = stepContext.Context.Activity.Text;
            var dialogOptions = GetDialogOptionsValue(stepContext);
            var previousQnAId = Convert.ToInt32(dialogOptions[PreviousQnAId]);

            if (previousQnAId > 0)
            {
                return await stepContext.ReplaceDialogAsync(QnAMakerDialogName, dialogOptions, cancellationToken).ConfigureAwait(false);
            }

            ConversationEntity conInfo = await this.GetConversationInfoAsync(stepContext.Context, cancellationToken);
            if (queryResult.Answers.First().Id != -1)
            {
                var answerData = queryResult.Answers.First();
                AnswerModel answerModel = new AnswerModel();

                if (Validators.IsValidJSON(answerData.Answer))
                {
                    answerModel = JsonConvert.DeserializeObject<AnswerModel>(answerData.Answer);
                }

                if (!string.IsNullOrEmpty(answerModel?.Title) || !string.IsNullOrEmpty(answerModel?.Subtitle) || !string.IsNullOrEmpty(answerModel?.ImageUrl) || !string.IsNullOrEmpty(answerModel?.RedirectionUrl))
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(MessagingExtensionQnaCard.GetEndUserRichCard(reply, answerData))).ConfigureAwait(false);
                }
                else
                {
                    // Chitchat not need to return card
                    var metadata = (from r in answerData.Metadata where r.Name.Equals("editorial") & r.Value.Equals("chitchat") select r).FirstOrDefault();
                    if (metadata != null)
                    {
                        await stepContext.Context.SendActivityAsync(answerData.Answer, reply).ConfigureAwait(false);
                    }
                    else
                    {
                        string project = (from r in answerData.Metadata where r.Name.Equals("project") select r).FirstOrDefault()?.Value;
                        conInfo.Project = project;
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(ResponseCard.GetCard(answerData.Questions.FirstOrDefault(), answerData.Answer, reply, project, this.appBaseUri))).ConfigureAwait(false);
                    }
                }

                conInfo.FinalAnswer = answerData.Answer;
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInputCard.GetCard(reply, this.appBaseUri))).ConfigureAwait(false);
                conInfo.FinalAnswer = null;
            }

            await this.conversationProvider.UpsertConversationAsync(conInfo).ConfigureAwait(false);
            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Get current conversation info
        /// </summary>
        /// <returns>conversation info</returns>
        private async Task<ConversationEntity> GetConversationInfoAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var conversationStateAccessors = this.conversationState.CreateProperty<ConversationEntity>(nameof(ConversationEntity));
            var conInfo = await conversationStateAccessors.GetAsync(turnContext, () => new ConversationEntity(), cancellationToken);
            return conInfo;
        }
    }
}
