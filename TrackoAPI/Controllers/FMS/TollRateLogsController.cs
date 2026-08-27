using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TollRateLogsController : ODataController
    //ODataController
    {
        private readonly ITollRateLogService _objTollRateLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public TollRateLogsController(IUnitOfWorkAsync unitOfWorkAsync, ITollRateLogService service)
        {
            _objTollRateLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/TollRateLogs
        [HttpGet, EnableQuery]
        public IQueryable<TollRateLog> Get()
        {
            return _objTollRateLogService.Queryable();
        }
        // GET: odata/TollRateLogs(5)
        [EnableQuery]
        public SingleResult<TollRateLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objTollRateLogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/TollRateLogs(5)
        public async Task<IHttpActionResult> Put(long key, TollRateLog objTollRateLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTollRateLog.Id)
            {
                return BadRequest();
            }
            objTollRateLog.ObjectState = ObjectState.Modified;
            _objTollRateLogService.Update(objTollRateLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objTollRateLog);
        }
        // POST: odata/TollRateLogs
        public async Task<IHttpActionResult> Post(TollRateLog objTollRateLog)
        {
            objTollRateLog.ObjectState = ObjectState.Added;
            _objTollRateLogService.Insert(objTollRateLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objTollRateLog);
        }
        //// PATCH: odata/TollRateLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TollRateLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TollRateLog objTollRateLog = await _objTollRateLogService.FindAsync(key);
            if (objTollRateLog == null)
            {
                return NotFound();
            }
            objTollRateLog.ObjectState = ObjectState.Modified;
            patch.Patch(objTollRateLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objTollRateLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTollRateLog = await _objTollRateLogService.FindAsync(key);
            if (objTollRateLog == null)
            {
                return NotFound();
            }
            objTollRateLog.ObjectState = ObjectState.Deleted;
            _objTollRateLogService.Delete(objTollRateLog);
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