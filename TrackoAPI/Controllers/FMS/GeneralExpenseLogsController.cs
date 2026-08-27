using System;
using System.Data;
using System.Data.Entity;
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
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class GeneralExpenseLogsController : ODataController
    {
        //Error Group :TADV
        private readonly IGeneralExpenseLogService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public GeneralExpenseLogsController(IUnitOfWorkAsync unitOfWorkAsync, IGeneralExpenseLogService repo)
        {
            _repo = repo;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/GeneralExpenseLogs
        [HttpGet,EnableQuery]
        public IQueryable<GeneralExpenseLog> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/GeneralExpenseLogs(5)
        [EnableQuery]
        public SingleResult<GeneralExpenseLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        
        // PUT: odata/GeneralExpenseLogs(5)
        public async Task<IHttpActionResult> Put(long key, GeneralExpenseLog expense)
        {
            _ = Task.CompletedTask;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != expense.Id)
            {
                return BadRequest();
            }
            if (expense.Amount <= 0) return BadRequest("Expense Amount is Zero which is not allowed.");
            #region Expense Logic
            expense.ObjectState = ObjectState.Modified;
            expense.ConstCurTypeId = Helper.ConstCurTypeId;
            //advance.Amount = advance.FuelQty > 0 ? advance.FuelAmount : advance.CashAmount;
            #endregion
            if (!Request.IsBatchRequest())
            {
                _unitOfWorkAsync.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            try
            {
                if (expense.GenerateVoucher)
                {


                    #region Voucher Logic

                    var voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>()
                        .Query(x => x.Id == expense.VoucherId && x.VoucherTypeId == expense.VoucherTypeId)
                        .Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x)
                        .FirstOrDefault();
                    expense.fK_Voucher = voucher ?? new Voucher();
                    _repo.PrepareV(expense);

                    #endregion

                    #region VoucherDetails Logic

                    _repo.PrepareVD(expense);

                    #endregion

                    #region Voucher Detail Refrence

                    expense.fK_Voucher.VoucherDetails.ForEach(x =>
                        new Action<VoucherDetail, GeneralExpenseLog>(_repo.PrepareVDR).Invoke(x, expense));

                    #endregion

                    #region Validations

                    if (expense.fK_Voucher.VoucherDetails.Count(x => x.ObjectState == ObjectState.Added) <= 1)
                    {
                        return BadRequest(
                            "Atleast two VD are required in Expense Transaction Voucher"); //Atleast two VD are required in Advance Transaction Voucher
                    }


                    #endregion
                }

                _repo.Update(expense);
                await _unitOfWorkAsync.SaveChangesAsync();

                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Commit();
                }
                return Updated(expense);
            }
            catch
            {
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Rollback();
                }

                throw;
            }
        }
        // POST: odata/GeneralExpenseLogs
        public async Task<IHttpActionResult> Post(GeneralExpenseLog expenseLog)
        {
            if (expenseLog.Amount <= 0) return BadRequest("Expense Amount is Zero which is not allowed.");
            if (!Request.IsBatchRequest())
            {
                _unitOfWorkAsync.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            try
            {
                #region General Expense Logic

                expenseLog.ObjectState = ObjectState.Added;

                //advance.Amount = advance.FuelQty > 0 ? advance.FuelAmount : advance.CashAmount;

                #endregion


                if (expenseLog.GenerateVoucher) { 
                    #region Voucher Logic

                    if (expenseLog.VoucherId > 0)
                    {
                        expenseLog.fK_Voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>()
                            .Query(x => x.Id == expenseLog.VoucherId)
                            .Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x)
                            .FirstOrDefault();
                    }

                if (expenseLog.fK_Voucher == null)
                {
                    expenseLog.fK_Voucher = new Voucher { ObjectState = ObjectState.Added };
                }

                _repo.PrepareV(expenseLog);

                #endregion

                #region Voucher Detail Logic

                _repo.PrepareVD(expenseLog);

                #endregion

                #region Voucher Detail Refrence

                expenseLog.fK_Voucher.VoucherDetails.ForEach(x =>
                    new Action<VoucherDetail, GeneralExpenseLog>(_repo.PrepareVDR).Invoke(x, expenseLog));

                #endregion

                #region Validations
                if (expenseLog.fK_Voucher.VoucherDetails.Count(x => x.ObjectState == ObjectState.Added) <= 1)
                {
                    return BadRequest(
                        "Atleast two VD are required in Expense Transaction Voucher"); //Atleast two VD are required in Advance Transaction Voucher
                }

                #endregion
            }
                _repo.Insert(expenseLog);

                await _unitOfWorkAsync.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Commit();
                }

                return Created(expenseLog);
            }
            catch (Exception e)
            {
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Rollback();
                }
                throw;
            }
        }
        //// PATCH: odata/GeneralExpenseLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<GeneralExpenseLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!Request.IsBatchRequest())
            {
                _unitOfWorkAsync.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            try
            {


                var expense = await _repo.Queryable().FirstOrDefaultAsync(x=>x.Id==key);
               
                if (expense == null)
                {
                    return NotFound();
                }

                patch.Patch(expense);
                if (expense.Amount <= 0) return BadRequest("Expense Amount is Zero which is not allowed.");

                #region Expense Logic

                //advance.Amount = advance.FuelQty > 0 ? advance.FuelAmount : advance.CashAmount;
                expense.ObjectState = ObjectState.Modified;

                #endregion
                if (expense.GenerateVoucher)
                {
                    var voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>()
                    .Query(x => x.Id == expense.VoucherId)
                    .Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x)
                    .FirstOrDefault();

                    #region Voucher Logic

                    expense.fK_Voucher = voucher ?? new Voucher();
                    _repo.PrepareV(expense);

                    #endregion

                    #region VoucherDetails Logic

                    _repo.PrepareVD(expense);

                    #endregion

                    #region Voucher Detail Refrence

                    expense.fK_Voucher.VoucherDetails.ForEach(x =>
                        new Action<VoucherDetail, GeneralExpenseLog>(_repo.PrepareVDR).Invoke(x, expense));

                    #endregion

                    #region Validations


                    if (expense.fK_Voucher.VoucherDetails.Count(x => x.ObjectState == ObjectState.Added) <= 1)
                    {
                        return BadRequest(
                            "Atleast two VD are required in Expense Transaction Voucher"); //Atleast two VD are required in Advance Transaction Voucher
                    }

                    #endregion
                }
                await _unitOfWorkAsync.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Commit();
                }
                return Updated(expense);
            }
            catch
            {
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Rollback();
                }
                throw;
            }
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            var expense = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (expense == null)
            {
                return NotFound();
            }
            var uow = _unitOfWorkAsync;
            var anotherkey = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fK_Voucher":                    
                    expense.VoucherId = anotherkey;
                    expense.ObjectState = ObjectState.Modified;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
           
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            if (!Request.IsBatchRequest())
            {
                _unitOfWorkAsync.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            try
            {
                //var objGeneralExpenseL1og = await _repo.FindAsync(key);
                var objGeneralExpenseLog = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
                if (objGeneralExpenseLog == null)
                {
                    return NotFound();
                }
                var vrepo = _unitOfWorkAsync.RepositoryAsync<Voucher>();
                if (await _repo.Queryable().CountAsync(x => x.VoucherId == objGeneralExpenseLog.VoucherId&&x.Id==objGeneralExpenseLog.Id) ==0)
                {
                    var voucher = await
                        vrepo
                            .Queryable()
                            .Include(x => x.VoucherDetails)
                            .FirstOrDefaultAsync(x => x.Id == objGeneralExpenseLog.VoucherId);
                    if (voucher != null)
                    {
                        objGeneralExpenseLog.fK_Voucher = voucher;
                    }

                    objGeneralExpenseLog.fK_Voucher.ObjectState = ObjectState.Deleted;
                    objGeneralExpenseLog.fK_Voucher.VoucherDetails.ForEach(x => x.ObjectState = ObjectState.Deleted);
                    objGeneralExpenseLog.fK_Voucher.VoucherDetails.ForEach(x =>
                        x.VoucherDetailReferences.ForEach(y => y.ObjectState = ObjectState.Deleted));
                }
                objGeneralExpenseLog.ObjectState = ObjectState.Deleted;
                _repo.Delete(objGeneralExpenseLog);

                await _unitOfWorkAsync.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch
            {
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Rollback();
                }
                throw;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}