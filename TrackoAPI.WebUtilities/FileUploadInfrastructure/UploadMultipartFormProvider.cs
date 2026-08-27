using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using TrackoApi.Service;

namespace TrackoAPI.WebUtilities.FileUploadInfrastructure
{
    public class UploadMultipartFormProvider : MultipartFormDataStreamProvider
    {
        private readonly IDocumetsService _service;

        public UploadMultipartFormProvider(string rootPath, IDocumetsService service) : base(rootPath)
        {
            _service = service;
        }

        public override string GetLocalFileName(HttpContentHeaders headers)
        {
            if (headers?.ContentDisposition != null)
            {
                var filename = headers.ContentDisposition.FileName.TrimEnd('"').TrimStart('"').Split('.');
                long id;
                if (long.TryParse(filename[0], out id) && id > 0)
                {
                    var obj = _service.NewFileName(id).Split('_');
                    try
                    {
                        return $"\\{obj[0]}\\{obj[1]}\\{filename[0]}.{filename[1]}";
                    }
                    catch (Exception ex)
                    {
                        //ex.ToExceptionless().AddObject(obj).AddObject(headers.ContentDisposition).Submit();
                        throw ex;
                    }
                }
            }
            return base.GetLocalFileName(headers);
        }

        /// <summary>
        /// Gets the stream.
        /// </summary>
        /// <param name="parent">The parent.</param>
        /// <param name="headers">The headers.</param>
        /// <returns>Stream.</returns>
        public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
        {
            if (!MultipartFormDataStreamProviderHelper.IsFileContent(parent, headers))
            {
                return (Stream)new MemoryStream();
            }
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (headers == null)
                throw new ArgumentNullException("headers");
            string str;
            string file = string.Empty;
            try
            {
                file = this.GetLocalFileName(headers);
                str = Path.Combine(this.RootPath + Path.GetDirectoryName(file), Path.GetFileName(file));
                var dir = new DirectoryInfo(Path.GetDirectoryName(str));
                if (!dir.Exists)
                {
                    dir.Create();
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Invalid Path {this.RootPath} and {Path.GetFileName(file)}", ex);
            }
            this.FileData.Add(new MultipartFileData(headers, str));
            return (Stream)File.Create(str, this.BufferSize, FileOptions.Asynchronous);
            //return base.GetStream(parent, headers);
        }
    }

    internal static class MultipartFormDataStreamProviderHelper
    {
        public static bool IsFileContent(HttpContent parent, HttpContentHeaders headers)
        {
            if (parent == null)
                throw new ArgumentNullException("parent");
            if (headers == null)
                throw new ArgumentNullException("headers");
            ContentDispositionHeaderValue contentDisposition = headers.ContentDisposition;
            if (contentDisposition == null)
                throw new InvalidOperationException("Content-Disposition was null");
            //throw Error.InvalidOperation(System.Resources.MultipartFormDataStreamProviderNoContentDisposition, (object)"Content-Disposition");
            return !string.IsNullOrEmpty(contentDisposition.FileName);
        }

        public static async Task ReadFormDataAsync(Collection<HttpContent> contents, NameValueCollection formData, CancellationToken cancellationToken)
        {
            foreach (HttpContent httpContent in contents)
            {
                ContentDispositionHeaderValue contentDisposition = httpContent.Headers.ContentDisposition;
                if (string.IsNullOrEmpty(contentDisposition.FileName))
                {
                    string formFieldName = UnquoteToken(contentDisposition.Name) ?? string.Empty;
                    cancellationToken.ThrowIfCancellationRequested();
                    string formFieldValue = await httpContent.ReadAsStringAsync();
                    formData.Add(formFieldName, formFieldValue);
                }
            }
        }

        public static string UnquoteToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token) || !token.StartsWith("\"", StringComparison.Ordinal) || (!token.EndsWith("\"", StringComparison.Ordinal) || token.Length <= 1))
                return token;
            return token.Substring(1, token.Length - 2);
        }
    }
}