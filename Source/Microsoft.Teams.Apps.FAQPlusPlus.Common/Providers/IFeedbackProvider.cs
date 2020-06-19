namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of feedback provider.
    /// </summary>
    public interface IFeedbackProvider
    {
        /// <summary>
        /// Save of update feedback.
        /// </summary>
        /// <param name="feedback">Feedback received from bot based on which appropriate row will replaced or inserted in table storage.</param>
        /// <returns>that resolves successfully if the data was saved successfully.</returns>
        Task UpsertFeecbackAsync(FeedbackEntity feedback);
    }
}
