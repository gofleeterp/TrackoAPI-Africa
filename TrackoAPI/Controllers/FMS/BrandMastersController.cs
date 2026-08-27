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
    public class BrandMastersController : ODataController
    //ODataController
    {
        private readonly IBrandMasterService _objBrandMasterService;

        public BrandMastersController(IBrandMasterService service)
        {
            _objBrandMasterService = service;
        }
        // GET: odata/BrandMasters
        [HttpGet, EnableQuery]
        public IQueryable<BrandMaster> Get() => _objBrandMasterService.Queryable();

        // GET: odata/BrandMasters(5)
        [EnableQuery]
        public SingleResult<BrandMaster> Get([FromODataUri] long key) => SingleResult.Create(_objBrandMasterService.Queryable().Where(t => t.Id == key));
        // PUT: odata/BrandMasters(5)
        public async Task<IHttpActionResult> Put(long key, BrandMaster objBrandMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objBrandMaster.Id)
            {
                return BadRequest();
            }
            objBrandMaster.ObjectState = ObjectState.Modified;
            _objBrandMasterService.Update(objBrandMaster);

            try
            {
              await Request.GetContext().SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrandMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objBrandMaster);
        }
        // POST: odata/BrandMasters
        public async Task<IHttpActionResult> Post(BrandMaster objBrandMaster)
        {
            objBrandMaster.ObjectState = ObjectState.Added;
            _objBrandMasterService.Insert(objBrandMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BrandMasterExists(objBrandMaster.BrandName))
                {
                    throw new BusinessException(ErrorCode.GLB104,"Record Already Exists");
                    //return Conflict();
                }
                throw;
            }
            return Created(objBrandMaster);
        }
        //// PATCH: odata/BrandMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<BrandMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BrandMaster objBrandMaster = await _objBrandMasterService.FindAsync(key);
            if (objBrandMaster == null)
            {
                return NotFound();
            }
            objBrandMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objBrandMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BrandMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objBrandMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objBrandMaster = await _objBrandMasterService.FindAsync(key);
            if (objBrandMaster == null)
            {
                return NotFound();
            }
            objBrandMaster.ObjectState = ObjectState.Deleted;
            _objBrandMasterService.Delete(objBrandMaster);
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

        private bool BrandMasterExists(string brandName) => _objBrandMasterService.Query(e => e.BrandName == brandName).Select().Any();
        private bool BrandMasterExists(long key) => _objBrandMasterService.Query(e => e.Id == key).Select().Any();
    }
}