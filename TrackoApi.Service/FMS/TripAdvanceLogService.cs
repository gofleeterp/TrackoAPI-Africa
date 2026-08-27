// ***********************************************************************
// Assembly         : TrackoApi.Service
// Author           : Admin
// Created          : 02-07-2016
//
// Last Modified By : Admin
// Last Modified On : 03-30-2016
// ***********************************************************************
// <copyright file="TripAdvanceLogService.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;

using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

using TrackoAPI.Repository;
using TrackoAPI.ViewModels.AMS;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.vw.ts;

namespace TrackoApi.Service
{
    /// <summary>
    /// Interface ITripAdvanceLogService
    /// </summary>
    //TripAdvanceLogS.TripAdvanceLog}" />
    public interface ITripAdvanceLogService : IService<TripAdvanceLog>
    {
        /// <summary>
        /// Gets all trip advance log list.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>IQueryable&lt;TripAdvanceLog&gt;.</returns>
        IQueryable<TripAdvanceLog> GetAllTripAdvanceLogList(int id);
        /// <summary>
        /// Prepares the VDR.
        /// </summary>
        /// <param name="vd">The vd.</param>
        /// <param name="advance">The advance.</param>
        void PrepareVDR(VoucherDetail vd, TripAdvanceLog advance, List<FakeVDRs> _vwVDRs = null);
        /// <summary>
        /// Prepares the vd.
        /// </summary>
        /// <param name="advance">The advance.</param>
        void PrepareVD(TripAdvanceLog advance);
        /// <summary>
        /// Prepares the v.
        /// </summary>
        /// <param name="advance">The advance.</param>
        void PrepareV(TripAdvanceLog advance);
        /// <summary>
        /// Bulks the advance.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="vch">The VCH.</param>
        /// <returns>Voucher.</returns>
        Voucher BulkAdvance(vwAdvanceVoucher doc, Voucher vch);
        Task<Voucher> BulkAdvanceAsync(vwAdvanceVoucher doc, Voucher vch);
        /// <summary>
        /// Gets the queryable bulk entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>IQueryable&lt;vwAdvanceVoucher&gt;.</returns>
        IQueryable<vwAdvanceVoucher> GetQueryableBulkEntryByKey(long key);
        /// <summary>
        /// Gets the bulk entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>vwAdvanceVoucher.</returns>
        vwAdvanceVoucher GetBulkEntryByKey(long key);
        /// <summary>
        /// Bulks the delete.
        /// </summary>
        /// <param name="vch">The VCH.</param>
        void BulkDelete(Voucher vch);

        /// <summary>
        /// Fuels the expanses.
        /// </summary>
        /// <param name="settlementId">The settlement identifier.</param>
        /// <param name="tripLogIds">The trip log ids.</param>
        /// <returns>IQueryable&lt;TripAdvanceLog&gt;.</returns>
        IQueryable<TripAdvanceLog> FuelExpanses(long? settlementId, string tripLogIds = null);

        /// <summary>
        /// Batches the insert.
        /// </summary>
        /// <param name="docs">The docs.</param>
        /// <param name="transaction">The database transaction.</param>
        Task BatchInsert(List<vwAdvanceVoucher> docs, IDbTransaction transaction);
    }
    /// <summary>
    /// Class TripAdvanceLogService.
    /// </summary>
    //TripAdvanceLogS.TripAdvanceLog}" />
    /// <seealso cref="TrackoApi.Service.ITripAdvanceLogService" />
    public class TripAdvanceLogService : Service<TripAdvanceLog>, ITripAdvanceLogService
    {
        /// <summary>
        /// The _repository
        /// </summary>
        private readonly IRepositoryAsync<TripAdvanceLog> _repository;
        /// <summary>
        /// Initializes a new instance of the <see cref="TripAdvanceLogService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// 
        public static long ConstantCurrencyId = 0;
        public TripAdvanceLogService(IRepositoryAsync<TripAdvanceLog> repository) : base(repository)
        {
            _repository = repository;
            try
            {
                var o = _repository.GetRepository<ApiConfiguration>().Queryable().Where(x => x.Key == "ConstantCurrencyId").Select(y => y.Value).FirstOrDefault();
                long.TryParse(o, out ConstantCurrencyId);
            }
            catch { }
        }

        /// <summary>
        /// Gets all trip advance log list.
        /// </summary>
        /// <param name="brandid">The brandid.</param>
        /// <returns>IQueryable&lt;TripAdvanceLog&gt;.</returns>
        public IQueryable<TripAdvanceLog> GetAllTripAdvanceLogList(int brandid)
        {
            return _repository.GetAllTripAdvanceLogList(brandid);
        }
        //public void ValidateCUD(TripAdvanceLog adv)
        //{
        //    if(adv.ObjectState==ObjectState.Added||adv.ObjectState==ObjectState.Modified)
        //    {
        //        /*IsBlackListed*/
        //        var isblackliested=this._repository.GetRepository<DriverMaster>().
        //    }
        //}

        /// <summary>
        /// Prepares the v.
        /// </summary>
        /// <param name="advance">The advance.</param>
        /// 
        public void PrepareV(TripAdvanceLog advance)
        {
            if (advance.fk_Voucher == null)
            {
                if (advance.VoucherId > 0)
                {
                    advance.fk_Voucher = _repository.GetRepository<Voucher>().Queryable().Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).FirstOrDefault();
                }
                if (advance.fk_Voucher == null)
                {
                    advance.fk_Voucher = new Voucher();
                }
                
            }
            advance.fk_Voucher.IsCCRequired = true;
            advance.fk_Voucher.ConstCurTypeId = advance.ConstCurTypeId;
            advance.fk_Voucher.CurTypeId = advance.CurTypeId;
            advance.fk_Voucher.CurRate = advance.CurRate;

            advance.fk_Voucher.OfficeId = advance.OfficeId;
            advance.fk_Voucher.VoucherNo = advance.VoucherNo;
            advance.fk_Voucher.VoucherDate = advance.AdvanceDate;
            advance.fk_Voucher.VoucherDateTime = advance.AdvanceDate;
            advance.fk_Voucher.ObjectState = advance.fk_Voucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            advance.fk_Voucher.VoucherAmount = (advance.Amount * 1) + advance.fk_Voucher.Amount7;
            advance.fk_Voucher.VoucherTypeId = advance.AdvanceTypeId.GetValueOrDefault(0);
            advance.fk_Voucher.Account1Id = advance.DebitAccountId.GetValueOrDefault(0);
            advance.fk_Voucher.Account2Id = advance.CreditAccountId.GetValueOrDefault(0);
            advance.fk_Voucher.Account3Id = advance.IGSTAccountId;
            advance.fk_Voucher.Account4Id = advance.CGSTAccountId;
            advance.fk_Voucher.Account5Id = advance.SGSTAccountId;
            advance.fk_Voucher.Account6Id = advance.RoundUpAccountId;
            advance.fk_Voucher.Amount1 = advance.Amount * 1;
            advance.fk_Voucher.Amount2 = (advance.LoanAdjusted > 0 ? advance.PaidAmount : advance.BasicAmt > 0 ? advance.BasicAmt : advance.Amount) * -1;
            advance.fk_Voucher.Amount3 = advance.IGSTAmt;
            advance.fk_Voucher.Amount4 = advance.CGSTAmt;
            advance.fk_Voucher.Amount5 = advance.SGSTAmt;
            advance.fk_Voucher.Amount6 = advance.RoundUp;
            advance.fk_Voucher.UserRemark = advance.Remark;
            //TODO:Setup Account Narration from Template located with VoucherType
            advance.fk_Voucher.AccountingRemark = "";
            
            /*Currency*/
            advance.fk_Voucher.CurRate = ((advance.fk_Voucher.ConstCurTypeId == advance.fk_Voucher.CurTypeId) || advance.fk_Voucher.CurRate <= 0) ? 1 : advance.fk_Voucher.CurRate;
            
            if (advance.fk_Voucher.CurTypeId != advance.fk_Voucher.ConstCurTypeId & advance.fk_Voucher.CurTypeId.GetValueOrDefault() > 0 && advance.fk_Voucher.CurRate <= 0)
            {
                throw new BusinessException(ErrorCode.CUR100, "V1: Currency Rate need to be defined!!");
            }
        }

        /// <summary>
        /// Prepares the vd.
        /// </summary>
        /// <param name="advance">The advance.</param>
        public void PrepareVD(TripAdvanceLog advance)
        {
            advance.fk_Voucher.VoucherDetails.ForEach(x => x.ObjectState = ObjectState.Deleted);
            var vdDr = new VoucherDetail()
            {
                OfficeId = advance.fk_Voucher.OfficeId,
                AccountId = advance.fk_Voucher.Account1Id.Value,
                OrderId = 1,
                Amount = advance.fk_Voucher.Amount1,
                Narration = advance.fk_Voucher.UserRemark,
                ObjectState = ObjectState.Added,
                VoucherId = advance.fk_Voucher.Id,
                Particular = "Expense Booked",
                ConstCurTypeId = advance.ConstCurTypeId,
                CurTypeId = advance.CurTypeId,
                CurRate = advance.CurRate,
                IsCCRequired= advance.fk_Voucher.IsCCRequired
            };
            var vdCr = new VoucherDetail()
            {
                OfficeId = advance.fk_Voucher.OfficeId,
                AccountId = advance.fk_Voucher.Account2Id.Value,
                OrderId = 2,
                Amount = advance.fk_Voucher.Amount2,
                Narration = advance.fk_Voucher.UserRemark,
                ObjectState = ObjectState.Added,
                VoucherId = advance.fk_Voucher.Id,
                Particular = "Payable Amount",
                ConstCurTypeId = advance.ConstCurTypeId,
                CurTypeId = advance.CurTypeId,
                CurRate = advance.CurRate,
                IsCCRequired = advance.fk_Voucher.IsCCRequired
            };
            advance.fk_Voucher.VoucherDetails.Add(vdCr);
            advance.fk_Voucher.VoucherDetails.Add(vdDr);
            

            if (advance.IGSTAmt > 0)
            {
                if (advance.IGSTAccountId.GetValueOrDefault()>0)
                {
                    var igstvd = new VoucherDetail()
                    {
                        OfficeId = advance.fk_Voucher.OfficeId,
                        AccountId = advance.fk_Voucher.Account3Id ?? 0,
                        OrderId = 3,
                        Amount = advance.fk_Voucher.Amount3,
                        Narration = advance.fk_Voucher.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = advance.fk_Voucher.Id,
                        Particular = "IGST Input",
                        ConstCurTypeId = advance.ConstCurTypeId,
                        CurTypeId = advance.CurTypeId,
                        CurRate = advance.CurRate,
                        IsCCRequired = advance.fk_Voucher.IsCCRequired
                    };
                    advance.fk_Voucher.VoucherDetails.Add(igstvd);
                }
                else
                {
                    throw new BusinessException(ErrorCode.GLB106, "IGST Account is required");
                }
                
            }
            /*
            if (advance.SGSTAmt > 0)
            {
                if (advance.SGSTAccountId.GetValueOrDefault() > 0)
                {
                    var sgstvd = new VoucherDetail()
                    {
                        OfficeId = advance.fk_Voucher.OfficeId,
                        AccountId = advance.fk_Voucher.Account4Id ?? 0,
                        OrderId = 4,
                        Amount = advance.fk_Voucher.Amount4,
                        Narration = advance.fk_Voucher.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = advance.fk_Voucher.Id,
                        Particular = "SGST Input",
                        ConstCurTypeId = advance.ConstCurTypeId,
                        CurTypeId = advance.CurTypeId,
                        CurRate = advance.CurRate,
                        IsCCRequired = advance.fk_Voucher.IsCCRequired
                    };
                    advance.fk_Voucher.VoucherDetails.Add(sgstvd);
                }
                else
                {
                    throw new BusinessException(ErrorCode.GLB106, "SGST Account is required");
                }

            }
            if (advance.CGSTAmt > 0)
            {
                if (advance.CGSTAccountId.GetValueOrDefault() > 0)
                {
                    var cgstvd = new VoucherDetail()
                    {
                        OfficeId = advance.fk_Voucher.OfficeId,
                        AccountId = advance.fk_Voucher.Account5Id ?? 0,
                        OrderId = 5,
                        Amount = advance.fk_Voucher.Amount5,
                        Narration = advance.fk_Voucher.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = advance.fk_Voucher.Id,
                        Particular = "CGST Input",
                        ConstCurTypeId = advance.ConstCurTypeId,
                        CurTypeId = advance.CurTypeId,
                        CurRate = advance.CurRate,
                        IsCCRequired = advance.fk_Voucher.IsCCRequired
                    };
                    advance.fk_Voucher.VoucherDetails.Add(cgstvd);
                }
                else
                {
                    throw new BusinessException(ErrorCode.GLB106, "SGST Account is required");
                }

            }
            */
            if (advance.RoundUp != 0)
            {
                if (advance.RoundUpAccountId.GetValueOrDefault() > 0)
                {
                    var roundupvd = new VoucherDetail()
                    {
                        OfficeId = advance.fk_Voucher.OfficeId,
                        AccountId = advance.fk_Voucher.Account6Id ?? 0,
                        OrderId = 6,
                        Amount = advance.fk_Voucher.Amount6,
                        Narration = advance.fk_Voucher.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = advance.fk_Voucher.Id,
                        Particular = "RoundUp",
                        ConstCurTypeId = advance.ConstCurTypeId,
                        CurTypeId = advance.CurTypeId,
                        CurRate = advance.CurRate,
                        IsCCRequired = advance.fk_Voucher.IsCCRequired
                    };
                    advance.fk_Voucher.VoucherDetails.Add(roundupvd);
                }
                else
                {
                    throw new BusinessException(ErrorCode.GLB106, "RoundUp Account is required");
                }

            }

            /*Driver Loan Adjustment*/
            if (advance.fk_Voucher.Account7Id.GetValueOrDefault() > 0)
            {
                var vdloanCr = new VoucherDetail()
                {
                    OfficeId = advance.fk_Voucher.OfficeId,
                    AccountId = advance.fk_Voucher.Account7Id.Value,
                    OrderId = 7,
                    Amount = advance.fk_Voucher.Amount7,
                    Narration = advance.fk_Voucher.UserRemark,
                    ObjectState = ObjectState.Added,
                    VoucherId = advance.fk_Voucher.Id,
                    Particular = "Loan Adjustement",
                    ConstCurTypeId = advance.ConstCurTypeId,
                    CurTypeId = advance.CurTypeId,
                    CurRate = advance.CurRate,
                    IsCCRequired = advance.fk_Voucher.IsCCRequired
                };
                advance.fk_Voucher.VoucherDetails.Add(vdloanCr);
            }
        }

        /// <summary>
        /// Prepares the VDR.
        /// </summary>
        /// <param name="vd">The vd.</param>
        /// <param name="advance">The advance.</param>
        public void PrepareVDR(VoucherDetail vd, TripAdvanceLog advance,List<FakeVDRs> _vwVDRs=null)
        {

            vd.VoucherDetailReferences.ForEach(x => x.ObjectState = ObjectState.Deleted);

            if (vd.ObjectState == ObjectState.Added)
            {
                if(advance.AdvanceTypeId== 76/*HS Payment*/|| advance.AdvanceTypeId == 78/*HS On Account*/)
                {
                    var ishspayment = advance.AdvanceTypeId == 76;
                    
                    var hsinfo = ishspayment? _repository.GetRepository<VehicleMovementLog>()
                        .Queryable().Where(x => x.Id == advance.TripLogId)
                        .Select(x =>new
                        {
                            x.Id,
                            x.TriplogNo,
                            VDRInfo = x.fk_VDR != null ? new {x.fk_VDR.Amount,x.fk_VDR.ReferenceNo }:null,
                            x.TotalHSAmount,
                            x.MarketFreight,
                           // Amount = x.fk_VDR!=null? x.fk_VDR.Amount:x.HSChg,                            
                            x.VDRId,
                            //x.fk_VDR.ReferenceNo,
                            TotalPaid=x.TripAdvances.Where(y=>y.VoucherId!=vd.VoucherId).Sum(y=> (decimal?)(y.CashAmount>0?y.CashAmount:y.FuelAmount))??0
                        }).FirstOrDefault():null;
                    var hsbalance = hsinfo==null?0: (((hsinfo.VDRInfo?.Amount*-1 ?? (hsinfo.TotalHSAmount>0? hsinfo.TotalHSAmount: hsinfo.MarketFreight))) - hsinfo.TotalPaid - advance.Amount);
                    if (ishspayment && hsbalance<0) 
                    {
                        throw new BusinessException(ErrorCode.VCH109, "Total paid payment amount exceeded hirecharges amount");
                    }
                    if (vd.Amount != 0)/*On Account/Against Reference Payment VDR*/
                    {
                        var vdr = new VoucherDetailReference()
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd.Amount,
                            ReferenceNo = ishspayment&& hsinfo?.VDRId != null ? (hsinfo?.VDRInfo?.ReferenceNo??hsinfo?.TriplogNo) : advance.ReferenceNo,
                            RefId = hsinfo?.VDRId,
                            VDRTypeId = ishspayment && hsinfo?.VDRId != null ? 1014/*Against ref*/ : 1448/*On Account VDR*/,
                            VoucherDetailId = vd.Id,
                            AccountId = vd.AccountId,
                            DueDate = advance.AdvanceDate,
                            TransactionId = advance.Id,
                            IsCCRequired = advance.fk_Voucher.IsCCRequired
                        };
                        if (!ishspayment || (ishspayment && hsinfo?.VDRId != null))
                        {
                            vd.VoucherDetailReferences.Add(vdr);
                            advance.fk_VDR = vdr;
                            advance.VDRId = vdr.Id;
                        }
                    }
                    else/*New VDR for Payment Entries*/
                    {
                        var isRefEnabled =
                _repository.GetRepository<Ledger>()
                    .Queryable()
                    .Where(x => x.Id == vd.AccountId)
                    .Select(y => new { y.ReferenceFlag })
                    .FirstOrDefault();
                        if (isRefEnabled == null || !isRefEnabled.ReferenceFlag) return;
                        var vdr = new VoucherDetailReference()
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd.Amount,
                            ReferenceNo = ishspayment && hsinfo?.VDRId != null ? (hsinfo?.VDRInfo?.ReferenceNo ?? hsinfo?.TriplogNo) : advance.ReferenceNo,
                            VDRTypeId =  1013,
                            VoucherDetailId = vd.Id,
                            AccountId = vd.AccountId,
                            DueDate = advance.AdvanceDate,
                            TransactionId = advance.Id,
                            IsCCRequired = advance.fk_Voucher.IsCCRequired
                        };
                        vd.VoucherDetailReferences.Add(vdr);
                        advance.fk_VDR = vdr;
                        advance.VDRId = vdr.Id;
                    }

                }
                else
                {
                    _vwVDRs = _vwVDRs ?? new List<FakeVDRs>();
                    if (_vwVDRs.Any() && vd.OrderId == 7) {
                        /*Driver Loan Adjustment Vd*/
                        _vwVDRs.ForEach(k =>
                        {
                            var _r = _repository.GetRepository<VoucherDetailReference>()
                            .Queryable()
                            .Where(x => x.Id == k.AVDRId)
                            .Select(y => new { y.Id, y.ReferenceNo, y.CurTypeId, y.CurRate })
                            .FirstOrDefault();

                            if (_r == null) return;
                            if (k.Adjusted == 0) return;

                            
                            var vdr = new VoucherDetailReference()
                            {
                                ObjectState = ObjectState.Added,
                                Amount = -k.Adjusted,
                                Amount_MNC = 0,
                                ReferenceNo = _r.ReferenceNo,
                                VDRTypeId = 1014,/*againstref*/
                                RefId = _r?.Id,/*againstref*/
                                VoucherDetailId = vd.Id,
                                AccountId = vd.AccountId,
                                DueDate = advance.AdvanceDate,
                                TransactionId = advance.Id,
                                ConstCurTypeId = advance.ConstCurTypeId,
                                CurTypeId = advance.CurTypeId,
                                CurRate = advance.CurRate,
                                OldCurRate = k.CurRate,
                                IsCCRequired = true
                            };

                            if /*1 Kwacha - Kwacha*/(advance.CurTypeId == Helper.ConstCurTypeId)
                            {                                
                                if (k.CurTypeId == Helper.ConstCurTypeId && k.Balance==0)
                                {   
                                    /*1.1 Kwacha to Kwacha - Full and Final*/
                                    vdr.VDRRefAmount= -k.BalanceInDocmentValueId;
                                    vdr.Amount_MNC = -k.BalanceInLandingValueId;
                                    vdr.IsCCRequired = false;
                                }
                                else if (k.CurTypeId == Helper.ConstCurTypeId && k.Balance!=0)
                                {
                                    /*1.2 Kwacha to Kwacha - Partial*/
                                    vdr.VDRRefAmount = -k.Adjusted;
                                    vdr.Amount_MNC = -k.Adjusted;
                                    vdr.IsCCRequired = false;
                                }
                                else if (k.CurTypeId != Helper.ConstCurTypeId && k.Balance==0)
                                {
                                    /*2.1 Kwacha to USD - Full and Final*/
                                    vdr.VDRRefAmount = -k.BalanceInDocmentValueId;
                                    vdr.Amount_MNC = -k.BalanceInLandingValueId;
                                    vdr.IsCCRequired = false;
                                }
                                else if (k.CurTypeId != Helper.ConstCurTypeId && k.Balance!=0)
                                {
                                    /*2.2 Kwacha to USD - Partial*/
                                    vdr.VDRRefAmount = -Math.Round(k.Adjusted / k.CurRate, 2);
                                    vdr.Amount_MNC = -k.Adjusted;
                                    vdr.IsCCRequired = false;
                                }
                            }
                            else {
                                if (k.CurTypeId == advance.CurTypeId && k.Balance==0)
                                {
                                    /*3.1 USD to USD - Full and Final*/
                                    vdr.VDRRefAmount = -k.BalanceInDocmentValueId;
                                    vdr.Amount_MNC = -k.BalanceInLandingValueId;
                                    vdr.IsCCRequired = false;
                                }
                                else if (k.CurTypeId == advance.CurTypeId && k.Balance!=0)
                                {
                                    /*3.2 USD to USD - Partial*/
                                    vdr.VDRRefAmount = -k.Adjusted;
                                    vdr.IsCCRequired = true;
                                }
                                else if (k.CurTypeId != advance.CurTypeId && k.Balance==0)
                                {
                                    /*4.1 USD to Kwacha - Full and Final*/
                                    vdr.VDRRefAmount = -k.BalanceInDocmentValueId;
                                    vdr.Amount_MNC = -k.BalanceInDocmentValueId;
                                    vdr.IsCCRequired = false;
                                }
                                else if (k.CurTypeId != advance.CurTypeId && k.Balance!=0)
                                {
                                    /*4.2 USD to Kwacha - Partial*/
                                    vdr.VDRRefAmount = -Math.Round(k.Adjusted * k.CurRate, 2);
                                    vdr.Amount_MNC = Math.Round(k.Adjusted * k.CurRate, 2);
                                    vdr.IsCCRequired = false;
                                }
                            }
                            vd.VoucherDetailReferences.Add(vdr);
                        });
                    }
                    else
                    {
                        bool forcerefenabled = false;
                        /*if debit is tca ref will be forcily enabled*/
                        if ((advance.AdvanceTypeId == 94 && advance.CreditAccountId == vd.AccountId) || advance.AdvanceTypeId == 1 || advance.AdvanceTypeId == 2 || advance.AdvanceTypeId == 3 || (advance.AdvanceTypeId == 133 && vd.Amount > 0/*in shortage vdr is required at debit side only*/))
                        {
                            forcerefenabled = true;
                        }

                        if (!forcerefenabled)
                        {
                            var isRefEnabled =
                            _repository.GetRepository<Ledger>()
                            .Queryable()
                            .Where(x => x.Id == vd.AccountId)
                            .Select(y => new { y.ReferenceFlag })
                            .FirstOrDefault();

                            if (isRefEnabled == null || !isRefEnabled.ReferenceFlag) return;
                        }

                        advance.ReferenceNo = (advance.AdvanceTypeId == 94) ? advance.VoucherNo : advance.ReferenceNo;

                        var vdrtype = 1013/*NewRef*/;

                        var vdr = new VoucherDetailReference()
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd.Amount,
                            ReferenceNo = (vd.Amount > 0 ? advance.VoucherNo : advance.ReferenceNo),
                            VDRTypeId = vdrtype,
                            VoucherDetailId = vd.Id,
                            AccountId = vd.AccountId,
                            DueDate = advance.AdvanceDate,
                            TransactionId = advance.Id,
                            ConstCurTypeId = advance.ConstCurTypeId,
                            CurTypeId = advance.CurTypeId,
                            CurRate = advance.CurRate,
                            IsCCRequired = advance.fk_Voucher.IsCCRequired
                        };
                        

                        vd.VoucherDetailReferences.Add(vdr);
                        advance.fk_VDR = vdr;
                        advance.VDRId = vdr.Id;
                    }
                }
                
            }
        }

        /// <summary>
        /// Prepares the naration.
        /// </summary>
        /// <param name="advance">The advance.</param>
        public void PrepareNaration(TripAdvanceLog advance)
        {

        }
        /// <summary>
        /// Gets the queryable bulk entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>IQueryable&lt;vwAdvanceVoucher&gt;.</returns>
        public IQueryable<vwAdvanceVoucher> GetQueryableBulkEntryByKey(long key)
        {
            var listOppLineData = new Queue<vwAdvanceVoucher>();
            var vch = _repository.GetBulkEntryByVoucherId(key);
            if (vch == null)
            {
                return listOppLineData.AsQueryable();
            }
            listOppLineData.Enqueue(vch);
            return listOppLineData.AsQueryable();
        }

        /// <summary>
        /// Gets the bulk entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>vwAdvanceVoucher.</returns>
        public vwAdvanceVoucher GetBulkEntryByKey(long key)
        {
            return _repository.GetBulkEntryByVoucherId(key);
        }
        public Task<Voucher> BulkAdvanceAsync(vwAdvanceVoucher doc, Voucher vch)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Bulks the advance.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="vch">The VCH.</param>
        /// <returns>Voucher.</returns>
        /// <exception cref="BusinessException">
        /// </exception>
        public Voucher BulkAdvance(vwAdvanceVoucher doc, Voucher vch)
        {
            vch = vch ?? new Voucher();
            vch.PageId = doc.PageId;
            var newAdvs = new List<TripAdvanceLog>();
            //if(doc.TripAdvanceLogs.Where(x=>x.TripLogId))
            for (int i = 0; i < doc.TripAdvanceLogs.Count; i++)
            {
                var ad = doc.TripAdvanceLogs.ElementAt(i);
                var adv = this.Find(ad.AdvanceId) ?? new TripAdvanceLog();
                adv.Id = ad.AdvanceId;
                adv.ReferenceNo = string.IsNullOrWhiteSpace(ad.ReferenceNo) ? (doc.TripAdvanceLogs.Count(x => string.IsNullOrWhiteSpace(x.ReferenceNo)) > 1 ? doc.DocumentNo + "-" + i : doc.DocumentNo) : ad.ReferenceNo;
                adv.AdvanceDate = ad.AdvanceDate;
                adv.fk_Voucher = vch;
                adv.VoucherNo = doc.DocumentNo;
                adv.SettledRefId = ad.SettledRefId;
                adv.ObjectState = ad.AdvanceId > 0 ? (ad.SettlementId.GetValueOrDefault(0) == 0 ? ObjectState.Modified : ObjectState.Unchanged) : ObjectState.Added;
                adv.FuelAmount = ad.FuelQty * ad.FuelRate;
                //adv.Amount = ad.FuelQty > 0 ? ad.FuelAmount : ad.CashAmount;
                adv.VoucherId = vch.Id;
                adv.FuelQty = ad.FuelQty;
                adv.AdvanceTypeId = ad.AdvanceTypeId;
                adv.ExpenseId = ad.ExpenseId;
                adv.CashAmount = ad.CashAmount;
                adv.OfficeId = doc.OfficeId;
                adv.CreditAccountId = doc.CrAccountId;
                adv.FuelRate = ad.FuelRate;
                adv.DebitAccountId = doc.DrAccountId;
                adv.DriverId = ad.DriverId.GetValueOrDefault()==0?null:ad.DriverId;
                adv.FuelId = ad.FuelId;
                adv.Remark = ad.Remark;
                adv.TripLogId = ad.TripLogId;
                adv.VehicleId = ad.VehicleId;
                adv.HireVehicleId = ad.HireVehicleId;
                adv.ViewId = ad.ViewId;
                adv.PaidInId = (ad.PaidInId <= 0 || ad.PaidInId == null) ? 1430 : ad.PaidInId; //1430=Cash
                adv.IsBulkEntry = true;
                adv.Ref1 = ad.Ref1;
                adv.ThirdPartyRefNo = ad.ThirdPartyRefNo;
                adv.IGSTAccountId = ad.IGSTAccountId??doc.IGSTAccountId;
                adv.IGSTRate = ad.IGSTRate;
                adv.IGSTAmt = ad.IGSTAmount;
                adv.CGSTAccountId = ad.CGSTAccountId?? doc.CGSTAccountId ;
                adv.CGSTRate = ad.CGSTRate;
                adv.CGSTAmt = ad.CGSTAmount;
                adv.SGSTAccountId = ad.SGSTAccountId?? doc.SGSTAccountId;
                adv.SGSTRate = ad.SGSTRate;
                adv.SGSTAmt = ad.SGSTAmount;
                adv.HSNCodeId = ad.HSNCodeId ?? doc.HSNCodeId; 
                adv.BasicAmt = ad.NetAmount;
                if (adv.DataView == null)
                {
                    adv.DataView=new List<JsonDataEntity>();
                }
               
                if (ad.DataView != null && ad.DataView.Any())
                {
                    adv.DataView?.RemoveAll(x => ad.DataView.Any(y => y.DataName == x.DataName));
                    foreach (var entity in ad.DataView)
                    {
                        adv.DeleteAndAdd(entity);
                    }
                }
                if ((adv.IGSTAmt + adv.CGSTAmt + adv.SGSTAmt) == 0)
                {
                    adv.BasicAmt = adv.Amount;
                }
            newAdvs.Add(adv);
            }
            //Delete All the Advance that was mapped to this voucherid before now but not now
            var ids = newAdvs.Select(x => x.Id);
            var deletedRecords = (from a in Queryable().Where(x => x.VoucherId == vch.Id)
                                  where !ids.Contains(a.Id)
                                  select a).ToList();
            foreach (var x in deletedRecords)
            {
                if (x.SettlementId.HasValue) throw new BusinessException(ErrorCode.TADV105, $"Reference No {x.ReferenceNo} is settled");
                x.ObjectState = ObjectState.Deleted;
                x.VoucherId = 0;
                x.fk_Voucher = null;
                Delete(x);
            }

            //Prepare Voucher And Voucher Details
            var vchamount = Math.Abs(newAdvs.Sum(x => x.Amount));
            PrepareBulkV(vchamount, vch, doc);
            if (vch.VoucherAmount == 0)
            {
                throw new BusinessException(ErrorCode.VCH113,"Sum of total records cannot be zero");
            }
            vch.ViewId = doc.ViewId;

            foreach (VoucherDetail detail in vch.VoucherDetails)
            {
                PrepareBulkVdr(detail, newAdvs, deletedRecords);
            }
            #region Validations
            if (vch.Amount1 + vch.Amount2 != 0 || vch.VoucherDetails.Sum(x => x.Amount) != 0)
            {
                throw new BusinessException(ErrorCode.VCH104);//Credit and Debit Amount mismatch for Voucher
            }
            if (vch.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) <= 1)
            {
                throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
            }
            //if (vch.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) == 0)
            //{
            //    throw new BusinessException(ErrorCode.TADV102);//Atlead one VDR is Required in Advance Transaction
            //}
            if (vch.VoucherDetails.Any(voucherDetail => voucherDetail.VoucherDetailReferences.Count(x => x.ObjectState != ObjectState.Deleted) > 0 && voucherDetail.Amount != voucherDetail.VoucherDetailReferences.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.Amount)))
            {
                throw new BusinessException(ErrorCode.VCH106);//VoucherDetail and VoucherDetailReference Amount Doesn't Tally
            }
            #endregion
            foreach (var log in newAdvs)
            {
                if (log.Id > 0)
                {
                    Update(log);
                }
                else
                {
                    Insert(log);
                }
            }
            return vch;
        }
        /// <summary>
        /// Prepares the bulk v.
        /// </summary>
        /// <param name="totalAmt">The total amt.</param>
        /// <param name="vch">The VCH.</param>
        /// <param name="vw">The vw.</param>
        public void PrepareBulkV(decimal totalAmt, Voucher vch, vwAdvanceVoucher vw)
        {
            vch.ConstCurTypeId = vw.ConstCurTypeId;
            vch.CurTypeId = vw.CurTypeId;
            vch.CurRate = vw.CurRate;
            vch.IsCCRequired = true;
            vch.OfficeId = vw.OfficeId;
            vch.VoucherNo = vw.DocumentNo;
            vch.VoucherDate = vw.DocumentDate;
            vch.VoucherDateTime = vw.DocumentDate;
            vch.ObjectState = vch.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            vch.VoucherAmount = totalAmt;
            vch.VoucherTypeId = vw.AdvanceTypeId;
            vch.Account1Id = vw.DrAccountId;
            vch.Account2Id = vw.CrAccountId;
            vch.Amount1 = totalAmt;
            vch.Amount2 = -totalAmt;
            vch.UserRemark = vw.Remark;
            //TODO:Setup Account Narration from Template located with VoucherType
            vch.AccountingRemark = "";
            //Prepare Voucher Details
            PrepareBulkVd(vch, vw);
        }

        /// <summary>
        /// Prepares the bulk vd.
        /// </summary>
        /// <param name="vch">The VCH.</param>
        /// <param name="vw">The vw.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        /// <exception cref="BusinessException">VoucherDetails.Count LT 2</exception>
        public void PrepareBulkVd(Voucher vch, vwAdvanceVoucher vw)
        {
            try
            {
                vch.VoucherDetails?.RemoveAll(x => x.Id == 0);
            }
            catch (Exception)
            {
                //Ignore
            }


            if (vch.Id > 0 && vch.VoucherDetails != null && vch.VoucherDetails.TrueForAll(x => x.Id > 0))
            {
                if (vch.VoucherDetails.Count < 2)
                {
                    throw new BusinessException(ErrorCode.VCH105);
                }
                foreach (var detail in vch.VoucherDetails)
                {
                    detail.ConstCurTypeId = vch.ConstCurTypeId;
                    detail.CurTypeId = vch.CurTypeId;
                    detail.CurRate = vch.CurRate;
                    detail.IsCCRequired = vch.IsCCRequired;
                    detail.OfficeId = vch.OfficeId;
                    detail.AccountId = detail.OrderId == 1 ? vch.Account1Id.GetValueOrDefault() : vch.Account2Id.GetValueOrDefault();
                    detail.OrderId = detail.OrderId == 1 ? 1 : 2;
                    detail.Amount = detail.OrderId == 1 ? vch.Amount1 : vch.Amount2;
                    detail.Narration = vch.UserRemark;
                    detail.ObjectState = ObjectState.Modified;
                    detail.VoucherId = vch.Id;
                }
            }
            else
            {
                if (vch.VoucherDetails == null)
                {
                    vch.VoucherDetails = new List<VoucherDetail>();
                }
                for (var i = 1; i <= 2; i++)
                {
                    var vd = new VoucherDetail()
                    {
                        IsCCRequired = vch.IsCCRequired,
                        OfficeId = vch.OfficeId,
                        AccountId = i == 1 ? vch.Account1Id.GetValueOrDefault() : vch.Account2Id.GetValueOrDefault(),
                        OrderId = i,
                        Amount = i == 1 ? vch.Amount1 : vch.Amount2,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        ConstCurTypeId = vch.ConstCurTypeId,
                        CurTypeId = vch.CurTypeId,
                        CurRate = vch.CurRate
                    };
                    vch.VoucherDetails.Add(vd);
                }
            }

        }

        /// <summary>
        /// Prepares the bulk Voucher Detail Reference.
        /// </summary>
        /// <param name="v">The Voucher Detail</param>
        /// <param name="a">The Active Advances</param>
        /// <param name="d">Deleted Advances</param>
        public void PrepareBulkVdr(VoucherDetail v, List<TripAdvanceLog> a, List<TripAdvanceLog> d)
        {
            var existingVdrIds = v.VoucherDetailReferences?.Select(x => (long?)x.Id).ToList() ?? new List<long?>();
            var vdrDbRefs = _repository.GetRepository<VoucherDetailReference>().Queryable().Where(x => existingVdrIds.Contains(x.RefId)).Select(x => x.RefId).Distinct().ToList();
            var settledRefNos = a.Where(x => x.SettlementId.HasValue).Select(x => x.ReferenceNo);
            if (v.VoucherDetailReferences != null && v.VoucherDetailReferences.Any())
            {
                vdrDbRefs.AddRange(v.VoucherDetailReferences.Where(x => settledRefNos.Contains(x.ReferenceNo)).Select(x => (long?)x.Id).ToList());
                vdrDbRefs = vdrDbRefs.Distinct().ToList();
            }

            //Mark VDR's as Deleted only those are Unsettled
            foreach (VoucherDetailReference reference in v.VoucherDetailReferences)
            {
                if (vdrDbRefs.FirstOrDefault(x => x == reference.Id) == null)
                {
                    reference.ObjectState = ObjectState.Deleted;
                }
                else
                {
                    reference.ObjectState = ObjectState.Unchanged;
                }
                reference.IsCCRequired = v.IsCCRequired;
            }
            var lRepo = _repository.GetRepository<Ledger>().Queryable();
            var isRefEnabled = lRepo.Any(x => x.Id == v.AccountId && x.ReferenceFlag);
            if (!isRefEnabled) return;
            foreach (TripAdvanceLog log in a.Where(x => x.SettlementId.GetValueOrDefault(0) == 0))
            {
                //if (v.ObjectState == ObjectState.Added)
                //{

                //}

                var vdr = new VoucherDetailReference()
                {
                    ObjectState = ObjectState.Added,
                    Amount = v.Amount > 0 ? log.Amount : -log.Amount,
                    ReferenceNo = log.ReferenceNo,
                    VDRTypeId = 1013,
                    VoucherDetailId = v.Id,
                    ConstCurTypeId = v.ConstCurTypeId,
                    CurTypeId = v.CurTypeId,
                    CurRate = v.CurRate,
                    IsCCRequired = v.IsCCRequired
                };
                v.VoucherDetailReferences.Add(vdr);
                log.fk_VDR = vdr;
                log.VDRId = vdr.Id;
                if(log.ObjectState==ObjectState.Unchanged)log.ObjectState=ObjectState.Modified;
            }
        }

        /// <summary>
        /// Bulks the delete.
        /// </summary>
        /// <param name="vch">The VCH.</param>
        /// <exception cref="BusinessException">Condition.</exception>
        public void BulkDelete(Voucher vch)
        {
            var qr = Queryable().Where(x => x.VoucherId == vch.Id).ToList();
            if (qr.Any(x => x.SettlementId.HasValue))
            {
                throw new BusinessException(ErrorCode.TADV105, qr.Where(x => x.SettlementId.HasValue).Select(x => x.ReferenceNo).JoinStrings(" , "));
            }
            qr.ForEach(x =>
            {
                x.ObjectState = ObjectState.Deleted;
                x.fk_Voucher = vch;
            });
            vch.ObjectState = ObjectState.Deleted;
            vch.VoucherDetails.ForEach(x => { x.ObjectState = ObjectState.Deleted; x.VoucherDetailReferences.ForEach(y => y.ObjectState = ObjectState.Deleted); });

        }

        /// <summary>
        /// Fuels the expanses.
        /// </summary>
        /// <param name="settlementId">The settlement identifier.</param>
        /// <param name="tripLogIds">The trip log ids.</param>
        /// <returns>IQueryable&lt;TripAdvanceLog&gt;.</returns>
        public IQueryable<TripAdvanceLog> FuelExpanses(long? settlementId, string tripLogIds = null)
        {
            List<long> tslids = new List<long>();
            if (!string.IsNullOrWhiteSpace(tripLogIds))
            {
                long id = 0;
                tripLogIds.Split(',').ToList().ForEach(x =>
                {

                    if (long.TryParse(x, out id))
                    {
                        tslids.Add(id);
                    }
                });
            }

            if (tslids.Any())
            {
                return this.Queryable().Where(x => tslids.Contains(x.TripLogId.GetValueOrDefault(0)));
            }
            if (settlementId.HasValue)
            {
                return
                    this.Queryable()
                        .Where(x => x.FuelExpanses.All(y => y.SettlementId.HasValue && y.SettlementId == settlementId));
            }
            return null;
        }
        #region Batch Methods

        public async Task BatchInsert(List<vwAdvanceVoucher> docs,IDbTransaction transaction)
        {
            if(docs.Any(x=> x.TripAdvanceLogs==null||x.TripAdvanceLogs.Count<=0)) throw new BusinessException(ErrorCode.GLB106,"One of Voucher does not have Advance Details");
            var vs=new List<Voucher>();
            var vds=new List<VoucherDetail>();
            var vdrs=new List<VoucherDetailReference>();
            var advances=new List<TripAdvanceLog>();
            var acids = docs.Select(x => x.CrAccountId).Union(docs.Select(x => x.DrAccountId)).Distinct().ToList();
            var acrefs= await _repository.GetRepository<Ledger>().Queryable().AsNoTracking().Select(x=>new {x.Id,x.ReferenceFlag}).Where(x=>acids.Contains(x.Id)).ToListAsync();
            var doe = DateTime.Now;
            //var financeStatus = TrackoApi.Core.Helpers.Helper.GetFinanceStatus();
            var dictionary=new Dictionary<int,long>();
            foreach (var doc in docs)
            {

                var vch = new Voucher { PageId = doc.PageId,
                    ConstCurTypeId = Helper.ConstCurTypeId,
                    CurTypeId = doc.CurTypeId,
                    CurRate = doc.CurRate,
                    IsCCRequired = true
                };
                var batchid = Guid.NewGuid().ToString("N");
                
                
                long? fy;
                var fykey = doc.DocumentDate.Date.Month;
                if (dictionary.ContainsKey(fykey))
                {
                    fy = dictionary[fykey];
                }
                else
                {
                    var fydb=this._repository.GetRepository<FinancialYear>().Queryable().Where(
                        x =>
                            x.OpeningDate <= doc.DocumentDate && x.ClosingDate >= doc.DocumentDate &&
                            x.IsActive).Select(x => new { x.Id, x.IsLocked, x.Name }).FirstOrDefault();
                    fy = fydb?.Id;
                    if (fydb == null)
                    {
                        throw new BusinessException(ErrorCode.VCH100, $"Document Date is {doc.DocumentDate.Date:dd-MM-yyyy}");
                    }
                    if (fydb.IsLocked)
                    {
                        throw new BusinessException(ErrorCode.VCH112,$"Document Date is {doc.DocumentDate.Date:dd-MM-yyyy}");
                    }
                }
                        

                //switch (financeStatus)
                //{
                //    case FinanceStatus.NA:
                //        vch.FinancialYearId = null;
                //        vch.IsAccepted = false;
                //        vch.IsAccountsVisiblity = false;
                //        vch.IsAudited = false;
                //        break;
                //    case FinanceStatus.DirectImport:
                        vch.FinancialYearId = fy;
                        vch.IsAccepted = true;
                        vch.IsAccountsVisiblity = true;
                        vch.IsAudited = false;
                //        break;
                //    case FinanceStatus.ApprovalRequired:

                //        vch.FinancialYearId = fy;
                //        vch.IsAccepted = false;
                //        vch.IsAccountsVisiblity = true;
                //        vch.IsAudited = false;
                //        break;
                //}
                for (var i = 0; i < doc.TripAdvanceLogs.Count; i++)
                {
                    var ad = doc.TripAdvanceLogs.ElementAt(i);
                    var adv = new TripAdvanceLog
                    {
                        ConstCurTypeId = Helper.ConstCurTypeId,
                        CurTypeId = doc.CurTypeId,
                        CurRate = doc.CurRate,
                        Id = ad.AdvanceId,
                        ReferenceNo =
                            string.IsNullOrWhiteSpace(ad.ReferenceNo)
                                ? (doc.TripAdvanceLogs.Count(x => string.IsNullOrWhiteSpace(x.ReferenceNo)) > 1
                                    ? doc.DocumentNo + "-" + i
                                    : doc.DocumentNo)
                                : ad.ReferenceNo,
                        AdvanceDate = ad.AdvanceDate,
                        VoucherDate=doc.DocumentDate,
                        fk_Voucher = vch,
                        VoucherNo = doc.DocumentNo,
                        ObjectState = ObjectState.Added,
                        FuelAmount = ad.FuelAmount==0? Math.Round(ad.FuelQty*ad.FuelRate, 2):ad.FuelAmount,
                        VoucherId = vch.Id,
                        FuelQty = Math.Round(ad.FuelQty, 2),
                        AdvanceTypeId = ad.AdvanceTypeId,
                        CashAmount = Math.Round(ad.CashAmount, 2),
                        OfficeId = doc.OfficeId,
                        CreditAccountId = doc.CrAccountId,
                        FuelRate = Math.Round(ad.FuelRate, 2),
                        DebitAccountId = doc.DrAccountId,
                        DriverId = ad.DriverId.GetValueOrDefault()==0?null:ad.DriverId,
                        FuelId = ad.FuelId,
                        Remark = ad.Remark,
                        TripLogId = ad.TripLogId,
                        VehicleId = ad.VehicleId,
                        HireVehicleId = ad.HireVehicleId,
                        ViewId = ad.ViewId,
                        PaidInId = (ad.PaidInId <= 0 || ad.PaidInId == null) ? 1430 : ad.PaidInId,
                        IsBulkEntry = true,
                        Ref1 = ad.Ref1,
                        ThirdPartyRefNo=ad.ThirdPartyRefNo,
                        BatchId = batchid,
                        CreatedSessionId = Helper.SessionId(),
                        CreatedDOE = doe,
                        HSNCodeId=ad.HSNCodeId,
                        IGSTAccountId=ad.IGSTAccountId,
                        IGSTRate=ad.IGSTRate,
                        IGSTAmt=ad.IGSTAmount,
                        CGSTAccountId = ad.CGSTAccountId,
                        CGSTRate = ad.CGSTRate,
                        CGSTAmt = ad.CGSTAmount,
                        SGSTAccountId = ad.SGSTAccountId,
                        SGSTRate = ad.SGSTRate,
                        SGSTAmt = ad.SGSTAmount,
                        BasicAmt=ad.NetAmount
                    };
                    //adv.Amount = ad.FuelQty > 0 ? ad.FuelAmount : ad.CashAmount;
                    //1430=Cash
                    if ((adv.IGSTAmt + adv.CGSTAmt + adv.SGSTAmt) == 0)
                    {
                        adv.BasicAmt = adv.Amount;
                    }
                    advances.Add(adv);
                }
                var basicAmt = advances.Where(x=>x.BatchId== batchid).Sum(x => x.Amount);
                var netamount = advances.Where(x => x.BatchId == batchid).Sum(x => x.BasicAmt);
                var igstamt = advances.Where(x => x.BatchId == batchid).Sum(x => x.IGSTAmt);
                var cgstamt = advances.Where(x => x.BatchId == batchid).Sum(x => x.CGSTAmt);
                var sgstamt = advances.Where(x => x.BatchId == batchid).Sum(x => x.SGSTAmt);
                if (netamount == 0)
                {
                    netamount = basicAmt;
                }
                vch.OfficeId = doc.OfficeId;
                vch.VoucherNo = doc.DocumentNo;
                vch.VoucherDate = doc.DocumentDate;
                vch.VoucherDateTime = doc.DocumentDate;
                vch.ObjectState = vch.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                vch.VoucherAmount = netamount;
                vch.VoucherTypeId = doc.AdvanceTypeId;
                vch.Account1Id = doc.DrAccountId;
                vch.Account2Id = doc.CrAccountId;
                vch.Amount1 = basicAmt * 1;
                vch.Amount2 = netamount * -1;
                if (igstamt > 0)
                {
                    vch.Amount3 = igstamt;
                    vch.Account3Id = doc.IGSTAccountId;
                }
                if (cgstamt > 0)
                {
                    vch.Amount4 = cgstamt;
                    vch.Account4Id = doc.CGSTAccountId;
                }
                if (sgstamt > 0)
                {
                    vch.Amount5 = sgstamt;
                    vch.Account5Id = doc.SGSTAccountId;
                }
                vch.UserRemark = doc.Remark;
                //TODO:Setup Account Narration from Template located with VoucherType
                vch.AccountingRemark = "";
                vch.BatchId =doc.BatchId= batchid;
                vch.ViewId = doc.ViewId;
                vch.CreatedSessionId = Helper.SessionId();
                vch.CreatedDOE = doe;
                vs.Add(vch);
                if (vch.VoucherDetails == null)
                {
                    vch.VoucherDetails = new List<VoucherDetail>();
                }
                for (var i = 1; i <= 5; i++)
                {
                    var vd = new VoucherDetail
                    {
                        OfficeId = vch.OfficeId,
                        AccountId = vch.GetPropertyValue<long?>($"Account{i}Id") ?? 0,// i == 1 ? vch.Account1Id.Value : vch.Account2Id.Value,
                        OrderId = i,
                        Amount = vch.GetPropertyValue<decimal>($"Amount{i}"), //i == 1 ? vch.Amount1 : vch.Amount2,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        BatchId = vch.BatchId,
                        CreatedSessionId = Helper.SessionId(),
                        CreatedDOE = doe,
                        ConstCurTypeId = vch.ConstCurTypeId,
                        CurTypeId = vch.CurTypeId,
                        CurRate = vch.CurRate,
                        IsCCRequired = vch.IsCCRequired,
                    };
                    vd.Amount_MNC = vd.Amount;

                    if (vd.IsCCRequired)
                    {
                        vd.Amount_MNC = vd.Amount * vd.CurRate;
                    }

                    if (vd.Amount != 0)
                    {

                        vch.VoucherDetails.Add(vd);
                        vds.Add(vd);
                        if (vd.VoucherDetailReferences == null) vd.VoucherDetailReferences = new List<VoucherDetailReference>();
                        //var lRepo = _repository.GetRepository<Ledger>().Queryable();
                        var isRefEnabled = acrefs.Any(x => x.Id == vd.AccountId && x.ReferenceFlag);
                        if (isRefEnabled && i <= 2)
                        {
                            foreach (TripAdvanceLog log in advances.Where(x => x.BatchId == batchid))
                            {
                                var vdr = new VoucherDetailReference
                                {
                                    ObjectState = ObjectState.Added,
                                    //Amount = vd.Amount > 0 ? log.Amount : -log.Amount,
                                    ReferenceNo = log.ReferenceNo,
                                    VDRTypeId = 1013,
                                    VoucherDetailId = vd.Id,
                                    BatchId = batchid,
                                    CreatedSessionId = Helper.SessionId(),
                                    CreatedDOE = doe,
                                    AccountId = vd.AccountId,
                                    DueDate = log.AdvanceDate,
                                    ConstCurTypeId = vd.ConstCurTypeId,
                                    CurTypeId = vd.CurTypeId,
                                    CurRate = vd.CurRate,
                                    IsCCRequired = vch.IsCCRequired,

                                };
                                switch (i)
                                {
                                    case 1/*Expense A/c*/:
                                        vdr.Amount = vd.Amount > 0 ? log.Amount : -log.Amount;
                                        break;
                                    case 2/*Vendor A/c*/:
                                        vdr.Amount = vd.Amount > 0 ? log.BasicAmt : -log.BasicAmt;
                                        break;
                                }
                                vdr.Amount_MNC = vdr.Amount;
                                if (vdr.IsCCRequired)
                                {
                                    vdr.Amount_MNC = vdr.Amount * vdr.CurRate;
                                }

                                vd.VoucherDetailReferences.Add(vdr);
                                vdrs.Add(vdr);
                            }
                        }
                    }
                }
                #region Validations
                if (vch.VoucherDetails.Sum(x => x.Amount) != 0)
                {
                    throw new BusinessException(ErrorCode.VCH104);//Credit and Debit Amount mismatch for Voucher
                }
                if (vch.VoucherDetails.Count <= 1)
                {
                    throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                }
                //if (vch.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) == 0)
                //{
                //    throw new BusinessException(ErrorCode.TADV102);//Atlead one VDR is Required in Advance Transaction
                //}
                if (vch.VoucherDetails.Any(voucherDetail => voucherDetail.VoucherDetailReferences.Count > 0 && voucherDetail.Amount != voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)))
                {
                    throw new BusinessException(ErrorCode.VCH106,$"Doc No {doc.DocumentNo}");//VoucherDetail and VoucherDetailReference Amount Doesn't Tally
                }
                #endregion
            }
            //Insert Vouchers
            this._repository.UOW.BulkInsert(vs,transaction);
            //Insert Vouchers Details
            var vids = vs.Select(x => x.BatchId).ToList();
            var vsbatches =await
                _repository.GetRepository<Voucher>()
                    .Queryable()
                    .Where(y => vids.Contains(y.BatchId)).Select(x=>new {x.BatchId,x.Id}).ToListAsync();
            Parallel.ForEach(vds, vd =>
            {
                vd.VoucherId = vsbatches?.FirstOrDefault(x => x.BatchId == vd.BatchId)?.Id??0;
            });
            if(vds.Any(x=>x.VoucherId==0))throw new BusinessException(ErrorCode.GLB106,"Voucher Integrity Failed!!");
            await Task.Factory.StartNew(()=> this._repository.UOW.BulkInsert(vds, transaction));
            
            //Insert Voucher Details
            var vdrsbatches =
               await  _repository.GetRepository<VoucherDetail>()
                    .Queryable()
                    .Where(y => vids.ToList().Contains(y.BatchId)).Select(x => new { x.BatchId, x.Id,x.OrderId }).ToListAsync();
            Parallel.ForEach(vds, vd =>
            {
                foreach (var vdr in vd.VoucherDetailReferences)
                {
                    vdr.VoucherDetailId= vdrsbatches?.FirstOrDefault(x => x.BatchId == vdr.BatchId && x.OrderId == vd.OrderId)?.Id ?? 0;
                }
            });
            if (vdrs.Any(x => x.VoucherDetailId == 0)) throw new BusinessException(ErrorCode.GLB106, "Voucher Reference Integrity Failed!!");
            await Task.Factory.StartNew(() => this._repository.UOW.BulkInsert(vdrs, transaction));
            //Insert Advances
            var vdids = vdrsbatches.Select(x => x.Id).ToList();
            var vdrids =await _repository.GetRepository<VoucherDetailReference>().Queryable()
                .Where(x => vdids.Contains(x.VoucherDetailId)).Select(x => new
                {
                    x.Id,
                    x.ReferenceNo,
                    x.BatchId
                }).ToListAsync();
            Parallel.ForEach(advances, ad =>
            {
                ad.VoucherId = vsbatches?.FirstOrDefault(x => x.BatchId == ad.BatchId)?.Id ?? 0;
                ad.VDRId = vdrids?.FirstOrDefault(x => x.BatchId == ad.BatchId && x.ReferenceNo == ad.ReferenceNo)?.Id;
            });
            if (advances.Any(x => x.VoucherId == 0)) throw new BusinessException(ErrorCode.GLB106, "Voucher Advance Mapping Integrity Failed!!");
            this._repository.UOW.BulkInsert(advances, transaction);
        }

        

        #endregion
    }
}
