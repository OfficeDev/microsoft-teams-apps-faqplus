﻿// <copyright file="ShareFeedbackCardPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// This model class is responsible to model user activity with bot-
    /// asking a question or providing feedback on app or on results given by the bot to the user.
    /// </summary>
    public class ShareFeedbackCardPayload : TeamsAdaptiveSubmitActionData
    {
        /// <summary>
        /// Gets or sets the action when user submits feedback rating.
        /// </summary>
        public string Rating { get; set; }

        /// <summary>
        /// Gets or sets the bot feedback.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the question for the expert being asked by the user through Response card-
        /// Response Card: Response generated by the bot to user question by calling QnA Maker service.
        /// </summary>
        public string UserQuestion { get; set; }

        /// <summary>
        /// Gets or sets the value of projectMetadata.
        /// </summary>
        public string Project { get; set; }

        /// <summary>
        /// Gets or sets the answer for the expert- Answer sent to the SME team along with feedback
        /// provided by the user on response given by bot calling QnA Maker service.
        /// </summary>
        public string KnowledgeBaseAnswer { get; set; }
    }
}