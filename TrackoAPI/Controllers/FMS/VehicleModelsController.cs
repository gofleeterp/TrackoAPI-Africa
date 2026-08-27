using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleModelsController : ODataController
    //ODataController
    {
        private readonly IVehicleModelService _objVehicleModelService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public VehicleModelsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleModelService service)
        {
            _objVehicleModelService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/VehicleModels
        [HttpGet, EnableQuery]
        public IQueryable<VehicleModel> Get()
        {
            return _objVehicleModelService.Queryable();
        }
        // GET: odata/VehicleModels(5)
        [EnableQuery]
        public SingleResult<VehicleModel> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleModelService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleModels(5)
        public async Task<IHttpActionResult> Put(long key, VehicleModel objVehicleModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleModel.Id)
            {
                return BadRequest();
            }
            objVehicleModel.ObjectState = ObjectState.Modified;
            _objVehicleModelService.Update(objVehicleModel);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleModelExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVehicleModel);
        }
        // POST: odata/VehicleModels
        public async Task<IHttpActionResult> Post(VehicleModel objVehicleModel)
        {
            objVehicleModel.ObjectState = ObjectState.Added;
            _objVehicleModelService.Insert(objVehicleModel);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VehicleModelExists(objVehicleModel.ModelName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objVehicleModel);
        }
        //// PATCH: odata/VehicleModels(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleModel> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleModel objVehicleModel = await _objVehicleModelService.FindAsync(key);
            if (objVehicleModel == null)
            {
                return NotFound();
            }
            objVehicleModel.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleModel);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleModelExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVehicleModel);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleModel = await _objVehicleModelService.FindAsync(key);
            if (objVehicleModel == null)
            {
                return NotFound();
            }
            objVehicleModel.ObjectState = ObjectState.Deleted;
            _objVehicleModelService.Delete(objVehicleModel);
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

        private bool VehicleModelExists(string modelName)
        {
            return _objVehicleModelService.Query(e => e.ModelName == modelName).Select().Any();
        }
        private bool VehicleModelExists(long key)
        {
            return _objVehicleModelService.Query(e => e.Id == key).Select().Any();
        }
    }
}