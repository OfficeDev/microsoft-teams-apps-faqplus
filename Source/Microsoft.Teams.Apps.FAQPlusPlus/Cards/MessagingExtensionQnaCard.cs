// <copyright file="MessagingExtensionQnaCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Web;
    using AdaptiveCards;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker.Models;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;
    using Newtonsoft.Json;

    /// <summary>
    /// Messaging extension: question and answer related cards.
    /// </summary>
    public static class MessagingExtensionQnaCard
    {
        /// <summary>
        /// Feedback - text that renders share feedback card.
        /// </summary>
        private const string GoToOriginalThreadUrl = "https://teams.microsoft.com/l/message/";

        /// <summary>
        /// Short date and time format to support adaptive card text feature.
        /// </summary>
        /// <remarks>
        /// refer adaptive card text feature https://docs.microsoft.com/en-us/adaptive-cards/authoring-cards/text-features#datetime-formatting-and-localization.
        /// </remarks>
        private const string AdaptiveCardShortDateTimeFormat = "{{{{DATE({0}, SHORT)}}}} {{{{TIME({1})}}}}";

        /// <summary>
        /// Date time format to support adaptive card text feature.
        /// </summary>
        /// <remarks>
        /// refer adaptive card text feature https://docs.microsoft.com/en-us/adaptive-cards/authoring-cards/text-features#datetime-formatting-and-localization.
        /// </remarks>
        private const string Rfc3339DateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";

        /// <summary>
        /// Truncate the lenth of answer to show in thumbnail card.
        /// </summary>
        private const int TruncateAnswerLength = 50;

        /// <summary>
        /// Truncate the lenth of answer to show in thumbnail card.
        /// </summary>
        private const string CardActionType = "task/fetch";

        /// <summary>
        /// Represents the add card action.
        /// </summary>
        private const string AddAction = "Add";

        /// <summary>
        /// Represents the edit card action.
        /// </summary>
        private const string EditAction = "Edit";

        /// <summary>
        /// Represents the edit card action.
        /// </summary>
        private const string DeleteAction = "Delete";

        /// <summary>
        /// Represents the command text to identify the action.
        /// </summary>
        private const string PreviewCardCommandText = "previewcard";

        /// <summary>
        /// Add question card task module.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <param name="appBaseUri">Application base URI.</param>
        /// <returns>Rich card as attachment.</returns>
        public static Attachment AddQuestionForm(AdaptiveSubmitActionData qnaPairEntity, string appBaseUri)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            if (qnaPairEntity != null)
            {
                var container = new AdaptiveContainer()
                {
                    Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = Strings.QuestionLabelText,
                            HorizontalAlignment = textAlignment,
                            Size = AdaptiveTextSize.Small,
                        },
                        new AdaptiveTextInput
                        {
                            Id = "updatedquestion",
                            MaxLength = 100,
                            Style = AdaptiveTextInputStyle.Text,
                            Value = qnaPairEntity.UpdatedQuestion?.Trim(),
                            Placeholder = Strings.QuestionPlaceholderText,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = Strings.DescriptionLabelText,
                            HorizontalAlignment = textAlignment,
                            Size = AdaptiveTextSize.Small,
                        },
                        new AdaptiveTextInput
                        {
                            Id = "description",
                            IsMultiline = true,
                            MaxLength = 500,
                            Style = AdaptiveTextInputStyle.Text,
                            Value = qnaPairEntity.Description?.Trim(),
                            Placeholder = Strings.DescriptionPlaceholderText,
                        },
                        new AdaptiveColumnSet
                            {
                                Columns = new List<AdaptiveColumn>
                                {
                                    new AdaptiveColumn
                                    {
                                        Width = AdaptiveColumnWidth.Auto,
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveImage
                                            {
                                                Url = new Uri(string.Format("{0}/content/InformationIcon.png", appBaseUri)),
                                                AltText = "Info icon",
                                            },
                                        },
                                    },
                                    new AdaptiveColumn
                                    {
                                        Width = AdaptiveColumnWidth.Auto,
                                        Items = new List<AdaptiveElement>
                                        {
                                            new AdaptiveTextBlock
                                            {
                                                Text = Strings.SuggestedText,
                                                HorizontalAlignment = textAlignment,
                                                Wrap = true,
                                                Size = AdaptiveTextSize.Small,
                                            },
                                        },
                                    },
                                },
                            },

                        new AdaptiveTextBlock
                        {
                            Text = $"**{Strings.OptionalFieldDisplayText}**",
                            HorizontalAlignment = textAlignment,
                            Size = AdaptiveTextSize.Medium,
                            Separator = true,
                        },

                        new AdaptiveTextBlock
                        {
                            Text = Strings.TitleLabelText,
                            HorizontalAlignment = textAlignment,
                            Size = AdaptiveTextSize.Small,
                        },
                        new AdaptiveTextInput
                        {
                            Id = "title",
                            MaxLength = 50,
                            Style = AdaptiveTextInputStyle.Text,
                            Value = qnaPairEntity.Title?.Trim(),
                            Placeholder = Strings.TitlePlaceholderText,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = Strings.SubtitleLabelText,
                            HorizontalAlignment = textAlignment,
                            Size = AdaptiveTextSize.Small,
                        },
                        new AdaptiveTextInput
                        {
                            Id = "subtitle",
                            MaxLength = 50,
                            Style = AdaptiveTextInputStyle.Text,
                            Value = qnaPairEntity.Subtitle?.Trim(),
                            Placeholder = Strings.SubtitlePlaceholderText,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = Strings.ImageLabelText,
                            HorizontalAlignment = textAlignment,
                            Size = AdaptiveTextSize.Small,
                        },
                        new AdaptiveTextInput
                        {
                            Id = "imageurl",
                            MaxLength = 200,
                            Style = AdaptiveTextInputStyle.Text,
                            Value = qnaPairEntity.ImageUrl?.Trim(),
                            Placeholder = Strings.ImageUrlPlaceholderText,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = Strings.RedirectionLabelText,
                            HorizontalAlignment = textAlignment,
                            Size = AdaptiveTextSize.Small,
                        },
                        new AdaptiveTextInput
                        {
                            Id = "redirectionurl",
                            MaxLength = 200,
                            Style = AdaptiveTextInputStyle.Text,
                            Value = qnaPairEntity.RedirectionUrl?.Trim(),
                            Placeholder = Strings.RedirectionUrlPlaceholderText,
                        },
                    },
                };

                if (qnaPairEntity.IsQnaNullOrEmpty)
                {
                    string errorMessageText;
                    errorMessageText = string.IsNullOrWhiteSpace(qnaPairEntity.UpdatedQuestion?.Trim()) ? Strings.EmptyQuestionErrorText : Strings.EmptyDescriptionErrorText;
                    container.Items.Add(new AdaptiveTextBlock
                    {
                        Text = errorMessageText,
                        HorizontalAlignment = textAlignment,
                        Size = AdaptiveTextSize.Small,
                        Color = AdaptiveTextColor.Attention,
                    });
                }
                else if (qnaPairEntity.IsHTMLPresent)
                {
                    container.Items.Add(new AdaptiveTextBlock
                    {
                        Text = Strings.HTMLErrorText,
                        HorizontalAlignment = textAlignment,
                        Size = AdaptiveTextSize.Small,
                        Color = AdaptiveTextColor.Attention,
                    });
                }
                else if (qnaPairEntity.IsQuestionAlreadyExists)
                {
                    container.Items.Add(new AdaptiveTextBlock
                    {
                        Text = Strings.QuestionAlreadyExistsErrorText,
                        HorizontalAlignment = textAlignment,
                        Size = AdaptiveTextSize.Small,
                        Color = AdaptiveTextColor.Attention,
                    });
                }
                else if (qnaPairEntity.IsInvalidImageUrl)
                {
                    container.Items.Add(new AdaptiveTextBlock
                    {
                        Text = Strings.InvalidImageUrlErrorText,
                        Size = AdaptiveTextSize.Small,
                        Color = AdaptiveTextColor.Attention,
                        HorizontalAlignment = textAlignment,
                    });
                }
                else if (qnaPairEntity.IsInvalidRedirectUrl)
                {
                    container.Items.Add(new AdaptiveTextBlock
                    {
                        Text = Strings.InvalidRedirectUrlText,
                        Size = AdaptiveTextSize.Small,
                        Color = AdaptiveTextColor.Attention,
                        HorizontalAlignment = textAlignment,
                    });
                }

                card.Body.Add(container);

                card.Actions.Add(
                    new AdaptiveSubmitAction()
                    {
                        Title = Strings.PreviewButtonText,
                        Data = new AdaptiveSubmitActionData
                        {
                            PreviewButtonCommandText = PreviewCardCommandText,
                            OriginalQuestion = qnaPairEntity.OriginalQuestion?.Trim(),
                            UpdateHistoryData = qnaPairEntity.UpdateHistoryData,
                        },
                    });

                card.Actions.Add(
                    new AdaptiveSubmitAction()
                    {
                        Title = Strings.SaveButtonText,
                        Data = new AdaptiveSubmitActionData
                        {
                            OriginalQuestion = qnaPairEntity.OriginalQuestion?.Trim(),
                            UpdateHistoryData = qnaPairEntity.UpdateHistoryData,
                        },
                    });
            }

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Normal card to show.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <param name="editedBy">Question edited by.</param>
        /// <param name="actionPerformed">Action performed by user.</param>
        /// <returns>Normal card as attachment.</returns>
        public static Attachment ShowNormalCard(AdaptiveSubmitActionData qnaPairEntity, string editedBy, string actionPerformed = "")
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            if (qnaPairEntity != null)
            {
                var container = new AdaptiveContainer()
                {
                    Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Default,
                            Wrap = true,
                            Text = string.Format(CultureInfo.InvariantCulture, Strings.QuestionTitle, qnaPairEntity.UpdatedQuestion?.Trim()),
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Default,
                            Wrap = true,
                            Text = $"{Strings.AnswerTitle} {qnaPairEntity.Description?.Trim()}",
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Small,
                            Wrap = true,
                            Text = actionPerformed + " " + editedBy,
                            HorizontalAlignment = textAlignment,
                        },
                    },
                };

                if (qnaPairEntity.IsTestKnowledgeBase)
                {
                    container.Items.Add(new AdaptiveTextBlock
                    {
                        Size = AdaptiveTextSize.Small,
                        Wrap = true,
                        Text = $"**{Strings.WaitMessageAnswer}**",
                        HorizontalAlignment = textAlignment,
                    });
                }

                string actionData = string.Empty;
                if (string.IsNullOrEmpty(qnaPairEntity.UpdateHistoryData))
                {
                    actionData = string.Format(
                        "{0}${1}|{2}|{3}",
                        Strings.UpdateHistoryHeadersText,
                        editedBy,
                        actionPerformed == Strings.EntryCreatedByText ? AddAction : EditAction,
                        DateTime.UtcNow.ToString(Rfc3339DateTimeFormat, CultureInfo.InvariantCulture));
                }
                else
                {
                    qnaPairEntity.UpdateHistoryData = GetLast10HistoryRecord(qnaPairEntity.UpdateHistoryData);
                    actionData = string.Format(
                        "{0}${1}|{2}|{3}",
                        qnaPairEntity.UpdateHistoryData,
                        editedBy,
                        actionPerformed == Strings.EntryCreatedByText ? AddAction : EditAction,
                        DateTime.UtcNow.ToString(Rfc3339DateTimeFormat, CultureInfo.InvariantCulture));
                }

                card.Body.Add(container);
                card.Actions.Add(
                  new AdaptiveSubmitAction()
                  {
                      Title = Strings.EditButtonText,
                      Data = new AdaptiveSubmitActionData
                      {
                          MsTeams = new CardAction
                          {
                              Type = CardActionType,
                          },
                          OriginalQuestion = qnaPairEntity.UpdatedQuestion?.Trim(),
                          UpdatedQuestion = qnaPairEntity.UpdatedQuestion?.Trim(),
                          Description = qnaPairEntity.Description != null ? qnaPairEntity.Description.Trim() : string.Empty,
                          Title = qnaPairEntity.Title != null ? qnaPairEntity.Title.Trim() : string.Empty,
                          Subtitle = qnaPairEntity.Subtitle != null ? qnaPairEntity.Subtitle.Trim() : string.Empty,
                          ImageUrl = qnaPairEntity.ImageUrl ?? string.Empty,
                          RedirectionUrl = qnaPairEntity.RedirectionUrl ?? string.Empty,
                          UpdateHistoryData = actionData,
                      },
                  });

                card.Actions.Add(
                    new AdaptiveShowCardAction()
                    {
                        Title = Strings.DeleteButtonText,
                        Card = DeleteEntry(qnaPairEntity.UpdatedQuestion?.Trim(), actionData),
                    });

                card.Actions.Add(
                   new AdaptiveShowCardAction()
                   {
                       Title = Strings.UpdateHistoryButtonText,
                       Card = UpdateHistory(actionData),
                   });
            }

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Confirmation card for delete button click.
        /// </summary>
        /// <param name="question">Question.</param>
        /// <param name="updateHistoryData">Holds the updated history data.</param>
        /// <returns>Delete confirmation adaptive card.</returns>
        public static AdaptiveCard DeleteEntry(string question, string updateHistoryData)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            var container = new AdaptiveContainer()
            {
                Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                           Text = Strings.DeleteConfirmation,
                           Wrap = true,
                           HorizontalAlignment = textAlignment,
                        },
                    },
            };
            card.Body.Add(container);

            card.Actions.Add(
               new AdaptiveSubmitAction()
               {
                   Title = Strings.Yes,
                   Data = new AdaptiveSubmitActionData
                   {
                       MsTeams = new CardAction
                       {
                           Type = ActionTypes.MessageBack,
                           Text = Constants.DeleteCommand,
                       },
                       OriginalQuestion = question,
                       UpdateHistoryData = updateHistoryData,
                   },
               });

            card.Actions.Add(
              new AdaptiveSubmitAction()
              {
                  Title = Strings.No,
                  Data = new AdaptiveSubmitActionData
                  {
                      MsTeams = new CardAction
                      {
                          Type = ActionTypes.MessageBack,
                          Text = Constants.NoCommand,
                      },
                  },
              });

            return card;
        }

        /// <summary>
        /// Messaging extension attachment of all answers.
        /// </summary>
        /// <param name="qnaDocuments">List of question and answer object.</param>
        /// <param name="activitiesData">All activities mapping object.</param>
        /// <returns>Returns the list of all questions to show in messaging extension.</returns>
        public static IList<MessagingExtensionAttachment> GetAllKbQuestionsCard(IList<AzureSearchEntity> qnaDocuments, IEnumerable<ActivityEntity> activitiesData)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            var messagingExtensionAttachments = new List<MessagingExtensionAttachment>();

            if (qnaDocuments != null)
            {
                foreach (var qnaDocument in qnaDocuments)
                {
                    string customMessage = string.Empty;

                    DateTime createdAt = default;
                    if (qnaDocument.Metadata.Count > 1)
                    {
                        var createdAtvalue = qnaDocument.Metadata.FirstOrDefault(metadata => metadata.Name == Constants.MetadataCreatedAt)?.Value;
                        createdAt = createdAtvalue != null ? new DateTime(long.Parse(createdAtvalue, CultureInfo.InvariantCulture)) : default;
                    }

                    string conversationId = string.Empty;
                    string activityId = string.Empty;
                    string activityReferenceId = string.Empty;
                    string dateString = string.Empty;
                    string answer = string.Empty;

                    if (Validators.IsValidJSON(qnaDocument.Answer))
                    {
                        answer = JsonConvert.DeserializeObject<AnswerModel>(qnaDocument.Answer)?.Description;
                    }
                    else
                    {
                        answer = qnaDocument.Answer;
                    }

                    string metadataCreatedAt = string.Empty;

                    if (qnaDocument.Metadata.Count > 1)
                    {
                        activityReferenceId = qnaDocument.Metadata.FirstOrDefault(metadata => metadata.Name == Constants.MetadataActivityReferenceId)?.Value;
                        conversationId = qnaDocument.Metadata.FirstOrDefault(metadata => metadata.Name == Constants.MetadataConversationId)?.Value;
                        activityId = activitiesData?.FirstOrDefault(activity => activity.ActivityReferenceId == activityReferenceId)?.ActivityId;
                        dateString = string.Format(CultureInfo.InvariantCulture, Strings.DateFormat, "{{DATE(" + createdAt.ToString(Rfc3339DateTimeFormat, CultureInfo.InvariantCulture) + ", SHORT)}}", "{{TIME(" + createdAt.ToString(Rfc3339DateTimeFormat, CultureInfo.InvariantCulture) + ")}}");
                        metadataCreatedAt = qnaDocument.Metadata.FirstOrDefault(metadata => metadata.Name == Constants.MetadataCreatedAt)?.Value;
                    }
                    else
                    {
                        customMessage = string.IsNullOrEmpty(metadataCreatedAt) ? Strings.ManuallyAddedQuestionMessage : string.Empty;
                    }

                    var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Text = string.Format(CultureInfo.InvariantCulture, Strings.QuestionTitle, qnaDocument.Questions[0]),
                                Size = AdaptiveTextSize.Default,
                                Wrap = true,
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveTextBlock
                            {
                                Text = $"{Strings.AnswerTitle} {answer}",
                                Size = AdaptiveTextSize.Default,
                                Wrap = true,
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveTextBlock
                            {
                                Text = dateString,
                                Wrap = true,
                                HorizontalAlignment = textAlignment,
                            },
                        },
                    };

                    if (!string.IsNullOrEmpty(conversationId) && !string.IsNullOrEmpty(activityId))
                    {
                        var threadId = HttpUtility.UrlDecode(conversationId);
                        var messageId = activityId;
                        card.Actions.Add(
                            new AdaptiveOpenUrlAction()
                            {
                                Title = Strings.GoToThread,
                                Url = new Uri($"{GoToOriginalThreadUrl}{threadId}/{messageId}"),
                            });
                    }

                    ThumbnailCard previewCard = new ThumbnailCard
                    {
                        Title = $"<b>{HttpUtility.HtmlEncode(qnaDocument.Questions[0])}</b>",
                        Subtitle = answer.Length <= TruncateAnswerLength ? HttpUtility.HtmlEncode(answer) : HttpUtility.HtmlEncode(answer.Substring(0, 45)) + "...",
                        Text = string.IsNullOrEmpty(customMessage) ? HttpUtility.HtmlEncode(createdAt) : customMessage,
                    };

                    messagingExtensionAttachments.Add(new Attachment
                    {
                        ContentType = AdaptiveCard.ContentType,
                        Content = card,
                    }.ToMessagingExtensionAttachment(previewCard.ToAttachment()));
                }
            }

            return messagingExtensionAttachments;
        }

        /// <summary>
        /// Deleted question and answer card.
        /// </summary>
        /// <param name="question">Question.</param>
        /// <param name="answerData">Answer.</param>
        /// <param name="deletedBy">Deleted user name.</param>
        /// <param name="oldActions">Actions performed by users.</param>
        /// <returns>Deleted card as response.</returns>
        public static Attachment DeletedEntry(string question, string answerData, string deletedBy, string oldActions)
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            AnswerModel answerModel = new AnswerModel();
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            if (Validators.IsValidJSON(answerData))
            {
                answerModel = JsonConvert.DeserializeObject<AnswerModel>(answerData);
            }

            if (answerModel != null && (!string.IsNullOrEmpty(answerModel.Title)
                || !string.IsNullOrEmpty(answerModel.Subtitle)
                || !string.IsNullOrEmpty(answerModel.ImageUrl)
                || !string.IsNullOrEmpty(answerModel.RedirectionUrl)))
            {
                var container = new AdaptiveContainer()
                {
                    Items = new List<AdaptiveElement>
                {
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Small,
                            Wrap = true,
                            Text = string.Format(CultureInfo.InvariantCulture, Strings.DeletedQnaPair, deletedBy),
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = answerModel.Title,
                            Size = AdaptiveTextSize.Large,
                            Weight = AdaptiveTextWeight.Bolder,
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = answerModel.Subtitle,
                            Size = AdaptiveTextSize.Medium,
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveImage
                        {
                            Url = !string.IsNullOrEmpty(answerModel?.ImageUrl?.Trim()) ? new Uri(answerModel?.ImageUrl?.Trim()) : default,
                            Size = AdaptiveImageSize.Auto,
                            Style = AdaptiveImageStyle.Default,
                            AltText = answerModel.Title,
                        },
                        new AdaptiveTextBlock
                        {
                            Text = answerModel.Description,
                            Size = AdaptiveTextSize.Small,
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Small,
                            Wrap = true,
                            Text = $"**{Strings.WaitMessageAnswer}**",
                            HorizontalAlignment = textAlignment,
                        },
                },
                };

                card.Body.Add(container);
            }
            else
            {
                var container = new AdaptiveContainer()
                {
                    Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Small,
                            Wrap = true,
                            Text = string.Format(CultureInfo.InvariantCulture, Strings.DeletedQnaPair, deletedBy),
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Default,
                            Wrap = true,
                            Text = string.Format(CultureInfo.InvariantCulture, Strings.QuestionTitle, question),
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Default,
                            Wrap = true,
                            Text = $"{Strings.AnswerTitle} {answerData}",
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Small,
                            Wrap = true,
                            Text = $"**{Strings.WaitMessageAnswer}**",
                            HorizontalAlignment = textAlignment,
                        },
                    },
                };
                card.Body.Add(container);
            }

            string actionData = string.IsNullOrEmpty(oldActions)
                ? string.Format("{0}${1}|{2}|{3}", Strings.UpdateHistoryHeadersText, deletedBy, DeleteAction, DateTime.UtcNow.ToString(Rfc3339DateTimeFormat, CultureInfo.InvariantCulture))
                : string.Format("{0}${1}|{2}|{3}", oldActions, deletedBy, DeleteAction, DateTime.UtcNow.ToString(Rfc3339DateTimeFormat, CultureInfo.InvariantCulture));

            card.Actions.Add(
              new AdaptiveShowCardAction()
              {
                  Title = Strings.UpdateHistoryButtonText,
                  Card = UpdateHistory(actionData),
              });

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Construct the rich card as response - when user preview a new QnA pair while adding it.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <param name="editedBy">Question edited by.</param>
        /// <param name="actionPerformed">Action performed by user.</param>
        /// <returns>Rich card as attachment.</returns>
        public static Attachment ShowRichCard(AdaptiveSubmitActionData qnaPairEntity, string editedBy = "", string actionPerformed = "")
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            string answer = string.Empty;

            if (qnaPairEntity != null)
            {
                if (Validators.IsValidJSON(qnaPairEntity.Description))
                {
                    var answerModel = JsonConvert.DeserializeObject<AnswerModel>(qnaPairEntity.Description);
                    answer = answerModel.Description;
                }
                else
                {
                    answer = qnaPairEntity.Description;
                }

                if (qnaPairEntity.IsPreviewCard)
                {
                    var container = new AdaptiveContainer()
                    {
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Text = qnaPairEntity.Title !=null ? qnaPairEntity.Title.Trim() : string.Empty,
                                Size = AdaptiveTextSize.Large,
                                Weight = AdaptiveTextWeight.Bolder,
                                Wrap = true,
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveTextBlock
                            {
                                Text = qnaPairEntity.Subtitle != null ? qnaPairEntity.Subtitle.Trim() : string.Empty,
                                Size = AdaptiveTextSize.Medium,
                                Wrap = true,
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveImage
                            {
                                Url = !string.IsNullOrEmpty(qnaPairEntity.ImageUrl?.Trim()) ? new Uri(qnaPairEntity.ImageUrl?.Trim()) : default,
                                Size = AdaptiveImageSize.Auto,
                                Style = AdaptiveImageStyle.Default,
                                AltText = qnaPairEntity.Title?.Trim(),
                            },
                            new AdaptiveTextBlock
                            {
                                Text = qnaPairEntity.Description != null ? qnaPairEntity.Description.Trim() : string.Empty,
                                Size = AdaptiveTextSize.Small,
                                Wrap = true,
                                HorizontalAlignment = textAlignment,
                            },
                        },
                    };

                    card.Body.Add(container);

                    if (!string.IsNullOrEmpty(qnaPairEntity.RedirectionUrl))
                    {
                        card.Actions.Add(
                            new AdaptiveOpenUrlAction
                            {
                                Title = Strings.OpenArticle,
                                Url = new Uri(qnaPairEntity.RedirectionUrl),
                            });
                    }

                    card.Actions.Add(
                        new AdaptiveSubmitAction()
                        {
                            Title = Strings.BackButtonText,
                            Data = new AdaptiveSubmitActionData
                            {
                                BackButtonCommandText = Strings.BackButtonCommandText,
                                UpdatedQuestion = qnaPairEntity.UpdatedQuestion?.Trim(),
                                OriginalQuestion = qnaPairEntity.OriginalQuestion?.Trim(),
                                Description = qnaPairEntity.Description != null ? qnaPairEntity.Description.Trim() : string.Empty,
                                Title = qnaPairEntity.Title != null ? qnaPairEntity.Title.Trim() : string.Empty,
                                Subtitle = qnaPairEntity.Subtitle != null ? qnaPairEntity.Subtitle.Trim() : string.Empty,
                                ImageUrl = qnaPairEntity.ImageUrl ?? string.Empty,
                                RedirectionUrl = qnaPairEntity.RedirectionUrl ?? string.Empty,
                                UpdateHistoryData = qnaPairEntity.UpdateHistoryData,
                            },
                        });
                }
                else
                {
                    var container = new AdaptiveContainer()
                    {
                        Items = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Size = AdaptiveTextSize.Default,
                                Wrap = true,
                                Text = string.Format(CultureInfo.InvariantCulture, Strings.QuestionTitle, qnaPairEntity.UpdatedQuestion?.Trim()),
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveTextBlock
                            {
                                Size = AdaptiveTextSize.Default,
                                Wrap = true,
                                Text = string.IsNullOrWhiteSpace(qnaPairEntity.Description)
                                ? qnaPairEntity.Description
                                : $"{Strings.AnswerTitle} {answer}",
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveTextBlock
                            {
                                Wrap = true,
                                Text = qnaPairEntity.Title?.Trim(),
                                Size = AdaptiveTextSize.Large,
                                Weight = AdaptiveTextWeight.Bolder,
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveTextBlock
                            {
                                Wrap = true,
                                Text = qnaPairEntity.Subtitle?.Trim(),
                                Size = AdaptiveTextSize.Medium,
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveImage
                            {
                                Url = !string.IsNullOrEmpty(qnaPairEntity.ImageUrl?.Trim()) ? new Uri(qnaPairEntity.ImageUrl?.Trim()) : default,
                                Size = AdaptiveImageSize.Auto,
                                Style = AdaptiveImageStyle.Default,
                                AltText = qnaPairEntity.Title?.Trim(),
                            },
                            new AdaptiveTextBlock
                            {
                                Text = qnaPairEntity.Description?.Trim(),
                                Size = AdaptiveTextSize.Small,
                                Wrap = true,
                                HorizontalAlignment = textAlignment,
                            },
                            new AdaptiveTextBlock
                            {
                                Size = AdaptiveTextSize.Small,
                                Wrap = true,
                                Text = actionPerformed + " " + editedBy,
                                HorizontalAlignment = textAlignment,
                            },
                        },
                    };

                    card.Body.Add(container);

                    if (qnaPairEntity.IsTestKnowledgeBase)
                    {
                        container.Items.Add(new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Small,
                            Wrap = true,
                            Text = $"**{Strings.WaitMessageAnswer}**",
                            HorizontalAlignment = textAlignment,
                        });
                    }

                    string actionData = string.Empty;
                    if (string.IsNullOrEmpty(qnaPairEntity.UpdateHistoryData))
                    {
                        actionData = string.Format(
                            "{0}${1}|{2}|{3}",
                            Strings.UpdateHistoryHeadersText,
                            editedBy,
                            actionPerformed == Strings.EntryCreatedByText ? AddAction : EditAction,
                            DateTime.UtcNow.ToString(Rfc3339DateTimeFormat, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        qnaPairEntity.UpdateHistoryData = GetLast10HistoryRecord(qnaPairEntity.UpdateHistoryData);
                        actionData = string.Format(
                            "{0}${1}|{2}|{3}",
                            qnaPairEntity.UpdateHistoryData,
                            editedBy,
                            actionPerformed == Strings.EntryCreatedByText ? AddAction : EditAction,
                            DateTime.UtcNow.ToString(Rfc3339DateTimeFormat, CultureInfo.InvariantCulture));
                    }

                    card.Actions.Add(
                      new AdaptiveSubmitAction()
                      {
                          Title = Strings.EditButtonText,
                          Data = new AdaptiveSubmitActionData
                          {
                              MsTeams = new CardAction
                              {
                                  Type = CardActionType,
                              },
                              OriginalQuestion = qnaPairEntity.UpdatedQuestion?.Trim(),
                              UpdatedQuestion = qnaPairEntity.UpdatedQuestion?.Trim(),
                              Description = qnaPairEntity.Description != null ? qnaPairEntity.Description.Trim() : string.Empty,
                              Title = qnaPairEntity.Title != null ? qnaPairEntity.Title.Trim() : string.Empty,
                              Subtitle = qnaPairEntity.Subtitle != null ? qnaPairEntity.Subtitle.Trim() : string.Empty,
                              ImageUrl = qnaPairEntity.ImageUrl ?? string.Empty,
                              RedirectionUrl = qnaPairEntity.RedirectionUrl ?? string.Empty,
                              UpdateHistoryData = actionData,
                          },
                      });

                    card.Actions.Add(
                        new AdaptiveShowCardAction()
                        {
                            Title = Strings.DeleteButtonText,
                            Card = DeleteEntry(qnaPairEntity.UpdatedQuestion?.Trim(), actionData),
                        });

                    card.Actions.Add(
                       new AdaptiveShowCardAction()
                       {
                           Title = Strings.UpdateHistoryButtonText,
                           Card = UpdateHistory(actionData),
                       });
                }
            }

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Construct the normal card as response - when user preview a new QnA pair while adding it.
        /// </summary>
        /// <param name="qnaPairEntity">Qna pair entity that contains question and answer information.</param>
        /// <returns>Preview normal card attachment.</returns>
        public static Attachment PreviewNormalCard(AdaptiveSubmitActionData qnaPairEntity)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));

            if (qnaPairEntity != null)
            {
                var container = new AdaptiveContainer()
                {
                    Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Default,
                            Wrap = true,
                            Text = qnaPairEntity.UpdatedQuestion?.Trim(),
                            HorizontalAlignment = textAlignment,
                        },
                        new AdaptiveTextBlock
                        {
                            Size = AdaptiveTextSize.Default,
                            Wrap = true,
                            Text = qnaPairEntity.Description?.Trim(),
                            HorizontalAlignment = textAlignment,
                        },
                    },
                };
                card.Body.Add(container);

                card.Actions.Add(
                        new AdaptiveSubmitAction()
                        {
                            Title = Strings.BackButtonText,
                            Data = new AdaptiveSubmitActionData
                            {
                                BackButtonCommandText = Strings.BackButtonCommandText,
                                UpdatedQuestion = qnaPairEntity.UpdatedQuestion?.Trim(),
                                OriginalQuestion = qnaPairEntity.OriginalQuestion?.Trim(),
                                Description = qnaPairEntity.Description != null ? qnaPairEntity.Description.Trim() : string.Empty,
                                Subtitle = qnaPairEntity.Subtitle != null ? qnaPairEntity.Subtitle.Trim() : string.Empty,
                                RedirectionUrl = qnaPairEntity.RedirectionUrl ?? string.Empty,
                                UpdateHistoryData = qnaPairEntity.UpdateHistoryData?.Trim(),
                            },
                        });
            }

            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };

            return adaptiveCardAttachment;
        }

        /// <summary>
        /// Construct the update history card - when user clicks on update history button on newly added card.
        /// </summary>
        /// <param name="actionsPerformed">Update history data.</param>
        /// <returns>Returns a adaptive card which shows the history of actions performed by users.</returns>
        public static AdaptiveCard UpdateHistory(string actionsPerformed)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            List<AdaptiveElement> updatedHistoryData = new List<AdaptiveElement>();
            IList<string> userActions = actionsPerformed?.Split("$").ToList();
            string headerData = userActions[0]; // Save header row.
            userActions.RemoveAt(0); // Remove header row.
            userActions.Add(headerData); // Add header row at the bottom of all records to show the data in descending order.

            for (var i = userActions.Count - 1; i >= 0; i--)
            {
                AdaptiveColumnSet userActionData = new AdaptiveColumnSet();
                AdaptiveColumn userName = new AdaptiveColumn();
                AdaptiveColumn action = new AdaptiveColumn();
                AdaptiveColumn timeStamp = new AdaptiveColumn();

                if (userActions[i].Length != 0)
                {
                    string[] splitValue = userActions[i].Split("|");
                    var timeStampData = splitValue[2] == Strings.UpdateHistoryDateHeaderText ? Strings.UpdateHistoryDateHeaderText : string.Format(CultureInfo.InvariantCulture, AdaptiveCardShortDateTimeFormat, splitValue[2], splitValue[2]);
                    userName.Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = i == userActions.Count - 1 ? $"**{splitValue[0]}**" : splitValue[0],
                            Wrap = true,
                            Size = AdaptiveTextSize.Default,
                            HorizontalAlignment = textAlignment,
                        },
                    };

                    action.Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = i == userActions.Count - 1 ? $"**{splitValue[1]}**" : splitValue[1],
                            Wrap = true,
                            Size = AdaptiveTextSize.Default,
                            HorizontalAlignment = textAlignment,
                        },
                    };

                    timeStamp.Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                            Text = i == userActions.Count - 1 ? $"**{timeStampData}**" : timeStampData,
                            Wrap = true,
                            Size = AdaptiveTextSize.Default,
                            HorizontalAlignment = textAlignment,
                        },
                    };
                    userActionData.Columns.Add(userName);
                    userActionData.Columns.Add(action);
                    userActionData.Columns.Add(timeStamp);
                    updatedHistoryData.Add(userActionData);
                }
            }

            // Get top 10 records including header values.
            card.Body = updatedHistoryData.Count > 11 ? updatedHistoryData.Take(11).ToList() : updatedHistoryData;
            return card;
        }

        /// <summary>
        /// Construct the card to render when there's an unrecognized input in a channel.
        /// </summary>
        /// <returns>Card as attachment.</returns>
        public static Attachment UnauthorizedUserActionCard()
        {
            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0));
            var container = new AdaptiveContainer()
            {
                Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock
                        {
                           Text = Strings.NonSMEErrorText,
                           Wrap = true,
                           HorizontalAlignment = AdaptiveHorizontalAlignment.Center,
                        },
                    },
            };
            card.Body.Add(container);

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        ///  Construct the card with validation errors while previewing it.
        /// </summary>
        /// <param name = "qnaPairEntity" >Qna pair entity that contains question and answer information.</param>
        /// <param name="appBaseUri">Application base uri.</param>
        /// <returns>Preview card attachment.</returns>
        public static Attachment PreviewCardResponse(AdaptiveSubmitActionData qnaPairEntity, string appBaseUri)
        {
            Attachment adaptiveCardEditor = new Attachment();

            if (Validators.IsQnaFieldsNullOrEmpty(qnaPairEntity))
            {
                qnaPairEntity.IsQnaNullOrEmpty = true;
                adaptiveCardEditor = AddQuestionForm(qnaPairEntity, appBaseUri);
            }
            else if (Validators.IsContainsHtml(qnaPairEntity))
            {
                // Show error if user has added HTML tags in Question, ImageURL, RedirectURL, description fields.
                qnaPairEntity.IsHTMLPresent = true;
                adaptiveCardEditor = AddQuestionForm(qnaPairEntity, appBaseUri);
            }
            else if (!string.IsNullOrEmpty(qnaPairEntity.UpdatedQuestion?.Trim())
                && !string.IsNullOrEmpty(qnaPairEntity.Description?.Trim())
                && string.IsNullOrEmpty(qnaPairEntity.Title?.Trim())
                && string.IsNullOrEmpty(qnaPairEntity.ImageUrl?.Trim())
                && string.IsNullOrEmpty(qnaPairEntity.Subtitle?.Trim())
                && string.IsNullOrEmpty(qnaPairEntity.RedirectionUrl?.Trim()))
            {
                // Show normal card if only question & answer fields are filled.
                adaptiveCardEditor = PreviewNormalCard(qnaPairEntity);
            }
            else if (!string.IsNullOrEmpty(qnaPairEntity.ImageUrl)
                && Validators.IsImageUrlInvalid(qnaPairEntity))
            {
                qnaPairEntity.IsInvalidImageUrl = true;
                adaptiveCardEditor = AddQuestionForm(qnaPairEntity, appBaseUri);
            }
            else if (!string.IsNullOrEmpty(qnaPairEntity.RedirectionUrl?.Trim())
                && !Regex.IsMatch(qnaPairEntity.RedirectionUrl?.Trim(), Constants.ValidRedirectUrlPattern))
            {
                // Show error if user has entered invalid Redirect URL.
                qnaPairEntity.IsInvalidRedirectUrl = true;
                adaptiveCardEditor = AddQuestionForm(qnaPairEntity, appBaseUri);
            }
            else
            {
                qnaPairEntity.IsPreviewCard = true;
                adaptiveCardEditor = ShowRichCard(qnaPairEntity);
            }

            return adaptiveCardEditor;
        }

        /// <summary>
        /// Get the latest 10 update history records.
        /// </summary>
        /// <param name="actionsPerformed">action record details in following Name|Action|Date$ format.</param>
        /// <returns>action record string with latest 10 entries.</returns>
        /// <remarks>method skips the header.</remarks>
        private static string GetLast10HistoryRecord(string actionsPerformed)
        {
            var userActions = actionsPerformed?.Split("$").ToList();

            if (userActions.Count > 10)
            {
                // remove header
                string header = userActions[0];
                userActions.Remove(header);

                return string.Format("{0}${1}", header, string.Join("$", userActions.TakeLast(10)));
            }

            return actionsPerformed;
        }
    }
}
