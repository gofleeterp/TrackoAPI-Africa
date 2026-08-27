using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DriverNextStatusMappingsController : ODataController
    //ODataController
    {
        private readonly IDriverNextStatusMappingService _service;

        public DriverNextStatusMappingsController(IDriverNextStatusMappingService service)
        {
            _service = service;
        }
        // GET: odata/DriverNextStatusMappings
        [HttpGet, EnableQuery]
        public IQueryable<DriverNextStatusMapping> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/DriverNextStatusMappings(5)
        [EnableQuery]
        public SingleResult<DriverNextStatusMapping> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        [EnableQuery,HttpGet,ODataRoute("GetNextStatusByDriverId(driverId={driverId},vehicleId={vehicleId})")]
        public IQueryable<DriverNextStatusMapping> GetAllNextStatus([FromODataUri] long driverId, [FromODataUri] long vehicleId, ODataQueryOptions<DriverNextStatusMapping> query)
        {
            var uow = Request.GetContext();
            var repo = uow
                .Repository<DriverVehicleMapping>();
            var currentStatus = repo
                    .Queryable().OrderByDescending(x=>x.StatusDate)
                    .Where(x => x.DriverId == driverId &&x.VehicleId == vehicleId)
                    .Select(x => x.DriverStatusId)
                    .FirstOrDefault();
            if (currentStatus > 0)
            {
                currentStatus = repo
                    .Queryable().OrderByDescending(x => x.StatusDate)
                    .Where(x => x.DriverId == driverId)
                    .Select(x => x.DriverStatusId)
                    .FirstOrDefault();
                currentStatus = currentStatus == 0 ? repo
                        .Queryable().OrderByDescending(x => x.StatusDate)
                        .Where(x => x.VehicleId == vehicleId)
                        .Select(x => x.DriverStatusId)
                        .FirstOrDefault() : currentStatus;
            }
            
            if (currentStatus == 0) currentStatus = 1343;
            var data= _service.Queryable().Where(x=>x.CurrentStatusId==currentStatus);
            query.ApplyTo(data);
            return data;
        }
        // PUT: odata/DriverNextStatusMappings(5)
        public async Task<IHttpActionResult> Put(long key, DriverNextStatusMapping enitity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != enitity.Id)
            {
                return BadRequest();
            }
            if (_service.Queryable().Any(x => x.CurrentStatusId>0 && x.NextStatusId>0 && x.Id == key))
            {
                throw new BusinessException(ErrorCode.GLB106, "Only Current Transaction can be updated");
            }
            enitity.ObjectState = ObjectState.Modified;
            _service.Update(enitity);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DriverNextMapingExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(enitity);
        }
        // POST: odata/DriverNextStatusMappings
        public async Task<IHttpActionResult> Post(DriverNextStatusMapping enitity)
        {
            enitity.ObjectState = ObjectState.Added;
            _service.Insert(enitity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DriverNextMapingExists(enitity))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(enitity);
        }
        //// PATCH: odata/DriverVehicleMappings(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DriverNextStatusMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DriverNextStatusMapping objDriverNextStatusMapping = await _service.FindAsync(key);

            if (objDriverNextStatusMapping == null)
            {
                return NotFound();
            }
            if (objDriverNextStatusMapping.NextStatusId>0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Only Current Transaction can be updated");
            }
            objDriverNextStatusMapping.ObjectState = ObjectState.Modified;
            patch.Patch(objDriverNextStatusMapping);
            try
            {
                _service.Update(objDriverNextStatusMapping);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DriverNextMapingExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDriverNextStatusMapping);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var enitity = await _service.FindAsync(key);
            if (enitity == null)
            {
                return NotFound();
            }
            if (enitity.NextStatusId>0)
            {
                throw new BusinessException(ErrorCode.GLB106,"Only Current Status can be deleted.");
            }
            enitity.ObjectState = ObjectState.Deleted;
            _service.Delete(enitity);
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
        private bool DriverNextMapingExists(DriverNextStatusMapping map)
        {
            return _service.Query(e => e.NextStatusId == map.NextStatusId && e.CurrentStatusId==map.CurrentStatusId).Select().Any();
        }
        private bool DriverNextMapingExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
    }
}