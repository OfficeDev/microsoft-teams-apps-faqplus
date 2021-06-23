// <copyright file="SOSProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Newtonsoft.Json;
    using RestSharp;

    /// <summary>
    /// SOSProvider will help to create and query SOS ticket.
    /// </summary>
    public class SOSClient : ISOSClient
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SOSClient"/> class.
        /// </summary>
        /// <param name="baseUrl">base URL of sos service.</param>
        /// <param name="basicAuth">basic Auth sos service.</param>
        public SOSClient(string baseUrl, SOSBasicAuth basicAuth)
        {
            this.BaseUrl = baseUrl;
            this.BasicAuth = basicAuth;
        }

        /// <summary>
        /// Gets or sets the basic URL of sos service.
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// Gets or sets the basic Auth sos service.
        /// </summary>
        public SOSBasicAuth BasicAuth { get; set; }

        /// <summary>
        /// Create SOS request.
        /// </summary>
        /// <param name="request">the json format of request.</param>
        /// <returns> representing the result of the asynchronous operation.</returns>
        public async Task<SOSRequestResult> CreateRequestAsync(SOSRequest request)
        {
            RestClient client = new RestClient(this.BaseUrl);
            string requestString = $"{this.BaseUrl}/api/ubis2/request";

            RestRequest httpRequest = new RestRequest(requestString, Method.POST);

            httpRequest.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{this.BasicAuth.Username}:{this.BasicAuth.Password}")));
            httpRequest.AddHeader("Accept", "application/json");
            httpRequest.AddHeader("Content-Type", "application/json");
            httpRequest.AddJsonBody(JsonConvert.SerializeObject(request));

            var res = await client.ExecuteAsync(httpRequest);
            if (res.StatusCode == System.Net.HttpStatusCode.Created)
            {
                return JsonConvert.DeserializeObject<SOSRequestResult>(res.Content.ToString());
            }

            return null;
        }

        /// <summary>
        /// Query SOS ticket.
        /// </summary>
        /// <param name="requestId">ticket number.</param>
        /// <returns>representing the result of the asynchronous operation.</returns>
        public async Task<SOSQueryResult> QueryRequestAsync(string requestId)
        {
            RestClient client = new RestClient(this.BaseUrl);
            string requestString = $"{this.BaseUrl}/api/ubis2/request?query=task_effective_number={requestId}";
            RestRequest httpRequest = new RestRequest(requestString, Method.GET);
            httpRequest.AddHeader("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{this.BasicAuth.Username}:{this.BasicAuth.Password}")));
            httpRequest.AddHeader("Accept", "application/json");
            httpRequest.AddHeader("Content-Type", "application/json");

            var res = await client.ExecuteAsync(httpRequest);
            if (res.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject<SOSQueryResult>(res.Content.ToString());
            }

            return null;
        }
    }
}
