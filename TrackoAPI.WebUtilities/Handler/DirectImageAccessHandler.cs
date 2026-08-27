using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http.Hosting;
using TrackoApi.Core.Helpers;

namespace TrackoAPI.WebUtilities.Handler
{
    public class DirectImageAccessHandler: DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var configuredPath = Utilities.FileUploadFolder();
            if (!string.IsNullOrWhiteSpace(configuredPath))
            {
                configuredPath = ("/$" + (configuredPath.StartsWith("~") ? configuredPath.Remove(0, 2) : (configuredPath.StartsWith("\\") ? configuredPath.Remove(0, 1) : configuredPath))).ToLower();
            }
            if (string.IsNullOrWhiteSpace(configuredPath)) return await base.SendAsync(request, cancellationToken);
            var isFileAccess = request.RequestUri.LocalPath.ToLower().Contains(configuredPath);
            if (!isFileAccess)
            {

                return await base.SendAsync(request, cancellationToken);
            }
            var actualpath = request.RequestUri.LocalPath.ToLower().Replace("$", "");
            var filepath = HostingEnvironment.MapPath(actualpath);
            if (string.IsNullOrWhiteSpace(filepath) || !File.Exists(filepath))
            {
                var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return await tsc.Task;
            }
            else
            {
                var contentType = System.Web.MimeMapping.GetMimeMapping(filepath);
                var response = request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StreamContent(File.OpenRead(filepath));
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                var tsc = new TaskCompletionSource<HttpResponseMessage>();
                tsc.SetResult(response);
                return await tsc.Task;
            }
        }

        //private static bool HasImageExtension(string source)
        //{
        //    return (source.EndsWith(".png") || source.EndsWith(".jpg") || source.EndsWith(".jpeg") || source.EndsWith(".jfif") || source.EndsWith(".bmp") || source.EndsWith(".tif") || source.EndsWith(".tiff") || source.EndsWith(".gif"));
        //}
    }
}
