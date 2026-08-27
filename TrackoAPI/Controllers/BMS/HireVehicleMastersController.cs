using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class HireVehiclesController : ODataController
    //ODataController
    {
        private readonly IHireVehicleService _repo;

        public HireVehiclesController(IHireVehicleService service)
        {
            _repo = service;
        }
        // GET: odata/MaterialMasters
        [HttpGet, EnableQuery]
        public IQueryable<HireVehicle> Get() => _repo.Queryable();

        // GET: odata/MaterialMasters(5)
        [EnableQuery]
        public SingleResult<HireVehicle> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/MaterialMasters(5)
        public async Task<IHttpActionResult> Put(long key, HireVehicle objHireVehicle)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objHireVehicle.Id)
            {
                return BadRequest();
            }
            objHireVehicle.ObjectState = ObjectState.Modified;
            _repo.Update(objHireVehicle);

            try
            {
             
                await Request.GetContext().SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HireVehicleExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objHireVehicle);
        }
        // POST: odata/MaterialMasters
        public async Task<IHttpActionResult> Post(HireVehicle objMaterialMaster)
        {
            objMaterialMaster.ObjectState = ObjectState.Added;
            _repo.Insert(objMaterialMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (HireVehicleExists(objMaterialMaster.VehicleNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objMaterialMaster);
        }
        //// PATCH: odata/MaterialMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<HireVehicle> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            HireVehicle objMaterialMaster = await _repo.FindAsync(key);
            if (objMaterialMaster == null)
            {
                return NotFound();
            }
            objMaterialMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objMaterialMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HireVehicleExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objMaterialMaster = await _repo.FindAsync(key);
            if (objMaterialMaster == null)
            {
                return NotFound();
            }
            objMaterialMaster.ObjectState = ObjectState.Deleted;
            _repo.Delete(objMaterialMaster);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing&&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool HireVehicleExists(string vehicleNo) => _repo.Query(e => e.VehicleNo == vehicleNo).Select().Any();
        private bool HireVehicleExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}