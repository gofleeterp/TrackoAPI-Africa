using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS.GPS;
using TrackoApi.Service.FMS.GPS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.FMS.GPS
{
    [AuthorizeEx]
    public class GPSKmLogsController : ODataController
    //ODataController
    {
        private readonly IGPSKmLogService _service;

        public GPSKmLogsController(IGPSKmLogService service)
        {
            _service = service;
        }
        // GET: odata/GPSKmLogs
        [HttpGet, EnableQuery]
        public IQueryable<GPSKmLog> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/GPSKmLogs(5)
        [EnableQuery]
        public SingleResult<GPSKmLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/GPSKmLogs(5)
        public async Task<IHttpActionResult> Put(long key, GPSKmLog objGPSKmLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objGPSKmLog.Id)
            {
                return BadRequest();
            }
            objGPSKmLog.ObjectState = ObjectState.Modified;
            _service.Update(objGPSKmLog);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objGPSKmLog);
        }
        // POST: odata/GPSKmLogs
        public async Task<IHttpActionResult> Post(GPSKmLog objGPSKmLog)
        {
            objGPSKmLog.ObjectState = ObjectState.Added;
            _service.Insert(objGPSKmLog);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objGPSKmLog);
        }
        //// PATCH: odata/GPSKmLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<GPSKmLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GPSKmLog objGPSKmLog = await _service.FindAsync(key);

            if (objGPSKmLog == null)
            {
                return NotFound();
            }
           
            objGPSKmLog.ObjectState = ObjectState.Modified;
            patch.Patch(objGPSKmLog);
            try
            {
                _service.Update(objGPSKmLog);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objGPSKmLog);
        }
        // DELETE: odata/GPSKmLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objGPSKmLog = await _service.FindAsync(key);
            if (objGPSKmLog == null)
            {
                return NotFound();
            }
            //if (objDriverMaster.NextLogId.HasValue)
            //{
            //    throw new BusinessException(ErrorCode.GLB106,"Only Current Status can be deleted.");
            //}
            objGPSKmLog.ObjectState = ObjectState.Deleted;
            _service.Delete(objGPSKmLog);
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
