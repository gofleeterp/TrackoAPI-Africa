using System;
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
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleAccidentEstimatesController : ODataController
    //ODataController
    {
        private readonly IVehicleAccidentEstimateService _objVehicleAccidentEstimateService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public VehicleAccidentEstimatesController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleAccidentEstimateService service)
        {
            _objVehicleAccidentEstimateService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/VehicleAccidentEstimates
        [HttpGet, EnableQuery]
        public IQueryable<VehicleAccidentEstimate> Get()
        {
            return _objVehicleAccidentEstimateService.Queryable();
        }
        // GET: odata/VehicleAccidentEstimates(5)
        [EnableQuery]
        public SingleResult<VehicleAccidentEstimate> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleAccidentEstimateService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleAccidentEstimates(5)
        public async Task<IHttpActionResult> Put(long key, VehicleAccidentEstimate objVehicleAccidentEstimate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleAccidentEstimate.Id)
            {
                return BadRequest();
            }
            objVehicleAccidentEstimate.ObjectState = ObjectState.Modified;
            _objVehicleAccidentEstimateService.Update(objVehicleAccidentEstimate);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleAccidentEstimateExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVehicleAccidentEstimate);
        }
        // POST: odata/VehicleAccidentEstimates

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] long key, string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var vehicleaccidentest = await _objVehicleAccidentEstimateService.FindAsync(key);
            if (vehicleaccidentest == null)
            {
                return NotFound();
            }
            var newrecordid = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_VehicleAccidentClaim":
                    if (!uow.RepositoryAsync<VehicleAccidentClaim>().Queryable().Any(x => x.Id == newrecordid))
                    {
                        return NotFound();
                    }
                    vehicleaccidentest.AccidentClaimId = newrecordid;
                    vehicleaccidentest.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }


        public async Task<IHttpActionResult> Post(VehicleAccidentEstimate objVehicleAccidentEstimate)
        {
            objVehicleAccidentEstimate.ObjectState = ObjectState.Added;
            _objVehicleAccidentEstimateService.Insert(objVehicleAccidentEstimate);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VehicleAccidentEstimateExists(objVehicleAccidentEstimate.ItemLabourName, objVehicleAccidentEstimate.AccidentClaimId.GetValueOrDefault()))
                {
                    throw new BusinessException(ErrorCode.GLB104, "This Item | Labour already exists");
                }
                throw;
            }
            return Created(objVehicleAccidentEstimate);
        }
        //// PATCH: odata/VehicleAccidentEstimates(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleAccidentEstimate> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleAccidentEstimate objVehicleAccidentEstimate = await _objVehicleAccidentEstimateService.FindAsync(key);
            if (objVehicleAccidentEstimate == null)
            {
                return NotFound();
            }
            objVehicleAccidentEstimate.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleAccidentEstimate);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleAccidentEstimateExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVehicleAccidentEstimate);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleAccidentEstimate = await _objVehicleAccidentEstimateService.FindAsync(key);
            if (objVehicleAccidentEstimate == null)
            {
                return NotFound();
            }
            objVehicleAccidentEstimate.ObjectState = ObjectState.Deleted;
            _objVehicleAccidentEstimateService.Delete(objVehicleAccidentEstimate);
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

        private bool VehicleAccidentEstimateExists(string itemLabour,long accidentClaimId)
        {
            return _objVehicleAccidentEstimateService.Query(e => e.ItemLabourName == itemLabour &&  e.AccidentClaimId == accidentClaimId).Select().Any();
        }
        private bool VehicleAccidentEstimateExists(long key)
        {
            return _objVehicleAccidentEstimateService.Query(e => e.Id == key).Select().Any();
        }
    }
}