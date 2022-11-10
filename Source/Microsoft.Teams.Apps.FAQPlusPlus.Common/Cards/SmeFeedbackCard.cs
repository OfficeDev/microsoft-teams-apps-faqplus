﻿// <copyright file="SmeFeedbackCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;

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
        /// <param name="userDetails">User details.</param>
        /// <returns>Sme facing feedback notification card.</returns>
        public static Attachment GetCard(ShareFeedbackCardPayload data, TeamsChannelAccount userDetails)
        {
            var textAlignment = CultureInfo.CurrentCulture.TextInfo.IsRightToLeft ? AdaptiveHorizontalAlignment.Right : AdaptiveHorizontalAlignment.Left;

            // Constructing adaptive card that is sent to SME team.
            AdaptiveCard smeFeedbackCard = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
               {
                   new AdaptiveTextBlock()
                   {
                       Text = Strings.SMEFeedbackHeaderText,
                       Weight = AdaptiveTextWeight.Bolder,
                       Size = AdaptiveTextSize.Medium,
                       HorizontalAlignment = textAlignment,
                   },
                   new AdaptiveTextBlock()
                   {
                       Text = string.Format(CultureInfo.InvariantCulture, Strings.FeedbackAlertText, userDetails?.Name),
                       Wrap = true,
                       HorizontalAlignment = textAlignment,
                   },
                   new AdaptiveTextBlock()
                   {
                       Text = Strings.RatingTitle,
                       Weight = AdaptiveTextWeight.Bolder,
                       Wrap = true,
                       HorizontalAlignment = textAlignment,
                   },
                   new AdaptiveTextBlock()
                   {
                       Text = GetRatingDisplayText(data?.Rating),
                       Spacing = AdaptiveSpacing.None,
                       Wrap = true,
                       HorizontalAlignment = textAlignment,
                   },
               },
                Actions = new List<AdaptiveAction>
               {
                   new AdaptiveOpenUrlAction
                   {
                       Title = string.Format(CultureInfo.InvariantCulture, Strings.ChatTextButton, userDetails?.GivenName),
                       UrlString = $"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(userDetails.UserPrincipalName)}",
                   },
               },
            };

            // Description fact is available in the card only when user enters description text.
            if (!string.IsNullOrWhiteSpace(data.Description))
            {
                smeFeedbackCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = Strings.DescriptionText,
                    Weight = AdaptiveTextWeight.Bolder,
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                });

                smeFeedbackCard.Body.Add(new AdaptiveTextBlock()
                {
                    Text = CardHelper.TruncateStringIfLonger(data.Description, CardHelper.DescriptionMaxDisplayLength),
                    Spacing = AdaptiveSpacing.None,
                    Wrap = true,
                    HorizontalAlignment = textAlignment,
                });
            }

            // Question asked fact and view article show card is available when feedback is on Question Answering response.
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
                                   HorizontalAlignment = textAlignment,
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

            return Strings.ResourceManager.GetString($"{rating}RatingText", CultureInfo.CurrentCulture);
        }
    }
}