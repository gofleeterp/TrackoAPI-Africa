using Hangfire;
using Hangfire.Storage.Monitoring;

using Microsoft.VisualBasic.Logging;
using Microsoft.VisualStudio.Services.Common;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.AMS;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.vw.ts;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx,EnableQuery(MaxNodeCount =5000)]
    public class TripAdvanceLogsController : ODataController
    {
        //Error Group :TADV
        private readonly ITripAdvanceLogService _tripAdvanceLogService;

        
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private int _AdvanceLoanAdjustmentNatureId;
        public TripAdvanceLogsController(IUnitOfWorkAsync unitOfWorkAsync, ITripAdvanceLogService advanceService)
        {
            _tripAdvanceLogService = advanceService;
            _unitOfWorkAsync = unitOfWorkAsync;
            _AdvanceLoanAdjustmentNatureId = _tripAdvanceLogService.GetConfigValue<int>("AdvanceLoanAdjustmentNatureId");
        }

        // GET: odata/TripAdvanceLogs
        [HttpGet, EnableQuery(MaxNodeCount = 5000)]
        public IQueryable<TripAdvanceLog> Get()
        {
            return _tripAdvanceLogService.Queryable();
        }

        // GET: odata/TripAdvanceLogs(5)
        [EnableQuery]
        public SingleResult<TripAdvanceLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_tripAdvanceLogService.Queryable().Where(t => t.Id == key));
        }

        [HttpGet, EnableQuery]
        public IQueryable<TripAdvanceLog> GetUnsettledAdvances()
        {
            var result = from t in _tripAdvanceLogService.Queryable()
                         join s in _tripAdvanceLogService.Queryable().DefaultIfEmpty()
                         on t.Id equals s.SettledRefId
                         into sg
                         where sg.Sum(x => (x.CashAmount + x.FuelAmount) - (t.CashAmount + t.FuelAmount)) > 0
                         select t;
            return result;
        }

        [EnableQuery]
        public IQueryable<TripFuelExpense> GetFuelExpanses(long? settlementId, string tripLogIds = null)
        {
            if (!settlementId.HasValue && string.IsNullOrWhiteSpace(tripLogIds))
            {
                return null;
            }
            var data = _tripAdvanceLogService.FuelExpanses(settlementId, tripLogIds).SelectMany(x => x.FuelExpanses, ((a,
                   l) => new TripFuelExpense
                   {
                       Id = l.Id,
                       RefNo = a.ReferenceNo,
                       SettlementId = l.SettlementId,
                       AdvanceId = a.Id,
                       TriplogId = l.TripLogId,
                       UsedQty = l.FuelQty,
                       CrAccountId = a.CreditAccountId.GetValueOrDefault(0),
                       ShortageQty = l.ShortFuelQty,
                       TypeId = l.ExpenseTypeId,
                       Date = a.AdvanceDate,
                       AdavnceTypeId = a.AdvanceTypeId.GetValueOrDefault(0),
                       BalanceQty = a.BalanceQty,
                       CrAccount = a.fk_CreditAccount.AccountName,
                       Description = l.Remarks,
                       Driver = a.fk_Driver.DriverName,
                       FuelType = a.fk_FuelType.Name,
                       FuelTypeId = a.FuelId.GetValueOrDefault(0),
                       IsDeletedId = false,
                       Rate = a.FuelRate,
                       ShortageFuelAmt = l.ShortFuelAmt,
                       TotalFuelAmt = a.Amount,
                       TotalQty = a.FuelQty,
                       UsedFuelAmt = l.SettledAmount,
                   }));
            return data;
        }

        // PUT: odata/TripAdvanceLogs(5)
        private string GetLiveDbLevelValidation(TripAdvanceLog _record, IUnitOfWorkAsync _uow,int _action)
        {
            var obj = new
            {
                _record.DebitAccountId,
                _record.CreditAccountId,
                _record.TripLogId,
                _record.VoucherDate,
                _record.ThirdPartyRefNo,
                _record.SettlementId,
                _record.SettledRefId,
                _record.VDRId,
                _record.RequestStatusId,
                _record.RequestDate,
                _record.RequestAmount,
                _record.Ref1Id,
                _record.VoucherId,
                _record.BalanceQty,
                _record.BatchId,
                _record.DriverId,
                _record.HireVehicleId,
                _record.OfficeId,
                _record.FuelId,
                _record.FuelRate,
                _record.ReferenceNo,
                _record.ViewId,
                _record.ExpenseId,
                _record.ParentAdvanceLogId,
                _record.Ref1,
                _record.CurTypeId,
                _record.CurRate,
                ActionId=_action
            };

            var livevalidationerr = _uow.SqlQueryAsync(
            "[dbo].[Proc_GBL_TAL_LiveValidationV1]",
            new SqlParameter() { Value = _record.Id, ParameterName = "parameter1" }/*advanceid*/,
            new SqlParameter() { Value = _record.FuelQty, ParameterName = "parameter2" }/*fuelqty*/,
            new SqlParameter() { Value = _record.VehicleId, ParameterName = "parameter3" }/*vehicleid*/,
            new SqlParameter() { Value = _record.AdvanceTypeId, ParameterName = "parameter4" }/*AdvanceTypeId*/,
            new SqlParameter() { Value = _record.CashAmount, ParameterName = "parameter5" }/*CashAmount*/,
            new SqlParameter() { Value = _record.RequestQty, ParameterName = "parameter6" }/*RequestQty*/,
            new SqlParameter() { Value = _record.AdvanceDate, ParameterName = "parameter7" }/*AdvanceDate*/,
            new SqlParameter() { Value = _record.PaidInId, ParameterName = "parameter8" }/*PaidInId*/,
            new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter9" }/*SessionId*/,
            new SqlParameter() { Value = _record.FuelAmount, ParameterName = "parameter10" }/*FuelAmount*/,
            new SqlParameter() { Value = JsonConvert.SerializeObject(obj), ParameterName = "parameter11" }/*model*/
            ).Result;

            if (livevalidationerr != null && livevalidationerr?.Rows?.Count > 0)
            {
                return Utilities.To<string>(livevalidationerr.Rows[0]["ErrorMessage"]);
            }
            return "";
        }
        private string SaveOtherDetails(TripAdvanceLog _record, IUnitOfWorkAsync _uow)
        {
            var err = _uow.SqlQueryAsync(
            "[dbo].[Proc_GBL_TAL_OtherAction]",
            new SqlParameter() { Value = _record.Id, ParameterName = "parameter1" }/*advanceid*/,
            new SqlParameter() { Value = _record.VoucherNo, ParameterName = "parameter2" }/*VoucherNo*/,
            new SqlParameter() { Value = _record.AdvanceTypeId, ParameterName = "parameter3" }/*AdvanceTypeId*/,
            new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter4" }/*SessionId*/
            ).Result;

            if (err != null && err?.Rows?.Count > 0)
            {
                return Utilities.To<string>(err.Rows[0]["ErrorMessage"]);
            }
            return "";
        }
        
        public async Task<IHttpActionResult> Put(long key, TripAdvanceLog advance)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != advance.Id)
            {
                return BadRequest();
            }
            
            try
            {
                var uow = Request.GetContext();

                #region Loan Adjustmentlogic
                List<FakeVDRs> loanvdrs = new List<FakeVDRs>();
                if (advance.AdvanceTypeId == 1)
                {
                    var jdata = advance.DataView.Where(x => x.DataName == "LoanAdvanceAjustment1107").FirstOrDefault();
                    if (jdata != null)
                    {
                        loanvdrs = JsonConvert.DeserializeObject<List<FakeVDRs>>(jdata.RawJson);
                    }
                    if (loanvdrs.Any(x => x.AVDRId.GetValueOrDefault() <= 0))
                    {
                        throw new BusinessException(ErrorCode.TADV100, $"HINT:Invalid Loan Entry selected.AVDRId cannot be null");
                    }
                    if (loanvdrs.Any())
                    {
                        if (_AdvanceLoanAdjustmentNatureId <= 0)
                        {
                            throw new BusinessException(ErrorCode.TADV100, $"HINT:Advance Loan Adjustment Nature is not configured");
                        }

                        advance.IsAutoAPRL = true;
                        advance.RequestStatusId = 1597;
                        advance.CashAmount = advance.RequestAmount;
                        advance.LoanAdjusted = loanvdrs.Sum(x => x.Adjusted);
                        advance.PaidAmount = advance.CashAmount - advance.LoanAdjusted;
                        advance.Ref1Id = _AdvanceLoanAdjustmentNatureId;
                    }
                }
                #endregion

                if ((advance.AdvanceTypeId==1|| advance.AdvanceTypeId == 2 || advance.AdvanceTypeId == 110 || advance.AdvanceTypeId == 112) &&advance.TripLogId > 0 && await uow.RepositoryAsync<VehicleMovementLog>().Queryable().AnyAsync(x => x.Id == advance.TripLogId && x.SettlementId > 0).ConfigureAwait(true))
                {
                    return BadRequest("Advance cannot be paid against Settled Trip");
                }
                
                var err = GetLiveDbLevelValidation(advance, uow,1/*Update*/);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }

                if (!Request.IsBatchRequest())
                {
                    uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }

                if (advance.Amount <= 0 && advance.RequestStatusId != 1598 &&
                    advance.RequestStatusId != 1596 /*New Request*/
                ) return BadRequest("Advance Amount is Zero which is not allowed.");

                if (advance.Amount <= 0 && (advance.RequestAmount <= 0 && advance.RequestQty <= 0) && advance.RequestStatusId != 1598)
                    return BadRequest("Either Advance Amount or Request Amount or Request Qty should be greater than zero.");
                await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET VDRId=NULL WHERE Id={key}");

                #region Advance Logic

                advance.ObjectState = ObjectState.Modified;
                //TODO:Fuel Rate should be fetched from Database
                if (advance.AdvanceTypeId == 2 || advance.AdvanceTypeId == 3)
                {
                    if (advance.FuelAmount <= 0 && advance.RequestStatusId.GetValueOrDefault(1597) == 1597)
                    {
                        advance.FuelAmount = Math.Round(advance.FuelQty * advance.FuelRate, 2);
                    }
                    if (advance.FuelRate <= 0 && advance.FuelAmount > 0 && advance.FuelQty > 0)
                    {
                        advance.FuelRate = Math.Round(advance.FuelAmount / advance.FuelQty, 2);
                    }
                }
                //advance.Amount = advance.FuelQty > 0 ? advance.FuelAmount : advance.CashAmount;
                if ((advance.IGSTAmt + advance.CGSTAmt + advance.SGSTAmt) == 0)
                {
                    advance.BasicAmt = advance.Amount;
                }
                #endregion Advance Logic
                

                if (advance.RequestStatusId != 1596 && advance.RequestStatusId != 1598 /*New Request*/)
                {
                    #region Voucher Logic
                    var voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>()
                        .Query(x => ((x.Id == advance.VoucherId) || (x.VoucherNo == advance.VoucherNo)) && x.VoucherTypeId == advance.AdvanceTypeId)
                        .Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x)
                        .FirstOrDefault();

                    advance.fk_Voucher = voucher ?? new Voucher();
                    advance.ConstCurTypeId = Helper.ConstCurTypeId;

                    if (loanvdrs.Any())
                    {
                        advance.fk_Voucher.Account7Id = advance.DriverId;
                        advance.fk_Voucher.Amount7 = -advance.LoanAdjusted;
                    }
                    _tripAdvanceLogService.PrepareV(advance);

                    #endregion Voucher Logic

                    #region VoucherDetails Logic

                    _tripAdvanceLogService.PrepareVD(advance);

                    #endregion VoucherDetails Logic

                    #region Voucher Detail Refrence

                    advance.fk_Voucher.VoucherDetails.ForEach(x =>
                        new Action<VoucherDetail, TripAdvanceLog,List<FakeVDRs>>(_tripAdvanceLogService.PrepareVDR)
                            .Invoke(x, advance,loanvdrs));

                    #endregion Voucher Detail Refrence

                    #region Voucher Validations
                    if (!(advance.AdvanceTypeId == 110 || advance.AdvanceTypeId == 111))
                    {
                        if (advance.fk_Voucher.VoucherDetails.Sum(x => x.Amount) != 0)
                        {
                            throw new BusinessException(ErrorCode.TADV100, $"HINT:VD_SUM:{advance.fk_Voucher.VoucherDetails.Sum(x => x.Amount)},Amt1:{advance.fk_Voucher.Amount1}, Amt2:{advance.fk_Voucher.Amount2}, AdvAmt:{advance.Amount}");
                        }
                    }
                    else if (advance.fk_Voucher.VoucherDetails.Sum(x => x.Amount) != 0)
                    {
                        throw new BusinessException(ErrorCode.TADV100, $"HINT:VD_SUM:{advance.fk_Voucher.VoucherDetails.Sum(x => x.Amount)}");
                    }

                    if (advance.fk_Voucher.VoucherDetails.Count(x => x.ObjectState == ObjectState.Added) <= 1)
                    {
                        throw new BusinessException(ErrorCode.TADV101,$"VDCount:{advance.fk_Voucher.VoucherDetails.Count}");//Atleast two VD are required in Advance Transaction Voucher
                    }

                    //if (advance.fk_Voucher.VoucherDetails.Count(x =>
                    //        x.VoucherDetailReferences.Count != 0 && x.ObjectState == ObjectState.Added) == 0)
                    //{
                    //    return BadRequest("TADV102"); //Atlead one VDR is Required in Advance Transaction
                    //}

                    //foreach (var voucherDetail in advance.fk_Voucher.VoucherDetails.Where(voucherDetail =>
                    //    voucherDetail.VoucherDetailReferences.Count > 0 && voucherDetail.Amount !=
                    //    voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)))
                    //{
                    //    throw new BusinessException(ErrorCode.TADV103, $"VDAmount:{voucherDetail.Amount}, VDRSum:{voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)}"); //VD and VDR Amount Doesn't Tally");
                    //}

                    #endregion Voucher Validations

                }

                if (advance.RequestStatusId == 1596 /*Reject Request*/ ||
                    advance.RequestStatusId == 1598 /*New Request*/)
                {
                    var telRepo = _unitOfWorkAsync.RepositoryAsync<TripExpenseLog>();
                    var existingExpD = telRepo.Queryable().FirstOrDefault(x => x.IsAuto && x.TripAdvanceLogId == key);
                    if (existingExpD != null)
                    {
                        existingExpD.ObjectState = ObjectState.Deleted;
                    }
                    var voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>().Queryable().FirstOrDefault(x =>
                        x.Id == advance.VoucherId && x.VoucherTypeId == advance.AdvanceTypeId);
                    if (voucher != null)
                    {
                        voucher.ObjectState = ObjectState.Deleted;
                    }
                    advance.VoucherId = null;
                    advance.fk_Voucher = null;
                }

                /*Loan*/
                if (advance.RequestStatusId == 1598 /*Reject Request*/)
                { 
                    var voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>().Queryable().FirstOrDefault(x =>
                        x.ReferenceTransactionId == advance.Id && x.VoucherTypeId == 134);
                    if (voucher != null)
                    {
                        voucher.ObjectState = ObjectState.Deleted;
                    }
                    await _unitOfWorkAsync.SaveChangesAsync();
                }
                
                _tripAdvanceLogService.Update(advance);
                await _unitOfWorkAsync.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Commit();
                }
                /*def stock recalcl*/
                if (advance.AdvanceTypeId == 112 && advance.SettledRefId.GetValueOrDefault() > 0)
                {
                    await
                        _unitOfWorkAsync.ExecSqlQueryAsync(
                            $"update b set b.BalanceQty=b.FuelQty-isnull(isqty.IssuedDef,0) from dbo.tTripAdvanceLog b left join (select i.SettledRefId,IssuedDef=sum(i.FuelQty) from dbo.tTripAdvanceLog as i group by i.SettledRefId)as isqty on isqty.SettledRefId=b.Id where b.Id={advance.SettledRefId}");

                }
                var lnerr = SaveOtherDetails(advance, uow);
                if (!string.IsNullOrWhiteSpace(lnerr))
                {
                    return BadRequest(lnerr);
                }
               
                return Updated(advance);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Rollback();
                }
                throw;
            }
        }

        // POST: odata/TripAdvanceLogs
        public async Task<IHttpActionResult> Post(TripAdvanceLog advance)
        {
            try
            {
                if ((advance.AdvanceTypeId == 1 || advance.AdvanceTypeId == 2 || advance.AdvanceTypeId == 110 || advance.AdvanceTypeId == 112) && advance.TripLogId > 0 && await _unitOfWorkAsync.RepositoryAsync<VehicleMovementLog>().Queryable().AnyAsync(x => x.Id == advance.TripLogId && x.SettlementId > 0).ConfigureAwait(true))
                {
                    return BadRequest("Advance cannot be paid against Settled Trip");
                }
                
                if(await _tripAdvanceLogService.Queryable().AnyAsync(x => x.VoucherNo == advance.VoucherNo&&x.CreditAccountId!=advance.CreditAccountId))
                {
                    return BadRequest("Advance Number is duplicate");
                }

                if (advance.DriverId>0&&await _unitOfWorkAsync.RepositoryAsync<DriverMaster>().Queryable().AnyAsync(x => x.Id == advance.DriverId && x.Status!=Models.Shared.MasterStatus.Active))
                {
                    return BadRequest("Advance can only be paid to Active Drivers.");
                }

                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.BeginTransaction(IsolationLevel.ReadCommitted);
                }


                if (advance.AdvanceTypeId == 76 /*HS Payment Require HireSlip Reference*/&& advance.TripLogId.GetValueOrDefault() == 0)
                {
                    return BadRequest("Hire Vehicle payment require HireSlip Number. If you are intend to pay without Hireslip please choose OnAccount as Advance Type.");
                }
                if (advance.Amount <= 0 && (advance.RequestStatusId != 1596/*New Request*/ && advance.RequestStatusId != 1598/*Reject Request*/)) return BadRequest("Advance Amount is Zero which is not allowed.");
                if (advance.Amount <= 0 && (advance.RequestAmount <= 0 && advance.RequestQty <= 0) &&
                    advance.RequestStatusId != 1598)
                {
                    return BadRequest("Either Advance Amount or Request Amount or Request Qty should be greater than zero.");
                }

                #region Advance Logic
                advance.ObjectState = ObjectState.Added;
                advance.ConstCurTypeId = Helper.ConstCurTypeId;
                if (advance.AdvanceTypeId == 2 || advance.AdvanceTypeId == 3||advance.AdvanceTypeId==112)
                {
                    if (advance.FuelAmount <= 0&&advance.RequestStatusId.GetValueOrDefault(1597)==1597)
                    {
                        advance.FuelAmount = Math.Round(advance.FuelQty * advance.FuelRate, 2);
                    }
                    if (advance.FuelRate <= 0&& advance.FuelAmount>0&& advance.FuelQty>0)
                    {
                        advance.FuelRate = Math.Round(advance.FuelAmount / advance.FuelQty, 2);
                    }
                }
                //advance.Amount = advance.FuelQty > 0 ? advance.FuelAmount : advance.CashAmount;
                if ((advance.IGSTAmt + advance.CGSTAmt + advance.SGSTAmt+advance.RoundUp) == 0)
                {
                    advance.BasicAmt = advance.Amount;
                }
                #endregion Advance Logic

                var err = GetLiveDbLevelValidation(advance, _unitOfWorkAsync, 0/*New*/);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
                #region Loan Adjustmentlogic
                List<FakeVDRs> loanvdrs = new List<FakeVDRs>();
                if (advance.AdvanceTypeId == 1)
                {
                    var jdata = advance.DataView.Where(x => x.DataName == "LoanAdvanceAjustment1107").FirstOrDefault();
                    if (jdata != null)
                    {
                        /*List<dynamic> models = JsonConvert.DeserializeObject<List<dynamic>>(jdata.RawJson);*/
                        loanvdrs = JsonConvert.DeserializeObject<List<FakeVDRs>>(jdata.RawJson);
                    }
                    if (loanvdrs.Any(x=>x.AVDRId.GetValueOrDefault()<=0))
                    {
                        throw new BusinessException(ErrorCode.TADV100, $"HINT:Invalid Loan Entry selected.AVDRId cannot be null");
                    }

                    if (loanvdrs.Any())
                    {
                        if (_AdvanceLoanAdjustmentNatureId <= 0)
                        {
                            throw new BusinessException(ErrorCode.TADV100, $"HINT:Advance Loan Adjustment Nature is not configured");
                        }

                        advance.IsAutoAPRL = true;
                        advance.RequestStatusId = 1597;
                        advance.CashAmount = advance.RequestAmount;
                        advance.LoanAdjusted = loanvdrs.Sum(x => x.Adjusted);
                        advance.PaidAmount = advance.CashAmount - advance.LoanAdjusted;
                        advance.Ref1Id = _AdvanceLoanAdjustmentNatureId;
                    }
                }
                #endregion
                if (advance.RequestStatusId != 1596/*New Request*/ && advance.RequestStatusId != 1598/*Reject Request*/)
                {
                    #region Voucher Logic
                    if (advance.VoucherId == null)
                    {
                        /*Deleting junk voucher*/
                        var voucher1 = _unitOfWorkAsync.RepositoryAsync<Voucher>()
                            .Query(x => (x.VoucherNo == advance.VoucherNo) && x.VoucherTypeId == advance.AdvanceTypeId)
                            .Select(x => x)
                            .FirstOrDefault();

                        if (voucher1 != null)
                        {
                            voucher1.ObjectState = ObjectState.Deleted;
                            await _unitOfWorkAsync.SaveChangesAsync();
                        }
                    }
                    if (advance.VoucherId > 0)
                    {
                        advance.fk_Voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>().Query(x => x.Id == advance.VoucherId && x.VoucherTypeId == advance.AdvanceTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                    }
                    if (advance.fk_Voucher == null) { advance.fk_Voucher = new Voucher { ObjectState = ObjectState.Added }; }
                    if (loanvdrs.Any())
                    {
                        advance.fk_Voucher.Account7Id = advance.DriverId;
                        advance.fk_Voucher.Amount7 = -advance.LoanAdjusted;
                    }

                    _tripAdvanceLogService.PrepareV(advance);

                    #endregion Voucher Logic

                    #region Voucher Detail Logic

                    _tripAdvanceLogService.PrepareVD(advance);

                    #endregion Voucher Detail Logic

                    #region Voucher Detail Refrence

                    advance.fk_Voucher.VoucherDetails
                    .ForEach(x => new Action<VoucherDetail, TripAdvanceLog,List<FakeVDRs>>(_tripAdvanceLogService.PrepareVDR)
                    .Invoke(x, advance,loanvdrs));

                    #endregion Voucher Detail Refrence

                    #region Voucher Validations
                    if (!(advance.AdvanceTypeId == 110 || advance.AdvanceTypeId == 111))
                    {
                        if (advance.fk_Voucher.VoucherDetails.Sum(x => x.Amount) != 0)
                        {
                            throw new BusinessException(ErrorCode.TADV100, $"HINT:VD_SUM:{advance.fk_Voucher.VoucherDetails.Sum(x => x.Amount)},Amt1:{advance.fk_Voucher.Amount1}, Amt2:{advance.fk_Voucher.Amount2}, AdvAmt:{advance.Amount}");
                        }
                    }
                    else if(advance.fk_Voucher.VoucherDetails.Sum(x => x.Amount) != 0)
                    {
                        throw new BusinessException(ErrorCode.TADV100, $"HINT:VD_SUM:{advance.fk_Voucher.VoucherDetails.Sum(x => x.Amount)}");
                    }
                    if (advance.fk_Voucher.VoucherDetails.Count(x => x.ObjectState == ObjectState.Added) <= 1)
                    {
                        throw new BusinessException(ErrorCode.TADV101, $"VDCount:{advance.fk_Voucher.VoucherDetails.Count}");//Atleast two VD are required in Advance Transaction Voucher
                    }
                    //if (advance.fk_Voucher.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState == ObjectState.Added) == 0)
                    //{
                    //    return BadRequest("TADV102");//Atlead one VDR is Required in Advance Transaction
                    //}
                    //foreach (var voucherDetail in advance.fk_Voucher.VoucherDetails.Where(voucherDetail => voucherDetail.VoucherDetailReferences.Count > 0 && voucherDetail.Amount != voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)))
                    //{
                    //    throw new BusinessException(ErrorCode.TADV103, $"VDAmount:{voucherDetail.Amount}, VDRSum:{voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)}");//VD and VDR Amount Doesn't Tally
                    //}

                    #endregion Voucher Validations
                }
                if (advance.RequestStatusId == 1596/*Reject Request*/ || advance.RequestStatusId == 1598 /*New Request*/)
                {
                    var voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>().Queryable().FirstOrDefault(x => x.Id == advance.VoucherId && x.VoucherTypeId == advance.AdvanceTypeId);
                    if (voucher != null)
                    {
                        voucher.ObjectState = ObjectState.Deleted;
                    }
                    advance.VoucherId = null;
                    advance.fk_Voucher = null;
                }
                
                _tripAdvanceLogService.Insert(advance);
                await _unitOfWorkAsync.SaveChangesAsync();
                //if (advance.VehicleId > 0 && _tripAdvanceLogService.GetConfigValue<int>("RunFuelAutomationProcess") == 1)
                //{
                //    var differTime = _tripAdvanceLogService.GetConfigValue<double>("FuelAutomationTriggerInterval");
                //    if (differTime < 2)
                //    {
                //        differTime = 2;
                //    }
                //    BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunFuelAutomationByVehicle(advance.VehicleId.GetValueOrDefault(), Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(differTime));
                //}
                //_audit.Insert(new ApiRecordAccessLog() { ObjectState = ObjectState.Added, RecordId = advance.Id, UserId = this.GetClaimByKey<long>("UserId"), SessionId = this.GetClaimByKey<long>("SessionId"), Type = AccessType.Created, ViewId = 1005, RecordName = advance.VoucherNo });
                //await _unitOfWorkAsync.SaveChangesAsync();

                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Commit();
                }

                /*def stock recalcl*/
                if (advance.AdvanceTypeId == 112 && advance.SettledRefId.GetValueOrDefault() > 0)
                {
                    await
                        _unitOfWorkAsync.ExecSqlQueryAsync(
                            $"update b set b.BalanceQty=b.FuelQty-isnull(isqty.IssuedDef,0) from dbo.tTripAdvanceLog b left join (select i.SettledRefId,IssuedDef=sum(i.FuelQty) from dbo.tTripAdvanceLog as i group by i.SettledRefId)as isqty on isqty.SettledRefId=b.Id where b.Id={advance.SettledRefId}");

                }
                var lnerr = SaveOtherDetails(advance, _unitOfWorkAsync);
                if (!string.IsNullOrWhiteSpace(lnerr))
                {
                    return BadRequest(lnerr);
                }

                return Created(advance);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Rollback();
                }
                throw;
            }
        }

        //// PATCH: odata/TripAdvanceLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TripAdvanceLog> patch)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.BeginTransaction(IsolationLevel.ReadCommitted);
                }

                await _unitOfWorkAsync.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET VDRId=NULL WHERE Id={key}");

                var advance = await _tripAdvanceLogService.FindAsync(key);
                if (advance == null)
                {
                    return NotFound();
                }
                
                /*QCId*/
                patch.TryGetPropertyValue("DataView", out var dv);
                if (dv is List<JsonDataEntity> dataview)
                {
                    foreach (var entity in dataview)
                    {
                        advance.DeleteAndAdd(entity);
                    }
                }
                patch.Patch(advance);
                advance.ConstCurTypeId = Helper.ConstCurTypeId;
                #region Loan Adjustmentlogic
                List<FakeVDRs> loanvdrs = new List<FakeVDRs>();
                if (advance.AdvanceTypeId == 1)
                {
                    var jdata = advance.DataView.Where(x => x.DataName == "LoanAdvanceAjustment1107").FirstOrDefault();
                    if (jdata != null)
                    {
                        loanvdrs = JsonConvert.DeserializeObject<List<FakeVDRs>>(jdata.RawJson);
                    }
                    if (loanvdrs.Any(x => x.AVDRId.GetValueOrDefault() <= 0))
                    {
                        throw new BusinessException(ErrorCode.TADV100, $"HINT:Invalid Loan Entry selected.AVDRId cannot be null");
                    }
                    if (loanvdrs.Any())
                    {
                        if (_AdvanceLoanAdjustmentNatureId <= 0)
                        {
                            throw new BusinessException(ErrorCode.TADV100, $"HINT:Advance Loan Adjustment Nature is not configured");
                        }

                        advance.IsAutoAPRL = true;
                        advance.RequestStatusId = 1597;
                        advance.CashAmount = advance.RequestAmount;
                        advance.LoanAdjusted = loanvdrs.Sum(x => x.Adjusted);
                        advance.PaidAmount = advance.CashAmount - advance.LoanAdjusted;
                        advance.Ref1Id = _AdvanceLoanAdjustmentNatureId;
                    }
                }
                #endregion
                if ((advance.AdvanceTypeId == 1 || advance.AdvanceTypeId == 2 || advance.AdvanceTypeId == 110 || advance.AdvanceTypeId == 112) && advance.TripLogId > 0 && await _unitOfWorkAsync.RepositoryAsync<VehicleMovementLog>().Queryable().AnyAsync(x => x.Id == advance.TripLogId && x.SettlementId > 0).ConfigureAwait(true))
                {
                    return BadRequest("Advance cannot be paid against Settled Trip");
                }
                
                advance.VDRId = null;
                if (advance.Amount <= 0 && advance.RequestStatusId != 1596 && advance.RequestStatusId != 1598/*New Request*/) return BadRequest("Advance Amount is Zero which is not allowed.");
                if (advance.Amount <= 0 && (advance.RequestAmount <= 0 && advance.RequestQty <= 0) && advance.RequestStatusId != 1598)
                    return BadRequest("Either Advance Amount or Request Amount or Request Qty should be greater than zero.");

                #region Advance Logic

                //TODO:Fuel Rate should be fatched from Database
                if (advance.AdvanceTypeId == 2 || advance.AdvanceTypeId == 3|| advance.AdvanceTypeId == 112 || advance.AdvanceTypeId == 110)
                {
                    if (advance.FuelAmount <= 0 && advance.RequestStatusId.GetValueOrDefault(1597) == 1597)
                    {
                        advance.FuelAmount = Math.Round(advance.FuelQty * advance.FuelRate, 2);
                    }
                    if (advance.FuelRate <= 0 && advance.FuelAmount > 0 && advance.FuelQty > 0)
                    {
                        advance.FuelRate = Math.Round(advance.FuelAmount / advance.FuelQty, 2);
                    }
                }
                if ((advance.IGSTAmt + advance.CGSTAmt + advance.SGSTAmt) == 0){
                    advance.BasicAmt = advance.Amount;
                }
                advance.ObjectState = ObjectState.Modified;

                #endregion Advance Logic
                
                var err = GetLiveDbLevelValidation(advance, _unitOfWorkAsync, 1/*Update*/);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
                

                if (advance.RequestStatusId != 1596/*New Request*/ && advance.RequestStatusId != 1598/*Request Reject*/)
                {
                    var voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>().Query(x =>((x.Id == advance.VoucherId) || (x.VoucherNo==advance.VoucherNo)) && x.VoucherTypeId == advance.AdvanceTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();

                    #region Voucher Logic

                    advance.fk_Voucher = voucher ?? new Voucher();
                    
                    advance.fk_Voucher.Account7Id = null;                    
                    advance.fk_Voucher.Amount7 = 0;

                    if (loanvdrs.Any())
                    {
                        advance.fk_Voucher.Account7Id = advance.DriverId;
                        advance.fk_Voucher.Amount7 = -advance.LoanAdjusted;
                    }

                    _tripAdvanceLogService.PrepareV(advance);

                    #endregion Voucher Logic

                    #region VoucherDetails Logic

                    _tripAdvanceLogService.PrepareVD(advance);

                    #endregion VoucherDetails Logic

                    #region Voucher Detail Refrence
                    advance.fk_Voucher.VoucherDetails.ForEach(x => new Action<VoucherDetail, TripAdvanceLog,List<FakeVDRs>>(_tripAdvanceLogService.PrepareVDR)
                    .Invoke(x, advance, loanvdrs));
                    #endregion Voucher Detail Refrence

                    #region Validations

                    //if (advance.FK_Voucher.Amount1 + advance.FK_Voucher.Amount2 != 0 || (advance.FK_Voucher.Amount1 > 0 ? advance.FK_Voucher.Amount1 : advance.FK_Voucher.Amount2) != advance.Amount || advance.Amount <= 0 || advance.FK_Voucher.VoucherDetails.Sum(x => x.Amount) != 0)
                    //{
                    //    return BadRequest("TADV100");//Amount Validation Failed
                    //}
                    if (advance.fk_Voucher.VoucherDetails.Count(x => x.ObjectState == ObjectState.Added) <= 1)
                    {
                        throw new BusinessException(ErrorCode.TADV101, $"VDCount:{advance.fk_Voucher.VoucherDetails.Count}");//Atleast two VD are required in Advance Transaction Voucher
                    }
                    //if (advance.fk_Voucher.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState == ObjectState.Added) == 0)
                    //{
                    //    return BadRequest("TADV102");//Atlead one VDR is Required in Advance Transaction
                    //}
                    //foreach (var voucherDetail in advance.fk_Voucher.VoucherDetails.Where(voucherDetail => voucherDetail.VoucherDetailReferences.Count > 0 && voucherDetail.Amount != voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)))
                    //{
                    //    throw new BusinessException(ErrorCode.TADV103, $"VDAmount:{voucherDetail.Amount}, VDRSum:{voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)}");//VD and VDR Amount Doesn't Tally
                    //}

                    #endregion Validations
                }
                if (advance.RequestStatusId == 1598/*Reject Request*/)
                {
                    var childTransaction = await _tripAdvanceLogService.Queryable().Where(x => x.ParentAdvanceLogId == key)
                        .Select(x => x.ReferenceNo).FirstOrDefaultAsync();
                    if (childTransaction != null)
                    {
                        throw new BusinessException(ErrorCode.GLB106, $"Unable to reject parent transaction. first try to free it up from child transaction Ref No {childTransaction}");
                    }
                }
                if (advance.RequestStatusId == 1596/*New Request*/ || advance.RequestStatusId == 1598 /*Reject Request*/)
                {
                    var telRepo = _unitOfWorkAsync.RepositoryAsync<TripExpenseLog>();
                    var existingExpD = telRepo.Queryable().FirstOrDefault(x => x.IsAuto && x.TripAdvanceLogId == key);
                    if (existingExpD != null)
                    {
                        existingExpD.ObjectState = ObjectState.Deleted;
                    }
                    var vouchers = await _unitOfWorkAsync.RepositoryAsync<Voucher>().Queryable().Where(x => (x.Id == advance.VoucherId || x.VoucherNo == advance.VoucherNo) && x.VoucherTypeId == advance.AdvanceTypeId).ToListAsync();
                    if (vouchers.Any())
                    {
                        vouchers.ForEach(voucher => voucher.ObjectState = ObjectState.Deleted);
                    }
                    advance.VoucherId = null;

                    advance.fk_Voucher = null;
                }
                /*Loan*/
                if (advance.RequestStatusId == 1598 /*Reject Request*/)
                {
                    var jRepo = _unitOfWorkAsync.RepositoryAsync<JsonTransactionLog>();
                    var a = jRepo.Queryable().FirstOrDefault(x => x.Key == "LoanAdvanceAjustment1107" && x.RecordId == key);
                    if (a != null)
                    {
                        a.ObjectState = ObjectState.Deleted;
                    }

                    var voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>().Queryable().FirstOrDefault(x =>
                        x.ReferenceTransactionId == advance.Id && x.VoucherTypeId == 134);
                    if (voucher != null)
                    {
                        voucher.ObjectState = ObjectState.Deleted;
                    }                    
                }
                
                await _unitOfWorkAsync.SaveChangesAsync();
                
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Commit();
                }
                
                /*def stock recalcl*/
                if (advance.AdvanceTypeId == 112 && advance.SettledRefId.GetValueOrDefault() > 0)
                {
                    await
                        _unitOfWorkAsync.ExecSqlQueryAsync(
                            $"update b set b.BalanceQty=b.FuelQty-isnull(isqty.IssuedDef,0) from dbo.tTripAdvanceLog b left join (select i.SettledRefId,IssuedDef=sum(i.FuelQty) from dbo.tTripAdvanceLog as i group by i.SettledRefId)as isqty on isqty.SettledRefId=b.Id where b.Id={advance.SettledRefId}");

                }

                var lnerr = SaveOtherDetails(advance, _unitOfWorkAsync);
                if (!string.IsNullOrWhiteSpace(lnerr))
                {
                    return BadRequest(lnerr);
                }

                return Updated(advance);
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

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var advanceLog = await _tripAdvanceLogService.Queryable().Include(x=>x.FuelExpanses).FirstOrDefaultAsync(x=>x.Id==key);
            if (advanceLog == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            var err = GetLiveDbLevelValidation(advanceLog, _unitOfWorkAsync, 3/*delete*/);
            if (!string.IsNullOrWhiteSpace(err))
            {
                return BadRequest(err);
            }

            if (advanceLog.TripLogId > 0&&advanceLog.AdvanceTypeId!=3 && await _unitOfWorkAsync.RepositoryAsync<VehicleMovementLog>().Queryable().AnyAsync(x => x.Id == advanceLog.TripLogId && x.SettlementId > 0).ConfigureAwait(true))
            {
                return BadRequest($"Settled Advance {advanceLog.VoucherNo} Cannot be deleted");
            }
            if (advanceLog.FuelExpanses!=null&&advanceLog.FuelExpanses.Any(x=>x.SettlementId>0))
            {
                return BadRequest($"The Advance No {advanceLog.VoucherNo} has been referenced in Trip Expense and that Trip Expense has been settled: Hind SettlementIds: {advanceLog.FuelExpanses?.Where(x=>x.SettlementId>0)?.Select(x=>x.SettlementId)?.JoinStrings(",")}");
            }
            try
            {
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                if (advanceLog.FuelExpanses != null)
                {
                    foreach (var exp in advanceLog.FuelExpanses)
                    {
                        exp.ObjectState = ObjectState.Deleted;
                        _unitOfWorkAsync.RepositoryAsync<TripExpenseLog>().Delete(exp);
                    }
                }

                var vouchers = await
                _unitOfWorkAsync.RepositoryAsync<Voucher>()
                    .Queryable()
                    .Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences))
                        .Where(x => (x.Id == advanceLog.VoucherId || x.VoucherNo == advanceLog.VoucherNo) && x.VoucherTypeId == advanceLog.AdvanceTypeId).ToListAsync();
                vouchers?.ForEach(voucher =>
                {
                    if (voucher == null) return;
                    if (advanceLog.VoucherId > 0) advanceLog.fk_Voucher = vouchers?.FirstOrDefault(x => x.Id == advanceLog.VoucherId);
                    voucher.ObjectState = ObjectState.Deleted;
                    voucher.VoucherDetails?.ForEach(x => x.ObjectState = ObjectState.Deleted);
                    voucher.VoucherDetails?.ForEach(x => x.VoucherDetailReferences?.ForEach(y => y.ObjectState = ObjectState.Deleted));
                });
                
                advanceLog.ObjectState = ObjectState.Deleted;
                _tripAdvanceLogService.Delete(advanceLog);
                try
                {
                    var _stockdef = await
                    _unitOfWorkAsync.RepositoryAsync<TripAdvanceLog>()
                    .Queryable()
                    .Where(x => x.Id == advanceLog.SettledRefId).FirstOrDefaultAsync();

                    decimal? _stockissued =
                   _unitOfWorkAsync.Repository<TripAdvanceLog>()
                    .Queryable()
                    .Where(x => x.Id != advanceLog.Id && x.SettledRefId == advanceLog.SettledRefId && x.AdvanceTypeId == 112)
                    .Sum(k => k.FuelQty);

                    _stockdef.BalanceQty = _stockdef.FuelQty - _stockissued.GetValueOrDefault();
                    _stockdef.ObjectState = ObjectState.Modified;
                }
                catch { }

                /*Loan*/
                if (advanceLog.RequestStatusId == 1598 /*Reject Request*/)
                {
                    var jRepo = _unitOfWorkAsync.RepositoryAsync<JsonTransactionLog>();
                    var a = jRepo.Queryable().FirstOrDefault(x => x.Key == "LoanAdvanceAjustment1107" && x.RecordId == key);
                    if (a != null)
                    {
                        a.ObjectState = ObjectState.Deleted;
                    }

                    var voucher = _unitOfWorkAsync.RepositoryAsync<Voucher>().Queryable().FirstOrDefault(x =>
                        x.ReferenceTransactionId == advanceLog.Id && x.VoucherTypeId == 134);
                    if (voucher != null)
                    {
                        voucher.ObjectState = ObjectState.Deleted;
                    }
                    await _unitOfWorkAsync.SaveChangesAsync();
                }                

                await _unitOfWorkAsync.SaveChangesAsync();               

                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Commit();
                }

                /*def stock recal*/
                
                return StatusCode(HttpStatusCode.NoContent);
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

        [AcceptVerbs("DELETE")]
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
            string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var advancelog = await _tripAdvanceLogService.FindAsync(key);
            if (advancelog == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_Triplog":
                    advancelog.TripLogId = null;
                    advancelog.ObjectState = ObjectState.Modified;
                    break;
                case "fk_Settlement":
                    advancelog.SettlementId = null;
                    advancelog.fk_Settlement = null;
                    advancelog.ObjectState = ObjectState.Modified;
                    break;
                case "fk_SettledRefAdvance":
                    advancelog.SettledRefId = null;
                    advancelog.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var adv = await _tripAdvanceLogService.FindAsync(key);
            if (adv == null)
            {
                return NotFound();
            }
            var advId = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_Triplog":
                    if (!uow.RepositoryAsync<VehicleMovementLog>().Queryable().Any(x => x.Id == advId))
                    {
                        return NotFound();
                    }
                    adv.TripLogId = advId;
                    adv.ObjectState = ObjectState.Modified;
                    break;

                case "fk_SettledRefAdvance":
                    if (!uow.RepositoryAsync<TripAdvanceLog>().Queryable().Any(x => x.Id == advId))
                    {
                        return NotFound();
                    }
                    adv.SettledRefId = advId;
                    adv.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
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
    }
}