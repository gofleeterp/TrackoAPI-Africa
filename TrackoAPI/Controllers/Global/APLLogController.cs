using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using System.Web.OData.Routing;
using TrackoApi.Models.Global;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class APLLogsController : ODataController
    {
        private readonly IAPLLogService _objAPLLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        public APLLogsController(IUnitOfWorkAsync unitOfWorkAsync, IAPLLogService service)
        {
            _objAPLLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/APLLogs
        [HttpGet, EnableQuery]
        public IQueryable<APLLog> Get()
        {
            return _objAPLLogService.Queryable();
        }
        // GET: odata/APLLogs(5)
        [EnableQuery]
        public SingleResult<APLLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objAPLLogService.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/APLLogs(5)
        public async Task<IHttpActionResult> Put([FromODataUri] long key, APLLog objAPLLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objAPLLog.Id)
            {
                return BadRequest();
            }
            objAPLLog.ObjectState = ObjectState.Modified;
            _objAPLLogService.Update(objAPLLog);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLLog);
        }
        // POST: odata/APLLogs
        public async Task<IHttpActionResult> Post(APLLog objAPLLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            objAPLLog.ObjectState = ObjectState.Added;
            _objAPLLogService.Insert(objAPLLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objAPLLog);
        }
        //// PATCH: odata/APLLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<APLLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            APLLog objAPLLog = await _objAPLLogService.FindAsync(key);
            if (objAPLLog == null)
            {
                return NotFound();
            }
            objAPLLog.ObjectState = ObjectState.Modified;
            patch.Patch(objAPLLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objAPLLog = await _objAPLLogService.FindAsync(key);
            if (objAPLLog == null)
            {
                return NotFound();
            }
            objAPLLog.ObjectState = ObjectState.Deleted;
            _objAPLLogService.Delete(objAPLLog);
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