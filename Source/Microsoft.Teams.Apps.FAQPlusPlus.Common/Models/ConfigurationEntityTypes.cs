// <copyright file="ConfigurationEntityTypes.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// Configuration entity type names.
    /// </summary>
    public static class ConfigurationEntityTypes
    {
        /// <summary>
        /// Team entity.
        /// </summary>
        public const string TeamId = "TeamId";

        /// <summary>
        /// Knowledge base entity.
        /// </summary>
        public const string KnowledgeBaseId = "KnowledgeBaseId";

        /// <summary>
        /// Welcome message entity.
        /// </summary>
        public const string WelcomeMessageText = "WelcomeMessageText";

        /// <summary>
        /// Help tab text entity.
        /// </summary>
        public const string HelpTabText = "HelpTabText";

        /// <summary>
        /// AssignTimeout text entity.
        /// </summary>
        public const string AssignTimeout = "AssignTimeoutText";

        /// <summary>
        /// PendingTimeout text entity.
        /// </summary>
        public const string PendingTimeout = "PendingTimeoutText";

        /// <summary>
        /// ResolveTimeout text entity.
        /// </summary>
        public const string ResolveTimeout = "ResolveTimeoutText";

        /// <summary>
        /// QnaMaker endpoint key entity.
        /// </summary>
        public const string QnAMakerEndpointKey = "QnaMakerEndpointKey";

        /// <summary>
        /// Subjects entity.
        /// </summary>
        public const string Subjects = "Subjects";
    }
}
