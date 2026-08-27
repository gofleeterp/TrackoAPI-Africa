using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.FMS.Battery;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class BatteryLogsController : ODataController
    //ODataController
    {
        private readonly IBatteryLogService _repo;

        public BatteryLogsController(IBatteryLogService service)
        {
            _repo = service;
        }
        //PUT: odata/BatteryLogs(key)/relationName/$ref
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var spareLog = await _repo.Queryable().AnyAsync(p => p.Id == key);
            if (!spareLog)
            {
                return NotFound();
            }
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_GatePass":
                    await
                            Request.GetContext()
                                .Context.Database.ExecuteSqlCommandAsync(
                                    $"UPDATE dbo.tBatteryLog SET GatePassId={id} WHERE Id={key}");
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            //await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        
        // GET: odata/BatteryLogs
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<BatteryLog> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/BatteryLogs(5)
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<BatteryLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/BatteryLogs(5)
        public async Task<IHttpActionResult> Put(long key, BatteryLog objBatteryLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objBatteryLog.Id)
            {
                return BadRequest();
            }
            objBatteryLog.ObjectState = ObjectState.Modified;
            objBatteryLog.ConstCurTypeId = Helper.ConstCurTypeId;
            _repo.Update(objBatteryLog);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objBatteryLog);
        }
        // POST: odata/BatteryLogs
        public async Task<IHttpActionResult> Post(BatteryLog objBatteryLog)
        {
            objBatteryLog.ObjectState = ObjectState.Added;
            objBatteryLog.ConstCurTypeId = Helper.ConstCurTypeId;
            _repo.Insert(objBatteryLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(objBatteryLog);
        }
        //// PATCH: odata/BatteryLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<BatteryLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BatteryLog objBatteryLog = await _repo.FindAsync(key);
            if (objBatteryLog == null)
            {
                return NotFound();
            }
            objBatteryLog.ObjectState = ObjectState.Modified;
            patch.Patch(objBatteryLog);
            objBatteryLog.ConstCurTypeId = Helper.ConstCurTypeId;
            await Request.GetContext().SaveChangesAsync();

            return Updated(objBatteryLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objBatteryLog = await _repo.FindAsync(key);
            if (objBatteryLog == null)
            {
                return NotFound();
            }
            objBatteryLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(objBatteryLog);
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
        
        //GET: odata/GetView(5)GetBatteryIssueReceiptBill
        /// <exclude />
        [HttpGet, ODataRoute("GetBatteryPurchaseBill(key={key})")]
        public vwBatteryBillView GetPurchaseView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetPurchaseBillView(key, 43);
        }
        [HttpGet, ODataRoute("GetBatteryChassisBill(key={key})")]
        public vwBatteryChassisBill GetBatteryChassisView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetChassisBillView(key);
        }
        [HttpGet, ODataRoute("GetBatteryResaleBill(key={key})")]
        public vwBatteryBillView GetBatteryResaleView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetBatteryResaleBill(key);
        }
        [HttpGet, ODataRoute("GetBatteryClaimRefurbishBill(key={key})")]
        public vwBatteryBillView GetBatteryClaimView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetBatteryClaimBillView(key);
        }

        [HttpGet, ODataRoute("GetBatteryScrapBill(key={key})")]
        public vwBatteryBillView GetBatteryScrapBillView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetBatteryScrapBillView(key);
        }

        [HttpGet, ODataRoute("GetBatteryStoretransferOutBill(key={key})")]
        public vwBatteryBillView GetBatteryStoretransferOutBillView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetBatteryStoretransferOutBillView(key);
        }
        [HttpGet, ODataRoute("GetBatteryStoretransferInBill(key={key})")]
        public vwBatteryBillView GetBatteryStoretransferInBillView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetBatteryStoretransferInBillView(key);
        }
        [HttpGet, ODataRoute("GetBatteryRejectBill(key={key})")]
        public vwBatteryBillView GetBatteryRejectView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetBatteryRejectBillView(key);
        }
        [HttpGet, ODataRoute("GetBatteryRefurbishReceiptBill(key={key})")]
        public vwBatteryBillView GetBatteryRefurbishReceiptBillView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetBatteryRefurbishReceiptBillView(key);
        }



        [HttpPost, ODataRoute("PostBatteryIssueBill")]
        public IHttpActionResult PostBatteryIssueBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<BatteryLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bill.ConstCurTypeId = Helper.ConstCurTypeId;
            bill.VoucherTypeId = 50;
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                _repo.InsertUpdateIssue(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        [HttpPost, ODataRoute("PostBatteryReceiptBill")]
        public IHttpActionResult PostBatteryReceiptBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<BatteryLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bill.VoucherTypeId = 51;
            bill.ConstCurTypeId = Helper.ConstCurTypeId;
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                _repo.InsertUpdateReceipt(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }


        [HttpPost, ODataRoute("PostBatteryPurchaseBill")]
        public IHttpActionResult PostPurchaseBillView(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.CurTypeId <= 0 || bill.CurRate <= 0)
            {
                return BadRequest("Currency Type/ CurRate is required");
            }
            if (bill.VoucherTypeId.GetValueOrDefault() == 0)
            {
                bill.VoucherTypeId = 43;
            }
            
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<BatteryLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id&&x.VoucherTypeId==bill.VoucherTypeId))
            {
                return NotFound();
            }
            if (bill.CalVat && ((bill.IGSTAmount == 0 && ((bill.CGSTAmount == 0 || bill.SGSTAmount == 0) ? 1 : 0) == 1) || (bill.IGSTAmount > 0 && (bill.CGSTAmount + bill.SGSTAmount) > 0)))
            {
                ModelState.AddModelError("GSTAmount", "GSTAmount Amount is not valid");
            }
            if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0)
            {
                ModelState.AddModelError("ExpenseLedgerId", "Primary Debit Account is required");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);

            if (bill.VoucherTypeId == 136)
            {
                try
                {
                    bill.ConstCurTypeId = Helper.ConstCurTypeId;
                    _repo.InsertOrUpdatePurchaseBillMRNView(bill, Request.GetContext());
                    Request.GetContext().SaveChanges();
                    Request.GetContext().Commit();
                }
                catch (Exception)
                {
                    Request.GetContext().Rollback();
                    throw;
                }
            }
            else if (bill.VoucherTypeId == 138)
            {
                try
                {
                    bill.ConstCurTypeId = Helper.ConstCurTypeId;
                    _repo.InsertOrUpdatePurchaseBillMRNSettlementView(bill, Request.GetContext());
                    Request.GetContext().SaveChanges();
                    Request.GetContext().Commit();
                }
                catch (Exception)
                {
                    Request.GetContext().Rollback();
                    throw;
                }
            }
            else {
                try
                {
                    bill.ConstCurTypeId = Helper.ConstCurTypeId;
                    _repo.InsertOrUpdatePurchaseBillView(bill, Request.GetContext());
                    Request.GetContext().SaveChanges();
                    Request.GetContext().Commit();
                }
                catch (Exception)
                {
                    Request.GetContext().Rollback();
                    throw;
                }
            }
            return Ok();
        }
        [HttpPost, ODataRoute("PostBatteryClaimRefurbishBill")]
        public IHttpActionResult PostClaimBillView(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }

            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<BatteryLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }


            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            //55 Sent for Retreating
            //56 Sent for Claim

            if (bill.VoucherTypeId != 55 && bill.VoucherTypeId != 56)
            {
                throw new BusinessException(ErrorCode.GLB106, "Vouchertype should be either claim or Refurbish.");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateBatteryClaim(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }

        [HttpPost, ODataRoute("PostBatteryResaleBill")]
        public IHttpActionResult PostResaleBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<BatteryLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
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
            bill.VoucherTypeId = 44;
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateBatteryReSale(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        [HttpPost, ODataRoute("PostBatteryIssueReceiptBill")]
        public IHttpActionResult PostBatteryIssueReceiptBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            if (bill.Id > 0 && !Request.GetContext().RepositoryAsync<BatteryLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
            {
                return NotFound();
            }
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            bill.VoucherTypeId = 50;
            bill.ConstCurTypeId = Helper.ConstCurTypeId;
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                _repo.InsertUpdateBatteryIR(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        [HttpPost, ODataRoute("PostChasisBatteryBill")]
        public IHttpActionResult PostChasisBillView(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryChassisBill;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateChasisBatteryBill(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        
        [HttpPost, ODataRoute("PostBatteryScrapBill")]
        public IHttpActionResult PostBatteryScrapBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateBatteryScrap(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        [HttpPost, ODataRoute("PostBatteryStocktransferOutBill")]
        public IHttpActionResult PostBatteryStocktransferOutBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateBatteryStocktransferOutBillView(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        [HttpPost, ODataRoute("PostBatteryStocktransferInBill")]
        public IHttpActionResult PostBatteryStocktransferInBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateBatteryStocktransferInBillView(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        [HttpPost, ODataRoute("PostBatteryRejectBill")]
        public IHttpActionResult PostBatteryRejectBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateBatteryReject(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }

        [HttpPost, ODataRoute("PostBatteryRefurbishReceiptBill")]
        public IHttpActionResult PostBatteryRefurbishReceiptBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateBatteryRefurbishReceipt(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        [HttpPost, ODataRoute("PostBatteryClaimReceiptBill")]
        public IHttpActionResult PostInsertUpdateBatteryClaimReceiptBill(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateBatteryClaimReceiptBillView(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        [HttpPost, ODataRoute("PostBatteryClaimSettlement")]
        public IHttpActionResult PostInsertUpdateBatteryClaimSettlement(ODataActionParameters odataParam)
        {
            var bill = odataParam["bill"] as vwBatteryBillView;
            if (bill == null)
            {
                return BadRequest("Invalid Parameter");
            }
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                bill.ConstCurTypeId = Helper.ConstCurTypeId;
                _repo.InsertUpdateBatteryClaimSettlement(bill, Request.GetContext());
                Request.GetContext().SaveChanges();
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        #endregion

        [ODataRoute("DeleteBatteryTransaction"), HttpPost]
        public IHttpActionResult DeleteGraph(ODataActionParameters param)
        {
            if (!param.ContainsKey("key"))
            {
                throw new BusinessException(ErrorCode.GLB106, @"Transaction Identification is required.");
            }
            var key = (long)param["key"];
            Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                _repo.DeleteGraph(key, Request.GetContext());
                Request.GetContext().SaveChanges();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            Request.GetContext().Commit();
            return Ok();

        }
        
    }
}