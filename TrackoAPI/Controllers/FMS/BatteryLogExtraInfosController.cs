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
    public class BatteryLogExtraInfosController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<BatteryLogExtraInfo> _log;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public BatteryLogExtraInfosController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWorkAsync = unitOfWorkAsync;
            _log = unitOfWorkAsync.RepositoryAsync<BatteryLogExtraInfo>();
        }
        // GET: odata/BatteryLogs
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<BatteryLogExtraInfo> Get()
        {
            return _log.Queryable();
        }
        // GET: odata/BatteryLogs(5)
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<BatteryLogExtraInfo> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_log.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/BatteryLogs(5)
        public async Task<IHttpActionResult> Put(long key, BatteryLogExtraInfo objBatteryLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objBatteryLog.Id)
            {
                return BadRequest();
            }
            objBatteryLog.ObjectState = ObjectState.Modified;
            _log.Update(objBatteryLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objBatteryLog);
        }
        // POST: odata/BatteryLogs
        public async Task<IHttpActionResult> Post(BatteryLogExtraInfo objBatteryLog)
        {
            objBatteryLog.ObjectState = ObjectState.Added;
            _log.Insert(objBatteryLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objBatteryLog);
        }
        //// PATCH: odata/BatteryLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<BatteryLogExtraInfo> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BatteryLogExtraInfo objBatteryLog = await _log.FindAsync(key);
            if (objBatteryLog == null)
            {
                return NotFound();
            }
            objBatteryLog.ObjectState = ObjectState.Modified;
            patch.Patch(objBatteryLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Updated(objBatteryLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objBatteryLog = await _log.FindAsync(key);
            if (objBatteryLog == null)
            {
                return NotFound();
            }
            objBatteryLog.ObjectState = ObjectState.Deleted;
            _log.Delete(objBatteryLog);
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