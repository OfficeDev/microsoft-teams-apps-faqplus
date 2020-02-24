// <copyright file="Constants.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common
{
    /// <summary>
    /// constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Source.
        /// </summary>
        public const string Source = "Editorial";

        /// <summary>
        /// Delete command.
        /// </summary>
        public const string DeleteCommand = "delete";

        /// <summary>
        /// No command.
        /// </summary>
        public const string NoCommand = "no";

        /// <summary>
        /// Image url valid pattern validation expression.
        /// </summary>
        public const string ValidRedirectUrlPattern = @"^(http|https|)\:\/\/[0-9a-zA-Z]([-.\w]*[0-9a-zA-Z])*(:(0-9)*)*(\/?)([a-zA-Z0-9\-\.\?\,\'\/\\\+&%\$#_]*)?([a-zA-Z0-9\-\?\,\'\/\+&%\$#_]+)";

        /// <summary>
        /// Image redirect url invalid pattern validation expression.
        /// </summary>
        public const string InvalidRedirectUrlDomainPattern = @"(.jpeg|.JPEG|.jpg|.JPG|.gif|.GIF|.png|.PNG|.docs|.xls|.exe|.pptx|.dll)$";

        /// <summary>
        /// Table name which stores activity id of responded card.
        /// </summary>
        public const string ActivityTableName = "ActivityEntity";

        /// <summary>
        /// Qna metadata createdat name.
        /// </summary>
        public const string MetadataCreatedAt = "createdat";

        /// <summary>
        /// Qna metadata createdby name.
        /// </summary>
        public const string MetadataCreatedBy = "createdby";

        /// <summary>
        /// Qna metadata conversationid name.
        /// </summary>
        public const string MetadataConversationId = "conversationid";

        /// <summary>
        /// Qna metadata updatedat name.
        /// </summary>
        public const string MetadataUpdatedAt = "updatedat";

        /// <summary>
        /// Qna metadata updatedby name.
        /// </summary>
        public const string MetadataUpdatedBy = "updatedby";

        /// <summary>
        /// Qna metadata activity reference id name.
        /// </summary>
        public const string MetadataActivityReferenceId = "activityreferenceid";

        // Commands supported by the bot

        /// <summary>
        /// TeamTour - text that triggers team tour action.
        /// </summary>
        public const string TeamTour = "team tour";

        /// <summary>
        /// TakeAtour - text that triggers take a tour action for the user.
        /// </summary>
        public const string TakeATour = "take a tour";

        /// <summary>
        /// AskAnExpert - text that renders the ask an expert card.
        /// </summary>
        public const string AskAnExpert = "ask an expert";

        /// <summary>
        /// Feedback - text that renders share feedback card.
        /// </summary>
        public const string ShareFeedback = "share feedback";

        /// <summary>
        /// Table name where SME activity details from bot will be saved.
        /// </summary>
        public const string TicketTableName = "Tickets";

        /// <summary>
        /// Partition key for fetching knowledgebase id from table storage.
        /// </summary>
        public const string ConfigurationInfoPartitionKey = "ConfigurationInfo";

        /// <summary>
        /// Partition key for fetching knowledgebase id from table storage.
        /// </summary>
        public const string KnowledgebaseRowKey = "KnowledgeBaseId";

        /// <summary>
        /// Blob Container Name.
        /// </summary>
        public const string StorageContainer = "faqplus-search-container";

        /// <summary>
        /// Folder inside blob container.
        /// </summary>
        public const string BlobFolderName = "faqplus-metadata";

        /// <summary>
        /// Key to get the value from settings.
        /// </summary>
        public const string AzureWebJobsStorage = "AzureWebJobsStorage";
    }
}
