// <copyright file="AccountController.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Configuration.Controllers
{
    using System.Web;
    using System.Web.Mvc;
    using Microsoft.Owin.Security;
    using Microsoft.Owin.Security.Cookies;
    using Microsoft.Owin.Security.OpenIdConnect;

    /// <summary>
    /// Account Controller
    /// </summary>
    public class AccountController : Controller
    {
        /// <summary>
        /// sign in
        /// </summary>
        public void SignIn()
        {
            if (!this.Request.IsAuthenticated)
            {
                this.HttpContext.GetOwinContext().Authentication.Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    new string[] { OpenIdConnectAuthenticationDefaults.AuthenticationType, "AppLogin" });
            }
        }

        /// <summary>
        /// sign out
        /// </summary>
        public void SignOut()
        {
            string callbackUrl = this.Url.Action("SignOutCallback", "Account", routeValues: null, protocol: this.Request.Url.Scheme);

            this.HttpContext.GetOwinContext().Authentication.SignOut(
                new AuthenticationProperties { RedirectUri = callbackUrl }, new string[] { OpenIdConnectAuthenticationDefaults.AuthenticationType, CookieAuthenticationDefaults.AuthenticationType, "AppLogin" });
        }

        /// <summary>
        /// callback method
        /// </summary>
        /// <returns> returns view</returns>
        public ActionResult SignOutCallback()
        {
            if (this.Request.IsAuthenticated)
            {
                return this.RedirectToAction("Index", "Home");
            }

            return this.View();
        }

        /// <summary>
        /// Invalid User
        /// </summary>
        /// <returns>Action Result</returns>
        public ActionResult InvalidUser()
        {
            this.HttpContext.GetOwinContext().Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
            return this.View();
        }
    }
}