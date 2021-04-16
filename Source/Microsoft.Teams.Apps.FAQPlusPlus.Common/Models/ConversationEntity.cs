// <copyright file="ConversationEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// To record the conversation last active time and subject user selected.
    /// </summary>
    public class ConversationEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the unique QnA id.
        /// </summary>
        public string ConversationID { get; set; }

        /// <summary>
        /// Gets or sets the unique QnA id.
        /// </summary>
        public string QnAID { get; set; }

        /// <summary>
        /// Gets or sets the SessionId.
        /// </summary>
        public string SessionId { get; set; }

        /// <summary>
        /// Gets or sets user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets user email.
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets user object id.
        /// </summary>
        public string UserObjectId { get; set; }

        /// <summary>
        ///  Gets or sets the question user asked.
        /// </summary>
        public string Question { get; set; }

        /// <summary>
        ///  Gets or sets the options user selected in multi-turn.
        /// </summary>
        public string Answer { get; set; }

        /// <summary>
        ///  Gets or sets the score of final answer.
        /// </summary>
        public string Score { get; set; }

        /// <summary>
        ///  Gets or sets project metadata related to this QnA pari.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        ///  Gets or sets the previous QnAID for multi-turn.
        /// </summary>
        public string PreviousQnAID { get; set; }

        /// <summary>
        ///  Gets or sets the prompts.
        /// </summary>
        public string Prompts { get; set; }
    }
}
