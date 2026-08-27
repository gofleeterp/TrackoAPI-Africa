using EntityFramework.Extensions;
using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global.DTS;
using TrackoApi.Service.TMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNDTSStatusLogsController : ODataController
    //ODataController
    {
        private readonly ICNDTSStatusLogService _repo;

        public CNDTSStatusLogsController(ICNDTSStatusLogService service)
        {
            _repo = service;
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var entity = await _repo.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Deleted;
            var uow = Request.GetContext();
            var dateid = (await uow.RepositoryAsync<DTSStatus>()
                             .Queryable()
                             .Where(x => x.Id == entity.StatusId && x.DateId > 0)
                             .Select(x => new { x.DateId })
                             .FromCacheFirstOrDefaultAsync())?.DateId ?? 0;

            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (dateid == 1558)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tCNMaster] SET PODDate=NULL WHERE Id={entity.CNId}");
                }
                _repo.Delete(entity);
                await Request.GetContext().SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                uow.Rollback();
                throw;
            }
        }

        // GET: odata/CNDTSStatusLogs
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<CNDTSStatusLog> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/CNDTSStatusLogs(5)
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<CNDTSStatusLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        //// PATCH: odata/CNDTSStatusLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNDTSStatusLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CNDTSStatusLog ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            ch.ObjectState = ObjectState.Modified;
            patch.Patch(ch);
            var uow = Request.GetContext();
            var dateid = (await uow.RepositoryAsync<DTSStatus>()
                             .Queryable()
                             .Where(x => x.Id == ch.StatusId && x.DateId > 0)
                             .Select(x => new { x.DateId })
                             .FromCacheFirstOrDefaultAsync())?.DateId ?? 0;

            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (dateid == 1558)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tCNMaster] SET PODDate='{ch.StartDate.ToString("yyyy-MM-dd")}' WHERE Id={ch.CNId}");
                }
                await uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return Updated(ch);
            }
            catch (Exception e)
            {
                uow.Rollback();
                throw;
            }
        }

        // POST: odata/CNDTSStatusLogs
        public async Task<IHttpActionResult> Post(CNDTSStatusLog entity)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var dateid = (await uow.RepositoryAsync<DTSStatus>()
                                 .Queryable()
                                 .Where(x => x.Id == entity.StatusId && x.DateId > 0)
                                 .Select(x => new { x.DateId })
                                 .FromCacheFirstOrDefaultAsync())?.DateId ?? 0;
                if (dateid == 1558)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tCNMaster] SET PODDate='{entity.StartDate.ToString("yyyy-MM-dd")}' WHERE Id={entity.CNId}");
                }

                entity.ObjectState = ObjectState.Added;
                var ch = _repo.Insert(entity);
                await Request.GetContext().SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return Created(ch);
            }
            catch (Exception e)
            {
                uow.Rollback();
                throw;
            }
        }

        // POST: odata/VehicleMovementLogs(key)/CNDTSStatuss
        [ODataRoute("CNDTSStatuses({key})/Logs")]
        public async Task<IHttpActionResult> PostCNDTSStatuses([FromODataUri]long key, [FromBody] CNDTSStatusLog log)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            if (!await uow.RepositoryAsync<CNDTSStatus>().Queryable().AnyAsync(x => x.Id == key))
                return BadRequest("Invalid Status Document");
            log.CNDTSStatusId = key;
            log.ObjectState = ObjectState.Added;
            var item = _repo.Insert(log);
            await uow.SaveChangesAsync();
            if (log.PreviousLogId.GetValueOrDefault() > 0)
            {
                await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tCNDTSStatusLog] SET NextLogId={log.Id} WHERE Id={log.PreviousLogId}");
                await _repo.FindAsync(log.PreviousLogId);
            }
            return Created(item);
        }

        // PUT: odata/CNDTSStatusLogs(5)
        public async Task<IHttpActionResult> Put(long key, CNDTSStatusLog entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var dateid = (await uow.RepositoryAsync<DTSStatus>()
                                 .Queryable()
                                 .Where(x => x.Id == entity.StatusId && x.DateId > 0)
                                 .Select(x => new { x.DateId })
                                 .FromCacheFirstOrDefaultAsync())?.DateId ?? 0;
                if (dateid == 1558)
                {
                    await uow.ExecSqlQueryAsync(
                        $"UPDATE [dbo].[tCNMaster] SET PODDate='{entity.StartDate.ToString("yyyy-MM-dd")}' WHERE Id={entity.CNId}");
                }
                entity.ObjectState = ObjectState.Modified;
                _repo.Update(entity);
                await Request.GetContext().SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return Updated(entity);
            }
            catch (Exception e)
            {
                uow.Rollback();
                throw;
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext()?.Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}