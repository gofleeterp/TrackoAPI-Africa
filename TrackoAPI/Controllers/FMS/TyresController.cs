using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TyresController : ODataController
    //ODataController
    {
        private readonly ITyreMasterService _objTyreMasterService;

        public TyresController(ITyreMasterService service)
        {
            _objTyreMasterService = service;
        }
        // GET: odata/TyreMasters
        [HttpGet, EnableQuery]
        public IQueryable<TyreMaster> Get()
        {
            return _objTyreMasterService.Queryable();
        }
        // GET: odata/TyreMasters(5)
        [EnableQuery]
        public SingleResult<TyreMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objTyreMasterService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/TyreMasters(5)
        public async Task<IHttpActionResult> Put(long key, TyreMaster objTyreMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTyreMaster.Id)
            {
                return BadRequest();
            }
            objTyreMaster.ObjectState = ObjectState.Modified;
            _objTyreMasterService.Update(objTyreMaster);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TyreMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTyreMaster);
        }
        // POST: odata/TyreMasters
        public async Task<IHttpActionResult> Post(TyreMaster objTyreMaster)
        {
            objTyreMaster.ObjectState = ObjectState.Added;
            _objTyreMasterService.Insert(objTyreMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TyreMasterExists(objTyreMaster.TyreNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objTyreMaster);
        }
        //// PATCH: odata/TyreMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TyreMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TyreMaster objTyreMaster = await _objTyreMasterService.FindAsync(key);
            if (objTyreMaster == null)
            {
                return NotFound();
            }
            objTyreMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objTyreMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TyreMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTyreMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTyreMaster = await _objTyreMasterService.FindAsync(key);
            if (objTyreMaster == null)
            {
                return NotFound();
            }
            objTyreMaster.ObjectState = ObjectState.Deleted;
            _objTyreMasterService.Delete(objTyreMaster);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
                
            }
            base.Dispose(disposing);
        }

        private bool TyreMasterExists(string tyreNo)
        {
            return _objTyreMasterService.Query(e => e.TyreNo == tyreNo).Select().Any();
        }
        private bool TyreMasterExists(long key)
        {
            return _objTyreMasterService.Query(e => e.Id == key).Select().Any();
        }
    }
}