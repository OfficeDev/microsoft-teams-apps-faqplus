// <copyright file="ConversationProperty.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// To record the conversation last active time and subject user selected.
    /// </summary>
    public class ConversationProperty
    {
        /// <summary>
        /// Gets or sets the times continous times no answer to user question.
        /// </summary>
        public int ContinousFailureTimes { get; set; }

        /// <summary>
        /// Gets or sets the last time send recommend.
        /// </summary>
        public DateTime LastRecommendTime { get; set; }
    }
}
