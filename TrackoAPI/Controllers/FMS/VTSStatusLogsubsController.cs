using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Newtonsoft.Json;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global.DTS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.vw.ts;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VTSStatusLogsubsController : ODataController
    //ODataController
    {
        private readonly IVTSStatusLogsubService _service;
        private readonly IUnitOfWorkAsync _uow;

        public VTSStatusLogsubsController(IUnitOfWorkAsync unitOfWorkAsync, IVTSStatusLogsubService service)
        {
            _service = service;
            _uow = unitOfWorkAsync;
        }
        // GET: odata/VTSStatusLogsubs
        [HttpGet, EnableQuery]
        public IQueryable<VTSStatusLogsub> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/VTSStatusLogsubs(5)
        [EnableQuery]
        public SingleResult<VTSStatusLogsub> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VTSStatusLogsubs(5)
        public async Task<IHttpActionResult> Put(long key, VTSStatusLogsub entity)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            

            try
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }

                var err = GetLiveDbLevelValidation(entity, _uow);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }

                entity.ObjectState = ObjectState.Modified;
                _service.Update(entity);

                await _uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    _uow.Commit();
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
            return Updated(entity);
        }
        private string GetLiveDbLevelValidation(VTSStatusLogsub _record, IUnitOfWorkAsync _uow)
        {
            var livevalidationerr = _uow.SqlQueryAsync(
            "[dbo].[Proc_GBL_VTSSub_LiveValidationV1]",
            new SqlParameter() { Value = _record.Id, ParameterName = "parameter1" }/*Id*/,
            new SqlParameter() { Value = _record.VTSLogId, ParameterName = "parameter2" }/*VTSLogId*/,
            new SqlParameter() { Value = _record.StartDate, ParameterName = "parameter3" }/*StartDate*/,
            new SqlParameter() { Value = _record.DTSStatusId, ParameterName = "parameter4" }/*DTSStatusId*/,
            new SqlParameter() { Value = _record.LocationId, ParameterName = "parameter5" }/*LocationId*/,
            new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter9" }/*SessionId*/
            ).Result;

            if (livevalidationerr != null && livevalidationerr?.Rows?.Count > 0)
            {
                return Utilities.To<string>(livevalidationerr.Rows[0]["ErrorMessage"]);
            }
            return "";
        }


        // POST: odata/VTSStatusLogsubs
        public async Task<IHttpActionResult> Post(VTSStatusLogsub entity)
        {
            entity.ObjectState = ObjectState.Added;
            
            try
            { 
                if (!Request.IsBatchRequest())
                {
                    _uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                var err = GetLiveDbLevelValidation(entity, _uow);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
                _service.Insert(entity);
                await _uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    _uow.Commit();
                }
            }
            catch (DbUpdateException)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
            return Created(entity);
        }
        //// PATCH: odata/VTSStatusLogsubs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VTSStatusLogsub> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            VTSStatusLogsub entity = await _service.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            
            try
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                entity.ObjectState = ObjectState.Modified;
                patch.Patch(entity);

                var err = GetLiveDbLevelValidation(entity, _uow);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }

                await _uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    _uow.Commit();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
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
            
            try
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                entity.ObjectState = ObjectState.Deleted;
                _service.Delete(entity);

                var err = GetLiveDbLevelValidation(entity, _uow);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }

                await _uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    _uow.Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _uow.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}