using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Hosting;

namespace TrackoAPI.WebUtilities.Handler
{
    public class DollarFormatHandler : DelegatingHandler
    {
        //HttpConfiguration.MessageHandlers.Add(new DollarFormatHandler());
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var queryParams = request.GetQueryNameValuePairs();
            var dollarFormat = queryParams.Where(kvp => kvp.Key == "$format").Select(kvp => kvp.Value).FirstOrDefault();

            if (dollarFormat != null)
            {
                request.Headers.Accept.Clear();
                request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(dollarFormat));

                // remove $format from the request.
                request.Properties[HttpPropertyKeys.RequestQueryNameValuePairsKey] = queryParams.Where(kvp => kvp.Key != "$format");
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}
