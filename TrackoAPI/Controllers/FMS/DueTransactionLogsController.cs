using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Management;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Repository.Pattern.Core.UnitOfWork;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.CronJobs;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DueTransactionLogsController : ODataController
    //ODataController
    {
        private readonly IDueTransactionLogService _objDueTransactionLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public DueTransactionLogsController(IUnitOfWorkAsync unitOfWorkAsync, IDueTransactionLogService service)
        {
            _objDueTransactionLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/DueTransactionLogs
        [HttpGet, EnableQuery]
        public IQueryable<DueTransactionLog> Get()
        {
            return _objDueTransactionLogService.Queryable();
        }
        // GET: odata/DueTransactionLogs(5)
        [EnableQuery]
        public SingleResult<DueTransactionLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objDueTransactionLogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/DueTransactionLogs(5)
        public async Task<IHttpActionResult> Put(long key, DueTransactionLog objDueTransactionLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDueTransactionLog.Id)
            {
                return BadRequest();
            }
            objDueTransactionLog.ConstCurTypeId = Helper.ConstCurTypeId;
            objDueTransactionLog.ObjectState = ObjectState.Modified;
            _objDueTransactionLogService.Update(objDueTransactionLog);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DueTransactionLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = objDueTransactionLog.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = objDueTransactionLog.VoucherNo, ParameterName = "parameter3" },
                new SqlParameter() { Value = objDueTransactionLog.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            return Updated(objDueTransactionLog);
        }
        // POST: odata/DueTransactionLogs
        public async Task<IHttpActionResult> Post(DueTransactionLog objDueTransactionLog)
        {
            objDueTransactionLog.ObjectState = ObjectState.Added;
            objDueTransactionLog.ConstCurTypeId=Helper.ConstCurTypeId;
            _objDueTransactionLogService.Insert(objDueTransactionLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                //if (DueTransactionLogExists(objDueTransactionLog.))
                //{
                //    return Conflict();
                //}
                throw;
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = objDueTransactionLog.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = objDueTransactionLog.VoucherNo, ParameterName = "parameter3" },
                new SqlParameter() { Value = objDueTransactionLog.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            return Created(objDueTransactionLog);
        }
        //// PATCH: odata/DueTransactionLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DueTransactionLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DueTransactionLog objDueTransactionLog = await _objDueTransactionLogService.FindAsync(key);
            objDueTransactionLog.ConstCurTypeId = Helper.ConstCurTypeId;
            if (objDueTransactionLog == null)
            {
                return NotFound();
            }
            objDueTransactionLog.ObjectState = ObjectState.Modified;
            patch.Patch(objDueTransactionLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DueTransactionLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = objDueTransactionLog.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = objDueTransactionLog.VoucherNo, ParameterName = "parameter3" },
                new SqlParameter() { Value = objDueTransactionLog.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            return Updated(objDueTransactionLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDueTransactionLog = await _objDueTransactionLogService.FindAsync(key);
            if (objDueTransactionLog == null)
            {
                return NotFound();
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_TSL_Delete]",
                new SqlParameter() { Value = objDueTransactionLog.Id, ParameterName = "parameter1" },//TSLID
                new SqlParameter() { Value = objDueTransactionLog.VoucherId, ParameterName = "parameter2" },//TransactionId
                new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter3" },//CSId
                new SqlParameter() { Value = objDueTransactionLog.ViewId, ParameterName = "parameter4" }//ViewId
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            objDueTransactionLog.ObjectState = ObjectState.Deleted;
            _objDueTransactionLogService.Delete(objDueTransactionLog);
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

        //private bool DueTransactionLogExists(string DueName)
        //{
        //    return _objDueTransactionLogService.Query(e => e.Name == DueName).Select().Any();
        //}
        private bool DueTransactionLogExists(long key)
        {
            return _objDueTransactionLogService.Query(e => e.Id == key).Select().Any();
        }

        
    }
}