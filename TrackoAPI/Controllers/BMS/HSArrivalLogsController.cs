using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service.BMS;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.BMS
{
    public class HMArrivalLogsController : ODataController
    {
        private readonly IHMArrivalLogService _repo;
        public HMArrivalLogsController(IHMArrivalLogService service)
        {
            _repo = service;
        }
        // GET: odata/HMArrivalLogs
        [HttpGet, EnableQuery]
        public IQueryable<HMArrivalLog> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/HMArrivalLogs(5)
        [EnableQuery]
        public SingleResult<HMArrivalLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/HMArrivalLogs(5)
        public async Task<IHttpActionResult> Put(long key, HMArrivalLog hmar)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != hmar.Id)
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
                hmar.ObjectState = ObjectState.Modified;
                _repo.Update(hmar);
                await uow.SaveChangesAsync();
            }
            catch
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(hmar);
        }
        // POST: odata/HMArrivalLogs
        public async Task<IHttpActionResult> Post(HMArrivalLog hmar)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                hmar.ObjectState = ObjectState.Added;
                _repo.Insert(hmar);
                await uow.SaveChangesAsync();
            }
            catch
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Created(hmar);
        }
        //// PATCH: odata/HMArrivalLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<HMArrivalLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            HMArrivalLog ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                patch.TryGetPropertyValue("Data", out var dv);
                patch.Patch(ch);
                ch.ObjectState = ObjectState.Modified;
                if (dv is List<JsonDataEntity> dataview && dataview.Any())
                {
                    foreach (var entity in dataview)
                    {
                        ch.DeleteAndAdd(entity);
                    }
                }
                await uow.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(ch);
        }
        
        // DELETE: odata/HMArrivalLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            var hmar = await _repo.FindAsync(key);
            if (hmar == null)
            {
                return NotFound();
            }
            hmar.ObjectState = ObjectState.Deleted;
            _repo.Delete(hmar);
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                await uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var log = _repo.Queryable().SingleOrDefault(p => p.Id == key);
            if (log == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_Settlement":
                    log.fk_Settlement = null;
                    log.SettlementId = null;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var log = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (log == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_Settlement":
                    log.SettlementId = id;
                    log.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && !Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}