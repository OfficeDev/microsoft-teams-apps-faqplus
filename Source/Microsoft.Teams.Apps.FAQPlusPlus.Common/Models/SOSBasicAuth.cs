// <copyright file="SOSBasicAuth.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    /// <summary>
    /// SOSBasicAuth.
    /// </summary>
    public class SOSBasicAuth
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SOSBasicAuth"/> class.
        /// Init SOSBasicAuth.
        /// </summary>
        /// <param name="username">username.</param>
        /// <param name="password">passowrd.</param>
        public SOSBasicAuth(string username, string password)
        {
            this.Username = username;
            this.Password = password;
        }

        /// <summary>
        ///  Gets or Sets username.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or Sets password.
        /// </summary>
        public string Password { get; set; }
    }
}
