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
                    imgUrl = "https://dev-dolphin.azurewebsites.net/content/Information.png";
                    break;
                case (int)NotificationType.Warning:
                    imgUrl = "https://dev-dolphin.azurewebsites.net/content/warning.png";
                    break;
                case (int)NotificationType.Error:
                    imgUrl = "https://dev-dolphin.azurewebsites.net/content/error.png";
                    break;

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
                                     Style = AdaptiveImageStyle.Default,
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



            if (!string.IsNullOrWhiteSpace(notificationDataEntity.ButtonTitle) && !string.IsNullOrWhiteSpace(notificationDataEntity.ButtonLink))
            {
                card.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Title = notificationDataEntity.ButtonTitle,
                    Url = new Uri(notificationDataEntity.ButtonLink),
                });
            }
            return card;
            //return this.CreateAdaptiveCard(
            //    notificationDataEntity.Title,
            //    //notificationDataEntity.ImageLink,
            //    notificationDataEntity.Summary,
            //    notificationDataEntity.Author,
            //    notificationDataEntity.ButtonTitle,
            //    notificationDataEntity.ButtonLink,
            //    notificationDataEntity.Facts);
        }

        /// <summary>
        /// Create an adaptive card instance.
        /// </summary>
        /// <param name="title">The adaptive card's title value.</param>
        /// <param name="imageUrl">The adaptive card's image URL.</param>
        /// <param name="summary">The adaptive card's summary value.</param>
        /// <param name="author">The adaptive card's author value.</param>
        /// <param name="buttonTitle">The adaptive card's button title value.</param>
        /// <param name="buttonUrl">The adaptive card's button url value.</param>
        /// <returns>The created adaptive card instance.</returns>
        public AdaptiveCard CreateAdaptiveCard(
            string title,
            //string imageUrl,
            string summary,
            string author,
            string buttonTitle,
            string buttonUrl)
        {
            var version = new AdaptiveSchemaVersion(1, 2);
            AdaptiveCard card = new AdaptiveCard(version);

            card.Body.Add(new AdaptiveTextBlock()
            {
                Text = title,
                Size = AdaptiveTextSize.ExtraLarge,
                Weight = AdaptiveTextWeight.Bolder,
                Wrap = true,
            });

            if (!string.IsNullOrWhiteSpace(summary))
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = summary,
                    Wrap = true,
                });
            }

            if (!string.IsNullOrWhiteSpace(author))
            {
                card.Body.Add(new AdaptiveTextBlock()
                {
                    Text = author,
                    Size = AdaptiveTextSize.Small,
                    Weight = AdaptiveTextWeight.Lighter,
                    Wrap = true,
                });
            }

            if (!string.IsNullOrWhiteSpace(buttonTitle)
                && !string.IsNullOrWhiteSpace(buttonUrl))
            {
                card.Actions.Add(new AdaptiveOpenUrlAction()
                {
                    Title = buttonTitle,
                    Url = new Uri(buttonUrl),
                });
            }

            return card;
        }
    }
}
