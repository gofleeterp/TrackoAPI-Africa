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
    public class VehicleOwnerMappingsController : ODataController
    //ODataController
    {
        private readonly IVehicleOwnerMappingService _objVehicleOwnerMappingService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public VehicleOwnerMappingsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleOwnerMappingService service)
        {
            _objVehicleOwnerMappingService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/VehicleOwnerMappings
        [HttpGet, EnableQuery]
        public IQueryable<VehicleOwnerMapping> Get()
        {
            return _objVehicleOwnerMappingService.Queryable();
        }
        // GET: odata/VehicleOwnerMappings(5)
        [EnableQuery]
        public SingleResult<VehicleOwnerMapping> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleOwnerMappingService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleOwnerMappings(5)
        public async Task<IHttpActionResult> Put(long key, VehicleOwnerMapping objVehicleOwnerMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleOwnerMapping.Id)
            {
                return BadRequest();
            }
            objVehicleOwnerMapping.ObjectState = ObjectState.Modified;
            _objVehicleOwnerMappingService.Update(objVehicleOwnerMapping);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objVehicleOwnerMapping);
        }
        // POST: odata/VehicleOwnerMappings
        public async Task<IHttpActionResult> Post(VehicleOwnerMapping objVehicleOwnerMapping)
        {
            objVehicleOwnerMapping.ObjectState = ObjectState.Added;
            _objVehicleOwnerMappingService.Insert(objVehicleOwnerMapping);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objVehicleOwnerMapping);
        }
        //// PATCH: odata/VehicleOwnerMappings(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleOwnerMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleOwnerMapping objVehicleOwnerMapping = await _objVehicleOwnerMappingService.FindAsync(key);
            if (objVehicleOwnerMapping == null)
            {
                return NotFound();
            }
            objVehicleOwnerMapping.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleOwnerMapping);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objVehicleOwnerMapping);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleOwnerMapping = await _objVehicleOwnerMappingService.FindAsync(key);
            if (objVehicleOwnerMapping == null)
            {
                return NotFound();
            }
            objVehicleOwnerMapping.ObjectState = ObjectState.Deleted;
            _objVehicleOwnerMappingService.Delete(objVehicleOwnerMapping);
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