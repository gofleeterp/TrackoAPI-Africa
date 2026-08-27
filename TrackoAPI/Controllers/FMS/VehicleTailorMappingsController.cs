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
    public class VehicleTailorMappingsController : ODataController
    //ODataController
    {
        private readonly IVehicleTailorMappingService _objVehicleTailorMappingService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public VehicleTailorMappingsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleTailorMappingService service)
        {
            _objVehicleTailorMappingService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/VehicleTailorMappings
        [HttpGet, EnableQuery]
        public IQueryable<VehicleTailorMapping> Get()
        {
            return _objVehicleTailorMappingService.Queryable();
        }
        // GET: odata/VehicleTailorMappings(5)
        [EnableQuery]
        public SingleResult<VehicleTailorMapping> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleTailorMappingService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleTailorMappings(5)
        public async Task<IHttpActionResult> Put(long key, VehicleTailorMapping objVehicleTailorMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleTailorMapping.Id)
            {
                return BadRequest();
            }
            objVehicleTailorMapping.ObjectState = ObjectState.Modified;
            _objVehicleTailorMappingService.Update(objVehicleTailorMapping);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objVehicleTailorMapping);
        }
        // POST: odata/VehicleTailorMappings
        public async Task<IHttpActionResult> Post(VehicleTailorMapping objVehicleTailorMapping)
        {
            objVehicleTailorMapping.ObjectState = ObjectState.Added;
            _objVehicleTailorMappingService.Insert(objVehicleTailorMapping);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objVehicleTailorMapping);
        }
        //// PATCH: odata/VehicleTailorMappings(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleTailorMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleTailorMapping objVehicleTailorMapping = await _objVehicleTailorMappingService.FindAsync(key);
            if (objVehicleTailorMapping == null)
            {
                return NotFound();
            }
            objVehicleTailorMapping.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleTailorMapping);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objVehicleTailorMapping);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleTailorMapping = await _objVehicleTailorMappingService.FindAsync(key);
            if (objVehicleTailorMapping == null)
            {
                return NotFound();
            }
            objVehicleTailorMapping.ObjectState = ObjectState.Deleted;
            _objVehicleTailorMappingService.Delete(objVehicleTailorMapping);
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