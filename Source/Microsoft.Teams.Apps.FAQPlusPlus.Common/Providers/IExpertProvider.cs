// <copyright file="ITicketsProvider.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of Tickets provider.
    /// </summary>
    public interface IExpertProvider
    {
        /// <summary>
        /// Store or update ticket entity in table storage.
        /// </summary>
        /// <param name="expert">Represents expert entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        Task UpserExpertAsync(ExpertEntity expert);

        /// <summary>
        /// Get already saved entity detail from storage table.
        /// </summary>
        /// <param name="expertId">expert channelaccount id.</param>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        Task<ExpertEntity> GetExpertAsync(string expertId);

        /// <summary>
        /// Get all already saved entity detail from storage table.
        /// </summary>
        /// <returns><see cref="Task"/> Already saved entity detail.</returns>
        Task<List<ExpertEntity>> GetExpertsAsync();

        /// <summary>
        /// Store or update ticket entity in table storage.
        /// </summary>
        /// <param name="experts">Represents expert entity used for storage and retrieval.</param>
        /// <returns><see cref="Task"/> that represents configuration entity is saved or updated.</returns>
        Task UpserExpertsAsync(List<ExpertEntity> experts);
    }
}
