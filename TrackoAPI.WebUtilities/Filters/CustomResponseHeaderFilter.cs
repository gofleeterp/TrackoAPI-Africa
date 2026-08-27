using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace TrackoAPI.WebUtilities.Filters
{
    public class CustomResponseHeaderFilter : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            BindCustomHeaders(ref actionExecutedContext);
            base.OnActionExecuted(actionExecutedContext);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            BindCustomHeaders(ref actionExecutedContext);
            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }
        private void BindCustomHeaders(ref HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                var actionconsumedTime = actionExecutedContext.Request.Properties.FirstOrDefault(x => x.Key == "ConsumedTime");
                actionExecutedContext.Response.Headers.Add("ConsumedTime", actionconsumedTime.Value.ToString());
            }
            catch (Exception e)
            {
                //e.ToExceptionless().AddObject(actionExecutedContext.Request.Properties);
            }
        }
    }

}