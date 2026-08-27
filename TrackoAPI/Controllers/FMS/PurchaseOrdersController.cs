using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using System.Web.OData.Routing;
using System.Data.Entity;
using TrackoAPI.WebUtilities.Helper;
using TrackoAPI.ViewModels.Global;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System;
using TrackoApi.Models.FMS;
using System.Web.Management;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class PurchaseOrdersController : ODataController
    //ODataController
    {
        private readonly IPurchaseOrderService _objPurchaseOrderService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public PurchaseOrdersController(IUnitOfWorkAsync unitOfWorkAsync, IPurchaseOrderService service)
        {
            _objPurchaseOrderService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/PurchaseOrders
        [HttpGet, EnableQuery]
        public IQueryable<PurchaseOrder> Get()
        {
            return _objPurchaseOrderService.Queryable();
        }
        // GET: odata/PurchaseOrders(5)
        [EnableQuery]
        public SingleResult<PurchaseOrder> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objPurchaseOrderService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PurchaseOrders(5)
        public async Task<IHttpActionResult> Put(long key, PurchaseOrder objPurchaseOrder)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objPurchaseOrder.Id)
            {
                return BadRequest();
            }
            objPurchaseOrder.ObjectState = ObjectState.Modified;

            var err = GetLiveDbLevelValidation(objPurchaseOrder, _unitOfWorkAsync, "Put");

            if (!string.IsNullOrWhiteSpace(err))
            {
                return BadRequest(err);
            }

            _objPurchaseOrderService.Update(objPurchaseOrder);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseOrderExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            
            return Updated(objPurchaseOrder);
        }
        // POST: odata/PurchaseOrders
        public async Task<IHttpActionResult> Post(PurchaseOrder objPurchaseOrder)
        {
            objPurchaseOrder.ObjectState = ObjectState.Added;

            var err = GetLiveDbLevelValidation(objPurchaseOrder, _unitOfWorkAsync, "Patch");

            if (!string.IsNullOrWhiteSpace(err))
            {
                return BadRequest(err);
            }

            _objPurchaseOrderService.Insert(objPurchaseOrder);
            try
            {
                
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PurchaseOrderExists(objPurchaseOrder.PONo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = objPurchaseOrder.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = objPurchaseOrder.PONo, ParameterName = "parameter3" },
                new SqlParameter() { Value = objPurchaseOrder.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            return Created(objPurchaseOrder);
        }
        //// PATCH: odata/PurchaseOrders(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PurchaseOrder> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            patch.TryGetPropertyValue("DataView", out var dv);
            PurchaseOrder objPurchaseOrder = await _objPurchaseOrderService.FindAsync(key);
            if (objPurchaseOrder == null)
            {
                return NotFound();
            }
            objPurchaseOrder.ObjectState = ObjectState.Modified;
            patch.Patch(objPurchaseOrder);
            try
            {
                if (dv is List<JsonDataEntity> dataview && dataview.Any())
                {
                    foreach (var entity in dataview)
                    {
                        objPurchaseOrder.DeleteAndAdd(entity);
                    }
                }
                var err = GetLiveDbLevelValidation(objPurchaseOrder, _unitOfWorkAsync, "Patch");

                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseOrderExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            
            return Updated(objPurchaseOrder);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objPurchaseOrder = await _objPurchaseOrderService.FindAsync(key);
            if (objPurchaseOrder == null)
            {
                return NotFound();
            }
            objPurchaseOrder.ObjectState = ObjectState.Deleted;
            _objPurchaseOrderService.Delete(objPurchaseOrder);
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

        private bool PurchaseOrderExists(string poNo)
        {
            return _objPurchaseOrderService.Query(e => e.PONo == poNo).Select().Any();
        }
        private bool PurchaseOrderExists(long key)
        {
            return _objPurchaseOrderService.Query(e => e.Id == key).Select().Any();
        }
        //POST:odata/PurchaseOrders(key)/Logs
        [ODataRoute("PurchaseOrders({key})/Logs")]
        public async Task<IHttpActionResult> PostPurchaseOrderLogs([FromODataUri] long key, [FromBody] PurchaseOrderLog log)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var po = await _objPurchaseOrderService.Queryable().Include(x => x.Logs).FirstOrDefaultAsync(x => x.Id == key);

            if (po == null)
            {
                return NotFound();
            }
            log.PurchaseOrderId = key;
            var uow = Request.GetContext();
            log.ObjectState = ObjectState.Added;
            po.Logs.Add(log);
            po.POValue = po.Logs.Sum(x => x.TotalAmount);
            po.ObjectState = ObjectState.Modified;
            await uow.SaveChangesAsync();
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = po.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = po.PONo, ParameterName = "parameter3" },
                new SqlParameter() { Value = po.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            return Created(log);
        }

        private string GetLiveDbLevelValidation(PurchaseOrder _record, IUnitOfWorkAsync _uow,string _actionId="Post")
        {
            ////
            ///Action 0:Post,1:Put,2:Patch,3:delete
            ////
            try
            {
                var livevalidationerr = _uow.SqlQueryAsync(
                "[dbo].[Proc_GBL_PO_LiveValidationV1]",
                new SqlParameter() { Value = _record.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = _record.PODate, ParameterName = "parameter2" },
                new SqlParameter() { Value = _record.TypeId, ParameterName = "parameter3" },
                new SqlParameter() { Value = _record.UsagePointId, ParameterName = "parameter4" },
                new SqlParameter() { Value = _record.VendorId, ParameterName = "parameter5" },
                new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter6" },
                new SqlParameter() { Value = JsonConvert.SerializeObject(_record), ParameterName = "parameter7" },
                new SqlParameter() { Value = _actionId, ParameterName = "parameter8" }
                ).Result;

                if (livevalidationerr != null && livevalidationerr?.Rows?.Count > 0)
                {
                    return Utilities.To<string>(livevalidationerr.Rows[0]["ErrorMessage"]);
                }
                return "";
            }
            catch (Exception ex)
            {
                return $"Live Validation Error:{ex.GetBaseException().Message}";
            }
        }
    }
}