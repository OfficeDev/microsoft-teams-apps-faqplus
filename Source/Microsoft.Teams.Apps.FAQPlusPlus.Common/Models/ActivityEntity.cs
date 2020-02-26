// <copyright file="ActivityEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Activity entity to store activity id and guid for mapping purpose.
    /// </summary>
    public class ActivityEntity : TableEntity
    {
        /// <summary>
        /// Gets or sets activity id.
        /// </summary>
        public string ActivityId { get; set; }
    }
}
