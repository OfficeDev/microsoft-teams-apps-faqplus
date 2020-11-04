namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of conversation provider.
    /// </summary>
    public interface IConversationProvider
    {
        /// <summary>
        /// Save of update conversation.
        /// </summary>
        /// <param name="conversation">Conversation when user asked question will replaced or inserted in table storage.</param>
        /// <returns>that resolves successfully if the data was saved successfully.</returns>
        Task UpsertConversationAsync(ConversationEntity conversation);


        /// <summary>
        /// get recently asked questions with answers.
        /// </summary>
        /// <param name="days">recent n days.</param>
        /// <returns>list of conversation entity.</returns>
        Task<List<ConversationEntity>> GetRecentAskedQnAListAsync(int days);
    }
}
