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
    public class VehicleAccidentClaimsController : ODataController
    //ODataController
    {
        private readonly IVehicleAccidentClaimService _objVehicleAccidentClaimService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public VehicleAccidentClaimsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleAccidentClaimService service)
        {
            _objVehicleAccidentClaimService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/VehicleAccidentClaims
        [HttpGet, EnableQuery]
        public IQueryable<VehicleAccidentClaim> Get()
        {
            return _objVehicleAccidentClaimService.Queryable();
        }
        // GET: odata/VehicleAccidentClaims(5)
        [EnableQuery]
        public SingleResult<VehicleAccidentClaim> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleAccidentClaimService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleAccidentClaims(5)
        public async Task<IHttpActionResult> Put(long key, VehicleAccidentClaim objVehicleAccidentClaim)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleAccidentClaim.Id)
            {
                return BadRequest();
            }
            objVehicleAccidentClaim.ObjectState = ObjectState.Modified;
            _objVehicleAccidentClaimService.Update(objVehicleAccidentClaim);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleAccidentClaimExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVehicleAccidentClaim);
        }
        // POST: odata/VehicleAccidentClaims
        public async Task<IHttpActionResult> Post(VehicleAccidentClaim objVehicleAccidentClaim)
        {
            objVehicleAccidentClaim.ObjectState = ObjectState.Added;
            _objVehicleAccidentClaimService.Insert(objVehicleAccidentClaim);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VehicleAccidentClaimExists(objVehicleAccidentClaim.DocumentNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "This Item | Labour already exists");
                }
                throw;
            }
            return Created(objVehicleAccidentClaim);
        }
        //// PATCH: odata/VehicleAccidentClaims(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleAccidentClaim> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleAccidentClaim objVehicleAccidentClaim = await _objVehicleAccidentClaimService.FindAsync(key);
            if (objVehicleAccidentClaim == null)
            {
                return NotFound();
            }
            objVehicleAccidentClaim.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleAccidentClaim);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleAccidentClaimExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVehicleAccidentClaim);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleAccidentClaim = await _objVehicleAccidentClaimService.FindAsync(key);
            if (objVehicleAccidentClaim == null)
            {
                return NotFound();
            }
            objVehicleAccidentClaim.ObjectState = ObjectState.Deleted;
            _objVehicleAccidentClaimService.Delete(objVehicleAccidentClaim);
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

        private bool VehicleAccidentClaimExists(string documentNo)
        {
            return _objVehicleAccidentClaimService.Query(e => e.DocumentNo == documentNo).Select().Any();
        }
        private bool VehicleAccidentClaimExists(long key)
        {
            return _objVehicleAccidentClaimService.Query(e => e.Id == key).Select().Any();
        }
    }
}