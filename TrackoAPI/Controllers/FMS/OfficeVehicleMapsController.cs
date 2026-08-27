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
using TrackoApi.Service.FMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.FMS
{
    [AuthorizeEx]
    public class OfficeVehicleMapsController : ODataController
    //ODataController
    {
        private readonly IOfficeVehicleMapService _service;

        public OfficeVehicleMapsController(IOfficeVehicleMapService service)
        {
            _service = service;
        }
        // GET: odata/OfficeVehicleMaps
        [HttpGet, EnableQuery]
        public IQueryable<OfficeVehicleMap> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/OfficeVehicleMaps(5)
        [EnableQuery]
        public SingleResult<OfficeVehicleMap> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/OfficeVehicleMaps(5)
        public async Task<IHttpActionResult> Put(long key, OfficeVehicleMap objOfficeVehicleMap)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objOfficeVehicleMap.Id)
            {
                return BadRequest();
            }
            objOfficeVehicleMap.ObjectState = ObjectState.Modified;
            _service.Update(objOfficeVehicleMap);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OfficeVehicleMapingExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objOfficeVehicleMap);
        }
        // POST: odata/DriverVehicleMappings
        public async Task<IHttpActionResult> Post(OfficeVehicleMap objOfficeVehicleMap)
        {
            objOfficeVehicleMap.ObjectState = ObjectState.Added;
            _service.Insert(objOfficeVehicleMap);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (OfficeVehicleMapingExists(objOfficeVehicleMap))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objOfficeVehicleMap);
        }
        //// PATCH: odata/OfficeVehicleMaps(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<OfficeVehicleMap> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            OfficeVehicleMap objDriverMaster = await _service.FindAsync(key);

            if (objDriverMaster == null)
            {
                return NotFound();
            }
            //if (objDriverMaster.NextLogId.HasValue)
            //{
            //    throw new BusinessException(ErrorCode.GLB106, "Only Current Status can be updated.");
            //}
            objDriverMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objDriverMaster);
            try
            {
                _service.Update(objDriverMaster);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OfficeVehicleMapingExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDriverMaster);
        }
        // DELETE: odata/OfficeVehicleMaps(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDriverMaster = await _service.FindAsync(key);
            if (objDriverMaster == null)
            {
                return NotFound();
            }
            //if (objDriverMaster.NextLogId.HasValue)
            //{
            //    throw new BusinessException(ErrorCode.GLB106,"Only Current Status can be deleted.");
            //}
            objDriverMaster.ObjectState = ObjectState.Deleted;
            _service.Delete(objDriverMaster);
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
        private bool OfficeVehicleMapingExists(OfficeVehicleMap map)
        {
            return _service.Query(e => e.OfficeId == map.OfficeId && e.VehicleId==map.VehicleId).Select().Any();
        }
        private bool OfficeVehicleMapingExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
    }
}