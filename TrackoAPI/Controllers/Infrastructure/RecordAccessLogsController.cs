using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    
    [AuthorizeEx]
    public class RecordAccessLogsController : ODataController
    //ODataController
    {
        private readonly IRecordAccessLogService _objRecordAccessLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public RecordAccessLogsController(IUnitOfWorkAsync unitOfWorkAsync, IRecordAccessLogService service)
        {
            _objRecordAccessLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/RecordAccessLogs
        [HttpGet, EnableQuery]
        public IQueryable<ApiRecordAccessLog> Get()
        {
            return _objRecordAccessLogService.Queryable();
        }
        // GET: odata/RecordAccessLogs(5)
        [EnableQuery]
        public SingleResult<ApiRecordAccessLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objRecordAccessLogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/RecordAccessLogs(5)
        public async Task<IHttpActionResult> Put(long key, ApiRecordAccessLog entity)
        {
            return StatusCode(HttpStatusCode.Forbidden);
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //if (key != entity.Id)
            //{
            //    return BadRequest();
            //}
            //entity.ObjectState = ObjectState.Modified;
            //_objRecordAccessLogService.Update(entity);
            //await _unitOfWorkAsync.SaveChangesAsync();

            //return Updated(entity);
        }
        // POST: odata/RecordAccessLogs
        public async Task<IHttpActionResult> Post(ApiRecordAccessLog entity)
        {
            entity.ObjectState = ObjectState.Added;
            entity.SessionId = Helper.SessionId();
            entity.UserId = Helper.GetLoggedInUserId();
            entity.TimeStamp=DateTime.Now;
            _objRecordAccessLogService.Insert(entity);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(entity);
        }
        //// PATCH: odata/RecordAccessLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ApiRecordAccessLog> patch)
        {
            return StatusCode(HttpStatusCode.Forbidden);
            //if (!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);
            //}

            //ApiRecordAccessLog entity = await _objRecordAccessLogService.FindAsync(key);
            //if (entity == null)
            //{
            //    return NotFound();
            //}
            //entity.ObjectState = ObjectState.Modified;
            //patch.Patch(entity);
            //await _unitOfWorkAsync.SaveChangesAsync();
            //return Updated(entity);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            return StatusCode(HttpStatusCode.Forbidden);
            //var entity = await _objRecordAccessLogService.FindAsync(key);
            //if (entity == null)
            //{
            //    return NotFound();
            //}
            //entity.ObjectState = ObjectState.Deleted;
            //_objRecordAccessLogService.Delete(entity);
            //await _unitOfWorkAsync.SaveChangesAsync();
            //return StatusCode(HttpStatusCode.NoContent);
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