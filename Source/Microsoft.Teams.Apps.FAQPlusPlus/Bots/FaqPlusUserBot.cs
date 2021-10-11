// <copyright file="FaqPlusUserBot.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Bots
{
    using System;
    using System.Globalization;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.ApplicationInsights.DataContracts;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Teams;
    using Microsoft.Bot.Schema;
    using Microsoft.Bot.Schema.Teams;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models.Configuration;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Components;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Extensions;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Helpers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Properties;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.TeamsActivity;

    /// <summary>
    /// Class that handles the teams activity of Faq Plus User bot.
    /// This class handles all user activities - ask expert, submit feedback, ask question.
    /// This class also supports backward compatibility for legacy expert bot. It allows expert to change status of ticket, and update/delete question card.
    /// </summary>
    public class FaqPlusUserBot : TeamsActivityHandler
    {
         /// <summary>
        /// Represents the conversation type as personal.
        /// </summary>
        private const string ConversationTypePersonal = "personal";

        /// <summary>
        ///  Represents the conversation type as channel.
        /// </summary>
        private const string ConversationTypeChannel = "channel";

        /// <summary>
        /// Represents a set of key/value application configuration properties for FaqPlusPlus bot.
        /// </summary>
        private readonly BotSettings options;

        private readonly ILogger<FaqPlusUserBot> logger;
        private readonly string appBaseUri;
        private readonly IBotCommandResolver botCommandResolver;
        private readonly IConversationService conversationService;
        private readonly ITaskModuleActivity taskModuleActivity;
        private readonly TurnContextExtension turnContextExtension;

        /// <summary>
        /// Initializes a new instance of the <see cref="FaqPlusUserBot"/> class.
        /// </summary>
        /// <param name="optionsAccessor">A set of key/value application configuration properties for bot.</param>
        /// <param name="logger">Instance to send logs to the Application Insights service.</param>
        /// <param name="botCommandResolver">Resolves bot command.</param>
        /// <param name="conversationService">Conversation service to send welcome card.</param>
        /// <param name="taskModuleActivity">Instance to call teams activity when task module is invoked in teams chat.</param>
        /// <param name="turnContextExtension">Turn context extension object.</param>
        public FaqPlusUserBot(
            IOptionsMonitor<BotSettings> optionsAccessor,
            ILogger<FaqPlusUserBot> logger,
            IBotCommandResolver botCommandResolver,
            IConversationService conversationService,
            ITaskModuleActivity taskModuleActivity,
            TurnContextExtension turnContextExtension)
        {
            if (optionsAccessor == null)
            {
                throw new ArgumentNullException(nameof(optionsAccessor));
            }

            this.options = optionsAccessor.CurrentValue;
            this.appBaseUri = this.options.AppBaseUri;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.botCommandResolver = botCommandResolver ?? throw new ArgumentNullException(nameof(botCommandResolver));
            this.conversationService = conversationService ?? throw new ArgumentNullException(nameof(conversationService));
            this.taskModuleActivity = taskModuleActivity ?? throw new ArgumentNullException(nameof(taskModuleActivity));
            this.turnContextExtension = turnContextExtension ?? throw new ArgumentNullException(nameof(turnContextExtension));
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

                // Get the current culture info to use in resource files
                string locale = turnContext?.Activity.Entities?.FirstOrDefault(entity => entity.Type == "clientInfo")?.Properties["locale"]?.ToString();

                if (!string.IsNullOrEmpty(locale))
                {
                    CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(locale);
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

                switch (message.Conversation.ConversationType.ToLower())
                {
                    case ConversationTypePersonal:
                        await this.botCommandResolver.ResolveBotCommandInPersonalChatAsync(
                            message,
                            turnContext,
                            cancellationToken).ConfigureAwait(false);
                        break;

                    case ConversationTypeChannel:
                        await this.botCommandResolver.ResolveBotCommandInTeamChatAsync(
                            message,
                            turnContext,
                            cancellationToken).ConfigureAwait(false);
                        break;

                    default:
                        this.logger.LogWarning($"Received unexpected conversationType {message.Conversation.ConversationType}");
                        break;
                }
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

                if (activity.Conversation.ConversationType.ToLower() == ConversationTypePersonal)
                {
                   await this.conversationService.SendWelcomeCardInPersonalChatAsync(activity.MembersAdded, turnContext, cancellationToken).ConfigureAwait(false);
                   return;
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Error processing conversationUpdate: {ex.Message}", SeverityLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Invoke when user clicks on edit button on a question in SME team.
        /// This is to support backward compatibility for legacy bot.
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
        /// This is to support backward compatibility for legacy bot.
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
    }
}
