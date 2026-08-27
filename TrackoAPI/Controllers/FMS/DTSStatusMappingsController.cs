using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global.DTS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DTSStatusMappingsController : ODataController
    //ODataController
    {
        private readonly IDTSStatusMappingService _objIdtsStatusMappingService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public DTSStatusMappingsController(IUnitOfWorkAsync unitOfWorkAsync, IDTSStatusMappingService service)
        {
            _objIdtsStatusMappingService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/DTSStatusMappings
        [HttpGet, EnableQuery]
        public IQueryable<DTSStatusMapping> Get()
        {
            return _objIdtsStatusMappingService.Queryable();
        }
        // GET: odata/DTSStatusMappings(5)
        [EnableQuery]
        public SingleResult<DTSStatusMapping> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objIdtsStatusMappingService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/DTSStatusMappings(5)
        public async Task<IHttpActionResult> Put(long key, DTSStatusMapping objDTSStatusMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDTSStatusMapping.Id)
            {
                return BadRequest();
            }
            objDTSStatusMapping.ObjectState = ObjectState.Modified;
            _objIdtsStatusMappingService.Update(objDTSStatusMapping);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DTSStatusMappingExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDTSStatusMapping);
        }
        // POST: odata/DTSStatusMappings
        public async Task<IHttpActionResult> Post(DTSStatusMapping objDTSStatusMapping)
        {
            objDTSStatusMapping.ObjectState = ObjectState.Added;
            //var dateid =
            //    _unitOfWorkAsync.RepositoryAsync<DTSStatus>()
            //        .Queryable()
            //        .Any(x => x.Id == objDTSStatusMapping.CurrentStatusId&&x.DateId>0);
            //if (dateid && objDTSStatusMapping.CurrentStatusId == objDTSStatusMapping.NextStatusId)
            //{
            //    throw new 
            //}
            //TODO:Add Logic for Self Child Restriction e.g. Sent For Loading can't be Child of Sent For Loading
            _objIdtsStatusMappingService.Insert(objDTSStatusMapping);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DTSStatusMappingExists(objDTSStatusMapping.CurrentStatusId, objDTSStatusMapping.NextStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name | Code already exists");
                }
                throw;
            }
            return Created(objDTSStatusMapping);
        }
        //// PATCH: odata/DTSStatusMappings(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DTSStatusMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DTSStatusMapping objDTSStatusMapping = await _objIdtsStatusMappingService.FindAsync(key);
            if (objDTSStatusMapping == null)
            {
                return NotFound();
            }
            objDTSStatusMapping.ObjectState = ObjectState.Modified;
            patch.Patch(objDTSStatusMapping);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DTSStatusMappingExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDTSStatusMapping);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDTSStatusMapping = await _objIdtsStatusMappingService.FindAsync(key);
            if (objDTSStatusMapping == null)
            {
                return NotFound();
            }
            objDTSStatusMapping.ObjectState = ObjectState.Deleted;
            _objIdtsStatusMappingService.Delete(objDTSStatusMapping);
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

        private bool DTSStatusMappingExists(long currentStatusId, long nextStatusId)
        {
            return _objIdtsStatusMappingService.Query(e => e.CurrentStatusId == currentStatusId && e.NextStatusId == nextStatusId).Select().Any();
        }
        private bool DTSStatusMappingExists(long key)
        {
            return _objIdtsStatusMappingService.Query(e => e.Id == key).Select().Any();
        }
    }
}