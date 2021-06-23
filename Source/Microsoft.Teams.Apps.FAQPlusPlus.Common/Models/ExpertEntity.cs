// <copyright file="ExpertEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Microsoft.WindowsAzure.Storage.Table;

    /// <summary>
    /// Represents  expert entity used for storage and retrieval.
    /// </summary>
    public class ExpertEntity : TableEntity
    {
        /// <summary>
        ///    Gets or sets channel id for the user or bot on this channel (Example: joe@smith.com, or @joesmith or 123456).
        /// </summary>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets display friendly name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets given name part of the user name.
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        ///  Gets or sets surname part of the user name.
        /// </summary>
        public string Surname { get; set; }

        /// <summary>
        ///  Gets or sets email Id of the user.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets unique user principal name.
        /// </summary>
        public string UserPrincipalName { get; set; }

        /// <summary>
        /// Gets or sets the TenantId.
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Gets or sets the UserRole.
        /// </summary>
        public string UserRole { get; set; }
    }
}
