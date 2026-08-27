using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

using Newtonsoft.Json;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.FileUploadInfrastructure;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    /// <summary>
    /// This sample controller reads the contents of an HTML file upload asynchronously and writes one or more body parts to a local file.
    /// </summary>
    [AuthorizeEx]
    [RoutePrefix("api/FileUpload")]
    public class FileUploadController : ApiController
    {
        private readonly IDocumetsService _service;
        private readonly IHangfireJobProcessor _jobProcessor;
        const int BufferSize = 50 * 1024;
        public FileUploadController(IDocumetsService service,IHangfireJobProcessor jobProcessor)
        {
            _service = service;
            _jobProcessor = jobProcessor;
        }
        static readonly string ServerUploadFolder = Path.GetTempPath();
        
        [HttpGet,Route("GetFileAsync({fileId})")]
        public Task<HttpResponseMessage> Get(long fileId)
        {
            // NOTE: If there was any other 'async' stuff here, then you would need to return
            // a Task<IHttpActionResult>, but for this simple case you need not.
            var configuredPath = Utilities.FileUploadFolder();
            var uploadPath = HttpContext.Current.Server.MapPath(configuredPath);
            
            var tenantid = this.GetClaimByKey<string>("ClientKey");
            if (!string.IsNullOrWhiteSpace(tenantid))
            {
                uploadPath =Path.Combine(uploadPath ?? throw new InvalidOperationException(), tenantid);
            }
            var fileName = _service.Queryable().Where(x => x.IsUploadCompleted && x.Id == fileId).Select(x => x.ServerFilePath).FirstOrDefault();
            //var filepath =Path.Combine(uploadPath, fileName);
            var filepath = fileName.Contains(uploadPath ?? throw new InvalidOperationException()) ? fileName : uploadPath+(uploadPath.EndsWith("\\") ? "" : "\\") + fileName;
            if (!File.Exists(filepath))
            {
                return Task.FromResult(Request.CreateResponse(HttpStatusCode.NotFound));
            }
            // Open file and read response from it. If read fails then return 503 Service Not Available           
            try
            {
                // Create StreamContent from FileStream. FileStream will get closed when StreamContent is closed
                FileStream fStream = new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, useAsync: true);
                HttpResponseMessage response = Request.CreateResponse();
                response.Content = new StreamContent(fStream);
                return Task.FromResult(response);
            }
            catch (Exception e)
            {
                return Task.FromResult(Request.CreateErrorResponse(HttpStatusCode.ServiceUnavailable, e));
            }
        }
        [HttpGet, Route("DocumentLiksByFileId({fileId})")]
        public async Task<IHttpActionResult> GetDocumentLiksByFileId([FromUri] long fileId)
        {
            var file =await _service.Queryable().FirstOrDefaultAsync(x => x.IsUploadCompleted && x.Id == fileId);
            if (file == null||string.IsNullOrWhiteSpace(file.ImageUrl)) return NotFound();
            var basepath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + RequestContext.VirtualPathRoot + file.ImageUrl.Replace(file.Name,"");
            var files = new Dictionary<string, string>
            {
                {"original", basepath + file.Name}
            };
            if (!HasImageExtension(file.Name)) return Ok(files);
            files.Add("small", basepath + "thumbnails/small/" + file.Name);
            files.Add("smallx2", basepath + "thumbnails/smallx2/" + file.Name);
            files.Add("medium", basepath + "thumbnails/medium/" + file.Name);
            return Ok(files);
        }
        [HttpGet, Route("DocumentLiksByRecordId({typeId},{natureId},{recordId})")]
        public async Task<IHttpActionResult> DocumentLiksByRecordId([FromUri] long typeId, [FromUri] long natureId,[FromUri] long recordId)
        {
            var file = await _service.Queryable().Where(x => x.IsUploadCompleted && x.RecordId == recordId&&x.RelatedId==typeId&&x.NatureId== natureId).ToListAsync();
            if (!file.Any()) return NotFound();
            var files = new Dictionary<string, IDictionary<string,string>>();
            foreach (var apiFile in file.Where(x=>!string.IsNullOrWhiteSpace(x.ImageUrl)))
            {
                var basepath = Request.RequestUri.GetLeftPart(UriPartial.Authority) + RequestContext.VirtualPathRoot + apiFile.ImageUrl.Replace(apiFile.Name, "");
                var dic = new Dictionary<string, string>
                {
                    {"original", basepath + apiFile.Name}
                };
                if (HasImageExtension(apiFile.Name))
                {
                    dic.Add("small", basepath + "thumbnails/small/" + apiFile.Name);
                    dic.Add("smallx2", basepath + "thumbnails/smallx2/" + apiFile.Name);
                    dic.Add("medium", basepath + "thumbnails/medium/" + apiFile.Name);
                }
                files.Add(apiFile.Name,dic);
            }
            
            return Ok(files);
        }
        private void CreateThumbnail(string fileName)
        {
            try
            {
                var file = new FileInfo(fileName);
                if (!HasImageExtension(fileName) || file.Directory == null|| !file.Exists) return;
                var thumdir = Path.Combine(file.Directory.FullName, "thumbnails");
                using (Image image = Image.FromFile(fileName))
                {
                    var smalldir = Path.Combine(thumdir, "small");
                    if (Directory.Exists(smalldir))
                    {
                        Directory.CreateDirectory(smalldir);
                    }
                    using (Image small = image.GetThumbnailImage(75, 75, () => false, IntPtr.Zero))
                    {
                        small.Save(Path.ChangeExtension(Path.Combine(smalldir, file.Name), file.Extension));
                    }


                    var smallx2dir = Path.Combine(thumdir, "smallx2");
                    if (Directory.Exists(smallx2dir))
                    {
                        Directory.CreateDirectory(smallx2dir);
                    }
                    using (Image small = image.GetThumbnailImage(150, 150, () => false, IntPtr.Zero))
                    {
                        small.Save(Path.ChangeExtension(Path.Combine(smallx2dir, file.Name), file.Extension));
                    }


                    var mediumdir = Path.Combine(thumdir, "medium");
                    if (Directory.Exists(mediumdir))
                    {
                        Directory.CreateDirectory(mediumdir);
                    }
                    using (Image small = image.GetThumbnailImage(480, 320, () => false, IntPtr.Zero))
                    {
                        small.Save(Path.ChangeExtension(Path.Combine(mediumdir, file.Name), file.Extension));
                    }
                }
            }
            catch (Exception e)
            {
                //Ignore
            }
            
        }
        public async Task<FileUploadResult> Post()
        {
            if (!Request.Content.IsMimeMultipartContent())
                throw new HttpResponseException(Request.CreateResponse(HttpStatusCode.NotAcceptable,
                    "This request is not properly formatted"));

            //var configuredPath = ConfigurationManager.AppSettings["FileUploadFolderName"];
            //if (string.IsNullOrWhiteSpace(configuredPath))
            //    configuredPath = "~/Files";
            var configuredPath = Utilities.FileUploadFolder();
            var uploadPath = HttpContext.Current.Server.MapPath(configuredPath);

            var tenantid = Request.GetContext().Context.Clients.Select(x => x.ClientKey).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantid))
            {
                uploadPath += (uploadPath.EndsWith("/") ? "" : "/") + tenantid;
            }
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            var multipartFormDataStreamProvider = new UploadMultipartFormProvider(uploadPath, _service);
            await Request.Content.ReadAsMultipartAsync(multipartFormDataStreamProvider);

            string localFileName =
                multipartFormDataStreamProvider.FileData.Select(x => x.LocalFileName).FirstOrDefault();
            var apifile = JsonConvert.DeserializeObject<ApiFile>(multipartFormDataStreamProvider.FormData["apiFileObject"]);
            var obj = _service.Find(apifile.Id);
            var fileinfo = new FileInfo(localFileName);
            
            if (obj != null)
            {
                obj.ObjectState = ObjectState.Modified;
                obj.ServerFilePath = localFileName.Replace(uploadPath.Replace("/","\\"), "");
                obj.IsUploadCompleted = true;
                obj.Size = fileinfo.Length;
                obj.Name = fileinfo.Name;
                var imagerightpartpath = localFileName.Replace(HostingEnvironment.ApplicationPhysicalPath, "").Replace("\\", "/");
                if (imagerightpartpath.StartsWith("/"))
                {
                    imagerightpartpath = "/$" + imagerightpartpath.PadLeft(1);
                }
                else
                {
                    imagerightpartpath = "/$" + imagerightpartpath;
                }

                obj.UrlPath = $"{(Request.RequestUri.GetLeftPart(UriPartial.Authority) + RequestContext.VirtualPathRoot)}/{imagerightpartpath}";
                obj.ImageUrl = imagerightpartpath;
                _service.Update(obj);
                await Request.GetContext().SaveChangesAsync();
                if (HasImageExtension(localFileName))
                {
                    Hangfire.BackgroundJob.Enqueue(() => _jobProcessor.CreateThumbnail(fileinfo.FullName));
                }
                if (!string.IsNullOrWhiteSpace(localFileName) && File.Exists(localFileName))
                    return new FileUploadResult()
                    {
                        FileName = Path.GetFileName(localFileName),
                        FileLength = fileinfo.Length,
                        ServerFilePath = obj.UrlPath,
                        Id = obj.Id
                    };
               
            }
            
            return new FileUploadResult();
        }
        private static string GetNameHeaderValue(ICollection<NameValueHeaderValue> headerValues, string name)
        {
            var nameValueHeader = headerValues?.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            return nameValueHeader?.Value;
        }
        private static bool HasImageExtension(string source)
        {
            return (source.EndsWith(".png") || source.EndsWith(".jpg") || source.EndsWith(".jpeg") || source.EndsWith(".jfif") || source.EndsWith(".bmp") || source.EndsWith(".tif") || source.EndsWith(".tiff") || source.EndsWith(".gif"));
        }
    }
}
