using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace TrackoAPI.WebUtilities.FileUploadInfrastructure
{
    public class FileActionResult : IHttpActionResult
    {

        public FileActionResult(string filePath)
        {
            if (filePath.StartsWith("~/"))
            {
                this.RelativePath = filePath;
            }
            else
            {
                this.RelativePath = "~/" + filePath;
            }
        }

        public string RelativePath { get; private set; }
        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            string path = System.Web.HttpContext.Current.Server.MapPath(RelativePath);
            HttpResponseMessage response = new HttpResponseMessage
            {
                Content = new StreamContent(File.OpenRead(path))
            };
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");

            // NOTE: Here I am just setting the result on the Task and not really doing any async stuff. 
            // But let's say you do stuff like contacting a File hosting service to get the file, then you would do 'async' stuff here.

            return Task.FromResult(response);
        }
    }
}