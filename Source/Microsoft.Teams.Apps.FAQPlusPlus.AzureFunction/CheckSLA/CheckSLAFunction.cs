// <copyright file="CheckSLAFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.CheckSLA
{
    using System;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Extensions.Logging;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.DataQueue;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.PrepareToSendQueue;

    public class CheckSLAFunction
    {
        private readonly NotificationDataRepository notificationDataRepository;
        private readonly SentNotificationDataRepository sentNotificationDataRepository;
        private readonly PrepareToSendQueue prepareToSendQueue;

        public CheckSLAFunction(
            NotificationDataRepository notificationDataRepository,
            SentNotificationDataRepository sentNotificationDataRepository,
            PrepareToSendQueue prepareToSendQueu)
        {
            this.notificationDataRepository = notificationDataRepository;
            this.sentNotificationDataRepository = sentNotificationDataRepository;
            this.prepareToSendQueue = prepareToSendQueu;
            // Create a sent notification based on the draft notification.
            var sentNotificationEntity = new NotificationDataEntity
            {
                PartitionKey = NotificationDataTableNames.DraftNotificationsPartition,
                RowKey = "xxx1",
                Id = "xxx2",
                Title = "Test1",
                ImageLink = "Test2",
                Summary = "Test3",
                Author = "Test4",
                ButtonTitle = "Test5",
                ButtonLink = "Test6",
                CreatedBy = "Test7",
                CreatedDate = DateTime.Now,
                SentDate = null,
                IsDraft = false,
                Succeeded = 0,
                Failed = 0,
                Throttled = 0,
                SendingStartedDate = DateTime.UtcNow,
                Status = NotificationStatus.Queued.ToString(),
                GroupsInString = string.Empty,
                RostersInString = string.Empty,
                TeamsInString = string.Empty,
            };
            await this.notificationDataRepository.CreateOrUpdateAsync(sentNotificationEntity);

            var prepareToSendQueueMessageContent = new PrepareToSendQueueMessageContent
            {
                NotificationId = sentNotificationEntity.Id,
            };

            await this.prepareToSendQueue.SendAsync(prepareToSendQueueMessageContent);

            var forceCompleteDataQueueMessageContent = new DataQueueMessageContent
            {
                NotificationId = sentNotificationEntity.Id,
                ForceMessageComplete = true,
            };
        }

        [FunctionName("CheckSLAFunction")]
        public void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
        }
    }
}
