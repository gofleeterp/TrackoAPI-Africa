using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS.Loan;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class LoanLogsController : ODataController
    //ODataController
    {
        private readonly ILoanLogService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public LoanLogsController(IUnitOfWorkAsync unitOfWorkAsync, ILoanLogService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/LoanLogs
        [HttpGet, EnableQuery]
        public IQueryable<LoanLog> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/LoanLogs(5)
        [EnableQuery]
        public SingleResult<LoanLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/LoanLogs(5)
        public async Task<IHttpActionResult> Put(long key, LoanLog objLoanLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objLoanLog.Id)
            {
                return BadRequest();
            }
            objLoanLog.ObjectState = ObjectState.Modified;
            _repo.Update(objLoanLog);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLoanLog);
        }
        // POST: odata/LoanLogs
        public async Task<IHttpActionResult> Post(LoanLog objLoanLog)
        {
            objLoanLog.ObjectState = ObjectState.Added;
            _repo.Insert(objLoanLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                //if (LoanLogExists(objLoanLog.PONo))
                //{
                //    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                //}
                throw;
            }
            return Created(objLoanLog);
        }
        //// PATCH: odata/LoanLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<LoanLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            LoanLog objLoanLog = await _repo.FindAsync(key);
            if (objLoanLog == null)
            {
                return NotFound();
            }
            objLoanLog.ObjectState = ObjectState.Modified;
            patch.Patch(objLoanLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLoanLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objLoanLog = await _repo.FindAsync(key);
            if (objLoanLog == null)
            {
                return NotFound();
            }
            objLoanLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(objLoanLog);
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

        //private bool LoanLogExists(string poNo)
        //{
        //    return _repo.Query(e => e.PONo == poNo).Select().Any();
        //}
        private bool LoanLogExists(long key)
        {
            return _repo.Query(e => e.Id == key).Select().Any();
        }
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
            string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            var loan = _repo.Queryable().SingleOrDefault(p => p.Id == key);
            if (loan == null)
            {
                return NotFound();
            }
            try
            {
                switch (navigationProperty)
                {
                    case "fk_VDR":
                        loan.fk_VDR = null;
                        loan.VDRId = null;
                        //loan.LoanVoucherId = null;
                        loan.ObjectState = ObjectState.Modified;
                        break;
                    case "fk_LoanVoucher":
                        loan.fk_LoanVoucher = null;
                        loan.LoanVoucherId = null;
                        loan.ObjectState = ObjectState.Modified;
                        break;
                    case "fk_RepVoucher":
                        loan.fk_RepVoucher = null;
                        loan.RepVoucherId = null;
                        loan.RepDate = null;
                        loan.RepayVoucherNo = null;
                        loan.ObjectState = ObjectState.Modified;
                        break;
                    default:
                        return StatusCode(HttpStatusCode.NotImplemented);
                }
                await uow.SaveChangesAsync();
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
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
            string navigationProperty, [FromBody] Uri link)
        {
            var loan = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (loan == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var id = Request.GetKeyFromUri<long>(link);
                switch (navigationProperty)
                {
                    case "fk_VDR":
                        var vdr =
                            await
                                uow.RepositoryAsync<VoucherDetailReference>().Queryable().Where(x => x.Id == id).Select(
                                    x =>
                                   new {
                                        x.Id,
                                        x.fk_VoucherDetail.VoucherId
                                    }).FirstOrDefaultAsync();
                        if (vdr==null)
                        {
                            return NotFound();
                        }
                        //bill.VoucherId = id;
                        loan.VDRId = id;
                        loan.LoanVoucherId = vdr.VoucherId;
                        loan.ObjectState=ObjectState.Modified;
                        var result = await uow.SaveChangesAsync();
                        if (result <= 0)
                        {
                            return BadRequest("Invalid Voucher for Bill");
                        }
                        break;

                    case "fk_LoanVoucher":
                        var vdr1 =
                            await
                                uow.RepositoryAsync<Voucher>().Queryable().AnyAsync(x => x.Id == id);
                        if (!vdr1)
                        {
                            return NotFound();
                        }
                        //bill.VoucherId = id;
                        loan.LoanVoucherId = id;
                        loan.ObjectState = ObjectState.Modified;
                        var result1 = await uow.SaveChangesAsync();
                        if (result1 <= 0)
                        {
                            return BadRequest("Invalid Voucher for Bill");
                        }
                        break;
                    case "fk_RepVoucher":
                        var vdr2 =
                            await
                                uow.RepositoryAsync<Voucher>().Queryable().FirstOrDefaultAsync(x => x.Id == id);
                        if (vdr2==null)
                        {
                            return NotFound();
                        }
                        //bill.VoucherId = id;
                        loan.RepVoucherId = id;
                        loan.RepayVoucherNo = vdr2.VoucherNo;
                        loan.RepAmount = loan.InstallmentAmount;
                        loan.RepDate = vdr2.VoucherDate;
                        loan.ObjectState = ObjectState.Modified;
                        var result2 = await uow.SaveChangesAsync();
                        if (result2 <= 0)
                        {
                            return BadRequest("Invalid Voucher for Bill");
                        }
                        break;
                    
                    default:
                        return StatusCode(HttpStatusCode.NotImplemented);
                }
                await uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
            }
            catch (Exception e)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}