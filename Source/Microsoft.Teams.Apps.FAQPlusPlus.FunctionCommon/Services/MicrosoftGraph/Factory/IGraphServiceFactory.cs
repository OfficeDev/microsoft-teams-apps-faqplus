// <copyright file="IGraphServiceFactory.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.AzureFunctionCommon.Services.MicrosoftGraph
{
    /// <summary>
    /// Interface for Graph Service Factory.
    /// </summary>
    public interface IGraphServiceFactory
    {
        /// <summary>
        /// Creates an instance of <see cref="IUsersService"/> implementation.
        /// </summary>
        /// <returns>Returns an implementation of <see cref="IUsersService"/>.</returns>
        IUsersService GetUsersService();

        /// <summary>
        /// Creates an instance of <see cref="IGroupsService"/> implementation.
        /// </summary>
        /// <returns>Returns an implementation of <see cref="IGroupsService"/>.</returns>
        IGroupsService GetGroupsService();

        /// <summary>
        /// Creates an instance of <see cref="IGroupMembersService"/> implementation.
        /// </summary>
        /// <returns>Returns in implementation of <see cref="IGroupMembersService"/>.</returns>
        IGroupMembersService GetGroupMembersService();

        /// <summary>
        /// Creates an instance of <see cref="IChatsService"/> implementation.
        /// </summary>
        /// <returns>Returns an implementation of <see cref="IChatsService"/>.</returns>
        IChatsService GetChatsService();

        /// <summary>
        /// Creates an instance of <see cref="IAppManagerService"/> implementation.
        /// </summary>
        /// <returns>Returns an implementation of <see cref="IAppManagerService"/>.</returns>
        IAppManagerService GetAppManagerService();

        /// <summary>
        /// Creates an instance of <see cref="IAppCatalogService"/> implementation.
        /// </summary>
        /// <returns>Returns an implementation of <see cref="IAppCatalogService"/>.</returns>
        IAppCatalogService GetAppCatalogService();
    }
}
