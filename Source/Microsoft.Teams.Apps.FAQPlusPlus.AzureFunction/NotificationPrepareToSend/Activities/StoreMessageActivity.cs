﻿// <copyright file="StoreMessageActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.NotificationPrepareToSend
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.AdaptiveCard;

    /// <summary>
    /// Stores the message in sending notification data table.
    /// </summary>
    public class StoreMessageActivity
    {
        private readonly SendingNotificationDataRepository sendingNotificationDataRepository;
        private readonly AdaptiveCardCreator adaptiveCardCreator;

        /// <summary>
        /// Initializes a new instance of the <see cref="StoreMessageActivity"/> class.
        /// </summary>
        /// <param name="notificationRepo">Sending notification data repository.</param>
        /// <param name="cardCreator">The adaptive card creator.</param>
        public StoreMessageActivity(
            SendingNotificationDataRepository notificationRepo,
            AdaptiveCardCreator cardCreator)
        {
            this.sendingNotificationDataRepository = notificationRepo ?? throw new ArgumentNullException(nameof(notificationRepo));
            this.adaptiveCardCreator = cardCreator ?? throw new ArgumentNullException(nameof(cardCreator));
        }

        /// <summary>
        /// Stores the message in sending notification data table.
        /// </summary>
        /// <param name="notification">A notification to be sent to recipients.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        [FunctionName(FunctionNames.StoreMessageActivity)]
        public async Task RunAsync(
            [ActivityTrigger] NotificationDataEntity notification)
        {
            var serializedContent = this.adaptiveCardCreator.CreateAdaptiveCard(notification).ToJson();

            var sendingNotification = new SendingNotificationDataEntity
            {
                PartitionKey = NotificationDataTableNames.SendingNotificationsPartition,
                RowKey = notification.RowKey,
                NotificationId = notification.Id,
                Content = serializedContent,
            };

            await this.sendingNotificationDataRepository.CreateOrUpdateAsync(sendingNotification);
        }
    }
}
