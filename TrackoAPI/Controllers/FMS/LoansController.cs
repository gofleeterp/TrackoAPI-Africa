using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS.Loan;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using System.Web.OData.Routing;
using System.Data.Entity;
using System.Net.Http;
using EntityFramework.Extensions;
using TrackoApi.Models.AMS;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class LoansController : ODataController
    //ODataController
    {
        private readonly ILoanService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public LoansController(IUnitOfWorkAsync unitOfWorkAsync, ILoanService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/Loans
        [HttpGet, EnableQuery]
        public IQueryable<Loan> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/Loans(5)
        [EnableQuery]
        public SingleResult<Loan> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/Loans(5)
        public async Task<IHttpActionResult> Put(long key, Loan objLoan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objLoan.Id)
            {
                return BadRequest();
            }
            objLoan.ObjectState = ObjectState.Modified;
            _repo.Update(objLoan);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLoan);
        }
        // POST: odata/Loans
        public async Task<IHttpActionResult> Post(Loan objLoan)
        {
            objLoan.ObjectState = ObjectState.Added;
            _repo.Insert(objLoan);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LoanExists(objLoan.LoanNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objLoan);
        }
        //// PATCH: odata/Loans(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<Loan> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Loan objLoan = await _repo.FindAsync(key);
            if (objLoan == null)
            {
                return NotFound();
            }
            objLoan.ObjectState = ObjectState.Modified;
            patch.Patch(objLoan);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLoan);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objLoan = await _repo.FindAsync(key);
            if (objLoan == null)
            {
                return NotFound();
            }
            objLoan.ObjectState = ObjectState.Deleted;
            _repo.Delete(objLoan);
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

        private bool LoanExists(string loanNo)
        {
            return _repo.Query(e => e.LoanNo == loanNo).Select().Any();
        }
        private bool LoanExists(long key)
        {
            return _repo.Query(e => e.Id == key).Select().Any();
        }
        //POST:odata/Loans(key)/Logs
        [ODataRoute("Loans({key})/Logs")]
        public async Task<IHttpActionResult> PostLoanLogs([FromODataUri] long key, [FromBody] LoanLog log)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var loan = await _repo.Queryable().Include(x => x.Logs).FirstOrDefaultAsync(x => x.Id == key);

            if (loan == null)
            {
                return NotFound();
            }
            log.LoanId = key;
            var uow = Request.GetContext();
            log.ObjectState = ObjectState.Added;
            loan.Logs.Add(log);
            loan.InstallmentAmount = loan.Logs.Sum(x => x.InstallmentAmount);
            loan.PrincipalAmount= loan.Logs.Sum(x => x.PrincipalAmount);
            loan.InterestAmount = loan.Logs.Sum(x => x.InterestAmount);
            loan.NoofInstallment = loan.Logs.Count();
            loan.ObjectState = ObjectState.Modified;
            await uow.SaveChangesAsync();
            return Created(log);
        }
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
            string navigationProperty, [FromBody] Uri link)
        {
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
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
                    case "fk_Voucher":
                        long? voucherid = 0;
                        voucherid = loan.VoucherId;
                        loan.fk_Voucher = null;
                        loan.VoucherId = null;
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

                        break;
                    default:
                        return StatusCode(HttpStatusCode.NotImplemented);
                }
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Commit();
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
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
            string navigationProperty, [FromBody] Uri link)
        {
            var bill = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (bill == null)
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
                    var result =await _repo.Queryable().Where(x=>x.Id==key).UpdateAsync(x=>new Loan()
                    {
                        VoucherId = id
                    });
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