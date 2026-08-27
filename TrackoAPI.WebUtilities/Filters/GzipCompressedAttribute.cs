using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace TrackoAPI.WebUtilities.Filters
{
    public class GzipCompressedAttribute : System.Web.Http.Filters.ActionFilterAttribute
    {
        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actContext, CancellationToken token)
        {
            bool supportGZip = actContext.Request.Headers.AcceptEncoding.Any(x => x.Value == "gzip");

            if (!supportGZip)
            {
                await base.OnActionExecutedAsync(actContext, token);
                return;
            }

            var contentStream = await actContext.Response.Content.ReadAsStreamAsync();

            actContext.Response.Content = new PushStreamContent(async (stream, content, context) =>
             {
                 using (contentStream)
                 using (var zipStream = new GZipStream(stream, CompressionLevel.Optimal))
                 {
                     await contentStream.CopyToAsync(zipStream);
                 }
             });

            actContext.Response.Content.Headers.Remove("Content-Type");
            actContext.Response.Content.Headers.Add("Content-encoding", "gzip");
            actContext.Response.Content.Headers.Add("Content-Type", "application/json");
        }
    }
}
