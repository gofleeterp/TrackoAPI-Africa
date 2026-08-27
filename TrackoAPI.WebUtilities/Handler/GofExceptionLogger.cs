using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using TrackoAPI.WebUtilities.Helper;
using Unity;

namespace TrackoAPI.WebUtilities.Handler
{
    public class GofExceptionLogger:ExceptionLogger
    {
        private readonly ILogger logger;

        public GofExceptionLogger()
        {
            var config = Unity.Config.UnityCore.Container;
            logger = config.Resolve<ILogger>();
        }
        public override void Log(ExceptionLoggerContext context)
        {
            
            if (logger != null) {
                var req=context.ExceptionContext.Request;
                var isClientInDebugMode= req.GetHeader("IsTraceEnabled").ToLower() == "true";
                
                    var reqcontent = new
                {
                    req.Method,
                    RequestUri = req.RequestUri.OriginalString,
                    Content = isClientInDebugMode? req.Content.ReadAsStringAsync().Result:null,
                    TenantName=TrackoApi.Core.Helpers.Helper.TenantShortName,
                    TenantId= TrackoApi.Core.Helpers.Helper.LoggedInTenantId,
                    TrackoApi.Core.Helpers.Helper.UserName,
                    LoggedInUserFullName=TrackoApi.Core.Helpers.Helper.GetLoggedInUserFullName(),
                    LoggedInUserId =TrackoApi.Core.Helpers.Helper.GetLoggedInUserId()
                };
                logger.Error(new {
                    Exception = context.Exception.ToStringDemystified(),
                    Request = reqcontent
                });
            }
            base.Log(context);
        }
        public override Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            //var config = Unity.Config.UnityCore.Container;
            //var logger = config.Resolve<ILogger>();
            if (logger != null)
            {
                var req = context.ExceptionContext.Request;
                var isClientInDebugMode = req.GetHeader("IsTraceEnabled").ToLower() == "true";

                var reqcontent = new
                {
                    req.Method,
                    RequestUri = req.RequestUri.OriginalString,
                    Content = isClientInDebugMode ? req.Content.ReadAsStringAsync().Result : null,
                    TenantName = TrackoApi.Core.Helpers.Helper.TenantShortName,
                    TenantId = TrackoApi.Core.Helpers.Helper.LoggedInTenantId,
                    TrackoApi.Core.Helpers.Helper.UserName,
                    LoggedInUserFullName = TrackoApi.Core.Helpers.Helper.GetLoggedInUserFullName(),
                    LoggedInUserId = TrackoApi.Core.Helpers.Helper.GetLoggedInUserId()
                };
                logger.Error(new
                {
                    Exception = context.Exception.ToStringDemystified(),
                    Request = reqcontent
                });
            }
            return base.LogAsync(context, cancellationToken);
        }

        public override bool ShouldLog(ExceptionLoggerContext context)
        {
            if(context.Request.GetHeader("IsTraceEnabled").ToLower() == "true")
            {
                return true;
            }
            return base.ShouldLog(context);
        }
    }
}
