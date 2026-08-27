using Microsoft.Owin.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;

namespace TrackoAPI.Infrastructure.Filters
{
    [AttributeUsage(validOn: AttributeTargets.Class|AttributeTargets.Method)]
    public class ApiKeyAttribute : AuthorizationFilterAttribute
    {
        public override async Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            try
            {

                var qn = actionContext.Request.RequestUri.ParseQueryString();
                if (!actionContext.Request.Headers.Any(x => x.Key.ToLower() == "apikey") && !qn.AllKeys.Any(x => x.ToLower() == "apikey"))
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(System.Net.HttpStatusCode.Unauthorized, "Authorization header not found");
                    actionContext.Response.ReasonPhrase = "Authorization header not found";
                    return;
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

                var _gc = actionContext.RequestContext.Configuration.DependencyResolver.GetService(typeof(IGlobalStore)) as IGlobalStore;
                TPTokenViewModel tokenInfo = null;
                tokenInfo = !Helper.HostedOnPremise ? _gc.GetOrAddToken(apikey, GetTokenInfoFromDb) : _gc.GetOrAddToken(apikey);
                if (tokenInfo == null)
                {                    
                    actionContext.Response = actionContext.Request.CreateErrorResponse(System.Net.HttpStatusCode.Unauthorized, "Api Key is not valid");
                    return;
                }

                var isvalid = tokenInfo.IsValidCall(out var errormessage, actionContext.ControllerContext.ControllerDescriptor.ControllerName, actionContext.ActionDescriptor.ActionName);
                if (!isvalid)
                {
                    actionContext.Response = actionContext.Request.CreateErrorResponse(System.Net.HttpStatusCode.Unauthorized, errormessage);
                    return;
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
                await base.OnAuthorizationAsync(actionContext, cancellationToken);
            }
            catch (Exception ex)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(System.Net.HttpStatusCode.InternalServerError, ex.GetBaseException().Message);
            }
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
                    TenantId = x.TenantId
                }).FirstOrDefault();
            }
        }
        //public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        //{
        //    if(!actionContext.Request.Headers.Any(x=>x.Key.ToLower()=="apikey"))
        //    {
        //        actionContext.Response.StatusCode = System.Net.HttpStatusCode.Unauthorized;
        //        return actionContext.Response;
        //    }
        //    var apikey = actionContext.Request.Headers.FirstOrDefault(x => x.Key.ToLower() == "apikey").Value?.FirstOrDefault();
        //    actionContext.ControllerContext.Contro
        //    return await continuation();
        //}

    }
}
