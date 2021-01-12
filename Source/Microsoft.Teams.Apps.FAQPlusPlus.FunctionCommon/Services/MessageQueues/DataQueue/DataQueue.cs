// <copyright file="DataQueue.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MessageQueues.DataQueue
{
    using Microsoft.Extensions.Options;

    /// <summary>
    /// The message queue service connected to the "company-communicator-data" queue in Azure service bus.
    /// </summary>
    public class DataQueue : BaseQueue<DataQueueMessageContent>
    {
        /// <summary>
        /// Queue name of the data queue.
        /// </summary>
        public const string QueueName = "faq-plus-data";

        /// <summary>
        /// Initializes a new instance of the <see cref="DataQueue"/> class.
        /// </summary>
        /// <param name="messageQueueOptions">The message queue options.</param>
        public DataQueue(IOptions<MessageQueueOptions> messageQueueOptions)
            : base(
                  serviceBusConnectionString: messageQueueOptions.Value.ServiceBusConnection,
                  queueName: DataQueue.QueueName)
        {
        }
    }
}
