using EntityFramework.Extensions;

using LinqKit;

using MoreLinq;

using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

using TrackoAPI.Reporting.Models;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.FMS.Tyres;

namespace TrackoApi.Service
{
    public interface ITyreLogService : IService<TyreLog>
    {
        IQueryable<TyreLog> GetAllTyreLogList(int id);
        vwTyreBillView GetPurchaseBillView(long id, long type);
        vwTyreChassisBill GetChassisBillView(long key);
        vwTyreBillView GetTyreResaleBill(long key);
        vwTyreBillView GetTyreClaimBillView(long key);
        vwTyreBillView GetTyreScrapBillView(long key);
        vwTyreBillView GetTyreStoretransferOutBillView(long key);
        vwTyreBillView GetTyreStoretransferInBillView(long key);
        vwTyreBillView GetTyreRejectBillView(long key);
        vwTyreBillView GetTyreRemouldReceiptBillView(long key);

        TyreLogExtraInfo InsertOrUpdatePurchaseBillMRNSettlementView(vwTyreBillView view, IUnitOfWorkAsync _uom);
        TyreLogExtraInfo InsertOrUpdatePurchaseBillMRNView(vwTyreBillView view, IUnitOfWorkAsync _uom);
        TyreLogExtraInfo InsertOrUpdatePurchaseBillView(vwTyreBillView view, IUnitOfWorkAsync _uom);
        Task<TyreLogExtraInfo> InsertUpdateChasisTyreBillAsync(vwTyreChassisBill view, IUnitOfWorkAsync _uom);
        TyreLogExtraInfo InsertUpdateTyreIR(vwTyreBillView view, IUnitOfWorkAsync uom);
        TyreLogExtraInfo InsertUpdateTyreReSale(vwTyreBillView view, IUnitOfWorkAsync _uom);
        TyreLogExtraInfo InsertUpdateTyreClaim(vwTyreBillView view, IUnitOfWorkAsync _uom);
        Task DeleteGraphAsync(long key, IUnitOfWorkAsync uow);
        Task DeleteBySQLProc(long key, IUnitOfWorkAsync uow);
        TyreLogExtraInfo InsertUpdateTyreScrap(vwTyreBillView bill, IUnitOfWorkAsync unitOfWorkAsync);
        TyreLogExtraInfo InsertUpdateTyreStocktransferOutBillView(vwTyreBillView bill, IUnitOfWorkAsync unitOfWorkAsync);
        TyreLogExtraInfo InsertUpdateTyreStocktransferInBillView(vwTyreBillView bill, IUnitOfWorkAsync unitOfWorkAsync);
        TyreLogExtraInfo InsertUpdateTyreReject(vwTyreBillView view, IUnitOfWorkAsync _uom);
        TyreLogExtraInfo InsertUpdateTyreClaimReceiptBillView(vwTyreBillView view, IUnitOfWorkAsync _uom);
        TyreLogExtraInfo InsertUpdateTyreRemouldReceipt(vwTyreBillView view, IUnitOfWorkAsync _uom);
        TyreLogExtraInfo InsertUpdateTyreClaimSettlement(vwTyreBillView view, IUnitOfWorkAsync _uom);

        IQueryable<TyreLog> GetReportData(string classIds, string accountIds,
            long categoryId, string ledgerFilterType);

        TyreLogExtraInfo InsertUpdateReceipt(vwTyreBillView view, IUnitOfWorkAsync uom);
        TyreLogExtraInfo InsertUpdateIssue(vwTyreBillView view, IUnitOfWorkAsync uom);
    }
    public class TyreLogService : Service<TyreLog>, ITyreLogService
    {
        private readonly IRepositoryAsync<TyreLog> _repository;
        public TyreLogService(IRepositoryAsync<TyreLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<TyreLog> GetAllTyreLogList(int brandid)
        {
            return _repository.GetAllTyreLogList(brandid);
        }

        public vwTyreBillView GetPurchaseBillView(long id, long type)
        {
            return _repository.GeTyreBillPurchaseView(id, type);
        }

        public vwTyreChassisBill GetChassisBillView(long key)
        {
            return _repository.GetChassisBillView(key);
        }
        public vwTyreBillView GetTyreClaimBillView(long key)
        {
            return _repository.GetTyreClaimBillView(key);
        }
        public TyreLogExtraInfo InsertUpdateTyreReSale(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.ResaleLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account {view.PrimaryCreditAccountId} or Primary Credit Ammount {view.PrimaryCreditAmount} has Invalid Value.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 || view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account {view.PrimaryDebitAccountId} or Primary Debit Ammount {view.PrimaryDebitAmount} has Invalid Value.");
            }
            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<TyreLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<TyreLifePerformanceLog>();
            Voucher v = new Voucher();
            //Collect Distincs ReferenceId's from Posted SpareLogs
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == 38);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = vRepo.Query(x => x.Id == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (v == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.VoucherId == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var issuerefids = view.ResaleLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var newtyrestatus = new long[] { 1099,1100 };
            List<TyreLog> issueReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => issuerefids.Contains(x.Id)).ToList();
            var newTyreLogList = new List<TyreLog>();
            var oldTyreLogs = new List<TyreLog>();
            foreach (var l in view.ResaleLog)
            {
                /************************************************************
                *************||Tyre Issue Logics Start||*********************
                *************************************************************/
                #region Tyre Issue Logic
                var i = new TyreLog();//Issued Log
                var ir = issueReferenceLogs.Find(x => x.Id == l.ReferenceId);//Issue Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.TyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.TyreNo}");
                }
                if ((!newtyrestatus.Contains(ir.TyreStatusId)) || (ir.TyreStatusId == 1100 && ir.VoucherTypeId != 66))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {l.TyreNo} can't be resaled");
                }
                
                if (l.Id > 0)
                {
                    i = existingTyreLogs.Find(x => x.Id == l.Id);//Issued Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Tyre has been altered restore all tyre status to previous logs status
                    //oldTyreLogs.Add(RestorePreviousTyreStatus(i));
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                }
                i.TSLId = l.TSLId;
                i.Rate = l.PurchaseAmount;
                i.OtherAmount = l.OtherAmt;
                i.SubTotal = l.NetValue;
                i.TyreTotalAmount = l.NetValue;                
                i.NetAmount = l.NetValue;
                i.DiscountAmount = i.DiscountPercent = 0;
                i.IsStepney = false;
                i.IsException =l.IsException;
                i.KmReading = 0;
                i.KmRun = 0;
                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.TyreId = ir.TyreId;
                i.fk_Tyre = ir.fk_Tyre;
                i.TyreLife = ir.TyreLife;
                i.TyreStatusId = 1108;//Resaled
                i.TyreNo = i.fk_Tyre.TyreNo;
                i.VoucherTypeId = 38;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = ir.TyreLife;
                i.fk_Tyre.S_StatusId = 1108;//Resaled
                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        i.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }
                if (i.Id > 0)
                {
                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newTyreLogList.Add(i);
                #endregion
            }
            var tyreRepo = _uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    _repository.Update(RestorePreviousTyreStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }
            #region Prepare Issue Voucher
            var totalStoreCreditAmt = -issueReferenceLogs.Sum(x => x.NetAmount);
            var totalIncomeCredit = -view.ResaleLog.Sum(x => x.OtherAmt);
            var totalVendorAmt = view.ResaleLog.Sum(x => x.NetValue);

            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.ConstCurTypeId = view.ConstCurTypeId;

            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = 38;
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount;
            v.Account1Id = view.PrimaryDebitAccountId;
            v.Account2Id = view.PrimaryCreditAccountId;
            v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
            v.Account3Id = view.OtherLedgerId;
            v.Amount3 = view.OtherAmount;
            v.Account7Id = view.TCSAccountId;
            v.Amount7 = view.TCSAmount;
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";

            //if (v.Amount1 != totalVendorAmt)
            //{
            //    throw new BusinessException(ErrorCode.GLB106, $"Tyre Total Net Value {totalVendorAmt} Does't match Voucher Primary Debit Amount {v.Amount1}");
            //}
            if (v.Amount2 != totalStoreCreditAmt)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Credit Amount {totalStoreCreditAmt} Does't match Voucher Primary Credit Amount {v.Amount2}");
            }
            if (totalIncomeCredit != 0 && (v.Amount3 != totalIncomeCredit))
            {
                throw new BusinessException(ErrorCode.GLB106, $"Income Total Amount {totalIncomeCredit} Does't match Voucher Primary Credit Amount {v.Amount3}");
            }
            PrepareVoucherDetails(_repository, v);
            #endregion
            #region Validations
            var vdrrequired =
                _repository.GetRepository<VoucherType>()
                    .Queryable()
                    .Where(x => x.Id == 38)
                    .Select(x => new
                    {
                        x.VDRRequired,
                        x.VDRequired
                    })
                    .FirstOrDefault();
            if (vdrrequired != null)
            {
                if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                {
                    throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                }

                if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
                {
                    throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
                }
            }
            #endregion
            tei = tei ?? new TyreLogExtraInfo();
            tei.fk_Voucher = v;

            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;

            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherId = v.Id;
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = 38;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.TCSAccountId = view.TCSAccountId;
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newTyreLogList)
            {
                log.VoucherId = v.Id;
                log.fk_Voucher = v;
                log.fk_Tyre.S_VoucherId = v.Id;
                log.fk_Tyre.S_VoucherDate = v.VoucherDate;
                log.fk_Tyre.fk_S_Voucher = v;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            _uom.SaveChanges();
            return tei;
        }

        public TyreLogExtraInfo InsertUpdateTyreClaim(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.ClaimLog.Count == 0) 
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Account {view.PrimaryCreditAccountId} is required.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Tyre Vendor Account {view.PrimaryDebitAccountId} is required");
            }

            var teiRepo = _uom.RepositoryAsync<TyreLogExtraInfo>();


            //Collect Distincs ReferenceId's from Posted SpareLogs
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var claimrefids = view.ClaimLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var allowedStatus = view.VoucherTypeId == 39|| view.VoucherTypeId == 122 ? new long[] { 1100 } : new long[] { 1100, 1099 };
            List<TyreLog> claimReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => claimrefids.Contains(x.Id)).ToList();
            var newTyreLogList = new List<TyreLog>();
            var oldTyreLogs = new List<TyreLog>();
            foreach (var l in view.ClaimLog)
            {
                /************************************************************
                *************||Tyre claim Logics Start||*********************
                *************************************************************/
                #region Tyre claim Logic
                var i = new TyreLog();//claim Log
                var ir = claimReferenceLogs.Find(x => x.Id == l.ReferenceId);//Issue Reference Log

                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.TyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.TyreNo}");
                }
                if (!allowedStatus.Contains(ir.TyreStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {l.TyreNo} can't be send for claim / remould");
                }
                if (l.Id > 0)
                {
                    i = existingTyreLogs.Find(x => x.Id == l.Id);//claim Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }

                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Tyre has been altered restore all tyre status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                    //oldTyreLogs.Add(RestorePreviousTyreStatus(i));
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }

                //39 Sent for Retreating
                //40 Sent for Claim
                //1105 SFR Sent For Remould
                //1106 SFC Sent For Claim
                i.TSLId = l.TSLId;
                i.Rate =
                    i.SubTotal =
                        i.OtherAmount = i.NetAmount = i.TyreTotalAmount = i.DiscountAmount = i.DiscountPercent = i.KmReading = i.KmRun = 0;
                i.IsStepney = false;
                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.TyreId = ir.TyreId;
                i.ReasonId = ir.ReasonId;
                i.fk_Tyre = ir.fk_Tyre;
                i.TyreLife = ir.TyreLife;
                i.TyreStatusId = (view.VoucherTypeId == 39 ? 1105: view.VoucherTypeId == 122 ? 1929 : 1106);
                i.TyreNo = i.fk_Tyre.TyreNo;
                i.IsException = l.IsException;
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = ir.TyreLife;
                i.fk_Tyre.S_StatusId = i.TyreStatusId;
                i.CalVat = view.CalVat;
                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        i.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }
                if (i.Id > 0)
                {
                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newTyreLogList.Add(i);
                #endregion
            }
            var tyreRepo = _uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    _repository.Update(RestorePreviousTyreStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }


            tei = tei ?? new TyreLogExtraInfo();
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.ViewId = view.ViewId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.TCSAccountId = view.TCSAccountId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newTyreLogList)
            {
                log.fk_Tyre.S_VoucherDate = tei.VoucherDate;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            _uom.SaveChanges();
            return tei;
        }

        public TyreLogExtraInfo InsertUpdateTyreIR(vwTyreBillView view, IUnitOfWorkAsync uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.IssueReceiptLogs.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account {view.PrimaryCreditAccountId} has Invalid Value.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account {view.PrimaryDebitAccountId} has Invalid Value.");
            }
            Voucher v = null;

            #region Check Circuler Reference for Tyres
            //var duplicatecheckgroup = view.IssueReceiptLogs.GroupBy(x => new { x.IssueTyreId, x.ReceiptTyreId });
            if (view.IssueReceiptLogs.GroupBy(x => new { x.IssueTyreId, x.ReceiptTyreId }).ToList().Any(x => x.Count() > 1))
            {
                throw new BusinessException(ErrorCode.GLB106, "Same tyre can't be issued against it's receipt.");
            }
            var groupbyvehicle = view.IssueReceiptLogs.GroupBy(x => x.VehicleId).ToList();
            foreach (IGrouping<long, vwTyreIssueReceipt> grouping in groupbyvehicle)
            {
                var issuelist = grouping.Select(x => x.IssueTyreId).ToList();
                var receiptlist = grouping.Select(x => x.ReceiptTyreId).ToList();
                if (issuelist.TrueForAll(x => receiptlist.Contains(x)))
                {
                    throw new BusinessException(ErrorCode.GLB106, "Both receipt and issue operation can't be done in same transaction for same Tyre.");
                }

                if (issuelist.GroupBy(x => x).Any(x => x.Count() > 1))
                {
                    throw new BusinessException(ErrorCode.GLB106, "Same tyre can't be issued more than one in Single Transaction.");
                }
                if (receiptlist.GroupBy(x => x).Any(x => x.Count() > 1))
                {
                    throw new BusinessException(ErrorCode.GLB106, "Same tyre can't be received more than one in Single Transaction.");
                }
            }
            #endregion
            var vRepo = uom.RepositoryAsync<Voucher>();
            var teiRepo = uom.RepositoryAsync<TyreLogExtraInfo>();
            var tpiRepo = uom.RepositoryAsync<TyreLifePerformanceLog>();
            var tyreCheckRepo = uom.RepositoryAsync<TyreCheck>();
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            if (view.Id > 0)
            {//Try to find existing tyre extra info record
                tei = teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && (x.VoucherTypeId == 35 || x.VoucherTypeId == 34));
            }
            if (view.Id > 0 && tei != null && vRepo.Queryable().Any(x => x.Id == tei.VoucherId && (x.VoucherTypeId == 35 || x.VoucherTypeId == 34)))
            {
                //Try to find existing voucher record
                v = vRepo.Queryable().Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).FirstOrDefault(x => x.Id == tei.VoucherId && (x.VoucherTypeId == 35 || x.VoucherTypeId == 34));
            }
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (tei != null && tei.Id > 0)
            {
                //In-case updating existing record find all existing attached Tyre Logs
                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_TyreCheck).Include(x => x.fk_PreviousLog).Include(x => x.fk_NextLog).Where(x => (x.ExtraInfoId == tei.Id) && (x.VoucherTypeId == 35 || x.VoucherTypeId == 34)).ToList();
            }
            //Extract Ids of Primary Key
            var oldissueids = existingTyreLogs.Where(x => x.VoucherTypeId == 34 && x.Id > 0).Select(x => x.Id).ToList();
            var oldreceiptids = existingTyreLogs.Where(x => x.VoucherTypeId == 35 && x.Id > 0).Select(x => x.Id).ToList();


            var issuerefids = view.IssueReceiptLogs.Select(x => x.IssueReferenceId).ToList();
            var receptrefids = view.IssueReceiptLogs.Select(x => x.ReceiptReferenceId).ToList();

            //Fatch Tyre Performance Logs in case updating record
            var issueTyrePerformance = tpiRepo.Queryable().Where(x => oldissueids.Contains(x.FirstIssueLogId.Value)).ToList();
            var receiptTyrePerformance = tpiRepo.Queryable().Where(x => oldreceiptids.Contains(x.LastReceiptLogId.Value)).ToList();

            List<TyreLog> issueReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => issuerefids.Contains(x.Id)).ToList();
            List<TyreLog> receiptReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => receptrefids.Contains(x.Id)).ToList();

            //Fatch Tyre Performance Logs for fresh receipt so that we could update LastReceiptLogId
            var receiptLogTyrePerformanceIds = receiptReferenceLogs.Select(x => x.TyreId + "-" + x.TyreLife).ToList();
            //TODO:Check if It works
            var receiptTpData = tpiRepo.Queryable().Where(x => receiptLogTyrePerformanceIds.Contains(x.TyreId + "-" + x.Life)).ToList();

            var newtyrevtypes = new long[] { 66,27,135, 29, 32,79, 41 };
            var issueNetamount = issueReferenceLogs.Where(x => newtyrevtypes.Contains(x.VoucherTypeId)).Sum(x => x.NetAmount);

            var cv = issueReferenceLogs.Any(x => newtyrevtypes.Contains(x.VoucherTypeId) && x.NetAmount > 0);
            if (cv)
            {
                v = v ?? new Voucher();
            }
            else
            {
                if (v != null && v.Id > 0)
                {
                    v.ObjectState = ObjectState.Deleted;
                    foreach (var x in v.VoucherDetails)
                    {
                        x.ObjectState = ObjectState.Deleted;
                        foreach (var y in x.VoucherDetailReferences) y.ObjectState = ObjectState.Deleted;
                    }
                    vRepo.Delete(v);
                }
            }
            var oldTyreLogs = new List<TyreLog>();
            var newIssuedLogs = new List<TyreLog>();
            var newReceiptLogs = new List<TyreLog>();
            var newTyrePerformance = new List<TyreLifePerformanceLog>();
            foreach (var l in view.IssueReceiptLogs)
            {
                /************************************************************
                *************||Tyre Issue Logics Start||*********************
                *************************************************************/
                #region Tyre Issue Logic

                var i = new TyreLog();//Issued Log
                var ir = issueReferenceLogs.Find(x => x.Id == l.IssueReferenceId);//Issue Reference Log

                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log didn't found for Tyre No {l.IssueTyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.IssueTyreNo}");
                }
                if (l.IssueLogId > 0)
                {
                    i = existingTyreLogs.Find(x => x.Id == l.IssueLogId);//Issued Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.IssueTyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //var tp = issueTyrePerformance.FirstOrDefault(x => x.FirstIssueLogId == i.Id);
                    //tp.ObjectState=ObjectState.Deleted;
                    //tpiRepo.Delete(tp);
                    //if Tyre has been altered restore all tyre status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                }
                i.TSLId = l.TSLId;
                i.NetAmount = i.Rate = i.SubTotal = i.TyreTotalAmount = l.IssueAmount;
                i.DiscountAmount = i.OtherAmount = i.DiscountPercent = 0;
                i.IsException = l.IsException;
                i.IsStepney = l.IsStepney;
                i.JobsheetId = l.JobSheetId;
                i.KmReading = l.IssueOnKM;
                i.KmRun = 0;
                i.MechanicId = l.MechanicId;
                i.CreditAccountId = ir.DebitAccountId;//view.PrimaryDebitAccountId.GetValueOrDefault(0);
                i.DebitAccountId = view.PrimaryCreditAccountId;
                i.Remark = l.IssueRemark;
                i.TyreId = ir.TyreId;
                i.fk_Tyre = ir.fk_Tyre;
                i.TyreLife = ir.TyreLife;
                i.TyreStatusId = 1103;//OnVehicle
                i.TyreNo = i.fk_Tyre.TyreNo;
                i.VoucherTypeId = 34;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.VehicleId = l.VehicleId;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = ir.TyreLife;
                i.fk_Tyre.S_StatusId = 1103;//OnVehicle
                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                //if (!string.IsNullOrWhiteSpace(l.IssueRowVersionId))
                //{
                //    i.RowVersion = Encoding.UTF8.GetBytes(l.IssueRowVersionId);
                //}
                #region TyreCheck Issue
                if (i.fk_TyreCheck == null || i.fk_TyreCheck.Id == 0)
                {
                    i.fk_TyreCheck = new TyreCheck();
                }
                i.fk_TyreCheck.AirPressure = l.IssuePSI;
                i.fk_TyreCheck.CheckDate = view.DocumentDate;
                i.fk_TyreCheck.KmRun = 0;
                i.fk_TyreCheck.Remarks = l.IssueRemark;
                i.fk_TyreCheck.TreadDepth = l.IssueTreadWear;
                i.fk_TyreCheck.TyreId = i.TyreId;
                i.fk_TyreCheck.VehicleId = i.VehicleId.Value;
                i.fk_TyreCheck.WheelPositionId = l.WheelPositionId;
                i.fk_TyreCheck.fk_Tyre = i.fk_Tyre;
                i.fk_TyreCheck.ObjectState = i.fk_TyreCheck.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                #endregion
                if (i.Id > 0)
                {

                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;

                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newIssuedLogs.Add(i);
                #endregion

                /************************************************************
                *************||Tyre Receipt Logics Start||*******************
                *************************************************************/
                #region Tyre Receipt Logic
                var r = new TyreLog();
                var rr = receiptReferenceLogs.Find(x => x.Id == l.ReceiptReferenceId);

                if (rr == null || rr.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.ReceiptTyreNo}");
                }
                if (l.ReceiptLogId > 0)
                {
                    r = existingTyreLogs.Find(x => x.Id == l.ReceiptLogId);
                    if (r.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.ReceiptTyreNo}[Referenced Transaction No :{r.fk_NextLog.VoucherNo}]");
                    }
                }
                if (rr != null && rr.NextLogId > 0 && r.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Invalid Reference for Tyre No {rr.TyreNo}.");
                }
                //Check if Tyre No has been altered
                if (r.Id > 0 && r.PreviousLogId != rr.Id)
                {
                    //Restore Last Receipt Log Id when Receipt Tyre No Changed
                    var td = receiptTyrePerformance.FirstOrDefault(x => x.LastReceiptLogId == r.Id);
                    if (td != default(TyreLifePerformanceLog))
                    {
                        var lastReceipt = _repository.GetLastTyreLogByStatusAndLife(r.TyreId, 35, r.TyreLife, r.Id);
                        td.LastReceiptLogId = lastReceipt?.Id;
                        td.ObjectState = ObjectState.Modified;
                        tpiRepo.Update(td);
                    }
                    //if Tyre has been altered restore all tyre status to previous logs status
                    //oldTyreLogs.Add(RestorePreviousTyreStatus(r));
                    //r.fk_Tyre = null;
                    //r.TyreId = 0;
                    //r.fk_PreviousLog = null;
                    //r.PreviousLogId = null;
                }
                r.NetAmount = r.Rate = r.SubTotal = r.TyreTotalAmount = l.ReceiptAmount;
                r.DiscountAmount = r.OtherAmount = r.DiscountPercent = 0;
                r.IsRemoulded = rr.IsRemoulded;
                r.IsStepney = rr.IsStepney;
                r.JobsheetId = l.JobSheetId;
                r.IsException = l.IsException;
                r.KmReading = l.ReceiptOutKm;
                r.KmRun = l.ReceiptOutKm - rr.KmReading;//Calculate Difference
                r.MechanicId = l.MechanicId;
                r.CreditAccountId = rr.DebitAccountId;//l.OwnerId.GetValueOrDefault(0) == 0 ? rr.CreditAccountId : l.OwnerId.GetValueOrDefault(0);
                r.DebitAccountId = view.PrimaryDebitAccountId.Value;
                r.Remark = l.ReceiptRemark;
                r.TyreId = l.ReceiptTyreId;
                r.fk_Tyre = rr.fk_Tyre;
                r.ReasonId = l.ReasonId;
                r.NextUseId = l.NextUseId;
                r.TyreLife = rr.TyreLife;
                r.TyreStatusId = 1100;
                r.TyreNo = rr.fk_Tyre.TyreNo;
                r.VoucherTypeId = 35;
                r.VoucherDate = view.DocumentDate;
                r.VoucherNo = view.DocumentNo;
                r.VehicleId = l.VehicleId;
                r.NextUseId = l.NextUseId;
                r.fk_Tyre.S_VoucherTypeId = r.VoucherTypeId;
                r.fk_Tyre.ObjectState = ObjectState.Modified;
                r.fk_Tyre.S_VoucherDate = r.VoucherDate;
                r.fk_Tyre.S_CreditAccountId = r.CreditAccountId;
                r.fk_Tyre.S_DebitAccountId = r.DebitAccountId;
                r.fk_Tyre.S_Life = rr.TyreLife;
                r.fk_Tyre.S_StatusId = r.TyreStatusId;
                r.PreviousLogId = rr.Id;
                r.fk_PreviousLog = rr;
                //Set Issue Receipt Entry in Cross
                i.fk_IssueReceipt = r;
                if (r.Id > 0) i.IssueReceiptId = r.Id;
                if (r.IssueReceiptId.GetValueOrDefault(0) == 0)
                {
                    r.fk_IssueReceipt = null;
                    r.IssueReceiptId = null;
                }
                //if (!string.IsNullOrWhiteSpace(l.ReceiptRowVersionId))
                //{
                //    r.RowVersion = Encoding.UTF8.GetBytes(l.ReceiptRowVersionId);
                //}
                //r.PreviousLogId = ir.Id;
                rr.NextLogId = r.Id;
                rr.fk_NextLog = r;
                #region TyreCheck Receipt
                if (r.fk_TyreCheck == null || r.fk_TyreCheck.Id == 0)
                {
                    r.fk_TyreCheck = new TyreCheck();
                }
                r.fk_TyreCheck.AirPressure = 0;
                r.fk_TyreCheck.CheckDate = view.DocumentDate;
                r.fk_TyreCheck.KmRun = l.ReceiptKmRun;
                r.fk_TyreCheck.Remarks = l.ReceiptRemark;
                r.fk_TyreCheck.TreadDepth = l.ReceiptTreadWear;
                r.fk_TyreCheck.TyreId = r.TyreId;
                r.fk_TyreCheck.VehicleId = r.VehicleId.Value;
                r.fk_TyreCheck.WheelPositionId = l.WheelPositionId;
                r.fk_TyreCheck.fk_Tyre = r.fk_Tyre;
                r.fk_TyreCheck.ObjectState = r.fk_TyreCheck.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                #endregion
                if (r.Id > 0)
                {
                    r.ObjectState = ObjectState.Modified;
                    r.fk_Tyre.S_TyreLogId = r.Id;

                }
                else
                {
                    r.ObjectState = ObjectState.Added;
                    r.fk_Tyre.fk_S_TyreLog = r;
                }
                newReceiptLogs.Add(r);
                #endregion

            }
            var tyreRepo = uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {


                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }
            if (cv)
            {

                #region Prepare Issue Voucher
                v.VoucherDate = view.DocumentDate;
                v.VoucherDateTime = view.DocumentDate;
                v.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault(0);
                v.VoucherNo = view.DocumentNo;
                v.Amount1 = view.PrimaryDebitAmount;
                v.Account1Id = view.PrimaryDebitAccountId;
                v.Account2Id = view.PrimaryCreditAccountId;
                v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;

                v.Account3Id = view.CGSTLedgerId;
                v.Amount3 = view.CGSTAmount;

                v.Account4Id = view.OtherLedgerId;
                v.Amount4 = view.OtherAmount;
                v.Account5Id = view.SGSTLedgerId;
                v.Amount5 = view.SGSTAmount;
                v.Account6Id = view.IGSTLedgerId;
                v.Amount6 = view.IGSTAmount;
                v.UserRemark = view.Narration;
                v.OfficeId = view.OfficeId;
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //TODO:Setup Account Narration from Template located with VoucherType
                v.AccountingRemark = "";

                if (v.Amount1 != issueNetamount)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre Total Net Value {issueNetamount} Does't match Voucher Primary Debit Amount {v.Amount1}");
                }
                if (v.Amount2 != -issueNetamount)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Bill Total Amount {-issueNetamount} Does't match Voucher Primary Credit Amount {v.Amount2}");
                }
                PrepareVoucherDetails(_repository, v);
                #endregion
                #region Validations
                var vdrrequired =
                    _repository.GetRepository<VoucherType>()
                        .Queryable()
                        .Where(x => x.Id == 34)
                        .Select(x => new
                        {
                            x.VDRRequired,
                            x.VDRequired
                        })
                        .FirstOrDefault();
                if (vdrrequired != null)
                {
                    if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                    {
                        throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                    }

                    if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
                    {
                        throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
                    }
                }
                #endregion

            }
            tei = tei ?? new TyreLogExtraInfo();
            if (cv) tei.fk_Voucher = v;
            tei.OfficeId = view.OfficeId;
            tei.CalVat = view.CalVat;
            tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            if (cv) tei.VoucherId = v.Id;
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = 34;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            var mlids = newIssuedLogs.Select(x => $"{(x.TyreLife - 1)}-{x.TyreId}").ToList();
            var mileageList = tpiRepo.Queryable().Where(x => mlids.Contains((x.Life + "-" + x.TyreId))).Select(x => new { Mileage = x.TyrePreviousMileage + x.TyreLifeMileage, x.TyreId }).ToList();
            foreach (var log in newIssuedLogs)
            {
                if (cv)
                {
                    log.VoucherId = v.Id;
                    log.fk_Voucher = v;
                    log.fk_Tyre.S_VoucherId = v.Id;
                    log.fk_Tyre.S_VoucherDate = v.VoucherDate;
                    log.fk_Tyre.fk_S_Voucher = v;

                }
                else
                {//If Voucher is not applicable set voucher values as null
                    log.VoucherId = null;
                    log.fk_Voucher = null;
                    log.fk_Tyre.S_VoucherId = null;
                    log.fk_Tyre.S_VoucherDate = view.DocumentDate;
                    log.fk_Tyre.fk_S_Voucher = null;
                }
                //Only Create Tyre Performance in case Tyre is issued first time
                if (log.fk_PreviousLog.TyreStatusId == 1099)
                {
                    var tpi = issueTyrePerformance.FirstOrDefault(x => x.FirstIssueLogId == log.Id) ?? new TyreLifePerformanceLog();
                    if (tpi.Id == 0) tpi.FirstIssueLogId = log.Id;
                    tpi.CurrentMileage = 0;
                    tpi.Life = log.TyreLife;
                    tpi.TyreLifeMileage = 0;
                    tpi.LifeStartDate = log.VoucherDate;
                    var mileage = mileageList.FirstOrDefault(x => x.TyreId == log.TyreId);
                    tpi.TyrePreviousMileage = mileage?.Mileage ?? 0;
                    tpi.PurchaseAmount = log.NetAmount;
                    tpi.SupplierId = log.DebitAccountId;
                    tpi.LifeEndDate = null;
                    tpi.fk_FirstIssueLog = log;
                    tpi.ObjectState = tpi.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    newTyrePerformance.Add(tpi);
                }
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                log.ObjectState = log.Id > 0 ? ObjectState.Modified : ObjectState.Added; ;
                _repository.Update(log.fk_PreviousLog);
                if (log.fk_TyreCheck.Id > 0)
                {
                    tyreCheckRepo.Update(log.fk_TyreCheck);
                }
                else
                {
                    tyreCheckRepo.Insert(log.fk_TyreCheck);
                }
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            foreach (var log in newReceiptLogs)
            {
                if (cv)
                {
                    log.VoucherId = v.Id;
                    log.fk_Voucher = v;
                    log.fk_Tyre.S_VoucherId = v.Id;
                    log.fk_Tyre.S_VoucherDate = v.VoucherDate;
                    log.fk_Tyre.fk_S_Voucher = v;
                }
                else
                {//If Voucher is not applicable set voucher values as null
                    log.VoucherId = null;
                    log.fk_Voucher = null;
                    log.fk_Tyre.S_VoucherId = null;
                    log.fk_Tyre.S_VoucherDate = view.DocumentDate;
                    log.fk_Tyre.fk_S_Voucher = null;
                }
                //Extract Tyre Performance for current record in loop and set LastReceiptLog as this
                var tpd = receiptTpData.FirstOrDefault(x => x.Life == log.TyreLife && log.TyreId == x.TyreId);
                if (tpd != null && ((tpd.LastReceiptLogId.HasValue && tpd.LastReceiptLogId < log.Id) || log.Id == 0))
                {
                    tpd.LastReceiptLogId = log.Id;
                    tpd.fk_LastReceiptLog = log;
                    tpd.ObjectState = ObjectState.Modified;
                    tpd.TyreLifeMileage += log.KmRun;//Update Life Milage
                    newTyrePerformance.Add(tpd);

                }
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                log.ObjectState = log.Id > 0 ? ObjectState.Modified : ObjectState.Added; ;
                _repository.Update(log.fk_PreviousLog);
                if (log.fk_TyreCheck.Id > 0)
                {
                    tyreCheckRepo.Update(log.fk_TyreCheck);
                }
                else
                {
                    tyreCheckRepo.Insert(log.fk_TyreCheck);
                }
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            uom.SaveChanges();
            if (view.Id > 0)
            {
                var newLogsIds = newIssuedLogs.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                newLogsIds.AddRange(newReceiptLogs.Where(x => x.Id > 0).Select(x => x.Id));
                newLogsIds = newLogsIds.Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    var td = receiptTyrePerformance.FirstOrDefault(x => x.LastReceiptLogId == log.Id || x.FirstIssueLogId == log.Id);
                    if (td != default(TyreLifePerformanceLog))
                    {
                        if (td.FirstIssueLogId == log.Id)
                        {
                            td.ObjectState = ObjectState.Deleted;
                            tpiRepo.Delete(td);
                        }
                        else
                        {
                            if (td.LastReceiptLogId > 0 && log.Id != td.LastReceiptLogId)
                            {
                                throw new BusinessException(ErrorCode.GLB106, "Cannot Delete Tyre Performance Log as it is Locked");
                            }
                            //Find Last Log Other than current in loop
                            TyreLog lstTl = _repository.GetLastTyreLogByStatusAndLife(log.TyreId, (td.FirstIssueLogId == log.Id ? 27 : 34), log.TyreLife, log.Id);
                            td.LastReceiptLogId = lstTl?.Id;
                            td.ObjectState = ObjectState.Modified;
                            tpiRepo.Update(td);
                        }

                    }
                    //if (log.NextLogId!=null&&deletedIds.Contains(log.NextLogId))
                    //{
                    //    log.NextLogId = null;
                    //    log.fk_NextLog = null;

                    //}//|| deletedIds.Contains(log.PreviousLogId))
                    //if (log.PreviousLogId != null && deletedIds.Contains(log.PreviousLogId))
                    //{
                    //    log.PreviousLogId = null;
                    //    log.fk_PreviousLog = null;
                    //}
                    if (log.fk_TyreCheck != null && log.fk_TyreCheck.Id > 0)
                    {
                        log.fk_TyreCheck.ObjectState = ObjectState.Deleted;
                        tyreCheckRepo.Delete(log.fk_TyreCheck);
                    }
                    log.IssueReceiptId = null;
                    log.fk_IssueReceipt = null;
                    _repository.Update(RestorePreviousTyreStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }
            }
            uom.SaveChanges();
            foreach (var log in newIssuedLogs)
            {
                log.fk_IssueReceipt.IssueReceiptId = log.Id;
                log.fk_IssueReceipt.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_IssueReceipt);
            }
            uom.SaveChanges();
            var vehids =
                newIssuedLogs.Select(x => x.VehicleId)
                    .ToList();
            vehids.AddRange(newReceiptLogs.Select(x => x.VehicleId));
            vehids = vehids.Distinct().ToList();
            //var modelcount=
            //_repository.Queryable().Count(x=>x.VehicleId==)
            foreach (var log in newTyrePerformance)
            {
                log.TyreId = log.fk_FirstIssueLog.TyreId;
                log.fk_Tyre = log.fk_FirstIssueLog.fk_Tyre;
                if (log.Id > 0) tpiRepo.Update(log);
                else tpiRepo.Insert(log);
            }
            uom.SaveChanges();
            return tei;
        }
        public TyreLogExtraInfo InsertUpdateReceipt(vwTyreBillView view, IUnitOfWorkAsync uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.ReceiptLogs.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Receipt Details is Missing");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account {view.PrimaryDebitAccountId} has Invalid Value.");
            }
            #region Check Circuler Reference for Tyres
            //var duplicatecheckgroup = view.IssueReceiptLogs.GroupBy(x => new { x.IssueTyreId, x.ReceiptTyreId });
           
            var groupbyvehicle = view.ReceiptLogs.GroupBy(x => x.VehicleId).ToList();
            if (groupbyvehicle.Select(grouping => grouping.Select(x => x.ReceiptTyreId).ToList()).Any(receiptlist => receiptlist.GroupBy(x => x).Any(x => x.Count() > 1)))
            {
                throw new BusinessException(ErrorCode.GLB106, "Same tyre can't be received more than one in Single Transaction.");
            }
            #endregion
            var teiRepo = uom.RepositoryAsync<TyreLogExtraInfo>();
            var tpiRepo = uom.RepositoryAsync<TyreLifePerformanceLog>();
            var tyreCheckRepo = uom.RepositoryAsync<TyreCheck>();
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            if (view.Id > 0)
            {//Try to find existing tyre extra info record
                tei = teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == 35);
            }
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (tei != null && tei.Id > 0)
            {
                //In-case updating existing record find all existing attached Tyre Logs
                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_TyreCheck).Include(x => x.fk_PreviousLog).Include(x => x.fk_NextLog).Where(x => (x.ExtraInfoId == tei.Id) && x.VoucherTypeId == 35).ToList();
            }
            //Extract Ids of Primary Key
            var oldreceiptids = existingTyreLogs.Where(x => x.VoucherTypeId == 35 && x.Id > 0).Select(x => x.Id).ToList();

            var receptrefids = view.ReceiptLogs.Select(x => x.ReceiptReferenceId).ToList();

            //Fatch Tyre Performance Logs in case updating record
            var receiptTyrePerformance = tpiRepo.Queryable().Where(x => oldreceiptids.Contains(x.LastReceiptLogId.Value)).ToList();
            
            List<TyreLog> receiptReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => receptrefids.Contains(x.Id)).ToList();

            //Fatch Tyre Performance Logs for fresh receipt so that we could update LastReceiptLogId
            var receiptLogTyrePerformanceIds = receiptReferenceLogs.Select(x => x.TyreId + "-" + x.TyreLife).ToList();
            //TODO:Check if It works
            var receiptTpData = tpiRepo.Queryable().Where(x => receiptLogTyrePerformanceIds.Contains(x.TyreId + "-" + x.Life)).ToList();
            
            var oldTyreLogs = new List<TyreLog>();
            var newReceiptLogs = new List<TyreLog>();
            var newTyrePerformance = new List<TyreLifePerformanceLog>();
            foreach (var l in view.ReceiptLogs)
            {
                
                /************************************************************
                *************||Tyre Receipt Logics Start||*******************
                *************************************************************/
                #region Tyre Receipt Logic
                var r = new TyreLog();
                var rr = receiptReferenceLogs.Find(x => x.Id == l.ReceiptReferenceId);

                if (rr == null || rr.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.ReceiptTyreNo}");
                }
                if (rr.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.ReceiptTyreNo}");
                }
                if (l.ReceiptLogId > 0)
                {
                    r = existingTyreLogs.Find(x => x.Id == l.ReceiptLogId);
                    if (r.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.ReceiptTyreNo}[Referenced Transaction No :{r.fk_NextLog.VoucherNo}]");
                    }
                }
                if (rr != null && rr.NextLogId > 0 && r.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Invalid Reference for Tyre No {rr.TyreNo}.");
                }
                //Check if Tyre No has been altered
                if (r.Id > 0 && r.PreviousLogId != rr.Id)
                {
                    //Restore Last Receipt Log Id when Receipt Tyre No Changed
                    var td = receiptTyrePerformance.FirstOrDefault(x => x.LastReceiptLogId == r.Id);
                    if (td != default(TyreLifePerformanceLog))
                    {
                        var lastReceipt = _repository.GetLastTyreLogByStatusAndLife(r.TyreId, 35, r.TyreLife, r.Id);
                        td.LastReceiptLogId = lastReceipt?.Id;
                        td.ObjectState = ObjectState.Modified;
                        tpiRepo.Update(td);
                    }
                    //if Tyre has been altered restore all tyre status to previous logs status
                    //oldTyreLogs.Add(RestorePreviousTyreStatus(r));
                    //r.fk_Tyre = null;
                    //r.TyreId = 0;
                    //r.fk_PreviousLog = null;
                    //r.PreviousLogId = null;
                }
                r.IsException = l.IsException;
                r.CreditAccountId = rr.DebitAccountId;
                r.IgnoreValidation = true;
                r.NetAmount = r.Rate = r.TyreTotalAmount = r.SubTotal = 0;
                r.EstScrapValue = l.ReceiptAmount;
                r.DiscountAmount = r.OtherAmount = r.DiscountPercent = 0;
                r.IsRemoulded = rr.IsRemoulded;
                r.IsStepney = rr.IsStepney;
                r.JobsheetId = l.JobSheetId;
                r.KmReading = l.ReceiptOutKm;
                if (l.KmSourceId.GetValueOrDefault(1483) == 1483)
                {
                    r.KmRun = l.ReceiptOutKm - rr.KmReading;//Calculate Difference
                }
                else
                {
                    r.KmRun = l.ReceiptKmRun;
                }
                r.MechanicId = l.MechanicId;
                r.CreditAccountId = rr.DebitAccountId;//l.OwnerId.GetValueOrDefault(0) == 0 ? rr.CreditAccountId : l.OwnerId.GetValueOrDefault(0);
                r.DebitAccountId = view.PrimaryDebitAccountId.Value;
                r.Remark = l.ReceiptRemark;
                r.TyreId = l.ReceiptTyreId;
                r.fk_Tyre = rr.fk_Tyre;
                r.ReasonId = l.ReasonId;
                r.NextUseId = l.NextUseId;
                r.TyreLife = rr.TyreLife;
                r.TyreStatusId = 1100;
                r.TyreNo = rr.fk_Tyre.TyreNo;
                r.OdoKm = l.OdoKm;
                r.GPSKm = l.GpsKm;
                r.TLKm = l.TLKm;
                r.JobCardKm = l.JobKm;
                r.KMSourceId = l.KmSourceId;
                r.VoucherTypeId = 35;
                r.VoucherDate = view.DocumentDate;
                r.VoucherNo = view.DocumentNo;
                r.VehicleId = l.VehicleId;
                r.NextUseId = l.NextUseId;
                r.fk_Tyre.S_VoucherTypeId = r.VoucherTypeId;
                r.fk_Tyre.ObjectState = ObjectState.Modified;
                r.fk_Tyre.S_VoucherDate = r.VoucherDate;
                r.fk_Tyre.S_CreditAccountId = r.CreditAccountId;
                r.fk_Tyre.S_DebitAccountId = r.DebitAccountId;
                r.fk_Tyre.S_Life = rr.TyreLife;
                r.fk_Tyre.S_StatusId = r.TyreStatusId;
                r.PreviousLogId = rr.Id;
                r.fk_PreviousLog = rr;
                rr.NextLogId = r.Id;
                rr.fk_NextLog = r;

                #region TyreCheck Receipt //added by sanjay
                if (r.fk_TyreCheck == null || r.fk_TyreCheck.Id == 0)
                {
                    r.fk_TyreCheck = new TyreCheck();
                }
                r.fk_TyreCheck.AirPressure = l.AirPressure;
                r.fk_TyreCheck.CheckDate = view.DocumentDate;
                r.fk_TyreCheck.KmRun = r.KmRun;
//                r.fk_TyreCheck.Remarks = null;
                r.fk_TyreCheck.TreadDepth = l.NSD1;
                r.fk_TyreCheck.TreadDepth2 = l.NSD2;
                r.fk_TyreCheck.TreadDepth3 = l.NSD3;

                r.fk_TyreCheck.WheelPositionId = l.WheelPositionId;
                r.fk_TyreCheck.TyreId = r.TyreId;
                r.fk_TyreCheck.VehicleId = r.VehicleId.Value;
                r.fk_TyreCheck.fk_Tyre = r.fk_Tyre;
                r.fk_TyreCheck.ObjectState = r.fk_TyreCheck.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                #endregion
                if (r.Id > 0)
                {
                    r.ObjectState = ObjectState.Modified;
                    r.fk_Tyre.S_TyreLogId = r.Id;

                }
                else
                {
                    r.ObjectState = ObjectState.Added;
                    r.fk_Tyre.fk_S_TyreLog = r;
                    r.fk_Tyre.S_TyreLogId = r.Id;
                }
                newReceiptLogs.Add(r);
                #endregion

            }
            var tyreRepo = uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {


                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }
            
            tei = tei ?? new TyreLogExtraInfo();
            tei.OfficeId = view.OfficeId;
            tei.CalVat = view.CalVat;
            tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = null;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = 35;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            
            foreach (var log in newReceiptLogs)
            {
                //Extract Tyre Performance for current record in loop and set LastReceiptLog as this
                var tpd = receiptTpData.FirstOrDefault(x => x.Life == log.TyreLife && log.TyreId == x.TyreId);
                if (tpd != null && ((tpd.LastReceiptLogId.HasValue && tpd.LastReceiptLogId < log.Id) || log.Id == 0))
                {
                    tpd.LastReceiptLogId = log.Id;
                    tpd.fk_LastReceiptLog = log;
                    tpd.ObjectState = ObjectState.Modified;
                    tpd.TyreLifeMileage += log.KmRun;//Update Life Millage
                    newTyrePerformance.Add(tpd);

                }
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                log.ObjectState = log.Id > 0 ? ObjectState.Modified : ObjectState.Added; ;
                _repository.Update(log.fk_PreviousLog);

                //added by sanjay
                if (log.fk_TyreCheck.Id > 0)
                {
                    tyreCheckRepo.Update(log.fk_TyreCheck);
                }
                else
                {
                    tyreCheckRepo.Insert(log.fk_TyreCheck);
                }

                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            uom.SaveChanges();
            if (view.Id > 0)
            {
                var newLogsIds = newReceiptLogs.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    var td = receiptTyrePerformance.FirstOrDefault(x => x.LastReceiptLogId == log.Id || x.FirstIssueLogId == log.Id);
                    if (td != default(TyreLifePerformanceLog))
                    {
                        if (td.FirstIssueLogId == log.Id)
                        {
                            td.ObjectState = ObjectState.Deleted;
                            tpiRepo.Delete(td);
                        }
                        else
                        {
                            if (td.LastReceiptLogId > 0 && log.Id != td.LastReceiptLogId)
                            {
                                throw new BusinessException(ErrorCode.GLB106, "Cannot Delete Tyre Performance Log as it is Locked");
                            }
                            //Find Last Log Other than current in loop
                            TyreLog lstTl = _repository.GetLastTyreLogByStatusAndLife(log.TyreId, (td.FirstIssueLogId == log.Id ? 27 : 34), log.TyreLife, log.Id);
                            td.LastReceiptLogId = lstTl?.Id;
                            td.ObjectState = ObjectState.Modified;
                            tpiRepo.Update(td);
                        }

                    }
                    //added by sanjay
                    if (log.fk_TyreCheck != null && log.fk_TyreCheck.Id > 0)
                    {
                        log.fk_TyreCheck.ObjectState = ObjectState.Deleted;
                        tyreCheckRepo.Delete(log.fk_TyreCheck);
                    }

                    log.IssueReceiptId = null;
                    log.fk_IssueReceipt = null;
                    _repository.Update(RestorePreviousTyreStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }
            }
            uom.SaveChanges();
                        var vehids =
                newReceiptLogs.Select(x => x.VehicleId)
                    .ToList();
            vehids.AddRange(newReceiptLogs.Select(x => x.VehicleId));
            vehids = vehids.Distinct().ToList();
            //var modelcount=
            //_repository.Queryable().Count(x=>x.VehicleId==)
            foreach (var log in newTyrePerformance)
            {
                //log.TyreId = log.fk_FirstIssueLog.TyreId;
                //log.fk_Tyre = log.fk_FirstIssueLog.fk_Tyre;
                //BugFix:WorkItem 60 point No 4
                if (log.Id > 0) tpiRepo.Update(log);
                else tpiRepo.Insert(log);
            }
            uom.SaveChanges();
            return tei;
        }
        public TyreLogExtraInfo InsertUpdateIssue(vwTyreBillView view, IUnitOfWorkAsync uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.IssueLogs.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Issue Log Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account {view.PrimaryCreditAccountId} has Invalid Value.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account {view.PrimaryDebitAccountId} has Invalid Value.");
            }
            Voucher v = null;

            #region Check Circuler Reference for Tyres
            //var duplicatecheckgroup = view.IssueReceiptLogs.GroupBy(x => new { x.IssueTyreId, x.ReceiptTyreId });
            
            var groupbyvehicle = view.IssueLogs.GroupBy(x => x.VehicleId).ToList();
            if (groupbyvehicle.Select(grouping => grouping.Select(x => x.IssueTyreId).ToList()).Any(issuelist => issuelist.GroupBy(x => x).Any(x => x.Count() > 1)))
            {
                throw new BusinessException(ErrorCode.GLB106, "Same tyre can't be issued more than one in Single Transaction.");
            }
            long PricipalOwnerId = 0;
            long VehicleOwnerId = 0;

            #endregion
            var vRepo = uom.RepositoryAsync<Voucher>();
            var teiRepo = uom.RepositoryAsync<TyreLogExtraInfo>();
            var tpiRepo = uom.RepositoryAsync<TyreLifePerformanceLog>();
            
            var tyreCheckRepo = uom.RepositoryAsync<TyreCheck>();
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            if (view.Id > 0)
            {//Try to find existing tyre extra info record
                tei = teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == 34);
            }
            if (view.Id > 0 && tei != null && vRepo.Queryable().Any(x => x.Id == tei.VoucherId && x.VoucherTypeId == 34))
            {
                //Try to find existing voucher record
                v = vRepo.Queryable().Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).FirstOrDefault(x => x.Id == tei.VoucherId && x.VoucherTypeId == 34);
            }
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (tei != null && tei.Id > 0)
            {
                //In-case updating existing record find all existing attached Tyre Logs
                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_TyreCheck).Include(x => x.fk_PreviousLog).Include(x => x.fk_NextLog).Where(x => (x.ExtraInfoId == tei.Id) && x.VoucherTypeId == 34).ToList();
            }

            //Extract Ids of Primary Key
            var oldissueids = existingTyreLogs.Where(x => x.VoucherTypeId == 34 && x.Id > 0).Select(x => x.Id).ToList();
           
            var issuerefids = view.IssueLogs.Select(x => x.IssueReferenceId).ToList();

            //Fatch Tyre Performance Logs in case updating record
            var issueTyrePerformance = tpiRepo.Queryable().Where(x => oldissueids.Contains(x.FirstIssueLogId.Value)).ToList();
            
            List<TyreLog> issueReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre.fk_PurchaseTyreLog.ExtraInfo).Where(x => issuerefids.Contains(x.Id)).ToList();
            
            /*var newtyrevtypes = new long[] { 66,27, 29, 32,79, 41, 28 ,121,135,19 };*/
            var newTyreStatus = new long[] { 1099, 1203 };//New & Old Batteries can be issued
            decimal issueNetamount = 0;
            //try
            //{
            //    issueNetamount = Math.Round(issueReferenceLogs.Where(x => newtyrevtypes.Contains(x.VoucherTypeId)).Sum(x => x.SubTotal * x.fk_Tyre.fk_PurchaseTyreLog.ExtraInfo.CurRate), 2);
            //}
            //catch { }

            var cv = issueReferenceLogs.Any(x => newTyreStatus.Contains(x.TyreStatusId) && x.NetAmount > 0) && view.PrimaryDebitAmount != 0;
            if (cv)
            {
                v = v ?? new Voucher();
            }
            else
            {
                if (v != null && v.Id > 0)
                {
                    v.ObjectState = ObjectState.Deleted;
                    foreach (var x in v.VoucherDetails)
                    {
                        x.ObjectState = ObjectState.Deleted;
                        foreach (var y in x.VoucherDetailReferences) y.ObjectState = ObjectState.Deleted;
                    }
                    vRepo.Delete(v);
                }
            }
            if (view.VoucherTypeId == 34)
            {
                try
                {
                    var PricipalOwnerIdQuery =
                            _repository.GetRepository<ApiConfiguration>()
                                .Queryable()
                                .Where(x => x.Key == "PricipalOwnerId")
                                .Select(x => x.Value)
                                .FromCacheFirstOrDefault();
                    long.TryParse(PricipalOwnerIdQuery, out PricipalOwnerId);
                }
                catch { }

                if (PricipalOwnerId > 0)
                {

                    var vehlist = view.IssueLogs.DistinctBy(k => k.VehicleId).Select(x => x.VehicleId).ToList();
                    if (vehlist != null && vehlist.Count > 0)
                    {
                        var vdata =
                                _repository.GetRepository<VehicleMaster>()
                                    .Queryable()
                                    .Where(x => vehlist.Contains(x.Id))
                                    .Include(x => x.fk_VehicleOwner)  // Ensure related data is loaded
                                    .DistinctBy(k => new { k.OwnerPartyId, k.fk_VehicleOwner.ReferenceFlag })
                                    .Select(x => new { x.OwnerPartyId, x.fk_VehicleOwner.ReferenceFlag })
                                    .ToList();

                        if (vdata?.Count() > 1)
                        {
                            throw new BusinessException(ErrorCode.GLB106, "Only one vehicle is permitted in the case of an non-company vehicle(s)");
                        }
                        VehicleOwnerId = vdata.FirstOrDefault().OwnerPartyId ?? 0;
                        if (PricipalOwnerId != VehicleOwnerId && VehicleOwnerId > 0)
                        {
                            if (!vdata.FirstOrDefault().ReferenceFlag)
                            {
                                throw new BusinessException(ErrorCode.GLB106, "Bill by Bill Flag should be ON for Vehicle Owner");
                            }
                            else
                            {
                                view.PrimaryDebitAccountId = VehicleOwnerId;
                            }
                        }
                    }
                }
            }
            var oldTyreLogs = new List<TyreLog>();
            var newIssuedLogs = new List<TyreLog>();
            var newTyrePerformance = new List<TyreLifePerformanceLog>();
            foreach (var l in view.IssueLogs)
            {
                /************************************************************
                *************||Tyre Issue Logics Start||*********************
                *************************************************************/
                #region Tyre Issue Logic
                issueNetamount += l.IssueAmount;

                var i = new TyreLog(); //Issued Log
                i.IsException = l.IsException;
                var ir = issueReferenceLogs.Find(x => x.Id == l.IssueReferenceId);//Issue Reference Log

                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log didn't found for Tyre No {l.IssueTyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.IssueTyreNo}");
                }
                if (l.IssueLogId > 0)
                {
                    i = existingTyreLogs.Find(x => x.Id == l.IssueLogId);//Issued Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.IssueTyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //var tp = issueTyrePerformance.FirstOrDefault(x => x.FirstIssueLogId == i.Id);
                    //tp.ObjectState=ObjectState.Deleted;
                    //tpiRepo.Delete(tp);
                    //if Tyre has been altered restore all tyre status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                }
                i.TSLId = l.TSLId;
                i.IssueReceiptId = l.ReceiptLogId;
                i.IgnoreValidation = true;
                i.NetAmount = i.Rate = i.TyreTotalAmount = i.SubTotal = l.IssueAmount;
                i.DiscountAmount = i.OtherAmount = i.DiscountPercent = 0;
                i.IsStepney = l.IsStepney;
                i.JobsheetId = l.JobSheetId;
                i.KmReading = l.IssueOnKM;
                i.KmRun = 0;
                i.MechanicId = l.MechanicId;
                i.CreditAccountId = view.PrimaryCreditAccountId; //view.PrimaryDebitAccountId.GetValueOrDefault(0);
                i.DebitAccountId = view.PrimaryDebitAccountId.GetValueOrDefault(0);
                i.Remark = l.IssueRemark;
                i.TyreId = ir.TyreId;
                i.fk_Tyre = ir.fk_Tyre;
                i.TyreLife = ir.TyreLife;
                i.TyreStatusId = 1103;//OnVehicle
                i.TyreNo = i.fk_Tyre.TyreNo;
                i.VoucherTypeId = 34;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.VehicleId = l.VehicleId;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = ir.TyreLife;
                i.fk_Tyre.S_StatusId = 1103;//OnVehicle
                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                //if (!string.IsNullOrWhiteSpace(l.IssueRowVersionId))
                //{
                //    i.RowVersion = Encoding.UTF8.GetBytes(l.IssueRowVersionId);
                //}
                #region TyreCheck Issue
                if (i.fk_TyreCheck == null || i.fk_TyreCheck.Id == 0)
                {
                    i.fk_TyreCheck = new TyreCheck();
                }
                i.fk_TyreCheck.AirPressure = l.IssuePSI;
                i.fk_TyreCheck.CheckDate = view.DocumentDate;
                i.fk_TyreCheck.KmRun = 0;
               // i.fk_TyreCheck.Remarks = l.IssueRemark;

                i.fk_TyreCheck.TreadDepth = l.NSD1;
                i.fk_TyreCheck.TreadDepth2 = l.NSD2;
                i.fk_TyreCheck.TreadDepth3 = l.NSD3;
                i.fk_TyreCheck.TreadDepth4 = l.NSD4;


                i.fk_TyreCheck.TyreId = i.TyreId;
                i.fk_TyreCheck.VehicleId = i.VehicleId.Value;
                i.fk_TyreCheck.WheelPositionId = l.WheelPositionId;
                i.fk_TyreCheck.fk_Tyre = i.fk_Tyre;
                i.fk_TyreCheck.ObjectState = i.fk_TyreCheck.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                #endregion
                if (i.Id > 0)
                {

                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;

                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newIssuedLogs.Add(i);
                #endregion
            }
            var tyreRepo = uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {
                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }
            if (cv)
            {

                #region Prepare Issue Voucher
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
                v.ConstCurTypeId = view.ConstCurTypeId;

                v.VoucherDate = view.DocumentDate;
                v.VoucherDateTime = view.DocumentDate;
                v.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault(0);
                v.VoucherNo = view.DocumentNo;
                v.Amount1 = view.PrimaryDebitAmount;
                v.Account1Id = view.PrimaryDebitAccountId;
                v.Account2Id = view.PrimaryCreditAccountId;
                v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
                v.Account3Id = view.CGSTLedgerId;
                v.Amount3 = view.CGSTAmount;
                v.Account4Id = view.OtherLedgerId;
                v.Amount4 = view.OtherAmount;
                v.Account5Id = view.SGSTLedgerId;
                v.Amount5 = view.SGSTAmount;
                v.Account6Id = view.IGSTLedgerId;
                v.Amount6 = view.IGSTAmount;
                v.UserRemark = view.Narration;
                v.OfficeId = view.OfficeId;
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //TODO:Setup Account Narration from Template located with VoucherType
                v.AccountingRemark = "";

                if (v.Amount1 != issueNetamount)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre Total Net Value {issueNetamount} Does't match Voucher Primary Debit Amount {v.Amount1}");
                }
                if (v.Amount2 != -issueNetamount)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Bill Total Amount {-issueNetamount} Does't match Voucher Primary Credit Amount {v.Amount2}");
                }
                PrepareVoucherDetails(_repository, v);
                #endregion
                #region Validations
                var vdrrequired =
                    _repository.GetRepository<VoucherType>()
                        .Queryable()
                        .Where(x => x.Id == 34)
                        .Select(x => new
                        {
                            x.VDRRequired,
                            x.VDRequired
                        })
                        .FirstOrDefault();
                if (vdrrequired != null)
                {
                    if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                    {
                        throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                    }

                    if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
                    {
                        throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
                    }
                }
                #endregion
            }
            tei = tei ?? new TyreLogExtraInfo();
            if (cv) tei.fk_Voucher = v;

            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.OfficeId = view.OfficeId;
            tei.CalVat = view.CalVat;
            tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            if (cv) tei.VoucherId = v.Id;
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = 34;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            var mlids = newIssuedLogs.Select(x => $"{(x.TyreLife - 1)}-{x.TyreId}").ToList();
            var mileageList = tpiRepo.Queryable().Where(x => mlids.Contains((x.Life + "-" + x.TyreId))).Select(x => new { Mileage = x.TyrePreviousMileage + x.TyreLifeMileage, x.TyreId }).ToList();
            foreach (var log in newIssuedLogs)
            {
                if (cv)
                {
                    log.VoucherId = v.Id;
                    log.fk_Voucher = v;
                    log.fk_Tyre.S_VoucherId = v.Id;
                    log.fk_Tyre.S_VoucherDate = v.VoucherDate;
                    log.fk_Tyre.fk_S_Voucher = v;

                }
                else
                {//If Voucher is not applicable set voucher values as null
                    log.VoucherId = null;
                    log.fk_Voucher = null;
                    log.fk_Tyre.S_VoucherId = null;
                    log.fk_Tyre.S_VoucherDate = view.DocumentDate;
                    log.fk_Tyre.fk_S_Voucher = null;
                }
                //Only Create Tyre Performance in case Tyre is issued first time
                if (log.fk_PreviousLog.TyreStatusId == 1099)
                {
                    var tpi = issueTyrePerformance.FirstOrDefault(x => x.FirstIssueLogId == log.Id) ?? new TyreLifePerformanceLog();
                    if (tpi.Id == 0) tpi.FirstIssueLogId = log.Id;
                    tpi.CurrentMileage = 0;
                    tpi.Life = log.TyreLife;
                    tpi.TyreLifeMileage = 0;
                    tpi.LifeStartDate = log.VoucherDate;
                    var mileage = mileageList.FirstOrDefault(x => x.TyreId == log.TyreId);
                    tpi.TyrePreviousMileage = mileage?.Mileage ?? 0;
                    tpi.PurchaseAmount = log.NetAmount;
                    tpi.SupplierId = log.DebitAccountId;
                    tpi.LifeEndDate = null;
                    tpi.fk_FirstIssueLog = log;
                    tpi.ObjectState = tpi.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    newTyrePerformance.Add(tpi);
                }
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                log.ObjectState = log.Id > 0 ? ObjectState.Modified : ObjectState.Added; ;
                _repository.Update(log.fk_PreviousLog);
                if (log.fk_TyreCheck.Id > 0)
                {
                    tyreCheckRepo.Update(log.fk_TyreCheck);
                }
                else
                {
                    tyreCheckRepo.Insert(log.fk_TyreCheck);
                }
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            uom.SaveChanges();
            if (view.Id > 0)
            {
                var newLogsIds = newIssuedLogs.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                newLogsIds = newLogsIds.Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    //if (log.NextLogId!=null&&deletedIds.Contains(log.NextLogId))
                    //{
                    //    log.NextLogId = null;
                    //    log.fk_NextLog = null;

                    //}//|| deletedIds.Contains(log.PreviousLogId))
                    //if (log.PreviousLogId != null && deletedIds.Contains(log.PreviousLogId))
                    //{
                    //    log.PreviousLogId = null;
                    //    log.fk_PreviousLog = null;
                    //}
                    if (log.fk_TyreCheck != null && log.fk_TyreCheck.Id > 0)
                    {
                        log.fk_TyreCheck.ObjectState = ObjectState.Deleted;
                        tyreCheckRepo.Delete(log.fk_TyreCheck);
                    }
                    log.IssueReceiptId = null;
                    log.fk_IssueReceipt = null;
                    _repository.Update(RestorePreviousTyreStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }
            }
            uom.SaveChanges();
            var vehids =
                newIssuedLogs.Select(x => x.VehicleId)
                    .ToList();
            vehids = vehids.Distinct().ToList();
            //var modelcount=
            //_repository.Queryable().Count(x=>x.VehicleId==)
            foreach (var log in newTyrePerformance)
            {
                log.TyreId = log.fk_FirstIssueLog.TyreId;
                log.fk_Tyre = log.fk_FirstIssueLog.fk_Tyre;
                if (log.Id > 0) tpiRepo.Update(log);
                else tpiRepo.Insert(log);
            }
            

            try
            {
                var tpt = _repository.GetRepository<TPTRequestPool>();

                if (view.Id == 0 && PricipalOwnerId > 0 && VehicleOwnerId > 0 && (PricipalOwnerId != VehicleOwnerId))
                {
                    TPTRequestPool tpr = new TPTRequestPool();
                    tpr.ObjectState = ObjectState.Added;
                    tpr.RequestId = Guid.NewGuid().ToString();
                    tpr.ViewId = tei.ViewId.GetValueOrDefault();
                    tpr.RecordId = tei.Id;
                    tpr.DocNo = tei.VoucherNo;
                    tpr.BatchId = tpr.RequestId;
                    tpr.IsProceeded = false;
                    tpr.CreatedTime = DateTime.Now;

                    tpr.TypeKey = "ZRA_TYR_ISSUE_SALE";
                    tpt.Insert(tpr);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
            uom.SaveChanges();
            return tei;
        }

        public async Task RestorePreviousTyreStatusAsync(TyreLog current)
        {
            var p = current.fk_PreviousLog;
            if (current.VoucherTypeId != 42 && current.VoucherTypeId != 135 && current.VoucherTypeId != 27 && current.VoucherTypeId!=66&&!(current.TyreStatusId == 1099 && current.VoucherTypeId == 32))
            {
                if (p == null)
                {
                    await _repository.Queryable().Include(x => x.fk_Tyre).Where(x => x.Id == current.PreviousLogId).LoadAsync();
                    p = current.fk_PreviousLog;
                }
                if (p == null)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Unable to Restore Tyre Status as previous log not found");
                }
                p.NextLogId = null;
                p.fk_NextLog = null;
                p.ScrapCost = 0;//added by sanjay

                p.fk_Tyre.S_StatusId = p.TyreStatusId;
                p.fk_Tyre.S_DebitAccountId = p.DebitAccountId;
                p.fk_Tyre.S_CreditAccountId = p.CreditAccountId;
                p.fk_Tyre.S_TyreLogId = p.Id;
                p.fk_Tyre.S_VoucherDate = p.VoucherDate;
                p.fk_Tyre.S_VoucherId = p.VoucherId;
                p.fk_Tyre.S_Life = p.TyreLife;
                p.fk_Tyre.S_VoucherTypeId = p.VoucherTypeId;

                p.fk_Tyre.fk_S_TyreLog = null;
                p.fk_Tyre.fk_S_OtherAccount = null;
                p.fk_Tyre.fk_S_Status = null;
                p.fk_Tyre.fk_S_DebitAccount = null;
                p.fk_Tyre.fk_S_Voucher = null;
                p.fk_Tyre.fk_S_VoucherType = null;
                p.ObjectState = ObjectState.Modified;
                p.fk_Tyre.ObjectState = ObjectState.Modified;
            }
            current.PreviousLogId = null;
            current.fk_PreviousLog = null;
            if (current.ObjectState != ObjectState.Deleted)
            {
                current.ObjectState=ObjectState.Modified;
            }
        }
        private TyreLog RestorePreviousTyreStatus(TyreLog current)
        {
            //if Tyre has been altered restore all tyre status to previous logs status
            if (current.VoucherTypeId == 42 || current.VoucherTypeId == 135 || current.VoucherTypeId== 27 || current.VoucherTypeId==66) return current;//lokesh null
            var p = current.fk_PreviousLog;
            if (p == null )
            {
                _repository.Queryable().Include(x => x.fk_Tyre).Where(x => x.Id == current.PreviousLogId).Load();
                p = current.fk_PreviousLog;
                if (p == null)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Unable to Restore Tyre Status as previous log not found");
                }
            }
            p.NextLogId = null;
            p.fk_NextLog = null;
            p.ScrapCost = 0;//added by sanjay

            p.fk_Tyre.S_StatusId = p.TyreStatusId;
            p.fk_Tyre.S_DebitAccountId = p.DebitAccountId;
            p.fk_Tyre.S_CreditAccountId = p.CreditAccountId;
            p.fk_Tyre.S_TyreLogId = p.Id;
            p.fk_Tyre.S_VoucherDate = p.VoucherDate;
            p.fk_Tyre.S_VoucherId = p.VoucherId;
            p.fk_Tyre.S_Life = p.TyreLife;
            p.fk_Tyre.S_VoucherTypeId = p.VoucherTypeId;

            p.fk_Tyre.fk_S_TyreLog = null;
            p.fk_Tyre.fk_S_OtherAccount = null;
            p.fk_Tyre.fk_S_Status = null;
            p.fk_Tyre.fk_S_DebitAccount = null;
            p.fk_Tyre.fk_S_Voucher = null;
            p.fk_Tyre.fk_S_VoucherType = null;

            current.PreviousLogId = null;
            current.fk_PreviousLog = null;

            p.ObjectState = ObjectState.Modified;
            p.fk_Tyre.ObjectState = ObjectState.Modified;
            return p;
        }

        public async Task<TyreLogExtraInfo> InsertUpdateChasisTyreBillAsync(vwTyreChassisBill view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.TyreLogs.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            //view.PrimaryDebitAccountId consider as VehicleId
            if (string.IsNullOrWhiteSpace(view.DocumentNumber))
            {
                throw new BusinessException(ErrorCode.GLB106, "Document Number is Required");
            }
            var tpiRepo = _uom.RepositoryAsync<TyreLifePerformanceLog>();
            var tyreCheckRepo = _uom.RepositoryAsync<TyreCheck>();
            var tyreIds = view.TyreLogs.Select(x => x.TyreId).ToArray();
            var deletedtyrechecks = view.Id > 0 ? await _repository.Queryable().Where(x => x.ExtraInfoId == view.Id && x.VoucherTypeId == 42 && x.TyreCheckId > 0&& !tyreIds.Contains(x.TyreId)).Select(x => x.TyreCheckId).ToListAsync() : new List<long?>();
            if (deletedtyrechecks.Any())
            {
                await _uom.ExecSqlQueryAsync($"update dbo.tTyreLog  SET TyreCheckId=NULL where TyreCheckId IS NOT NULL AND TyreCheckId in({deletedtyrechecks.JoinStrings(",")})");
                
                await _uom.ExecSqlQueryAsync($"DELETE FROM dbo.tTyreCheck WHERE Id in({deletedtyrechecks.JoinStrings(",")})");
            }
            var oldTyreLogList = view.Id>0? await _repository.Queryable().Include(x => x.fk_Tyre).Where(x => x.ExtraInfoId == view.Id && x.VoucherTypeId == 42).ToListAsync():new List<TyreLog>(); 
            var oldtpiIdlist = oldTyreLogList.Select(x => x.Id).ToList();
            var oldTyrePerformance = await tpiRepo.Queryable().Where(x => oldtpiIdlist.Contains(x.FirstIssueLogId.Value)).ToListAsync();
            //var newlogList=new List<TyreLog>();
            var newTyrePerformance = new List<TyreLifePerformanceLog>();
            var newissuelogs = new List<TyreLog>();
            foreach (var log in view.TyreLogs)
            {
                var i = log.Id > 0 ? oldTyreLogList.FirstOrDefault(x => x.Id == log.Id) : new TyreLog();
                if (i == null)
                {
                    throw new BusinessException(ErrorCode.GLB106, "One of Tyre Log Transaction didn't found for update");
                }
                var tpi = oldTyrePerformance.FirstOrDefault(x => x.FirstIssueLogId == i.Id) ?? new TyreLifePerformanceLog();
                i.IsException = log.IsException;
                if (i.NextLogId > 0)
                {
                    i.ObjectState = ObjectState.Unchanged;
                    newissuelogs.Add(i);
                    if (tpi == null)
                    {
                        if (tpi.Id == 0) tpi.FirstIssueLogId = i.Id;
                        tpi.CurrentMileage = 0;
                        tpi.Life = 0;
                        tpi.TyreLifeMileage = 0;
                        tpi.LifeStartDate = log.LogDate ?? view.IssueDate;
                        tpi.TyrePreviousMileage = 0;
                        tpi.PurchaseAmount = i.NetAmount;
                        tpi.SupplierId = i.DebitAccountId;
                        tpi.LifeEndDate = null;
                        tpi.fk_FirstIssueLog = i;
                    }
                    tpi.ObjectState = tpi.Id > 0 ? ObjectState.Unchanged : ObjectState.Added;
                    newTyrePerformance.Add(tpi);
                    continue;
                    //if (i.TyreNo != log.TyreNo || i.TyreId != log.TyreId)
                    //{
                    //    throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Tyre No:{log.TyreNo}");
                    //}
                }

                var duplicatetyres =
                    _repository.Queryable().FirstOrDefault(x => x.TyreNo == log.TyreNo && x.TyreId != log.TyreId && (x.VoucherTypeId == 135 || x.VoucherTypeId == 27 || x.VoucherTypeId == 32 || x.VoucherTypeId == 79 || x.VoucherTypeId == 42));
                if (duplicatetyres != null)
                {
                    throw new BusinessException(ErrorCode.GLB105, $"Tyre No [  { log.TyreNo }  ] already Exists");
                }

                i.CreditAccountId = i.DebitAccountId = view.StoreId;
                if (log.OwnerId.HasValue && log.OwnerId > 0)
                {
                    i.DebitAccountId = log.OwnerId.Value;
                }
                i.ObjectState = i.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                #region Tyre Issue Code
                i.TSLId = log.TSLId;
                i.VehicleId = log.VehicleId;
                i.VoucherTypeId = 42;
                i.TyreLife = 0;
                i.TyreNo = log.TyreNo;
                i.KmReading = log.KmReading;
                i.KmRun = 0;
                i.TyreStatusId = 1103;
                i.Remark = log.Remark;
                i.AirPressure = log.AirPressure;
                i.DiscountAmount = i.DiscountPercent = i.CGSTAmount = i.CGSTPercent = i.SGSTAmount = i.SGSTPercent = i.IGSTAmount = i.IGSTPercent = i.OtherAmount = i.ScrapCost = i.TransferPrice = 0;
                i.Rate = i.SubTotal = i.NetAmount=i.TyreTotalAmount = log.NetAmount;
                i.IsStepney = log.IsStepney;
                i.VoucherDate = log.LogDate ?? view.IssueDate;
                i.VoucherNo = view.DocumentNumber;
                //if (i.Id == 0) t.fk_NextLog = i;

                if (tpi.Id == 0) tpi.FirstIssueLogId = i.Id;
                tpi.CurrentMileage = 0;
                tpi.Life = 0;
                tpi.TyreLifeMileage = 0;
                tpi.LifeStartDate = log.LogDate ?? view.IssueDate;
                tpi.TyrePreviousMileage = 0;
                tpi.PurchaseAmount = i.NetAmount;
                tpi.SupplierId = i.DebitAccountId;
                tpi.LifeEndDate = null;
                tpi.fk_FirstIssueLog = i;
                tpi.ObjectState = tpi.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                newTyrePerformance.Add(tpi);
                #endregion

                if (i.Id == 0)
                {
                    var tyre = new TyreMaster()
                    {
                        BrandId = log.BrandId.GetValueOrDefault(0),
                        IsAnalysis = true,
                        ObjectState = ObjectState.Added,
                        OpeningKm = log.OpeningKM,
                        OpeningMonth = log.OpeningMonth,
                        ProdMonth = log.ProductionMonth.GetValueOrDefault(),
                        S_Life = 0,
                        S_CreditAccountId = i.CreditAccountId,
                        S_StatusId = 1103,
                        S_DebitAccountId = i.DebitAccountId,
                        S_VoucherDate = log.LogDate ?? view.IssueDate,
                        TyreNo = i.TyreNo,
                        S_VoucherTypeId = 42,
                    };
                    i.fk_Tyre = tyre;
                }
                else
                {
                    i.fk_Tyre.BrandId = log.BrandId.GetValueOrDefault(0);
                    i.fk_Tyre.IsAnalysis = true;
                    i.fk_Tyre.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.OpeningKm = log.OpeningKM;
                    i.fk_Tyre.OpeningMonth = log.OpeningKM;
                    i.fk_Tyre.ProdMonth = log.ProductionMonth.GetValueOrDefault();
                    i.fk_Tyre.S_Life = 0;
                    i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                    i.fk_Tyre.S_StatusId = 1103;
                    i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                    i.fk_Tyre.S_VoucherDate = log.LogDate ?? view.IssueDate;
                    i.fk_Tyre.TyreNo = i.TyreNo;
                    i.fk_Tyre.S_VoucherTypeId = 42;
                }
                //if (t.NextLogId == 0) t.fk_NextLog.fk_Tyre = t.fk_Tyre;
                newissuelogs.Add(i);
                #region TyreCheck Issue
                if (i.fk_TyreCheck == null || i.fk_TyreCheck.Id == 0)
                {
                    i.fk_TyreCheck = new TyreCheck();
                }
                i.fk_TyreCheck.AirPressure = log.AirPressure;
                i.fk_TyreCheck.CheckDate = log.LogDate ?? view.IssueDate;
                i.fk_TyreCheck.KmRun = 0;
                i.fk_TyreCheck.Remarks = log.Remark;
                i.fk_TyreCheck.TreadDepth = log.NSD;
                i.fk_TyreCheck.TyreId = i.TyreId;
                i.fk_TyreCheck.VehicleId = i.VehicleId.Value;
                i.fk_TyreCheck.WheelPositionId = log.WheelPositionId.GetValueOrDefault(0)==0?(long?)null: log.WheelPositionId;
                i.fk_TyreCheck.fk_Tyre = i.fk_Tyre;
                i.fk_TyreCheck.ObjectState = i.fk_TyreCheck.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                #endregion
            }

            var newLogsIds = newissuelogs.Where(x => x.Id > 0).Select(x => x.Id).ToList();
            var deletedLogs = oldTyreLogList.Where(x => !newLogsIds.Contains(x.Id)).ToList();
            
            if (deletedLogs.Any())
            {
                if (deletedLogs.Any(x => x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.NextLogId > 0).Select(x => x.TyreNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                await _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).LoadAsync();

                foreach (var log in deletedLogs)
                {
                    
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                    
                }
            }
            var newperfLogsIds = newTyrePerformance.Where(x => x.Id > 0).Select(x => x.Id).ToList();
            var deletedperfLogs = oldTyrePerformance.Where(x => !newperfLogsIds.Contains(x.Id)).ToList();
            if (deletedperfLogs.Any())
            {
                foreach (var log in deletedperfLogs)
                {
                    log.ObjectState = ObjectState.Deleted;                    
                    tpiRepo.Delete(log);
                }
            }
            var tyreRepo = _uom.Repository<TyreMaster>();
            foreach (var log in newissuelogs)
            {
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    //_repository.Update(log.fk_NextLog);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    //_repository.Insert(log.fk_NextLog);
                    tyreRepo.Insert(log.fk_Tyre);
                }
                if (log.fk_TyreCheck != null)
                {
                    if (log.fk_TyreCheck.Id > 0)
                    {
                        tyreCheckRepo.Update(log.fk_TyreCheck);
                    }
                    else
                    {
                        tyreCheckRepo.Insert(log.fk_TyreCheck);
                    }
                }
            }
            _uom.SaveChanges();
            
            foreach (var log in newTyrePerformance)
            {
                log.TyreId = log.fk_FirstIssueLog.TyreId;
                log.fk_Tyre = log.fk_FirstIssueLog.fk_Tyre;
                if (log.Id > 0) tpiRepo.Update(log);
                else tpiRepo.Insert(log);
            }
            _uom.SaveChanges();
            long? _vehicleid= view.TyreLogs.Select(x => x.VehicleId).FirstOrDefault();//added by sanjay
            _vehicleid = _vehicleid == 0 ? null: _vehicleid;

            var teiRepo = _uom.Repository<TyreLogExtraInfo>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id==view.Id) ?? new TyreLogExtraInfo();
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;

            tei.VoucherNo = view.DocumentNumber;
            tei.PageId = view.PageId;
            tei.VoucherTypeId = 42;
            tei.OfficeId = view.OfficeId;
            tei.VoucherDate = view.IssueDate;
            tei.CrAccountId=tei.DrAccountId = view.StoreId;
            tei.ViewId = view.ViewId;
            tei.VehicleId = _vehicleid; //added by sanjay
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var x in newissuelogs)
            {
                x.fk_Tyre.PurchaseLogId = x.Id;
                if (x.fk_Tyre.S_TyreLogId.GetValueOrDefault(x.Id) == x.Id)
                {
                    x.fk_Tyre.S_TyreLogId = x.Id;
                }                
                x.fk_Tyre.ObjectState = ObjectState.Modified;
                x.ObjectState = ObjectState.Modified;
                x.ExtraInfo = tei;
                tyreRepo.Update(x.fk_Tyre);
            }
            _uom.SaveChanges();
            return tei;
        }
        public TyreLogExtraInfo InsertOrUpdatePurchaseBillMRNSettlementView(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.Tyres.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account {view.PrimaryCreditAccountId} or Primary Credit Ammount {view.PrimaryCreditAmount} has Invalid Value.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 || view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account {view.PrimaryDebitAccountId} or Primary Debit Ammount {view.PrimaryDebitAmount} has Invalid Value.");
            }
            if (view.PostDiscountAcId.GetValueOrDefault() <= 0 && view.PostDiscountAmt != 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"PostDiscount Account {view.PostDiscountAcId} or PostDiscount Amount {view.PostDiscountAmt} has Invalid Value.");
            }
            if (view.RoundOffAccId.GetValueOrDefault() <= 0 && view.RoundOffAmt != 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"RoundOff Account {view.RoundOffAccId} or RoundOff Ammount {view.RoundOffAmt} has Invalid Value.");
            }

            long InventoryControlAcId;
            var GRNControlAcQuery =
                    _repository.GetRepository<ApiConfiguration>()
                        .Queryable()
                        .Where(x => x.Key == "InventoryControlAcId")
                        .Select(x => x.Value)
                        .FirstOrDefault();
            if (!long.TryParse(GRNControlAcQuery, out InventoryControlAcId))
            {
                throw new BusinessException(ErrorCode.GLB103,
                    "Inventory Control Account need to be configured.");
            }

            if (!_repository.GetRepository<Ledger>().Queryable().Any(x => x.Id == InventoryControlAcId && x.ReferenceFlag))
            {
                throw new BusinessException(ErrorCode.GLB103,
                    "Inventory Control Account need to be configured Bill By Bill");
            }

            /*forcily debit accountis control account*/
            view.ProvisionalAcId = view.PrimaryDebitAccountId;
            view.PrimaryDebitAccountId = InventoryControlAcId;

            var teiRepo = _repository.GetRepository<TyreLogExtraInfo>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id) ?? new TyreLogExtraInfo();
            if (tei == default(TyreLogExtraInfo) && view.Id > 0)
            {
                //Through error if voucher not found with provided vouchertypeid
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }

            var tpiRepo = _uom.RepositoryAsync<TyreLifePerformanceLog>();
            #region Tyre Log Preparation

            Voucher v = new Voucher();
            if (view.Id > 0 && view.VoucherTypeId == 137 /*Tyre MRN*/)
            {
                /*If it is existing transaction try to fatch v / vd / vdr and SpareLabour info from database*/
                v = _repository.GetRepository<Voucher>().Query(x => x.Id == tei.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (v == null) { v = new Voucher(); }
            }

            //Collect Distincs ReferenceId's from Posted SpareLogs
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                existingTyreLogs = _repository.Queryable().Where(x => x.BillExtraInfoId == view.Id).ToList();
            }
            var oldtpiIdlist = existingTyreLogs.Select(x => x.Id).ToList();
            var newLogsIds = view.Tyres.Where(x => x.Id > 0).Select(x => x.Id);
            #endregion
            if (view.Id > 0)
            {                
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();

                foreach (var log in deletedLogs)
                {
                    log.BillExtraInfoId = null;
                    log.fk_Bill = null;
                }
            }
            if (view.VoucherTypeId == 137)
            {
                #region Prepare Voucher
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
                v.ConstCurTypeId = view.ConstCurTypeId;

                v.VoucherDate = view.DocumentDate;
                v.VoucherDateTime = view.DocumentDate;
                v.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault(0);
                v.VoucherNo = view.DocumentNo;
                v.Amount1 = view.PrimaryDebitAmount;
                v.Account1Id = view.PrimaryDebitAccountId;
                v.Account2Id = view.PrimaryCreditAccountId;
                v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;

                v.Account4Id = view.OtherLedgerId;
                v.Amount4 = view.OtherAmount;

                v.Account6Id = view.IGSTLedgerId;
                v.Amount6 = view.IGSTAmount;

                v.Account8Id = view.RoundOffAccId;
                v.Amount8 = view.RoundOffAmt;
                v.Account9Id = view.PostDiscountAcId;
                v.Amount9 = -view.PostDiscountAmt;
                v.UserRemark = view.Narration;
                v.OfficeId = view.OfficeId;
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //TODO:Setup Account Narration from Template located with VoucherType
                v.AccountingRemark = "";
                
                #endregion
                
                tei.fk_Voucher = v;
                tei.VoucherId = v.Id;
            }           

            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;

            tei.CalVat = view.CalVat;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.CalOthAmt = view.CalOthAmt;
            tei.OtherAcId = view.OtherLedgerId;
            tei.OtherChgAmt = view.OtherAmount;
            tei.TaxServiceTypeId = view.TyreHSNCodeId;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.OfficeId = view.OfficeId;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.RoundOffAccId = view.RoundOffAccId;
            tei.RoundOffAmt = view.RoundOffAmt;
            tei.PostDiscountAcId = view.PostDiscountAcId;
            tei.PostDiscountAmt = view.PostDiscountAmt;

            tei.ProvisionalAcId = view.ProvisionalAcId;

            tei.TCSRate = view.TCSRate;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.OtherHSNCodeId = view.OtherHSNId;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);

            _uom.SaveChanges();
            var existinglogs = _repository.Queryable().Where(x => newLogsIds.Contains(x.Id)).ToList();
            List<VoucherDetailReference> vdrids = new List<VoucherDetailReference>();
            foreach (var x in existinglogs.GroupBy(p=>p.VoucherId))
            {
                try
                {
                    var _vdrId = _repository.GetRepository<VoucherDetailReference>().Queryable().Where(k => k.fk_VoucherDetail.VoucherId == x.Key && k.fk_VoucherDetail.AccountId == InventoryControlAcId).FirstOrDefault();
                    if (_vdrId != null)
                    {
                        _vdrId.Amount = x.Sum(y=>y.SubTotal);
                        vdrids.Add(_vdrId);
                    }
                }
                catch { }
            }

            existinglogs.ForEach(x =>
            {
                x.BillExtraInfoId = tei.Id;
                x.fk_Bill = tei;
                x.ObjectState = ObjectState.Modified;
            });

            PrepareVoucherDetails(_repository, v,vdrids);            
            _uom.SaveChanges();
            return tei;
        }

        public TyreLogExtraInfo InsertOrUpdatePurchaseBillMRNView(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.Tyres.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account {view.PrimaryCreditAccountId} or Primary Credit Ammount {view.PrimaryCreditAmount} has Invalid Value.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 || view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account {view.PrimaryDebitAccountId} or Primary Debit Ammount {view.PrimaryDebitAmount} has Invalid Value.");
            }
            long InventoryControlAcId;
            var GRNControlAcQuery =
                    _repository.GetRepository<ApiConfiguration>()
                        .Queryable()
                        .Where(x => x.Key == "InventoryControlAcId")
                        .Select(x => x.Value)
                        .FirstOrDefault();
            if (!long.TryParse(GRNControlAcQuery, out InventoryControlAcId))
            {
                throw new BusinessException(ErrorCode.GLB103,
                    "Inventory Control Account need to be configured.");
            }

            if (!_repository.GetRepository<Ledger>().Queryable().Any(x => x.Id == InventoryControlAcId && x.ReferenceFlag))
            {
                throw new BusinessException(ErrorCode.GLB103,
                    "Inventory Control Account need to be configured Bill By Bill");
            }

            if (view.VoucherTypeId == 135)
            {
                view.ProvisionalAcId = view.PrimaryCreditAccountId;
                view.PrimaryCreditAccountId = InventoryControlAcId;
            }

            var teiRepo = _repository.GetRepository<TyreLogExtraInfo>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id) ?? new TyreLogExtraInfo();
            if (tei == default(TyreLogExtraInfo) && view.Id > 0)
            {
                //Through error if voucher not found with provided vouchertypeid
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }

            
            #region Tyre Log Preparation

            Voucher v = new Voucher();
            if (view.Id > 0 && view.VoucherTypeId == 135 /*Tyre MRN*/)
            {
                /*If it is existing transaction try to fatch v / vd / vdr and SpareLabour info from database*/
                v = _repository.GetRepository<Voucher>().Query(x => x.Id == tei.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (v == null) { v = new Voucher(); }
            }

            //Collect Distincs ReferenceId's from Posted SpareLogs
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == view.Id).ToList();
            }
            var oldtpiIdlist = existingTyreLogs.Select(x => x.Id).ToList();
            var newTyreLogList = new List<TyreLog>();
            
            foreach (var l in view.Tyres)
            {
                l.TyreStatusId = 1099;

                var t = new TyreLog();
                if (view.Id > 0 && l.Id > 0)
                {
                    t = existingTyreLogs.Find(x => x.Id == l.Id);
                    if (t.NextLogId != null && t.NextLogId > 0) //&&t.fk_ChildLog.ParentLogId==t.Id)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{t.fk_NextLog.VoucherNo}]");
                    }
                }

                var duplicatetyres =
                    _repository.Queryable().FirstOrDefault(x => x.TyreNo == l.TyreNo && x.TyreId != l.TyreId && (x.VoucherTypeId == 66 || x.VoucherTypeId == 135 || x.VoucherTypeId == 27 || x.VoucherTypeId == 32 || x.VoucherTypeId == 79 || x.VoucherTypeId == 42));
                if (duplicatetyres != null)
                {
                    throw new BusinessException(ErrorCode.GLB105, $"Tyre No [  {l.TyreNo}  ] already Exists");
                }

                t.IsException = l.IsException;
                t.Rate = l.Rate;
                t.DiscountPercent = l.DiscountPercent;
                t.DiscountAmount = l.DiscountAmount;
                t.OtherAmount = l.OtherAmount;
                t.SubTotal = l.SubTotal;
                t.TubeRate = l.TubeRate;
                t.TubeDiscountPercent = l.TubeDiscountPercent;
                t.TubeDiscountAmount = l.TubeDiscountAmount;
                t.TubeOtherAmount = l.TubeOtherAmount;
                t.TubeSubTotal = l.TubeSubTotal;
                t.FlapRate = l.FlapRate;
                t.FlapDiscountPercent = l.FlapDiscountPercent;
                t.FlapDiscountAmount = l.FlapDiscountAmount;
                t.FlapOtherAmount = l.FlapOtherAmount;
                t.FlapSubTotal = l.FlapSubTotal;

                t.VehicleId = l.VehicleId;
                t.Remark = l.Remark;
                t.TyreId = l.TyreId;


                t.TubeCGSTAmount = l.TubeCGSTAmount;
                t.TubeSGSTAmount = l.TubeSGSTAmount;
                t.TubeIGSTAmount = l.TubeIGSTAmount;

                t.FlapCGSTAmount = l.FlapCGSTAmount;
                t.FlapSGSTAmount = l.FlapSGSTAmount;
                t.FlapIGSTAmount = l.FlapIGSTAmount;

                t.TyreTotalAmount = l.TyreTotalAmount;
                t.TubeTotalAmount = l.TubeTotalAmount;
                t.FlapTotalAmount = l.FlapTotalAmount;
                t.RoundUpAmount = l.RoundUpAmount;
                t.NetAmount = l.NetAmount;
                t.POLogId = l.PurchaseId;
                //t.TaxServiceTypeId = l.TaxServiceTypeId;
                t.CGSTAmount = l.CGSTAmount;
                t.SGSTAmount = l.SGSTAmount;
                t.IGSTAmount = l.IGSTAmount;
                t.CGSTPercent = l.CGSTPercent;
                t.SGSTPercent = l.SGSTPercent;
                t.IGSTPercent = l.IGSTPercent;

                t.TubeCGSTPercent = l.TubeCGSTPercent;
                t.TubeSGSTPercent = l.TubeSGSTPercent;
                t.TubeIGSTPercent = l.TubeIGSTPercent;

                t.FlapCGSTPercent = l.FlapCGSTPercent;
                t.FlapSGSTPercent = l.FlapSGSTPercent;
                t.FlapIGSTPercent = l.FlapIGSTPercent;

                t.WarrantyDays = l.WarrantyDays;
                t.WarrantyKm = l.WarrantyKm;
                t.VoucherDate = view.DocumentDate;
                t.VoucherNo = view.DocumentNo;
                t.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
                //if id is gt Zero Mark entity as Modified
                t.ObjectState = t.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        t.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }

                t.IsRemoulded = false;
                t.CreditAccountId = view.PrimaryCreditAccountId;
                t.DebitAccountId = view.PrimaryDebitAccountId.GetValueOrDefault();
                t.IssueReceiptId = null;
                t.JobsheetId = null;
                t.POLogId = l.PurchaseId;
                t.IsStepney = false;
                t.KmReading = 0;
                t.KmRun = 0;
                //t.ParentLogId = null;
                t.ReasonId = null;
                t.TyreLife = 0;
                t.TyreStatusId = l.TyreStatusId;
                t.TyreNo = l.TyreNo;
                //t.TaxServiceTypeId = l.TaxServiceTypeId;
                t.CalOthAmt = view.CalOthAmt;
                t.CalVat = view.CalVat;
                if (t.Id == 0)
                {
                    var tyre = new TyreMaster()
                    {
                        BrandId = l.BrandId.GetValueOrDefault(0),
                        IsAnalysis = true,
                        ObjectState = ObjectState.Added,
                        OpeningKm = 0,
                        OpeningMonth = 0,
                        ProdMonth = l.ProductionMonth.GetValueOrDefault(),
                        //fk_PurchaseTyreLog = t,
                        fk_PurchaseVoucher = (view.VoucherTypeId == 135 ? null : v),
                        S_Life = 0,
                        S_CreditAccountId = t.CreditAccountId,
                        S_StatusId = l.TyreStatusId,
                        S_DebitAccountId = t.DebitAccountId,
                        S_VoucherDate = view.DocumentDate,
                        TyreNo = t.TyreNo,
                        S_VoucherTypeId = t.VoucherTypeId,
                        // TaxServiceTypeId=t.TaxServiceTypeId
                    };
                    t.fk_Tyre = tyre;
                    // newTyres.Add(tyre);
                }
                else
                {
                    t.fk_Tyre.BrandId = l.BrandId.GetValueOrDefault(0);
                    t.fk_Tyre.IsAnalysis = true;
                    t.fk_Tyre.ObjectState = ObjectState.Modified;
                    t.fk_Tyre.OpeningKm = 0;
                    t.fk_Tyre.OpeningMonth = 0;
                    t.fk_Tyre.ProdMonth = l.ProductionMonth.GetValueOrDefault();
                    t.fk_Tyre.S_Life = 0;
                    t.fk_Tyre.S_CreditAccountId = t.CreditAccountId;
                    t.fk_Tyre.S_StatusId = l.TyreStatusId;
                    t.fk_Tyre.S_DebitAccountId = t.DebitAccountId;
                    t.fk_Tyre.S_VoucherDate = view.DocumentDate;
                    t.fk_Tyre.TyreNo = t.TyreNo;
                    t.fk_Tyre.S_VoucherTypeId = view.VoucherTypeId.Value;
                }                
                
                t.fk_Voucher = v;
                newTyreLogList.Add(t);
            }
            #endregion
            var tyreRepo = _repository.GetRepository<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id);
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.fk_NextLog != null))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                var tyresToBeDeleted = new List<long>();
                foreach (var log in deletedLogs)
                {
                    _repository.Update(RestorePreviousTyreStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    tyresToBeDeleted.Add(log.TyreId);
                    _repository.Delete(log);
                }
                if (tyresToBeDeleted.Any())
                {
                    _uom.Context.Database.ExecuteSqlCommand($"DELETE [dbo].[tTyreMillageLog] WHERE TyreId in({tyresToBeDeleted.JoinStrings(",")})");
                }
            }
            
            #region Prepare Voucher
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.ConstCurTypeId = view.ConstCurTypeId;


            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault(0);
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount;
            v.Account1Id = view.PrimaryDebitAccountId;
            v.Account2Id = view.PrimaryCreditAccountId;
            v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
            v.Account3Id = view.CGSTLedgerId;
            v.Amount3 = view.CGSTAmount;
            v.Account4Id = view.OtherLedgerId;
            v.Amount4 = view.OtherAmount;
            v.Account5Id = view.SGSTLedgerId;
            v.Amount5 = view.SGSTAmount;
            v.Account6Id = view.IGSTLedgerId;
            v.Amount6 = view.IGSTAmount;
            v.Account7Id = view.TCSAccountId;
            v.Amount7 = view.TCSAmount;
            v.Account8Id = view.RoundOffAccId;
            v.Amount8 = view.RoundOffAmt;
            v.Account9Id = view.PostDiscountAcId;
            v.Amount9 = -view.PostDiscountAmt;
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";
            PrepareVoucherDetails(_repository, v);
            #endregion
            #region Validations
            var vdrrequired =
                _repository.GetRepository<VoucherType>()
                    .Queryable()
                    .Where(x => x.Id == v.VoucherTypeId)
                    .Select(x => new
                    {
                        x.VDRRequired,
                        x.VDRequired
                    })
                    .FromCacheFirstOrDefault();
            if (vdrrequired != null)
            {
                if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                {
                    throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                }

                if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
                {
                    throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
                }
            }
            #endregion
            tei.fk_Voucher = v;
            tei.VoucherId = v.Id;            

            foreach (var log in newTyreLogList)
            {
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
                        
            _uom.SaveChanges();            

            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CalVat = view.CalVat;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.CalOthAmt = view.CalOthAmt;
            tei.OtherAcId = view.OtherLedgerId;
            tei.OtherChgAmt = view.OtherAmount;
            tei.TaxServiceTypeId = view.TyreHSNCodeId;
            tei.TubeHSNCodeId = view.TubeHSNCodeId;
            tei.FlapHSNCodeId = view.FlapHSNCodeId;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.OfficeId = view.OfficeId;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.RoundOffAccId = view.RoundOffAccId;
            tei.RoundOffAmt = view.RoundOffAmt;
            tei.PostDiscountAcId = view.PostDiscountAcId;
            tei.PostDiscountAmt = view.PostDiscountAmt;
            tei.ProvisionalAcId = view.ProvisionalAcId;
            tei.TCSRate = view.TCSRate;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.OtherHSNCodeId = view.OtherHSNId;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var x in newTyreLogList)
            {
                x.fk_Tyre.PurchaseLogId = x.Id;
                x.fk_Tyre.S_TyreLogId = x.Id;

                if (v != null && v.Id > 0) x.fk_Tyre.S_VoucherId = v.Id;

                x.fk_Tyre.ObjectState = ObjectState.Modified;
                x.ExtraInfoId = tei.Id;
                x.ExtraInfo = tei;
                x.ObjectState = ObjectState.Modified;
                tyreRepo.Update(x.fk_Tyre);
            }
            _uom.SaveChanges();
            return tei;
        }


        
        public TyreLogExtraInfo InsertOrUpdatePurchaseBillView(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.Tyres.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account {view.PrimaryCreditAccountId} or Primary Credit Ammount {view.PrimaryCreditAmount} has Invalid Value.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 || view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account {view.PrimaryDebitAccountId} or Primary Debit Ammount {view.PrimaryDebitAmount} has Invalid Value.");
            }
            if (view.PostDiscountAcId.GetValueOrDefault() <= 0 && view.PostDiscountAmt != 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"PostDiscount Account {view.PostDiscountAcId} or PostDiscount Amount {view.PostDiscountAmt} has Invalid Value.");
            }
            if (view.RoundOffAccId.GetValueOrDefault() <= 0 && view.RoundOffAmt != 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"RoundOff Account {view.RoundOffAccId} or RoundOff Ammount {view.RoundOffAmt} has Invalid Value.");
            }
            var teiRepo = _repository.GetRepository<TyreLogExtraInfo>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id) ?? new TyreLogExtraInfo();
            if (tei == default(TyreLogExtraInfo) && view.Id > 0)
            {
                //Through error if voucher not found with provided vouchertypeid
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }
            
            //var tpiRepo = _uom.RepositoryAsync<TyreLifePerformanceLog>();
            #region Tyre Log Preparation
            
            Voucher v = new Voucher();
            if (view.Id > 0 && view.VoucherTypeId!=135 /*Tyre MRN*/)
            {
                /*If it is existing transaction try to fatch v / vd / vdr and SpareLabour info from database*/
                v = _repository.GetRepository<Voucher>().Query(x => x.Id == tei.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
            }

            //Collect Distincs ReferenceId's from Posted SpareLogs
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == view.Id).ToList();
            }
            var oldtpiIdlist = existingTyreLogs.Select(x => x.Id).ToList();
            //var oldTyrePerformance = tpiRepo.Queryable().Where(x => oldtpiIdlist.Contains(x.FirstIssueLogId.Value)).ToList();
            var newTyreLogList = new List<TyreLog>();
            
            //var newTyrePerformance = new List<TyreLifePerformanceLog>();
            foreach (var l in view.Tyres)
            {
                l.TyreStatusId = view.VoucherTypeId == 66 ? 1100 : 1099;

                var t = new TyreLog();
                if (view.Id > 0 && l.Id > 0)
                {
                    t = existingTyreLogs.Find(x => x.Id == l.Id);
                    if (t.NextLogId != null && t.NextLogId > 0) //&&t.fk_ChildLog.ParentLogId==t.Id)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{t.fk_NextLog.VoucherNo}]");
                    }
                }
                
                var duplicatetyres=
                    _repository.Queryable().FirstOrDefault(x => x.TyreNo == l.TyreNo && x.TyreId != l.TyreId && (x.VoucherTypeId == 66 || x.VoucherTypeId == 135 || x.VoucherTypeId==27 || x.VoucherTypeId == 32 || x.VoucherTypeId == 79 || x.VoucherTypeId == 42));
                if (duplicatetyres != null)
                {
                    throw new BusinessException(ErrorCode.GLB105, $"Tyre No [  { l.TyreNo }  ] already Exists");
                }
                
                t.IsException = l.IsException;
                t.Rate = l.Rate;
                t.DiscountPercent = l.DiscountPercent;
                t.DiscountAmount = l.DiscountAmount;
                t.OtherAmount = l.OtherAmount;
                t.SubTotal = l.SubTotal;
                t.TubeRate = l.TubeRate;
                t.TubeDiscountPercent = l.TubeDiscountPercent;
                t.TubeDiscountAmount = l.TubeDiscountAmount;
                t.TubeOtherAmount = l.TubeOtherAmount;
                t.TubeSubTotal = l.TubeSubTotal;
                t.FlapRate = l.FlapRate;
                t.FlapDiscountPercent = l.FlapDiscountPercent;
                t.FlapDiscountAmount = l.FlapDiscountAmount;
                t.FlapOtherAmount = l.FlapOtherAmount;
                t.FlapSubTotal = l.FlapSubTotal;

                t.VehicleId = l.VehicleId;
                t.Remark = l.Remark;
                t.TyreId = l.TyreId;
                

                t.TubeCGSTAmount = l.TubeCGSTAmount;
                t.TubeSGSTAmount = l.TubeSGSTAmount;
                t.TubeIGSTAmount = l.TubeIGSTAmount;

                t.FlapCGSTAmount = l.FlapCGSTAmount;
                t.FlapSGSTAmount = l.FlapSGSTAmount;
                t.FlapIGSTAmount = l.FlapIGSTAmount;

                t.TyreTotalAmount = l.TyreTotalAmount;
                t.TubeTotalAmount = l.TubeTotalAmount;
                t.FlapTotalAmount = l.FlapTotalAmount;
                t.RoundUpAmount = l.RoundUpAmount;
                t.NetAmount = l.NetAmount;
                t.POLogId = l.PurchaseId;
                //t.TaxServiceTypeId = l.TaxServiceTypeId;
                t.CGSTAmount = l.CGSTAmount;
                t.SGSTAmount = l.SGSTAmount;
                t.IGSTAmount = l.IGSTAmount;
                t.CGSTPercent = l.CGSTPercent;
                t.SGSTPercent = l.SGSTPercent;
                t.IGSTPercent = l.IGSTPercent;

                t.TubeCGSTPercent = l.TubeCGSTPercent;
                t.TubeSGSTPercent = l.TubeSGSTPercent;
                t.TubeIGSTPercent = l.TubeIGSTPercent;

                t.FlapCGSTPercent = l.FlapCGSTPercent;
                t.FlapSGSTPercent = l.FlapSGSTPercent;
                t.FlapIGSTPercent = l.FlapIGSTPercent;

                t.WarrantyDays = l.WarrantyDays;
                t.WarrantyKm = l.WarrantyKm;
                t.VoucherDate = view.DocumentDate;
                t.VoucherNo = view.DocumentNo;
                t.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
                //if id is gt Zero Mark entity as Modified
                t.ObjectState = t.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        t.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }

                #region Voucher Tyre 27 => Inward of Purchase Tyre
                if (view.VoucherTypeId == 135 || view.VoucherTypeId == 27 || view.VoucherTypeId == 66 || view.VoucherTypeId == 32 || view.VoucherTypeId == 42) //Inward of Purchased Tyres
                {
                    t.IsRemoulded = false;
                    t.CreditAccountId = view.PrimaryCreditAccountId;
                    t.DebitAccountId = view.PrimaryDebitAccountId.GetValueOrDefault();
                    t.IssueReceiptId = null;
                    t.JobsheetId = null;
                    t.POLogId = l.PurchaseId;
                    t.IsStepney = false;
                    t.KmReading = 0;
                    t.KmRun = 0;
                    //t.ParentLogId = null;
                    t.ReasonId = null;
                    t.TyreLife = 0;
                    t.TyreStatusId = l.TyreStatusId;
                    t.TyreNo = l.TyreNo;
                    //t.TaxServiceTypeId = l.TaxServiceTypeId;
                    t.CalOthAmt = view.CalOthAmt;
                    t.CalVat = view.CalVat;
                    if (t.Id == 0)
                    {
                        var tyre = new TyreMaster()
                        {
                            BrandId = l.BrandId.GetValueOrDefault(0),
                            IsAnalysis = true,
                            ObjectState = ObjectState.Added,
                            OpeningKm = 0,
                            OpeningMonth = 0,
                            ProdMonth = l.ProductionMonth.GetValueOrDefault(),
                            //fk_PurchaseTyreLog = t,
                            fk_PurchaseVoucher = (view.VoucherTypeId == 135 ? null : v),
                            S_Life = 0,
                            S_CreditAccountId = t.CreditAccountId,
                            S_StatusId = l.TyreStatusId,
                            S_DebitAccountId = t.DebitAccountId,
                            S_VoucherDate = view.DocumentDate,
                            TyreNo = t.TyreNo,
                            S_VoucherTypeId = t.VoucherTypeId,
                            // TaxServiceTypeId=t.TaxServiceTypeId
                        };
                        t.fk_Tyre = tyre;
                        // newTyres.Add(tyre);
                    }
                    else
                    {
                        t.fk_Tyre.BrandId = l.BrandId.GetValueOrDefault(0);
                        t.fk_Tyre.IsAnalysis = true;
                        t.fk_Tyre.ObjectState = ObjectState.Modified;
                        t.fk_Tyre.OpeningKm = 0;
                        t.fk_Tyre.OpeningMonth = 0;
                        t.fk_Tyre.ProdMonth = l.ProductionMonth.GetValueOrDefault();
                        t.fk_Tyre.S_Life = 0;
                        t.fk_Tyre.S_CreditAccountId = t.CreditAccountId;
                        t.fk_Tyre.S_StatusId = l.TyreStatusId;
                        t.fk_Tyre.S_DebitAccountId = t.DebitAccountId;
                        t.fk_Tyre.S_VoucherDate = view.DocumentDate;
                        t.fk_Tyre.TyreNo = t.TyreNo;
                        t.fk_Tyre.S_VoucherTypeId = view.VoucherTypeId.Value;
                    }
                }
                #endregion

                t.fk_Voucher = (view.VoucherTypeId == 135 ? null : v);
                newTyreLogList.Add(t);
            }
            #endregion
            var tyreRepo = _repository.GetRepository<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id);
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.fk_NextLog != null))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                var tyresToBeDeleted = new List<long>();
                foreach (var log in deletedLogs)
                {
                    _repository.Update(RestorePreviousTyreStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    tyresToBeDeleted.Add(log.TyreId);
                    _repository.Delete(log);
                }
                if (tyresToBeDeleted.Any())
                {
                    if (tei.VoucherTypeId == 42 || tei.VoucherTypeId == 135 || tei.VoucherTypeId == 27 || tei.VoucherTypeId == 66 || tei.VoucherTypeId == 32)
                    {
                        _uom.Context.Database.ExecuteSqlCommand($"DELETE [dbo].[tTyreMillageLog] WHERE TyreId in({tyresToBeDeleted.JoinStrings(",")})");                        
                    }
                }
            }            
            if (view.VoucherTypeId != 135)
            {
                #region Prepare Voucher
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
                v.ConstCurTypeId = view.ConstCurTypeId;
                

                v.VoucherDate = view.DocumentDate;
                v.VoucherDateTime = view.DocumentDate;
                v.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault(0);
                v.VoucherNo = view.DocumentNo;
                v.Amount1 = view.PrimaryDebitAmount;
                v.Account1Id = view.PrimaryDebitAccountId;
                v.Account2Id = view.PrimaryCreditAccountId;
                v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
                v.Account3Id = view.CGSTLedgerId;
                v.Amount3 = view.CGSTAmount;
                v.Account4Id = view.OtherLedgerId;
                v.Amount4 = view.OtherAmount;
                v.Account5Id = view.SGSTLedgerId;
                v.Amount5 = view.SGSTAmount;
                v.Account6Id = view.IGSTLedgerId;
                v.Amount6 = view.IGSTAmount;
                v.Account7Id = view.TCSAccountId;
                v.Amount7 = view.TCSAmount;
                v.Account8Id = view.RoundOffAccId;
                v.Amount8 = view.RoundOffAmt;
                v.Account9Id = view.PostDiscountAcId;
                v.Amount9 = -view.PostDiscountAmt;
                v.UserRemark = view.Narration;
                v.OfficeId = view.OfficeId;
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //TODO:Setup Account Narration from Template located with VoucherType
                v.AccountingRemark = "";
                PrepareVoucherDetails(_repository, v);
                #endregion
                #region Validations
                var vdrrequired =
                    _repository.GetRepository<VoucherType>()
                        .Queryable()
                        .Where(x => x.Id == v.VoucherTypeId)
                        .Select(x => new
                        {
                            x.VDRRequired,
                            x.VDRequired
                        })
                        .FromCacheFirstOrDefault();
                if (vdrrequired != null)
                {
                    if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                    {
                        throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                    }

                    if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
                    {
                        throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
                    }
                }
                #endregion
                tei.fk_Voucher = v;
                tei.VoucherId = v.Id;
            }

            foreach (var log in newTyreLogList)
            {
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            //var netamount = newTyreLogList.Sum(x => x.NetAmount);
            //var cgstamount = view.CalVat ? newTyreLogList.Sum(x => x.CGSTAmount+x.CGSTAmount+ CGSTAmount) : 0;
            //var sgstamount = view.CalVat ? newTyreLogList.Sum(x => x.SGSTAmount+ SGSTAmount+ SGSTAmount) : 0;
            //var igstamount = view.CalVat ? newTyreLogList.Sum(x => x.IGSTAmount+ IGSTAmount+ IGSTAmount) : 0;
            //var othamount = view.CalOthAmt ? newTyreLogList.Sum(x => x.OtherAmount) : 0;
            //if (v.Amount1 != netamount)
            //{
            //    throw new BusinessException(ErrorCode.GLB106, $"Tyre Total Net Value {netamount} Does't match Voucher Primary Debit Amount {v.Amount1}");
            //}
            //if (v.Amount2 != -(netamount + cgstamount + sgstamount + igstamount + othamount))
            //{
            //    throw new BusinessException(ErrorCode.GLB106, $"Bill Total Amount {-(netamount + cgstamount + sgstamount + igstamount + othamount)} Does't match Voucher Primary Debit Amount {v.Amount2}");
            //}            
            _uom.SaveChanges();

            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CalVat = view.CalVat;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.CalOthAmt = view.CalOthAmt;
            tei.OtherAcId = view.OtherLedgerId;
            tei.OtherChgAmt = view.OtherAmount;
            tei.TaxServiceTypeId = view.TyreHSNCodeId;
            tei.TubeHSNCodeId = view.TubeHSNCodeId;
            tei.FlapHSNCodeId = view.FlapHSNCodeId;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.OfficeId = view.OfficeId;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.RoundOffAccId = view.RoundOffAccId;
            tei.RoundOffAmt = view.RoundOffAmt;
            tei.PostDiscountAcId = view.PostDiscountAcId;
            tei.PostDiscountAmt = view.PostDiscountAmt;
            tei.TCSRate = view.TCSRate;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.OtherHSNCodeId = view.OtherHSNId;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var x in newTyreLogList)
            {
                x.fk_Tyre.PurchaseLogId = x.Id;
                x.fk_Tyre.S_TyreLogId = x.Id;

                if (v != null && v.Id > 0) x.fk_Tyre.S_VoucherId = v.Id;

                x.fk_Tyre.ObjectState = ObjectState.Modified;
                x.ExtraInfoId = tei.Id;
                x.ExtraInfo = tei;
                x.ObjectState = ObjectState.Modified;
                tyreRepo.Update(x.fk_Tyre);
            }
            _uom.SaveChanges();
            return tei;
        }

        private static void PrepareVoucherDetails(IRepository<TyreLog> repository, Voucher v,List<VoucherDetailReference> againstrefvdrs = null)
        {
            foreach (VoucherDetail vd in v.VoucherDetails)
            {
                vd.ObjectState = ObjectState.Deleted;
                foreach (VoucherDetailReference reference in vd.VoucherDetailReferences)
                {
                    reference.ObjectState = ObjectState.Deleted;
                }
            }
            var ledgerRepo = repository.GetRepository<Ledger>().Queryable();
            var offices = ledgerRepo.Where(x => x.Id == v.Account1Id || x.Id == v.Account2Id || x.Id == v.Account3Id || x.Id == v.Account4Id || x.Id == v.Account5Id || x.Id == v.Account6Id|| x.Id == v.Account7Id|| x.Id == v.Account8Id|| x.Id == v.Account9Id)
                .Select(x => new { x.Id, x.OfficeId, x.ReferenceFlag }).ToList();
            if (v.Account1Id.HasValue && v.Amount1 != 0)
            {
                var a1 = new VoucherDetail() { };
                a1.AccountId = v.Account1Id.Value;
                a1.Amount = v.Amount1;
                a1.OrderId = 1;
                
                a1.CurTypeId = v.CurTypeId;
                a1.CurRate = v.CurRate;
                a1.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account1Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account1Id}");
                }
                a1.OfficeId = (ledger.OfficeId??0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a1.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a1);
                if (ledger.ReferenceFlag || v.VoucherTypeId == 137) { PrepareVDR(a1, v.VoucherNo, againstrefvdrs); }
            }
            if (v.Account2Id.HasValue && v.Amount2 != 0)
            {
                var a2 = new VoucherDetail() { };
                a2.AccountId = v.Account2Id.Value;
                a2.Amount = v.Amount2;
                a2.OrderId = 2;

                a2.CurTypeId = v.CurTypeId;
                a2.CurRate = v.CurRate;
                a2.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account2Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account2Id}");
                }
                a2.OfficeId = (ledger.OfficeId??0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a2.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a2);
                if (ledger.ReferenceFlag || v.VoucherTypeId == 135 || v.VoucherTypeId== 137) { PrepareVDR(a2, v.VoucherNo); }
            }
            if (v.Account3Id > 0 && v.Amount3!=0)
            {
                var a3 = new VoucherDetail() { };
                a3.AccountId = v.Account3Id.GetValueOrDefault(0);
                a3.Amount = v.Amount3;
                a3.OrderId = 3;

                a3.CurTypeId = v.CurTypeId;
                a3.CurRate = v.CurRate;
                a3.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account3Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account3Id}");
                }
                a3.OfficeId = (ledger.OfficeId??0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a3.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a3);
                if (ledger.ReferenceFlag) { PrepareVDR(a3, v.VoucherNo); }
            }
            if (v.Account4Id > 0 && v.Amount4 != 0)
            {
                var a4 = new VoucherDetail() { };
                a4.AccountId = v.Account4Id.GetValueOrDefault(0);
                a4.Amount = v.Amount4;
                a4.OrderId = 4;

                a4.CurTypeId = v.CurTypeId;
                a4.CurRate = v.CurRate;
                a4.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account4Id)
                     .Select(x => new { x.OfficeId, x.ReferenceFlag })
                     .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account4Id}");
                }
                a4.OfficeId = (ledger.OfficeId??0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a4.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a4);
                if (ledger.ReferenceFlag) { PrepareVDR(a4, v.VoucherNo); }
            }
            if (v.Account5Id > 0 && v.Amount5 != 0)
            {
                var a5 = new VoucherDetail() { };
                a5.AccountId = v.Account5Id.GetValueOrDefault(0);
                a5.Amount = v.Amount5;
                a5.OrderId = 5;

                a5.CurTypeId = v.CurTypeId;
                a5.CurRate = v.CurRate;
                a5.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account5Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account5Id}");
                }
                a5.OfficeId = (ledger.OfficeId??0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a5.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a5);
                if (ledger.ReferenceFlag) { PrepareVDR(a5, v.VoucherNo); }
            }

            if (v.Account6Id > 0 && v.Amount6 != 0)
            {
                var a6 = new VoucherDetail() { };
                a6.AccountId = v.Account6Id.GetValueOrDefault(0);
                a6.Amount = v.Amount6;
                a6.OrderId = 6;
                a6.CurTypeId = v.CurTypeId;
                a6.CurRate = v.CurRate;
                a6.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account6Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account6Id}");
                }
                a6.OfficeId = (ledger.OfficeId??0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a6.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a6);
                if (ledger.ReferenceFlag) { PrepareVDR(a6, v.VoucherNo); }
            }
            if (v.Account7Id > 0 && v.Amount7 != 0)
            {
                var a7 = new VoucherDetail() { };
                a7.AccountId = v.Account7Id.GetValueOrDefault(0);
                a7.Amount = v.Amount7;
                a7.OrderId = 7;

                a7.CurTypeId = v.CurTypeId;
                a7.CurRate = v.CurRate;
                a7.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account7Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account7Id}");
                }
                a7.OfficeId = (ledger.OfficeId??0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a7.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a7);
                if (ledger.ReferenceFlag) { PrepareVDR(a7, v.VoucherNo); }
            }
            if (v.Account8Id > 0 && v.Amount8 != 0)
            {
                var a8 = new VoucherDetail() { };
                a8.AccountId = v.Account8Id.GetValueOrDefault(0);
                a8.Amount = v.Amount8;
                a8.OrderId = 8;

                a8.CurTypeId = v.CurTypeId;
                a8.CurRate = v.CurRate;
                a8.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account8Id)
                        .Select(x => new { x.OfficeId, x.ReferenceFlag })
                        .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification Account8 :{v.Account8Id}");
                }
                a8.OfficeId = (ledger.OfficeId ?? 0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a8.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a8);
                if (ledger.ReferenceFlag) { PrepareVDR(a8, v.VoucherNo); }
            }
            if (v.Account9Id > 0 && v.Amount9 != 0)
            {
                var a9 = new VoucherDetail() { };
                a9.AccountId = v.Account9Id.GetValueOrDefault(0);
                a9.Amount = v.Amount9;
                a9.OrderId = 9;

                a9.CurTypeId = v.CurTypeId;
                a9.CurRate = v.CurRate;
                a9.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == v.Account9Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account9Id}");
                }
                a9.OfficeId = (ledger.OfficeId??0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a9.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a9);
                if (ledger.ReferenceFlag) { PrepareVDR(a9, v.VoucherNo); }
            }
            if (v.Account10Id > 0 && v.Amount10 != 0)
            {
                var a10 = new VoucherDetail() { };
                a10.AccountId = v.Account10Id.GetValueOrDefault(0);
                a10.Amount = v.Amount10;
                a10.OrderId = 10;

                a10.CurTypeId = v.CurTypeId;
                a10.CurRate = v.CurRate;
                a10.ConstCurTypeId = v.ConstCurTypeId;

                var ledger = offices.Where(x => x.Id == a10.AccountId)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account10Id}");
                }
                a10.OfficeId = (ledger.OfficeId ?? 0) == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a10.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a10);
                if (ledger.ReferenceFlag) { PrepareVDR(a10, v.VoucherNo); }
            }
        }

        private static void PrepareVDR(VoucherDetail vd, string voucherNo, List<VoucherDetailReference> againstrefvdrs = null)
        
        {
            if (againstrefvdrs != null && againstrefvdrs.Any())
            {
                foreach (var avdr in againstrefvdrs.GroupBy(x=>x.Id))
                {
                    var vdr = new VoucherDetailReference()
                    {
                        Amount = avdr.Sum(x=>x.Amount),
                        ObjectState = ObjectState.Added,
                        ReferenceNo = avdr.FirstOrDefault().ReferenceNo,
                        VDRTypeId = 1014,
                        RefId = avdr.Key,
                        CurTypeId = vd.CurTypeId,
                        CurRate = vd.CurRate,
                        ConstCurTypeId = vd.ConstCurTypeId
                    };
                    vd.VoucherDetailReferences.Add(vdr);
                }
            }
            else
            {

                var vdr = new VoucherDetailReference()
                {
                    Amount = vd.Amount,
                    ObjectState = ObjectState.Added,
                    ReferenceNo = voucherNo,
                    VDRTypeId = 1013,
                    CurTypeId = vd.CurTypeId,
                    CurRate = vd.CurRate,
                    ConstCurTypeId = vd.ConstCurTypeId
                };
                vd.VoucherDetailReferences = new List<VoucherDetailReference>() { vdr };
            }
        }

        //public async Task DeleteGraphAsync(long key, IUnitOfWorkAsync uow)
        //{

        //}
        /// <exception cref="BusinessException">Invalid VoucherId.</exception>
        /// 

        public async Task DeleteBySQLProc(long key, IUnitOfWorkAsync uow)
        {
            try
            {
                await uow.ExecuteProcedureAsync("[dbo].[Proc_GBL_Tyre_Transaction_Delete]",
                new[] { new SqlParameter("parameter1", key)});
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public async Task DeleteGraphAsync(long key, IUnitOfWorkAsync uow)
        {
            var teiRepo = uow.RepositoryAsync<TyreLogExtraInfo>();

            var settingids = new[] { "VoucherVisiblityFlag" };
            var settings = uow.RepositoryAsync<ApiConfiguration>().Queryable().Where(x => settingids.Contains(x.Value)).ToList();
            var tei = teiRepo.Find(key);
            if (tei == null) throw new BusinessException(ErrorCode.GLB109, $"The selected transaction is not existing.");
            var typeCanBeDeleted = new List<long>() { 135,137,27, 28, 29, 30, 31, 32,79, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42,66 };
            if (!typeCanBeDeleted.Contains(tei.VoucherTypeId))
            {
                throw new BusinessException(ErrorCode.GLB106, "Only Tyre transactions can be deleted through this Command");
            }
            Voucher voucher = null;
            if (tei.VoucherId.HasValue)
            {
                voucher =
               _repository.GetRepository<Voucher>()
                   .Queryable()
                   .Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences))
                   .FirstOrDefault(x => x.Id == tei.VoucherId);
            }
            #region Voucher Deletion process
           
            if (voucher != default(Voucher))
            {
                #region//Deletion Condition Defined Here
                //Deletion Case 1: Audited vouchers cannot be deleted from Fleet until & unless user not deleted the voucher from accounts.
                if (voucher.IsAudited)
                {
                    throw new BusinessException(ErrorCode.VCH102);//The Audited Voucher Transaction cannot be deleted.
                }

                //Deletion Case 2: In case of Manual authorization voucher cannot be deleted automatically until & unless user not deleted the voucher from accounts.
                var visiblatyFlag = settings.FirstOrDefault(x => x.Key == "VoucherVisiblityFlag")?.Value;

                if (visiblatyFlag != null && voucher.IsAccepted && int.Parse(visiblatyFlag) == 2)
                {
                    throw new BusinessException(ErrorCode.VCH101);//Cannot Modify Accepted Transaction
                }
                #endregion//Deletion Condition End
                var refids = new List<long?>();
                voucher.ObjectState = ObjectState.Deleted;
                foreach (var detail in voucher.VoucherDetails)
                {
                    detail.ObjectState = ObjectState.Deleted;
                    foreach (var reference in detail.VoucherDetailReferences)
                    {
                        refids.Add(reference.Id);
                        reference.ObjectState = ObjectState.Deleted;
                    }
                }
               
                //Deletion Case 100:Check if VDR has not been referenced in other voucher(s)
                if (uow.RepositoryAsync<VoucherDetailReference>().Queryable().Any(x => x.RefId.HasValue && refids.Contains(x.RefId)))
                {
                    throw new BusinessException(ErrorCode.VCH103, $"The transaction has been referenced in other voucher(s)");
                }
            }
            
            #endregion VoucherEnd

            #region Tyre Deletion Process Checks


             //Remove Voucher link from Tyre Extra Info
                                  //Deletion Case 1: The Whole Transaction shall be deleted if & only if nextlogid(s) of all tyre(s) within a transaction is null
            var list = _repository.Queryable().Include(x => x.fk_Tyre)
                   .Include(x => x.fk_NextLog).Include(x => x.fk_TyreCheck)
                   .Include(x => x.fk_PreviousLog).Where(x => (x.ExtraInfoId == tei.Id) || (x.VoucherTypeId == 137 && x.BillExtraInfoId == tei.Id)).ToList();


            if (tei.VoucherTypeId == 137)
            {
                foreach (var log in list)
                {
                    log.BillExtraInfoId = null;
                    log.fk_Bill = null;
                    log.ObjectState = ObjectState.Modified;
                }
            }
            else
            {
                var tyres = _repository.Queryable().Where(x => x.ExtraInfoId == tei.Id).Select(x => x.TyreId).Distinct().ToList();
                
                if (tyres.Count > 0)
                {
                    try
                    {
                        await uow.ExecSqlQueryAsync("UPDATE [tTyreLog] SET TyreCheckId=NULL WHERE ExtraInfoId=@id", new SqlParameter("id", tei.Id));
                        await uow.ExecSqlQueryAsync($"update  [tTyreCheck] set NextlogId=null WHERE NextlogId in (select x.Id from [tTyreCheck] as x WHERE TyreId in({tyres.JoinStrings(",")}) AND CheckDate>=@date)", new SqlParameter("date", tei.VoucherDate.Date));
                        await uow.ExecSqlQueryAsync($"update  [tTyreCheck] set Previouslogid=null WHERE Previouslogid in (select x.Id from [tTyreCheck] as x WHERE TyreId in({tyres.JoinStrings(",")}) AND CheckDate>=@date)", new SqlParameter("date", tei.VoucherDate.Date));
                        await uow.ExecSqlQueryAsync($"DELETE [tTyreCheck] WHERE TyreId in({tyres.JoinStrings(",")}) AND CheckDate>=@date", new SqlParameter("date", tei.VoucherDate.Date));
                    }
                    catch
                    {
                        //Ignore
                    }
                }
                tei.VoucherId = null;
                if (list.Any(x => x.NextLogId > 0 && x.ExtraInfoId == tei.Id))
                {
                    var invalidrows = list.Where(x => x.NextLogId > 0).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine);
                    throw new BusinessException(ErrorCode.GLB108, $"Transaction cannot delete some of the tyre(s) has been referenced.{Environment.NewLine} Tyre Nos=>Child VoucherNo:{invalidrows}");
                }
                #endregion Tyre Deletion Process Checks end

                var tpRepo = uow.RepositoryAsync<TyreLifePerformanceLog>();
                List<TyreLifePerformanceLog> tplist = null;

                //if (tei.VoucherTypeId == 34 || tei.VoucherTypeId == 35) //35:Tyre Receipt, 34: Tyre Issue
                //{
                //    tplist = (from t in tpRepo.Queryable()
                //              join p in _repository.Queryable().Where(x => x.ExtraInfoId == tei.Id)
                //              on (t.TyreId + "" + t.Life) equals (p.TyreId + "" + p.TyreLife)
                //              where (t.FirstIssueLogId == p.Id || t.LastReceiptLogId == p.Id) && t.Life == p.TyreLife
                //              select t
                //               ).ToList();
                //}

                if (tei.VoucherTypeId == 135  || tei.VoucherTypeId == 27 || tei.VoucherTypeId == 42 || tei.VoucherTypeId == 66 || tei.VoucherTypeId == 32) //27:Tyre Purchased, 42:Chassis Tyre
                {
                    await uow.ExecSqlQueryAsync($"Update [mTyreMaster] set PurchaseLogId=null,Status_TyreLogId =null,PurchaseVoucherId=null,Status_VoucherId=null WHERE Id in({tyres.JoinStrings(",")})");
                    _repository.UOW.SaveChanges();
                }

                foreach (var log in list)
                {
                    if (log.VoucherTypeId == 34)//Issue Log: Restore Last issue in Tyre Performance Log
                    { 
                        if (log.fk_TyreCheck != null)
                            log.fk_TyreCheck.ObjectState = ObjectState.Deleted;
                    }

                    if (!(log.TyreStatusId == 1099 && log.VoucherTypeId == 32))
                    {
                        //uow.Context.Database.ExecuteSqlCommand($"DELETE [dbo].[tTyrelog] WHERE Id in({log.Id})");
                        log.ObjectState = ObjectState.Deleted;
                        _repository.Delete(log);
                    }
                    await RestorePreviousTyreStatusAsync(log);
                    log.IgnoreValidation = true;
                    log.IssueReceiptId = null;
                    log.fk_IssueReceipt = null;
                }
                
                uow.SaveChanges();
                
                foreach (var log in list)
                {
                    if (log.TyreStatusId == 1099 && log.VoucherTypeId == 32)
                    {
                        log.ObjectState = ObjectState.Deleted;
                        await RestorePreviousTyreStatusAsync(log);
                        _repository.Delete(log);
                    }
                }

                uow.SaveChanges();
                if (voucher != default(Voucher))
                {
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
                tei.ObjectState = ObjectState.Deleted;
                teiRepo.Delete(tei);
                uow.SaveChanges();

                var tyrelist = list.Where(x => x.TyreStatusId != 1163).Select(x => x.TyreId).Distinct().ToList();
                if (tei.VoucherTypeId == 42 || tei.VoucherTypeId == 135 || tei.VoucherTypeId == 27 || tei.VoucherTypeId == 66 || tei.VoucherTypeId == 32)
                {
                    /*uow.Context.Database.ExecuteSqlCommand($"DELETE [dbo].[tTyreMillageLog] WHERE TyreId in({tyrelist.JoinStrings(",")})");*/
                    uow.Context.Database.ExecuteSqlCommand($"DELETE [dbo].[tTyreCheck] WHERE TyreId in({tyrelist.JoinStrings(",")})");
                    uow.Context.Database.ExecuteSqlCommand($"DELETE FROM dbo.[mTyreMaster] WHERE Id in({tyrelist.JoinStrings(",")})");
                }
            }
        }
        
        public vwTyreBillView GetTyreResaleBill(long key)
        {
            return _repository.GetTyreResaleBillView(key);
        }
        public TyreLogExtraInfo InsertUpdateTyreScrap(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            view.VoucherTypeId = 37;//Tyre Scrap
            if (view.ScrapLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }

            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Account {view.PrimaryCreditAccountId}");
            }

            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 || view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Buyer Account {view.PrimaryDebitAccountId} or Buyer Ammount {view.PrimaryDebitAmount} is required.");
            }

            if (view.OtherLedgerId <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106,
                    $"Tyre Income Account {view.OtherLedgerId} or Tyre Income Ammount {view.PrimaryCreditAmount} is required.");
            }

            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<TyreLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<TyreLifePerformanceLog>();
            Voucher v = new Voucher();

            //Collect Distince ReferenceId's from Posted SpareLogs
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = vRepo.Query(x => x.Id == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (v == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.VoucherId == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var issuerefids = view.ScrapLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var newtyrestatus = new long[] { 1100 };
            List<TyreLog> scrapReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => issuerefids.Contains(x.Id)).ToList();
            var newTyreLogList = new List<TyreLog>();
            var oldTyreLogs = new List<TyreLog>();
            foreach (var l in view.ScrapLog)
            {
                /************************************************************
                *************||Tyre Scrap Logics Start||*********************
                *************************************************************/
                #region Tyre Scrap Logic
                var i = new TyreLog();//Scrap Log
                var ir = scrapReferenceLogs.FirstOrDefault(x => x.Id == l.ReferenceId);//Scrap Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.TyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.TyreNo}");
                }
                if (!newtyrestatus.Contains(ir.TyreStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {l.TyreNo} can't be scrap");
                }
                if (l.Id > 0)
                {
                    i = existingTyreLogs.Find(x => x.Id == l.Id);//Issued Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Tyre has been altered restore all tyre status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                    //oldTyreLogs.Add(RestorePreviousTyreStatus(i));
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }
                i.TSLId = l.TSLId;
                i.Rate = i.SubTotal =  l.TyreCost;
                i.DiscountAmount = i.DiscountPercent = i.OtherAmount = i.KmReading = i.KmRun = 0;
               
                i.OtherAmount = i.OtherAmount;
                i.IsStepney = false;
                i.IsException = l.IsException;
                
                i.CGSTAmount = l.CGSTAmount;
                i.SGSTAmount = l.SGSTAmount;
                i.IGSTAmount = l.IGSTAmount;
                i.CGSTPercent = l.CGSTPercent;
                i.SGSTPercent = l.SGSTPercent;
                i.IGSTPercent = l.IGSTPercent;

                i.TyreTotalAmount = l.TyreTotalAmount;                
                i.RoundUpAmount = l.RoundUpAmount;
                i.NetAmount =  l.NetAmount;

                i.CreditAccountId = view.PrimaryCreditAccountId;//StoreId
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;//VendorId
                i.Remark = l.Remark;
                i.TyreId = ir.TyreId;
                i.fk_Tyre = ir.fk_Tyre;
                i.TyreLife = ir.TyreLife;
                i.TyreStatusId = 1107;//Scrap
                i.TyreNo = i.fk_Tyre.TyreNo;
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = ir.TyreLife;
                i.fk_Tyre.S_StatusId = i.TyreStatusId;//Scrap
                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        i.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }
                if (i.Id > 0)
                {
                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newTyreLogList.Add(i);
                #endregion
            }
            var tyreRepo = _uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    _repository.Update(RestorePreviousTyreStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }
            #region Prepare Scrap Voucher


            //var totalScrap = view.ScrapLog.Sum(x => x.TyreCost);
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.ConstCurTypeId = view.ConstCurTypeId;

            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.Value;
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount;
            v.Account1Id = view.PrimaryDebitAccountId;//VendorId
            v.Account2Id = view.OtherLedgerId;//Income Id
            v.Amount2 = -Math.Abs(view.PrimaryCreditAmount);
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;

            v.Account3Id = view.CGSTLedgerId;
            v.Amount3 = view.CGSTAmount;
            v.Account4Id = view.SGSTLedgerId;
            v.Amount4 = view.SGSTAmount;
            v.Account5Id = view.IGSTLedgerId;
            v.Amount5 = view.IGSTAmount;

            v.Account7Id = view.TCSAccountId;
            v.Amount7 = view.TCSAmount;

            v.Account8Id = view.RoundOffAccId;
            v.Amount8 = view.RoundOffAmt;

            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";

            //if (v.Amount1 != totalScrap)
            //{
            //    throw new BusinessException(ErrorCode.GLB106, $"Buyer Total Amount {totalScrap} Does't match Voucher Primary Debit Amount {v.Amount1}");
            //}
            //if (v.Amount2 != -totalScrap)
            //{
            //    throw new BusinessException(ErrorCode.GLB106, $"Income Total Amount {-totalScrap} Does't match Voucher Primary Credit Amount {v.Amount2}");
            //}

            PrepareVoucherDetails(_repository, v);
            #endregion
            #region Validations
            var vdrrequired =
                _repository.GetRepository<VoucherType>()
                    .Queryable()
                    .Where(x => x.Id == view.VoucherTypeId)
                    .Select(x => new
                    {
                        x.VDRRequired,
                        x.VDRequired
                    })
                    .FirstOrDefault();
            if (vdrrequired != null)
            {
                if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                {
                    throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                }

                if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
                {
                    throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
                }
            }
            #endregion
            tei = tei ?? new TyreLogExtraInfo();
            tei.fk_Voucher = v;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;

            tei.CalVat = view.CalVat;
            tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherId = v.Id;
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;//StoreId
            tei.DrAccountId = view.PrimaryDebitAccountId;//VendorId
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.VoucherNo = view.DocumentNo;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.TCSAccountId = view.TCSAccountId;
            
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.RoundOffAccId = view.RoundOffAccId;
            tei.RoundOffAmt = view.RoundOffAmt;

            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);

            foreach (var log in newTyreLogList)
            {
                if (v != null && v.Id > 0) log.VoucherId = v.Id;
                if (v != null && v.Id > 0) log.fk_Voucher = v;
                if (v != null && v.Id > 0) log.fk_Tyre.S_VoucherId = v.Id;
                log.fk_Tyre.S_VoucherDate = v.VoucherDate;
                log.fk_Tyre.fk_S_Voucher = v;
                log.VoucherId = v?.Id;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            _uom.SaveChanges();
            return tei;
        }

        public vwTyreBillView GetTyreScrapBillView(long key)
        {
            return _repository.GetTyreScrapBillView(key);
        }
        public TyreLogExtraInfo InsertUpdateTyreStocktransferOutBillView(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            view.VoucherTypeId = 36;//Outward of Transfered Tyres
            if (view.StoreTransferLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Sender Store Account {view.PrimaryCreditAccountId} is required.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Receiver Store Account {view.PrimaryDebitAccountId} is required.");
            }
            var transitStore = _repository.GetRepository<ApiConfiguration>().Find("TransitStoreId");
            long transitStoreId = 0;
            long.TryParse(transitStore.Value, out transitStoreId);
            if (transitStore == null || !long.TryParse(transitStore.Value, out transitStoreId)) throw new BusinessException(ErrorCode.GLB103, "Transit Store need to be configured.");

            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<TyreLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<TyreLifePerformanceLog>();
            Voucher v = new Voucher();
            //Collect Distincs ReferenceId's from Posted SpareLogs
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                //If it is existing transaction try to fatch v/vd/vdr from database
                if (tei.VoucherId.GetValueOrDefault() > 0) //Added by Sanjay Kushwaha
                {
                    v =
                        vRepo.Query(x => x.Id == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId)
                            .Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences))
                            .Select(x => x)
                            .FirstOrDefault();
                    if (v == null)
                        throw new BusinessException(ErrorCode.VCH108,
                            $"The Transaction you are trying to update, doesn't exist");
                }
                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var StoreTransferrefids = view.StoreTransferLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var newtyrestatus = new long[] { 1099, 1100 };
            List<TyreLog> StoreTransferReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => StoreTransferrefids.Contains(x.Id)).ToList();
            var newTyreLogList = new List<TyreLog>();
            var oldTyreLogs = new List<TyreLog>();
            foreach (var l in view.StoreTransferLog)
            {
                /************************************************************
                *************||Tyre StoreTransfer Logics Start||*********************
                *************************************************************/
                #region Tyre StoreTransfer Logic
                var i = new TyreLog();//StoreTransfer Log
                var ir = StoreTransferReferenceLogs.Find(x => x.Id == l.ReferenceId);//StoreTransfer Reference Log

                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.TyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.TyreNo}");
                }
                if (!newtyrestatus.Contains(ir.TyreStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {l.TyreNo} can't be StoreTransfer");
                }
                if (l.Id > 0)
                {
                    i = existingTyreLogs.Find(x => x.Id == l.Id);//StoreTransfer Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Tyre has been altered restore all tyre status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                    //oldTyreLogs.Add(RestorePreviousTyreStatus(i));
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }
                i.TSLId = l.TSLId;
                i.Rate = i.TyreTotalAmount= i.SubTotal = i.NetAmount = l.TyreCost;
                i.OtherAmount = 0;
                i.DiscountAmount = i.DiscountPercent = i.KmRun = i.KmReading = 0;
                i.IsStepney = false;
                i.IsException = l.IsException;
                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = transitStoreId;
                i.Remark = l.Remark;
                i.TyreId = ir.TyreId;
                i.fk_Tyre = ir.fk_Tyre;
                i.TyreLife = ir.TyreLife;
                i.TyreStatusId = ir.TyreStatusId;// Shall retain old tyre status in case of StoreTransfer
                i.TyreNo = i.fk_Tyre.TyreNo;
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = ir.TyreLife;
                i.fk_Tyre.S_StatusId = i.TyreStatusId;
                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        i.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }
                if (i.Id > 0)
                {
                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newTyreLogList.Add(i);
                #endregion
            }
            var tyreRepo = _uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                //var deletedids = deletedLogs.Select(x => x.Id).ToList();
                //var parents = _repository.Queryable().Where(x => deletedids.Contains(x.NextLogId.Value)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Update(RestorePreviousTyreStatus(log));
                    
                    _repository.Delete(log);
                }
                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }
            #region Prepare StoreTransfer Voucher
            var totalVendorAmt = newTyreLogList.Sum(x => x.NetAmount);
            
            

            if (totalVendorAmt > 0)
            {
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
                v.ConstCurTypeId = view.ConstCurTypeId;
                v.ViewId=view.ViewId;
                v.VoucherDate = view.DocumentDate;
                v.VoucherDateTime = view.DocumentDate;
                v.VoucherTypeId = view.VoucherTypeId.Value;
                v.VoucherNo = view.DocumentNo;
                v.Amount1 = view.PrimaryDebitAmount;
                v.Account1Id = transitStoreId;
                v.Account2Id = view.PrimaryCreditAccountId;
                v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
                v.UserRemark = view.Narration;
                v.OfficeId = view.OfficeId;
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //TODO:Setup Account Narration from Template located with VoucherType
                v.AccountingRemark = "";

                if (v.Amount1 != totalVendorAmt)
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"Voucher Amount {totalVendorAmt} does't match Voucher Primary Debit Amount {v.Amount1}");
                }
                if (v.Amount2 != -totalVendorAmt)
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"Voucher Amount {-totalVendorAmt} does't match Voucher Primary Credit Amount {v.Amount2}");
                }

                PrepareVoucherDetails(_repository, v);

                #endregion

                #region Validations

                var vdrrequired =
                    _repository.GetRepository<VoucherType>()
                        .Queryable()
                        .Where(x => x.Id == view.VoucherTypeId)
                        .Select(x => new
                        {
                            x.VDRRequired,
                            x.VDRequired
                        })
                        .FirstOrDefault();
                if (vdrrequired != null)
                {
                    if (vdrrequired.VDRequired > 0 &&
                        v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                    {
                        throw new BusinessException(ErrorCode.VCH105);
                    }

                    if (vdrrequired.VDRRequired > 0 &&
                        !(v.VoucherDetails.Count(
                            x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >=
                          vdrrequired.VDRRequired))
                    {
                        throw new BusinessException(ErrorCode.VCH111,
                            "At least one VDR is Required");
                    }
                }

                #endregion
            }
            else
            {
                v = null;
            }

            tei = tei ?? new TyreLogExtraInfo();
            tei.fk_Voucher = v;
            tei.VoucherId = v?.Id;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;

            tei.VendorReferenceNo = view.VendorReferenceNo;

            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = transitStoreId;
            tei.ProvisionalAcId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);

            foreach (var log in newTyreLogList)
            {
                log.fk_Voucher = v;
                log.VoucherId = v?.Id;
                log.fk_Tyre.fk_S_Voucher = v;
                log.fk_Tyre.S_VoucherId = log.VoucherId;
                log.fk_Tyre.S_VoucherDate = view.DocumentDate;
                
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            _uom.SaveChanges();
            return tei;
        }
        public TyreLogExtraInfo InsertUpdateTyreStocktransferInBillView(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            view.VoucherTypeId = 28;//Inward of Transfered Tyres
            if (view.StoreTransferLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0)    
            {
                throw new BusinessException(ErrorCode.GLB106, $"Sender Store Account {view.PrimaryCreditAccountId} is required.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Receiver Store Account {view.PrimaryDebitAccountId} is required.");
            }
            var transitStore = _repository.GetRepository<ApiConfiguration>().Find("TransitStoreId");
            long transitStoreId = 0;
            long.TryParse(transitStore.Value, out transitStoreId);
            if (transitStore == null || !long.TryParse(transitStore.Value, out transitStoreId)) throw new BusinessException(ErrorCode.GLB103, "Transit Store need to be configured.");


            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<TyreLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<TyreLifePerformanceLog>();
            Voucher v = new Voucher();
            //Collect Distincs ReferenceId's from Posted SpareLogs
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                //If it is existing transaction try to fatch v/vd/vdr from database
                if (tei.VoucherId.GetValueOrDefault() > 0) //Added by Sanjay Kushwaha
                {
                    v =
                        vRepo.Query(x => x.Id == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId)
                            .Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences))
                            .Select(x => x)
                            .FirstOrDefault();
                    if (v == null)
                        throw new BusinessException(ErrorCode.VCH108,
                            $"The Transaction you are trying to update, doesn't exist");
                }
                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.VoucherId == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var StoreTransferrefids = view.StoreTransferLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();

            List<TyreLog> StoreTransferReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => StoreTransferrefids.Contains(x.Id)).ToList();
            var newTyreLogList = new List<TyreLog>();
            var oldTyreLogs = new List<TyreLog>();
            foreach (var l in view.StoreTransferLog)
            {
                /************************************************************
                *************||Tyre StoreTransfer Logics Start||*********************
                *************************************************************/
                #region Tyre StoreTransfer Logic
                var i = new TyreLog();//StoreTransfer Log
                var ir = StoreTransferReferenceLogs.Find(x => x.Id == l.ReferenceId);//StoreTransfer Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.TyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.TyreNo}");
                }
                if (ir.VoucherTypeId != 36)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {l.TyreNo} can't be Inwarded into Store. Only stock transfered tyre(s) can be inwarded through this transaction");
                }
                if (l.Id > 0)
                {
                    if (existingTyreLogs != null)
                    {
                        i = existingTyreLogs.Find(x => x.Id == l.Id);//StoreTransfer Log
                    }

                    if (i != null && i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }

                //if (l.Id > 0)
                //{
                //    i = existingTyreLogs.Find(x => x.Id == l.Id);//StoreTransfer Log
                //    if (i.NextLogId > 0)
                //    {
                //        throw new BusinessException(ErrorCode.GLB105,
                //            $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                //    }
                //}

                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Tyre has been altered restore all tyre status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                    //oldTyreLogs.Add(RestorePreviousTyreStatus(i));
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }
                i.TSLId = l.TSLId;
                i.Rate = i.TyreTotalAmount= i.SubTotal = l.TyreCost;
                i.OtherAmount = 0;
                i.NetAmount = l.TyreCost;
                i.DiscountAmount = i.DiscountPercent = i.KmRun = i.KmReading = 0;
                i.IsStepney = false;
                i.IsException = l.IsException;

                i.CreditAccountId = transitStoreId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.TyreId = ir.TyreId;
                i.fk_Tyre = ir.fk_Tyre;
                i.TyreLife = ir.TyreLife;
                i.TyreStatusId = ir.TyreStatusId;// Shall retain old tyre status in case of StoreTransfer
                i.TyreNo = i.fk_Tyre.TyreNo;
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = ir.TyreLife;
                i.fk_Tyre.S_StatusId = i.TyreStatusId;
                i.fk_PreviousLog = ir;
                i.PreviousLogId = ir.Id;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        i.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }
                if (i.Id > 0)
                {
                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newTyreLogList.Add(i);
                #endregion
            }
            var tyreRepo = _uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Update(RestorePreviousTyreStatus(log));
                    _repository.Delete(log);
                }

                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }
            #region Prepare StoreTransfer Voucher
            var totalVendorAmt = StoreTransferReferenceLogs.Sum(x => x.NetAmount);
            
            if (totalVendorAmt > 0)
            {
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
                v.ConstCurTypeId = view.ConstCurTypeId;
                v.ViewId = view.ViewId;
                v.VoucherDate = view.DocumentDate;
                v.VoucherDateTime = view.DocumentDate;
                v.VoucherTypeId = view.VoucherTypeId.Value;
                v.VoucherNo = view.DocumentNo;
                v.Amount1 = view.PrimaryDebitAmount;
                v.Account1Id = view.PrimaryDebitAccountId;
                v.Account2Id = transitStoreId;
                v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
                v.UserRemark = view.Narration;
                v.OfficeId = view.OfficeId;
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //TODO:Setup Account Narration from Template located with VoucherType
                v.AccountingRemark = "";

                if (v.Amount1 != totalVendorAmt)
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"Voucher Amount {totalVendorAmt} does't match Voucher Primary Debit Amount {v.Amount1}");
                }
                if (v.Amount2 != -totalVendorAmt)
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"Voucher Amount {-totalVendorAmt} does't match Voucher Primary Credit Amount {v.Amount2}");
                }

                PrepareVoucherDetails(_repository, v);
                #endregion

                #region Validations

                var vdrrequired =
                    _repository.GetRepository<VoucherType>()
                        .Queryable()
                        .Where(x => x.Id == view.VoucherTypeId)
                        .Select(x => new
                        {
                            x.VDRRequired,
                            x.VDRequired
                        })
                        .FirstOrDefault();
                if (vdrrequired != null)
                {
                    if (vdrrequired.VDRequired > 0 &&
                        v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                    {
                        throw new BusinessException(ErrorCode.VCH105);
                    }

                    if (vdrrequired.VDRRequired > 0 &&
                        !(v.VoucherDetails.Count(
                            x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >=
                          vdrrequired.VDRRequired))
                    {
                        throw new BusinessException(ErrorCode.VCH111,
                            "At least one VDR is Required");
                    }
                }

                #endregion
            }
            else
            {
                v = null;
            }


            tei = tei ?? new TyreLogExtraInfo();
            tei.fk_Voucher = v;
            tei.VoucherId = v?.Id;

            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;

            tei.VendorReferenceNo = view.VendorReferenceNo;
            
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = transitStoreId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.ProvisionalAcId = view.PrimaryCreditAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);

            foreach (var log in newTyreLogList)
            {
                log.fk_Voucher = v;
                log.VoucherId = v?.Id;

                log.fk_Tyre.fk_S_Voucher = v;
                log.fk_Tyre.S_VoucherId = log.VoucherId;
                log.fk_Tyre.S_VoucherDate = view.DocumentDate;
                
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            _uom.SaveChanges();
            return tei;
        }
        public vwTyreBillView GetTyreStoretransferOutBillView(long key)
        {
            return _repository.GetTyreStoretransferOutBillView(key);
        }
        public vwTyreBillView GetTyreStoretransferInBillView(long key)
        {
            return _repository.GetTyreStoretransferInBillView(key);
        }
        public TyreLogExtraInfo InsertUpdateTyreReject(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.RejectLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Tyre Vendor Account {view.PrimaryCreditAccountId} is required.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Account {view.PrimaryDebitAccountId} is required");
            }

            var teiRepo = _uom.RepositoryAsync<TyreLogExtraInfo>();
            if (teiRepo.Queryable().Any(x => x.VoucherNo == view.DocumentNo && view.Id <= 0))
            {
                throw new BusinessException(ErrorCode.GLB106, $"Document No {view.DocumentNo} already exists.");
            }
            if (view.Id <= 0 && view.Tyres.Any(x => x.Id > 0)) throw new BusinessException(ErrorCode.GLB106, $"Incomplete Transaction.");
            //TODO:Implement Document No change restriction validation
            //Collect Distincs ReferenceId's from Posted SpareLogs
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");


                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var Rejectrefids = view.RejectLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            //1105: Sent for Remould,1929: Sent for repairing,1106: SEnt for claim
            var allowedStatus = view.VoucherTypeId == 30 ? new long[] { 1105, 1929 } : new long[] { 1106 };
            List<TyreLog> RejectReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => Rejectrefids.Contains(x.Id)).ToList();
            var newTyreLogList = new List<TyreLog>();
            var oldTyreLogs = new List<TyreLog>();
            foreach (var l in view.RejectLog)
            {
                /************************************************************
                *************||Tyre Reject Logics Start||*********************
                *************************************************************/
                #region Tyre Reject Logic
                var i = new TyreLog();//Reject Log
                var ir = RejectReferenceLogs.Find(x => x.Id == l.ReferenceId);//Issue Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.TyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.TyreNo}");
                }
                if (!allowedStatus.Contains(ir.TyreStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre Status has already been changed. So the Tyre No {l.TyreNo} can't be inwarded.");
                }
                if (l.Id > 0)
                {
                    i = existingTyreLogs.Find(x => x.Id == l.Id);//Reject Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Tyre has been altered restore all tyre status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                    //oldTyreLogs.Add(RestorePreviousTyreStatus(i));
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }

                //30  Inward of Retreat Rejected Tyres
                //31  Inward of Claim Rejected Tyres
                //1100 Old Stock
                i.TSLId = l.TSLId;
                i.Rate =
                    i.SubTotal =
                        i.OtherAmount = i.NetAmount= i.TyreTotalAmount = i.DiscountAmount = i.DiscountPercent = i.KmReading = i.KmRun = 0;
                i.IsStepney = false;
                i.IsException = l.IsException;
                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.TyreId = ir.TyreId;
                i.ReasonId = ir.ReasonId;
                i.fk_Tyre = ir.fk_Tyre;
                i.TyreLife = ir.TyreLife;
                i.TyreStatusId = 1100;
                i.TyreNo = i.fk_Tyre.TyreNo;

                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = ir.TyreLife;
                i.fk_Tyre.S_StatusId = i.TyreStatusId;

                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        i.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }
                if (i.Id > 0)
                {
                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newTyreLogList.Add(i);
                #endregion
            }
            var tyreRepo = _uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Update(RestorePreviousTyreStatus(log));
                   
                    _repository.Delete(log);
                }

                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }


            tei = tei ?? new TyreLogExtraInfo();
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.TCSAccountId = view.TCSAccountId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newTyreLogList)
            {
                log.fk_Tyre.S_VoucherDate = tei.VoucherDate;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            _uom.SaveChanges();
            return tei;
        }
        public vwTyreBillView GetTyreRejectBillView(long key)
        {
            return _repository.GetTyreRejectBillView(key);
        }
        public TyreLogExtraInfo InsertUpdateTyreClaimReceiptBillView(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            if (view.Tyres.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }

            if (view.PrimaryCreditAccountId <= 0 && view.PrimaryCreditAmount > 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Vendor Name is Required");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 && view.PrimaryDebitAmount > 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Name is Required");
            }

            var teiRepo = _repository.GetRepository<TyreLogExtraInfo>();
            var vRepo = _uom.RepositoryAsync<Voucher>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id) ?? new TyreLogExtraInfo();
            if (tei == default(TyreLogExtraInfo) && view.Id > 0)
            {
                //Through error if voucher not found with provided vouchertypeid
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }
            Voucher v = new Voucher();
            #region Tyre Log Preparation

            //Collect Distincs ReferenceId's from Posted SpareLogs
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Include(x=>x.fk_PreviousLog).Where(x => x.ExtraInfoId == view.Id && x.TyreStatusId==1099).ToList();
            }
            var refIds = view.Tyres.Select(x => x.ReferenceId).ToList();
            var referencelogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => refIds.Contains(x.Id)).ToList();
            var newTyreLogList = new List<TyreLog>();
            var claimReceiptLog = new List<TyreLog>();
            var tyreRepo = _repository.GetRepository<TyreMaster>();
            foreach (var l in view.Tyres)
            {
                var t = new TyreLog();
                if (view.Id > 0 && l.Id > 0)
                {
                    t = existingTyreLogs.Find(x => x.Id == l.Id);
                    if (t.NextLogId != null && t.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{t.fk_NextLog.VoucherNo}]");
                    }
                }

                var duplicatetyres =
                    _repository.Queryable().FirstOrDefault(x => x.TyreNo == l.TyreNo && x.TyreId != l.TyreId && (x.VoucherTypeId == 135 || x.VoucherTypeId == 27 || x.VoucherTypeId == 32 || x.VoucherTypeId == 42 || x.VoucherTypeId == 79));
                if (duplicatetyres != null)
                {
                    throw new BusinessException(ErrorCode.GLB105, $"Tyre No [  { l.TyreNo }  ] already Exists");
                }


                #region//Claim Section start
                var crt = new TyreLog();

                var xx = referencelogs.FirstOrDefault(x => x.Id == l.ReferenceId);
               
                if (view.Id > 0 && l.Id > 0)
                {
                    var previouslogids = existingTyreLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                    if (previouslogids != null && previouslogids.Any())
                    {
                        _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                    }

                    var oldcrt =
                        existingTyreLogs.FirstOrDefault(x => x.fk_PreviousLog.PreviousLogId == l.ReferenceId);
                    if (oldcrt == default(TyreLog))
                    {
                        throw new BusinessException(ErrorCode.GLB106, $"Change of Tyre No ' { l.TyreNo } ' Not allowed.Use Delete Option in-case of Tyre Number Change.");
                    }
                }

                if (xx == default(TyreLog) || xx.TyreStatusId != 1106 || (xx.NextLogId != null && t.Id == 0))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre {l.TyreNo} has invalid reference. Because Ref Tyre No '{xx.TyreNo}' is out of stock");
                }

                #endregion




                #region New Tyre Entry of ClaimPassed
                t.IsException = l.IsException;
                t.Rate = l.Rate;
                t.DiscountPercent = l.DiscountPercent;
                t.DiscountAmount = l.DiscountAmount;
                t.OtherAmount = l.OtherAmount;
                t.SubTotal = l.SubTotal;
                t.TubeRate = l.TubeRate;
                t.TubeDiscountPercent = l.TubeDiscountPercent;
                t.TubeDiscountAmount = l.TubeDiscountAmount;
                t.TubeOtherAmount = l.TubeOtherAmount;
                t.TubeSubTotal = l.TubeSubTotal;
                t.FlapRate = l.FlapRate;
                t.FlapDiscountPercent = l.FlapDiscountPercent;
                t.FlapDiscountAmount = l.FlapDiscountAmount;
                t.FlapOtherAmount = l.FlapOtherAmount;
                t.FlapSubTotal = l.FlapSubTotal;
                t.VehicleId = l.VehicleId;
                t.Remark = l.Remark;
                t.TyreId = l.TyreId;
                t.CGSTAmount = l.CGSTAmount;
                t.SGSTAmount = l.SGSTAmount;
                t.IGSTAmount = l.IGSTAmount;

                t.TubeCGSTAmount = l.TubeCGSTAmount;
                t.TubeSGSTAmount = l.TubeSGSTAmount;
                t.TubeIGSTAmount = l.TubeIGSTAmount;

                t.FlapCGSTAmount = l.FlapCGSTAmount;
                t.FlapSGSTAmount = l.FlapSGSTAmount;
                t.FlapIGSTAmount = l.FlapIGSTAmount;

                t.TyreTotalAmount = l.TyreTotalAmount;
                t.TubeTotalAmount = l.TubeTotalAmount;
                t.FlapTotalAmount = l.FlapTotalAmount;
                t.RoundUpAmount = l.RoundUpAmount;
                t.NetAmount = l.NetAmount;
                t.POLogId = l.PurchaseId;
                t.CGSTPercent = l.CGSTPercent;
                t.SGSTPercent = l.SGSTPercent;
                t.IGSTPercent = l.IGSTPercent;

                t.TubeCGSTPercent = l.TubeCGSTPercent;
                t.TubeSGSTPercent = l.TubeSGSTPercent;
                t.TubeIGSTPercent = l.TubeIGSTPercent;

                t.FlapCGSTPercent = l.FlapCGSTPercent;
                t.FlapSGSTPercent = l.FlapSGSTPercent;
                t.FlapIGSTPercent = l.FlapIGSTPercent;




                t.WarrantyDays = l.WarrantyDays;
                t.WarrantyKm = l.WarrantyKm;
                t.VoucherDate = view.DocumentDate;
                t.VoucherNo = view.DocumentNo;
                t.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();

                t.IsRemoulded = false;
                t.CreditAccountId = view.PrimaryCreditAccountId;
                t.DebitAccountId = view.PrimaryDebitAccountId.GetValueOrDefault();
                t.TransferPrice = l.CarriedCost;// added by sanjay
                t.TyreLife = 0;
                t.TyreStatusId = 1099;
                t.TyreNo = l.TyreNo;
                t.CalOthAmt = view.CalOthAmt;
                t.CalVat = view.CalVat;
                
                //For New Tyre
                if (t.Id == 0)
                {

                    var tyre = new TyreMaster()
                    {
                        BrandId = l.BrandId.GetValueOrDefault(0),
                        IsAnalysis = true,
                        ObjectState = ObjectState.Added,
                        OpeningKm = 0,
                        OpeningMonth = 0,
                        ProdMonth = l.ProductionMonth.GetValueOrDefault(),
                        S_Life = 0,
                        S_CreditAccountId = t.CreditAccountId,
                        S_StatusId = 1099,
                        S_DebitAccountId = t.DebitAccountId,
                        S_VoucherDate = view.DocumentDate,
                        TyreNo = t.TyreNo,
                        S_VoucherTypeId = t.VoucherTypeId
                    };
                    t.fk_Tyre = tyre;
                }
                else
                {
                    t.fk_Tyre.BrandId = l.BrandId.GetValueOrDefault(0);
                    t.fk_Tyre.IsAnalysis = true;
                    t.fk_Tyre.ObjectState = ObjectState.Modified;
                    t.fk_Tyre.ProdMonth = l.ProductionMonth.GetValueOrDefault();
                    t.fk_Tyre.S_Life = 0;
                    t.fk_Tyre.S_CreditAccountId = t.CreditAccountId;
                    t.fk_Tyre.S_StatusId = 1099;
                    t.fk_Tyre.S_DebitAccountId = t.DebitAccountId;
                    t.fk_Tyre.S_VoucherDate = view.DocumentDate;
                    t.fk_Tyre.TyreNo = t.TyreNo;
                    t.fk_Tyre.S_VoucherTypeId = view.VoucherTypeId.Value;

                    //Fetching or Claimed pass record
                    if (t.fk_PreviousLog != null)
                    {
                        crt = t.fk_PreviousLog;
                    }
                }
               
                //if id is gt Zero Mark entity as Modified
                t.ObjectState = t.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        t.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }

                crt.Remark = "ClaimPassed";
                crt.TyreId = xx.TyreId;
                crt.TyreNo = xx.TyreNo;
                crt.TyreLife = xx.TyreLife;
                //crt.TaxServiceTypeId = xx.TaxServiceTypeId;
                crt.IsRemoulded = xx.IsRemoulded;
                crt.VoucherDate = view.DocumentDate;
                crt.VoucherNo = view.DocumentNo;
                crt.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
                crt.ScrapCost = l.CarriedCost;
                crt.TyreStatusId = 1163;//Claim Passed
                
                crt.PreviousLogId = xx.Id;
                crt.fk_PreviousLog = xx;
                crt.CreditAccountId = view.PrimaryCreditAccountId;
                crt.DebitAccountId = view.PrimaryDebitAccountId.GetValueOrDefault();

                crt.ObjectState = crt.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                crt.fk_Tyre = xx.fk_Tyre;

                crt.fk_Tyre.S_Life = crt.TyreLife;
                crt.fk_Tyre.S_CreditAccountId = crt.CreditAccountId;
                crt.fk_Tyre.S_StatusId = crt.TyreStatusId;
                crt.fk_Tyre.S_DebitAccountId = crt.DebitAccountId;
                crt.fk_Tyre.S_VoucherDate = crt.VoucherDate;
               // crt.fk_Tyre.S_VoucherId = v.Id;
                crt.fk_Tyre.S_VoucherTypeId = crt.VoucherTypeId;


                crt.fk_Tyre.ObjectState = ObjectState.Modified;


                xx.fk_NextLog = crt;
                xx.NextLogId = crt.Id;
                xx.ObjectState = ObjectState.Modified;

                claimReceiptLog.Add(crt);

                t.PreviousLogId = crt.Id;
                t.fk_PreviousLog = crt;
                
                newTyreLogList.Add(t);
                #endregion
            }
            #endregion
            

            #region Prepare Voucher
            if (view.Id > 0)
            {
                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = _repository.GetRepository<Voucher>().Query(x => x.Id == tei.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
            }
            var netamount = view.Tyres.Sum(x => x.NetAmount);
            if (netamount > 0)
            {
                v = v ?? new Voucher();
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
                v.ConstCurTypeId = view.ConstCurTypeId;

                v.VoucherDate = view.DocumentDate;
                v.VoucherDateTime = view.DocumentDate;
                v.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault(0);
                v.VoucherNo = view.DocumentNo;
                v.Amount1 = view.PrimaryDebitAmount;
                v.Account1Id = view.PrimaryDebitAccountId;
                v.Account2Id = view.PrimaryCreditAccountId;
                v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
                v.Account3Id = view.CGSTLedgerId;
                v.Amount3 = view.CGSTAmount;
                v.Account4Id = view.OtherLedgerId;
                v.Amount4 = view.OtherAmount;
                v.Account5Id = view.SGSTLedgerId;
                v.Amount5 = view.SGSTAmount;
                v.Account6Id = view.IGSTLedgerId;
                v.Amount6 = view.IGSTAmount;
                v.Account7Id = view.TCSAccountId;
                v.Amount7 = view.TCSAmount;
                v.UserRemark = view.Narration;
                v.OfficeId = view.OfficeId;
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //TODO:Setup Account Narration from Template located with VoucherType
                v.AccountingRemark = "";
                PrepareVoucherDetails(_repository, v);
            }
            #endregion

            foreach (var log in newTyreLogList)
            {
                if (netamount > 0)
                {
                    log.VoucherId = v.Id;
                    log.fk_Voucher = v;
                }
                else
                {
                    log.VoucherId = null;
                    log.fk_Voucher =null;
                }

                _repository.Insert(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            
            //var netamount = newTyreLogList.Sum(x => x.NetAmount);
            //var cgstamount = view.CalVat ? newTyreLogList.Sum(x => x.CGSTAmount) : 0;
            //var sgstamount = view.CalVat ? newTyreLogList.Sum(x => x.SGSTAmount) : 0;
            //var igstamount = view.CalVat ? newTyreLogList.Sum(x => x.IGSTAmount) : 0;
            //var othamount = view.CalOthAmt ? newTyreLogList.Sum(x => x.OtherAmount) : 0;
            //if (v.Amount1 != netamount)
            //{
            //    throw new BusinessException(ErrorCode.GLB106, $"Tyre Total Net Value {netamount} Does't match Voucher Primary Debit Amount {v.Amount1}");
            //}
            //if (v.Amount2 != -(netamount + cgstamount +sgstamount +igstamount + othamount))
            //{
            //    throw new BusinessException(ErrorCode.GLB106, $"Bill Total Amount {-(netamount + cgstamount + sgstamount + igstamount + othamount)} Does't match Voucher Primary Credit Amount {v.Amount2}");
            //}
            #region Validations
            if (netamount > 0)
            {
                var vdrrequired =
                    _repository.GetRepository<VoucherType>()
                        .Queryable()
                        .Where(x => x.Id == v.VoucherTypeId)
                        .Select(x => new
                        {
                            x.VDRRequired,
                            x.VDRequired
                        })
                        .FirstOrDefault();
                if (vdrrequired != null)
                {
                    if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                    {
                        throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                    }

                    if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
                    {
                        throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
                    }
                }
            }
            else if (v != null && v.Id > 0)
            {

                v.ObjectState = ObjectState.Deleted;
                foreach (var x in v.VoucherDetails)
                {
                    x.ObjectState = ObjectState.Deleted;
                    foreach (var y in x.VoucherDetailReferences) y.ObjectState = ObjectState.Deleted;
                }
                vRepo.Delete(v);
            }

            #endregion
            _uom.SaveChanges();

            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;

            tei.CalVat = view.CalVat;
            tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.OfficeId = view.OfficeId;
            tei.VoucherId = netamount > 0 ? v.Id : (long?)null;
            tei.VoucherNo = view.DocumentNo;
            tei.fk_Voucher = netamount > 0 ? v : null;
            tei.PageId = view.PageId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.VoucherDate = view.DocumentDate;
            tei.TaxServiceTypeId = view.TyreHSNCodeId;
            tei.TubeHSNCodeId = view.TubeHSNCodeId;
            tei.FlapHSNCodeId = view.FlapHSNCodeId;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.TCSAccountId = view.TCSAccountId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var x in newTyreLogList)
            {
                //Updating Voucher info in New Logs
                if (netamount > 0)
                {
                    x.VoucherId = v?.Id;
                    x.fk_Voucher = v;

                    //Updating Tyre Master Info
                    x.fk_Tyre.PurchaseVoucherId = v?.Id;
                    x.fk_Tyre.fk_PurchaseVoucher = v;
                    x.fk_Tyre.S_VoucherId = v?.Id;
                    x.fk_Tyre.fk_S_Voucher = v;
                }

                x.fk_Tyre.PurchaseLogId = x.Id;
                x.fk_Tyre.S_TyreLogId = x.Id;
                x.fk_Tyre.ObjectState = ObjectState.Modified;

                x.ExtraInfoId = tei.Id;
                x.ExtraInfo = tei;
                x.ObjectState = ObjectState.Modified;

                //Updating tei in claimed passed tyre
                x.fk_PreviousLog.ExtraInfoId = tei.Id;
                x.fk_PreviousLog.ExtraInfo = tei;
                if (netamount > 0)
                {
                    x.fk_PreviousLog.fk_Tyre.S_VoucherId = v?.Id;
                }
                x.fk_PreviousLog.fk_Tyre.S_TyreLogId = x.fk_PreviousLog.Id;
                x.fk_PreviousLog.fk_Tyre.fk_S_TyreLog = x.fk_PreviousLog;

                x.fk_PreviousLog.ObjectState = ObjectState.Modified;
                x.fk_PreviousLog.fk_Tyre.ObjectState = ObjectState.Modified;
                tyreRepo.Update(x.fk_Tyre);
                _repository.Update(x);
            }
            
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id);
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();

                if (deletedLogs.Any(x => x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                if (previouslogids.Any())
                {
                    _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                }
                var deletedTyres = new List<TyreMaster>();
                foreach (var log in deletedLogs)
                {
                    //restoring tyre record for sent for claimed tyre
                    _repository.Update(RestorePreviousTyreStatus(log.fk_PreviousLog));

                    //Deleting Claim Tyre Log
                    log.fk_PreviousLog.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log.fk_PreviousLog);

                    //Deleting Current Log
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);

                    //Deleting Tyre
                    log.fk_Tyre.S_TyreLogId = null;
                    log.fk_Tyre.PurchaseLogId = null;
                    log.fk_Tyre.fk_S_TyreLog = null;
                    log.fk_Tyre.fk_PurchaseTyreLog = null;
                    log.fk_Tyre.ObjectState = ObjectState.Modified;
                    tyreRepo.Update(log.fk_Tyre);
                    deletedTyres.Add(log.fk_Tyre);
                }
                _uom.SaveChanges();
                
                foreach (var t in deletedTyres)
                {
                    //Deleting Tyre
                    t.ObjectState = ObjectState.Deleted;
                    t.S_TyreLogId = null;
                    t.PurchaseLogId = null;
                    t.fk_S_TyreLog = null;
                    t.fk_PurchaseTyreLog = null;
                    tyreRepo.Delete(t);
                }
            }
            _uom.SaveChanges();
            return tei;
        }

        public vwTyreBillView GetTyreRemouldReceiptBillView(long key)
        {
            return _repository.GetTyreRemouldReceiptBillView(key);
        }
        public TyreLogExtraInfo InsertUpdateTyreRemouldReceipt(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            view.VoucherTypeId = view.VoucherTypeId == null ? 29 : view.VoucherTypeId;
            if (view.RemouldReceiptLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }

            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Tyre Vendor Account {view.PrimaryCreditAccountName} & Retraiting Cost is required.");
            }

            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Account {view.PrimaryDebitAccountName} & Retraiting Cost is required");
            }

            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<TyreLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<TyreLifePerformanceLog>();
            Voucher v = new Voucher();
            //Collect Distincs ReferenceId's from Posted SpareLogs
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = vRepo.Query(x => x.Id == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                

                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            
            if (v == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

            var RemouldReceiptrefids = view.RemouldReceiptLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var allowedStatus = new long[] { view.VoucherTypeId == 121/*Tyre Repair*/ ? 1929/*Sent for Reparing*/ : 1105 /*Sent For Remould*/};
            List<TyreLog> RemouldReceiptReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => RemouldReceiptrefids.Contains(x.Id)).ToList();
            var tyrelist = view.RemouldReceiptLog.Select(x => x.TyreId).ToList();
            var purchaseCosts = _uom.RepositoryAsync<TyreMaster>().Queryable().Where(x => tyrelist.Contains(x.Id)).Select(x => new
            {
                x.Id,
                PurchaseCost = x.fk_PurchaseTyreLog.NetAmount
            }).ToList();
            var newTyreLogList = new List<TyreLog>();
            var oldTyreLogs = new List<TyreLog>();
            foreach (var l in view.RemouldReceiptLog)
            {
                /************************************************************
                *************||Tyre RemouldReceipt Logics Start||*********************
                *************************************************************/
                #region Tyre RemouldReceipt Logic
                var i = new TyreLog();//RemouldReceipt Log
                var ir = RemouldReceiptReferenceLogs.Find(x => x.Id == l.ReferenceId);//Issue Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.TyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.TyreNo}");
                }
                if (!allowedStatus.Contains(ir.TyreStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre Status has already been changed. So the Tyre No {l.TyreNo} can't be inwarded.");
                }
                if (l.Id > 0)
                {
                    i = existingTyreLogs.Find(x => x.Id == l.Id);//RemouldReceiptLog
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                if (view.VoucherTypeId == 29)
                {
                    var _tyrePurchaseCost = purchaseCosts.FirstOrDefault(x => x.Id == l.TyreId);
                    if (_tyrePurchaseCost != null && l.CarriedCost > _tyrePurchaseCost.PurchaseCost)
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                            $"CarriedCost for the Tyre No {ir.TyreNo} should be less than purchase cost");
                    }
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Tyre has been altered restore all tyre status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                }
                i.IsException = l.IsException;
                //29  Remoudl received
                //1099 New Tyre Stock
                //1105 Sent for remould
                i.CalVat = view.CalVat;
                i.TSLId = l.TSLId;
                i.Rate = l.Amount;
                //i.TaxServiceTypeId = l.ServiceTaxTypeId;

                i.RubberTypeId = view.VoucherTypeId == 29 ? l.RubberTypeId : null;
                i.ReasonId = view.VoucherTypeId == 121 ? l.ReasonId : null;

                i.CGSTPercent = l.CGSTPercentage;
                i.CGSTAmount = l.CGSTAmount;
                i.SGSTPercent = l.SGSTPercentage;
                i.SGSTAmount = l.SGSTAmount;
                i.IGSTPercent = l.IGSTPercentage;
                i.IGSTAmount = l.IGSTAmount;
                i.SubTotal = i.Rate;
                i.TyreTotalAmount = i.SubTotal + (i.CalVat ? 0 : i.CGSTAmount + i.SGSTAmount + i.IGSTAmount);
                i.NetAmount = i.TyreTotalAmount + l.RoundUpAmount;

                //added by sanjay
                i.fk_Tyre = ir.fk_Tyre;
                /*
                 1100
                 */
                i.TyreStatusId = view.VoucherTypeId == 121? 1100/*Old Stock*/: 1099/*New Stock*/;
                i.TransferPrice = view.VoucherTypeId == 121 ? 0 : l.CarriedCost;
                i.TyreLife = view.VoucherTypeId == 121 ? ir.TyreLife : (ir.TyreLife + 1);

                i.OtherAmount = i.DiscountAmount = i.DiscountPercent = i.KmReading = i.KmRun = 0;
                i.IsStepney = false;
                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.RoundUpAmount = l.RoundUpAmount;
                i.Remark = l.Remark;
                i.TyreId = ir.TyreId;               
                i.TyreNo = i.fk_Tyre.TyreNo;
                
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = ir.TyreLife;
                i.fk_Tyre.S_StatusId = i.TyreStatusId;

                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                ir.ScrapCost = view.VoucherTypeId == 121 ? 0 : l.CarriedCost; //added by sanjay//adding carried cost(TP) in scrap value of old record in case of remoulding
                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        i.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }

                if (i.Id > 0)
                {
                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newTyreLogList.Add(i);
                #endregion
            }
            var tyreRepo = _uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Update(RestorePreviousTyreStatus(log));
                    
                    _repository.Delete(log);
                }

                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }
            #region Prepare Issue Voucher

            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.Value;
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount;
            v.Account1Id = view.PrimaryDebitAccountId;//StoreId
            v.Account2Id = view.PrimaryCreditAccountId;//VendorId
            v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
            v.Account3Id = view.CGSTLedgerId;
            v.Amount3 = view.CGSTAmount;
            v.Account4Id = view.SGSTLedgerId;
            v.Amount4 = view.SGSTAmount;
            v.Account5Id = view.IGSTLedgerId;
            v.Amount5 = view.IGSTAmount;
            v.Account7Id = view.TCSAccountId;
            v.Amount7 = view.TCSAmount;
            v.Account8Id = view.RoundOffAccId;
            v.Amount8 = view.RoundOffAmt;
            v.Account9Id = view.PostDiscountAcId;
            v.Amount9 = -view.PostDiscountAmt;
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            tei.VoucherId = v?.Id;
            tei.fk_Voucher = v;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";

            var cgstamount = view.CalVat ? view.RemouldReceiptLog.Sum(x => x.CGSTAmount) : 0;
            var sgstamount = view.CalVat ? view.RemouldReceiptLog.Sum(x => x.SGSTAmount) : 0;
            var igstamount = view.CalVat ? view.RemouldReceiptLog.Sum(x => x.IGSTAmount) : 0;
            var totalRemoudCost = view.RemouldReceiptLog.Sum(x => x.TyreCost);

            if (v.Amount1 != totalRemoudCost)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Total Amount {totalRemoudCost} Does't match Voucher Primary Debit Amount {v.Amount1}");
            }
            if (v.Amount2 != -(totalRemoudCost + cgstamount + sgstamount + igstamount+view.TCSAmount))
            {
                throw new BusinessException(ErrorCode.GLB106, $"Vendor Total Amount {-totalRemoudCost + cgstamount + sgstamount + igstamount} Does't match Voucher Primary Credit Amount {v.Amount2}");
            }

            PrepareVoucherDetails(_repository, v);
            #endregion
            #region Validations
            var vdrrequired =
                _repository.GetRepository<VoucherType>()
                    .Queryable()
                    .Where(x => x.Id == view.VoucherTypeId)
                    .Select(x => new
                    {
                        x.VDRRequired,
                        x.VDRequired
                    })
                    .FirstOrDefault();
            if (vdrrequired != null)
            {
                if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                {
                    throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                }

                if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
                {
                    throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
                }
            }
            #endregion

            tei = tei ?? new TyreLogExtraInfo();
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.VoucherNo = view.DocumentNo;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TaxServiceTypeId = view.TyreHSNCodeId;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.CalVat = view.CalVat;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newTyreLogList)
            {
                log.VoucherId = v.Id;
                log.fk_Voucher = v;
                log.fk_Tyre.S_VoucherId = v.Id;
                log.fk_Tyre.S_VoucherDate = v.VoucherDate;
                log.fk_Tyre.fk_S_Voucher = v;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            _uom.SaveChanges();
            return tei;
        }
        public TyreLogExtraInfo InsertUpdateTyreClaimSettlement(vwTyreBillView view, IUnitOfWorkAsync _uom)
        {
            view.ConstCurTypeId = Helper.ConstCurTypeId;
            view.VoucherTypeId = 33;
            if (view.TyreClaimSettlementLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Tyre Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0 && view.PrimaryCreditAmount > 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Vendor Name is Required");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 && view.PrimaryDebitAmount > 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Name is Required");
            }
            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<TyreLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<TyreLifePerformanceLog>();
            Voucher v = new Voucher();
            //Collect Distincs ReferenceId's from Posted SpareLogs
            TyreLogExtraInfo tei = new TyreLogExtraInfo();
            List<TyreLog> existingTyreLogs = new List<TyreLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = vRepo.Query(x => x.Id == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                
                existingTyreLogs = _repository.Queryable().Include(x => x.fk_Tyre).Include(x => x.fk_NextLog).Where(x => x.VoucherId == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            if (v == null) throw new BusinessException(ErrorCode.VCH108, $"Voucher: The Transaction you are trying to update, doesn't exist");

            var issuerefids = view.TyreClaimSettlementLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var newtyrestatus = new long[] { 1106 };
            List<TyreLog> issueReferenceLogs = _repository.Queryable().Include(x => x.fk_Tyre).Where(x => issuerefids.Contains(x.Id)).ToList();
            var newTyreLogList = new List<TyreLog>();
            var oldTyreLogs = new List<TyreLog>();
            foreach (var l in view.TyreClaimSettlementLog)
            {
                /************************************************************
                *************||Tyre Claim Settlement||*********************
                *************************************************************/
                #region Tyre Claim Settlement
                var i = new TyreLog();//Claim Settlement Log
                var ir = issueReferenceLogs.Find(x => x.Id == l.ReferenceId);//Claim Settlement Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Tyre No {l.TyreNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"Previous Log voucher type and current Voucher Type should be different for Tyre No {l.TyreNo}");
                }
                if (!newtyrestatus.Contains(ir.TyreStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {l.TyreNo} can't be ClaimSettlementd");
                }

                if (l.Id > 0)
                {
                    i = existingTyreLogs.Find(x => x.Id == l.Id);//Issued Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Tyre Log Information that has been referenced/issued.Ref Tyre No:{l.TyreNo}[Referenced Transaction No :{i.fk_NextLog.VoucherNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Tyre No {ir.TyreNo} is out of stock");
                }
                //Check if Tyre No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Tyre has been altered restore all tyre status to previous logs status
                    //oldTyreLogs.Add(RestorePreviousTyreStatus(i));
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    //i.fk_Tyre = null;
                    //i.TyreId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    throw new BusinessException(ErrorCode.GLB106, "Change of Tyre Number Not allowed.Use Delete Option in-case of Tyre Number Change.");
                }
                i.IsException = l.IsException;
                i.TSLId = l.TSLId;
                i.Rate = i.SubTotal = l.TyreRate;
                i.IGSTPercent = l.IGSTPercentage;
                i.CGSTPercent = l.CGSTPercentage;
                i.SGSTPercent = l.SGSTPercentage;
                i.IGSTAmount = l.IGSTAmount;
                i.SGSTAmount = l.SGSTAmount;
                i.CGSTAmount = l.CGSTAmount;
                i.OtherAmount = 0;
                i.NetAmount=i.TyreTotalAmount = i.SubTotal + (view.CalVat ? 0 : l.CGSTAmount + l.SGSTAmount + l.IGSTAmount);
                i.DiscountAmount = i.DiscountPercent = 0;
                i.IsStepney = false;
                i.KmReading = 0;
                i.KmRun = 0;
                i.CalVat = view.CalVat;
                i.RoundUpAmount = l.RoundUpAmount;
                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.TyreId = ir.TyreId;
                i.fk_Tyre = ir.fk_Tyre;
                i.TyreLife = ir.TyreLife;
                i.TyreStatusId = 1223;//ClaimSettlementd
                i.TyreNo = i.fk_Tyre.TyreNo;
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.VoucherDate = view.DocumentDate;
                i.VoucherNo = view.DocumentNo;
                i.fk_Tyre.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Tyre.ObjectState = ObjectState.Modified;
                i.fk_Tyre.S_VoucherDate = i.VoucherDate;
                i.fk_Tyre.S_CreditAccountId = i.CreditAccountId;
                i.fk_Tyre.S_DebitAccountId = i.DebitAccountId;
                i.fk_Tyre.S_Life = i.TyreLife;
                i.fk_Tyre.S_StatusId = i.TyreStatusId;//ClaimSettlementd
                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                try
                {
                    if (!string.IsNullOrWhiteSpace(l.RowVersionId))
                    {
                        i.RowVersion = Encoding.UTF8.GetBytes(l.RowVersionId);
                    }
                }
                catch (Exception exz)
                {
                    //ignore
                }
                if (i.Id > 0)
                {
                    i.ObjectState = ObjectState.Modified;
                    i.fk_Tyre.S_TyreLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Tyre.fk_S_TyreLog = i;
                }
                newTyreLogList.Add(i);
                #endregion
            }
            var tyreRepo = _uom.RepositoryAsync<TyreMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newTyreLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingTyreLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Tyre Log Information that has been referenced/issued.{Environment.NewLine}Ref Tyre Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.TyreNo + "=>" + x.fk_NextLog.VoucherNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    _repository.Update(RestorePreviousTyreStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldTyreLogs.Any())
                {
                    foreach (var log in oldTyreLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Tyre.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        tyreRepo.Update(log.fk_Tyre);
                    }
                }
            }
            #region Prepare Issue Voucher
            var billamount = view.TyreClaimSettlementLog.Sum(x => x.TyreCost)+ view.OtherAmount+ view.CGSTAmount+ view.SGSTAmount+view.SGSTAmount+view.RoundOffAmt-view.PostDiscountAmt+view.TCSAmount;
            
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.ConstCurTypeId = view.ConstCurTypeId;

            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.Value;
            v.VoucherNo = view.DocumentNo;
            v.Amount1 =view.PrimaryDebitAmount.Dr();
            v.Account1Id = view.PrimaryDebitAccountId;/*TyreVendorAc Dr*/
            v.Account2Id = view.PrimaryCreditAccountId;/*ExpenseA/c Cr*/
            v.Amount2 = view.PrimaryCreditAmount.Cr();
            v.Account3Id = view.CGSTLedgerId;
            v.Amount3 = view.CGSTAmount.Cr();
            v.Account4Id = view.OtherLedgerId;
            v.Amount4 = view.OtherAmount.Cr();
            v.Account5Id = view.SGSTLedgerId;
            v.Amount5 = view.SGSTAmount.Cr();
            v.Account6Id = view.IGSTLedgerId;
            v.Amount6 = view.IGSTAmount.Cr();
            v.Account7Id = view.TCSAccountId;
            v.Amount7 = view.TCSAmount.Cr();
            v.Account8Id = view.RoundOffAccId;
            v.Amount8 = view.RoundOffAmt.Reverse();
            v.Account9Id = view.PostDiscountAcId;
            v.Amount9 = view.PostDiscountAmt.Cr();
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";

            PrepareVoucherDetails(_repository, v);
            #endregion
            #region Validations
            var vdrrequired =
                _repository.GetRepository<VoucherType>()
                    .Queryable()
                    .Where(x => x.Id == view.VoucherTypeId)
                    .Select(x => new
                    {
                        x.VDRRequired,
                        x.VDRequired
                    })
                    .FirstOrDefault();
            if (vdrrequired != null)
            {
                if (vdrrequired.VDRequired > 0 && v.VoucherDetails.Count(x => x.ObjectState != ObjectState.Deleted) < vdrrequired.VDRequired)
                {
                    throw new BusinessException(ErrorCode.VCH105);//Atleast two Voucher Details are required in Voucher
                }

                if (vdrrequired.VDRRequired > 0 && !(v.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState != ObjectState.Deleted) >= vdrrequired.VDRRequired))
                {
                    throw new BusinessException(ErrorCode.VCH111, "At least one VDR is Required in SpareLog Transaction");//Atlead one VDR is Required in SpareLog Transaction
                }
            }
            #endregion
            tei = tei ?? new TyreLogExtraInfo();
            tei.fk_Voucher = v;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherId = v.Id;
            tei.VoucherDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.VoucherNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.RoundOffAccId = view.RoundOffAccId;
            tei.RoundOffAmt = view.RoundOffAmt;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.TCSAccountId = view.TCSAccountId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (view.JsonData != null)
            {
                foreach (var entity in view.JsonData)
                {
                    tei.DeleteAndAdd(entity);
                }
            }
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newTyreLogList)
            {
                log.VoucherId = v.Id;
                log.fk_Voucher = v;
                log.fk_Tyre.S_VoucherId = v.Id;
                log.fk_Tyre.S_VoucherDate = v.VoucherDate;
                log.fk_Tyre.fk_S_Voucher = v;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    tyreRepo.Update(log.fk_Tyre);
                }
                else
                {
                    _repository.Insert(log);
                    tyreRepo.Insert(log.fk_Tyre);
                }
            }
            _uom.SaveChanges();
            return tei;
        }

        public IQueryable<TyreLog> GetReportData(string classIds, string accountIds, long categoryId,
            string ledgerFilterType)
        {
           return this._repository.GetReportData(classIds, accountIds, categoryId, ledgerFilterType);
        }
    }
}
