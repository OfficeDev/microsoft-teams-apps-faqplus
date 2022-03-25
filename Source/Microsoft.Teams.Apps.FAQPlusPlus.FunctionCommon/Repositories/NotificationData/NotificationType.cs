// <copyright file="NotificationType.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.NotificationData
{
    /// <summary>
    /// Represents the type of notification.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Represents an info notification.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Represents an warning notification.
        /// </summary>
        Warning = 1,

        /// <summary>
        /// Represents a error notification.
        /// </summary>
        Error = 2,
    }
}
