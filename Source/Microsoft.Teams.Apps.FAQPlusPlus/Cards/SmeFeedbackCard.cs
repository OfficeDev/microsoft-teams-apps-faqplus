// <copyright file="SmeFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    ///  This class process sending a notification card to SME team-
    ///  whenever user submits a feedback through bot menu or from response card.
    /// </summary>
    public static class SmeFeedbackCard
    {
        /// <summary>
        /// This method will construct the card for SME team which will have the
        /// feedback details given by the user.
        /// </summary>
        /// <param name="data">User activity payload.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Sme facing feedback notification card.</returns>
        public static Attachment GetCard(FeedbackEntity data, string appBaseUri)
        {
            // Constructing adaptive card that is sent to SME team.
            AdaptiveCard smeFeedbackCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
               {
                   new AdaptiveColumnSet
                   {
                       Columns = new List<AdaptiveColumn>
                       {
                           new AdaptiveColumn
                           {
                               Items = new List<AdaptiveElement>
                               {
                                   new AdaptiveImage
                                   {
                                       Style = AdaptiveImageStyle.Default,
                                       Size = AdaptiveImageSize.Medium,
                                       Url = new Uri(appBaseUri + "/content/feedback_channel.png"),
                                   },
                               },
                               Width = "auto",
                           },
                           new AdaptiveColumn
                           {
                               Items = new List<AdaptiveElement>
                               {
                                   new AdaptiveTextBlock()
                                   {
                                       Text = Strings.SMEFeedbackHeaderText,
                                       Weight = AdaptiveTextWeight.Bolder,
                                       Wrap = true,
                                   },
                                   new AdaptiveTextBlock()
                                   {
                                       Text = string.Format(CultureInfo.InvariantCulture, Strings.FeedbackAlertText, data.UserName),
                                       Spacing = AdaptiveSpacing.None,
                                       IsSubtle = true,
                                       Wrap = true,
                                   },
                               },
                               Width = "stretch",
                           },
                       },
                   },
                   new AdaptiveTextBlock()
                   {
                       Text = Strings.RatingTitle,
                       Weight = AdaptiveTextWeight.Bolder,
                       Wrap = true,
                   },
                   new AdaptiveTextBlock()
                   {
                       Text = GetRatingDisplayText(data?.Rating),
                       Spacing = AdaptiveSpacing.None,
                       Wrap = true,
                   },
               },
                Actions = new List<AdaptiveAction>
                {
                    new AdaptiveOpenUrlAction
                    {
                        Title = string.Format(CultureInfo.InvariantCulture, Strings.ChatTextButton, data.UserGivenName),
                        UrlString = $"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(data.UserPrincipalName)}",
                    },
                },
            };

            // Description fact is available in the card only when user enters description text.
            if (!string.IsNullOrWhiteSpace(data.Subject))
            {
                smeFeedbackCard.Body.Insert(2, new AdaptiveTextBlock()
                {
                    Text = Strings.SubjectFact,
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true,
                });

                smeFeedbackCard.Body.Insert(3, new AdaptiveTextBlock()
                {
                    Text = data.Subject,
                    Spacing = AdaptiveSpacing.None,
                    Wrap = true,
                });
            }

            // Description fact is available in the card only when user enters description text.
            if (!string.IsNullOrWhiteSpace(data.Description))
            {
                smeFeedbackCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = Strings.DescriptionText,
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true,
                });

                smeFeedbackCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = CardHelper.TruncateStringIfLonger(data.Description, CardHelper.DescriptionMaxDisplayLength),
                    Spacing = AdaptiveSpacing.None,
                    Wrap = true,
                });
            }

            // Question asked fact and view article show card is available when feedback is on QnA Maker response.
            if (!string.IsNullOrWhiteSpace(data.KnowledgeBaseAnswer) && !string.IsNullOrWhiteSpace(data.UserQuestion))
            {
                smeFeedbackCard.Body.Add(new AdaptiveFactSet
                {
                    Facts = new List<AdaptiveFact>
                    {
                        new AdaptiveFact()
                        {
                            Title = Strings.QuestionAskedFactTitle,
                            Value = data.UserQuestion,
                        },
                    },
                });

                smeFeedbackCard.Actions.AddRange(new List<AdaptiveAction>
                {
                    new AdaptiveShowCardAction
                    {
                        Title = Strings.ViewArticleButtonText,
                        Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                        {
                            Body = new List<AdaptiveElement>
                            {
                               new AdaptiveTextBlock
                               {
                                   Text = CardHelper.TruncateStringIfLonger(data.KnowledgeBaseAnswer, CardHelper.KnowledgeBaseAnswerMaxDisplayLength),
                                   Wrap = true,
                               },
                            },
                        },
                    },
                });
            }

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = smeFeedbackCard,
            };
        }

        // Return the display string for the given rating
        private static string GetRatingDisplayText(string rating)
        {
            if (!Enum.TryParse(rating, out FeedbackRating value))
            {
                throw new ArgumentException($"{rating} is not a valid rating value", nameof(rating));
            }

            return Strings.ResourceManager.GetString($"{rating}RatingText", CultureInfo.InvariantCulture);
        }
    }
}