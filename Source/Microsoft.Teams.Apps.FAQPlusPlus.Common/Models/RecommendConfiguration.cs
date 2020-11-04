// <copyright file="RecommendConfiguration.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Represents the RecommendConfiguration.
    /// </summary>
    public class RecommendConfiguration
    {
        /// <summary>
        /// Gets or sets recommend interval only once during this interval.
        /// </summary>
        public int RecommendationIntervalInMinutes { get; set; }

        /// <summary>
        /// Gets or sets failure times when to send the recommend.
        /// </summary>
        public int RecommendationContinousFailureTimes { get; set; }
    }
}
