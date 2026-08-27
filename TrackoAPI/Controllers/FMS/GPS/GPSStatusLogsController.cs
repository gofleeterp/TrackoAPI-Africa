using Repository.Pattern.Core.Repositories;
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
    public class GPSStatusLogsController : ODataController
    //ODataController
    {
        private readonly Repository<GPSStatusLog> _service;

        public GPSStatusLogsController(IRepositoryAsync<GPSStatusLog> service)
        {
            _service = (Repository<GPSStatusLog>)service;
        }
        // GET: odata/GPSStatusLogs
        [HttpGet, EnableQuery]
        public IQueryable<GPSStatusLog> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/GPSStatusLogs(5)
        [EnableQuery]
        public SingleResult<GPSStatusLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/GPSStatusLogs(5)
        public async Task<IHttpActionResult> Put(long key, GPSStatusLog objGPSStatusLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objGPSStatusLog.Id)
            {
                return BadRequest();
            }
            objGPSStatusLog.ObjectState = ObjectState.Modified;
            _service.Update(objGPSStatusLog);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objGPSStatusLog);
        }
        // POST: odata/GPSStatusLogs
        public async Task<IHttpActionResult> Post(GPSStatusLog objGPSStatusLog)
        {
            objGPSStatusLog.ObjectState = ObjectState.Added;
            _service.Insert(objGPSStatusLog);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objGPSStatusLog);
        }
        //// PATCH: odata/GPSStatusLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<GPSStatusLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GPSStatusLog objGPSStatusLog = await _service.FindAsync(key);

            if (objGPSStatusLog == null)
            {
                return NotFound();
            }
           
            objGPSStatusLog.ObjectState = ObjectState.Modified;
            patch.Patch(objGPSStatusLog);
            try
            {
                _service.Update(objGPSStatusLog);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objGPSStatusLog);
        }
        // DELETE: odata/GPSStatusLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objGPSStatusLog = await _service.FindAsync(key);
            if (objGPSStatusLog == null)
            {
                return NotFound();
            }
            //if (objDriverMaster.NextLogId.HasValue)
            //{
            //    throw new BusinessException(ErrorCode.GLB106,"Only Current Status can be deleted.");
            //}
            objGPSStatusLog.ObjectState = ObjectState.Deleted;
            _service.Delete(objGPSStatusLog);
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
