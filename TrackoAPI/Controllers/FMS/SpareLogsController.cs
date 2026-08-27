using AutoMapper;

//using HibernatingRhinos.Profiler.Appender.ProfiledDataAccess;
using Newtonsoft.Json;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.FMS.Repairs;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class SpareLogsController : ODataController
    //ODataController
    {
        private readonly ISpareLogService _repo;
        private readonly IUnitOfWorkAsync _uow;

        // private static List<vwSparePurchaseBill> _localDb=new List<vwSparePurchaseBill>();
        public SpareLogsController(IUnitOfWorkAsync unitOfWorkAsync, ISpareLogService service)
        {
            _repo = service;
            _uow = unitOfWorkAsync;
        }

        [HttpPost]
        public async Task<IHttpActionResult> AMCPayment(ODataActionParameters parameters)
        {
            var ivouchers = parameters["vouchers"] as string;
            if (ivouchers == null) return BadRequest("No Payment Vouchers Records found.");
            var vouchers = JsonConvert.DeserializeObject<List<Voucher>>(ivouchers);

            var iextrainfoids = parameters["extrainfoids"] as IEnumerator<long>;
            if (iextrainfoids == null) return BadRequest("No Bill Records found to do payment.");
            var extrainfoids = iextrainfoids.ToList();

            var ilogids = parameters["logids"] as IEnumerator<long>;
            if (ilogids == null) return BadRequest("No Bill Log Records found to do payment.");
            var logids = ilogids.ToList();

            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            Voucher paymentvoucher = null;
            try
            {
                var _mapper = new MapperConfiguration(x => x.CreateMap<Voucher, Voucher>()).CreateMapper();
                var slerepo = uow.RepositoryAsync<SpareLogExtraInfo>();
                var extrainfo = await slerepo.Queryable().Where(x => extrainfoids.Contains(x.Id)).ToListAsync();
                var billvoucher = vouchers.FirstOrDefault(x => x.VoucherTypeId == 83);
                billvoucher.ConstCurTypeId = Helper.ConstCurTypeId;
                if (billvoucher == null)
                {
                    return BadRequest("AMC bill voucher not attached.");
                }
                paymentvoucher = vouchers.FirstOrDefault(x => x.VoucherTypeId == 84);
                paymentvoucher.ConstCurTypeId = Helper.ConstCurTypeId;
                if (paymentvoucher == null)
                {
                    return BadRequest("AMC Payment voucher not attached.");
                }
                var vrepo = uow.RepositoryAsync<Voucher>();

                if (billvoucher.Id > 0)
                {
                    var pymtv = await vrepo.FindAsync(billvoucher.Id);
                    billvoucher = _mapper.Map(billvoucher, pymtv);
                    billvoucher.ObjectState = ObjectState.Modified;
                }
                var logs = await uow.RepositoryAsync<RepairLabourLog>().Queryable().Where(x => logids.Contains(x.Id)).ToListAsync();
                var acrefids = vouchers.SelectMany(x => x.VoucherDetails, (p, c) => c.AccountId).Distinct().ToList();
                var accounts = await uow.RepositoryAsync<Ledger>().Queryable().Where(x => acrefids.Contains(x.Id) && x.ReferenceFlag)
                    .Select(x => x.Id).ToListAsync();
                await uow.ExecSqlQueryAsync($"DELETE FROM [dbo].[tVoucherVD] WHERE VoucherId IN({billvoucher.Id},{paymentvoucher.Id})");
                foreach (var x in billvoucher.VoucherDetails)
                {
                    x.Voucher = billvoucher;
                    x.VoucherId = billvoucher.Id;
                    x.ObjectState = ObjectState.Added;
                    
                    x.ConstCurTypeId = billvoucher.ConstCurTypeId;
                    x.CurTypeId= billvoucher.CurTypeId; 
                    x.CurRate= billvoucher.CurRate; 

                    if (x.OrderId == 1)
                    {
                        foreach (var info in extrainfo)
                        {
                            info.ConstCurTypeId = Helper.ConstCurTypeId;
                            if (accounts.Contains(x.AccountId))
                            {
                                var amount = logs.Where(z => z.ExtraInfoId == info.Id).Sum(y => y.NetAmount);
                                var cgstamt = logs.Where(z => z.ExtraInfoId == info.Id).Sum(y => y.CGSTAmount);
                                var sgstamt = logs.Where(z => z.ExtraInfoId == info.Id).Sum(y => y.SGSTAmount);
                                var igstamt = logs.Where(z => z.ExtraInfoId == info.Id).Sum(y => y.IGSTAmount);
                                var netamount = amount + (info.CalculateVat ? cgstamt + sgstamt + igstamt : 0);
                                var vdr = new VoucherDetailReference
                                {
                                    Id = 0,
                                    Amount = billvoucher.Amount1 > 0 ? netamount : -netamount,
                                    VoucherDetailId = x.Id,
                                    fk_VoucherDetail = x,
                                    ReferenceNo = info.VendorReferenceNo,
                                    DueDate = info.DocDate,
                                    VDRTypeId = 1013, // New Reference
                                    ObjectState = ObjectState.Added,

                                    CurTypeId = x.CurTypeId,
                                    CurRate = x.CurRate,
                                    ConstCurTypeId = x.ConstCurTypeId
                                };
                                x.VoucherDetailReferences.Add(vdr);
                                logs?.ForEach(log =>
                                {
                                    log.VoucherId = billvoucher.Id;
                                    log.fk_Voucher = billvoucher;
                                    log.ObjectState = ObjectState.Modified;
                                });
                            }
                            info.VoucherId = billvoucher.Id;
                            info.fk_Voucher = billvoucher;
                            info.ObjectState = ObjectState.Unchanged;
                        }
                    }
                }
                billvoucher.ObjectState = billvoucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                
                await uow.SaveChangesAsync();
                if (paymentvoucher.Id > 0)
                {
                    var pymtv = await vrepo.FindAsync(paymentvoucher.Id);
                    paymentvoucher = _mapper.Map(paymentvoucher, pymtv);
                }
                paymentvoucher.ObjectState = paymentvoucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                foreach (var x in paymentvoucher.VoucherDetails)
                {
                    x.Voucher = paymentvoucher;
                    x.VoucherId = paymentvoucher.Id;

                    x.ObjectState = ObjectState.Added;
                    x.CurTypeId = paymentvoucher.CurTypeId;
                    x.CurRate = paymentvoucher.CurRate;
                    x.ConstCurTypeId = paymentvoucher.ConstCurTypeId;

                    if (x.OrderId == 1)
                    {
                        foreach (var info in extrainfo)
                        {
                            if (accounts.Contains(x.AccountId))
                            {
                                var parentvdr =
                                    billvoucher?.VoucherDetails?.FirstOrDefault(z => z.Amount < 0)?
                                        .VoucherDetailReferences.FirstOrDefault(y => y.ReferenceNo == info.VendorReferenceNo);
                                if (parentvdr == null && x.OrderId == 1)
                                {
                                    return BadRequest($"Parent VDR not found for {info.VendorReferenceNo}");
                                }
                                var vdramount = logs.Where(y => y.ExtraInfoId == info.Id).Sum(y => y.NetAmount);
                                var cgstamt = logs.Where(z => z.ExtraInfoId == info.Id).Sum(y => y.CGSTAmount);
                                var sgstamt = logs.Where(z => z.ExtraInfoId == info.Id).Sum(y => y.SGSTAmount);
                                var igstamt = logs.Where(z => z.ExtraInfoId == info.Id).Sum(y => y.IGSTAmount);
                                var netamount = vdramount + (info.CalculateVat ? cgstamt + sgstamt + igstamt : 0);
                                var vdr = new VoucherDetailReference
                                {
                                    Id = 0,
                                    Amount = netamount * (x.Amount > 0 ? 1 : -1),
                                    VoucherDetailId = x.Id,
                                    fk_VoucherDetail = x,
                                    ReferenceNo = info.VendorReferenceNo,
                                    DueDate = info.DocDate,
                                    VDRTypeId = x.OrderId == 1 ? 1014 : 1013, // Agianst ref
                                    ObjectState = ObjectState.Added,
                                    RefId = parentvdr?.Id,

                                    CurTypeId = x.CurTypeId,
                                    CurRate = x.CurRate,
                                    ConstCurTypeId = x.ConstCurTypeId
                                };
                                x.VoucherDetailReferences.Add(vdr);
                            }
                            info.RelatedVoucherId = paymentvoucher.Id;
                            info.fk_RelatedVoucher = paymentvoucher;
                            info.VoucherId = billvoucher.Id;
                            info.fk_Voucher = billvoucher;
                            info.ObjectState = ObjectState.Modified;
                        }
                    }
                }
                paymentvoucher.ObjectState = paymentvoucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                foreach (var log in logs)
                {
                    log.VoucherId = billvoucher.Id;
                    log.fk_Voucher = billvoucher;
                    log.ObjectState = ObjectState.Modified;
                }
                await uow.SaveChangesAsync();
            }
            catch (Exception ex)
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
            return Ok();
        }

        //PUT: odata/SpareLogs(key)/relationName/$ref
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
                                    $"UPDATE dbo.tSpareLog SET GatePassId={id} WHERE Id={key}");
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            //await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objSpareLog = await _repo.FindAsync(key);
            if (objSpareLog == null)
            {
                return NotFound();
            }
            objSpareLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(objSpareLog);
            await _uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [ODataRoute("DeleteSpareTransaction"), HttpPost]
        public async Task<IHttpActionResult> DeleteGraph(ODataActionParameters param)
        {
            if (!Request.IsBatchRequest())
            {
                _uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (!param.ContainsKey("key"))
                {
                    throw new BusinessException(ErrorCode.GLB106, @"Transaction Identification is required.");
                }
                var key = (long)param["key"];
                await _repo.DeleteGraph(key);
                _uow.SaveChanges();
                if (!Request.IsBatchRequest())
                {
                    _uow.Commit();
                }
                return Ok();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
        }

        // GET: odata/SpareLogs
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<SpareLog> Get()
        {
            return _repo.Queryable();
        }

        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<SpareLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        [HttpGet]
        public decimal GetStockQty(long key)
        {
            return _repo.Queryable().Where(x => x.Id == key).Select(x => x.StockQty).FirstOrDefault();
        }

        #region Store Inward

        //GET: odata/GetView(5)
        /// <exclude />
        ///
        ///

        //GET: odata/GetView(5)
        [HttpGet, ODataRoute("GetSpareConsumeBill(key={key})")]
        public vwSparePurchaseBill GetConsumeView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetPurchaseBillView(key, 22);
        }

        [HttpGet, ODataRoute("GetPurchaseBillSettlment(key={key})")]
        public vwSparePurchaseBill GetPurchaseBillSettlmentView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetPurchaseBillView(key, 62);
        }

        [HttpGet, ODataRoute("GetSparePurchaseBill(key={key})")]
        public vwSparePurchaseBill GetPurchaseView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetPurchaseBillView(key, 61);
        }
        //GET: odata/GetView(5)
        [HttpGet, ODataRoute("GetStockTransferAcknowledgment(key={key})")]
        public vwSparePurchaseBill GetStockTransferAcknowledgmentView([FromODataUri]long key)//(ODataActionParameters odataParam)
        {
            return _repo.GetPurchaseBillView(key, 26);
        }

        [HttpPost, ODataRoute("PostSpareConsumeBill")]
        public async Task<IHttpActionResult> PostConsumeBillViewAsync(ODataActionParameters odataParam)
        {
            
            var bill = odataParam["bill"] as vwSparePurchaseBill;
            bill.ConstCurTypeId = Helper.ConstCurTypeId;
            if (bill == null)
            {
                return BadRequest("Parameter bill cannot be null");
            }
            if (bill.CurTypeId <= 0 || bill.CurRate <= 0)
            {
                return BadRequest("Currency Type/ CurRate is required");
            }
            try
            {
                var tripadvancejson = bill.JsonData?.FirstOrDefault(x => x.DataName.ToLower() == "advancerequest")?.RawJson;
                if (bill.Id > 0 && !_uow.RepositoryAsync<SpareLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
                {
                    return NotFound();
                }
                if (!Request.IsBatchRequest())
                {
                    _uow.BeginTransaction();
                }
                //if (bill.CalVat && ((bill.IGSTAmount == 0 && ((bill.CGSTAmount == 0 || bill.SGSTAmount == 0) ? 1 : 0) == 1) || (bill.IGSTAmount > 0 && (bill.CGSTAmount + bill.SGSTAmount) > 0)))
                //{
                //    ModelState.AddModelError("GSTAmount", "GSTAmount Amount is not valid");
                //}

                if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0 && bill.ExpenseLedgerId2.GetValueOrDefault(0) == 0)
                {
                    ModelState.AddModelError("ExpenseLedgerId", "Atleast on Expense Ledger is required");
                }
                if (!ModelState.IsValid)
                {
                    if (!Request.IsBatchRequest())
                    {
                        _uow.Rollback();
                    }
                    return BadRequest(ModelState);
                }
                bill.VoucherTypeId = 22;
                var sle = _repo.InsertOrUpdatePurchaseBillView(bill);
                await _uow.SaveChangesAsync();
                bill.Id = sle.Id;
                if (!string.IsNullOrWhiteSpace(tripadvancejson))
                {
                    var ad = JsonConvert.DeserializeObject<TripAdvanceLog>(tripadvancejson,new JsonSerializerSettings { 
                    
                    });
                    if (ad.Id > 0 || ad.RequestAmount > 0)
                    {
                        var repo = _uow.RepositoryAsync<TripAdvanceLog>();
                        var vrepo = _uow.RepositoryAsync<Voucher>();
                        if (ad.Id > 0)
                        {
                            await _uow.ExecSqlQueryAsync($"UPDATE [dbo].[tTripAdvanceLog] SET VDRId=NULL WHERE Id={ad.Id}");
                            var advance = await repo.FindAsync(ad.Id);
                            if (advance == null && ad.RequestAmount > 0)
                            {
                                throw new BusinessException(ErrorCode.GLB106, $"Corresponding Trip Advance with Id {ad.Id} Not Found");
                            }
                            if (ad.RequestAmount == 0)
                            {
                                advance.ObjectState = ObjectState.Deleted;
                                var voucher = await vrepo.FindAsync(advance.VoucherId);
                                if (voucher != null)
                                {
                                    voucher.ObjectState = ObjectState.Deleted;
                                }
                            }
                            else
                            {
                                advance.CurTypeId = ad.CurTypeId;
                                advance.CurRate = ad.CurRate;
                                advance.ConstCurTypeId = ad.ConstCurTypeId;

                                advance.FuelAmount = 0;
                                advance.DriverId = ad.DriverId;
                                advance.VoucherId = ad.VoucherId;
                                advance.AdvanceTypeId = ad.AdvanceTypeId;
                                advance.RequestStatusId = ad.RequestStatusId;
                                advance.CreditAccountId = ad.CreditAccountId;
                                advance.DebitAccountId = ad.DebitAccountId;
                                advance.VehicleId = ad.VehicleId;
                                advance.Remark = advance.RequestRemark = ad.Remark;
                                advance.HireVehicleId = ad.HireVehicleId;
                                advance.RequestAmount = ad.RequestAmount;
                                advance.CashAmount = ad.CashAmount;
                                advance.ObjectState = ObjectState.Modified;
                                advance.VoucherNo = ad.VoucherNo;
                                advance.VoucherDate = ad.VoucherDate;
                                advance.VoucherId = ad.VoucherId;
                                advance.AdvanceDate = ad.AdvanceDate;
                                advance.IsAutoAPRL = ad.IsAutoAPRL;
                            }
                            ad = advance;
                        }
                        else
                        {
                            ad.ObjectState = ObjectState.Added;
                            repo.Insert(ad);
                        }
                        if (ad.ObjectState != ObjectState.Deleted)
                        {
                            if (ad.Amount <= 0 && ad.RequestStatusId != 1596 && ad.RequestStatusId != 1598/*New Request*/)
                            {
                                throw new BusinessException(ErrorCode.GLB106, "Advance Amount is Zero which is not allowed.");
                            }
                            if (ad.Amount <= 0 && (ad.RequestAmount <= 0 && ad.RequestQty <= 0) && ad.RequestStatusId != 1598)
                            {
                                throw new BusinessException(ErrorCode.GLB106, "Either Advance Amount or Request Amount or Request Qty should be greater than zero.");
                            }
                            ad.FuelAmount = 0;
                            ad.OfficeId = bill.OfficeId;
                            if (ad.RequestStatusId != 1596/*New Request*/ && ad.RequestStatusId != 1598/*Request Reject*/)
                            {
                                var voucher = vrepo.Query(x => x.Id == ad.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();

                                #region Voucher Logic

                                ad.fk_Voucher = voucher ?? new Voucher();
                                repo.PrepareV(ad);

                                #endregion Voucher Logic

                                #region VoucherDetails Logic

                                repo.PrepareVD(ad);

                                #endregion VoucherDetails Logic

                                #region Voucher Detail Refrence

                                ad.fk_Voucher.VoucherDetails.ForEach(x => new Action<VoucherDetail, TripAdvanceLog>(repo.PrepareVDR).Invoke(x, ad));

                                #endregion Voucher Detail Refrence

                                #region Validations

                                if (ad.fk_Voucher.VoucherDetails.Count(x => x.ObjectState == ObjectState.Added) <= 1)
                                {
                                    return BadRequest("TADV101");//Atleast two VD are required in Advance Transaction Voucher
                                }
                                foreach (var voucherDetail in ad.fk_Voucher.VoucherDetails.Where(voucherDetail => voucherDetail.VoucherDetailReferences.Count > 0 && voucherDetail.Amount != voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)))
                                {
                                    return BadRequest("TADV103:" + voucherDetail.Id);//VD and VDR Amount Doesn't Tally
                                }

                                #endregion Validations
                            }
                            if (ad.RequestStatusId == 1598/*Reject Request*/)
                            {
                                var childTransaction = await repo.Queryable().Where(x => x.ParentAdvanceLogId == ad.Id)
                                    .Select(x => x.ReferenceNo).FirstOrDefaultAsync();
                                if (childTransaction != null)
                                {
                                    throw new BusinessException(ErrorCode.GLB106, $"Unable to reject parent transaction. first try to free it up from child transaction Ref No {childTransaction}");
                                }
                            }
                            if (ad.RequestStatusId == 1596/*New Request*/ || ad.RequestStatusId == 1598 /*Reject Request*/)
                            {
                                var telRepo = _uow.RepositoryAsync<TripExpenseLog>();
                                var existingExpD = telRepo.Queryable().FirstOrDefault(x => x.IsAuto && x.TripAdvanceLogId == ad.Id);
                                if (existingExpD != null)
                                {
                                    existingExpD.ObjectState = ObjectState.Deleted;
                                }
                                var vouchers = await vrepo.Queryable().Where(x => (x.Id == ad.VoucherId || x.VoucherNo == ad.VoucherNo) && x.VoucherTypeId == ad.AdvanceTypeId).ToListAsync();
                                if (vouchers.Any())
                                {
                                    vouchers.ForEach(voucher => voucher.ObjectState = ObjectState.Deleted);
                                }
                                ad.VoucherId = null;

                                ad.fk_Voucher = null;
                            }
                            ad.Ref1 = $"{sle.Id}";
                            ad.OfficeId = sle.OfficeId.GetValueOrDefault();
                        }
                        await _uow.SaveChangesAsync();
                    }
                }
                _uow.SaveChanges();
                if (odataParam.Any(x => x.Key == "procid") && odataParam["procid"] is long procid && procid > 0)
                {
                    var spname = await _uow.RepositoryAsync<ReportProcedure>().FindAsync(procid);
                    if (spname != null)
                    {
                        try
                        {
                            await _uow.ExecuteProcedureAsync(spname.StoredProcedureName, new SqlParameter("TransactionId", sle.Id), new SqlParameter("TransactionNumber", bill.DocumentNo), new SqlParameter("TransactionType", bill.ViewId));
                        }
                        catch (SqlException ex)
                        {
                            throw new BusinessException(ex);
                        }
                    }
                }
                if (!Request.IsBatchRequest())
                {
                    _uow.Commit();
                }
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }

            return Ok(bill.Id);
        }

        [HttpPost, ODataRoute("PostPurchaseBillSettlement")]
        public IHttpActionResult PostPurchaseBillSettlementView(ODataActionParameters odataParam)
        {
            try {
                var bill = odataParam["bill"] as vwSparePurchaseBill;
                if (bill == null)
                {
                    return BadRequest("Invalid Parameter");
                }
                if (bill.CurTypeId <= 0 || bill.CurRate <= 0)
                {
                    return BadRequest("Currency Type/ CurRate is required");
                }
                if (bill.Id > 0 && !_uow.RepositoryAsync<SpareLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
                {
                    return NotFound();
                }
                if (bill.CalVat && ((bill.IGSTAmount == 0 && ((bill.CGSTAmount == 0 || bill.SGSTAmount == 0) ? 1 : 0) == 1) || (bill.IGSTAmount > 0 && (bill.CGSTAmount + bill.SGSTAmount) > 0)))
                {
                    ModelState.AddModelError("VAT Amount", "VAT Amount is not valid");
                }
                if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0 && bill.ExpenseLedgerId2.GetValueOrDefault(0) == 0)
                {
                    ModelState.AddModelError("ExpenseLedgerId", "Atleast on Expense Ledger is required");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bill.VoucherTypeId = 62;
                var sei = _repo.InsertOrUpdateMaterialSettlementMRNView(bill);
                
                _uow.SaveChanges();
                bill.Id = sei.Id;
                return Ok(bill.Id);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
        }
        [HttpPost, ODataRoute("PostMaterialDeliveryChallan")]
        public IHttpActionResult PostMaterialDeliveryChallanView(ODataActionParameters odataParam)
        {
            try
            {
                var bill = odataParam["bill"] as vwSparePurchaseBill;
                if (bill == null)
                {
                    return BadRequest("Invalid Parameter");
                }
                if (bill.CurTypeId <= 0 || bill.CurRate <= 0)
                {
                    return BadRequest("Currency Type/ CurRate is required");
                }
                if (bill.Id > 0 && !_uow.RepositoryAsync<SpareLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
                {
                    return NotFound();
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var sle = _repo.InsertOrUpdateMaterialDeliveryChallanView(bill);
                
                _uow.SaveChanges();
                bill.Id = sle.Id;
                return Ok(sle.Id);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
        }

        [HttpPost, ODataRoute("PostSparePurchaseBill")]
        public IHttpActionResult PostPurchaseBillView(ODataActionParameters odataParam)
        {
            try
            {
                var bill = odataParam["bill"] as vwSparePurchaseBill;
                if (bill == null)
                {
                    return BadRequest("Invalid Parameter");
                }
                if (bill.CurTypeId <= 0 || bill.CurRate <= 0)
                {
                    return BadRequest("Currency Type/ CurRate is required");
                }
                if (bill.Id > 0 && !_uow.RepositoryAsync<SpareLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
                {
                    return NotFound();
                }
                if (bill.CalVat && ((bill.IGSTAmount == 0 && ((bill.CGSTAmount == 0 || bill.SGSTAmount == 0) ? 1 : 0) == 1) || (bill.IGSTAmount > 0 && (bill.CGSTAmount + bill.SGSTAmount) > 0)))
                {
                    ModelState.AddModelError("GSTAmount", "GSTAmount Amount is not valid");
                }
                if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0 && bill.ExpenseLedgerId2.GetValueOrDefault(0) == 0)
                {
                    ModelState.AddModelError("ExpenseLedgerId", "Atleast on Expense Ledger is required");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bill.VoucherTypeId = (bill.VoucherTypeId.GetValueOrDefault(0) == 0 ? 61 : bill.VoucherTypeId);
                var sle = _repo.InsertOrUpdatePurchaseBillView(bill);
                
                _uow.SaveChanges();
                bill.Id = sle.Id;
                return Ok(sle.Id);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
        }
        [HttpPost, ODataRoute("PostSpareMRN")]
        public IHttpActionResult PostSpareMaterialMRNView(ODataActionParameters odataParam)
        {
            try
            {
                var bill = odataParam["bill"] as vwSparePurchaseBill;

                if (bill.CurTypeId <= 0 || bill.CurRate <= 0)
                {
                    return BadRequest("Currency Type/ CurRate is required");
                }

                if (bill == null)
                {
                    return BadRequest("Invalid Parameter");
                }
                if (bill.Id > 0 && !_uow.RepositoryAsync<SpareLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
                {
                    return NotFound();
                }
                if (bill.CalVat && ((bill.IGSTAmount == 0 && ((bill.CGSTAmount == 0 || bill.SGSTAmount == 0) ? 1 : 0) == 1) || (bill.IGSTAmount > 0 && (bill.CGSTAmount + bill.SGSTAmount) > 0)))
                {
                    ModelState.AddModelError("GSTAmount", "GSTAmount Amount is not valid");
                }

                if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0 && bill.ExpenseLedgerId2.GetValueOrDefault(0) == 0)
                {
                    ModelState.AddModelError("ExpenseLedgerId", "Atleast on Expense Ledger is required");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bill.VoucherTypeId = (bill.VoucherTypeId.GetValueOrDefault(0) == 0 ? 23 : bill.VoucherTypeId);

                var sei = _repo.InsertOrUpdateMaterialMRNView(bill);
                
                _uow.SaveChanges();
                bill.Id = sei.Id;
                return Ok(bill.Id);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
        }
        [HttpPost, ODataRoute("PostStockTransferAcknowledgment")]
        public IHttpActionResult PostStockTransferAcknowledgmentView(ODataActionParameters odataParam)
        {
            try
            {
                var bill = odataParam["bill"] as vwSparePurchaseBill;
                if (bill == null)
                {
                    return BadRequest("Invalid Parameter");
                }
                if (bill.Id > 0 && !_uow.RepositoryAsync<SpareLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
                {
                    return NotFound();
                }

                if (bill.CalVat && ((bill.IGSTAmount == 0 && ((bill.CGSTAmount == 0 || bill.SGSTAmount == 0) ? 1 : 0) == 1) || (bill.IGSTAmount > 0 && (bill.CGSTAmount + bill.SGSTAmount) > 0)))
                {
                    ModelState.AddModelError("GSTAmount", "GSTAmount Amount is not valid");
                }
                if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0 && bill.ExpenseLedgerId2.GetValueOrDefault(0) == 0)
                {
                    ModelState.AddModelError("ExpenseLedgerId", "Atleast on Expense Ledger is required");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bill.VoucherTypeId = 26;
                var sei=_repo.InsertOrUpdatePurchaseBillView(bill);
                
                _uow.SaveChanges();
                bill.Id = sei.Id;
                return Ok(bill.Id);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
        }

        #endregion Store Inward

        #region Store Outward

        [HttpGet, ODataRoute("GetSpareIssueTransaction(key={key})")]
        public vwSparePurchaseBill GetSpareIssueView([FromODataUri] long key)
        {
            return _repo.GetPurchaseBillView(key, 24);
        }

        [HttpGet, ODataRoute("GetSpareOutwardTransaction(key={key})")]
        public vwSparePurchaseBill GetSpareOutwardView([FromODataUri] long key)
        {
            return _repo.GetPurchaseBillView(key, 25);
        }

        [HttpPost, ODataRoute("PostSpareIssueTransaction")]
        public IHttpActionResult PostSpareIssueView(ODataActionParameters odataParam)
        {
            try
            {
                var bill = odataParam["bill"] as vwSparePurchaseBill;
                if (bill == null)
                {
                    return BadRequest("Invalid Parameter");
                }
                if (bill.CurTypeId <= 0 || bill.CurRate <= 0)
                {
                    return BadRequest("Currency Type/ CurRate is required");
                }
                if (bill.Id > 0 && !_uow.RepositoryAsync<SpareLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
                {
                    return NotFound();
                }
                if (bill.CalVat && ((bill.IGSTAmount == 0 && ((bill.CGSTAmount == 0 || bill.SGSTAmount == 0) ? 1 : 0) == 1) || (bill.IGSTAmount > 0 && (bill.CGSTAmount + bill.SGSTAmount) > 0)))
                {
                    ModelState.AddModelError("GSTAmount", "GSTAmount Amount is not valid");
                }
                if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0 && bill.ExpenseLedgerId2.GetValueOrDefault(0) == 0)
                {
                    ModelState.AddModelError("ExpenseLedgerId", "Atleast on Expense Ledger is required");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bill.VoucherTypeId = 24;
                var sle = _repo.InsertOrUpdatePurchaseBillView(bill);
               
                _uow.SaveChanges();
                bill.Id = sle.Id;
                return Ok(bill.Id);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
        }

        [HttpPost, ODataRoute("PostSpareOutwardTransaction")]
        public IHttpActionResult PostSpareOutwardView(ODataActionParameters odataParam)
        {
            try
            {
                var bill = odataParam["bill"] as vwSparePurchaseBill;
                if (bill == null)
                {
                    return BadRequest("Invalid Parameter");
                }
                if (bill.Id > 0 && !_uow.RepositoryAsync<SpareLogExtraInfo>().Queryable().Any(x => x.Id == bill.Id))
                {
                    return NotFound();
                }
                if (bill.CalVat && ((bill.IGSTAmount == 0 && ((bill.CGSTAmount == 0 || bill.SGSTAmount == 0) ? 1 : 0) == 1) || (bill.IGSTAmount > 0 && (bill.CGSTAmount + bill.SGSTAmount) > 0)))
                {
                    ModelState.AddModelError("GSTAmount", "GSTAmount Amount is not valid");
                }
                if (bill.PrimaryDebitAccountId.GetValueOrDefault(0) == 0 && bill.ExpenseLedgerId2.GetValueOrDefault(0) == 0)
                {
                    ModelState.AddModelError("ExpenseLedgerId", "Atleast on Expense Ledger is required");
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                bill.VoucherTypeId = (bill.VoucherTypeId == 0 || bill.VoucherTypeId == null) ? 25 : bill.VoucherTypeId;

                var sei = _repo.InsertOrUpdatePurchaseBillView(bill);
                
                _uow.SaveChanges();
                bill.Id = sei.Id;
                return Ok(bill.Id);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    _uow.Rollback();
                }
                throw;
            }
        }

        #endregion Store Outward

        //// PATCH: odata/SpareLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        /// <exception cref="BusinessException">Not Allowed.</exception>
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<SpareLog> patch)
        {
            //throw new BusinessException(ErrorCode.GLB107, "Update for Individual SpareLog not allowed");
            if (patch.GetEntity().ExtraInfoId.GetValueOrDefault()<=0)
            {
                throw new BusinessException(ErrorCode.GLB107, "Update for Individual SpareLog without ExtraInfoId is not allowed");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SpareLog objSpareLog = await _repo.FindAsync(key);
            if (objSpareLog == null)
            {
                return NotFound();
            }

            objSpareLog.ObjectState = ObjectState.Modified;
            patch.Patch(objSpareLog);
            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return InternalServerError(ex);
            }

            return Updated(objSpareLog); 
        }

        // POST: odata/SpareLogs
        /// <exception cref="BusinessException">Not Allowed.</exception>
        public async Task<IHttpActionResult> Post(SpareLog objSpareLog)
        {
            throw new BusinessException(ErrorCode.GLB107, "Post for Individual SpareLog not allowed");
            objSpareLog.ObjectState = ObjectState.Added;
            _repo.Insert(objSpareLog);
            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return InternalServerError(ex);
            }
            return Created(objSpareLog);
        }

        [HttpPost]
        public IHttpActionResult PostBulkAmcExpenses(ODataActionParameters parameters)
        {
            var iamcexpenses = parameters["amcexpenses"] as IEnumerator<vwSparePurchaseBill>;
            if (iamcexpenses == null) return BadRequest("No Records found to upload");
            var expenses = iamcexpenses.ToList();
            var uow = Request.GetContext();
            _repo.Request = this.Request;
            var transaction = uow.Context.Database.CurrentTransaction ??
                                  uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                //#if !DEBUG
                if (expenses.Any(x=>x.CurTypeId <= 0 || x.CurRate <= 0))
                {
                    return BadRequest("Currency Type/ CurRate is required");
                }

                _repo.AmcBatchInsert(expenses, transaction.UnderlyingTransaction);
                //#elif DEBUG
                //_repo.AmcBatchInsert(expenses, transaction.UnderlyingTransaction is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction);
                //#endif
                //_repo.AmcBatchInsert(expenses, transaction.UnderlyingTransaction is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction);

                if (!Request.IsBatchRequest())
                {
                    transaction.Commit();
                    transaction.Dispose();
                }
                string batchids = expenses.Select(x => x.BatchId).Aggregate(string.Empty, (current, batchid) => current + ((string.IsNullOrWhiteSpace(current) ? "" : "^") + batchid));
                var item = new vwBatch { BatchId = batchids, BatchSize = expenses.Count };
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

        [HttpPost]
        public async Task<IHttpActionResult> PostBulkRepairExpenses(ODataActionParameters parameters)
        {
            var iexpenses = parameters["expenses"] as IEnumerator<vwSparePurchaseBill>;
            if (iexpenses == null) return BadRequest("No Records found to upload");
            var expenses = iexpenses.ToList();
            var uow = Request.GetContext();
            _repo.Request = this.Request;
            var transaction = uow.Context.Database.CurrentTransaction ??
                                  uow.Context.Database.BeginTransaction(IsolationLevel.ReadCommitted);
            try
            {
                if (expenses.Any(x => x.CurTypeId <= 0 || x.CurRate <= 0))
                {
                    return BadRequest("Currency Type/ CurRate is required");
                }

                _repo.BatchInsert(expenses, transaction.UnderlyingTransaction);
                //#elif DEBUG
                //_repo.BatchInsert(expenses, transaction.UnderlyingTransaction is ProfiledTransaction ? ((ProfiledTransaction)transaction.UnderlyingTransaction).Inner : transaction.UnderlyingTransaction);
                //#endif
                var spname = await uow.RepositoryAsync<ReportProcedure>().FindAsync(540);
                string batchids = expenses.Select(x => x.BatchId).Aggregate(string.Empty, (current, batchid) => current + ((string.IsNullOrWhiteSpace(current) ? "" : "^") + batchid));
                var item = new vwBatch { BatchId = batchids, BatchSize = expenses.Count };
                if (spname != null)
                {
                    try
                    {
                        await uow.ExecuteProcedureAsync(spname.StoredProcedureName, new SqlParameter("TransactionId", 0), new SqlParameter("TransactionNumber",JsonConvert.SerializeObject(item)), new SqlParameter("TransactionType", 1017));
                    }
                    catch (SqlException ex)
                    {
                        throw new BusinessException(ex);
                    }
                }
                if (!Request.IsBatchRequest())
                {
                    transaction.Commit();
                    transaction.Dispose();
                }
               
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

        // GET: odata/SpareLogs(5)
        // PUT: odata/SpareLogs(5)
        /// <exception cref="BusinessException">Not Allowed.</exception>
        public async Task<IHttpActionResult> Put(long key, SpareLog objSpareLog)
        {
            throw new BusinessException(ErrorCode.GLB107, "Update for Individual SpareLog not allowed");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objSpareLog.Id)
            {
                return BadRequest();
            }
            objSpareLog.ObjectState = ObjectState.Modified;
            _repo.Update(objSpareLog);

            try
            {
                await _uow.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                return InternalServerError(ex);
            }

            return Updated(objSpareLog);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _uow.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}