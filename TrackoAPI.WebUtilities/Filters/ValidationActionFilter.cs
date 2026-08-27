//using System.Web.OData.Extensions;
using Microsoft.OData.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using TrackoApi.Core.Helpers;

namespace TrackoAPI.WebUtilities.Filters
{
    public class ValidationActionFilter : ActionFilterAttribute
    {
        private Stopwatch _st;

        public ValidationActionFilter()
        {
            _st = new Stopwatch();
        }
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            _st.Start();
            var modelState = actionContext.ModelState;

            if (!modelState.IsValid)
            {
                
                var errors= (from state in modelState.ToList()
                             from error in state.Value.Errors
                             let exmsg = ($"ModelState failed{(error?.Exception != null ? " with Error Message :" + error.Exception?.Message?.Split('.')[0] : "")}.")
                             select $"{(string.IsNullOrWhiteSpace(error.ErrorMessage) ? exmsg : error.ErrorMessage)}. Field:{state.Key.Replace("Id", "")}"
                                       into msg
                             select new ODataErrorDetail()
                             {
                                 ErrorCode = ErrorCode.GLB106.ToString(),
                                 Message = msg
                             }).ToList(); 
                var info = new Microsoft.OData.Core.ODataError
                {
                    ErrorCode = ErrorCode.GLB106.ToString(),
                    Message = "Validation Failed",
                    Target = $"https://africa.indiaweblab.com/fwlink?errorcode={ErrorCode.GLB106}",
                    Details = errors
                };
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, info);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            
            if (actionExecutedContext.Response!=null&&!actionExecutedContext.Response.IsSuccessStatusCode)
            {
                string hint = string.Empty;
                var context = actionExecutedContext.ActionContext.ActionDescriptor;
                hint += $"{Environment.NewLine}Area: {context.ControllerDescriptor.ControllerName}, Action: {context.ActionName}";
                switch (actionExecutedContext.Response.StatusCode)
                {
                        case HttpStatusCode.NotFound:
                        var info = new Microsoft.OData.Core.ODataError
                        {
                            ErrorCode = ErrorCode.GLB106.ToString(),
                            Message = $"One Of Transaction Not Found"+hint,
                            Target = $"https://africa.indiaweblab.com/fwlink?errorcode={ErrorCode.GLB106}"
                        };
                        actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.NotFound, info);
                        break;
                        case HttpStatusCode.BadRequest:
                        if (!actionExecutedContext.ActionContext.ModelState.IsValid)
                        {
                            var modstate = actionExecutedContext.ActionContext.ModelState;
                            var keys= (from state in modstate.ToList()
                                       from error in state.Value.Errors
                                       let exmsg = ($"ModelState failed{(error?.Exception != null ? " with Error Message :" + error.Exception?.Message?.Split('.')[0] : "")}.")
                                       select $"{(string.IsNullOrWhiteSpace(error.ErrorMessage) ? exmsg : error.ErrorMessage)}. Field:{state.Key.Replace("Id","")}"
                                       into msg
                                       select new ODataErrorDetail()
                                       {
                                           ErrorCode = ErrorCode.GLB106.ToString(),
                                           Message = msg
                                       }).ToList();
                            var modelstatefailed400 = new Microsoft.OData.Core.ODataError
                            {
                                ErrorCode = ErrorCode.GLB106.ToString(),
                                Message = "Validation Failed" + hint,
                                Target = $"https://africa.indiaweblab.com/fwlink?errorcode={ErrorCode.GLB106}",
                                Details = keys
                            };
                            actionExecutedContext.Response = actionExecutedContext.Request.CreateResponse(HttpStatusCode.BadRequest, modelstatefailed400);
                        }
                        break;
                }
                
            }                      
            base.OnActionExecuted(actionExecutedContext);
            if (actionExecutedContext?.Request?.Properties !=null&& actionExecutedContext.Request.Properties.All(x => x.Key != "ResultConsumedTime"))
            {
                actionExecutedContext.Request.Properties["ResultConsumedTime"] = _st.Elapsed.TotalMilliseconds;
            }
            if (actionExecutedContext?.Response?.Headers!=null&&!actionExecutedContext.Response.Headers.Contains("ResultConsumedTime"))
            {
                actionExecutedContext.Response.Headers.Add("ResultConsumedTime", _st.Elapsed.TotalMilliseconds.ToString());
            }
            _st.Stop();
        }
    }
}
