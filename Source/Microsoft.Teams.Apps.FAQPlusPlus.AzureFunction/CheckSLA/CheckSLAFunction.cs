// <copyright file="CheckSLAFunction.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.CheckSLA
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.Encodings.Web;
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

    public class CheckSLAFunction
    {
        private readonly NotificationDataRepository notificationDataRepository;
        private readonly PrepareToSendQueue prepareToSendQueue;
        private readonly ITicketsProvider ticketProvider;
        private readonly IConfigurationDataProvider configurationProvider;
        private readonly TableRowKeyGenerator tableRowKeyGenerator;
        private readonly IOptions<TicketExpertOptions> ticketExpertOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckSLAFunction"/> class.
        /// </summary>
        /// <param name="ticketExpertOptions">ticket options.</param>
        /// <param name="notificationDataRepository">table storage to store notification data.</param>
        /// <param name="prepareToSendQueu">service bus prepare queue.</param>
        /// <param name="tableRowKeyGenerator">to generate key of table row.</param>
        /// <param name="ticketProvider">table storage to store ticket.</param>
        /// <param name="configurationProvider">table storage wehere store SLA configuration.</param>
        public CheckSLAFunction(
            IOptions<TicketExpertOptions> ticketExpertOptions,
            NotificationDataRepository notificationDataRepository,
            PrepareToSendQueue prepareToSendQueu,
            TableRowKeyGenerator tableRowKeyGenerator,
            ITicketsProvider ticketProvider,
            IConfigurationDataProvider configurationProvider)
        {
            this.ticketExpertOptions = ticketExpertOptions;
            this.notificationDataRepository = notificationDataRepository;
            this.prepareToSendQueue = prepareToSendQueu;
            this.tableRowKeyGenerator = tableRowKeyGenerator;
            this.ticketProvider = ticketProvider;
            this.configurationProvider = configurationProvider;
        }

        /// <summary>
        /// Azure Function App triggered by timer.
        /// </summary>
        /// <param name="myTimer">The timer.</param>
        /// <param name="log">Logger.</param>
        [FunctionName("CheckSLAFunction")]
        public async void Run([TimerTrigger("0 */3 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            var assignTimeout = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.AssignTimeout));
            var unassignInterval = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.UnassigneInterval));
            var pendingTimeout = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingTimeout));
            var pendingInterval = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingInterval));
            var pendingCCInterval = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingCCInterval));
            var resolveTimeout = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.ResolveTimeout));
            var unResolveInterval = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.UnResolveInterval));
            var unResolveCCInterval = Convert.ToInt32(await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.UnResolveCCInterval));
            var cc = await this.configurationProvider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.ExpertsAdmins);

            var unResolvedTickets = await this.ticketProvider.GetTicketsAsync(false);

            var tasks = unResolvedTickets.Select(i => this.ProcessTicketAsync(i, pendingTimeout, pendingInterval, resolveTimeout, unResolveInterval));
            var results = await Task.WhenAll(tasks);
            log.LogInformation($"new message count: {results.Where(i => i == true).ToList().Count()}");

            var ccTasks = unResolvedTickets.Select(i => this.ProcessTicketCCAsync(i, assignTimeout, unassignInterval, pendingTimeout, pendingCCInterval, resolveTimeout, unResolveCCInterval, cc));
            var ccResults = await Task.WhenAll(ccTasks);
            log.LogInformation($"new cc message count: {ccResults.Where(i => i == true).ToList().Count()}");
        }

        private async Task<bool> ProcessTicketAsync(TicketEntity ticket, int pendingTimeout, int pendingInterval, int resolveTimeout, int unResolveInterval)
        {
            if (this.ShouldSendNotification(ticket, pendingTimeout, pendingInterval, resolveTimeout, unResolveInterval))
            {
                if (ticket.Status != 0)
                {
                    string title = string.Empty;
                    string description = string.Empty;

                    switch (ticket.Status)
                    {
                        // assigned but not resolved
                        case 1:
                            title = TicketNotificationStrings.TitleUnResolved;
                            description = string.Format(CultureInfo.InvariantCulture, TicketNotificationStrings.DescriptionUnresolved, (DateTime.UtcNow - (DateTime)ticket.DateAssigned).TotalHours.ToString("#0.0"));
                            break;

                        // pending
                        case 2:
                            title = TicketNotificationStrings.TitlePending;
                            description = string.Format(CultureInfo.InvariantCulture, TicketNotificationStrings.DescriptionPending, (DateTime.UtcNow - (DateTime)ticket.DatePending).TotalHours.ToString("#0.0"));
                            break;
                    }

                    // pending or resolve timeout, send notification to assignee
                    var newSentNotificationId = this.tableRowKeyGenerator.CreateNewKeyOrderingMostRecentToOldest();
                    var sentNotificationEntity = new NotificationDataEntity
                    {
                        PartitionKey = NotificationDataTableNames.SentNotificationsPartition,
                        RowKey = newSentNotificationId,
                        Id = newSentNotificationId,
                        Type = (int)NotificationType.Warning,
                        Title = title,
                        Summary = description,
                        Buttons = new List<NotificationButton>()
                        {
                             new NotificationButton
                             {
                                 Title = "Go to Ticket",
                                 Link = this.BuildTicketLink(ticket.SmeThreadConversationId),
                             },
                        },
                        Author = ticket.RequesterName,
                        CreatedBy = ticket.RequesterName,
                        CreatedDate = ticket.DateCreated.ToLocalTime(),
                        Facts = new List<NotificationFact>()
                        {
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
                        Users = new List<string>()
                        {
                            ticket.AssignedToUserPrincipalName,
                        },
                    };
                    if (!string.IsNullOrEmpty(ticket.Description))
                    {
                        sentNotificationEntity.Facts.Append(new NotificationFact
                        {
                            Title = "Description:",
                            Value = ticket.Description,
                        });
                    }

                    await this.notificationDataRepository.CreateOrUpdateAsync(sentNotificationEntity);

                    var prepareToSendQueueMessageContent = new PrepareToSendQueueMessageContent
                    {
                        NotificationId = sentNotificationEntity.Id,
                    };

                    await this.prepareToSendQueue.SendAsync(prepareToSendQueueMessageContent);

                    ticket.DateSendNotification = DateTime.UtcNow;

                    await this.ticketProvider.UpsertTicketAsync(ticket);
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> ProcessTicketCCAsync(TicketEntity ticket, int assignTimeout, int unassignInterval, int pendingTimeout, int pendingCCIntervalint, int resolveTimeout, int unResolveCCInterval, string cc)
        {
            if (this.ShouldSendCCNotification(ticket, assignTimeout, unassignInterval, pendingTimeout, pendingCCIntervalint, resolveTimeout, unResolveCCInterval))
            {
                // send notification to cc
                List<string> notificationCC = cc.Split(';').ToList();
                if (notificationCC?.Count > 0)
                {
                    string title = string.Empty;
                    string description = string.Empty;
                    switch (ticket.Status)
                    {
                        // assigned but not resolved
                        case 0:
                            title = TicketNotificationStrings.TitleUnassigned;
                            description = TicketNotificationStrings.DescriptionUnassigned;
                            break;
                        case 1:
                            title = TicketNotificationStrings.TitleUnResolved;
                            description = string.Format(CultureInfo.InvariantCulture, TicketNotificationStrings.DescriptionUnresolvedCC, ticket.AssignedToName, (DateTime.UtcNow - (DateTime)ticket.DateAssigned).TotalHours.ToString("#0.0"));
                            break;

                        // pending
                        case 2:
                            title = TicketNotificationStrings.TitlePending;
                            description = string.Format(CultureInfo.InvariantCulture, TicketNotificationStrings.DescriptionPendingCC, ticket.AssignedToName, (DateTime.UtcNow - (DateTime)ticket.DatePending).TotalHours.ToString("#0.0"));
                            break;
                    }

                    var messageToSend = string.Format(CultureInfo.InvariantCulture, "RE:{0}", ticket.Title);
                    var encodedMessage = Uri.EscapeDataString(messageToSend);

                    var newSentNotificationIdCC = this.tableRowKeyGenerator.CreateNewKeyOrderingMostRecentToOldest();

                    List<NotificationButton> buttons = new List<NotificationButton>()
                        {
                             new NotificationButton
                             {
                                 Title = "Go to Ticket",
                                 Link = this.BuildTicketLink(ticket.SmeThreadConversationId),
                             },
                        };

                    if (ticket.Status != 0)
                    {
                        buttons.Add(new NotificationButton
                        {
                            Title = $"Chat with {ticket.AssignedToName}",
                            Link = $"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(ticket.RequesterUserPrincipalName)}&message={encodedMessage}",
                        });
                    }

                    var sentNotificationEntityCC = new NotificationDataEntity
                    {
                        PartitionKey = NotificationDataTableNames.SentNotificationsPartition,
                        RowKey = newSentNotificationIdCC,
                        Id = newSentNotificationIdCC,
                        Type = (int)NotificationType.Warning,
                        Title = title,
                        Summary = description,
                        Buttons = buttons,
                        Author = ticket.RequesterName,
                        CreatedBy = ticket.RequesterName,
                        CreatedDate = ticket.DateCreated.ToLocalTime(),
                        Facts = new List<NotificationFact>()
                        {
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
                        Users = notificationCC,
                    };
                    if (!string.IsNullOrEmpty(ticket.Description))
                    {
                        sentNotificationEntityCC.Facts.Append(new NotificationFact
                        {
                            Title = "Description:",
                            Value = ticket.Description,
                        });
                    }

                    await this.notificationDataRepository.CreateOrUpdateAsync(sentNotificationEntityCC);

                    var prepareToSendQueueMessageContentCC = new PrepareToSendQueueMessageContent
                    {
                        NotificationId = sentNotificationEntityCC.Id,
                    };

                    await this.prepareToSendQueue.SendAsync(prepareToSendQueueMessageContentCC);
                }

                ticket.DateSendCCNotification = DateTime.UtcNow;

                await this.ticketProvider.UpsertTicketAsync(ticket);
                return true;
            }

            return false;
        }

        private bool ShouldSendNotification(TicketEntity ticket, int pendingTimeout, int pendingInterval, int resolveTimeout, int unResolveInterval)
        {
            bool shouldNotify = false;

            // assigned
            if (ticket.Status == 1)
            {
                if (ticket.DateSendNotification == null)
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DateAssigned, resolveTimeout);
                }
                else
                {
                    if (ticket.DateSendNotification > ticket.DateAssigned)
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DateSendNotification, unResolveInterval);
                    }
                    else
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DateAssigned, resolveTimeout);
                    }
                }
            }

            // pending
            else if (ticket.Status == 2)
            {
                if (ticket.DateSendNotification == null)
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DatePending, pendingTimeout);
                }
                else
                {
                    if (ticket.DateSendNotification > ticket.DatePending)
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DateSendNotification, pendingInterval);
                    }
                    else
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DatePending, pendingTimeout);
                    }
                }
            }

            return shouldNotify;
        }

        private bool ShouldSendCCNotification(TicketEntity ticket, int assignTimeout, int unassignInterval, int pendingTimeout, int pendingCCIntervalint, int resolveTimeout, int unResolveCCInterval)
        {
            bool shouldNotify = false;

            if (ticket.Status == 0)
            {
                if (ticket.DateSendCCNotification == null)
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DateCreated, assignTimeout);
                }
                else
                {
                    if (ticket.DateSendCCNotification > ticket.DateCreated)
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DateSendCCNotification, unassignInterval);
                    }
                    else
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DateCreated, assignTimeout);
                    }
                }
            }

            // assigned
            if (ticket.Status == 1)
            {
                if (ticket.DateSendCCNotification == null)
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DateAssigned, resolveTimeout);
                }
                else
                {
                    if (ticket.DateSendCCNotification > ticket.DateAssigned)
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DateSendCCNotification, unResolveCCInterval);
                    }
                    else
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DateAssigned, resolveTimeout);
                    }
                }
            }

            // pending
            else if (ticket.Status == 2)
            {
                if (ticket.DateSendCCNotification == null)
                {
                    shouldNotify = this.IsTimeout((DateTime)ticket.DatePending, pendingTimeout);
                }
                else
                {
                    if (ticket.DateSendCCNotification > ticket.DatePending)
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DateSendCCNotification, pendingCCIntervalint);
                    }
                    else
                    {
                        shouldNotify = this.IsTimeout((DateTime)ticket.DatePending, pendingTimeout);
                    }
                }
            }

            return shouldNotify;
        }

        private bool IsTimeout(DateTime time, int timeoutMin)
        {
            DateTime now = DateTime.UtcNow;
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
