namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers
{
    using System.Threading.Tasks;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;

    /// <summary>
    /// Interface of user action provider.
    /// </summary>
    public interface IUserActionProvider
    {
        /// <summary>
        /// Save of update userAction.
        /// </summary>
        /// <param name="userAction">userAction will replaced or inserted in table storage.</param>
        /// <returns>that resolves successfully if the data was saved successfully.</returns>
        Task UpsertUserActionAsync(UserActionEntity userAction);
    }
}
