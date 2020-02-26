// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using System;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// This a Program  main class for this Bot.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// This main class for this Bot.
        /// </summary>
        /// <param name="args">String of Arguments.</param>
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        /// <summary>
        /// This method will hit the Startup Method to set up the complete bot services.
        /// </summary>
        /// <param name="args">String of Arguments.</param>
        /// <returns>A unit of Execution.</returns>
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .ConfigureLogging((hostingContext, logging) =>
            {
                // hostingContext.HostingEnvironment can be used to determine environments as well.
                var appInsightKey = hostingContext.Configuration["ApplicationInsights:InstrumentationKey"];
                logging.AddApplicationInsights(appInsightKey);

                // This will capture Info level traces and above.
                var logLevel = LogLevel.Information;
                Enum.TryParse(hostingContext.Configuration["ApplicationInsightsLogLevel"], out logLevel);
                logging.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>(string.Empty, logLevel);
            });
    }
}