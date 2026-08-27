
using AutoMapper;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNBillPaymentsController : ODataController
    //ODataController
    {
        private readonly ICNBillPaymentService _repo;
        private ICNBillPaymentLogService _logRepo;
       
        public CNBillPaymentsController(ICNBillPaymentService service, ICNBillPaymentLogService LogService)
        {
            _repo = service;
            _logRepo = LogService;
        }
        // GET: odata/CNBillPayments
        [HttpGet, EnableQuery]
        public IQueryable<CNBillPayment> Get() => _repo.Queryable();

        // GET: odata/CNBillPayments(5)
        [EnableQuery]
        public SingleResult<CNBillPayment> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/CNBillPayments(5)
        public async Task<IHttpActionResult> Put(long key, CNBillPayment payment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != payment.Id)
            {
                return BadRequest();
            }
            payment.ObjectState = ObjectState.Modified;
            _repo.Update(payment);
            await Request.GetContext().SaveChangesAsync();
            return Updated(payment);
        }
        // POST: odata/CNBillPayments
        public async Task<IHttpActionResult> Post(CNBillPayment payment)
        {
            var uow = Request.GetContext();
            payment.ObjectState = ObjectState.Added;
            var logs = payment.BulkLog;
            
            try
            {
                if (!Request.IsBatchRequest())
                {
                    uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                _repo.Insert(payment);
                payment.BulkLog = logs;
                await _repo.PreparePaymentVoucherAsync(payment, uow);
                await uow.SaveChangesAsync();
                if (payment.PaymentLogs != null && payment.PaymentLogs.Count > 0)
                {
                    foreach (var p in payment.PaymentLogs)
                    {
                        if (p.BillLogId.GetValueOrDefault() > 0)
                        {
                            await _logRepo.UpdateBalanceAsync(p.BillLogId);
                        }
                        if (p.OnAccountRefId.GetValueOrDefault() > 0)
                        {
                            await _logRepo.UpdateOnAccountBalanceAsync(p.OnAccountRefId);
                        }
                    }
                }
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
            }
            catch (DbUpdateException)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (CNBillPaymentExists(payment.DocumentNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "DocumentNo should be unique");
                }

                throw;
            }
            return Created(payment);
        }
        //// PATCH: odata/CNBillPayments(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNBillPayment> patch)
        {
            var uow = Request.GetContext();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            patch.TryGetPropertyValue("BulkLog", out var logs);
            CNBillPayment payment = await _repo.FindAsync(key);

           
            if (payment == null)
            {
                return NotFound();
            }
            payment.ObjectState = ObjectState.Modified;
            var voucherid = payment.VoucherId;
            patch.Patch(payment);
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (logs is List<vwBillPaymentLog> list)
                {
                    payment.BulkLog = list;
                }
                payment.VoucherId = voucherid;
                await _repo.PreparePaymentVoucherAsync(payment, uow);
                var bd = await uow.SaveChangesAsync();
                if (payment.PaymentLogs != null && payment.PaymentLogs.Count > 0)
                {
                    foreach (var p in payment.PaymentLogs)
                    {
                        if (p.BillLogId.GetValueOrDefault() > 0)
                        {
                            await _logRepo.UpdateBalanceAsync(p.BillLogId);
                        }
                        if (p.OnAccountRefId.GetValueOrDefault() > 0)
                        {
                            await _logRepo.UpdateOnAccountBalanceAsync(p.OnAccountRefId);
                        }
                    }
                }
                if (voucherid.GetValueOrDefault() > 0 && bd > 0 && payment.VoucherId.GetValueOrDefault() == 0)
                {
                     _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={voucherid}");
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
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }

            return Updated(payment);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var payment = await _repo.Queryable().Include(x=>x.PaymentLogs).Where(x=>x.Id==key).FirstOrDefaultAsync();
            if (payment == null)
            {
                return NotFound();
            }
            payment.ObjectState = ObjectState.Deleted;
           payment.PaymentLogs?.ForEach(x=>x.ObjectState=ObjectState.Deleted);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var logids = payment.PaymentLogs?.Select(p => new { p.BillLogId, p.OnAccountRefId }).ToList();           

                _repo.ExecuteSql($"UPDATE [dbo].[tCNMaster] SET CNAdvanceId=NULL,CnAdvance=0 WHERE CNAdvanceId={payment.Id}");
                _repo.Delete(payment);
                var vchid = payment.VoucherId;
                var bd = await uow.SaveChangesAsync();
                if (vchid.GetValueOrDefault() > 0 && bd > 0)
                {
                    _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={vchid}");                    
                }
                if (logids != null && logids.Count > 0)
                {
                    foreach (var p in logids)
                    {
                        if (p.BillLogId.GetValueOrDefault() > 0)
                        {
                            await _logRepo.UpdateBalanceAsync(p.BillLogId);
                        }
                        if (p.OnAccountRefId.GetValueOrDefault() > 0)
                        {
                            await _logRepo.UpdateOnAccountBalanceAsync(p.OnAccountRefId);
                        }
                    }
                }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing && !Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
        private bool CNBillPaymentExists(string docno) => _repo.Query(e => e.DocumentNo == docno).Select().Any();

        private bool CNBillPaymentExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
       string navigationProperty, [FromBody] Uri link)
        {
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            }
            var payment = _repo.Queryable().SingleOrDefault(p => p.Id == key);
            if (payment == null)
            {
                return NotFound();
            }
            try
            {
                switch (navigationProperty)
                {
                    case "fk_Voucher":
                        long? voucherid = 0;
                        voucherid = payment.VoucherId;
                        payment.fk_Voucher = null;
                        payment.VoucherId = null;
                        var bd = await Request.GetContext().SaveChangesAsync();
                        if (voucherid.GetValueOrDefault() > 0 && bd > 0)
                        {
                            var result = _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={voucherid}");
                            if (result <= 0)
                            {
                                if (!Request.IsBatchRequest())
                                {
                                    Request.GetContext().Rollback();
                                }
                                return BadRequest("Unable to delete previous voucher");
                            }
                        }
                        if (!Request.IsBatchRequest())
                        {
                            Request.GetContext().Commit();
                        }
                        break;

                    default:
                        return StatusCode(HttpStatusCode.NotImplemented);
                }
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
        // POST: odata/CNBills(key)/BillLogs
        [AcceptVerbs("POST")]
        [ODataRoute("CNBillPayments({key})/PaymentLogs")]
        public async Task<IHttpActionResult> PostPaymentLogs([FromODataUri]long key, [FromBody] CNBillPaymentLog log)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var billExists = await _repo.Queryable().Include(x => x.PaymentLogs).FirstOrDefaultAsync(x => x.Id == key);
            if (billExists == null)
            {
                return NotFound();
            }
            
            log.PaymentId = key;
            log.ObjectState = ObjectState.Added;
            if (log.OnAccountRefId > 0)
            {
                var plog =await _logRepo.Queryable().Where(x => x.Id == log.OnAccountRefId).Select(x=>new { x.Amount, DocumentNo=x.fk_Payment.DocumentNo,OnAccountAmount=x.OnAcSettlements.Where(z=>z.Id!=log.Id).Sum(y=>(decimal?)y.Amount)}).FirstOrDefaultAsync();
                if(plog.Amount-plog.OnAccountAmount.GetValueOrDefault()-log.Amount<0)
                //if ((plog.Amount + (plog.fk_VDR?.AgainstReferences?.Sum(x => (decimal?)x.Amount)).GetValueOrDefault()) >= 0)
                {
                    
                    throw new BusinessException(ErrorCode.GLB106,
                        $"One of OnAccount or CN Advance with Doc No {plog.DocumentNo} has already been fully settled");
                }
            }
            //if (log.CNId > 0)
            //{
            //    uow.RepositoryAsync<CNBillLog>().
            //}
            _logRepo.Insert(log);
            await uow.SaveChangesAsync();
            if (log.BillLogId.GetValueOrDefault() > 0)
            {
                await _logRepo.UpdateBalanceAsync(log.BillLogId);
            }
            if (log.OnAccountRefId.GetValueOrDefault() > 0)
            {
                await _logRepo.UpdateOnAccountBalanceAsync(log.OnAccountRefId);
            }
            return Created(log);
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var payment = await _repo.Queryable().AnyAsync(x => x.Id == key);
            if (!payment)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_Voucher":
                    var voucher =
                        await
                            uow.RepositoryAsync<Voucher>().Queryable().AnyAsync(x => x.Id == id);
                    if (!voucher)
                    {
                        return NotFound();
                    }
                    //bill.VoucherId = id;
                    var result = _repo.ExecuteSql($"UPDATE [dbo].[tCNBillPayment] SET [VoucherId]={id} WHERE Id={key}");
                    if (result <= 0)
                    {
                        return BadRequest("Invalid Voucher for Bill");
                    }
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}