using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.FMS.Tyres;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TyreLogsController : ODataController
    //ODataController
    {
        private readonly ITyreLogService _objTyreLogService;

        public TyreLogsController(ITyreLogService service)
        {
            _objTyreLogService = service;
        }
        // GET: odata/TyreLogs
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<TyreLog> Get()
        {
            return _objTyreLogService.Queryable();
        }
        
        // GET: odata/TyreLogs(5)
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<TyreLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objTyreLogService.Queryable().Where(t => t.Id == key));
        }
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<TyreCheck> Getfk_TyreCheck([FromODataUri] long key)
        {
            return SingleResult.Create(_objTyreLogService.Queryable().Include(x => x.fk_TyreCheck.fk_WheelPosition).Select(x=>x.fk_TyreCheck).Where(t => t.Id == key));
        }
        //PUT: odata/TyreLogs(key)/relationName/$ref
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] long key,
        string navigationProperty, [FromBody] Uri link)
        {
            var tyrelog = await _objTyreLogService.Queryable().AnyAsync(x => x.Id == key && x.GatePassId == null);
            if (!tyrelog)
            {
                return NotFound();
            }
            var id= Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_GatePass":
                    await
                            Request.GetContext()
                                .Context.Database.ExecuteSqlCommandAsync(
                                    $"UPDATE dbo.tTyreLog SET GatePassId={id} WHERE Id={key}");
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
        // PUT: odata/TyreLogs(5)
        public async Task<IHttpActionResult> Put(long key, TyreLog objTyreLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTyreLog.Id)
            {
                return BadRequest();
            }
            objTyreLog.ObjectState = ObjectState.Modified;
            _objTyreLogService.Update(objTyreLog);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objTyreLog);
        }
        // POST: odata/TyreLogs
        public async Task<IHttpActionResult> Post(TyreLog objTyreLog)
        {
            objTyreLog.ObjectState = ObjectState.Added;
            _objTyreLogService.Insert(objTyreLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(objTyreLog);
        }
        //// PATCH: odata/TyreLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TyreLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TyreLog objTyreLog = await _objTyreLogService.FindAsync(key);
            if (objTyreLog == null)
            {
                return NotFound();
            }
            objTyreLog.ObjectState = ObjectState.Modified;
            patch.Patch(objTyreLog);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objTyreLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTyreLog = await _objTyreLogService.FindAsync(key);
            if (objTyreLog == null)
            {
                return NotFound();
            }
            objTyreLog.ObjectState = ObjectState.Deleted;
            _objTyreLogService.Delete(objTyreLog);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Actions & Functions
        
        //GET: odata/GetView(5)GetTyreIssueReceiptBill
        /// <exclude />
        [HttpGet, ODataRoute("GetTyrePurchaseBill(key={key})")]
        public vwTyreBillView GetPurchaseView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _objTyreLogService.GetPurchaseBillView(key, 27);
        }
        [HttpGet, ODataRoute("GetTyreChassisBill(key={key})")]
        public vwTyreChassisBill GetTyreChassisView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _objTyreLogService.GetChassisBillView(key);
        }
        [HttpGet, ODataRoute("GetTyreResaleBill(key={key})")]
        public vwTyreBillView GetTyreResaleView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _objTyreLogService.GetTyreResaleBill(key);
        }
        [HttpGet, ODataRoute("GetTyreClaimRemouldBill(key={key})")]
        public vwTyreBillView GetTyreClaimView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _objTyreLogService.GetTyreClaimBillView(key);
        }

        [HttpGet, ODataRoute("GetTyreScrapBill(key={key})")]
        public vwTyreBillView GetTyreScrapBillView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _objTyreLogService.GetTyreScrapBillView(key);
        }

        [HttpGet, ODataRoute("GetTyreStoretransferOutBill(key={key})")]
        public vwTyreBillView GetTyreStoretransferOutBillView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _objTyreLogService.GetTyreStoretransferOutBillView(key);
        }
        [HttpGet, ODataRoute("GetTyreStoretransferInBill(key={key})")]
        public vwTyreBillView GetTyreStoretransferInBillView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _objTyreLogService.GetTyreStoretransferInBillView(key);
        }
        [HttpGet, ODataRoute("GetTyreRejectBill(key={key})")]
        public vwTyreBillView GetTyreRejectView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _objTyreLogService.GetTyreRejectBillView(key);
        }
        [HttpGet, ODataRoute("GetTyreRemouldReceiptBill(key={key})")]
        public vwTyreBillView GetTyreRemouldReceiptBillView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _objTyreLogService.GetTyreRemouldReceiptBillView(key);
        }

        [HttpPost, ODataRoute("PostTyrePurchaseBill")]
        public IHttpActionResult PostPurchaseBillView(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<TyreLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }            

            if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0)
            {
                ModelState.AddModelError("ExpenseLedgerId", "Primary Debit Account is required");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (bill.CurTypeId <= 0 || bill.CurRate <= 0)
            {
                return BadRequest("Currency Type/ CurRate is required");
            }
            //bill.VoucherTypeId = 27;
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            if (bill.VoucherTypeId == 135)/*MRN*/
            {
                try
                {

                    var sei = _objTyreLogService.InsertOrUpdatePurchaseBillMRNView(bill, Request.GetContext());
                    Request.GetContext().SaveChanges();
                    Request.GetContext().Commit();
                    return Ok(sei.Id);
                }
                catch
                {
                    Request.GetContext().Rollback();
                    throw;
                }
            }
            else if (bill.VoucherTypeId == 137)/*MRN bill settlement*/
            {
                try
                {
                    
                    var sei = _objTyreLogService.InsertOrUpdatePurchaseBillMRNSettlementView(bill, Request.GetContext());
                    Request.GetContext().SaveChanges();
                    Request.GetContext().Commit();
                    return Ok(sei.Id);
                }
                catch
                {
                    Request.GetContext().Rollback();
                    throw;
                }
            }
            else {
                try
                {
                    var sei = _objTyreLogService.InsertOrUpdatePurchaseBillView(bill, Request.GetContext());
                    Request.GetContext().SaveChanges();
                    Request.GetContext().Commit();
                    return Ok(sei.Id);
                }
                catch
                {
                    Request.GetContext().Rollback();
                    throw;
                }
            }              
        }

        [HttpPost, ODataRoute("PostTyreClaimRemouldBill")]
        public IHttpActionResult PostClaimBillView(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }

            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<TyreLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //39 Sent for Retreating
            //40 Sent for Claim
            //122 Sent for Repair
            if (bill.VoucherTypeId != 39 && bill.VoucherTypeId != 40 && bill.VoucherTypeId != 122)
            {
                throw new BusinessException(ErrorCode.GLB106, "VoucherType should be either Claim,Remould or Repair.");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateTyreClaim(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();

                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }

        [HttpPost, ODataRoute("PostTyreResaleBill")]
        public IHttpActionResult PostResaleBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<TyreLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }
            if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0)
            {
                ModelState.AddModelError("PrimaryDebitAccountId", "Primary Debit Account is required");
            }
            if (bill.PrimaryCreditAccountId == 0)
            {
                ModelState.AddModelError("PrimaryCreditAccountId", "Primary Credit Account is required");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bill.VoucherTypeId = 28;
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateTyreReSale(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();

                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }
        [HttpPost, ODataRoute("PostTyreIssueReceiptBill")]
        public IHttpActionResult PostTyreIssueReceiptBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<TyreLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bill.VoucherTypeId = 34;
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateTyreIR(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }            
        }
        [HttpPost, ODataRoute("PostTyreIssueBill")]
        public IHttpActionResult PostTyreIssueBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<TyreLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bill.VoucherTypeId = 34;
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateIssue(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }
        [HttpPost, ODataRoute("PostTyreReceiptBill")]
        public IHttpActionResult PostTyreReceiptBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<TyreLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bill.VoucherTypeId = 35;
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateReceipt(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }
        [HttpPost, ODataRoute("PostChasisTyreBill")]
        public async Task<IHttpActionResult> PostChasisBillViewAsync(ODataActionParameters odataParam)
        {
            using (var uow = Request.GetContext())
            {
                var bill = odataParam["bill"] as vwTyreChassisBill;
                if (bill == null)
                {
                    return BadRequest("Invalid Parameter");
                }
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
                try
                {
                    var tei=await _objTyreLogService.InsertUpdateChasisTyreBillAsync(bill, uow);
                    await uow.SaveChangesAsync();
                    uow.Commit();
                    bill.Id = tei.Id;
                    return Ok(bill.Id);
                }
                catch (Exception ex)
                {
                    uow.Rollback();
                    throw;
                }                
            }
        }
        [HttpPost, ODataRoute("PostTyreClaimReceiptBill")]
        public IHttpActionResult InsertUpdateTyreClaimReceiptBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei = _objTyreLogService.InsertUpdateTyreClaimReceiptBillView(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }
        [HttpPost, ODataRoute("PostTyreScrapBill")]
        public IHttpActionResult PostTyreScrapBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateTyreScrap(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }            
        }
        [HttpPost, ODataRoute("PostTyreStocktransferOutBill")]
        public IHttpActionResult PostTyreStocktransferOutBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateTyreStocktransferOutBillView(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }            
        }
        [HttpPost, ODataRoute("PostTyreStocktransferInBill")]
        public IHttpActionResult PostTyreStocktransferInBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateTyreStocktransferInBillView(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }
        [HttpPost, ODataRoute("PostTyreRejectBill")]
        public IHttpActionResult PostTyreRejectBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateTyreReject(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }
        
        [HttpPost, ODataRoute("PostTyreRemouldReceiptBill")]
        public IHttpActionResult PostTyreRemouldReceiptBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateTyreRemouldReceipt(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }
        [HttpPost, ODataRoute("PostTyreClaimSettlementBill")]
        public IHttpActionResult PostTyreClaimSettlementBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwTyreBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                var tei=_objTyreLogService.InsertUpdateTyreClaimSettlement(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
                bill.Id = tei.Id;
                return Ok(bill.Id);
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }
        #endregion

        [ODataRoute("DeleteTyreTransaction"), HttpPost]
        public async Task<IHttpActionResult> DeleteGraph(ODataActionParameters param)
        {
            if (!param.ContainsKey("key"))
            {
                throw new BusinessException(ErrorCode.GLB106, @"Transaction Identification is required.");
            }
            var key = (long)param["key"];
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                /*await _objTyreLogService.DeleteGraphAsync(key, Request.GetContext());*/
                await _objTyreLogService.DeleteBySQLProc(key, Request.GetContext());
                
                await Request.GetContext().SaveChangesAsync();
                Request.GetContext().Commit();
                return Ok();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
        }
    }
}