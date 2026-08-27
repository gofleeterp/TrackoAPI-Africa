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
    public class VehicleCardMappingsController : ODataController
    //ODataController
    {
        private readonly IVehicleCardMappingService _objVehicleCardMappingService;
        private readonly IUnitOfWorkAsync _uow;

        public VehicleCardMappingsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleCardMappingService service)
        {
            _objVehicleCardMappingService = service;
            _uow = unitOfWorkAsync;
        }
        // GET: odata/VehicleCardMappings
        [HttpGet, EnableQuery]
        public IQueryable<VehicleCardMapping> Get()
        {
            return _objVehicleCardMappingService.Queryable();
        }
        // GET: odata/VehicleCardMappings(5)
        [EnableQuery]
        public SingleResult<VehicleCardMapping> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleCardMappingService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleCardMappings(5)
        public async Task<IHttpActionResult> Put(long key, VehicleCardMapping objVehicleCardMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleCardMapping.Id)
            {
                return BadRequest();
            }
            objVehicleCardMapping.ObjectState = ObjectState.Modified;
            _objVehicleCardMappingService.Update(objVehicleCardMapping);
            await _uow.SaveChangesAsync();
            return Updated(objVehicleCardMapping);
        }
        // POST: odata/VehicleCardMappings
        public async Task<IHttpActionResult> Post(VehicleCardMapping objVehicleCardMapping)
        {
            objVehicleCardMapping.ObjectState = ObjectState.Added;
            _objVehicleCardMappingService.Insert(objVehicleCardMapping);
            await _uow.SaveChangesAsync();
            return Created(objVehicleCardMapping);
        }
        //// PATCH: odata/VehicleCardMappings(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleCardMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleCardMapping objVehicleCardMapping = await _objVehicleCardMappingService.FindAsync(key);
            if (objVehicleCardMapping == null)
            {
                return NotFound();
            }
            objVehicleCardMapping.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleCardMapping);
            await _uow.SaveChangesAsync();
            return Updated(objVehicleCardMapping);
        }
        // DELETE: odata/VehicleCardMappings(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleCardMapping = await _objVehicleCardMappingService.FindAsync(key);
            if (objVehicleCardMapping == null)
            {
                return NotFound();
            }
            objVehicleCardMapping.ObjectState = ObjectState.Deleted;
            _objVehicleCardMappingService.Delete(objVehicleCardMapping);
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