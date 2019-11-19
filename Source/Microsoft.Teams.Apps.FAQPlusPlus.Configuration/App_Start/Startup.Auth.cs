// <copyright file="Startup.Auth.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration
{
    using System;
    using System.Configuration;
    using System.IdentityModel.Claims;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Helpers;
    using global::Owin;
    using Microsoft.IdentityModel.Protocols.OpenIdConnect;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.OpenIdConnect;

    /// <summary>
    /// Startup file
    /// </summary>
    public partial class Startup
    {
        private static string clientId = ConfigurationManager.AppSettings["ida:ClientId"];
        private static string aadInstance = EnsureTrailingSlash(ConfigurationManager.AppSettings["ida:AADInstance"]);
        private static string tenantId = ConfigurationManager.AppSettings["ida:TenantId"];
        private static string postLogoutRedirectUri = ConfigurationManager.AppSettings["ida:PostLogoutRedirectUri"];
        private static string authority = aadInstance + tenantId;

        /// <summary>
        /// Configure Auth
        /// </summary>
        /// <param name="app">App builder</param>
        /// <param name="container">DI container</param>
        public void ConfigureAuth(IAppBuilder app, Autofac.IContainer container)
        {
            app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

            app.UseCookieAuthentication(new CookieAuthenticationOptions());

            var validUpns = ConfigurationManager.AppSettings["ValidUpns"]
              ?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
              ?.Select(s => s.Trim())
              ?? new string[0];

            app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions("AppLogin")
            {
                ClientId = clientId,
                Authority = authority,
                PostLogoutRedirectUri = postLogoutRedirectUri,
                Notifications = new OpenIdConnectAuthenticationNotifications()
                {
                    SecurityTokenValidated = (context) =>
                    {
                        var upnClaim = context?.AuthenticationTicket?.Identity?.Claims?
                            .FirstOrDefault(c => c.Type == ClaimTypes.Upn);
                        var upn = upnClaim?.Value;

                        if (upn == null
                            || !validUpns.Contains(upn, StringComparer.OrdinalIgnoreCase))
                        {
                            context.OwinContext.Response.Redirect("/Account/InvalidUser");
                            context.HandleResponse();
                        }

                        return Task.CompletedTask;
                    },
                    RedirectToIdentityProvider = (context) =>
                    {
                        if (context.ProtocolMessage.RequestType == OpenIdConnectRequestType.Authentication)
                        {
                            context.ProtocolMessage.Prompt = OpenIdConnectPrompt.Login;
                        }

                        return Task.CompletedTask;
                    },
                },
            });
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.Upn;
        }

        private static string EnsureTrailingSlash(string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }

            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                return value + "/";
            }

            return value;
        }
    }
}