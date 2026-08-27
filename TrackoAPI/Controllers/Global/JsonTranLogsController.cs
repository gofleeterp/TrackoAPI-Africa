using Repository.Pattern.Core.Repositories;

using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class JsonTransactionLogsController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<JsonTransactionLog> _service;

        public JsonTransactionLogsController(IRepositoryAsync<JsonTransactionLog> service)
        {
            _service = service;
        }
        // GET: odata/JsonTransactionLogs
        [HttpGet, EnableQuery]
        public IQueryable<JsonTransactionLog> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/JsonTransactionLogs(5)
        [EnableQuery]
        public SingleResult<JsonTransactionLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/JsonTransactionLogs(5)
        public async Task<IHttpActionResult> Put(long key, JsonTransactionLog entity)
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
            _service.Update(entity);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JsonTransactionLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(entity);
        }
        // POST: odata/JsonTransactionLogs
        public async Task<IHttpActionResult> Post(JsonTransactionLog entity)
        {
            entity.ObjectState = ObjectState.Added;
            _service.Insert(entity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (JsonTransactionLogExists(entity.RecordId,entity.ViewId,entity.Key))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(entity);
        }
        //// PATCH: odata/JsonTransactionLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<JsonTransactionLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            JsonTransactionLog entity = await _service.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Modified;
            patch.Patch(entity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JsonTransactionLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(entity);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var entity = await _service.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Deleted;
            _service.Delete(entity);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                    Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool JsonTransactionLogExists(long recordId,long viewid,string key)
        {
            return _service.Query(e => e.RecordId == recordId&&e.ViewId==viewid&&e.Key==key).Select().Any();
        }
        private bool JsonTransactionLogExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] long key, string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var triplog = await _service.FindAsync(key);
            if (triplog == null)
            {
                return NotFound();
            }
            var newrecordid = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "RecordId":
                    triplog.RecordId = newrecordid;
                    triplog.ObjectState = ObjectState.Modified;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}