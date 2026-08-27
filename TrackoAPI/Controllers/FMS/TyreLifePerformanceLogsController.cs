using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TyreLifePerformanceLogsController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<TyreLifePerformanceLog> _log;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public TyreLifePerformanceLogsController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWorkAsync = unitOfWorkAsync;
            _log = unitOfWorkAsync.RepositoryAsync<TyreLifePerformanceLog>();
        }
        // GET: odata/TyreLifePerformanceLogs
        [HttpGet, EnableQuery(MaxNodeCount = 200)]
        public IQueryable<TyreLifePerformanceLog> Get()
        {
            return _log.Queryable();
        }
        // GET: odata/TyreLifePerformanceLogs(5)
        [EnableQuery]
        public SingleResult<TyreLifePerformanceLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_log.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/TyreLifePerformanceLogs(5)
        public async Task<IHttpActionResult> Put(long key, TyreLifePerformanceLog objTyreLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTyreLog.Id)
            {
                return BadRequest();
            }
            objTyreLog.ObjectState = ObjectState.Modified;
            _log.Update(objTyreLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objTyreLog);
        }
        // POST: odata/TyreLifePerformanceLogs
        public async Task<IHttpActionResult> Post(TyreLifePerformanceLog objTyreLog)
        {
            objTyreLog.ObjectState = ObjectState.Added;
            _log.Insert(objTyreLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objTyreLog);
        }
        //// PATCH: odata/TyreLifePerformanceLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TyreLifePerformanceLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TyreLifePerformanceLog objTyreLog = await _log.FindAsync(key);
            if (objTyreLog == null)
            {
                return NotFound();
            }
            objTyreLog.ObjectState = ObjectState.Modified;
            patch.Patch(objTyreLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Updated(objTyreLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTyreLog = await _log.FindAsync(key);
            if (objTyreLog == null)
            {
                return NotFound();
            }
            objTyreLog.ObjectState = ObjectState.Deleted;
            _log.Delete(objTyreLog);
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

        
    }
}