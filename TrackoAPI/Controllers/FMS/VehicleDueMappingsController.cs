using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleDueMappingsController : ODataController
    //ODataController
    {
        private readonly IVehicleDueMappingService _objVehicleDueMappingService;

        public VehicleDueMappingsController(IVehicleDueMappingService service)
        {
            _objVehicleDueMappingService = service;
        }
        // GET: odata/VehicleDueMappings
        [HttpGet, EnableQuery]
        public IQueryable<VehicleDueMapping> Get()
        {
            return _objVehicleDueMappingService.Queryable();
        }
        // GET: odata/VehicleDueMappings(5)
        [EnableQuery]
        public SingleResult<VehicleDueMapping> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleDueMappingService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleDueMappings(5)
        public async Task<IHttpActionResult> Put(long key, VehicleDueMapping objVehicleDueMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleDueMapping.Id)
            {
                return BadRequest();
            }
            objVehicleDueMapping.ObjectState = ObjectState.Modified;
            _objVehicleDueMappingService.Update(objVehicleDueMapping);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objVehicleDueMapping);
        }
        // POST: odata/VehicleDueMappings
        public async Task<IHttpActionResult> Post(VehicleDueMapping objVehicleDueMapping)
        {
            objVehicleDueMapping.ObjectState = ObjectState.Added;
            _objVehicleDueMappingService.Insert(objVehicleDueMapping);
            await Request.GetContext().SaveChangesAsync();
            return Created(objVehicleDueMapping);
        }
        //// PATCH: odata/VehicleDueMappings(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleDueMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleDueMapping objVehicleDueMapping = await _objVehicleDueMappingService.FindAsync(key);
            if (objVehicleDueMapping == null)
            {
                return NotFound();
            }
            objVehicleDueMapping.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleDueMapping);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objVehicleDueMapping);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleDueMapping = await _objVehicleDueMappingService.FindAsync(key);
            if (objVehicleDueMapping == null)
            {
                return NotFound();
            }
            objVehicleDueMapping.ObjectState = ObjectState.Deleted;
            _objVehicleDueMappingService.Delete(objVehicleDueMapping);
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
    }
}