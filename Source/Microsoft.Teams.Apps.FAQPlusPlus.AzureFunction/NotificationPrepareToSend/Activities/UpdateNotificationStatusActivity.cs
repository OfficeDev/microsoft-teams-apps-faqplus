﻿// <copyright file="UpdateNotificationStatusActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.NotificationPrepareToSend
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData;

    /// <summary>
    /// Update notification status activity.
    /// </summary>
    public class UpdateNotificationStatusActivity
    {
        private readonly NotificationDataRepository notificationRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateNotificationStatusActivity"/> class.
        /// </summary>
        /// <param name="notificationRepository">Notification data repository.</param>
        public UpdateNotificationStatusActivity(NotificationDataRepository notificationRepository)
        {
            this.notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
        }

        /// <summary>
        /// Updates notification status.
        /// </summary>
        /// <param name="input">Input.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName(FunctionNames.UpdateNotificationStatusActivity)]
        public async Task RunAsync(
            [ActivityTrigger](string notificationId, NotificationStatus status) input)
        {
            await this.notificationRepository.UpdateNotificationStatusAsync(input.notificationId, input.status);
        }
    }
}
