namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Represents Feedback entity used for storage and retrieval.
    /// </summary>
    public class FeedbackEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the unique feedback id.
        /// </summary>
        public string FeedbackId { get; set; }

        /// <summary>
        /// Gets or sets user email.
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets user given name.
        /// </summary>
        public string UserGivenName { get; set; }

        /// <summary>
        /// Gets or sets rating given for feedback.
        /// </summary>
        public string Rating { get; set; }

        /// <summary>
        /// Gets or sets description for feedback.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the question that was asked originally asked by the user.
        /// </summary>
        public string UserQuestion { get; set; }

        /// <summary>
        /// Gets or sets the response given by the bot to the user.
        /// </summary>
        public string KnowledgeBaseAnswer { get; set; }

        /// <summary>
        /// Gets or sets the subject related to that answer.
        /// </summary>
        public string Subject { get; set; }
    }
}
