// ***********************************************************************
// Assembly         : TrackoApi.Service
// Author           : Admin
// Created          : 02-07-2016
//
// Last Modified By : Admin
// Last Modified On : 03-30-2016
// ***********************************************************************
// <copyright file="GeneralExpenseLogService.cs" company="">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using System;
using Service.Pattern;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.FMS;
using Newtonsoft.Json;
using TrackoAPI.vw.ts;

namespace TrackoApi.Service
{
    /// <summary>
    /// Interface IGeneralExpenseLogService
    /// </summary>
    //GeneralExpenseLogS.GeneralExpenseLog}" />
    public interface IGeneralExpenseLogService : IService<GeneralExpenseLog>
    {
        
        /// <summary>
        /// Prepares the VDR.
        /// </summary>
        /// <param name="vd">The vd.</param>
        /// <param name="GeneralExpense">The GeneralExpense.</param>
        void PrepareVDR(VoucherDetail vd, GeneralExpenseLog GeneralExpense);
        /// <summary>
        /// Prepares the vd.
        /// </summary>
        /// <param name="GeneralExpense">The GeneralExpense.</param>
        void PrepareVD(GeneralExpenseLog GeneralExpense);
        /// <summary>
        /// Prepares the v.
        /// </summary>
        /// <param name="GeneralExpense">The GeneralExpense.</param>
        void PrepareV(GeneralExpenseLog GeneralExpense);
        /// <summary>
        /// Bulks the GeneralExpense.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="vch">The VCH.</param>
        /// <returns>Voucher.</returns>
        Voucher BulkGeneralExpense(vwGeneralExpenseVoucher doc, Voucher vch);
        /// <summary>
        /// Gets the queryable bulk entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>IQueryable&lt;vwGeneralExpenseVoucher&gt;.</returns>
        IQueryable<vwGeneralExpenseVoucher> GetQueryableBulkEntryByKey(long key);
        /// <summary>
        /// Gets the bulk entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>vwGeneralExpenseVoucher.</returns>
        vwGeneralExpenseVoucher GetBulkEntryByKey(long key);
        /// <summary>
        /// Bulks the delete.
        /// </summary>
        /// <param name="vch">The VCH.</param>
        void BulkDelete(Voucher vch);
        

        /// <summary>
        /// Batches the insert.
        /// </summary>
        /// <param name="docs">The docs.</param>
        /// <param name="transaction">The database transaction.</param>
        void BatchInsert(List<vwGeneralExpenseVoucher> docs, IDbTransaction transaction);
    }
    /// <summary>
    /// Class GeneralExpenseLogService.
    /// </summary>
    //GeneralExpenseLogS.GeneralExpenseLog}" />
    /// <seealso cref="TrackoApi.Service.IGeneralExpenseLogService" />
    public class GeneralExpenseLogService : Service<GeneralExpenseLog>, IGeneralExpenseLogService
    {
        /// <summary>
        /// The _repository
        /// </summary>
        private readonly IRepositoryAsync<GeneralExpenseLog> _repository;
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralExpenseLogService"/> class.
        /// </summary>
        /// <param name="repository">The repository.</param>
        public GeneralExpenseLogService(IRepositoryAsync<GeneralExpenseLog> repository) : base(repository)
        {
            _repository = repository;
        }
        /// <summary>
        /// Prepares the v.
        /// </summary>
        /// <param name="expense">The GeneralExpense.</param>
        public void PrepareV(GeneralExpenseLog expense)
        {
            expense.fK_Voucher.ConstCurTypeId = expense.ConstCurTypeId;
            expense.fK_Voucher.CurTypeId = expense.CurTypeId;
            expense.fK_Voucher.CurRate = expense.CurRate;

            expense.fK_Voucher.OfficeId = expense.OfficeId;
            expense.fK_Voucher.VoucherNo = expense.VoucherNo;
            expense.fK_Voucher.VoucherDate = expense.ExpenseDate;
            expense.fK_Voucher.VoucherDateTime = expense.ExpenseDate;
            expense.fK_Voucher.ObjectState = expense.fK_Voucher.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            expense.fK_Voucher.VoucherAmount = expense.Amount;
            expense.fK_Voucher.Account1Id = expense.DebitAccountId;
            expense.fK_Voucher.Account2Id = expense.CreditAccountId;
            expense.fK_Voucher.Amount1 = expense.Amount * 1;
            expense.fK_Voucher.Amount2 = expense.Amount * -1;
            expense.fK_Voucher.UserRemark = expense.Remark;
            expense.fK_Voucher.VoucherTypeId = expense.VoucherTypeId.GetValueOrDefault();
            //TODO:Setup Account Narration from Template located with VoucherType
            expense.fK_Voucher.AccountingRemark = "";
        }

        /// <summary>
        /// Prepares the vd.
        /// </summary>
        /// <param name="generalExpense">The GeneralExpense.</param>
        public void PrepareVD(GeneralExpenseLog generalExpense)
        {
            generalExpense.fK_Voucher.VoucherDetails.ForEach(x => x.ObjectState = ObjectState.Deleted);
            var vdDr = new VoucherDetail()
            {
                OfficeId = generalExpense.fK_Voucher.OfficeId,
                AccountId = generalExpense.fK_Voucher.Account1Id.GetValueOrDefault(),
                OrderId = 1,
                Amount = generalExpense.fK_Voucher.Amount1,
                Narration = generalExpense.fK_Voucher.UserRemark,
                ObjectState = ObjectState.Added,
                VoucherId = generalExpense.fK_Voucher.Id,
                ConstCurTypeId = generalExpense.fK_Voucher.ConstCurTypeId,
                CurTypeId = generalExpense.fK_Voucher.CurTypeId,
                CurRate = generalExpense.fK_Voucher.CurRate
            };
            var vdCr = new VoucherDetail()
            {
                OfficeId = generalExpense.fK_Voucher.OfficeId,
                AccountId = generalExpense.fK_Voucher.Account2Id.GetValueOrDefault(),
                OrderId = 2,
                Amount = generalExpense.fK_Voucher.Amount2,
                Narration = generalExpense.fK_Voucher.UserRemark,
                ObjectState = ObjectState.Added,
                VoucherId = generalExpense.fK_Voucher.Id,
                ConstCurTypeId = generalExpense.fK_Voucher.ConstCurTypeId,
                CurTypeId = generalExpense.fK_Voucher.CurTypeId,
                CurRate = generalExpense.fK_Voucher.CurRate
            };
            generalExpense.fK_Voucher.VoucherDetails.Add(vdCr);
            generalExpense.fK_Voucher.VoucherDetails.Add(vdDr);
        }
        /// <summary>
        /// Prepares the VDR.
        /// </summary>
        /// <param name="vd">The vd.</param>
        /// <param name="generalExpense">The GeneralExpense.</param>
        public void PrepareVDR(VoucherDetail vd, GeneralExpenseLog generalExpense)
        {
            vd.VoucherDetailReferences.ForEach(x => x.ObjectState = ObjectState.Deleted);
            if (vd.ObjectState == ObjectState.Added)
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
                    ReferenceNo = generalExpense.ReferenceNo,
                    VDRTypeId = 1013,
                    VoucherDetailId = vd.Id,
                    ConstCurTypeId = vd.ConstCurTypeId,
                    CurTypeId = vd.CurTypeId,
                    CurRate = vd.CurRate
                };
                vd.VoucherDetailReferences.Add(vdr);
            }
        }

        /// <summary>
        /// Prepares the naration.
        /// </summary>
        /// <param name="GeneralExpense">The GeneralExpense.</param>
        public void PrepareNaration(GeneralExpenseLog GeneralExpense)
        {

        }
        /// <summary>
        /// Gets the queryable bulk entry by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>IQueryable&lt;vwGeneralExpenseVoucher&gt;.</returns>
        public IQueryable<vwGeneralExpenseVoucher> GetQueryableBulkEntryByKey(long key)
        {
            var listOppLineData = new Queue<vwGeneralExpenseVoucher>();
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
        /// <returns>vwGeneralExpenseVoucher.</returns>
        public vwGeneralExpenseVoucher GetBulkEntryByKey(long key)
        {
            return _repository.GetBulkEntryByVoucherId(key);
        }
        /// <summary>
        /// Bulks the GeneralExpense.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="vch">The VCH.</param>
        /// <returns>Voucher.</returns>
        /// <exception cref="BusinessException">
        /// </exception>
        public Voucher BulkGeneralExpense(vwGeneralExpenseVoucher doc, Voucher vch)
        {
            vch = vch ?? new Voucher();
            vch.PageId = doc.PageId;
            var newAdvs = new List<GeneralExpenseLog>();
            //if(doc.GeneralExpenseLogs.Where(x=>x.TripLogId))
            for (int i = 0; i < doc.GeneralExpenseLogs.Count; i++)
            {
                var ad = doc.GeneralExpenseLogs.ElementAt(i);
                var adv = this.Find(ad.ExpenseId) ?? new GeneralExpenseLog();
                adv.Id = ad.ExpenseId;
                adv.ConstCurTypeId = doc.ConstCurTypeId;
                adv.CurTypeId = doc.CurTypeId;
                adv.CurRate = doc.CurRate;
                adv.ReferenceNo = string.IsNullOrWhiteSpace(ad.ReferenceNo) ? (doc.GeneralExpenseLogs.Count(x => string.IsNullOrWhiteSpace(x.ReferenceNo)) > 1 ? doc.DocumentNo + "-" + i : doc.DocumentNo) : ad.ReferenceNo;
                adv.ExpenseDate = ad.ExpenseDate;
                adv.fK_Voucher = vch;
                adv.VoucherNo = doc.DocumentNo;
                adv.ObjectState = ad.ExpenseId > 0 ?  ObjectState.Modified : ObjectState.Added;
                adv.Amount = ad.Amount;
                adv.VoucherId = vch.Id;
                adv.OfficeId = doc.OfficeId;
                adv.CreditAccountId = doc.CrAccountId;
                adv.DebitAccountId = doc.DrAccountId;
                adv.DriverId = ad.DriverId;
                adv.ExpenseNatureId = ad.ExpenseNatureId;
                adv.Remark = ad.Remark;
                adv.VehicleId = ad.VehicleId;
                adv.ViewId = ad.ViewId;
                adv.PaidInId = (ad.PaidInId <= 0 || ad.PaidInId == null) ? 1430 : ad.PaidInId; //1430=Cash
                adv.IsBulkEntry = true;
                adv.Ref1 = ad.Ref1;
                newAdvs.Add(adv);
            }
            //Delete All the GeneralExpense that was mapped to this voucherid before now but not now
            var ids = newAdvs.Select(x => x.Id);
            var deletedRecords = (from a in Queryable().Where(x => x.VoucherId == vch.Id)
                                  where !ids.Contains(a.Id)
                                  select a).ToList();
            foreach (var x in deletedRecords)
            {
                
                x.ObjectState = ObjectState.Deleted;
                x.VoucherId = 0;
                x.fK_Voucher = null;
                Delete(x);
            }

            //Prepare Voucher And Voucher Details
            PrepareBulkV(newAdvs.Sum(x => x.Amount), vch, doc);
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
            //    throw new BusinessException(ErrorCode.TADV102);//Atlead one VDR is Required in GeneralExpense Transaction
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
        public void PrepareBulkV(decimal totalAmt, Voucher vch, vwGeneralExpenseVoucher vw)
        {
            vch.ConstCurTypeId = vw.ConstCurTypeId;
            vch.CurTypeId = vw.CurTypeId;
            vch.CurRate = vw.CurRate;
            vch.OfficeId = vw.OfficeId;
            vch.VoucherNo = vw.DocumentNo;
            vch.VoucherDate = vw.DocumentDate;
            vch.VoucherDateTime = vw.DocumentDate;
            vch.ObjectState = vch.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            vch.VoucherAmount = totalAmt;
            vch.VoucherTypeId = 60;
            vch.Account1Id = vw.DrAccountId;
            vch.Account2Id = vw.CrAccountId;
            vch.Amount1 = totalAmt * 1;
            vch.Amount2 = totalAmt * -1;
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
        public void PrepareBulkVd(Voucher vch, vwGeneralExpenseVoucher vw)
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
                    detail.AccountId = detail.OrderId == 1 ? vch.Account1Id.Value : vch.Account2Id.Value;
                    detail.OrderId = detail.OrderId == 1 ? 1 : 2;
                    detail.Amount = detail.OrderId == 1 ? vch.Amount1 : vch.Amount2;
                    detail.Narration = vch.UserRemark;
                    detail.ObjectState = ObjectState.Modified;
                    detail.VoucherId = vch.Id;
                    detail.ConstCurTypeId = vch.ConstCurTypeId;
                    detail.CurTypeId = vch.CurTypeId;
                    detail.CurRate = vch.CurRate;
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
                        AccountId = i == 1 ? vch.Account1Id.Value : vch.Account2Id.Value,
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
        /// <param name="a">The Active GeneralExpenses</param>
        /// <param name="d">Deleted GeneralExpenses</param>
        public void PrepareBulkVdr(VoucherDetail v, List<GeneralExpenseLog> a, List<GeneralExpenseLog> d)
        {
            
            //Mark VDR's as Deleted only those are Unsettled
            foreach (VoucherDetailReference reference in v.VoucherDetailReferences)
            {
                reference.ObjectState = ObjectState.Deleted;
            }
            var lRepo = _repository.GetRepository<Ledger>().Queryable();
            var isRefEnabled = lRepo.Any(x => x.Id == v.AccountId && x.ReferenceFlag);
            if (!isRefEnabled) return;
            foreach (GeneralExpenseLog log in a)
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
                    CurRate = v.CurRate
                };
                v.VoucherDetailReferences.Add(vdr);
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
            
            qr.ForEach(x =>
            {
                x.ObjectState = ObjectState.Deleted;
                x.fK_Voucher = vch;
            });
            vch.ObjectState = ObjectState.Deleted;
            vch.VoucherDetails.ForEach(x => { x.ObjectState = ObjectState.Deleted; x.VoucherDetailReferences.ForEach(y => y.ObjectState = ObjectState.Deleted); });

        }

        /// <summary>
        /// Fuels the expanses.
        /// </summary>
        /// <param name="settlementId">The settlement identifier.</param>
        /// <param name="tripLogIds">The trip log ids.</param>
        /// <returns>IQueryable&lt;GeneralExpenseLog&gt;.</returns>
        
        #region Batch Methods

        public void BatchInsert(List<vwGeneralExpenseVoucher> docs,IDbTransaction transaction)
        {
            if(docs.Any(x=> x.GeneralExpenseLogs==null||x.GeneralExpenseLogs.Count<=0)) throw new BusinessException(ErrorCode.GLB106,"One of Voucher does not have GeneralExpense Details");
            var vs=new List<Voucher>();
            var vds=new List<VoucherDetail>();
            var vdrs=new List<VoucherDetailReference>();
            var GeneralExpenses=new List<GeneralExpenseLog>();
            var acids = docs.Select(x => x.CrAccountId).Union(docs.Select(x => x.DrAccountId)).Union(docs.Select(x => x.IGSTAccountId??0)).Union(docs.Select(x => x.CGSTAccountId ?? 0)).Union(docs.Select(x => x.SGSTAccountId ?? 0)).Distinct().ToList();
            var acrefs= _repository.GetRepository<Ledger>().Queryable().AsNoTracking().Select(x=>new {x.Id,x.ReferenceFlag}).Where(x=>acids.Contains(x.Id)).ToList();
            var doe = DateTime.Now;
            var fys=this._repository.GetRepository<FinancialYear>().Queryable().Where(x=>x.IsActive).Select(x=>new{x.Id,x.OpeningDate,x.ClosingDate}).ToList();
            var ct = Helper.ConstCurTypeId;
            foreach (var doc in docs)
            {
                doc.ConstCurTypeId = ct;
                var vch = new Voucher { PageId = doc.PageId,IsAccepted=true,IsAccountsVisiblity=true };
                var batchid = Guid.NewGuid().ToString("N");
                var expense_amt = Math.Round(doc.GeneralExpenseLogs.Sum(x => x.Amount),2);
                if (expense_amt != doc.BasicAmount)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Total of Expenses Amount {expense_amt} does not match with Bill Net Amount {doc.NetAmount}");
                }
                if((doc.BasicAmount+doc.IGSTAmount+doc.CGSTAmount+doc.SGSTAmount)!=doc.NetAmount)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Net Amount {doc.NetAmount} does not match with Bill BasicAmt({doc.BasicAmount})+GSTAmt({doc.IGSTAmount + doc.CGSTAmount + doc.SGSTAmount})");
                }
                for (var i = 0; i < doc.GeneralExpenseLogs.Count; i++)
                {
                    var ad = doc.GeneralExpenseLogs.ElementAt(i);
                    var adv = new GeneralExpenseLog
                    {
                        ConstCurTypeId = ad.ConstCurTypeId,
                        CurTypeId = ad.CurTypeId,
                        CurRate = ad.CurRate,

                        Id = ad.ExpenseId,
                        ReferenceNo =
                            string.IsNullOrWhiteSpace(ad.ReferenceNo)
                                ? (doc.GeneralExpenseLogs.Count(x => string.IsNullOrWhiteSpace(x.ReferenceNo)) > 1
                                    ? doc.DocumentNo + "-" + i
                                    : doc.DocumentNo)
                                : ad.ReferenceNo,
                        ExpenseDate = ad.ExpenseDate,
                        ExpenseNatureId=ad.ExpenseNatureId,
                        fK_Voucher = vch,
                        VoucherNo = doc.DocumentNo,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        Amount = ad.Amount,
                        OfficeId = doc.OfficeId,
                        CreditAccountId = doc.CrAccountId,
                        DebitAccountId = doc.DrAccountId,
                        DriverId = ad.DriverId,
                        Remark = ad.Remark,
                        VehicleId = ad.VehicleId,
                        ViewId = ad.ViewId,
                        PaidInId = (ad.PaidInId <= 0 || ad.PaidInId == null) ? 1430 : ad.PaidInId,
                        IsBulkEntry = true,
                        Ref1 = ad.Ref1,
                        BatchId = batchid,
                        CreatedSessionId = Helper.SessionId(),
                        CreatedDOE = doe,
                        Amount1=ad.Amount1,
                        Amount2=ad.Amount2,
                        CNId=ad.CNId,
                        SettlementId=ad.SettlementId,
                        TripLogId=ad.TripLogId,
                        VoucherTypeId=doc.VoucherTypeId
                        
                    };
                    //adv.Amount = ad.FuelQty > 0 ? ad.FuelAmount : ad.CashCashAmount;
                    //1430=Cash
                    GeneralExpenses.Add(adv);
                }
                vch.ConstCurTypeId = doc.ConstCurTypeId;
                vch.CurTypeId = doc.CurTypeId;
                vch.CurRate = doc.CurRate;
                vch.OfficeId = doc.OfficeId;
                vch.VoucherNo = doc.DocumentNo;
                vch.VoucherDate = doc.DocumentDate;
                vch.VoucherDateTime = doc.DocumentDate;
                vch.ObjectState = vch.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                vch.VoucherAmount =Math.Abs(doc.NetAmount);
                vch.VoucherTypeId = 60;
                vch.Account1Id = doc.DrAccountId;
                vch.Account2Id = doc.CrAccountId;
                vch.Account3Id = doc.IGSTAccountId;
                vch.Account4Id = doc.CGSTAccountId;
                vch.Account5Id = doc.SGSTAccountId;
                vch.Amount1 = expense_amt * 1;
                vch.Amount2 = doc.NetAmount * -1;
                vch.Amount3 = doc.IGSTAmount;
                vch.Amount4 = doc.CGSTAmount;
                vch.Amount5 = doc.SGSTAmount;
                vch.UserRemark = doc.Remark;
                //TODO:Setup Account Narration from Template located with VoucherType
                vch.AccountingRemark = "";
                vch.BatchId =doc.BatchId= batchid;
                vch.ViewId = doc.ViewId;
                vch.CreatedSessionId = Helper.SessionId();
                vch.CreatedDOE = doe;
                vch.FinancialYearId = fys.FirstOrDefault(x =>
                    x.OpeningDate.Date <= vch.VoucherDate.Date && x.ClosingDate.Date >= vch.VoucherDate.Date)?.Id;
                vch.JsonData = JsonConvert.SerializeObject(new 
                {
                    CGSTP = doc.CGSTRate,
                    IGSTP = doc.IGSTRate,
                    SGSTP = doc.SGSTRate
                });
                vs.Add(vch);
                if (vch.VoucherDetails == null)
                {
                    vch.VoucherDetails = new List<VoucherDetail>();
                }
                for (var i = 1; i <=5; i++)
                {
                    decimal amount = 0;
                    long accountid = 0;
                    decimal rate = 0;
                    var particular = "";
                    switch (i)
                    {
                        case 1:
                            amount = vch.Amount1;
                            accountid = vch.Account1Id ?? 0;
                            rate = 0;
                            particular = $"General Expense Booked Rs.{amount}";
                            break;
                        case 2:
                            amount = vch.Amount2;
                            accountid = vch.Account2Id ?? 0;
                            rate = 0;
                            particular = $"General Expense Booked Rs.{amount}";
                            break;
                        case 3:
                            amount = vch.Amount3;
                            accountid = vch.Account3Id ?? 0;
                            rate = doc.IGSTRate;
                            particular = $"GE IGST Booked @ {doc.IGSTRate}";
                            break;
                        case 4:
                            amount = vch.Amount4;
                            accountid = vch.Account4Id ?? 0;
                            rate = doc.CGSTRate;
                            particular = $"GE CGST Booked @ {doc.CGSTRate}";
                            break;
                        case 5:
                            amount = vch.Amount5;
                            accountid = vch.Account5Id ?? 0;
                            rate = doc.SGSTRate;
                            particular = $"GE SGST Booked @ {doc.SGSTRate}";
                            break;
                    }
                    if (amount == 0)
                    {
                        continue;
                    }
                    else if(accountid<=0)
                    {
                        throw new BusinessException(ErrorCode.GLB106, $"Account Name is Required For VD{i} and Amount {amount}.\n Hint:{particular}");
                    }
                    var vd = new VoucherDetail
                    {
                        OfficeId = vch.OfficeId,
                        OrderId = i,
                        Amount = amount,
                        AccountId = accountid,
                        Rate= rate,
                        Narration = vch.UserRemark,
                        ObjectState = ObjectState.Added,
                        VoucherId = vch.Id,
                        BatchId = vch.BatchId,
                        CreatedSessionId = Helper.SessionId(),
                        CreatedDOE = doe,
                        Particular=particular,
                        ConstCurTypeId = vch.ConstCurTypeId,
                        CurTypeId = vch.CurTypeId,
                        CurRate = vch.CurRate
                    };
                    vch.VoucherDetails.Add(vd);
                    vds.Add(vd);
                    if(vd.VoucherDetailReferences==null) vd.VoucherDetailReferences=new List<VoucherDetailReference>();
                    //var lRepo = _repository.GetRepository<Ledger>().Queryable();
                    var isRefEnabled = acrefs.Any(x => x.Id == vd.AccountId && x.ReferenceFlag);
                    if (isRefEnabled)
                    {
                        foreach (GeneralExpenseLog log in GeneralExpenses.Where(x => x.BatchId == batchid))
                        {
                            var vdr = new VoucherDetailReference
                            {
                                ObjectState = ObjectState.Added,
                                Amount = vd.Amount > 0 ? log.Amount : -log.Amount,
                                ReferenceNo = log.ReferenceNo,
                                VDRTypeId = 1013,
                                VoucherDetailId = vd.Id,
                                BatchId = batchid,
                                CreatedSessionId = Helper.SessionId(),
                                CreatedDOE = doe,
                                ConstCurTypeId = vch.ConstCurTypeId,
                                CurTypeId = vch.CurTypeId,
                                CurRate = vch.CurRate
                            };
                            vd.VoucherDetailReferences.Add(vdr);
                            vdrs.Add(vdr);

                        }
                    }
                    
                }
                #region Validations
                if (vch.Amount1 + vch.Amount2 + vch.Amount3 + vch.Amount4 + vch.Amount5 != 0 || vch.VoucherDetails.Sum(x => x.Amount) != 0)
                {
                    throw new BusinessException(ErrorCode.VCH104,$"Doc No {doc.DocumentNo}");//Credit and Debit Amount mismatch for Voucher
                }
                if (vch.VoucherDetails.Count <= 1)
                {
                    throw new BusinessException(ErrorCode.VCH105, $"Doc No {doc.DocumentNo}");//Atleast two Voucher Details are required in Voucher
                }
                //if (vch.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) == 0)
                //{
                //    throw new BusinessException(ErrorCode.TADV102);//Atlead one VDR is Required in GeneralExpense Transaction
                //}
                if (vch.VoucherDetails.Any(voucherDetail => voucherDetail.VoucherDetailReferences.Count > 0 && voucherDetail.Amount != voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)))
                {
                    throw new BusinessException(ErrorCode.VCH106, $"Doc No {doc.DocumentNo}");//VoucherDetail and VoucherDetailReference Amount Doesn't Tally
                }
                #endregion
            }
            //Insert Vouchers
            this._repository.UOW.BulkInsert(vs,transaction);
            //Insert Vouchers Details
            var vids = vs.Select(x => x.BatchId).ToList();
            var vsbatches =
                _repository.GetRepository<Voucher>()
                    .Queryable()
                    .Where(y => vids.Contains(y.BatchId)).Select(x=>new {x.BatchId,x.Id}).ToList();
            Parallel.ForEach(vds, vd =>
            {
                vd.VoucherId = vsbatches?.FirstOrDefault(x => x.BatchId == vd.BatchId)?.Id??0;
            });
            if(vds.Any(x=>x.VoucherId==0))throw new BusinessException(ErrorCode.GLB106,"Voucher Integrity Failed!!");
            this._repository.UOW.BulkInsert(vds, transaction);
            //Insert GeneralExpenses
            Parallel.ForEach(GeneralExpenses, ad =>
            {
                ad.VoucherId = vsbatches?.FirstOrDefault(x => x.BatchId == ad.BatchId)?.Id ?? 0;
            });
            if (GeneralExpenses.Any(x => x.VoucherId == 0)) throw new BusinessException(ErrorCode.GLB106, "Voucher GeneralExpense Mapping Integrity Failed!!");
            this._repository.UOW.BulkInsert(GeneralExpenses, transaction);
            //Insert Voucher Details
            var vdrsbatches =
                _repository.GetRepository<VoucherDetail>()
                    .Queryable()
                    .Where(y => vids.ToList().Contains(y.BatchId)).Select(x => new { x.BatchId, x.Id,x.OrderId }).ToList();
            Parallel.ForEach(vds, vd =>
            {
                foreach (var vdr in vd.VoucherDetailReferences)
                {
                    vdr.VoucherDetailId= vdrsbatches?.FirstOrDefault(x => x.BatchId == vdr.BatchId && x.OrderId == vd.OrderId)?.Id ?? 0;
                }
            });
            if (vdrs.Any(x => x.VoucherDetailId == 0)) throw new BusinessException(ErrorCode.GLB106, "Voucher Reference Integrity Failed!!");
            this._repository.UOW.BulkInsert(vdrs, transaction);

        }
        
        #endregion
    }
}
