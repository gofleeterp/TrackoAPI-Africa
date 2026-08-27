using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Exceptionless;
using Microsoft.Owin;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Global;

namespace TrackoApi.Core.Handler
{
    public class LogMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                //TODO:Add Logic to Log Exception
                ExceptionlessClient.Default.CreateLog(await response.Content.ReadAsStringAsync()).Submit();
            }
            return response;
        }
    }
    public class WebApiUsageHandler : DelegatingHandler
    {
        //private static readonly IApiUsageRepository _repo = new ApiUsageRepository();

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var req = request.Properties["MS_OwinContext"] as OwinContext;
            if (req==null||req.Authentication.User is WindowsPrincipal)
            {
                throw new BusinessException(ErrorCode.GLB100);
            }
            var tenantKey = req.GetClaimFromOwinContext<string>("TenantId");
            var appkey= req.GetClaimFromOwinContext<string>("ApplicationId");
            var userKey= req.GetClaimFromOwinContext<long>("UserId");
            if (string.IsNullOrWhiteSpace(tenantKey) || string.IsNullOrWhiteSpace(appkey) || userKey <= 0)
            {
                throw new BusinessException(ErrorCode.GLB102);
            }
            var apiRequest = WebApiUsageRequestExtract(request, tenantKey,appkey,userKey);
            apiRequest.Content = request.Content.ReadAsStringAsync().Result;
            ExceptionlessClient.Default.CreateLog("Api Requested by key:").AddObject(apiRequest).Submit();

            return base.SendAsync(request, cancellationToken).ContinueWith(
                task =>
                {
                    var apiResponse = WebApiUsageResponseExtract(task.Result, tenantKey, appkey, userKey);
                    apiResponse.Content = task.Result.Content.ReadAsStringAsync().Result;
                    //_repo.Add(apiResponse);
                    ExceptionlessClient.Default.CreateLog("Api response to key:").AddObject(apiResponse).Submit();
                    return task.Result;
                }, cancellationToken);
        }

        private WebApiUsageResponse WebApiUsageResponseExtract(HttpResponseMessage response, string tenantKey, string appKey, long userKey)
        {
            if (response != null)
            {
                var usage = new WebApiUsageResponse
                {
                    UsageType = response.GetType().Name,
                    StatusCode = Convert.ToInt32(response.StatusCode),
                    Timestamp = DateTime.Now,
                    TenantKey = tenantKey,
                    ApplicationKey = appKey,
                    UserKey = userKey
                };
                usage.extractHeaders(response.Headers);
                return usage;
            }
            throw new BusinessException(ErrorCode.GLB100,"response cannot be null");
        }

        private WebApiUsageRequest WebApiUsageRequestExtract(HttpRequestMessage request, string tenantKey,string appKey,long userKey)
        {
            if (request == null)
            {
                throw new BusinessException(ErrorCode.GLB100, "Request cannot be null");
            }
            var usage = new WebApiUsageRequest()
            {
                UsageType = request.GetType().Name,
                RequestMethod = request.Method.Method,
                Uri = request.RequestUri.ToString(),//HttpContextBase
                IP = HttpContext.Current.Request.UserHostAddress,
                TenantKey = tenantKey,
                Timestamp = DateTime.Now,
                ApplicationKey = appKey,
                UserKey = userKey
            };
            usage.extractHeaders(request.Headers);
            return usage;
        }
    }
}
