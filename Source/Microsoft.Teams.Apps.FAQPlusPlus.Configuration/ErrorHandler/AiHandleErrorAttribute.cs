// <copyright file="AiHandleErrorAttribute.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.ErrorHandler{    using System;    using System.Web.Mvc;    using Microsoft.ApplicationInsights;
    /// <summary>
    /// Report unhandled errors to Application Insights
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]    public class AiHandleErrorAttribute : HandleErrorAttribute    {        /// <inheritdoc/>
        public override void OnException(ExceptionContext filterContext)        {            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)            {
                // If customError is Off, then AI HTTPModule will report the exception
                if (filterContext.HttpContext.IsCustomErrorEnabled)                {                    var ai = new TelemetryClient();                    ai.TrackException(filterContext.Exception);                }            }            base.OnException(filterContext);        }    }}