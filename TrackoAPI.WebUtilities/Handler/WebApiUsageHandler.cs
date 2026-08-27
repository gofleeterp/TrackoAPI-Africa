using Microsoft.Owin;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using TrackoAPI.Models.Shared;

namespace TrackoAPI.WebUtilities.Handler
{
    public class WebApiUsageHandler : DelegatingHandler
    {
        //public static Dictionary<string, DateTime> RequestLog = new Dictionary<string, DateTime>();
        public static ObservableCollection<ApiSessionView> SessionLog = new ObservableCollection<ApiSessionView>();

        /// <exception cref="BusinessException">Session has expired.</exception>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // System.Runtime.Caching.MemoryCache.Default.Add(())
            //#if DEBUG
            //            var isMetaDataRequest = request.RequestUri.LocalPath.ToLower().Contains("$metadata")|| request.RequestUri.LocalPath.ToLower().Contains("%24metadata")|| request.RequestUri.LocalPath.ToLower().EndsWith("/odata") || request.RequestUri.LocalPath.ToLower().EndsWith("/mex")|| request.RequestUri.LocalPath.ToLower().Contains("/hangfire")|| request.RequestUri.LocalPath.ToLower().EndsWith("/api/apisecurity/registerdevice") || request.RequestUri.LocalPath.ToLower().Contains("/api/apisecurity/authorizedevice");
            //            if(isMetaDataRequest) return await base.SendAsync(request, cancellationToken);
            ////#endif
            //            if(request.RequestUri.LocalPath.Contains("/Tenant/")&&request.Headers.Contains("godkey")) return await base.SendAsync(request, cancellationToken);
            //            if (request.RequestUri.LocalPath.ToLower().Contains("/clientsettings/initializeclient")|| request.RequestUri.LocalPath.ToLower().Contains("/clientsettings/ip2location")) return await base.SendAsync(request, cancellationToken);
           
                var req = request.Properties["MS_OwinContext"] as OwinContext;
                if (req == null || req.Authentication.User is WindowsPrincipal)
                {
                    // throw new BusinessException(ErrorCode.GLB100);
                    var response = await base.SendAsync(request, cancellationToken);
                    return response;
                }

                var tenantKey = req.GetClaimFromOwinContext<string>("TenantId");
                var appkey = req.GetClaimFromOwinContext<string>("ApplicationId");
                var userKey = req.GetClaimFromOwinContext<long>("UserId");

                var logType = (LogType)req.GetClaimFromOwinContext<int>("LogType");
                if (string.IsNullOrWhiteSpace(tenantKey) || string.IsNullOrWhiteSpace(appkey) || userKey <= 0)
                {
                    throw new BusinessException(ErrorCode.GLB102);
                }
                var apilog = WebApiUsageRequestExtract(request, tenantKey, appkey, userKey);
                apilog.ResponseContent = request.Content.ReadAsStringAsync().Result;
                
                return await base.SendAsync(request, cancellationToken).ContinueWith(
                    task =>
                    {
                        if (task.IsFaulted && task.Exception != null)
                        {
                            var be = task.Exception.GetBaseException() as BusinessException;
                            var baseex = be ?? task.Exception.GetBaseException();
                            var stack =
                                $"Exception Type:{baseex.GetType().FullName}{Environment.NewLine} Message:{baseex.GetBaseException().Message} {Environment.NewLine} Stack Trace:{baseex.StackTrace}";
                            var message = "";
                            if (be?.ODataErrorDetails != null)
                            {
                                message = "Business logic Failed are:" + be.ODataErrorDetails.Select(x => $"{x.Message}").JoinStrings(Environment.NewLine);
                            }
                            apilog.ResponseContent = $"{message}{Environment.NewLine}{stack}";
                            apilog.ResponseStatusCode = be == null ? 500 : (int)be.HttpStatusCode;
                            apilog.ResponseTimestamp = DateTime.Now;
                        }
                        else
                        {
                            WebApiUsageResponseExtract(task.Result, apilog);
                            if (task.Result.Content != null)
                            {
                                try
                                {
                                    apilog.ResponseContent = task.Result.Content.ReadAsStringAsync().Result;
                                }
                                catch (Exception)
                                {
                                    apilog.ResponseContent = "";
                                }
                            }
                        }
                        if (logType == LogType.ErrorOnly && (apilog.ResponseStatusCode >= 200 && apilog.ResponseStatusCode <= 208))
                        {
                            return task.Result;
                        }
                        if (logType == LogType.AllButNot401 && apilog.ResponseStatusCode == 401)
                        {
                            return task.Result;
                        }
                        if (logType == LogType.ErrorExcept404 && (apilog.ResponseStatusCode >= 200 && apilog.ResponseStatusCode <= 208) && apilog.ResponseStatusCode != 404)
                        {
                            return task.Result;
                        }
                        if (logType == LogType.None)
                        {
                            return task.Result;
                        }
                        if (logType == LogType.NotAllBut401 && apilog.ResponseStatusCode != 401)
                        {
                            return task.Result;
                        }
                        if (logType == LogType.AllButNot404 && apilog.ResponseStatusCode == 404)
                        {
                            return task.Result;
                        }
                        if (!TrackoApi.Core.Helpers.Helper.HostedOnPremise && apilog.ResponseStatusCode==400)
                        {
                            using (var db = new TenantDbContext())
                            {
                                db.ApiLog.Add(apilog);
                                try
                                {
                                    db.SaveChanges();
                                }
                                catch (Exception ex)
                                {
                                    var tags = new[] { "ResponseLogError" };
                                    if (!string.IsNullOrWhiteSpace(tenantKey))
                                    {
                                        tags[1] = tenantKey;
                                    }
                                    //ex.ToExceptionless().MarkAsCritical().AddTags(tags).AddObject(apilog).Submit();
                                }
                            }
                        }
                        return task.Result;
                    }, cancellationToken);
            
        }

        private WebApiUsage WebApiUsageRequestExtract(HttpRequestMessage request, string tenantKey, string appKey,
            long userKey)
        {
            if (request == null)
            {
                throw new BusinessException(ErrorCode.GLB100, "Request cannot be null");
            }
            var entity = new WebApiUsage
            {
                RequestMethod = request.Method.Method,
                Uri = request.RequestUri.ToString(),
                IP = HttpContext.Current.Request.UserHostAddress,
                TenantKey = tenantKey,
                RequestTimestamp = DateTime.Now,
                ApplicationKey = appKey,
                UserKey = userKey
            };
            entity.RequestHeaders = entity.extractHeaders(request.Headers);
            return entity;
        }

        private void WebApiUsageResponseExtract(HttpResponseMessage response, WebApiUsage entity)
        {
            if (response == null)
            {
                throw new BusinessException(ErrorCode.GLB100, "response cannot be null");
            }
            entity.ResponseStatusCode = Convert.ToInt32(response.StatusCode);
            entity.ResponseTimestamp = DateTime.Now;
            entity.ResponseHeaders = entity.extractHeaders(response.Headers);
        }
    }
}