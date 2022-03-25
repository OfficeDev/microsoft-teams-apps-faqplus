// <copyright file="NotificationFacts.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Notification facts
    /// </summary>
    public class NotificationFact
    {
        /// <summary>
        /// Title of Fact
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Value of Fact 
        /// </summary>
        public string Value { get; set; }
    }
}
