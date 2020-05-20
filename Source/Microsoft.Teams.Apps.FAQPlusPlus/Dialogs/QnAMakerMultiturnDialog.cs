
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
    using Microsoft.Teams.Apps.FAQPlusPlus.Cards;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
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

        private readonly IQnaServiceProvider qnaServiceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="QnAMakerMultiturnDialog"/> class.
        /// </summary>
        /// <param name="qSP">Instantce of IQnaServiceProvider</param>
        public QnAMakerMultiturnDialog(IQnaServiceProvider qSP)
            : base(nameof(QnAMakerMultiturnDialog))
        {
            this.AddDialog(new WaterfallDialog(QnAMakerDialogName)
                .AddStep(this.CallGenerateAnswerAsync)
                .AddStep(this.CheckForMultiTurnPrompt)
                .AddStep(this.DisplayQnAResult));
            this.qnaServiceProvider = qSP;
        }

        /// <summary>
        /// 1
        /// </summary>
        /// <param name="stepContext">Context object containing information cached for a single step of conversation dialog with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>4</returns>
        private async Task<DialogTurnResult> CallGenerateAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values[CurrentQuery] = stepContext.Context.Activity.Text;
            QueryDTOContext context = stepContext.Options as QueryDTOContext;

            // Calling QnAMaker to get response.
            var response = await this.qnaServiceProvider.GenerateAnswerAsync(question: stepContext.Context.Activity.Text, isTestKnowledgeBase: false, context).ConfigureAwait(false);

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
                    var previousContextData = stepContext.Values.ContainsKey(QnAContextData) ? stepContext.Values[QnAContextData] as QueryDTOContext : new QueryDTOContext();
                    previousContextData.PreviousQnaId = answer.Id.ToString();
                    previousContextData.PreviousUserQuery = stepContext.Values[CurrentQuery].ToString();
                    stepContext.Values[QnAContextData] = previousContextData;

                    // Get multi-turn prompts card activity.
                    await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(ResponseCard.GetMultiturnCard(answer.Questions.First(), answer.Answer, answer.Context.Prompts))).ConfigureAwait(false);

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

            var previousContextData = stepContext.Values.ContainsKey(QnAContextData) ? stepContext.Values[QnAContextData] as QueryDTOContext : new QueryDTOContext();
            var previousQnAId = Convert.ToInt32(previousContextData.PreviousQnaId);
            if (previousQnAId > 0)
            {
                return await stepContext.ReplaceDialogAsync(QnAMakerDialogName, previousContextData, cancellationToken).ConfigureAwait(false);
            }

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
                        await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(ResponseCard.GetCard(answerData.Questions.FirstOrDefault(), answerData.Answer, reply))).ConfigureAwait(false);
                    }
                }
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Attachment(UnrecognizedInputCard.GetCard(reply))).ConfigureAwait(false);
            }

            return await stepContext.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
