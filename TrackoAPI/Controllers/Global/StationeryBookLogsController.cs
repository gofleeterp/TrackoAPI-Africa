using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class StationeryBookLogsController : ODataController
    //ODataController
    {
        private readonly IStationeryBookLogService _objStationeryBookLogService;
        private readonly IStationeryBookLogArchiveService _archService;

        public StationeryBookLogsController(IStationeryBookLogService service, IStationeryBookLogArchiveService archService)
        {
            _objStationeryBookLogService = service;
            _archService = archService;
        }
        // GET: odata/Vouchers
        [HttpGet, EnableQuery]
        public IQueryable<StationeryBookLog> Get()
        {
            return _objStationeryBookLogService.Queryable();
        }
        // GET: odata/Vouchers(5)
        [EnableQuery]
        public SingleResult<StationeryBookLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objStationeryBookLogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/Vouchers(5)
        public async Task<IHttpActionResult> Put(long key, StationeryBookLog objStationeryBookLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objStationeryBookLog.Id)
            {
                return BadRequest();
            }
            objStationeryBookLog.ObjectState = ObjectState.Modified;
            _objStationeryBookLogService.Update(objStationeryBookLog);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StationeryBookLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objStationeryBookLog);
        }
        
        [HttpGet,ODataRoute("GetStationaryByFieldId(fieldId={fieldId},viewId={viewId},typeId={typeId})")]
        public IHttpActionResult GetStationaryByFieldId([FromODataUri] long fieldId,long? viewId,long? typeId, ODataQueryOptions<StationeryBookLog> options)
        {
            viewId = viewId??0;
            typeId = typeId ?? 0;
            var booktype =
                Request.GetContext().Context.Database.SqlQuery<long?>(
                        "SELECT TOP 1 BookTypeId FROM [dbo].[mViewFieldBookMap] WHERE FieldId=@p0 AND ViewId=@p1 AND ISNULL(TypeId,0)=@p2 ORDER BY CDOE DESC",
                        fieldId, viewId, typeId).FirstOrDefault();
            if (booktype.GetValueOrDefault() == 0)
            {
                booktype = Request.GetContext()
                    .Repository<ViewField>()
                    .Queryable()
                    .Where(x => x.Id == fieldId)
                    .Select(x => x.BookTypeId)
                    .FirstOrDefault();
            }
            if (booktype.GetValueOrDefault() == 0) return Ok(new List<StationeryBookLog>().AsQueryable());
            var query = _objStationeryBookLogService.Queryable().Where(x => x.TypeId == booktype.Value);
            var result = options.ApplyTo(query.AsQueryable());
            return this.Ok(result, result.GetType());
        }
        // POST: odata/Vouchers
        public async Task<IHttpActionResult> Post(StationeryBookLog objStationeryBookLog)
        {
            objStationeryBookLog.ObjectState = ObjectState.Added;
            _objStationeryBookLogService.Insert(objStationeryBookLog);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StationeryBookLogExists(objStationeryBookLog.BookId,objStationeryBookLog.PageNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objStationeryBookLog);
        }
        //// PATCH: odata/Vouchers(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<StationeryBookLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StationeryBookLog objStationeryBookLog = await _objStationeryBookLogService.FindAsync(key);
            if (objStationeryBookLog == null)
            {
                return NotFound();
            }
            objStationeryBookLog.ObjectState = ObjectState.Modified;
            patch.Patch(objStationeryBookLog);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StationeryBookLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objStationeryBookLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objStationeryBookLog = await _objStationeryBookLogService.FindAsync(key);
            if (objStationeryBookLog == null)
            {
                return NotFound();
            }
            objStationeryBookLog.ObjectState = ObjectState.Deleted;
            _objStationeryBookLogService.Delete(objStationeryBookLog);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StationeryBookLogExists(long bookId,string pageNo)
        {
            return _objStationeryBookLogService.Query(e => e.BookId == bookId && e.PageNo==pageNo).Select().Any();
        }
        private bool StationeryBookLogExists(long key)
        {
            return _objStationeryBookLogService.Query(e => e.Id == key).Select().Any();
        }
    }
}