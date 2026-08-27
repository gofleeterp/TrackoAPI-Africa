using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.FMS.Dues;
using System.Web.UI.WebControls;
using System.ServiceModel.Description;

namespace TrackoApi.Service
{
    public interface IDueTransactionLogService : IService<DueTransactionLog>
    {
        IQueryable<DueTransactionLog> GetDueTransactionLogsByDueTypeId(int id);
        IQueryable<vwDueVoucher> GetQueryableBulkEntryByKey(long key);
        Voucher BulkDueEntry(vwDueVoucher doc, Voucher vch);
        vwDueVoucher GetBulkEntryByKey(long key);
        void BulkDelete(Voucher vch);
        void DeletePrepaidTaxEntry(long voucherid);
        Voucher GeneratePrepaidTaxEntry(long voucherid);        
    }
    public class DueTransactionLogService : Service<DueTransactionLog>, IDueTransactionLogService
    {
        private readonly IRepositoryAsync<DueTransactionLog> _repository;
        private int? ConstCurTypeId { get; set; }
        public DueTransactionLogService(IRepositoryAsync<DueTransactionLog> repository) : base(repository)
        {
            _repository = repository;
            ConstCurTypeId = Helper.ConstCurTypeId;
        }

        public IQueryable<DueTransactionLog> GetDueTransactionLogsByDueTypeId(int dueTypeId)
        {
            return _repository.GetDueTransactionLogsByDueTypeId(dueTypeId);
        }
        /// <summary>
        /// Gets the queryable bulk entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>IQueryable&lt;vwDueVoucher&gt;.</returns>
        public IQueryable<vwDueVoucher> GetQueryableBulkEntryByKey(long key)
        {
            var listOppLineData = new Queue<vwDueVoucher>();
            var vch = _repository.GetBulkEntryByVoucherId(key);
            if (vch == null)
            {
                return listOppLineData.AsQueryable();
            }
            listOppLineData.Enqueue(vch);
            return listOppLineData.AsQueryable();
        }

        /// <summary>
        /// Bulks the advance.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="vch">The VCH.</param>
        /// <returns>Voucher.</returns>
        /// <exception cref="BusinessException">
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count" />. </exception>
        public Voucher BulkDueEntry(vwDueVoucher doc, Voucher vch)
        {
            if ((doc.PaidAmount) != -doc.DueLogs.Where(x => !x.IsDeleted).Sum(x => x.DueAmount + x.OtherAmount+x.MiscCharg+x.IGSTPAmount+x.CGSTPAmount+x.SGSTPAmount + x.IGSTTPAmount + x.CGSTTPAmount + x.SGSTTPAmount))
            {
                throw new BusinessException(ErrorCode.VCH104, "Paid Amount Doesn't match with Sum of Other Amount+ Due Amount");
            }

            var paidDate = doc.PaidDate.Date;
            var fyDate =
                _repository.GetRepository<FinancialYear>()
                    .Queryable()
                    .Where(x =>DbFunctions.TruncateTime(x.OpeningDate) <= paidDate && DbFunctions.TruncateTime(x.ClosingDate) >= paidDate)
                    .Select(x => new { x.ClosingDate }).FirstOrDefault()?.ClosingDate;
            vch = vch ?? new Voucher();
            var newAdvs = new List<DueTransactionLog>();
            for (int i = 0; i < doc.DueLogs.Count; i++)
            {
                var dt = doc.DueLogs.ElementAt(i);
                var adv = this.Queryable().FirstOrDefault(x => x.Id == dt.Id) ?? new DueTransactionLog();
                adv.Id = dt.Id;
                adv.RefNo1 = string.IsNullOrWhiteSpace(dt.RefNo1) ? (doc.DueLogs.Count(x => string.IsNullOrWhiteSpace(x.RefNo1)) > 1 ? doc.DocumentNo + "-" + (string.IsNullOrWhiteSpace(dt.VehicleNo) ? i.ToString() : dt.VehicleNo) : doc.DocumentNo) : dt.RefNo1;
                adv.RefNo2 = dt.RefNo2;
                adv.ObjectState = dt.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                adv.OwnerId = dt.OwnerId>0?dt.OwnerId:null;
                adv.DueTypeId = doc.DueTypeId;
                adv.CurRate = doc.CurRate;
                adv.CurTypeId = doc.CurTypeId;   
                adv.ConstCurTypeId = this.ConstCurTypeId;
                adv.Remark = dt.Remark;
                adv.StartDate = dt.StartDate;
                adv.ExpiryDate = dt.ExpiryDate;
                adv.VehicleId = dt.VehicleId;
                adv.DueAmount = dt.DueAmount;
                adv.MiscCharge = dt.MiscCharg;
                adv.OtherAmount = dt.OtherAmount;
                adv.VoucherNo = doc.DocumentNo;
                adv.DueAccountId = doc.DueAccountId;
                adv.PayableAccountId = doc.PayableAccountId;
                adv.OthPayableAccountId = doc.OthPayableAccountId;
                adv.OtherAccountId = doc.OtherAccountId;
                adv.DueTypeId = doc.DueTypeId;
                adv.OfficeId = doc.OfficeId;
                adv.PaidDate = doc.PaidDate;
                adv.VoucherId = doc.Id;
                adv.ViewId = doc.ViewId;
                adv.IGSTPAmount = dt.IGSTPAmount;
                adv.IGSTPAmountP = dt.IGSTPAmountP;
                adv.CGSTPAmount = dt.CGSTPAmount;
                adv.CGSTPAmountP = dt.CGSTPAmountP;
                adv.SGSTPAmount = dt.SGSTPAmount;
                adv.SGSTPAmountP = dt.SGSTPAmountP;

                adv.IGSTTPAmount = dt.IGSTTPAmount;
                adv.IGSTTPAmountP = dt.IGSTTPAmountP;
                adv.CGSTTPAmount = dt.CGSTTPAmount;
                adv.CGSTTPAmountP = dt.CGSTTPAmountP;
                adv.SGSTTPAmount = dt.SGSTTPAmount;
                adv.SGSTTPAmountP = dt.SGSTTPAmountP;                
                if (adv.VoucherId == 0)
                {
                    adv.fk_Voucher = vch;
                }

                if (dt.InsuranceLog != null)
                {
                    var ins = _repository.GetRepository<DueInsuranceLog>().Find(adv.Id);
                    if (ins == null)
                    {
                        adv.fk_InsuranceLog = new DueInsuranceLog();
                    }
                    adv.fk_InsuranceLog.InsOfficerName = dt.InsuranceLog.InsOfficerName;
                    adv.fk_InsuranceLog.InsuredValue = dt.InsuranceLog.InsuredValue;
                    adv.fk_InsuranceLog.NCBAmount = dt.InsuranceLog.NCBAmount;
                    adv.fk_InsuranceLog.NCBPercent = dt.InsuranceLog.NCBPercent;
                    adv.fk_InsuranceLog.PACCount = dt.InsuranceLog.PACCount;
                    adv.fk_InsuranceLog.PACValue = dt.InsuranceLog.PACValue;
                    adv.fk_InsuranceLog.Premium = dt.InsuranceLog.Premium;
                    adv.fk_InsuranceLog.TPPremium = dt.InsuranceLog.TPPremium;
                    adv.fk_InsuranceLog.AgentName = dt.InsuranceLog.AgentName;
                    adv.fk_InsuranceLog.Compulsory = dt.InsuranceLog.Compulsory;
                    adv.fk_InsuranceLog.Discount = dt.InsuranceLog.Discount;
                    adv.fk_InsuranceLog.GVWOD = dt.InsuranceLog.GVWOD;
                    adv.fk_InsuranceLog.ImposedValue = dt.InsuranceLog.ImposedValue;
                    adv.fk_InsuranceLog.InsCompanyId = dt.InsuranceLog.InsCompanyId;
                    adv.fk_InsuranceLog.IsComprehensive = dt.InsuranceLog.IsComprehensive;
                    adv.fk_InsuranceLog.ObjectState = adv.fk_InsuranceLog.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                }
                if (dt.IsDeleted)
                {
                    adv.ObjectState = ObjectState.Deleted;
                }
                if (adv.StartDate.Date > adv.ExpiryDate.Date)
                {
                    throw new BusinessException(ErrorCode.DUET101, $"Ref No :{adv.RefNo1}");
                }
                //if (adv.StartDate.Date < doc.PaidDate.Date)
                //{
                //    throw new BusinessException(ErrorCode.DUET102, $"Ref No :{adv.RefNo1}");
                //}
                if (fyDate.HasValue && adv.ExpiryDate > fyDate)
                {
                    var prepaidtimespan = adv.ExpiryDate.Subtract(fyDate.Value).Days;
                    if (prepaidtimespan > 0)
                    {
                        var completetimespan = adv.ExpiryDate.Subtract(adv.StartDate).Days;
                        var totalprepaidTax = adv.DueAmount + adv.MiscCharge + adv.OtherAmount + adv.IGSTPAmount + adv.CGSTPAmount + adv.SGSTPAmount + adv.IGSTTPAmount + adv.CGSTTPAmount + adv.SGSTTPAmount;
                        adv.PrePaidTax = (totalprepaidTax / completetimespan) * prepaidtimespan;
                        adv.IsPrePaidTaxBooked = false;
                    }
                }
                newAdvs.Add(adv);
            }
            //Delete All the Advance that was mapped to this voucherid before now but not now
            var ids = newAdvs.Select(x => x.Id);
            var deletedRecords = from a in Queryable().Where(x => x.VoucherId == vch.Id)
                                 where !ids.Contains(a.Id)
                                 select a;
            foreach (var x in deletedRecords)
            {
                x.ObjectState = ObjectState.Deleted;
                x.VoucherId = 0;
                x.fk_Voucher = null;
            }
            //Prepare Voucher And Voucher Details
            vch.ConstCurTypeId = this.ConstCurTypeId;
            vch.CurTypeId = doc.CurTypeId;
            vch.CurRate = doc.CurRate;
            vch.IsCCRequired = true;
            PrepareBulkV(doc.PaidAmount, vch, doc);
            vch.ViewId= doc.ViewId;
            foreach (VoucherDetail detail in vch.VoucherDetails)
            {
                PrepareBulkVdr(vch,detail, newAdvs, deletedRecords);
            }
            #region Validations
            if (vch.VoucherDetails.Sum(x => x.Amount) != 0)
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

        /// <exception cref="ArgumentNullException"><paramref name="collection" /> is null.</exception>
        public vwDueVoucher GetBulkEntryByKey(long key)
        {
            return _repository.GetBulkEntryByVoucherId(key);
        }

        /// <summary>
        /// Prepares the bulk v.
        /// </summary>
        /// <param name="totalAmt">The total amt.</param>
        /// <param name="vch">The VCH.</param>
        /// <param name="vw">The vw.</param>
        /// <exception cref="BusinessException">VoucherDetails.Count LT 2</exception>
        /// <exception cref="ArgumentNullException"><paramref name="match" /> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count" />. </exception>
        public void PrepareBulkV(decimal totalAmt, Voucher vch, vwDueVoucher vw)
        {
            vch.OfficeId = vw.OfficeId;
            vch.VoucherNo = vw.DocumentNo;
            vch.VoucherDate = vw.PaidDate;
            vch.VoucherDateTime = vw.PaidDate;
            vch.ChequeDate = vw.ChequeDate;
            vch.ChequeNo = vw.ChequeNo;            
            vch.ObjectState = vch.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            vch.VoucherAmount = totalAmt;
            vch.VoucherTypeId = 20;
            vch.Account1Id = vw.DueAccountId;
            vch.Account2Id = vw.PayableAccountId;
            vch.Account3Id = vw.OtherAccountId;
            vch.Account4Id = vw.OthPayableAccountId;
            vch.Account5Id = vw.IGSTAccountId;/*IGST*/
            vch.Account6Id = vw.CGSTAccountId;/*CGST*/
            vch.Account7Id = vw.SGSTAccountId;/*SGST*/

            vch.Amount1 = vw.DueAmount;
            vch.Amount2 = vw.PayableAmount;
            vch.Amount3 = vw.OtherAmount;
            vch.Amount4 = vw.OthPayableAmount;
            vch.Amount5 = vw.IGSTAmount;/*IGST*/
            vch.Amount6 = vw.CGSTAmount;/*CGST*/
            vch.Amount7 = vw.SGSTAmount;/*SGST*/
            vch.UserRemark = vw.Narration;
            vch.PageId = vw.PageId;
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
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index" /> is less than 0.-or-<paramref name="index" /> is equal to or greater than <see cref="P:System.Collections.Generic.List`1.Count" />. </exception>
        public void PrepareBulkVd(Voucher vch, vwDueVoucher vw)
        {
            var ledgerRepo = _repository.GetRepository<Ledger>().Queryable();
            if (vch.Id > 0 && vch.VoucherDetails != null && vch.VoucherDetails.Any())
            {
                if (vch.VoucherDetails.Count < 2)
                {
                    throw new BusinessException(ErrorCode.VCH105);
                }
                for (var i = 1; i <= 7; i++)
                {
                    //check if 3rd and 4th VDs are required
                    if (i == 3 && (!vch.Account3Id.HasValue || vch.Amount3 == 0)) continue;
                    if (i == 4 && (!vch.Account4Id.HasValue || vch.Amount4 == 0)) continue;
                    if (i == 5 && (!vch.Account5Id.HasValue || vch.Amount5 == 0)) continue;
                    if (i == 6 && (!vch.Account6Id.HasValue || vch.Amount6 == 0)) continue;
                    if (i == 7 && (!vch.Account7Id.HasValue || vch.Amount7 == 0)) continue;
                    var vd = vch.VoucherDetails.FirstOrDefault(x => x.OrderId == i);
                    if (vd == null) continue;
                    vd.Narration = vch.UserRemark;
                    vd.ObjectState = ObjectState.Modified;
                    vd.VoucherId = vch.Id;
                    vd.ChequeNo = vch.ChequeNo;
                    vd.ChequeDate = vch.ChequeDate;
                    vd.ConstCurTypeId = vch.ConstCurTypeId;
                    vd.CurTypeId = vch.CurTypeId;
                    vd.CurRate = vch.CurRate;
                    SetAccountAmounts(vch, vd, i, ledgerRepo);
                }
            }
            else
            {
                if (vch.VoucherDetails == null)
                {
                    vch.VoucherDetails = new List<VoucherDetail>();
                }
                for (var i = 1; i <= 7; i++)
                {
                    //check if 3rd and 4th VDs are required
                    if (i == 3 && (!vch.Account3Id.HasValue || vch.Amount3 == 0)) continue;
                    if (i == 4 && (!vch.Account4Id.HasValue || vch.Amount4 == 0)) continue;
                    if (i == 5 && (!vch.Account5Id.HasValue || vch.Amount5 == 0)) continue;
                    if (i == 6 && (!vch.Account6Id.HasValue || vch.Amount6 == 0)) continue;
                    if (i == 7 && (!vch.Account7Id.HasValue || vch.Amount7 == 0)) continue;
                    var vd = new VoucherDetail()
                    {
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id
                    };
                    SetAccountAmounts(vch, vd, i, ledgerRepo);
                    vch.VoucherDetails.Add(vd);
                }
            }

        }

        /// <summary>
        /// Sets the account amounts.
        /// </summary>
        /// <param name="v">The voucher.</param>
        /// <param name="vd">The voucher detail.</param>
        /// <param name="index">The index.</param>
        private static void SetAccountAmounts(Voucher v, VoucherDetail vd, int index, IQueryable<Ledger> repo)
        {

            switch (index)
            {
                case 1:
                    vd.AccountId = v.Account1Id.Value;
                    vd.Amount = v.Amount1;
                    vd.OrderId = 1;
                    var account1 = repo.Where(x => x.Id == v.Account1Id)
                            .Select(x => x.OfficeId)
                            .FirstOrDefault()
                            .GetValueOrDefault(0);
                    vd.OfficeId = account1 == 0 ? v.OfficeId : account1;

                    break;
                case 2:
                    vd.AccountId = v.Account2Id.Value;
                    vd.Amount = v.Amount2;
                    vd.OrderId = 2;
                    var account2 = repo.Where(x => x.Id == v.Account2Id)
                            .Select(x => x.OfficeId)
                            .FirstOrDefault()
                            .GetValueOrDefault(0);
                    vd.OfficeId = account2 == 0 ? v.OfficeId : account2;
                    break;
                case 3:
                    vd.AccountId = v.Account3Id.GetValueOrDefault(0);
                    vd.Amount = v.Amount3;
                    vd.OrderId = 3;
                    var account3 = repo.Where(x => x.Id == v.Account1Id)
                            .Select(x => x.OfficeId)
                            .FirstOrDefault()
                            .GetValueOrDefault(0);
                    vd.OfficeId = account3 == 0 ? v.OfficeId : account3;
                    break;
                case 4:
                    vd.AccountId = v.Account4Id.GetValueOrDefault(0);
                    vd.Amount = v.Amount4;
                    vd.OrderId = 4;
                    var account4 = repo.Where(x => x.Id == v.Account1Id)
                            .Select(x => x.OfficeId)
                            .FirstOrDefault()
                            .GetValueOrDefault(0);
                    vd.OfficeId = account4 == 0 ? v.OfficeId : account4;
                    break;
                case 5:
                    vd.AccountId = v.Account5Id.GetValueOrDefault(0);
                    vd.Amount = v.Amount5;
                    vd.OrderId = 5;
                    var account5 = repo.Where(x => x.Id == v.Account1Id)
                            .Select(x => x.OfficeId)
                            .FirstOrDefault()
                            .GetValueOrDefault(0);
                    vd.OfficeId = account5 == 0 ? v.OfficeId : account5;
                    break;
                case 6:
                    vd.AccountId = v.Account6Id.GetValueOrDefault(0);
                    vd.Amount = v.Amount6;
                    vd.OrderId = 6;
                    var account6 = repo.Where(x => x.Id == v.Account1Id)
                            .Select(x => x.OfficeId)
                            .FirstOrDefault()
                            .GetValueOrDefault(0);
                    vd.OfficeId = account6 == 0 ? v.OfficeId : account6;
                    break;
                case 7:
                    vd.AccountId = v.Account7Id.GetValueOrDefault(0);
                    vd.Amount = v.Amount7;
                    vd.OrderId = 7;
                    var account7 = repo.Where(x => x.Id == v.Account1Id)
                            .Select(x => x.OfficeId)
                            .FirstOrDefault()
                            .GetValueOrDefault(0);
                    vd.OfficeId = account7 == 0 ? v.OfficeId : account7;
                    break;
            }
        }

        /// <summary>
        /// Prepares the bulk Voucher Detail Reference.
        /// </summary>
        /// <param name="v">The Voucher Detail</param>
        /// <param name="a">The Active Advances</param>
        /// <param name="d">Deleted Advances</param>
        /// <exception cref="BusinessException">Used reference cannot be deleted.</exception>
        public void PrepareBulkVdr(Voucher vch, VoucherDetail v, List<DueTransactionLog> a, IQueryable<DueTransactionLog> d)
        {
            try
            {
                var vdrRepo = _repository.GetRepository<VoucherDetailReference>().Queryable();
                //Mark VDR's as Deleted only those are Unsettled
                foreach (VoucherDetailReference reference in v.VoucherDetailReferences)
                {
                    reference.ObjectState = !vdrRepo.Where(x => x.RefId == reference.Id).Select(x => x.Id).Any() ? ObjectState.Deleted : ObjectState.Unchanged;
                }
                if (v.VoucherDetailReferences.Any(x => x.ObjectState == ObjectState.Unchanged))
                {
                    throw new BusinessException(ErrorCode.VCH103, $"{v.VoucherDetailReferences.Where(x => x.ObjectState == ObjectState.Unchanged).Select(x => x.ReferenceNo).JoinStrings(".")} are/is referenced in Accounts.");
                }
                var lRepo = _repository.GetRepository<Ledger>().Queryable();
                var isRefEnabled = lRepo.Any(x => x.Id == v.AccountId && x.ReferenceFlag);
                if (!isRefEnabled) return;
                if (v.OrderId==1 || v.OrderId == 2)
                {
                    foreach (var x in a.Where(x => x.ObjectState != ObjectState.Deleted))
                    {
                        decimal dueAmt = 0;
                        if (v.OrderId == 1)
                        { dueAmt = x.DueAmount + x.MiscCharge; }
                        if (v.OrderId == 2) { dueAmt = x.DueAmount + x.MiscCharge+x.OtherAmount+x.IGSTPAmount+x.CGSTPAmount+x.SGSTPAmount + x.IGSTTPAmount + x.CGSTTPAmount + x.SGSTTPAmount; }
                        var vdr = new VoucherDetailReference()
                        {
                            ObjectState = ObjectState.Added,
                            Amount = v.Amount > 0 ? dueAmt : -dueAmt,
                            ReferenceNo = x.RefNo1,
                            VDRTypeId = 1013,
                            VoucherDetailId = v.Id,
                            ConstCurTypeId = v.ConstCurTypeId,
                            CurTypeId = v.CurTypeId,
                            CurRate = v.CurRate
                        };
                        PrepareVdrAmount(v, vdr, x);
                        v.VoucherDetailReferences.Add(vdr);
                    }
                }
                else
                {
                    var vdr = new VoucherDetailReference()
                    {
                        ObjectState = ObjectState.Added,
                        Amount = v.Amount,
                        ReferenceNo = vch.VoucherNo,
                        VDRTypeId = 1013,
                        VoucherDetailId = v.Id,
                        ConstCurTypeId = v.ConstCurTypeId,
                        CurTypeId = v.CurTypeId,
                        CurRate = v.CurRate
                    };
                    v.VoucherDetailReferences.Add(vdr);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        static void PrepareVdrAmount(VoucherDetail vd, VoucherDetailReference vdr, DueTransactionLog a)
        {
            decimal dueamt = 0; 
            switch (vd.OrderId)
            {
                case 1:
                    dueamt= a.DueAmount + a.MiscCharge;
                    vdr.Amount = vd.Amount > 0 ? dueamt : -dueamt;
                    break;
                case 2:
                    dueamt = a.DueAmount + a.MiscCharge + a.OtherAmount + a.IGSTPAmount + a.CGSTPAmount + a.SGSTPAmount + a.IGSTTPAmount + a.CGSTTPAmount + a.SGSTTPAmount;
                    vdr.Amount = vd.Amount > 0 ? dueamt : -dueamt;
                    break;
                case 3:
                    vdr.Amount = vd.Amount > 0 ? a.OtherAmount : -a.OtherAmount;
                    break;
            }
        }

        /// <summary>
        /// Bulks the delete.
        /// </summary>
        /// <param name="vch">The VCH.</param>
        public void BulkDelete(Voucher vch)
        {
            var qr = Queryable().Where(x => x.VoucherId == vch.Id).ToList();
            qr.ForEach(x =>
            {
                x.ObjectState = ObjectState.Deleted;
                x.fk_Voucher = vch;
            });
            vch.ObjectState = ObjectState.Deleted;
            foreach (var x in vch.VoucherDetails)
            {
                x.ObjectState = ObjectState.Deleted;
                x.VoucherDetailReferences.ForEach(y => y.ObjectState = ObjectState.Deleted);
            }
            DeletePrepaidTaxEntry(vch.Id);
        }

        /// <summary>
        /// Generates the prepaid tax entry.
        /// </summary>
        /// <param name="voucherid">The voucherid.</param>
        /// <exception cref="BusinessException">Invalid VoucherId.</exception>
        public Voucher GeneratePrepaidTaxEntry(long voucherid)
        {
            try
            {
                var vRepo = _repository.GetRepository<Voucher>();
                if (vRepo.Queryable().Any(x => x.ReferenceTransactionId == voucherid && x.VoucherTypeId == 21))
                {
                    var voucher =
                        vRepo.Queryable().Where(x => x.Id == voucherid).Select(x => new { x.VoucherNo, x.VoucherDate }).FirstOrDefault();
                    var info = string.Empty;
                    if (voucher != null)
                    {
                        info = $"For Voucher Number :{voucher.VoucherNo} dated:{voucher.VoucherDate.ToShortDateString()}";
                    }
                    throw new BusinessException(ErrorCode.DUET103, info);
                }
                var dues = Queryable().Where(x => x.VoucherId == voucherid && x.PrePaidTax > 0);
                var pac = dues.Select(x => new
                {
                    x.fk_DueType.PrepaidAccountId,
                    x.fk_DueType.fk_PrepaidAccount.OfficeId,
                    x.DueAccountId,
                    x.PaidDate,
                    DueTypeName = x.fk_DueType.Name,
                    x.CurTypeId,
                    x.CurRate,
                    x.ConstCurTypeId
                }).FirstOrDefault();
                if (pac == null)
                {
                    throw new BusinessException(ErrorCode.DUET100, $"No due transaction are prepaid");
                }
                if (pac.PrepaidAccountId == null)
                {
                    throw new BusinessException(ErrorCode.DUET100, $"No prepaid account assigned on Due Type Master");
                }
                var vdRepo =
                    _repository.GetRepository<VoucherDetail>()
                        .Queryable()
                        .Include(x => x.VoucherDetailReferences.Select(y => y.AgainstReferences)).Where(x => x.VoucherId == voucherid && x.OrderId == 1);
                if (!vdRepo.AsNoTracking().Any()) throw new BusinessException(ErrorCode.DUET100, $"VoucherId:{voucherid}");
                var fy =
                    _repository.GetRepository<FinancialYear>()
                        .Queryable()
                        .Where(x => x.OpeningDate <= pac.PaidDate && x.ClosingDate >= pac.PaidDate)
                        .Select(x => new { x.ClosingDate }).FirstOrDefault();
                if (fy == null)
                {
                    throw new BusinessException(ErrorCode.DUET100, $"Financial Year has not been generated for parent tax entry. Date :{pac.PaidDate.ToShortDateString()}");
                }
                var voucherNo = vdRepo.AsNoTracking().Select(x => x.Voucher.VoucherNo).FirstOrDefault();
                var totalprepaidTax = dues.Sum(x => x.PrePaidTax);
                var v = new Voucher()
                {
                    CurTypeId=pac.CurTypeId,
                    CurRate=pac.CurRate,
                    ConstCurTypeId=pac.ConstCurTypeId,

                    OfficeId = vdRepo.AsNoTracking().Select(x => x.OfficeId).FirstOrDefault(),
                    VoucherNo = $"Pre-{voucherNo}",
                    VoucherDate = fy.ClosingDate,
                    VoucherDateTime = fy.ClosingDate,
                    ObjectState = ObjectState.Added,
                    VoucherAmount = -totalprepaidTax,
                    VoucherTypeId = 21,
                    Account1Id = pac.PrepaidAccountId.GetValueOrDefault(0),
                    Account2Id = pac.DueAccountId,
                    Amount1 = totalprepaidTax,
                    Amount2 = -totalprepaidTax,
                    UserRemark = $"Auto Generated Voucher against Prepaid Expense of type ({pac.DueTypeName}), paid dt:{pac.PaidDate.ToShortDateString()}, Voucher No : {voucherNo}",
                    //TODO:Setup Account Narration from Template located with VoucherType
                    AccountingRemark = "",
                    ReferenceTransactionId = voucherid,

                };
                var vdCr = new VoucherDetail()
                {
                    Voucher = v,
                    VoucherDetailReferences = new List<VoucherDetailReference>(),
                    AccountId = v.Account2Id.Value,
                    Amount = v.Amount2,
                    OfficeId=v.OfficeId,
                    ObjectState = ObjectState.Added,
                    CurTypeId = v.CurTypeId,
                    CurRate = v.CurRate,
                    ConstCurTypeId = v.ConstCurTypeId,
                };
                var vdDr = new VoucherDetail()
                {
                    Voucher = v,
                    VoucherDetailReferences = new List<VoucherDetailReference>(),
                    AccountId = v.Account1Id.Value,
                    Amount = v.Amount1,
                    OfficeId = v.OfficeId,
                    ObjectState = ObjectState.Added,
                    CurTypeId = v.CurTypeId,
                    CurRate = v.CurRate,
                    ConstCurTypeId = v.ConstCurTypeId,
                };
                v.VoucherDetails = new List<VoucherDetail>() { vdDr, vdCr };
                var vdrRepo = _repository.GetRepository<VoucherDetailReference>().Queryable();
                var parentVdId = vdRepo.Select(x => x.Id).FirstOrDefault();
                foreach (var result in dues.AsNoTracking().Select(x => new
                {
                    x.RefNo1,
                    x.PrePaidTax,
                    x.IsPrePaidTaxBooked
                }).Where(x => x.PrePaidTax > 0 && !x.IsPrePaidTaxBooked))
                {
                    #region  Against Reference Entry
                    var parentRef = vdrRepo.Where(x => x.ReferenceNo == result.RefNo1 && x.VoucherDetailId == parentVdId).Select(x => new
                    {
                        x.Id,
                        x.Amount,
                        x.CurTypeId,
                        x.CurRate,
                        x.ConstCurTypeId
                    }).FirstOrDefault();
                    if (parentRef == null)
                    {
                        throw new BusinessException(ErrorCode.VCH107, $"Parent VRD Entry not found for Reference No:{result.RefNo1}");
                    }
                    var vdr = new VoucherDetailReference()
                    {

                        ObjectState = ObjectState.Added,
                        Amount = v.Amount2 > 0 ? result.PrePaidTax : -result.PrePaidTax,
                        VDRTypeId = 1014,
                        RefId = parentRef.Id,
                        fk_VoucherDetail = vdCr,
                        CurTypeId=parentRef.CurTypeId,
                        CurRate=parentRef.CurRate,
                        ConstCurTypeId=parentRef.CurTypeId
                    };
                    var childsum = vdrRepo.Where(x => x.RefId == parentRef.Id).Sum(x =>(decimal?) x.Amount);
                    if (!childsum.HasValue) childsum = 0;
                    if ((parentRef.Amount > 0 && (parentRef.Amount + vdr.Amount + childsum) < 0) || (parentRef.Amount < 0 && (parentRef.Amount + vdr.Amount + childsum) > 0))
                    {
                        throw new BusinessException(ErrorCode.VCH109, $"Ref No:{vdr.ReferenceNo} or ParentRefId:{vdr.RefId.GetValueOrDefault(0)}, Ref balance Amt:{parentRef.Amount + childsum}");
                    }
                    vdCr.VoucherDetailReferences.Add(vdr);
                    #endregion
                    #region New Reference Entry
                    var vdrNew = new VoucherDetailReference()
                    {
                        ObjectState = ObjectState.Added,
                        Amount = v.Amount1 > 0 ? result.PrePaidTax : -result.PrePaidTax,
                        VDRTypeId = 1013,
                        ReferenceNo = result.RefNo1,
                        fk_VoucherDetail = vdDr,
                        CurTypeId = v.CurTypeId,
                        CurRate = v.CurRate,
                        ConstCurTypeId = v.CurTypeId
                    };
                    vdDr.VoucherDetailReferences.Add(vdrNew);
                    #endregion
                }
                foreach (var due in dues)
                {
                    due.IsPrePaidTaxBooked = true;
                }
                vRepo.Insert(v);
                return v;
            }
            catch (Exception ex)
            {
                if (ex is BusinessException)
                {
                    throw ex;
                }
              throw new BusinessException(ErrorCode.DUET100,ex.Message, ex);
            }
            
        }

        public void DeletePrepaidTaxEntry(long voucherid)
        {
            var voucherRepo = _repository.GetRepository<Voucher>();
            if (voucherRepo.Queryable().Count(x=>x.ReferenceTransactionId==voucherid && x.VoucherTypeId == 21) ==0)return;
            var voucher = voucherRepo.Query(x => x.ReferenceTransactionId == voucherid && x.VoucherTypeId == 21).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
            if (voucher == null) return;
            voucher.ObjectState = ObjectState.Deleted;
            foreach (var detail in voucher.VoucherDetails)
            {
                detail.ObjectState = ObjectState.Deleted;
                foreach (var reference in detail.VoucherDetailReferences)
                {
                    reference.ObjectState = ObjectState.Deleted;
                }
            }
        }
    }
}
