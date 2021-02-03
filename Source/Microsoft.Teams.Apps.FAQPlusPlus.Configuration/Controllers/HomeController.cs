﻿// <copyright file="HomeController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Controllers
{
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;
    using Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Home Controller.
    /// </summary>
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IConfigurationDataProvider configurationPovider;
        private readonly IQnAMakerClient qnaMakerClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="configurationPovider">configurationPovider dependency injection.</param>
        /// <param name="qnaMakerClient">qnaMakerClient dependency injection.</param>
        public HomeController(IConfigurationDataProvider configurationPovider, IQnAMakerClient qnaMakerClient)
        {
            this.configurationPovider = configurationPovider;
            this.qnaMakerClient = qnaMakerClient;
        }

        /// <summary>
        /// The landing page.
        /// </summary>
        /// <returns>Default landing page view.</returns>
        [HttpGet]
        public ActionResult Index()
        {
            return this.View();
        }

        /// <summary>
        /// Parse team id from first and then proceed to save it on success.
        /// </summary>
        /// <param name="teamId">Team id is the unique string.</param>
        /// <returns>View.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ParseAndSaveTeamIdAsync(string teamId = "")
        {
            string teamIdAfterParse = ParseTeamIdFromDeepLink(teamId ?? string.Empty);
            if (string.IsNullOrWhiteSpace(teamIdAfterParse))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "The provided team id is not valid.");
            }
            else
            {
                return await this.UpsertTeamIdAsync(teamIdAfterParse).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Save or update teamId in table storage which is received from View.
        /// </summary>
        /// <param name="teamId">Team id is the unique deep link URL string associated with each team.</param>
        /// <returns>View.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpsertTeamIdAsync(string teamId)
        {
            bool isSaved = await this.configurationPovider.UpsertEntityAsync(teamId, ConfigurationEntityTypes.TeamId).ConfigureAwait(false);
            if (isSaved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the team id due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved team id from table storage.
        /// </summary>
        /// <returns>Team id.</returns>
        [HttpGet]
        public async Task<string> GetSavedTeamIdAsync()
        {
            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.TeamId).ConfigureAwait(false);
        }

        /// <summary>
        /// Save or update knowledgeBaseId in table storage which is received from View.
        /// </summary>
        /// <param name="knowledgeBaseId">knowledgeBaseId is the unique string to identify knowledgebase.</param>
        /// <returns>View.</returns>
        [HttpGet]
        public async Task<ActionResult> UpsertKnowledgeBaseIdAsync(string knowledgeBaseId)
        {
            bool isSaved = await this.configurationPovider.UpsertEntityAsync(knowledgeBaseId, ConfigurationEntityTypes.KnowledgeBaseId).ConfigureAwait(false);
            if (isSaved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the knowledge base id due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Validate knowledgebase id from QnA Maker service first and then proceed to save it on success.
        /// The QnA Maker endpoint key is also refreshed as part of this process.
        /// </summary>
        /// <param name="knowledgeBaseId">knowledgeBaseId is the unique string to identify knowledgebase.</param>
        /// <returns>View.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ValidateAndSaveKnowledgeBaseIdAsync(string knowledgeBaseId)
        {
            bool isValidKnowledgeBaseId = await this.IsKnowledgeBaseIdValid(knowledgeBaseId).ConfigureAwait(false);
            if (isValidKnowledgeBaseId)
            {
                var endpointRefreshStatus = await this.RefreshQnAMakerEndpointKeyAsync().ConfigureAwait(false);
                if (!endpointRefreshStatus)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the QnAMaker endpoint key due to an internal error. Try again.");
                }

                return await this.UpsertKnowledgeBaseIdAsync(knowledgeBaseId).ConfigureAwait(false);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "The provided knowledgebase id is not valid.");
            }
        }

        /// <summary>
        /// Get already saved knowledgebase id from table storage.
        /// </summary>
        /// <returns>knowledgebase id.</returns>
        [HttpGet]
        public async Task<string> GetSavedKnowledgeBaseIdAsync()
        {
            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.KnowledgeBaseId).ConfigureAwait(false);
        }

        /// <summary>
        /// Save or update welcome message to be used by bot in table storage which is received from View.
        /// </summary>
        /// <param name="welcomeMessage">Welcome message text to show once the user install the bot for the very first time.</param>
        /// <returns>View.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveWelcomeMessageAsync(string welcomeMessage)
        {
            bool isSaved = await this.configurationPovider.UpsertEntityAsync(welcomeMessage, ConfigurationEntityTypes.WelcomeMessageText).ConfigureAwait(false);
            if (isSaved)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the welcome message due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved Welcome message from table storage.
        /// </summary>
        /// <returns>Welcome message.</returns>
        public async Task<string> GetSavedWelcomeMessageAsync()
        {
            var welcomeText = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(welcomeText))
            {
                await this.SaveWelcomeMessageAsync(Strings.DefaultWelcomeMessage).ConfigureAwait(false);
            }

            return await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.WelcomeMessageText).ConfigureAwait(false);
        }

        /// <summary>
        /// Save or update help tab text to be used by bot in table storage which is received from View.
        /// </summary>
        /// <param name="assignTimeout">timeout from unassigned to assigned.</param>
        /// <param name="pendingTimeout">timeout from pending to resolve.</param>
        /// <param name="resolveTimeout">timeout from assigned to resolve.</param>
        /// <param name="expertsAdmins">admins of expert channel</param>
        /// <returns>View.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SaveSLAAsync(string assignTimeout, string UnAssigneInterval, string pendingTimeout, string pendingInterval, string pendingCCInterval, string resolveTimeout, string unResolveInterval, string unResolveCCInterval, string expertsAdmins)
        {
            bool savedAssignTimeout = await this.configurationPovider.UpsertEntityAsync(assignTimeout, ConfigurationEntityTypes.AssignTimeout).ConfigureAwait(false);
            bool savedUnAssignInterval = await this.configurationPovider.UpsertEntityAsync(UnAssigneInterval, ConfigurationEntityTypes.UnassigneInterval).ConfigureAwait(false);
            bool savedPendingTimeout = await this.configurationPovider.UpsertEntityAsync(pendingTimeout, ConfigurationEntityTypes.PendingTimeout).ConfigureAwait(false);
            bool savedPendingInterval = await this.configurationPovider.UpsertEntityAsync(pendingInterval, ConfigurationEntityTypes.PendingInterval).ConfigureAwait(false);
            bool savedPendingCCInterval = await this.configurationPovider.UpsertEntityAsync(pendingCCInterval, ConfigurationEntityTypes.PendingCCInterval).ConfigureAwait(false);
            bool savedResolveTimeout = await this.configurationPovider.UpsertEntityAsync(resolveTimeout, ConfigurationEntityTypes.ResolveTimeout).ConfigureAwait(false);
            bool savedUnResolveInterval = await this.configurationPovider.UpsertEntityAsync(unResolveInterval, ConfigurationEntityTypes.UnResolveInterval).ConfigureAwait(false);
            bool savedUnResolveCCInterval = await this.configurationPovider.UpsertEntityAsync(unResolveCCInterval, ConfigurationEntityTypes.UnResolveCCInterval).ConfigureAwait(false);
            bool savedExpertsAdmins = await this.configurationPovider.UpsertEntityAsync(expertsAdmins, ConfigurationEntityTypes.ExpertsAdmins).ConfigureAwait(false);
            if (savedAssignTimeout && savedPendingTimeout && savedResolveTimeout && savedExpertsAdmins && savedUnAssignInterval && savedPendingInterval && savedPendingCCInterval && savedUnResolveInterval && savedUnResolveCCInterval)
            {
                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "Sorry, unable to save the help tab text due to an internal error. Try again.");
            }
        }

        /// <summary>
        /// Get already saved help tab message from table storage.
        /// </summary>
        /// <returns>Help tab text.</returns>
        [HttpGet]
        public async Task<string> GetSavedSLAAsync()
        {
            var assignTimeout = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.AssignTimeout).ConfigureAwait(false);
            var unAssigneInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.UnassigneInterval).ConfigureAwait(false);
            var pendingTimeout = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingTimeout).ConfigureAwait(false);
            var pendingInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingInterval).ConfigureAwait(false);
            var pendingCCInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingCCInterval).ConfigureAwait(false);
            var resolveTimeout = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.ResolveTimeout).ConfigureAwait(false);
            var unResolveInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.UnResolveInterval).ConfigureAwait(false);
            var unResolveCCInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.UnResolveCCInterval).ConfigureAwait(false);
            var expertsAdmins = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.ExpertsAdmins).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(assignTimeout) || string.IsNullOrWhiteSpace(pendingTimeout) || string.IsNullOrWhiteSpace(resolveTimeout)
                || string.IsNullOrWhiteSpace(unAssigneInterval) || string.IsNullOrWhiteSpace(pendingInterval) || string.IsNullOrWhiteSpace(pendingCCInterval) || string.IsNullOrWhiteSpace(unResolveInterval) || string.IsNullOrWhiteSpace(unResolveCCInterval))
            {
                await this.SaveSLAAsync(
                    string.IsNullOrWhiteSpace(assignTimeout) ? Strings.DefaultAssignTimeout : assignTimeout,
                    string.IsNullOrWhiteSpace(unAssigneInterval) ? Strings.DefaultUnAssignInterval : unAssigneInterval,
                    string.IsNullOrWhiteSpace(pendingTimeout) ? Strings.DefaultPendingTimeout : pendingTimeout,
                    string.IsNullOrWhiteSpace(pendingInterval) ? Strings.DefaultPendingInterval : pendingInterval,
                    string.IsNullOrWhiteSpace(pendingCCInterval) ? Strings.DefaultPendingCCInterval : pendingCCInterval,
                    string.IsNullOrWhiteSpace(resolveTimeout) ? Strings.DefaultResolveTimeout : resolveTimeout,
                    string.IsNullOrWhiteSpace(unResolveInterval) ? Strings.DefaultUnResolveInterval : unResolveInterval,
                    string.IsNullOrWhiteSpace(unResolveCCInterval) ? Strings.DefaultUnResolveCCInterval : unResolveCCInterval,
                    string.IsNullOrWhiteSpace(expertsAdmins) ? string.Empty : expertsAdmins).ConfigureAwait(false);
            }

            assignTimeout = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.AssignTimeout).ConfigureAwait(false);
            unAssigneInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.UnassigneInterval).ConfigureAwait(false);
            pendingTimeout = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingTimeout).ConfigureAwait(false);
            pendingInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingInterval).ConfigureAwait(false);
            pendingCCInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.PendingCCInterval).ConfigureAwait(false);
            resolveTimeout = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.ResolveTimeout).ConfigureAwait(false);
            unResolveInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.UnResolveInterval).ConfigureAwait(false);
            unResolveCCInterval = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.UnResolveCCInterval).ConfigureAwait(false);
            expertsAdmins = await this.configurationPovider.GetSavedEntityDetailAsync(ConfigurationEntityTypes.ExpertsAdmins).ConfigureAwait(false);

            return JsonConvert.SerializeObject(new SLAViewModel()
            {
                AssignTimeOut = assignTimeout,
                UnAssigneInterval = unAssigneInterval,
                PendingTimeOut = pendingTimeout,
                PendingInterval = pendingInterval,
                PendingCCInterval = pendingCCInterval,
                ResolveTimeOut = resolveTimeout,
                UnResolveInterval = unResolveInterval,
                UnResolveCCInterval = unResolveCCInterval,
                ExpertsAdmins = expertsAdmins,
            });
        }

        /// <summary>
        /// Based on deep link URL received find team id and return it to that it can be saved.
        /// </summary>
        /// <param name="teamIdDeepLink">team id deep link.</param>
        /// <returns>team id decoded string.</returns>
        private static string ParseTeamIdFromDeepLink(string teamIdDeepLink)
        {
            // team id regex match
            // for a pattern like https://teams.microsoft.com/l/team/19%3a64c719819fb1412db8a28fd4a30b581a%40thread.tacv2/conversations?groupId=53b4782c-7c98-4449-993a-441870d10af9&tenantId=72f988bf-86f1-41af-91ab-2d7cd011db47
            // regex checks for 19%3a64c719819fb1412db8a28fd4a30b581a%40thread.tacv2
            var match = Regex.Match(teamIdDeepLink, @"teams.microsoft.com/l/team/(\S+)/");

            if (!match.Success)
            {
                return string.Empty;
            }

            return HttpUtility.UrlDecode(match.Groups[1].Value);
        }

        /// <summary>
        /// Check if provided knowledgebase id is valid or not.
        /// </summary>
        /// <param name="knowledgeBaseId">knowledgebase id.</param>
        /// <returns>A <see cref="Task"/> of type bool where true represents provided knowledgebase id is valid while false indicates provided knowledgebase id is not valid.</returns>
        private async Task<bool> IsKnowledgeBaseIdValid(string knowledgeBaseId)
        {
            try
            {
                var knowledgebaseDetail = await this.qnaMakerClient.Knowledgebase.GetDetailsAsync(knowledgeBaseId).ConfigureAwait(false);
                return knowledgebaseDetail.Id == knowledgeBaseId;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return false;
            }
        }

        /// <summary>
        /// Update the saved endpoint key.
        /// </summary>
        /// <returns>A <see cref="Task"/> of type bool where true represents updated data is saved or updated successfully while false indicates failure in saving or updating the updated data.</returns>
        private async Task<bool> RefreshQnAMakerEndpointKeyAsync()
        {
            try
            {
                var endpointKeys = await this.qnaMakerClient.EndpointKeys.GetKeysAsync().ConfigureAwait(false);
                await this.configurationPovider.UpsertEntityAsync(endpointKeys.PrimaryEndpointKey, ConfigurationEntityTypes.QnAMakerEndpointKey).ConfigureAwait(false);
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return false;
            }
        }
    }
}