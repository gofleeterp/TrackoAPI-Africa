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
    public class TollMastersController : ODataController
    //ODataController
    {
        private readonly ITollMasterService _objTollMasterService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public TollMastersController(IUnitOfWorkAsync unitOfWorkAsync, ITollMasterService service)
        {
            _objTollMasterService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/TollMasters
        [HttpGet, EnableQuery]
        public IQueryable<TollMaster> Get()
        {
            return _objTollMasterService.Queryable();
        }
        // GET: odata/TollMasters(5)
        [EnableQuery]
        public SingleResult<TollMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objTollMasterService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/TollMasters(5)
        public async Task<IHttpActionResult> Put(long key, TollMaster objTollMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTollMaster.Id)
            {
                return BadRequest();
            }
            objTollMaster.ObjectState = ObjectState.Modified;
            _objTollMasterService.Update(objTollMaster);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TollMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTollMaster);
        }
        // POST: odata/TollMasters
        public async Task<IHttpActionResult> Post(TollMaster objTollMaster)
        {
            objTollMaster.ObjectState = ObjectState.Added;
            _objTollMasterService.Insert(objTollMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TollMasterExists(objTollMaster.Name))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objTollMaster);
        }
        //// PATCH: odata/TollMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TollMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TollMaster objTollMaster = await _objTollMasterService.FindAsync(key);
            if (objTollMaster == null)
            {
                return NotFound();
            }
            objTollMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objTollMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TollMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTollMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTollMaster = await _objTollMasterService.FindAsync(key);
            if (objTollMaster == null)
            {
                return NotFound();
            }
            objTollMaster.ObjectState = ObjectState.Deleted;
            _objTollMasterService.Delete(objTollMaster);
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

        private bool TollMasterExists(string name)
        {
            return _objTollMasterService.Query(e => e.Name == name).Select().Any();
        }
        private bool TollMasterExists(long key)
        {
            return _objTollMasterService.Query(e => e.Id == key).Select().Any();
        }
    }
}