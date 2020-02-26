// <copyright file="GlobalSuppressions.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.
[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "Not required to catch specific exception.", Scope = "member", Target = "~M:Microsoft.Teams.Apps.FAQPlusPlus.AzureFunction.PublishFunction.Run(Microsoft.Azure.WebJobs.TimerInfo,Microsoft.Extensions.Logging.ILogger)~System.Threading.Tasks.Task")]