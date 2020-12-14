// <copyright file="UserActionEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Represents the type of user action.
    /// </summary>
    public enum UserActionType
    {
        /// <summary>
        /// Request to ask expert.
        /// </summary>
        AskExpertReq,

        /// <summary>
        /// Request to share feedback.
        /// </summary>
        ShareFeedbackReq,

        /// <summary>
        /// Send ask expert card.
        /// </summary>
        AskExpert,

        /// <summary>
        /// Send share feedback card.
        /// </summary>
        ShareFeedback,

        /// <summary>
        /// share feedback when ticket resolved.
        /// </summary>
        ShareTicketFeedback,

        /// <summary>
        /// Take a tour in personal chat.
        /// </summary>
        TakeATour,

        /// <summary>
        /// Take a tour in teams channel.
        /// </summary>
        TeamTour,

        /// <summary>
        /// Change the status of ticket.
        /// </summary>
        ChangeStatus,

        /// <summary>
        /// Delete an QnA pair in KB.
        /// </summary>
        DeleteQnAPari,

        /// <summary>
        /// Not defined.
        /// </summary>
        NotDefined,
    }

    /// <summary>
    /// Represents UserAction entity used for storage and retrieval.
    /// </summary>
    public class UserActionEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the unique user action id.
        /// </summary>
        public string UserActionId { get; set; }

        /// <summary>
        /// Gets or sets user email.
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets user name.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Gets or sets user action.
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets remark of this action.
        /// </summary>
        public string Remark { get; set; }
    }
}
