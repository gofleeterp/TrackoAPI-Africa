//using HibernatingRhinos.Profiler.Appender.ProfiledDataAccess;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class BulkGeneralExpenseInsertController : ODataController
    {
        private readonly IGeneralExpenseLogService _tla;

        public BulkGeneralExpenseInsertController(IGeneralExpenseLogService tla, IUnitOfWorkAsync uow)
        {
            _tla = tla;
        }

        [HttpPost]
        public IHttpActionResult BulkPost(ODataActionParameters parameters)
        {
            var ivouchers = parameters["vouchers"] as IEnumerator<vwGeneralExpenseVoucher>;
            if (ivouchers == null) return BadRequest("No Records found to upload");
            var vouchers = ivouchers.ToList();
            var uow = Request.GetContext();
            _tla.Request = this.Request;
            var transaction = uow.Context.Database.CurrentTransaction ??
                                  uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                //#if !DEBUG
                _tla.BatchInsert(vouchers, transaction.UnderlyingTransaction);
                //#elif DEBUG
                //_tla.BatchInsert(vouchers, transaction.UnderlyingTransaction is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction);
                //#endif

                if (!Request.IsBatchRequest())
                {
                    transaction.Commit();
                    transaction.Dispose();
                }
                string batchids = vouchers.Select(x => x.BatchId).Aggregate(string.Empty, (current, batchid) => current + ((string.IsNullOrWhiteSpace(current) ? "" : "^") + batchid));
                var item = new vwBatch { BatchId = batchids, BatchSize = vouchers.Count };
                return Ok(item);
            }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    transaction.Rollback();
                    transaction.Dispose();
                }
                throw;
            }
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            try
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
                }
                var voucher = Request.GetContext().RepositoryAsync<Voucher>().Query(x => x.Id == key).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (voucher == null)
                {
                    return NotFound();
                }
                _tla.BulkDelete(voucher);
                await Request.GetContext().SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw;
            }
        }

        // GET: odata/BulkGeneralExpenseInserts(5)
        [EnableQuery]
        public SingleResult<vwGeneralExpenseVoucher> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_tla.GetQueryableBulkEntryByKey(key));
        }

        //// PATCH: odata/GeneralExpenseLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [System.Web.Http.AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<vwGeneralExpenseVoucher> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            vwGeneralExpenseVoucher advance;
            try
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
                }
                advance = _tla.GetBulkEntryByKey(key);
                if (advance == null)
                {
                    return NotFound();
                }
                patch.Patch(advance);
                if (advance.GeneralExpenseLogs.Any(x => x.Amount <= 0))
                    return BadRequest("One of Advance Amount is Zero which is not allowed.");
                var voucher = Request.GetContext().RepositoryAsync<Voucher>().Query(x => x.Id == key && x.VoucherTypeId == 60).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                advance.ConstCurTypeId = Helper.ConstCurTypeId;
                var vch = _tla.BulkGeneralExpense(advance, voucher);
                await Request.GetContext().SaveChangesAsync();
                advance.Id = vch.Id;
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().Commit();
            }
            return Updated(new vwGeneralExpenseVoucher() { Id = advance.Id });
        }

        // POST: odata/GeneralExpenseLogs
        public async Task<IHttpActionResult> Post(vwGeneralExpenseVoucher adv)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (adv.GeneralExpenseLogs.Any(x => x.Amount <= 0))
                return BadRequest("One of Advance Amount is Zero which is not allowed.");
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            }
            var voucher = Request.GetContext().RepositoryAsync<Voucher>().Query(x => x.Id == adv.Id && x.VoucherTypeId == 60).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
            adv.ConstCurTypeId = Helper.ConstCurTypeId;
            var vch = _tla.BulkGeneralExpense(adv, voucher);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw;
            }
            adv.Id = vch.Id;
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().Commit();
            }
            return Created(adv);
        }

        // PUT: odata/GeneralExpenseLogs(5)
        public async Task<IHttpActionResult> Put(long key, vwGeneralExpenseVoucher adv)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != adv.Id)
            {
                return BadRequest();
            }
            if (adv.GeneralExpenseLogs.Any(x => x.Amount <= 0))
                return BadRequest("One of Advance Amount is Zero which is not allowed.");
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            }
            var voucher = Request.GetContext().RepositoryAsync<Voucher>().Query(x => x.Id == key && x.VoucherTypeId == 60).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
            adv.ConstCurTypeId = Helper.ConstCurTypeId;
            _tla.BulkGeneralExpense(adv, voucher);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Commit();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().Commit();
            }
            return Ok();
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
    }
}