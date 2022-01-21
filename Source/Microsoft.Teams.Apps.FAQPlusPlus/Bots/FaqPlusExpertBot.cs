// <copyright file="FaqPlusExpertBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Components;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Extensions;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.TeamsActivity;

    /// <summary>
    /// Class that handles the teams activity of Faq Plus bot and messaging extension.
    /// This class handles all expert activities like - ticket update, messaging extension - query, fetch, submit, task module - add/edit answer.
    /// </summary>
    public class FaqPlusExpertBot : TeamsActivityHandler
    {
        /// <summary>
        /// Represents a set of key/value application configuration properties for FaqPlus expert bot.
        /// </summary>
        private readonly BotSettings options;

        private readonly string appBaseUri;
        private readonly ILogger<FaqPlusExpertBot> logger;

        private readonly IBotCommandResolver botCommandResolver;
        private readonly IConversationService conversationService;
        private readonly ITaskModuleActivity taskModuleActivity;
        private readonly IMessagingExtensionActivity messagingExtensionActivity;
        private readonly TurnContextExtension turnContextExtension;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaqPlusExpertBot"/> class.
        /// </summary>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for FaqPlusPlus bot.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="botCommandResolver">Resolves bot command.</param>
        /// <param name="conversationService">Conversation service to send welcome card.</param>
        /// <param name="taskModuleActivity">Instance to call teams activity when task module is invoked in teams chat.</param>
        /// <param name="messagingExtensionActivity">Instance to call teams activity when messaging extension is invoked.</param>
        /// <param name="turnContextExtension">Turn context extension object.</param>
        public FaqPlusExpertBot(
            IOptionsMonitor<BotSettings> optionsAccessor,
            ILogger<FaqPlusExpertBot> logger,
            IBotCommandResolver botCommandResolver,
            IConversationService conversationService,
            ITaskModuleActivity taskModuleActivity,
            IMessagingExtensionActivity messagingExtensionActivity,
            TurnContextExtension turnContextExtension)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            this.botCommandResolver = botCommandResolver ?? throw new ArgumentNullException(nameof(botCommandResolver));
            this.conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            this.taskModuleActivity = taskModuleActivity ?? throw new ArgumentNullException(nameof(taskModuleActivity));
            this.messagingExtensionActivity = messagingExtensionActivity ?? throw new ArgumentNullException(nameof(messagingExtensionActivity));
            this.turnContextExtension = turnContextExtension ?? throw new ArgumentNullException(nameof(turnContextExtension));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            this.options = optionsAccessor.CurrentValue;
            this.appBaseUri = this.options.AppBaseUri;
        }

        /// <summary>
        /// Handles an incoming activity.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.activityhandler.onturnasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        public override Task OnTurnAsync(
            ITurnContext turnContext,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (turnContext != null & !this.turnContextExtension.IsActivityFromExpectedTenant(turnContext))
                {
                    this.logger.LogWarning($"Unexpected tenant id {turnContext?.Activity.Conversation.TenantId}");
                    return Task.CompletedTask;
                }

                return base.OnTurnAsync(turnContext, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error at OnTurnAsync()");
                return base.OnTurnAsync(turnContext, cancellationToken);
            }
        }

        /// <summary>
        /// Invoked when a message activity is received from the user.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.activityhandler.onmessageactivityasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task OnMessageActivityAsync(
            ITurnContext<IMessageActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var message = turnContext?.Activity;
                this.logger.LogInformation($"from: {message.From?.Id}, conversation: {message.Conversation.Id}, replyToId: {message.ReplyToId}");
                try
                {
                    await this.turnContextExtension.SendTypingIndicatorAsync(turnContext).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Do not fail on errors sending the typing indicator
                    this.logger.LogWarning(ex, "Failed to send a typing indicator");
                }

                await this.botCommandResolver.ResolveBotCommandInTeamChatAsync(message, turnContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await turnContext.SendActivityAsync(Strings.ErrorMessage).ConfigureAwait(false);
                this.logger.LogError(ex, $"Error processing message: {ex.Message}", SeverityLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Invoke when a conversation update activity is received from the channel.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onconversationupdateactivityasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task OnConversationUpdateActivityAsync(
            ITurnContext<IConversationUpdateActivity> turnContext,
            CancellationToken cancellationToken)
        {
            try
            {
                var activity = turnContext?.Activity;
                this.logger.LogInformation("Received conversationUpdate activity");
                this.logger.LogInformation($"conversationType: {activity.Conversation.ConversationType}, membersAdded: {activity.MembersAdded?.Count}, membersRemoved: {activity.MembersRemoved?.Count}");

                if (activity?.MembersAdded?.Count == 0)
                {
                    this.logger.LogInformation("Ignoring conversationUpdate that was not a membersAdded event");
                    return;
                }

                await this.conversationService.SendWelcomeCardInTeamChatAsync(activity.MembersAdded, turnContext, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error processing conversationUpdate: {ex.Message}", SeverityLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Invoke when user clicks on edit button on a question in SME team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamstaskmodulefetchasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override Task<TaskModuleResponse> OnTeamsTaskModuleFetchAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest,
            CancellationToken cancellationToken)
        {
            try
            {
                return this.taskModuleActivity.OnFetchAsync(taskModuleRequest, this.appBaseUri);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error editing question : {ex.Message}", SeverityLevel.Error);
                return default;
            }
        }

        /// <summary>
        /// Invoked when the user submits a edited question from SME team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="taskModuleRequest">Task module invoke request value payload.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>A task that represents the work queued to execute.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamstaskmodulesubmitasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task<TaskModuleResponse> OnTeamsTaskModuleSubmitAsync(
            ITurnContext<IInvokeActivity> turnContext,
            TaskModuleRequest taskModuleRequest,
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.taskModuleActivity.OnSubmitAsync(turnContext, this.appBaseUri);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error submitting the question : {ex.Message}", SeverityLevel.Error);
                return default;
            }
        }

        /// <summary>
        /// Invoked when the user opens the messaging extension or searching any content in it.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="query">Contains messaging extension query keywords.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Messaging extension response object to fill compose extension section.</returns>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamsmessagingextensionqueryasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task<MessagingExtensionResponse> OnTeamsMessagingExtensionQueryAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.messagingExtensionActivity.QueryAsync(turnContext);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to handle the messaging extension command {turnContext?.Activity?.Name}: {ex.Message}", SeverityLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Invoked when user clicks on "Add new question" button on messaging extension from SME team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Action to be performed.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Response of messaging extension action.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamsmessagingextensionfetchtaskasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionFetchTaskAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            try
            {
                return this.messagingExtensionActivity.FetchTaskAsync(turnContext, action, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error while fetching task received by the bot.", SeverityLevel.Error);
                return default;
            }
        }

        /// <summary>
        /// Invoked when the user submits a new question from SME team.
        /// </summary>
        /// <param name="turnContext">Context object containing information cached for a single turn of conversation with a user.</param>
        /// <param name="action">Action to be performed.</param>
        /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
        /// <returns>Response of messaging extension action.</returns>
        /// <remarks>
        /// Reference link: https://docs.microsoft.com/en-us/dotnet/api/microsoft.bot.builder.teams.teamsactivityhandler.onteamsmessagingextensionsubmitactionasync?view=botbuilder-dotnet-stable.
        /// </remarks>
        protected override async Task<MessagingExtensionActionResponse> OnTeamsMessagingExtensionSubmitActionAsync(
            ITurnContext<IInvokeActivity> turnContext,
            MessagingExtensionAction action,
            CancellationToken cancellationToken)
        {
            try
            {
                return await this.messagingExtensionActivity.SubmitActionAsync(turnContext, action, cancellationToken);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Error at OnTeamsMessagingExtensionSubmitActionAsync()", SeverityLevel.Error);
                return default;
            }
        }
    }
}
