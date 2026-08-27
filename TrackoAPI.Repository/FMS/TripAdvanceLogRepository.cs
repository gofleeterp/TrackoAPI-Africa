using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.Models.Shared;
using System.Data.Entity;
using TrackoApi.Models.Base;
using TrackoAPI.vw.ts;

namespace TrackoAPI.Repository
{
   public static class TripAdvanceLogRepository
    {
        public static IQueryable<TripAdvanceLog> GetAllTripAdvanceLogList(this IRepository<TripAdvanceLog> repository,
            long id)
        {
            return repository
                .Queryable()
                .Where(x => id == x.Id);

        }
        public static vwAdvanceVoucher GetBulkEntryByVoucherId(this IRepository<TripAdvanceLog> repository, long key)
        {
            //var status= Helper.GetFinanceStatus()==FinanceStatus.ApprovalRequired;
            var voucher = repository.GetRepository<Voucher>().Queryable().Where(x => x.Id == key).Select(x => new vwAdvanceVoucher()
            {
                AdvanceTypeId = x.VoucherTypeId,
                OfficeId = x.OfficeId,
                Remark = x.UserRemark,
                AdvanceType = x.FK_VoucherType.VoucherTypeName,
                CrAccountId = x.Account2Id.Value,
                CrAccountName = x.Account2.AccountName,
                DocumentDate = x.VoucherDate,
                Id = x.Id,
                DocumentNo = x.VoucherNo,
                DrAccountId = x.Account1Id.Value,
                DrAccountName = x.Account1.AccountName,
                NetAmount = x.VoucherAmount,
                OfficeName = x.fk_Office.OfficeName,
                IsLocked = x.IsAccountsVisiblity&&x.IsAudited,
                PageId = x.PageId
            }).FirstOrDefault();
            if (voucher == null)
            {
                return null;
            }
            var advances = repository.Queryable()
                .Where(x => x.VoucherId == key)
                .Select(x => new vwTripAdvanceLog()
                {
                    VoucherNo = x.VoucherNo,
                    Amount = x.FuelAmount + x.CashAmount,
                    VoucherId = x.VoucherId,
                    AdvanceDate = x.AdvanceDate,
                    FuelQty = x.FuelQty,
                    AdvanceTypeId = x.AdvanceTypeId,
                    FuelAmount = x.FuelAmount,
                    CashAmount = x.CashAmount,
                    OfficeId = x.OfficeId,
                    CreditAccountId = x.CreditAccountId,
                    FuelRate = x.FuelRate,
                    VehicleId = x.VehicleId,
                    HireVehicleId = x.HireVehicleId,
                    DriverId = x.DriverId,
                    ReferenceNo = x.ReferenceNo,
                    DebitAccountId = x.DebitAccountId ?? 0,
                    SettlementId = x.SettlementId,
                    Remark = x.Remark,
                    AdvanceId = x.Id,
                    CreditAccount = x.fk_CreditAccount.AccountName,
                    DebitAccountName = x.fk_DebitAccount.AccountName,
                    DriverName =x.fk_Driver!=null? x.fk_Driver.DriverName:"",
                    FuelId = x.FuelId,
                    FuelTypeName = x.fk_FuelType.Name,

                    ExpenseId = x.ExpenseId,
                    Expense = x.fk_Expense!=null? x.fk_Expense.Name:"",
                    OfficeName = x.fk_Office.OfficeName,
                    SettlementNo = x.fk_Settlement.TripSheetNo,
                    TripLogId = x.TripLogId,
                    TripLogNo = x.fk_Triplog.TriplogNo,
                    VehicleNo = x.fk_Vehicle!=null?x.fk_Vehicle.VehicleNo:"",
                    HireVehicleNo =x.fk_HireVehicle!=null? x.fk_HireVehicle.VehicleNo:"",
                    PaidInId = x.PaidInId,
                    PaidIn = x.fk_PaidIn==null?null:x.fk_PaidIn.ConstantName,
                    Ref1 = x.Ref1,
                    Data=x.Data

                });
            voucher.TripAdvanceLogs = new List<vwTripAdvanceLog>(advances);
            return voucher;
        }
        /// <summary>
        /// Prepares the v.
        /// </summary>
        /// <param name="advance">The advance.</param>
        public static void PrepareV(this IRepository<TripAdvanceLog> _repository, TripAdvanceLog advance)
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
            advance.fk_Voucher.CurTypeId = advance.CurTypeId;
            advance.fk_Voucher.CurRate = advance.CurRate;
            advance.fk_Voucher.ConstCurTypeId = advance.ConstCurTypeId;

            advance.fk_Voucher.OfficeId = advance.OfficeId;
            advance.fk_Voucher.VoucherNo = advance.VoucherNo;
            advance.fk_Voucher.VoucherDate = advance.AdvanceDate;
            advance.fk_Voucher.VoucherDateTime = advance.AdvanceDate;
            advance.fk_Voucher.ObjectState = advance.fk_Voucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            advance.fk_Voucher.VoucherAmount = advance.Amount;
            advance.fk_Voucher.VoucherTypeId = advance.AdvanceTypeId.GetValueOrDefault(0);
            advance.fk_Voucher.Account1Id = advance.DebitAccountId.GetValueOrDefault(0);
            advance.fk_Voucher.Account2Id = advance.CreditAccountId.GetValueOrDefault(0);
            advance.fk_Voucher.Amount1 = advance.Amount * 1;
            advance.fk_Voucher.Amount2 = advance.Amount * -1;
            advance.fk_Voucher.UserRemark = advance.Remark;
            //TODO:Setup Account Narration from Template located with VoucherType
            advance.fk_Voucher.AccountingRemark = "";
        }

        /// <summary>
        /// Prepares the vd.
        /// </summary>
        /// <param name="advance">The advance.</param>
        public static void PrepareVD(this IRepository<TripAdvanceLog> _repository,TripAdvanceLog advance)
        {
            advance.fk_Voucher.VoucherDetails.ForEach(x => x.ObjectState = ObjectState.Deleted);
            var vdDr = new VoucherDetail()
            {
                OfficeId = advance.fk_Voucher.OfficeId,
                AccountId = advance.fk_Voucher.Account1Id.Value,
                OrderId = 1,
                Amount = advance.fk_Voucher.Amount1,
                //Narration = advance.fk_Voucher.UserRemark,
                ObjectState = ObjectState.Added,
                VoucherId = advance.fk_Voucher.Id,

                CurTypeId = advance.CurTypeId,
                CurRate = advance.CurRate,
                ConstCurTypeId = advance.ConstCurTypeId
            };
            var vdCr = new VoucherDetail()
            {
                OfficeId = advance.fk_Voucher.OfficeId,
                AccountId = advance.fk_Voucher.Account2Id.Value,
                OrderId = 2,
                Amount = advance.fk_Voucher.Amount2,
                //Narration = advance.fk_Voucher.UserRemark,
                ObjectState = ObjectState.Added,
                VoucherId = advance.fk_Voucher.Id,

                CurTypeId = advance.CurTypeId,
                CurRate = advance.CurRate,
                ConstCurTypeId = advance.ConstCurTypeId
            };
            advance.fk_Voucher.VoucherDetails.Add(vdCr);
            advance.fk_Voucher.VoucherDetails.Add(vdDr);
        }
        /// <summary>
        /// Prepares the VDR.
        /// </summary>
        /// <param name="vd">The vd.</param>
        /// <param name="advance">The advance.</param>
        public static void PrepareVDR(this IRepository<TripAdvanceLog> _repository, VoucherDetail vd, TripAdvanceLog advance)
        {
            vd.VoucherDetailReferences.ForEach(x => x.ObjectState = ObjectState.Deleted);
            if (vd.ObjectState == ObjectState.Added)
            {

                if (advance.AdvanceTypeId == 76/*HS Payment*/|| advance.AdvanceTypeId == 78/*HS On Account*/)
                {
                    var ishspayment = advance.AdvanceTypeId == 76;

                    var hsvdr = ishspayment ? _repository.GetRepository<VehicleMovementLog>().Queryable().Where(x => x.Id == advance.TripLogId && x.VDRId > 0)
                        .Select(x => new
                        {
                            x.Id,
                            Amount = x.fk_VDR.Amount,
                            x.VDRId,
                            x.fk_VDR.ReferenceNo,
                            TotalPaid = x.TripAdvances.Where(y => y.VoucherId != vd.VoucherId).Sum(y => (decimal?)(y.CashAmount > 0 ? y.CashAmount : y.FuelAmount))
                        }).FirstOrDefault() : null;

                    if (ishspayment && ((hsvdr?.Amount ?? 0) * -1) < (advance.Amount + (hsvdr?.TotalPaid ?? 0)))
                    {
                        throw new BusinessException(ErrorCode.VCH109, "Total paid payment amount exceeded hirecharges amount");
                    }
                    if (vd.Amount > 0)/*On Account/Against Reference Payment VDR*/
                    {
                        var vdr = new VoucherDetailReference()
                        {
                            ObjectState = ObjectState.Added,
                            Amount = vd.Amount,
                            ReferenceNo = ishspayment && hsvdr != null ? hsvdr.ReferenceNo : advance.ReferenceNo,
                            RefId = ishspayment && hsvdr != null ? hsvdr.VDRId : (long?)null,
                            VDRTypeId = ishspayment && hsvdr != null ? 1014/*Against ref*/ : 1448/*On Account VDR*/,
                            VoucherDetailId = vd.Id,
                            AccountId = vd.AccountId,
                            DueDate = advance.AdvanceDate,
                            TransactionId = advance.Id,

                            CurTypeId = advance.CurTypeId,
                            CurRate = advance.CurRate,
                            ConstCurTypeId = advance.ConstCurTypeId
                        };
                        vd.VoucherDetailReferences.Add(vdr);
                        advance.fk_VDR = vdr;
                        advance.VDRId = vdr.Id;
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
                            ReferenceNo = ishspayment && hsvdr != null ? hsvdr.ReferenceNo : advance.ReferenceNo,
                            VDRTypeId = 1013,
                            VoucherDetailId = vd.Id,
                            AccountId = vd.AccountId,
                            DueDate = advance.AdvanceDate,
                            TransactionId = advance.Id,

                            CurTypeId = advance.CurTypeId,
                            CurRate = advance.CurRate,
                            ConstCurTypeId = advance.ConstCurTypeId
                        };
                        vd.VoucherDetailReferences.Add(vdr);
                        advance.fk_VDR = vdr;
                        advance.VDRId = vdr.Id;
                    }

                }
                else
                {
                    var isRefEnabled =
                _repository.GetRepository<Ledger>()
                    .Queryable()
                    .Where(x => x.Id == vd.AccountId)
                    .Select(y => new { y.ReferenceFlag })
                    .FirstOrDefault();
                    if (isRefEnabled == null || !isRefEnabled.ReferenceFlag) return;
                    var vdrtype = advance.AdvanceTypeId == 94 ? 1014/*AgainstRef*/ : 1013/*NewRef*/;
                    var vdr = new VoucherDetailReference()
                    {
                        ObjectState = ObjectState.Added,
                        Amount = vd.Amount,
                        ReferenceNo = advance.ReferenceNo,
                        VDRTypeId = vdrtype,
                        VoucherDetailId = vd.Id,
                        AccountId = vd.AccountId,
                        DueDate = advance.AdvanceDate,
                        TransactionId = advance.Id,

                        CurTypeId = advance.CurTypeId,
                        CurRate = advance.CurRate,
                        ConstCurTypeId = advance.ConstCurTypeId
                    };
                    if (advance.AdvanceTypeId == 94)
                    {
                        var adv = _repository.Queryable().Where(x => x.Id == advance.SettledRefId).Select(x => x.VDRId).FirstOrDefault();
                        vdr.RefId = adv;
                    }
                    vd.VoucherDetailReferences.Add(vdr);
                    advance.fk_VDR = vdr;
                    advance.VDRId = vdr.Id;
                }

            }
        }
        /// <summary>
        /// Prepares the bulk v.
        /// </summary>
        /// <param name="totalAmt">The total amt.</param>
        /// <param name="vch">The VCH.</param>
        /// <param name="vw">The vw.</param>
        public static void PrepareBulkV(this IRepository<TripAdvanceLog> _repository,decimal totalAmt, Voucher vch, vwAdvanceVoucher vw)
        {
            vch.CurTypeId = vw.CurTypeId;
            vch.CurRate = vw.CurRate;
            vch.ConstCurTypeId = vw.ConstCurTypeId;

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
            PrepareBulkVd(_repository,vch, vw);
        }

        /// <summary>
        /// Prepares the bulk vd.
        /// </summary>
        /// <param name="vch">The VCH.</param>
        /// <param name="vw">The vw.</param>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        /// <exception cref="BusinessException">VoucherDetails.Count LT 2</exception>
        public static void PrepareBulkVd(this IRepository<TripAdvanceLog> _repository,Voucher vch, vwAdvanceVoucher vw)
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
                    detail.OfficeId = vch.OfficeId;
                    detail.AccountId = detail.OrderId == 1 ? vch.Account1Id.GetValueOrDefault() : vch.Account2Id.GetValueOrDefault();
                    detail.OrderId = detail.OrderId == 1 ? 1 : 2;
                    detail.Amount = detail.OrderId == 1 ? vch.Amount1 : vch.Amount2;
                    detail.Narration = vch.UserRemark;
                    detail.ObjectState = ObjectState.Modified;
                    detail.VoucherId = vch.Id;

                    detail.CurTypeId = vch.CurTypeId;
                    detail.CurRate = vch.CurRate;
                    detail.ConstCurTypeId = vch.ConstCurTypeId;
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
                        OfficeId = vch.OfficeId,
                        AccountId = i == 1 ? vch.Account1Id.GetValueOrDefault() : vch.Account2Id.GetValueOrDefault(),
                        OrderId = i,
                        Amount = i == 1 ? vch.Amount1 : vch.Amount2,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        CurTypeId = vch.CurTypeId,
                        CurRate = vch.CurRate,
                        ConstCurTypeId = vch.ConstCurTypeId
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
        public static void PrepareBulkVdr(this IRepository<TripAdvanceLog> _repository,VoucherDetail v, List<TripAdvanceLog> a, List<TripAdvanceLog> d)
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


                    CurTypeId = v.CurTypeId,
                    CurRate = v.CurRate,
                    ConstCurTypeId = v.ConstCurTypeId
                };
                v.VoucherDetailReferences.Add(vdr);
                log.fk_VDR = vdr;
                log.VDRId = vdr.Id;
                if (log.ObjectState == ObjectState.Unchanged) log.ObjectState = ObjectState.Modified;
            }
        }
    }
}
