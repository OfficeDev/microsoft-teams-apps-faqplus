// <copyright file="UserDataRepositoryExtensions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Repositories.Extensions
{
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.UserData;
    using Microsoft.Teams.Apps.FAQPlusPlus.Helpers;

    /// <summary>
    /// Extensions for the repository of the user data stored in the table storage.
    /// </summary>
    public static class UserDataRepositoryExtensions
    {
        /// <summary>
        /// Add personal data in Table Storage.
        /// </summary>
        /// <param name="userDataRepository">The user data repository.</param>
        /// <param name="activity">Bot conversation update activity instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task SaveUserDataAsync(
            this UserDataRepository userDataRepository,
            IConversationUpdateActivity activity)
        {
            var userDataEntity = UserDataRepositoryExtensions.ParseUserData(activity);
            if (userDataEntity != null)
            {
                await userDataRepository.InsertOrMergeAsync(userDataEntity);
            }
        }

        /// <summary>
        /// Add personal data in Table Storage.
        /// </summary>
        /// <param name="userDataRepository">The user data repository.</param>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task SaveUserDataAsync(
            this UserDataRepository userDataRepository,
            ITurnContext turnContext,
            CancellationToken cancellationToken)
        {
            var userDataEntity = await UserDataRepositoryExtensions.ParseUserDataAsync(turnContext, cancellationToken);
            if (userDataEntity != null)
            {
                await userDataRepository.InsertOrMergeAsync(userDataEntity);
            }
        }

        /// <summary>
        /// Remove personal data in table storage.
        /// </summary>
        /// <param name="userDataRepository">The user data repository.</param>
        /// <param name="activity">Bot conversation update activity instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public static async Task RemoveUserDataAsync(
            this UserDataRepository userDataRepository,
            IConversationUpdateActivity activity)
        {
            var userDataEntity = UserDataRepositoryExtensions.ParseUserData(activity);
            if (userDataEntity != null)
            {
                var found = await userDataRepository.GetAsync(UserDataTableNames.UserDataPartition, userDataEntity.AadId);
                if (found != null)
                {
                    await userDataRepository.DeleteAsync(found);
                }
            }
        }

        /// <summary>
        /// parse user data from activity.
        /// </summary>
        /// <param name="activity">Bot conversation update activity instance.</param>
        /// <returns>UserDataEntity.</returns>
        private static UserDataEntity ParseUserData(IConversationUpdateActivity activity)
        {
            var rowKey = activity?.From?.AadObjectId;
            if (rowKey != null)
            {
                var userDataEntity = new UserDataEntity
                {
                    PartitionKey = UserDataTableNames.UserDataPartition,
                    RowKey = activity?.From?.AadObjectId,
                    AadId = activity?.From?.AadObjectId,
                    UserId = activity?.From?.Id,
                    ConversationId = activity?.Conversation?.Id,
                    ServiceUrl = activity?.ServiceUrl,
                    TenantId = activity?.Conversation?.TenantId,
                };

                return userDataEntity;
            }

            return null;
        }

        /// <summary>
        /// parse user data from turnContext.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>UserDataEntity.</returns>
        private static async Task<UserDataEntity> ParseUserDataAsync(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var userDataEntity = ParseUserData(turnContext.Activity);
            if (userDataEntity != null)
            {
                var userDetails = await AdaptiveCardHelper.GetUserDetailsInPersonalChatAsync(turnContext, cancellationToken).ConfigureAwait(false);
                userDataEntity.Name = userDetails.Name;
                userDataEntity.Upn = userDetails.UserPrincipalName;
                userDataEntity.Email = userDetails.Email;
            }

            return userDataEntity;
        }
    }
}
