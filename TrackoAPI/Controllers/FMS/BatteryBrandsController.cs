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
    public class BatteryBrandsController : ODataController
    //ODataController
    {
        private readonly IBatteryBrandService _objBatteryBrandService;

        public BatteryBrandsController(IBatteryBrandService service)
        {
            _objBatteryBrandService = service;
        }
        // GET: odata/BatteryBrands
        [HttpGet, EnableQuery]
        public IQueryable<BatteryBrand> Get() => _objBatteryBrandService.Queryable();

        // GET: odata/BatteryBrands(5)
        [EnableQuery]
        public SingleResult<BatteryBrand> Get([FromODataUri] long key) => SingleResult.Create(_objBatteryBrandService.Queryable().Where(t => t.Id == key));
        // PUT: odata/BatteryBrands(5)
        public async Task<IHttpActionResult> Put(long key, BatteryBrand objBatteryBrand)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objBatteryBrand.Id)
            {
                return BadRequest();
            }
            objBatteryBrand.ObjectState = ObjectState.Modified;
            _objBatteryBrandService.Update(objBatteryBrand);

            try
            {
               await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BatteryBrandExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objBatteryBrand);
        }
        // POST: odata/BatteryBrands
        public async Task<IHttpActionResult> Post(BatteryBrand objBatteryBrand)
        {
            objBatteryBrand.ObjectState = ObjectState.Added;
            _objBatteryBrandService.Insert(objBatteryBrand);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BatteryBrandExists(objBatteryBrand.BrandName))
                {
                    throw new BusinessException(ErrorCode.GLB104,"Record Already Exists");
                    //return Conflict();
                }
                throw;
            }
            return Created(objBatteryBrand);
        }
        //// PATCH: odata/BatteryBrands(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<BatteryBrand> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BatteryBrand objBatteryBrand = await _objBatteryBrandService.FindAsync(key);
            if (objBatteryBrand == null)
            {
                return NotFound();
            }
            objBatteryBrand.ObjectState = ObjectState.Modified;
            patch.Patch(objBatteryBrand);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BatteryBrandExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objBatteryBrand);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objBatteryBrand = await _objBatteryBrandService.FindAsync(key);
            if (objBatteryBrand == null)
            {
                return NotFound();
            }
            objBatteryBrand.ObjectState = ObjectState.Deleted;
            _objBatteryBrandService.Delete(objBatteryBrand);
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

        private bool BatteryBrandExists(string brandName) => _objBatteryBrandService.Query(e => e.BrandName == brandName).Select().Any();
        private bool BatteryBrandExists(long key) => _objBatteryBrandService.Query(e => e.Id == key).Select().Any();
    }
}