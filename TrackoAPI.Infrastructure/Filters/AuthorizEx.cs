using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;

namespace TrackoAPI.Infrastructure.Filters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AuthorizeExAttribute : System.Web.Http.AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            if (actionContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Forbidden);
            }
            else
            {
                base.HandleUnauthorizedRequest(actionContext);
            }
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (SkipAuthorization(actionContext))
            {
                return;
            }
            var isAuthenticated = actionContext.RequestContext.Principal.Identity.IsAuthenticated;
            base.OnAuthorization(actionContext);
        }

        public Task OnAuthorizationAsyncOld(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (actionContext == null)
            {
                throw new ArgumentNullException("actionContext");
            }

            if (SkipAuthorization(actionContext))
            {
                return Task.CompletedTask;
            }
            var isAuthenticated = actionContext.RequestContext.Principal.Identity.IsAuthenticated;
            return base.OnAuthorizationAsync(actionContext, cancellationToken);
        }
        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {
                if (SkipAuthorization(actionContext))
                {
                    return Task.CompletedTask;
                }
                var qn = actionContext.Request.RequestUri.ParseQueryString();
                if (!actionContext.Request.Headers.Any(x => x.Key.ToLower() == "apikey") && !qn.AllKeys.Any(x => x.ToLower() == "apikey"))
                {
                    return base.OnAuthorizationAsync(actionContext, cancellationToken);
                }
                string apikey = "";
                if (actionContext.Request.Headers.Any(x => x.Key.ToLower() == "apikey"))
                {
                    apikey = actionContext.Request.Headers.FirstOrDefault(x => x.Key.ToLower() == "apikey").Value?.FirstOrDefault();
                }
                else if (qn.AllKeys.Any(x => x.ToLower() == "apikey"))
                {
                    var key = qn.AllKeys.FirstOrDefault(x => x.ToLower() == "apikey");
                    apikey = qn.Get(key);
                }
                if (string.IsNullOrWhiteSpace(apikey))
                {
                    return base.OnAuthorizationAsync(actionContext, cancellationToken);
                }
                else
                {
                    var _gc = actionContext.RequestContext.Configuration.DependencyResolver.GetService(typeof(IGlobalStore)) as IGlobalStore;
                    TPTokenViewModel tokenInfo = null;
                    tokenInfo = !Helper.HostedOnPremise ? _gc.GetOrAddToken(apikey, GetTokenInfoFromDb) : _gc.GetOrAddToken(apikey);
                    if (tokenInfo == null)
                    {
                        actionContext.Response = actionContext.Request.CreateErrorResponse(System.Net.HttpStatusCode.Unauthorized, "Api Key is not valid");
                        return Task.CompletedTask;
                    }

                    var isvalid = tokenInfo.IsValidCall(out var errormessage, actionContext.ControllerContext.ControllerDescriptor.ControllerName, actionContext.ActionDescriptor.ActionName);
                    if (!isvalid)
                    {
                        actionContext.Response = actionContext.Request.CreateErrorResponse(System.Net.HttpStatusCode.Unauthorized, errormessage);
                        return Task.CompletedTask;
                    }
                    var owin = actionContext.Request.GetOwinContext();
                    if (owin != null)
                    {
                        var identity = new ClaimsIdentity("ApiKey");
                        identity.AddClaim(new Claim("sub", tokenInfo.Appidentity));
                        identity.AddClaim(new Claim("role", "integration"));
                        identity.AddClaim(new Claim("TenantId", tokenInfo.TenantId));
                        identity.AddClaim(new Claim("ApplicationId", tokenInfo.Appidentity));
                        identity.AddClaim(new Claim("UserId", "0"));
                        identity.AddClaim(new Claim("apikey", apikey));
                        owin.Set("as:tenantid", tokenInfo.TenantId);
                        owin.Authentication.SignIn(new AuthenticationProperties()
                        {
                            AllowRefresh = true,
                            IsPersistent = false,
                            ExpiresUtc = DateTime.UtcNow.AddMinutes(10)
                        }, identity);
                    }
                    //return base.OnAuthorizationAsync(actionContext, cancellationToken);
                    return Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, ex.GetBaseException().Message);
                return Task.CompletedTask;
            }
        }
        private bool SkipAuthorization(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any()
                   || actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<AllowAnonymousAttribute>().Any();
        }
        private TPTokenViewModel GetTokenInfoFromDb(string token)
        {
            using (var db = new TenantDbContext())
            {
                return db.ThirdPartyTokens
                .Where(x => token == x.Token)
                .Select(x => new TPTokenViewModel
                {
                    Token = x.Token,
                    AllowedPath = x.AllowedPath,
                    Appidentity = x.Appidentity,
                    ExpiryDate = x.ExpiryDate,
                    Interval = x.Interval,
                    IsDeactivated = x.IsDeactivated,
                    JsonMetaData = x.JsonMetaData,
                    LastCalledTime = x.LastCalledTime,
                    TenantId = x.TenantId,
                }).FirstOrDefault();
            }
        }
    }
}
