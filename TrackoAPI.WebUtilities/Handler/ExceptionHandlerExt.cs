using Microsoft.OData.Core;
using NLog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Filters;
using System.Web.Http.Results;
using TrackoApi.Core.Helpers;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.WebUtilities.Handler
{
    //A global exception handler that will be used to catch any error
    public class ExceptionHandlerExt : ExceptionHandler
    {
        //A basic DTO to return back to the caller with data about the error
        private BusinessException CoreException;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static Exception GetInnermostException(Exception exception)
        {
            if (exception == null)
                return (Exception)null;
            Exception exception1 = exception;
            while (exception1.InnerException != null)
                exception1 = exception1.InnerException;
            return exception1;
        }

        public void BuildString(Exception ex, StringBuilder stringBuilder)
        {
            var exception = GetInnermostException(ex);
            stringBuilder.AppendLine(exception.Message).AppendLine(exception.StackTrace);
        }

        /// <exception cref="ArgumentNullException"><paramref name="key" /> is null.</exception>
        public override void Handle(ExceptionHandlerContext context)
        {
            try
            {
                LogException(context.Exception);
                if (context.Request.GetHeader("IsTraceEnabled").ToLower() == "true" && context.Request.Headers.Contains("SignalRConnectionId"))
                {
                    var connectionId = context.Request.GetHeader("SignalRConnectionId");
                    //context.Request.GetHubContext()?.PushEventSelf(connectionId, (context.Exception.GetBaseException()).ToString(), context.Request.RequestUri.ToString(), SignalR.Core.PushSelfMessageType.Error);
                    context.Request.GetHubContext()?.PushEventSelf(connectionId, (context.Exception).ToString(), context.Request.RequestUri.ToString(), SignalR.Core.PushSelfMessageType.Error);
                }                
            }
            catch
            {
                //Ignore
            }
            
            if (IsBusinessException(context.Exception))
            {
                bool isindebugMode = false;
#if DEBUG
                isindebugMode = true;
#endif
                
                var info = new Microsoft.OData.Core.ODataError
                {
                    ErrorCode = CoreException.ErrorCode.ToString(),
                    Message = CoreException.Message,
                    Target = $"https://africa.indiaweblab.com/fwlink?errorcode={CoreException.ErrorCode}",
                    Details = CoreException.ODataErrorDetails
                };
                if ((context.Request.GetHeader("IsTraceEnabled").ToLower() == "true" || isindebugMode))
                {
                    foreach (var error in CoreException.SqlErrors)
                    {
                        info.Details.Add(error);
                    }
                }
                var message = context.Request.CreateResponse(CoreException.HttpStatusCode, info);
                context.Result = new ResponseMessageResult(message);
            }
            else if (context.Exception.GetBaseException().Message ==
                     "'SingleResult`1' cannot be serialized using the ODataMediaTypeFormatter.")
            {
                var info = new Microsoft.OData.Core.ODataError
                {
                    ErrorCode = "GLB100",
                    Message = "Record Not Found",
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=GLB100"
                };
                var message = context.Request.CreateResponse(HttpStatusCode.NotFound, info);
                context.Result = new ResponseMessageResult(message);
            }
            else
            {
                var info = new ODataError
                {
                    ErrorCode = "Unkown",
                    Message = $"Opps... Something went wrong, And has been reported to technical Team.{Environment.NewLine} Please try again later. :(",
                    Details = new List<ODataErrorDetail>
                    {
                        new ODataErrorDetail
                        {
                            ErrorCode="Unkown",
                            Message= context.Exception?.GetBaseException().Message
                        }
                    }
                };
                context.Result = new ResponseMessageResult(context.Request.CreateResponse(HttpStatusCode.InternalServerError, info));
            }
        }

        private bool IsBusinessException(Exception ex)
        {
            var innerexception = ex.GetBusinessException();
            var isbe = innerexception != null;
            if (isbe)
            {
                CoreException = innerexception;
            }
            return isbe;
        }
        private void LogException(Exception exception)
        {
            // Implement your logging logic here
            Logger.Error(exception);
        }
    }

    

    public class CustomExceptionFilterAttribute : ExceptionFilterAttribute
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public override void OnException(HttpActionExecutedContext context)
        {
            // Log the exception, for example, using a logging framework like NLog or log4net.
            // You can also include the context.Request information if needed.
            LogException(context.Exception);

            // Create a generic error response
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("An unexpected error occurred. Please try again later."),
                ReasonPhrase = "Internal Server Error"
            };

            context.Response = response;
        }

        private void LogException(Exception exception)
        {
            // Implement your logging logic here
            Logger.Error(exception);
        }
    }

}