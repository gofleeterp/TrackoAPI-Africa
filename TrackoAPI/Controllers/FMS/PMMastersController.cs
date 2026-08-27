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
    public class PmMastersController : ODataController
    //ODataController
    {
        private readonly IPMMasterService _objPmMasterService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public PmMastersController(IUnitOfWorkAsync unitOfWorkAsync, IPMMasterService service)
        {
            _objPmMasterService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/PMMasters
        [HttpGet, EnableQuery]
        public IQueryable<PMMaster> Get()
        {
            return _objPmMasterService.Queryable();
        }
        // GET: odata/PMMasters(5)
        [EnableQuery]
        public SingleResult<PMMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objPmMasterService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PMMasters(5)
        public async Task<IHttpActionResult> Put(long key, PMMaster objPmMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objPmMaster.Id)
            {
                return BadRequest();
            }
            objPmMaster.ObjectState = ObjectState.Modified;
            _objPmMasterService.Update(objPmMaster);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PMMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objPmMaster);
        }
        // POST: odata/PMMasters
        public async Task<IHttpActionResult> Post(PMMaster objPmMaster)
        {
            objPmMaster.ObjectState = ObjectState.Added;
            _objPmMasterService.Insert(objPmMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PMMasterExists(objPmMaster.Name))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objPmMaster);
        }
        //// PATCH: odata/PMMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PMMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PMMaster objPmMaster = await _objPmMasterService.FindAsync(key);
            if (objPmMaster == null)
            {
                return NotFound();
            }
            objPmMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objPmMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PMMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objPmMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objPmMaster = await _objPmMasterService.FindAsync(key);
            if (objPmMaster == null)
            {
                return NotFound();
            }
            objPmMaster.ObjectState = ObjectState.Deleted;
            _objPmMasterService.Delete(objPmMaster);
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

        private bool PMMasterExists(string pmName)
        {
            return _objPmMasterService.Query(e => e.Name == pmName).Select().Any();
        }
        private bool PMMasterExists(long key)
        {
            return _objPmMasterService.Query(e => e.Id == key).Select().Any();
        }
    }
}