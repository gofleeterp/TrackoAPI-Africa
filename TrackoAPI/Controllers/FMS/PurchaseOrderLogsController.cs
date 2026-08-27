using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoApi.Models.FMS.Inventory;
using TrackoAPI.ViewModels.Global;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Management;
using TrackoApi.Core.Helpers;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class PurchaseOrderLogsController : ODataController
    //ODataController
    {
        private readonly IPurchaseOrderLogService _objPurchaseOrderLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public PurchaseOrderLogsController(IUnitOfWorkAsync unitOfWorkAsync, IPurchaseOrderLogService service)
        {
            _objPurchaseOrderLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/PurchaseOrderLogs
        [HttpGet, EnableQuery]
        public IQueryable<PurchaseOrderLog> Get()
        {
            return _objPurchaseOrderLogService.Queryable();
        }
        // GET: odata/PurchaseOrderLogs(5)
        [EnableQuery]
        public SingleResult<PurchaseOrderLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objPurchaseOrderLogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PurchaseOrderLogs(5)
        public async Task<IHttpActionResult> Put(long key, PurchaseOrderLog objPurchaseOrderLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objPurchaseOrderLog.Id)
            {
                return BadRequest();
            }
            objPurchaseOrderLog.ObjectState = ObjectState.Modified;
            _objPurchaseOrderLogService.Update(objPurchaseOrderLog);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseOrderLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = objPurchaseOrderLog.fk_PurchaseOrder.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = objPurchaseOrderLog.fk_PurchaseOrder.PONo, ParameterName = "parameter3" },
                new SqlParameter() { Value = objPurchaseOrderLog.fk_PurchaseOrder.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            return Updated(objPurchaseOrderLog);
        }
        // POST: odata/PurchaseOrderLogs
        public async Task<IHttpActionResult> Post(PurchaseOrderLog objPurchaseOrderLog)
        {
            objPurchaseOrderLog.ObjectState = ObjectState.Added;
            _objPurchaseOrderLogService.Insert(objPurchaseOrderLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                
                throw;
            }
            return Created(objPurchaseOrderLog);
        }
        //// PATCH: odata/PurchaseOrderLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PurchaseOrderLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PurchaseOrderLog objPurchaseOrderLog = await _objPurchaseOrderLogService.FindAsync(key);
            if (objPurchaseOrderLog == null)
            {
                return NotFound();
            }
            patch.TryGetPropertyValue("DataView", out var dv);
            objPurchaseOrderLog.ObjectState = ObjectState.Modified;
            patch.Patch(objPurchaseOrderLog);
            try
            {
                if (dv is List<JsonDataEntity> dataview && dataview.Any())
                {
                    foreach (var entity in dataview)
                    {
                        objPurchaseOrderLog.DeleteAndAdd(entity);
                    }
                }
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseOrderLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = objPurchaseOrderLog.fk_PurchaseOrder.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = objPurchaseOrderLog.fk_PurchaseOrder.PONo, ParameterName = "parameter3" },
                new SqlParameter() { Value = objPurchaseOrderLog.fk_PurchaseOrder.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            return Updated(objPurchaseOrderLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objPurchaseOrderLog = await _objPurchaseOrderLogService.FindAsync(key);
            if (objPurchaseOrderLog == null)
            {
                return NotFound();
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_TSL_Delete]",
                new SqlParameter() { Value = objPurchaseOrderLog.Id, ParameterName = "parameter1" },//TSLID
                new SqlParameter() { Value = objPurchaseOrderLog.PurchaseOrderId, ParameterName = "parameter2" },//TransactionId
                new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter3" },//CSId
                new SqlParameter() { Value = objPurchaseOrderLog.fk_PurchaseOrder.ViewId, ParameterName = "parameter4" }//ViewId
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }

            objPurchaseOrderLog.ObjectState = ObjectState.Deleted;
            _objPurchaseOrderLogService.Delete(objPurchaseOrderLog);
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

        
        private bool PurchaseOrderLogExists(long key)
        {
            return _objPurchaseOrderLogService.Query(e => e.Id == key).Select().Any();
        }
    }
}