using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Results;
using System.Web.OData.Extensions;
using Exceptionless;
using Microsoft.Data.OData;
using Microsoft.OData.Core;
using TrackoApi.Core.Helpers;

namespace TrackoApi.Core.Handler
{
    //A global exception handler that will be used to catch any error
    public class ExceptionHandlerExt: ExceptionHandler
    {
        //A basic DTO to return back to the caller with data about the error
        private BusinessException CoreException;
        private class ErrorInformation
        {
            public string Message { get; set; }
            public DateTime ErrorDate { get; set; }

            public string DetailError { get; set; }
        }
        public override void Handle(ExceptionHandlerContext context)
        {
            var sb=new StringBuilder();
            BuildString(context.Exception, sb);
            //var exception = sb.ToString();
            CoreException = context.Exception as BusinessException;
            if (CoreException == null)
            {
                IsBusinessException(context.Exception);
            }
            
            if(CoreException != null)
            {
                var message=context.Request.CreateErrorResponse(CoreException.HttpStatusCode,new Microsoft.OData.Core.ODataError {ErrorCode = CoreException.ErrorCode.ToString(),Message = CoreException.Message,Details = new List<ODataErrorDetail>() {new ODataErrorDetail() {ErrorCode = CoreException.ErrorCode.ToString(),Message = CoreException.ExtraInfo } } });
                context.Result=new ResponseMessageResult(message);
            }
            else
            {
                context.Exception.ToExceptionless().Submit();
                context.Result = new ResponseMessageResult(context.Request.CreateResponse(HttpStatusCode.InternalServerError,
                  new ErrorInformation { Message = "Opps... Something went wrong. Please try again later.", ErrorDate = DateTime.UtcNow,DetailError=sb.ToString() }));
            }
        }

        public void BuildString(Exception ex, StringBuilder stringBuilder)
        {
            if (ex.InnerException != null)
            {
                BuildString(ex.InnerException, stringBuilder);
            }
            else
            {
                stringBuilder.AppendLine(ex.Message).AppendLine(ex.StackTrace);
            }
        }

        private bool IsBusinessException(Exception ex)
        {
            var isBusinessException = ex is BusinessException;
            if (!isBusinessException && ex.InnerException != null)
            {
                isBusinessException= IsBusinessException(ex.InnerException);
            }
            if (isBusinessException && CoreException == null)
            {
                CoreException = ex as BusinessException;
            }
            return isBusinessException;
        }
    }
}
