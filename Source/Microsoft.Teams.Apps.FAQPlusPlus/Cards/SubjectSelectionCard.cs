// <copyright file="ResponseCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process SubjectSelectionCard- select subject before asking question.
    /// </summary>
    public static class SubjectSelectionCard
    {
        /// <summary>
        /// Construct the subject selection card - select subject before asking question.
        /// </summary>
        /// <param name="subjects">the array of subjects</param>
        /// <param name="currentSubject">current selected subject</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Response card.</returns>
        public static IEnumerable<Attachment> GetCards(Subject subjects, string currentSubject, string appBaseUri)
        {
            List<AdaptiveAction> projectActions = new List<AdaptiveAction>();
            if (subjects?.Project != null)
            {
                foreach (string subject in subjects.Project.Split(","))
                {
                    if (subject == currentSubject)
                    {
                        AdaptiveSubmitAction action = new AdaptiveSubmitAction();
                        action.Title = subject;
                        action.Data = new SubjectSelectionCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = subject,
                                Text = subject,
                                Image = appBaseUri + "/content/Star.png",
                            },
                            Subject = subject.Trim(),
                        };
                        action.AdditionalProperties.Add("iconUrl", appBaseUri + "/content/Star.png");
                        projectActions.Add(action);
                    }
                    else
                    {
                        projectActions.Add(
                       new AdaptiveSubmitAction
                       {
                           Title = subject,
                           Data = new SubjectSelectionCardPayload
                           {
                               MsTeams = new CardAction
                               {
                                   Type = ActionTypes.MessageBack,
                                   DisplayText = subject,
                                   Text = subject,
                               },
                               Subject = subject.Trim(),
                           },
                       });
                    }

                }
            }

            AdaptiveCard projectCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveTextBlock
                    {
                        Weight = AdaptiveTextWeight.Bolder,
                        Text = Strings.SubjectSelectionGreetingText,
                        Wrap = true,
                    },
                    new AdaptiveTextBlock
                    {
                        Text = Strings.SubjectSelectionHeaderText,
                        Wrap = true,
                    },
                    new AdaptiveImage
                    {
                        Type = "Image",

                        Url = new Uri(appBaseUri + "/content/Project_V1.png"),
                        Size = AdaptiveImageSize.Stretch,
                    },
                },
                Actions = projectActions,
            };

            List<AdaptiveAction> otherActions = new List<AdaptiveAction>();
            if (subjects?.Other != null)
            {
                foreach (string subject in subjects.Other.Split(","))
                {
                    if (subject == currentSubject)
                    {
                        AdaptiveSubmitAction action = new AdaptiveSubmitAction();
                        action.Title = subject;
                        action.Data = new SubjectSelectionCardPayload
                        {
                            MsTeams = new CardAction
                            {
                                Type = ActionTypes.MessageBack,
                                DisplayText = subject,
                                Text = subject,
                                Image = appBaseUri + "/content/Star.png",
                            },
                            Subject = subject.Trim(),
                        };
                        action.AdditionalProperties.Add("iconUrl", appBaseUri + "/content/Star.png");
                        otherActions.Add(action);
                    }
                    else
                    {
                        otherActions.Add(
                       new AdaptiveSubmitAction
                       {
                           Title = subject,
                           Data = new SubjectSelectionCardPayload
                           {
                               MsTeams = new CardAction
                               {
                                   Type = ActionTypes.MessageBack,
                                   DisplayText = subject,
                                   Text = subject,
                               },
                               Subject = subject.Trim(),
                           },
                       });
                    }
                }
            }

            AdaptiveCard otherCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                     new AdaptiveImage
                    {
                        Type = "Image",

                        Url = new Uri(appBaseUri + "/content/Subject_V4.png"),
                        Size = AdaptiveImageSize.Stretch,
                    },
                },
                Actions = otherActions,
            };

            List<Attachment> attachments = new List<Attachment>();
            attachments.Add(new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = projectCard,
            });
            attachments.Add(new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = otherCard,
            });
            return attachments;
        }
    }
}