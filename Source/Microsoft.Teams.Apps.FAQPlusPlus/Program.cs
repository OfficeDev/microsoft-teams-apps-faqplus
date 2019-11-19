// <copyright file="Program.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    /// <summary>
    /// This a Program  main class for this Bot.
    /// </summary>
    public class Program
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
                .UseStartup<Startup>();
    }
}