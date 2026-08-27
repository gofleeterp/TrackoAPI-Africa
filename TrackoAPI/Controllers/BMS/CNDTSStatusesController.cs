using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;

using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service.TMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNDTSStatusesController : ODataController
    //ODataController
    {
        private readonly ICNDTSStatusService _repo;

        public CNDTSStatusesController(ICNDTSStatusService service)
        {
            _repo = service;
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            try
            {
                if (!Request.IsBatchRequest())
                {
                    uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                var entity = await _repo.FindAsync(key);
                if (entity == null)
                {
                    return NotFound();
                }
                await uow.ExecSqlQueryAsync($"EXEC [dbo].[Proc_TRANS_1559_DeleteLogs] {key}");
                entity.ObjectState = ObjectState.Deleted;
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
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
        }

        // GET: odata/CNDTSStatuss
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<CNDTSStatus> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/CNDTSStatuss(5)
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<CNDTSStatus> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        //// PATCH: odata/CNDTSStatuss(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNDTSStatus> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CNDTSStatus ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            ch.ObjectState = ObjectState.Modified;
            patch.Patch(ch);
            var uow = Request.GetContext();
            await uow.SaveChangesAsync();

            return Updated(ch);
        }

        // POST: odata/CNDTSStatuss
        public async Task<IHttpActionResult> Post(CNDTSStatus entity)
        {
            entity.ObjectState = ObjectState.Added;
            var ch = _repo.Insert(entity);
            await Request.GetContext().SaveChangesAsync();
            return Created(ch);
        }

        // PUT: odata/CNDTSStatuss(5)
        public async Task<IHttpActionResult> Put(long key, CNDTSStatus entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            entity.ObjectState = ObjectState.Modified;
            _repo.Update(entity);
            await Request.GetContext().SaveChangesAsync();

            return Updated(entity);
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