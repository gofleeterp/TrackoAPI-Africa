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
    public class VTSVendorServicesController : ODataController
    //ODataController
    {
        private readonly IVTSVendorServiceService _objVTSVendorServiceService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public VTSVendorServicesController(IUnitOfWorkAsync unitOfWorkAsync, IVTSVendorServiceService service)
        {
            _objVTSVendorServiceService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/VTSVendorServices
        [HttpGet, EnableQuery]
        public IQueryable<VTSVendorService> Get()
        {
            return _objVTSVendorServiceService.Queryable();
        }
        // GET: odata/VTSVendorServices(5)
        [EnableQuery]
        public SingleResult<VTSVendorService> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVTSVendorServiceService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VTSVendorServices(5)
        public async Task<IHttpActionResult> Put(long key, VTSVendorService objVTSVendorService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVTSVendorService.Id)
            {
                return BadRequest();
            }
            objVTSVendorService.ObjectState = ObjectState.Modified;
            _objVTSVendorServiceService.Update(objVTSVendorService);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VTSVendorServiceExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVTSVendorService);
        }
        // POST: odata/VTSVendorServices
        public async Task<IHttpActionResult> Post(VTSVendorService objVTSVendorService)
        {
            objVTSVendorService.ObjectState = ObjectState.Added;
            _objVTSVendorServiceService.Insert(objVTSVendorService);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VTSVendorServiceExists(objVTSVendorService.ServiceTypeId, objVTSVendorService.VendorId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name | Code already exists");
                }
                throw;
            }
            return Created(objVTSVendorService);
        }
        //// PATCH: odata/VTSVendorServices(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VTSVendorService> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VTSVendorService objVTSVendorService = await _objVTSVendorServiceService.FindAsync(key);
            if (objVTSVendorService == null)
            {
                return NotFound();
            }
            objVTSVendorService.ObjectState = ObjectState.Modified;
            patch.Patch(objVTSVendorService);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VTSVendorServiceExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVTSVendorService);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVTSVendorService = await _objVTSVendorServiceService.FindAsync(key);
            if (objVTSVendorService == null)
            {
                return NotFound();
            }
            objVTSVendorService.ObjectState = ObjectState.Deleted;
            _objVTSVendorServiceService.Delete(objVTSVendorService);
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

        private bool VTSVendorServiceExists(long vendorId,long serviceTypeId)
        {
            return _objVTSVendorServiceService.Query(e => e.VendorId == vendorId && e.ServiceTypeId == serviceTypeId).Select().Any();
        }
        private bool VTSVendorServiceExists(long key)
        {
            return _objVTSVendorServiceService.Query(e => e.Id == key).Select().Any();
        }
    }
}