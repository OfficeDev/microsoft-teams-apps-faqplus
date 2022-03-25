// <copyright file="ISOSProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of service now provider.
    /// </summary>
    public interface ISOSClient
    {
        /// <summary>
        /// Gets or sets the base URL of sos service.
        /// </summary>
        string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the basic Auth sos service.
        /// </summary>
        SOSBasicAuth BasicAuth { get; set; }

        /// <summary>
        /// Create SOS request.
        /// </summary>
        /// <param name="request">the json format of request.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<SOSRequestResult> CreateRequestAsync(SOSRequest request);

        /// <summary>
        /// Query SOS ticket.
        /// </summary>
        /// <param name="requestId">ticket number.</param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        Task<SOSQueryResult> QueryRequestAsync(string requestId);
    }
}
