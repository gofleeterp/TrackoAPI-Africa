using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Hangfire;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global.CronJobs;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.Global
{
    [AuthorizeEx]
    public class SchedulesController : ODataController
    //ODataController
    {
        private readonly IScheduleLogService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly TimeZoneInfo timeZoneInfo;

        public SchedulesController(IUnitOfWorkAsync unitOfWorkAsync, IScheduleLogService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
            timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Helper.CountryTimeZone);//TimeZoneInfo.Local;
        }
        // GET: odata/ScheduleLog
        [HttpGet, EnableQuery]
        public IQueryable<ScheduleLog> Get() => _repo.Queryable();

        // GET: odata/ScheduleLogs(5)
        [EnableQuery]
        public SingleResult<ScheduleLog> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/ScheduleLogs(5)
        public async Task<IHttpActionResult> Put(long key, ScheduleLog objScheduleLog)
        {
            return BadRequest("Replace method is not allowed");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objScheduleLog.Id)
            {
                return BadRequest();
            }
            objScheduleLog.ObjectState = ObjectState.Modified;
            _repo.Update(objScheduleLog);

            try
            {
              
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SecheduleExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objScheduleLog);
        }
        // POST: odata/ScheduleLogs
        public async Task<IHttpActionResult> Post(ScheduleLog objScheduleLog)
        {
            objScheduleLog.ObjectState = ObjectState.Added;
            _repo.Insert(objScheduleLog);
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            }
            objScheduleLog.HangfireId = Guid.NewGuid().ToString("N")+"_"+Helper.TenantShortName.ToLower();
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
                if (objScheduleLog.ScheduleTypeId == 1493/*Recurring*/)
                {
                    RecurringJob.AddOrUpdate<IHangfireJobProcessor>(objScheduleLog.HangfireId, x => x.RunBusinessSchedule(null, objScheduleLog.Id, Helper.LoggedInTenantId), objScheduleLog.CronText, timeZone: timeZoneInfo, queue: "business_queue");
                }
                //else
                //{
                //    BackgroundJob. AddOrUpdate(objScheduleLog.HangfireId, () => new HangfireJobProcessor().RunBusinessSchedule(null, objScheduleLog.HangfireId, Helper.LoggedInTenantId), objScheduleLog.CronText, timeZone: timeZoneInfo, queue: "business_queue");
                //}
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Commit();
                }
            }
            catch (DbUpdateException)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                if (SecheduleExists(objScheduleLog))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Schedule Already Exists");
                }
                throw;
            }
            return Created(objScheduleLog);
        }
        //// PATCH: odata/ScheduleLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ScheduleLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ScheduleLog objScheduleLog = await _repo.FindAsync(key);
            if (objScheduleLog == null)
            {
                return NotFound();
            }
            var hangfireJobId = objScheduleLog.HangfireId;
            if (string.IsNullOrWhiteSpace(hangfireJobId))
            {
                hangfireJobId= Guid.NewGuid().ToString("N") + "_" + Helper.TenantShortName.ToLower();
            }
            objScheduleLog.ObjectState = ObjectState.Modified;
            patch.Patch(objScheduleLog);
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                objScheduleLog.HangfireId = hangfireJobId;
                await _unitOfWorkAsync.SaveChangesAsync();
                RecurringJob.AddOrUpdate<IHangfireJobProcessor>(objScheduleLog.HangfireId, x => x.RunBusinessSchedule(null, objScheduleLog.Id, Helper.LoggedInTenantId), objScheduleLog.CronText,timeZone: timeZoneInfo,queue:"business_queue");
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Commit();
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                if (!SecheduleExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objScheduleLog);
        }
        // DELETE: odata/ScheduleLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objScheduleLog = await _repo.FindAsync(key);
            if (objScheduleLog == null)
            {
                return NotFound();
            }
            var hangfireId = objScheduleLog.HangfireId;
            await _unitOfWorkAsync.ExecSqlQueryAsync($"DELETE FROM mJob where ScheduleId={key}");
            objScheduleLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(objScheduleLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            RecurringJob.RemoveIfExists(hangfireId);
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

        private bool SecheduleExists(ScheduleLog log)
        {
            if (
                _repo.Query(
                    e =>
                        e.CronText==log.CronText).Select().Any())
            {
                return true;
            }
            if (_repo.Query(x => x.ScheduleName == log.ScheduleName).Select().Any())
            {
                return true;
            }
            return false;
        } 
        private bool SecheduleExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
        
        }
    }