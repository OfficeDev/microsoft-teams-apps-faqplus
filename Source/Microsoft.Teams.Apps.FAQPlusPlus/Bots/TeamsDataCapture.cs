// <copyright file="TeamsDataCapture.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.TeamData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Repositories.UserData;
    using Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services;
    using Microsoft.Teams.Apps.FAQPlusPlus.Repositories.Extensions;

    /// <summary>
    /// Service to capture teams data.
    /// </summary>
    public class TeamsDataCapture
    {
        private const string PersonalType = "personal";
        private const string ChannelType = "channel";

        private readonly TeamDataRepository teamDataRepository;
        private readonly UserDataRepository userDataRepository;
        private readonly IAppSettingsService appSettingsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeamsDataCapture"/> class.
        /// </summary>
        /// <param name="teamDataRepository">Team data repository instance.</param>
        /// <param name="userDataRepository">User data repository instance.</param>
        /// <param name="appSettingsService">App Settings service.</param>
        public TeamsDataCapture(
            TeamDataRepository teamDataRepository,
            UserDataRepository userDataRepository,
            IAppSettingsService appSettingsService)
        {
            this.teamDataRepository = teamDataRepository ?? throw new ArgumentNullException(nameof(teamDataRepository));
            this.userDataRepository = userDataRepository ?? throw new ArgumentNullException(nameof(userDataRepository));
            this.appSettingsService = appSettingsService ?? throw new ArgumentNullException(nameof(appSettingsService));
        }

        /// <summary>
        /// Add channel or personal data in Table Storage.
        /// </summary>
        /// <param name="activity">Teams activity instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task OnBotAddedAsync(IConversationUpdateActivity activity)
        {
            // Take action if the event includes the bot being added.
            var membersAdded = activity.MembersAdded;
            if (membersAdded == null || !membersAdded.Any(p => p.Id == activity.Recipient.Id))
            {
                return;
            }

            switch (activity.Conversation.ConversationType)
            {
                case TeamsDataCapture.ChannelType:
                    await this.teamDataRepository.SaveTeamDataAsync(activity);
                    break;
                case TeamsDataCapture.PersonalType:
                    await this.userDataRepository.SaveUserDataAsync(activity);
                    break;
                default: break;
            }

            // Update service url app setting.
            await this.UpdateServiceUrl(activity.ServiceUrl);
        }

        /// <summary>
        /// Remove channel or personal data in table storage.
        /// </summary>
        /// <param name="activity">Teams activity instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task OnBotRemovedAsync(IConversationUpdateActivity activity)
        {
            var membersRemoved = activity.MembersRemoved;
            if (membersRemoved == null || !membersRemoved.Any())
            {
                return;
            }

            switch (activity.Conversation.ConversationType)
            {
                case TeamsDataCapture.ChannelType:
                    // Take action if the event includes the bot being removed.
                    if (membersRemoved.Any(p => p.Id == activity.Recipient.Id))
                    {
                        await this.teamDataRepository.RemoveTeamDataAsync(activity);
                    }

                    break;
                case TeamsDataCapture.PersonalType:
                    // The event triggered (when a user is removed from the tenant) doesn't
                    // include the bot in the member list being removed.
                    await this.userDataRepository.RemoveUserDataAsync(activity);
                    break;
                default: break;
            }
        }

        /// <summary>
        /// Update team information in the table storage.
        /// </summary>
        /// <param name="activity">Teams activity instance.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        public async Task OnTeamInformationUpdatedAsync(IConversationUpdateActivity activity)
        {
            await this.teamDataRepository.SaveTeamDataAsync(activity);
        }

        /// <summary>
        /// save user data in personal Chat.
        /// </summary>
        /// <param name="turnContex">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task OnPersonalTurnAsync(ITurnContext turnContex, CancellationToken cancellationToken)
        {
            await this.userDataRepository.SaveUserDataAsync(turnContex, cancellationToken);
        }

        private async Task UpdateServiceUrl(string serviceUrl)
        {
            // Check if service url is already synced.
            var cachedUrl = await this.appSettingsService.GetServiceUrlAsync();
            if (!string.IsNullOrWhiteSpace(cachedUrl))
            {
                return;
            }

            // Update service url.
            await this.appSettingsService.SetServiceUrlAsync(serviceUrl);
        }
    }
}
