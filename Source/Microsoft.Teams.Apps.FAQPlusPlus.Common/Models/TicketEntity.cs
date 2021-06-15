// <copyright file="TicketEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using Microsoft.Azure.Search;
    using Microsoft.WindowsAzure.Storage.Table;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents Ticket entity used for storage and retrieval.
    /// </summary>
    public class TicketEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets the unique ticket id.
        /// </summary>
        [Key]
        [JsonProperty("TicketId")]
        public string TicketId { get; set; }

        /// <summary>
        /// Gets or sets status of the ticket.
        /// </summary>
        [IsSortable]
        [IsFilterable]
        [JsonProperty("Status")]
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the ticket title.
        /// </summary>
        [IsSearchable]
        [JsonProperty("Title")]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the ticket description.
        /// </summary>
        [JsonProperty("Description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the created date of ticket.
        /// </summary>
        [IsSortable]
        [JsonProperty("DateCreated")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Gets or sets the created date of ticket.
        /// </summary>
        [IsSortable]
        [JsonProperty("Subject")]
        public string Subject { get; set; }

        /// <summary>
        /// Gets or sets the display name of the user that created the ticket.
        /// </summary>
        [IsSearchable]
        [JsonProperty("RequesterName")]
        public string RequesterName { get; set; }

        /// <summary>
        /// Gets or sets the user principal name (UPN) of the user that created the ticket.
        /// </summary>
        [JsonProperty("RequesterUserPrincipalName")]
        public string RequesterUserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets the given name of the user that created the ticket.
        /// </summary>
        [JsonProperty("RequesterGivenName")]
        public string RequesterGivenName { get; set; }

        /// <summary>
        /// Gets or sets the activity id of the root card for the user.
        /// </summary>
        [JsonProperty("RequesterCardActivityId")]
        public string RequesterCardActivityId { get; set; }

        /// <summary>
        /// Gets or sets the conversation id of the 1:1 chat with the user that created the ticket.
        /// </summary>
        [JsonProperty("RequesterConversationId")]
        public string RequesterConversationId { get; set; }

        /// <summary>
        /// Gets or sets the activity id of the root card in the SME channel.
        /// </summary>
        [JsonProperty("SmeCardActivityId")]
        public string SmeCardActivityId { get; set; }

        /// <summary>
        /// Gets or sets the conversation id of the thread pertaining to this ticket in the SME channel.
        /// </summary>
        [JsonProperty("SmeThreadConversationId")]
        public string SmeThreadConversationId { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time the ticket was last assigned.
        /// </summary>
        [IsSortable]
        [JsonProperty("DateAssigned")]
        public DateTime? DateAssigned { get; set; }

        /// <summary>
        /// Gets or sets the display name of the assigned SME currently working on the ticket.
        /// </summary>
        [IsSearchable]
        [IsFilterable]
        [JsonProperty("AssignedToName")]
        public string AssignedToName { get; set; }

        /// <summary>
        /// Gets or sets the UserPrincipalName of the assigned SME currently working on the ticket.
        /// </summary>
        [IsSearchable]
        [IsFilterable]
        [JsonProperty("AssignedToUserPrincipalName")]
        public string AssignedToUserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets the AAD object id of the assigned SME currently working on the ticket.
        /// </summary>
        [JsonProperty("AssignedToObjectId")]
        public string AssignedToObjectId { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time the ticket was closed.
        /// </summary>
        [IsSortable]
        [JsonProperty("DateClosed")]
        public DateTime? DateClosed { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time the ticket was pending.
        /// </summary>
        [IsSortable]
        [JsonProperty("DatePending")]
        public DateTime? DatePending { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time the ticket was updated in pending status.
        /// </summary>
        [IsSortable]
        [JsonProperty("DatePendingUpdate")]
        public DateTime? DatePendingUpdate { get; set; }


        /// <summary>
        /// Gets or sets the display name of the user that last modified the ticket.
        /// </summary>
        [JsonProperty("LastModifiedByName")]
        public string LastModifiedByName { get; set; }

        /// <summary>
        /// Gets or sets the AAD object id of the user that last modified the ticket.
        /// </summary>
        [JsonProperty("LastModifiedByObjectId")]
        public string LastModifiedByObjectId { get; set; }

        /// <summary>
        /// Gets or sets the question that the user typed.
        /// </summary>
        [JsonProperty("UserQuestion")]
        public string UserQuestion { get; set; }

        /// <summary>
        /// Gets or sets the comment for pending.
        /// </summary>
        [JsonProperty("PendingComment")]
        public string PendingComment { get; set; }

        /// <summary>
        /// Gets or sets the comment for resolve.
        /// </summary>
        [JsonProperty("ResolveComment")]
        public string ResolveComment { get; set; }

        /// <summary>
        /// Gets or sets the answer returned to the user from the knowledgebase.
        /// </summary>
        [JsonProperty("KnowledgeBaseAnswer")]
        public string KnowledgeBaseAnswer { get; set; }

        /// <summary>
        /// Gets or sets the feedback for this ticket.
        /// </summary>
        [JsonProperty("Feedback")]
        public string Feedback { get; set; }

        /// <summary>
        /// Gets or sets the feedback description for this ticket.
        /// </summary>
        [JsonProperty("FeedbackDescription")]
        public string FeedbackDescription { get; set; }

        /// <summary>
        /// Gets timestamp from storage table.
        /// </summary>
        [IsSortable]
        [JsonProperty("Timestamp")]
        public new DateTimeOffset Timestamp => base.Timestamp;

        /// <summary>
        /// Gets or sets the UTC date and time send notification according to SLA.
        /// </summary>
        [IsSortable]
        [JsonProperty("DateSendNotification")]
        public DateTime? DateSendNotification { get; set; }

        /// <summary>
        /// Gets or sets the UTC date and time send cc notification according to SLA.
        /// </summary>
        [IsSortable]
        [JsonProperty("DateSendCCNotification")]
        public DateTime? DateSendCCNotification { get; set; }


        /// <summary>
        /// Get the pending comment fo ticket.
        /// </summary>
        /// <param name="ticket">ticket entity.</param>
        /// <returns>pending comment.</returns>
        public static string GetPendingComment(TicketEntity ticket)
        {
            string[] pendingComments = ticket.PendingComment.Split(new string[] { "[*]" }, StringSplitOptions.None);
            string pendingComment = string.Empty;
            if (pendingComments.Length >= 1)
            {
                pendingComment = pendingComments[pendingComments.Length - 1];
            }

            return pendingComment;
        }

        /// <summary>
        /// Add pending comment for ticket.
        /// </summary>
        /// <param name="ticket">ticket entity.</param>
        /// <param name="comment">last comment.</param>
        public static void AddPendingComment(TicketEntity ticket, string comment)
        {
            ticket.PendingComment += "[*]" + comment;
            ticket.DatePendingUpdate = DateTime.UtcNow;
        }
    }
}