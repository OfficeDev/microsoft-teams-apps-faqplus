﻿// <copyright file="HandleFailureActivity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.NotificationPrepareToSend
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.DurableTask;
    using Microsoft.Extensions.Localization;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Resources;

    /// <summary>
    /// This class contains the "clean up" durable activity.
    /// If exceptions happen in the "prepare to send" operation, this method is called to log the exception.
    /// </summary>
    public class HandleFailureActivity
    {
        private readonly NotificationDataRepository notificationDataRepository;
        private readonly IStringLocalizer<Strings> localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="HandleFailureActivity"/> class.
        /// </summary>
        /// <param name="notificationDataRepository">Notification data repository.</param>
        /// <param name="localizer">Localization service.</param>
        public HandleFailureActivity(
            NotificationDataRepository notificationDataRepository,
            IStringLocalizer<Strings> localizer)
        {
            this.notificationDataRepository = notificationDataRepository ?? throw new ArgumentNullException(nameof(notificationDataRepository));
            this.localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        /// <summary>
        /// This method represents the "clean up" durable activity.
        /// If exceptions happen in the "prepare to send" operation,
        /// this method is called to do the clean up work, e.g. log the exception and etc.
        /// </summary>
        /// <param name="input">Input value.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        [FunctionName(FunctionNames.HandleFailureActivity)]
        public async Task RunAsync(
            [ActivityTrigger](NotificationDataEntity notification, Exception exception) input)
        {
            var errorMessage = this.localizer.GetString("FailtoPrepareMessageFormat", input.exception.Message);
            await this.notificationDataRepository
                .SaveExceptionInNotificationDataEntityAsync(input.notification.Id, errorMessage);
        }
    }
}