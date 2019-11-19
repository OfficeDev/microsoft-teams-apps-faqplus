// <copyright file="IQnAMakerFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Services
{
    using Microsoft.Bot.Builder.AI.QnA;

    /// <summary>
    /// Produces the right <see cref="QnAMaker"/> instance for a knowledge base.
    /// </summary>
    public interface IQnAMakerFactory
    {
        /// <summary>
        /// Gets the <see cref="QnAMaker"/> instance to use when querying the given knowledge base ID.
        /// </summary>
        /// <param name="knowledgeBaseId">The knowledge base ID</param>
        /// <param name="endpointKey">The endpoint key</param>
        /// <returns>A QnAMaker instance</returns>
        QnAMaker GetQnAMaker(string knowledgeBaseId, string endpointKey);
    }
}
