// <copyright file="AutofacConfig.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
using Azure.AI.Language.QuestionAnswering.Projects;

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration
{
    using System;
    using System.Configuration;
    using System.Reflection;
    using System.Web.Mvc;
    using Autofac;
    using Autofac.Integration.Mvc;
    using Azure;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Providers;

    /// <summary>
    /// Autofac configuration.
    /// </summary>
    public static class AutofacConfig
    {
        /// <summary>
        /// Register Autofac dependencies.
        /// </summary>
        /// <returns>Autofac container.</returns>
        public static IContainer RegisterDependencies()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());

            builder.Register(c => new ConfigurationDataProvider(
                 ConfigurationManager.AppSettings["StorageConnectionString"]))
                .As<IConfigurationDataProvider>()
                .SingleInstance();

            //var questionAnsweringProjectsClient = new QuestionAnsweringProjectsClient(
            //    new Uri(
            //        Environment.GetEnvironmentVariable("QnAMakerApiUrl"),
            //        new AzureKeyCredential(
            //        Environment.GetEnvironmentVariable("QnAMakerSubscriptionKey"))));

            //builder.Register(c => questionAnsweringProjectsClient)
            //    .As<QuestionAnsweringProjectsClient>()
            //    .SingleInstance();

            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            return container;
        }
    }
}