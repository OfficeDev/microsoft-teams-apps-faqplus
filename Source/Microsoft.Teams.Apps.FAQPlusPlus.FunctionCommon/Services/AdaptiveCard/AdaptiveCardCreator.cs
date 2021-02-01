// <copyright file="AdaptiveCardCreator.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.AdaptiveCard
{
    using System;
    using System.Collections.Generic;
    using AdaptiveCards;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData;

    /// <summary>
    /// Adaptive Card Creator service.
    /// </summary>
    public class AdaptiveCardCreator
    {
        /// <summary>
        /// Creates an adaptive card.
        /// </summary>
        /// <param name="notificationDataEntity">Notification data entity.</param>
        /// <returns>An adaptive card.</returns>
        public AdaptiveCard CreateAdaptiveCard(NotificationDataEntity notificationDataEntity)
        {
            string imgUrl = string.Empty;
            switch (notificationDataEntity.Type)
            {
                case (int)NotificationType.Info:
                    imgUrl = "https://dev-dolphin.azurewebsites.net/content/notification_info.png";
                    break;
                case (int)NotificationType.Warning:
                    imgUrl = "https://dev-dolphin.azurewebsites.net/content/notification_warning.png";
                    break;
                case (int)NotificationType.Error:
                    imgUrl = "https://dev-dolphin.azurewebsites.net/content/notification_error.png";
                    break;
                default: break;
            }

            AdaptiveCard card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 2));

            card.Body.Add(
                new AdaptiveTextBlock()
                {
                    Size = AdaptiveTextSize.Large,
                    Weight = AdaptiveTextWeight.Bolder,
                    Text = notificationDataEntity.Title,
                });
            card.Body.Add(
                 new AdaptiveColumnSet()
                 {
                     Columns = new List<AdaptiveColumn>()
                     {
                         new AdaptiveColumn()
                         {
                             Width = "auto",
                             Items = new List<AdaptiveElement>()
                             {
                                 new AdaptiveImage()
                                 {
                                     Style = AdaptiveImageStyle.Person,
                                     Size = AdaptiveImageSize.Medium,
                                     Url = new Uri(imgUrl),
                                 },
                             }
                         },
                         new AdaptiveColumn()
                         {
                             Width = "stretch",
                             Items = new List<AdaptiveElement>()
                             {

                                 new AdaptiveTextBlock()
                                 {
                                     Size = AdaptiveTextSize.Default,
                                     Weight = AdaptiveTextWeight.Bolder,
                                     Text = notificationDataEntity.Author,
                                     Wrap = true,
                                 },
                                 new AdaptiveTextBlock()
                                 {
                                     Spacing = AdaptiveSpacing.None,
                                     Text = notificationDataEntity.CreatedDate.ToLocalTime().ToString(),
                                     IsSubtle = true,
                                     Wrap = true,
                                 },
                             }
                         },
                     },
                 });

            card.Body.Add(
                new AdaptiveTextBlock()
                {
                    Text = notificationDataEntity.Summary,
                    Wrap = true,
                });

            if (notificationDataEntity.Facts != null)
            {
                AdaptiveFactSet factset = new AdaptiveFactSet();
                foreach (NotificationFact fact in notificationDataEntity.Facts)
                {
                    factset.Facts.Add(
                        new AdaptiveFact()
                        {
                            Title = fact.Title,
                            Value = fact.Value,
                        });
                }
                card.Body.Add(factset);
            }

            if (!string.IsNullOrWhiteSpace(notificationDataEntity.ButtonsInString))
            {
                foreach (NotificationButton button in notificationDataEntity.Buttons)
                {
                    card.Actions.Add(new AdaptiveOpenUrlAction()
                    {
                        Title = button.Title,
                        Url = new Uri(button.Link),
                    });
                }
            }
            return card;
        }

    }
}
