using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleMovementLogPickupDropsController : ODataController
    //ODataController
    {
        private readonly IVehicleMovementLogPickupDropService _objVehicleMovementLogPickupDropService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly IVehicleMovementLogService _tlrepo;
        private readonly bool IsNewGPSBatchTripUploadEnabled = false;
        public VehicleMovementLogPickupDropsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleMovementLogPickupDropService service,IVehicleMovementLogService tlservice)
        {
            _objVehicleMovementLogPickupDropService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
            _tlrepo = tlservice;
            IsNewGPSBatchTripUploadEnabled = _tlrepo.GetConfigValue<int>("IsNewGPSBatchTripUploadEnabled") == 1;
        }
        // GET: odata/VehicleMovementLogPickupDrops
        [HttpGet, EnableQuery]
        public IQueryable<VehicleMovementLogPickupDrop> Get()
        {
            return _objVehicleMovementLogPickupDropService.Queryable();
        }
        // GET: odata/VehicleMovementLogPickupDrops(5)
        [EnableQuery]
        public SingleResult<VehicleMovementLogPickupDrop> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleMovementLogPickupDropService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleMovementLogPickupDrops(5)
        public async Task<IHttpActionResult> Put(long key, VehicleMovementLogPickupDrop point)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != point.Id)
            {
                return BadRequest();
            }
            point.ObjectState = ObjectState.Modified;
            _objVehicleMovementLogPickupDropService.Update(point);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(point);
        }
        // POST: odata/VehicleMovementLogPickupDrops
        public async Task<IHttpActionResult> Post(VehicleMovementLogPickupDrop point)
        {
            point.ObjectState = ObjectState.Added;
            _objVehicleMovementLogPickupDropService.Insert(point);
            await _unitOfWorkAsync.SaveChangesAsync();
            if (!IsNewGPSBatchTripUploadEnabled)
            {
                await _tlrepo.PushToGpsProviderAsync(point);
            }
            else
            {
                await _tlrepo.ScheduleTripPushToGPSAsync(point.TriplogId, point.RouteId);
            }
            return Created(point);
        }
        //// PATCH: odata/VehicleMovementLogPickupDrops(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleMovementLogPickupDrop> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleMovementLogPickupDrop point = await _objVehicleMovementLogPickupDropService.FindAsync(key);
            if (point == null)
            {
                return NotFound();
            }
            point.ObjectState = ObjectState.Modified;
            patch.Patch(point);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(point);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var point = await _objVehicleMovementLogPickupDropService.FindAsync(key);
            if (point == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            point.ObjectState = ObjectState.Deleted;
            _objVehicleMovementLogPickupDropService.Delete(point);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}