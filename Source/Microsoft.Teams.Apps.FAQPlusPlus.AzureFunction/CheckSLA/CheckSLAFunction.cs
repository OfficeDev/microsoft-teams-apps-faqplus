// <copyright file="CheckSLAFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.CheckSLA
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Host;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.SentNotificationData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.DataQueue;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.PrepareToSendQueue;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using System.Text.Encodings.Web;
    using System.Collections.Generic;

    public class CheckSLAFunction
    {
        private readonly NotificationDataRepository notificationDataRepository;
        private readonly SentNotificationDataRepository sentNotificationDataRepository;
        private readonly PrepareToSendQueue prepareToSendQueue;
        private readonly ITicketsProvider ticketProvider;
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly TableRowKeyGenerator tableRowKeyGenerator;
        private readonly IOptions<TicketExpertOptions> ticketExpertOptions;

        public CheckSLAFunction(
            IOptions<TicketExpertOptions> ticketExpertOptions,
            NotificationDataRepository notificationDataRepository,
            SentNotificationDataRepository sentNotificationDataRepository,
            PrepareToSendQueue prepareToSendQueu,
            TableRowKeyGenerator tableRowKeyGenerator,
            ITicketsProvider ticketProvider,
            IConfigurationDataProvider configurationProvider)
        {
            this.ticketExpertOptions = ticketExpertOptions;
            this.notificationDataRepository = notificationDataRepository;
            this.sentNotificationDataRepository = sentNotificationDataRepository;
            this.prepareToSendQueue = prepareToSendQueu;
            this.tableRowKeyGenerator = tableRowKeyGenerator;
            this.ticketProvider = ticketProvider;
            this.configurationProvider = configurationProvider;
        }

        [FunctionName("CheckSLAFunction")]
        public async void Run([TimerTrigger("0 */3 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var assignTimeout = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.AssignTimeout));
            var pendingTimeout = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingTimeout));
            var resolveTimeout = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.ResolveTimeout));

            var unResolvedTickets = await this.ticketProvider.GetTicketsAsync(false);

            var tasks = unResolvedTickets.Select(i => this.ProcessTicketAsync(i, assignTimeout, pendingTimeout, resolveTimeout));

            var results = await Task.WhenAll(tasks);

            log.LogInformation($"new message count: {results.Where(i => i == true).ToList().Count()}");
        }

        private async Task<bool> ProcessTicketAsync(TicketEntity ticket, int assignTimeout, int pendingTimeout, int resolveTimeout)
        {
            if (this.ShouldSendNotification(ticket, assignTimeout, pendingTimeout, resolveTimeout))
            {
                var newSentNotificationId = this.tableRowKeyGenerator.CreateNewKeyOrderingMostRecentToOldest();

                var sentNotificationEntity = new NotificationDataEntity
                {
                    PartitionKey = NotificationDataTableNames.SentNotificationsPartition,
                    RowKey = newSentNotificationId,
                    Id = newSentNotificationId,
                    Type = (int)NotificationType.Warning,
                    Title = "Ticket need to update",
                    Summary = "You receive this notification because a ticket is exceed the time defined in  SLA",
                    ButtonTitle = "Go to Ticket",
                    ButtonLink = this.BuildTicketLink(ticket.SmeThreadConversationId),
                    Author = "CTU Production Services",
                    CreatedBy = "Dolphin",
                    CreatedDate = DateTime.Now,
                    Facts = new List<NotificationFact>()
                    {
                        new NotificationFact
                        {
                            Title = "Requester:",
                            Value = ticket.RequesterName,
                        },
                        new NotificationFact
                        {
                            Title = "Title:",
                            Value = ticket.Title,
                        },
                        new NotificationFact
                        {
                            Title = "Status:",
                            Value = ((TicketState)Enum.ToObject(typeof(TicketState), ticket.Status)).ToString(),
                        },
                    },
                    SentDate = null,
                    Succeeded = 0,
                    Failed = 0,
                    Throttled = 0,
                    SendingStartedDate = DateTime.UtcNow,
                    Status = NotificationStatus.Queued.ToString(),
                    GroupsInString = string.Empty,
                    RostersInString = string.Empty,
                    TeamsInString = string.Empty,
                    UserIdInString = ticket.AssignedToObjectId,
                };

                await this.notificationDataRepository.CreateOrUpdateAsync(sentNotificationEntity);

                var prepareToSendQueueMessageContent = new PrepareToSendQueueMessageContent
                {
                    NotificationId = sentNotificationEntity.Id,
                };

                await this.prepareToSendQueue.SendAsync(prepareToSendQueueMessageContent);
                return true;
            }

            return false;
        }

        private bool ShouldSendNotification(TicketEntity ticket, int assignTimeout, int pendingTimeout, int resolveTimeout)
        {
            bool shouldNotify = false;
            if (ticket.Status == 0)
            {
                if (ticket.DateSendNotification != null)
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DateSendNotification, assignTimeout);
                }
                else
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DateCreated, assignTimeout);
                }
            }
            else if (ticket.Status == 1)
            {
                if (ticket.DateSendNotification != null)
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DateSendNotification, resolveTimeout);
                }
                else
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DateAssigned, resolveTimeout);
                }
            }
            else if (ticket.Status == 2)
            {
                if (ticket.DateSendNotification != null)
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DateSendNotification, pendingTimeout);
                }
                else
                {
                    if (ticket.DatePending != null)
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DatePending, pendingTimeout);
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return shouldNotify;
        }

        private bool IsTimeout(DateTime time, int timeoutMin)
        {
            DateTime now = DateTime.UtcNow.AddHours(8);
            return (now - time).TotalMinutes >= timeoutMin;
        }

        private string BuildTicketLink(string conversationId)
        {
            var messageId = string.Empty;
            if (conversationId != null)
            {
                var index = conversationId.IndexOf('=');
                messageId = conversationId.Substring(index + 1, conversationId.Length - index - 1);
            }

            return "https://teams.microsoft.com/l/message/" + this.ticketExpertOptions.Value.TeamId + "/" + messageId + "?tenantId=" + this.ticketExpertOptions.Value.TenantId + "&groupId=" + this.ticketExpertOptions.Value.GroupId + "&parentMessageId=" + messageId + "&teamName=" + UrlEncoder.Default.Encode(this.ticketExpertOptions.Value.TeamName) + "&channelName=" + UrlEncoder.Default.Encode(this.ticketExpertOptions.Value.ChannelName);
        }
    }
}
