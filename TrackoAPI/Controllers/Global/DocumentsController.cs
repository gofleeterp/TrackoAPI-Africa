using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DocumentsController : ODataController
    //ODataController
    {
        private readonly IDocumetsService _service;

        public DocumentsController(IDocumetsService service)
        {
            _service = service;
        }
        // GET: odata/Documents
        [HttpGet, EnableQuery]
        public IQueryable<ApiFile> Get()
        {
            return _service.Queryable();
        }

        [HttpGet]
        public async Task<HttpResponseMessage> GetStream([FromODataUri] long key)
        {
            var filePath=_service.Queryable().Where(x=>x.Id==key).Select(x=>x.ServerFilePath).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);
            }
            //string path = System.Web.HttpContext.Current.Server.MapPath("~/" + filePath);
            var fi=new FileInfo(filePath);
            using (FileStream mem = new FileStream(filePath, FileMode.Open))
            {
                StreamContent sc = new StreamContent(mem);
                HttpResponseMessage response = new HttpResponseMessage {Content = sc};
                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeTypeMap.GetMimeType(fi.Extension));
                response.Content.Headers.ContentLength = mem.Length;
                response.StatusCode = HttpStatusCode.OK;
                return await Task.FromResult(response);
            }
        }
        [HttpGet]
        public bool GetIsUploadCompleted([FromODataUri] long key)
        {
            return _service.Queryable().Where(x => x.Id == key).Select(x => x.IsUploadCompleted).FirstOrDefault();
        }
        [HttpGet]
        public IQueryable<string> GetImageUrls([FromODataUri] long recordid, [FromODataUri] long typeid)
        {
            return _service.Queryable().Where(x => x.RecordId == recordid&&x.RelatedId== typeid).Select(x => x.ImageUrl);
        }
        // GET: odata/Documents(5)
        [EnableQuery]
        public SingleResult<ApiFile> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/Documents(5)
        public async Task<IHttpActionResult> Put(long key, ApiFile objApiFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objApiFile.Id)
            {
                return BadRequest();
            }
            objApiFile.ObjectState = ObjectState.Modified;
            _service.Update(objApiFile);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApiFileExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objApiFile);
        }
        // POST: odata/Documents
        public async Task<IHttpActionResult> Post(ApiFile objApiFile)
        {
            objApiFile.ObjectState = ObjectState.Added;
            objApiFile.IsUploadCompleted = false;
            objApiFile.ServerFilePath = "";
            _service.Insert(objApiFile);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ApiFileExists(objApiFile.Name))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objApiFile);
        }
        //// PATCH: odata/Documents(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ApiFile> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApiFile objApiFile = await _service.FindAsync(key);
            if (objApiFile == null)
            {
                return NotFound();
            }
            objApiFile.ObjectState = ObjectState.Modified;
            patch.Patch(objApiFile);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApiFileExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objApiFile);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objApiFile = await _service.FindAsync(key);
            if (objApiFile == null)
            {
                return NotFound();
            }
            objApiFile.ObjectState = ObjectState.Deleted;
            _service.Delete(objApiFile);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if(!Request.IsBatchRequest())
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ApiFileExists(string fileName)
        {
            return _service.Query(e => e.Name == fileName).Select().Any();
        }
        private bool ApiFileExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
    }
}