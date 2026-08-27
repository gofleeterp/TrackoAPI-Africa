using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TrackoAPI.WebUtilities.Handler
{
    public class ApiSessionView
    {
        public int Count { get; set; }
        public DateTime LastRequest { get; set; }
        public int LockedTimeOut { get; set; }
        public string TenentKey { get; set; }
        public string Url { get; set; }
        public long UserId { get; set; }
    }

    public class LogMessageHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                //TODO:Add Logic to Log Exception
                //if (response.StatusCode == HttpStatusCode.InternalServerError)
                //{
                //    ExceptionlessClient.Default.CreateException(response.)
                //    ExceptionlessClient.Default.CreateLog(await response.Content.ReadAsStringAsync()).Submit();
                //}
            }
            return response;
        }
    }
}