using System;
using System.Data;
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
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.FMS.Dues;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class BulkDueLogsController : ODataController
    {
        private readonly IDueTransactionLogService _dtl;
        public BulkDueLogsController(IDueTransactionLogService tla)
        {
            _dtl = tla;
        }
        // GET: odata/BulkDueLogs(5)
        [EnableQuery]
        public SingleResult<vwDueVoucher> Get([FromODataUri] long key)
        {
            if (key == 0)
            {
                return new SingleResult<vwDueVoucher>(null);
            }
            return SingleResult.Create<vwDueVoucher>(_dtl.GetQueryableBulkEntryByKey(key));
        }
        // PUT: odata/BulkDueLogs(5)
        public async Task<IHttpActionResult> Put(long key, vwDueVoucher adv)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                
                if (key != adv.Id)
                {
                    return BadRequest();
                }
                Request.GetContext().BeginTransaction();
                
                var vRepo = Request.GetContext().RepositoryAsync<Voucher>();
                if (!vRepo.Queryable().Any(x => x.Id == key))
                {
                    return NotFound();
                }
                if (adv.Id > 0)
                {
                    _dtl.DeletePrepaidTaxEntry(adv.Id);
                    await Request.GetContext().SaveChangesAsync();
                }
                var voucher =
                    Request.GetContext().RepositoryAsync<Voucher>()
                        .Query(x => x.Id == key && x.VoucherTypeId == 20)
                        .Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences))
                        .Select(x => x)
                        .FirstOrDefault();

                adv.ConstCurTypeId = Helper.ConstCurTypeId;
                voucher.IsCCRequired = true;
                _dtl.BulkDueEntry(adv, voucher);
                await Request.GetContext().SaveChangesAsync();
                var prepaidstatus=await Request.GetContext().RepositoryAsync<ApiConfiguration>().FindAsync("PrePaidTaxStatus");
                if (prepaidstatus != null && prepaidstatus.Value == "1")
                {
                    _dtl.GeneratePrepaidTaxEntry(voucher.Id);
                    await Request.GetContext().SaveChangesAsync();
                }
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Ok();
        }
        // POST: odata/BulkDueLogs
        public async Task<IHttpActionResult> Post(vwDueVoucher adv)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var voucher = new Voucher();
            voucher.IsCCRequired = true;
            try
            {
                Request.GetContext().BeginTransaction(IsolationLevel.Serializable);
                adv.ConstCurTypeId=Helper.ConstCurTypeId;
                voucher = _dtl.BulkDueEntry(adv, voucher);
                await Request.GetContext().SaveChangesAsync();
                var prepaidstatus = await Request.GetContext().RepositoryAsync<ApiConfiguration>().FindAsync("PrePaidTaxStatus");
                if (prepaidstatus != null && prepaidstatus.Value == "1")
                {
                    _dtl.GeneratePrepaidTaxEntry(voucher.Id);
                    await Request.GetContext().SaveChangesAsync();
                }
                Request.GetContext().Commit();
            }
            catch (DbUpdateException ex)
            {
                Request.GetContext().Rollback();
                return BadRequest(ex.Message);
            }
            catch (BusinessException ex)
            {
                Request.GetContext().Rollback();
                throw ex;
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw ex;
            }
            adv.Id = voucher.Id;
            return Created(adv);
        }
        //// PATCH: odata/BulkDueLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [System.Web.Http.AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<vwDueVoucher> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            vwDueVoucher advance;
            try
            {
                Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
                var vRepo = Request.GetContext().RepositoryAsync<Voucher>();
                if (!vRepo.Queryable().Any(x => x.Id == key))
                {
                    return NotFound();
                }
                if (key > 0)
                {
                    _dtl.DeletePrepaidTaxEntry(key);
                    await Request.GetContext().SaveChangesAsync();
                }
                advance = _dtl.GetBulkEntryByKey(key);
                if (advance == null)
                {
                    return NotFound();
                }
                patch.Patch(advance);
                advance.ConstCurTypeId = Helper.ConstCurTypeId;
                var voucher = Request.GetContext().RepositoryAsync<Voucher>().Query(x => x.Id == key&& x.VoucherTypeId == 20).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if(voucher==null)throw new BusinessException(ErrorCode.GLB106,"Voucher No Found");
                voucher.IsCCRequired = true;
                var vch = _dtl.BulkDueEntry(advance, voucher);
                await Request.GetContext().SaveChangesAsync();
                advance.Id = vch.Id;
                var prepaidstatus = await Request.GetContext().RepositoryAsync<ApiConfiguration>().FindAsync("PrePaidTaxStatus");
                if (prepaidstatus != null && prepaidstatus.Value == "1")
                {
                    _dtl.GeneratePrepaidTaxEntry(voucher.Id);
                    await Request.GetContext().SaveChangesAsync();
                }
                Request.GetContext().Commit();
            }
            catch (Exception ex)
            {
                Request.GetContext().Rollback();
                throw;
            }
            return Updated(new vwDueVoucher() {Id = advance.Id});
        }

        //POST: odata/CreatePrepiadTax(5)
        [HttpPost, ODataRoute("CreatePrepiadTax")]
        public async Task<IHttpActionResult> CreatePrepiadTax(ODataActionParameters parameter)
        {
            try
            {
                var key = (long)parameter["key"];
                Request.GetContext().BeginTransaction(IsolationLevel.Serializable);
                _dtl.DeletePrepaidTaxEntry(key);
                await Request.GetContext().SaveChangesAsync();
               var vch= _dtl.GeneratePrepaidTaxEntry(key);
                await Request.GetContext().SaveChangesAsync();
                Request.GetContext().Commit();
                return Ok();

            }
            catch (BusinessException ex)
            {
                Request.GetContext().Rollback();
                throw ex;
            }
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var voucher = Request.GetContext().RepositoryAsync<Voucher>().Query(x => x.Id == key).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
            if (voucher == null)
            {
                return NotFound();
            }
            _dtl.BulkDelete(voucher);
            var count = await Request.GetContext().SaveChangesAsync();
            if (count > 0)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            return BadRequest("No Transaction was Deleted");
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