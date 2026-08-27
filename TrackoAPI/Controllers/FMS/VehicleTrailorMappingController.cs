using Repository.Pattern.Core.UnitOfWork;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TrailorMappingsController : ODataController
    //ODataController
    {
        private readonly IVehicleTrailorMappingService _objVehicleTrailorMappingService;
        private readonly IUnitOfWorkAsync _uow;

        public TrailorMappingsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleTrailorMappingService service)
        {
            _objVehicleTrailorMappingService = service;
            _uow = unitOfWorkAsync;
        }
        // GET: odata/VehicleTrailorMapping
        [HttpGet, EnableQuery]
        public IQueryable<VehicleTrailorMapping> Get()
        {
            return _objVehicleTrailorMappingService.Queryable();
        }
        // GET: odata/VehicleTrailorMapping(5)
        [EnableQuery]
        public SingleResult<VehicleTrailorMapping> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleTrailorMappingService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleTrailorMapping(5)
        public async Task<IHttpActionResult> Put(long key, VehicleTrailorMapping objVehicleTrailorMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleTrailorMapping.Id)
            {
                return BadRequest();
            }
            objVehicleTrailorMapping.ObjectState = ObjectState.Modified;
            _objVehicleTrailorMappingService.Update(objVehicleTrailorMapping);
            await _uow.SaveChangesAsync();
            return Updated(objVehicleTrailorMapping);
        }
        // POST: odata/VehicleTrailorMapping
        public async Task<IHttpActionResult> Post(VehicleTrailorMapping objVehicleTrailorMapping)
        {
            objVehicleTrailorMapping.ObjectState = ObjectState.Added;
            _objVehicleTrailorMappingService.Insert(objVehicleTrailorMapping);
            await _uow.SaveChangesAsync();
            return Created(objVehicleTrailorMapping);
        }
        //// PATCH: odata/VehicleTrailorMappings(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleTrailorMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleTrailorMapping objVehicleTrailorMapping = await _objVehicleTrailorMappingService.FindAsync(key);
            if (objVehicleTrailorMapping == null)
            {
                return NotFound();
            }
            objVehicleTrailorMapping.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleTrailorMapping);
            await _uow.SaveChangesAsync();
            return Updated(objVehicleTrailorMapping);
        }
        // DELETE: odata/VehicleTrailorMapping(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleTrailorMapping = await _objVehicleTrailorMappingService.FindAsync(key);
            if (objVehicleTrailorMapping == null)
            {
                return NotFound();
            }
            objVehicleTrailorMapping.ObjectState = ObjectState.Deleted;
            _objVehicleTrailorMappingService.Delete(objVehicleTrailorMapping);
            await _uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Dispose();
                }
            }
            base.Dispose(disposing);
        }      

    }
}