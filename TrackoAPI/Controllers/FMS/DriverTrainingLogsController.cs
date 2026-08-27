using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Repository.Pattern.Core.UnitOfWork;
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
    public class DriverTrainingLogsController : ODataController
        //ODataController
    {
        private readonly IDriverTrainingLogService _objDriverTrainingLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public DriverTrainingLogsController(IUnitOfWorkAsync unitOfWorkAsync, IDriverTrainingLogService service)
        {
            _objDriverTrainingLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }

        // GET: odata/DriverTrainingLogs
        [HttpGet, EnableQuery]
        public IQueryable<DriverTrainingLog> Get()
        {
            return _objDriverTrainingLogService.Queryable();
        }

        // GET: odata/DriverTrainingLogs(5)
        [EnableQuery]
        public SingleResult<DriverTrainingLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objDriverTrainingLogService.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/DriverTrainingLogs(5)
        public async Task<IHttpActionResult> Put(long key, DriverTrainingLog objDriverTrainingLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDriverTrainingLog.Id)
            {
                return BadRequest();
            }
            objDriverTrainingLog.ObjectState = ObjectState.Modified;
            _objDriverTrainingLogService.Update(objDriverTrainingLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objDriverTrainingLog);
        }

        // POST: odata/DriverTrainingLogs
        public async Task<IHttpActionResult> Post(DriverTrainingLog objDriverTrainingLog)
        {
            objDriverTrainingLog.ObjectState = ObjectState.Added;
            _objDriverTrainingLogService.Insert(objDriverTrainingLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objDriverTrainingLog);
        }

        //// PATCH: odata/DriverTrainingLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DriverTrainingLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DriverTrainingLog objDriverTrainingLog = await _objDriverTrainingLogService.FindAsync(key);
            if (objDriverTrainingLog == null)
            {
                return NotFound();
            }
            objDriverTrainingLog.ObjectState = ObjectState.Modified;
            patch.Patch(objDriverTrainingLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objDriverTrainingLog);
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDriverTrainingLog = await _objDriverTrainingLogService.FindAsync(key);
            if (objDriverTrainingLog == null)
            {
                return NotFound();
            }
            objDriverTrainingLog.ObjectState = ObjectState.Deleted;
            _objDriverTrainingLogService.Delete(objDriverTrainingLog);
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