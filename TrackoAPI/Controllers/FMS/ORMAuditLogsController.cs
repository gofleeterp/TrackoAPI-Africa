using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ORMAuditLogsController : ODataController
    //ODataController
    {
        private readonly IORMAuditLogService _objORMAuditLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public ORMAuditLogsController(IUnitOfWorkAsync unitOfWorkAsync, IORMAuditLogService service)
        {
            _objORMAuditLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/ORMAuditLogs
        [HttpGet, EnableQuery]
        public IQueryable<ORMAuditLog> Get()
        {
            return _objORMAuditLogService.Queryable();
        }
        // GET: odata/ORMAuditLogs(5)
        [EnableQuery]
        public SingleResult<ORMAuditLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objORMAuditLogService.Queryable().Where(t => t.Id == key));
        }
        [HttpPost, ODataRoute("AlterORMAuditLogStatus")]
        public IHttpActionResult AlterORMAuditLogStatus(ODataActionParameters parameters)
        {
            object idsObj;
            List<long> ids = new List<long>();
            if (parameters.TryGetValue("ids", out idsObj))
            {
                var str = idsObj as string;
                if (!string.IsNullOrWhiteSpace(str))
                {
                    foreach (string s in str.Split(','))
                    {
                        try
                        {
                            ids.Add(long.Parse(s));
                        }
                        catch
                        {
                            return BadRequest($"Unable to Cast {s}");
                        }

                    }
                }
            }
            if (ids.Count == 0)
            {
                return BadRequest("No Ids supplied");
            }
            _objORMAuditLogService.AlterStatus(ids);
            if (_unitOfWorkAsync.SaveChanges() > 0)
            {
                return Ok();
            }
            return NotFound();
        }
        // PUT: odata/ORMAuditLogs(5)
        public async Task<IHttpActionResult> Put(long key, ORMAuditLog objORMAuditLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objORMAuditLog.Id)
            {
                return BadRequest();
            }
            objORMAuditLog.ObjectState = ObjectState.Modified;
            _objORMAuditLogService.Update(objORMAuditLog);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ORMAuditLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objORMAuditLog);
        }
        // POST: odata/ORMAuditLogs
        public async Task<IHttpActionResult> Post(ORMAuditLog objORMAuditLog)
        {
            objORMAuditLog.ObjectState = ObjectState.Added;
            _objORMAuditLogService.Insert(objORMAuditLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objORMAuditLog);
        }
        //// PATCH: odata/ORMAuditLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ORMAuditLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ORMAuditLog objORMAuditLog = await _objORMAuditLogService.FindAsync(key);
            if (objORMAuditLog == null)
            {
                return NotFound();
            }
            objORMAuditLog.ObjectState = ObjectState.Modified;
            patch.Patch(objORMAuditLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ORMAuditLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objORMAuditLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objORMAuditLog = await _objORMAuditLogService.FindAsync(key);
            if (objORMAuditLog == null)
            {
                return NotFound();
            }
            objORMAuditLog.ObjectState = ObjectState.Deleted;
            _objORMAuditLogService.Delete(objORMAuditLog);
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
        private bool ORMAuditLogExists(long key)
        {
            return _objORMAuditLogService.Query(e => e.Id == key).Select().Any();
        }
    }
}