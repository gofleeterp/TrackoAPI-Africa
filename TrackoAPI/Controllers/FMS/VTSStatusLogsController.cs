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
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VTSStatusLogsController : ODataController
    //ODataController
    {
        private readonly IVTSStatusLogService _service;
        private readonly IUnitOfWorkAsync _uow;

        public VTSStatusLogsController(IUnitOfWorkAsync unitOfWorkAsync, IVTSStatusLogService service)
        {
            _service = service;
            _uow = unitOfWorkAsync;
        }
        // GET: odata/VTSStatusLogs
        [HttpGet, EnableQuery]
        public IQueryable<VTSStatusLog> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/VTSStatusLogs(5)
        [EnableQuery]
        public SingleResult<VTSStatusLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VTSStatusLogs(5)
        public async Task<IHttpActionResult> Put(long key, VTSStatusLog entity)
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
                entity.ObjectState = ObjectState.Modified;
                _service.Update(entity);

                var err = GetLiveDbLevelValidation(entity, _uow);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }

                await _uow.SaveChangesAsync();
                await RunDateLogic(entity, 2);
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
                if (!VTSStatusLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            Request.GetHubContext().IntimateVTSStatusChangeForTripLog(entity);
            return Updated(entity);
        }
        
        // POST: odata/VTSStatusLogs
        public async Task<IHttpActionResult> Post(VTSStatusLog entity)
        {
            entity.ObjectState = ObjectState.Added;
            
            try
            {
                var dt = entity.Data ?? new List<JsonDataEntity>();
                if (dt.Any())
                {
                    entity.DataProps = JsonConvert.SerializeObject(dt);
                }
                if (!Request.IsBatchRequest())
                {
                    _uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                _service.Insert(entity);

                var err = GetLiveDbLevelValidation(entity, _uow);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }

                await _uow.SaveChangesAsync();
                await RunDateLogic(entity, 1);
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
                if (VTSStatusLogExists(entity.StartDate, entity.VehicleId,entity.HireVehicleId, entity.TriplogId.GetValueOrDefault()))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name | Code already exists");
                }
                throw;
            }
            Request.GetHubContext().IntimateVTSStatusChangeForTripLog(entity);
            return Created(entity);
        }
        //// PATCH: odata/VTSStatusLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VTSStatusLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var dataview = patch.GetEntity().Data;
            VTSStatusLog entity = await _service.FindAsync(key);
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

                if (dataview!=null && dataview.Any())
                {
                    foreach (var je in dataview)
                    {
                        entity.DeleteAndAdd(je);
                    }
                }
                await _uow.SaveChangesAsync();
                await RunDateLogic(entity, 2);
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
                if (!VTSStatusLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            Request.GetHubContext().IntimateVTSStatusChangeForTripLog(entity);
            return Updated(entity);
        }
        // DELETE: odata/Customers(5)
        private string GetLiveDbLevelValidation(VTSStatusLog _record, IUnitOfWorkAsync _uow)
        {
            var livevalidationerr = _uow.SqlQueryAsync(
            "[dbo].[Proc_GBL_VTS_LiveValidationV1]",
            new SqlParameter() { Value = _record.Id, ParameterName = "parameter1" },
            new SqlParameter() { Value = _record.VehicleId, ParameterName = "parameter2" },
            new SqlParameter() { Value = _record.TriplogId, ParameterName = "parameter3" },
            new SqlParameter() { Value = _record.DTSStatusId, ParameterName = "parameter4" },
            new SqlParameter() { Value = _record.StartDate, ParameterName = "parameter5" },
            new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter9" }
            ).Result;

            if (livevalidationerr != null && livevalidationerr?.Rows?.Count > 0)
            {
                return Utilities.To<string>(livevalidationerr.Rows[0]["ErrorMessage"]);
            }
            return "";
        }
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
                VTSStatusLog oldentity = entity.Clone();                
                await _uow.SaveChangesAsync();
                await RunDateLogic(oldentity, 3);
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

        private bool VTSStatusLogExists(DateTime vtsStatusDate,long? vehicleId, long? hireVehicleId, long triplogId)
        {
            return _service.Query(e => e.StartDate == vtsStatusDate && (e.VehicleId==vehicleId) && e.TriplogId==triplogId).Select().Any();
        }
        private bool VTSStatusLogExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }

        private async Task RunDateLogic(VTSStatusLog entity,int action)
        {
            if (entity.TriplogId > 0 &&_service.GetConfigValue<int>("IsVTSEnabled") >0)
            {
                var statusdate = await
                    _uow.RepositoryAsync<DTSStatus>()
                        .Queryable()
                        .Where(x => x.Id == entity.DTSStatusId)
                        .Select(x => new { x.DateId, x.NextStatusId, NextStatusDateId = x.fk_NextStatus.DateId })
                        .FirstOrDefaultAsync();
                if (statusdate == null) return;
                //@parameter6: Action 1:Create,2:Update,3:Delete
                //@parameter4:StatusId which should be auto triggered after this status
                //@parameter5:DateId of Status which should be auto triggered after this status
                try
                {
                    await _uow.ExecSqlQueryAsync($"[dbo].[Proc_TRANS_1624_ApplyTLDateLogic] @parameter1={entity.Id},@parameter2={entity.TriplogId.GetValueOrDefault()},@parameter3={statusdate.DateId.GetValueOrDefault(0)},@parameter4={statusdate.NextStatusId.GetValueOrDefault(0)},@parameter5={statusdate.NextStatusDateId.GetValueOrDefault(0)},@parameter6={action}");
                }
                catch (SqlException ex)
                {
                    throw new BusinessException(ex);
                }
                
                //switch (statusdate.DateId)
                //{
                //    case 1523://Send for Loading 
                //        break;
                //    case 1524://Loading Start 
                //        break;
                //    case 1525://Loading End 
                //        break;
                //    case 1526://Unloading R Date 
                //        break;
                //    case 1527://Unloading Date 
                //        break;
                //    case 1528://MT Running 
                //        break;
                //    case 1529://MT End 
                //        break;
                //    case 1530://Workshop In 
                //        break;
                //    case 1531://Workshop Out 
                //        break;
                //    case 1532://L Workshop In 
                //        break;
                //    case 1533://L Workshop Out 
                //        break;
                //    case 1534://ORM In 
                //        break;
                //    case 1535://ORM Out 
                //        break;
                //}
            }
        }
    }
}