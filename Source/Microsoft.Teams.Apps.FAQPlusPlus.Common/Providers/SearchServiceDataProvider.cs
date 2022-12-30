// <copyright file="SearchServiceDataProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::Azure.AI.Language.QuestionAnswering;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Newtonsoft.Json;

    /// <summary>
    /// Azure search service blob storage data provider.
    /// </summary>
    public class SearchServiceDataProvider : ISearchServiceDataProvider
    {
        /// <summary>
        /// File name storing JSON structured QnA records.
        /// </summary>
        private const string FaqPlusQnAFile = "/faqplusqnadata.json";

        private readonly IQuestionAnswerServiceProvider questionAnswerServiceProvider;

        private readonly string storageConnectionString;

        /// <summary>
        /// Initializes a new instance of the <see cref="SearchServiceDataProvider"/> class.
        /// </summary>
        /// <param name="questionAnswerServiceProvider">question and answer ServiceProvider.</param>
        /// <param name="storageConnectionString">Azure web job storage.</param>
        public SearchServiceDataProvider(IQuestionAnswerServiceProvider questionAnswerServiceProvider, string storageConnectionString)
        {
            this.questionAnswerServiceProvider = questionAnswerServiceProvider;
            this.storageConnectionString = storageConnectionString;
        }

        /// <summary>
        /// This method downloads the knowledgebase and stores the json string to blob storage.
        /// </summary>
        /// <param name="knowledgeBaseId">knowledgebase id.</param>
        /// <returns>Task of downloaded data.</returns>
        public async Task SetupAzureSearchDataAsync(string knowledgeBaseId)
        {
            var downloadedQnaDocuments = await this.questionAnswerServiceProvider.DownloadKnowledgebaseAsync(knowledgeBaseId).ConfigureAwait(false);

            string azureJson = this.GenerateFormattedJson(downloadedQnaDocuments);
            await this.AddDataToBlobStorageAsync(azureJson).ConfigureAwait(false);
        }

        /// <summary>
        /// Function to convert input JSON to align with Schema Definition.
        /// </summary>
        /// <param name="qnaDocuments">Qna documents.</param>
        /// <returns>Create json format for search.</returns>
        private string GenerateFormattedJson(IEnumerable<KnowledgeBaseAnswerDTO> qnaDocuments)
        {
            IList<AzureSearchEntity> searchEntityList = new List<AzureSearchEntity>();
            foreach (var item in qnaDocuments)
            {
                var createdDate = item.Metadata.FirstOrDefault(prop => prop.Key == Constants.MetadataCreatedAt).Value;
                var updatedDate = item.Metadata.FirstOrDefault(prop => prop.Key == Constants.MetadataUpdatedAt).Value;

                searchEntityList.Add(
                        new AzureSearchEntity()
                        {
                            Id = item.QnaId.ToString(),
                            Source = item.Source,
                            Questions = item.Questions,
                            Answer = item.Answer,
                            CreatedDate = createdDate != null ? new DateTimeOffset(new DateTime(Convert.ToInt64(createdDate))) : new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero),
                            UpdatedDate = updatedDate != null ? new DateTimeOffset(new DateTime(Convert.ToInt64(updatedDate))) : new DateTimeOffset(DateTime.MinValue, TimeSpan.Zero),

                            // Create the serach service without MetaData.
                            // MetaData in AzureSerach if of collection(Complex type) whereas the Json from KB is of Dictionary. Its throws error while serialization.
                            // Metadata = item.Metadata
                        });
            }

            return JsonConvert.SerializeObject(searchEntityList);
        }

        /// <summary>
        /// This method is used to store json to blob storage.
        /// </summary>
        /// <param name="jsonData">knowledgebase jsonData string.</param>
        /// <returns>Task of storage of json in blob.</returns>
        private async Task AddDataToBlobStorageAsync(string jsonData)
        {
            // Retrieve storage account from connection string.
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(this.storageConnectionString);

            // Create the blob client.
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Retrieve a reference to a container.
            CloudBlobContainer container = blobClient.GetContainerReference(Constants.StorageContainer);

            // Create the container if it doesn't already exist.
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);

            // Retrieve reference to a blob.
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(Constants.BlobFolderName + FaqPlusQnAFile);
            blockBlob.Properties.ContentType = "application/json";

            // Upload JSON to blob storage.
            await blockBlob.UploadTextAsync(jsonData).ConfigureAwait(false);
        }
    }
}
