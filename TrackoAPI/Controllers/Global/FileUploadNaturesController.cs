using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    public class FileUploadNaturesController : ODataController
    //ODataController
    {
        private readonly IFileUploadNatureService _service;

        public FileUploadNaturesController(IFileUploadNatureService service)
        {
            _service = service;
        }
        // GET: odata/FileUploadNatures
        [HttpGet, EnableQuery]
        public IQueryable<FileUploadNature> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/FileUploadNatures(5)
        [EnableQuery]
        public SingleResult<FileUploadNature> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/FileUploadNatures(5)
        public async Task<IHttpActionResult> Put(long key, FileUploadNature objFileUploadNature)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objFileUploadNature.Id)
            {
                return BadRequest();
            }
            objFileUploadNature.ObjectState = ObjectState.Modified;
            _service.Update(objFileUploadNature);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FileUploadNatureExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objFileUploadNature);
        }
        // POST: odata/FileUploadNatures
        public async Task<IHttpActionResult> Post(FileUploadNature objFileUploadNature)
        {
            objFileUploadNature.ObjectState = ObjectState.Added;
            _service.Insert(objFileUploadNature);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (FileUploadNatureExists(objFileUploadNature.Name))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objFileUploadNature);
        }
        //// PATCH: odata/FileUploadNatures(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<FileUploadNature> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            FileUploadNature objFileUploadNature = await _service.FindAsync(key);
            if (objFileUploadNature == null)
            {
                return NotFound();
            }
            objFileUploadNature.ObjectState = ObjectState.Modified;
            patch.Patch(objFileUploadNature);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FileUploadNatureExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objFileUploadNature);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objFileUploadNature = await _service.FindAsync(key);
            if (objFileUploadNature == null)
            {
                return NotFound();
            }
            objFileUploadNature.ObjectState = ObjectState.Deleted;
            _service.Delete(objFileUploadNature);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                    Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool FileUploadNatureExists(string fileName)
        {
            return _service.Query(e => e.Name == fileName).Select().Any();
        }
        private bool FileUploadNatureExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
    }
}