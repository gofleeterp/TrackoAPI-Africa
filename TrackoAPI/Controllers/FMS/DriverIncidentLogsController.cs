using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
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
    public class DriverIncidentLogsController : ODataController
    //ODataController
    {
        private readonly IDriverIncidentLogService _objDriverIncidentLogService;

        public DriverIncidentLogsController(IDriverIncidentLogService service)
        {
            _objDriverIncidentLogService = service;
        }
        // GET: odata/DriverEventLogs
        [HttpGet, EnableQuery]
        public IQueryable<DriverIncidentLog> Get()
        {
            return _objDriverIncidentLogService.Queryable();
        }
        // GET: odata/DriverEventLogs(5)
        [EnableQuery]
        public SingleResult<DriverIncidentLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objDriverIncidentLogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/DriverEventLogs(5)
        public async Task<IHttpActionResult> Put(long key, DriverIncidentLog objDriverEventLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDriverEventLog.Id)
            {
                return BadRequest();
            }
            objDriverEventLog.ObjectState = ObjectState.Modified;
            _objDriverIncidentLogService.Update(objDriverEventLog);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objDriverEventLog);
        }
        // POST: odata/DriverEventLogs
        public async Task<IHttpActionResult> Post(DriverIncidentLog objDriverEventLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            objDriverEventLog.ObjectState = ObjectState.Added;
            _objDriverIncidentLogService.Insert(objDriverEventLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(objDriverEventLog);
        }
        //// PATCH: odata/DriverEventLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DriverIncidentLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            DriverIncidentLog objDriverEventLog = await _objDriverIncidentLogService.FindAsync(key);
            if (objDriverEventLog == null)
            {
                return NotFound();
            }
            objDriverEventLog.ObjectState = ObjectState.Modified;
            patch.Patch(objDriverEventLog);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objDriverEventLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDriverEventLog = await _objDriverIncidentLogService.FindAsync(key);
            if (objDriverEventLog == null)
            {
                return NotFound();
            }
            objDriverEventLog.ObjectState = ObjectState.Deleted;
            _objDriverIncidentLogService.Delete(objDriverEventLog);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
            }
            base.Dispose(disposing);
        }

        
    }
}