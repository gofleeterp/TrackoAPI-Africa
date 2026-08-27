using System;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace TrackoAPI.WebUtilities.Filters
{
    public class CacheHeader : ActionFilterAttribute
    {
        private readonly double _ageInSeconds;
        public CacheHeader():this(TimeSpan.FromDays(1))
        {

        }
        public CacheHeader(TimeSpan cacheTime)
        {
            _ageInSeconds = cacheTime.TotalSeconds;
        }
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            try
            {
                actionExecutedContext.Response.Headers.Add("Cache-Control", $"public, max-age={_ageInSeconds}");
            }
            catch (Exception e)
            {
                //e.ToExceptionless().AddObject(actionExecutedContext.Request.Properties);
            }
            base.OnActionExecuted(actionExecutedContext);
        }
    }
}
