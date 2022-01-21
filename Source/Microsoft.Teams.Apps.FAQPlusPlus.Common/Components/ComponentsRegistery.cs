// <copyright file="ComponentsRegistery.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Components
{
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Components registerer class.
    /// </summary>
    public static class ComponentsRegistery
    {
        /// <summary>
        /// Service Collection extension.
        /// </summary>
        /// <param name="services">Servie collection.</param>
        /// <returns>Service collection.</returns>
        public static IServiceCollection AddComponentServices(this IServiceCollection services)
        {
            services.AddTransient<IBotCommandResolver, BotCommandResolver>();
            services.AddTransient<INotificationService, NotificationService>();
            services.AddTransient<IQnAPairServiceFacade, QnAPairServiceFacade>();
            services.AddTransient<IConversationService, ConversationService>();

            return services;
        }
    }
}
