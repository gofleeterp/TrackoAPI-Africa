using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace TrackoAPI.WebUtilities.Handler
{
    public class EncodingDelegateHandler : DelegatingHandler
    {
        private Stopwatch _st;
        public EncodingDelegateHandler()
        {
            _st = new Stopwatch();
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                if (response?.Headers!=null&&!response.Headers.Contains("ResultCompressTime"))
                {
                    response.Headers.Add("ResultCompressTime","0");
                }
                return response;
            }
            _st.Start();
            if (response.Content != null && response.RequestMessage.Headers.AcceptEncoding != null &&
                response.RequestMessage.Headers.AcceptEncoding.Count > 0)
            {
                string encodingType = response.RequestMessage.Headers.AcceptEncoding.First().Value;
                if (encodingType.ToLower().Contains("identity")) return response;
                response.Content = new CompressedContent(response.Content, encodingType);
                if (response?.Headers != null && !response.Headers.Contains("ResultCompressTime"))
                {
                    response.Headers.Add("ResultCompressTime", _st.ElapsedMilliseconds.ToString());
                }
                if (response?.Content != null && !response.Content.Headers.Contains("ResultCompressTime"))
                {
                    response.Content.Headers.Add("ResultCompressTime", _st.ElapsedMilliseconds.ToString());
                }
                _st.Stop();
            }

            return response;
        }
        //protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        //{
        //    var res= await base.SendAsync(request, cancellationToken).ContinueWith<HttpResponseMessage>((responseToCompleteTask) => 
        //    {

        //        HttpResponseMessage response = responseToCompleteTask.Result;
        //        if (!response.IsSuccessStatusCode) return response;
        //        if (response.Content != null&&response.RequestMessage.Headers.AcceptEncoding != null &&
        //            response.RequestMessage.Headers.AcceptEncoding.Count > 0)
        //        {
        //            string encodingType = response.RequestMessage.Headers.AcceptEncoding.First().Value;

        //            response.Content = new CompressedContent(response.Content, encodingType);
        //        }

        //        return response;
        //    },
        //    cancellationToken);
        //    return res;
        //}
    }

    public class CompressedContent : HttpContent
    {
        private readonly HttpContent _originalContent;
        private readonly string _encodingType;

        public CompressedContent(HttpContent content, string encodingType)
        {
            if (content == null)
            {
                throw new ArgumentNullException("content");
            }

            if (encodingType == null)
            {
                throw new ArgumentNullException("encodingType");
            }

            _originalContent = content;
            this._encodingType = encodingType.ToLowerInvariant();

            if (this._encodingType != "gzip" && this._encodingType != "deflate")
            {
                throw new InvalidOperationException(string.Format("Encoding '{0}' is not supported. Only supports gzip or deflate encoding.", this._encodingType));
            }

            // copy the headers from the original content
            foreach (KeyValuePair<string, IEnumerable<string>> header in _originalContent.Headers)
            {
                this.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            this.Headers.ContentEncoding.Add(encodingType);
        }

        protected override bool TryComputeLength(out long length)
        {
            length = -1;

            return false;
        }

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            Stream compressedStream = null;

            if (_encodingType.ToLower() == "gzip")
            {
                compressedStream = new GZipStream(stream, CompressionLevel.Fastest, leaveOpen: true);
            }
            else if (_encodingType.ToLower() == "deflate")
            {
                compressedStream = new DeflateStream(stream, CompressionLevel.Fastest, leaveOpen: true);
            }

            return _originalContent.CopyToAsync(compressedStream).ContinueWith(tsk =>
            {
                compressedStream?.Dispose();
            });
        }
    }
}
