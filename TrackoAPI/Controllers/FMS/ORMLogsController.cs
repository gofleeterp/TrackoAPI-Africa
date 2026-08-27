using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ORMLogsController : ODataController
    //ODataController
    {
        private readonly IORMLogService _objORMLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public ORMLogsController(IUnitOfWorkAsync unitOfWorkAsync, IORMLogService service)
        {
            _objORMLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/ORMLogs
        [HttpGet, EnableQuery]
        public IQueryable<ORMLog> Get()
        {
            return _objORMLogService.Queryable();
        }
        // GET: odata/ORMLogs(5)
        [EnableQuery]
        public SingleResult<ORMLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objORMLogService.Queryable().Where(t => t.Id == key));
        }
        [HttpPost, ODataRoute("AlterORMLogStatus")]
        public IHttpActionResult AlterORMLogStatus(ODataActionParameters parameters)
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
            _objORMLogService.AlterStatus(ids);
            if (_unitOfWorkAsync.SaveChanges() > 0)
            {
                return Ok();
            }
            return NotFound();
        }
        // PUT: odata/ORMLogs(5)
        public async Task<IHttpActionResult> Put(long key, ORMLog objORMLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objORMLog.Id)
            {
                return BadRequest();
            }
            objORMLog.ObjectState = ObjectState.Modified;
            _objORMLogService.Update(objORMLog);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ORMLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objORMLog);
        }
        // POST: odata/ORMLogs
        public async Task<IHttpActionResult> Post(ORMLog objORMLog)
        {
            objORMLog.ObjectState = ObjectState.Added;
            _objORMLogService.Insert(objORMLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ORMLogExists(objORMLog.ORMNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objORMLog);
        }
        //// PATCH: odata/ORMLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ORMLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ORMLog objORMLog = await _objORMLogService.FindAsync(key);
            if (objORMLog == null)
            {
                return NotFound();
            }
            objORMLog.ObjectState = ObjectState.Modified;
            patch.Patch(objORMLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ORMLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objORMLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objORMLog = await _objORMLogService.FindAsync(key);
            if (objORMLog == null)
            {
                return NotFound();
            }
            objORMLog.ObjectState = ObjectState.Deleted;
            _objORMLogService.Delete(objORMLog);
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

        private bool ORMLogExists(string ormNo)
        {
            return _objORMLogService.Query(e => e.ORMNo == ormNo).Select().Any();
        }
        private bool ORMLogExists(long key)
        {
            return _objORMLogService.Query(e => e.Id == key).Select().Any();
        }
    }
}