using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace TrackoAPI.WebUtilities.Filters
{
    public class IPHostValidationAttribute: ActionFilterAttribute
    {
        public static IQueryable<string> GetAuthorizedIPs()
        {

            var ips = new List<string>();

            ips.Add("127.0.0.1");
            ips.Add("::1");

            return ips.AsQueryable();
        }
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var resuest=HttpContext.Current.Request;
            var context = actionContext.Request.Properties["MS_HttpContext"] as System.Web.HttpContextBase;
            string userIP = context.Request.UserHostAddress;
            try
            {
                GetAuthorizedIPs().First(x => x == userIP);
            }
            catch (Exception)
            {
                actionContext.Response =
                   new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
                   {
                       Content = new StringContent("Unauthorized IP Address")
                   };
                return;
            }
        }
    }
}
