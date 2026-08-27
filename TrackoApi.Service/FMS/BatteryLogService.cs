using EntityFramework.Extensions;

using MoreLinq;

using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;
using TrackoAPI.ViewModels.FMS.Battery;

namespace TrackoApi.Service
{
    public interface IBatteryLogService : IService<BatteryLog>
    {
        IQueryable<BatteryLog> GetAllBatteryLogList(int id);
        vwBatteryBillView GetPurchaseBillView(long id, long type);
        vwBatteryChassisBill GetChassisBillView(long key);
        vwBatteryBillView GetBatteryResaleBill(long key);
        vwBatteryBillView GetBatteryClaimBillView(long key);
        vwBatteryBillView GetBatteryScrapBillView(long key);
        vwBatteryBillView GetBatteryStoretransferOutBillView(long key);
        vwBatteryBillView GetBatteryStoretransferInBillView(long key);
        vwBatteryBillView GetBatteryRejectBillView(long key);
        vwBatteryBillView GetBatteryRefurbishReceiptBillView(long key);

        void InsertOrUpdatePurchaseBillView(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        void InsertOrUpdatePurchaseBillMRNView(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        void InsertOrUpdatePurchaseBillMRNSettlementView(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        BatteryLogExtraInfo InsertUpdateChasisBatteryBill(vwBatteryChassisBill view, IUnitOfWorkAsync _uom);
        void InsertUpdateBatteryIR(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        void InsertUpdateBatteryReSale(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        void InsertUpdateBatteryClaim(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        void DeleteGraph(long key, IUnitOfWorkAsync uow);
        void InsertUpdateBatteryScrap(vwBatteryBillView bill, IUnitOfWorkAsync unitOfWorkAsync);
        void InsertUpdateBatteryStocktransferOutBillView(vwBatteryBillView bill, IUnitOfWorkAsync unitOfWorkAsync);
        void InsertUpdateBatteryStocktransferInBillView(vwBatteryBillView bill, IUnitOfWorkAsync unitOfWorkAsync);
        void InsertUpdateBatteryReject(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        void InsertUpdateBatteryClaimReceiptBillView(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        void InsertUpdateBatteryRefurbishReceipt(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        void InsertUpdateBatteryClaimSettlement(vwBatteryBillView view, IUnitOfWorkAsync _uom);
        void InsertUpdateReceipt(vwBatteryBillView view, IUnitOfWorkAsync uom);
        void InsertUpdateIssue(vwBatteryBillView view, IUnitOfWorkAsync uom);
    }
    
    public class BatteryLogService : Service<BatteryLog>, IBatteryLogService
    {
        private readonly IRepositoryAsync<BatteryLog> _repository;
        public BatteryLogService(IRepositoryAsync<BatteryLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<BatteryLog> GetAllBatteryLogList(int brandid)
        {
            return _repository.GetAllBatteryLogList(brandid);
        }

        public vwBatteryBillView GetPurchaseBillView(long id, long type)
        {
            return _repository.GeBatteryBillPurchaseView(id, type);
        }

        public vwBatteryChassisBill GetChassisBillView(long key)
        {
            return _repository.GetChassisBillView(key);
        }
        public vwBatteryBillView GetBatteryClaimBillView(long key)
        {
            return _repository.GetBatteryClaimBillView(key);
        }
        public void InsertUpdateBatteryReSale(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            if (view.ResaleLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
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
            var teiRepo = _uom.RepositoryAsync<BatteryLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            Voucher v = new Voucher();
            //Collect Distincs ReferenceId's from Posted SpareLogs
            BatteryLogExtraInfo tei=null;
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == 54);
                if(tei==null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = vRepo.Query(x => x.Id == tei.VoucherId&&x.VoucherTypeId==tei.VoucherTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (v == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var issuerefids = view.ResaleLog.Where(x=>x.ReferenceId>0).Select(x => x.ReferenceId).ToList();
            var newBatterystatus = new long[] { 1202 };
            List<BatteryLog> issueReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => issuerefids.Contains(x.Id)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var oldBatteryLogs = new List<BatteryLog>();
            foreach (var l in view.ResaleLog)
            {
               /************************************************************
               *************||Battery Issue Logics Start||*********************
               *************************************************************/
                #region Battery Issue Logic
                var i = new BatteryLog();//Issued Log
                var ir = issueReferenceLogs.Find(x => x.Id == l.ReferenceId);//Issue Reference Log
                if (ir==null ||ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Battery No {l.BatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.BatterySerialNo}");
                }
                if (!newBatterystatus.Contains(ir.BatteryStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {l.BatterySerialNo} can't be resaled");
                }
                if (l.Id > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.Id);//Issued Log
                    if (i.NextLogId >0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Battery has been altered restore all Battery status to previous logs status
                    //oldBatteryLogs.Add(RestorePreviousBatteryStatus(i));
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                }
                i.TSLId = l.TSLId;
                i.Rate = i.SubTotal = l.PurchaseAmount;
                i.OtherAmount = l.OtherAmt;
                i.NetAmount = l.NetValue;
                i.DiscountAmount = i.DiscountPercent = 0;
                i.BatteryAge = 0;
                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.BatteryId = ir.BatteryId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = ir.BatteryLife;
                i.BatteryStatusId = 1209;//Resaled
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;
                i.VoucherTypeId = 54;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = ir.BatteryLife;
                i.fk_Battery.S_StatusId = 1209;//Resaled
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
                    i.fk_Battery.S_BatteryLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newBatteryLogList.Add(i);
                #endregion
            }
            var BatteryRepo = _uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0)>0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }
            #region Prepare Issue Voucher
            var totalStoreCreditAmt = -issueReferenceLogs.Sum(x => x.NetAmount);
            var totalIncomeCredit = -view.ResaleLog.Sum(x => x.OtherAmt);
            var totalVendorAmt = view.ResaleLog.Sum(x => x.NetValue);

            v.ConstCurTypeId = view.ConstCurTypeId;
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = 54;
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount;
            v.Account1Id = view.PrimaryDebitAccountId;
            v.Account2Id = view.PrimaryCreditAccountId;
            v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
            //v.Account3Id = view.OtherLedgerId;
            //v.Amount3 = view.OtherAmount;
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";
            
            if (v.Amount1 != totalVendorAmt)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Battery Total Net Value {totalVendorAmt} Does't match Voucher Primary Debit Amount {v.Amount1}");
            }
            if (v.Amount2 != totalStoreCreditAmt)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Credit Amount {totalStoreCreditAmt} Does't match Voucher Primary Credit Amount {v.Amount2}");
            }
            if (totalIncomeCredit!=0&&(v.Amount3 != totalIncomeCredit))
            {
                throw new BusinessException(ErrorCode.GLB106, $"Income Total Amount {totalIncomeCredit} Does't match Voucher Primary Credit Amount {v.Amount3}");
            }
            PrepareVoucherDetails(_repository, v);
            #endregion
            #region Validations
            var vdrrequired =
                _repository.GetRepository<VoucherType>()
                    .Queryable()
                    .Where(x => x.Id == 54)
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
            tei = tei ?? new BatteryLogExtraInfo();
            tei.fk_Voucher = v;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherId = v.Id;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            //tei.TdSAccountId = view.TdSAccountId;
            //tei.TdSAmount = view.TdSAmount;
            //tei.TdSRate = view.TdSRate;
            tei.VoucherTypeId = 54;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newBatteryLogList)
            {
                log.VoucherId = v.Id;
                log.fk_Voucher = v;
                log.fk_Battery.S_ExtraInfoId = tei.Id;
                log.fk_Battery.S_DocDate = tei.DocDate;
                log.fk_Battery.fk_S_ExtraInfo  = tei;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            _uom.SaveChanges();
        }

        public void InsertUpdateBatteryClaim(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            if (view.ClaimLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Account {view.PrimaryCreditAccountId} is required.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Battery Vendor Account {view.PrimaryDebitAccountId} is required");
            }
            
            var teiRepo = _uom.RepositoryAsync<BatteryLogExtraInfo>();
            

            //Collect Distincs ReferenceId's from Posted SpareLogs
            BatteryLogExtraInfo tei = null;
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var claimrefids = view.ClaimLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var allowedStatus = view.VoucherTypeId == 55 ? new long[] { 1203 } : new long[] { 1203, 1202 };
            List<BatteryLog> claimReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => claimrefids.Contains(x.Id)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var oldBatteryLogs = new List<BatteryLog>();
            foreach (var l in view.ClaimLog)
            {
                /************************************************************
                *************||Battery claim Logics Start||*********************
                *************************************************************/
                #region Battery claim Logic
                var i = new BatteryLog();//claim Log
                var ir = claimReferenceLogs.Find(x => x.Id == l.ReferenceId);//Issue Reference Log

                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Battery No {l.BatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.BatterySerialNo}");
                }
                if (!allowedStatus.Contains(ir.BatteryStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {l.BatterySerialNo} can't be send for claim / remould");
                }
                if (l.Id > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.Id);//claim Log
                    if (i.NextLogId >0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }

                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Battery has been altered restore all Battery status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                    //oldBatteryLogs.Add(RestorePreviousBatteryStatus(i));
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }

                //55 Sent for Retreating
                //56 Sent for Claim
                //1207 SFR Sent For Remould
                //1208 SFC Sent For Claim
                i.TSLId = l.TSLId;
                i.Rate =
                    i.SubTotal =
                        i.OtherAmount = i.NetAmount = i.DiscountAmount = i.DiscountPercent = 0;
                        i.BatteryAge = 0;

                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.BatteryId = ir.BatteryId;
               i.ReasonId = ir.ReasonId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = ir.BatteryLife;
                i.BatteryStatusId = (view.VoucherTypeId == 55 ? 1207 : 1208);
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;

                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = ir.BatteryLife;
                i.fk_Battery.S_StatusId = i.BatteryStatusId;

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
                    i.fk_Battery.S_BatteryLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newBatteryLogList.Add(i);
                #endregion
            }
            var BatteryRepo = _uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }


            tei = tei ?? new BatteryLogExtraInfo();
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.TCSRate = view.TCSRate;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSAccountId = view.TCSAccountId;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            //tei.TdSRate = view.TdSRate;
            //tei.TdSAmount = view.TdSAmount;
            //tei.TdSAccountId = view.TdSAccountId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newBatteryLogList)
            {
                log.fk_Battery.S_DocDate = tei.DocDate;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            _uom.SaveChanges();
        }

        public void InsertUpdateBatteryIR(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            if (view.IssueReceiptLogs.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
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

            #region Check Circuler Reference for Batterys
            //var duplicatecheckgroup = view.IssueReceiptLogs.GroupBy(x => new { x.IssueBatteryId, x.ReceiptBatteryId });
            if (view.IssueReceiptLogs.GroupBy(x => new { x.IssueBatteryId, x.ReceiptBatteryId }).ToList().Any(x => x.Count() > 1))
            {
                throw new BusinessException(ErrorCode.GLB106, "Same Battery can't be issued against it's receipt.");
            }
            var groupbyvehicle = view.IssueReceiptLogs.GroupBy(x => x.VehicleId).ToList();
            foreach (IGrouping<long, vwBatteryIssueReceipt> grouping in groupbyvehicle)
            {
                var issuelist = grouping.Select(x => x.IssueBatteryId).ToList();
                var receiptlist = grouping.Select(x => x.ReceiptBatteryId).ToList();
                if (issuelist.TrueForAll(x => receiptlist.Contains(x)))
                {
                    throw new BusinessException(ErrorCode.GLB106, "Both receipt and issue operation can't be done in same transaction for same Battery.");
                }
                
                if (issuelist.GroupBy(x=>x).Any(x=>x.Count()>1))
                {
                    throw new BusinessException(ErrorCode.GLB106, "Same Battery can't be issued more than one in Single Transaction.");
                }
                if (receiptlist.GroupBy(x => x).Any(x => x.Count() > 1))
                {
                    throw new BusinessException(ErrorCode.GLB106, "Same Battery can't be received more than one in Single Transaction.");
                }
            }
            #endregion
            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<BatteryLogExtraInfo>();
            var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            var BatteryCheckRepo = _uom.RepositoryAsync<BatteryCheck>();
            BatteryLogExtraInfo tei =new BatteryLogExtraInfo();
            if (view.Id > 0)
            {//Try to find existing Battery extra info record
                tei = teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && (x.VoucherTypeId == 51 || x.VoucherTypeId == 50));
            }
            if (view.Id > 0&& tei !=null&& vRepo.Queryable().Any(x => x.Id==tei.VoucherId && (x.VoucherTypeId == 51 || x.VoucherTypeId == 50)))
            {
                //Try to find existing voucher record
                v = vRepo.Queryable().Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).FirstOrDefault(x=> x.Id == tei.VoucherId && (x.VoucherTypeId == 51 || x.VoucherTypeId == 50));
            }
            List<BatteryLog> existingBatteryLogs=new List<BatteryLog>();
            if (tei != null&&tei.Id>0)
            {
                //In-case updating existing record find all existing attached Battery Logs
                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x=>x.fk_BatteryCheck).Include(x => x.fk_PreviousLog).Include(x => x.fk_NextLog).Where(x => (x.ExtraInfoId == tei.Id) && (x.VoucherTypeId == 51 || x.VoucherTypeId == 50)).ToList();
            }
            //Extract Ids of Primary Key
            var oldissueids = existingBatteryLogs.Where(x=>x.VoucherTypeId== 50&&x.Id>0).Select(x => x.Id).ToList();
            var oldreceiptids = existingBatteryLogs.Where(x => x.VoucherTypeId == 51 && x.Id > 0).Select(x => x.Id).ToList();

           
            var issuerefids = view.IssueReceiptLogs.Select(x => x.IssueReferenceId).ToList();
            var receptrefids = view.IssueReceiptLogs.Select(x => x.ReceiptReferenceId).ToList();

            //Fatch Battery Performance Logs in case updating record
            var issueBatteryPerformance = tpiRepo.Queryable().Where(x => oldissueids.Contains(x.FirstIssueLogId.Value)).ToList();
            var receiptBatteryPerformance = tpiRepo.Queryable().Where(x => oldreceiptids.Contains(x.LastReceiptLogId.Value)).ToList();

            List<BatteryLog> issueReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => issuerefids.Contains(x.Id)).ToList();
            List<BatteryLog> receiptReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => receptrefids.Contains(x.Id)).ToList();

            //Fatch Battery Performance Logs for fresh receipt so that we could update LastReceiptLogId
            var receiptLogBatteryPerformanceIds = receiptReferenceLogs.Select(x => x.BatteryId + "-" + x.BatteryLife).ToList();
            //TODO:Check if It works
            var receiptTpData = tpiRepo.Queryable().Where(x => receiptLogBatteryPerformanceIds.Contains(x.BatteryId + "-" + x.Life)).ToList();

            var newBatteryvtypes = new long[] { 43, 45, 48, 57 };
            var issueNetamount = issueReferenceLogs.Where(x=> newBatteryvtypes.Contains(x.VoucherTypeId)).Sum(x => x.NetAmount);
            
            var cv = issueReferenceLogs.Any(x => newBatteryvtypes.Contains(x.VoucherTypeId) && x.NetAmount > 0);
            if (cv)
            {
                v = v ?? new Voucher();
            }
            else
            {
                if (v != null && v.Id > 0)
                {
                    v.ObjectState=ObjectState.Deleted;
                    foreach (var x in v.VoucherDetails)
                    {
                        x.ObjectState = ObjectState.Deleted;
                        foreach (var y in x.VoucherDetailReferences) y.ObjectState=ObjectState.Deleted;
                    }
                    vRepo.Delete(v);
                }
            }
            var oldBatteryLogs = new List<BatteryLog>();
            var newIssuedLogs = new List<BatteryLog>();
            var newReceiptLogs = new List<BatteryLog>();
            var newBatteryPerformance = new List<BatteryLifePerformanceLog>();
            foreach (var l in view.IssueReceiptLogs)
            {
                /************************************************************
                *************||Battery Issue Logics Start||*********************
                *************************************************************/
                #region Battery Issue Logic

                var i = new BatteryLog();//Issued Log
                var ir = issueReferenceLogs.Find(x => x.Id == l.IssueReferenceId);//Issue Reference Log
                
                if (ir==null||ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log didn't found for Battery No {l.IssueBatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.IssueBatterySerialNo}");
                }
                if (l.IssueLogId > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.IssueLogId);//Issued Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.IssueBatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //var tp = issueBatteryPerformance.FirstOrDefault(x => x.FirstIssueLogId == i.Id);
                    //tp.ObjectState=ObjectState.Deleted;
                    //tpiRepo.Delete(tp);
                    //if Battery has been altered restore all Battery status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                }
                i.TSLId = l.TSLId;
                i.NetAmount = i.Rate = i.SubTotal = l.IssueAmount;
                i.DiscountAmount = i.OtherAmount = i.DiscountPercent = 0;
               
                i.JobsheetId = l.JobSheetId;
               
                i.BatteryAge = 0;
                i.MechanicId = l.MechanicId;
                i.CreditAccountId = ir.DebitAccountId;//view.PrimaryDebitAccountId.GetValueOrDefault(0);
                i.DebitAccountId = view.PrimaryCreditAccountId;
                i.Remark = l.IssueRemark;
                i.BatteryId = ir.BatteryId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = ir.BatteryLife;
                i.BatteryStatusId = 1205;//OnVehicle
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;
                i.VoucherTypeId = 50;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.VehicleId = l.VehicleId;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = ir.BatteryLife;
                i.fk_Battery.S_StatusId = 1205;//OnVehicle
                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                if (!string.IsNullOrWhiteSpace(l.IssueRowVersionId))
                {
                    i.RowVersion = Encoding.UTF8.GetBytes(l.IssueRowVersionId);
                }
                #region BatteryCheck Issue
                if (i.fk_BatteryCheck == null || i.fk_BatteryCheck.Id == 0)
                {
                    i.fk_BatteryCheck = new BatteryCheck();
                }
                i.fk_BatteryCheck.GravityLevel = l.GravityLevel;
                i.fk_BatteryCheck.CheckDate = view.DocumentDate;
                i.fk_BatteryCheck.Remarks = l.IssueRemark;
                i.fk_BatteryCheck.IsWaterLevelChecked = l.IsWaterLevelChecked;
                i.fk_BatteryCheck.IsTerminalCarbonChecked = l.IsTerminalCarbonChecked;
                i.fk_BatteryCheck.BatteryId = i.BatteryId;
                i.fk_BatteryCheck.VehicleId = i.VehicleId.Value;
                i.fk_BatteryCheck.fk_Battery = i.fk_Battery;
                i.fk_BatteryCheck.ObjectState = i.fk_BatteryCheck.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                #endregion
                if (i.Id > 0)
                {
                    
                    i.ObjectState = ObjectState.Modified;
                    i.fk_Battery.S_BatteryLogId = i.Id;
                   
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newIssuedLogs.Add(i);
                #endregion

                /************************************************************
                *************||Battery Receipt Logics Start||*******************
                *************************************************************/
                #region Battery Receipt Logic
                var r = new BatteryLog();
                var rr = receiptReferenceLogs.Find(x => x.Id == l.ReceiptReferenceId);
                
                if (rr==null||ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Battery No {l.IssueBatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.IssueBatterySerialNo}");
                }
                if (l.ReceiptLogId > 0)
                {
                    r = existingBatteryLogs.Find(x => x.Id == l.ReceiptLogId);
                    if (r.NextLogId >0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.ReceiptBatterySerialNo}[Referenced Transaction No :{r.fk_NextLog.DocNo}]");
                    }
                }
                if (rr != null && rr.NextLogId > 0 &&r.Id==0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Invalid Reference for Battery No {rr.BatterySerialNo}.");
                }
                //Check if Battery No has been altered
                if (r.Id > 0 && r.PreviousLogId != rr.Id)
                {
                    //Restore Last Receipt Log Id when Receipt Battery No Changed
                    var td = receiptBatteryPerformance.FirstOrDefault(x => x.LastReceiptLogId == r.Id);
                    if (td != default(BatteryLifePerformanceLog))
                    {
                        var lastReceipt = _repository.GetLastBatteryLogByStatusAndLife(r.BatteryId, new long[] { 51 }, r.BatteryLife, r.Id);
                        td.LastReceiptLogId = lastReceipt?.Id;
                        td.ObjectState = ObjectState.Modified;
                        tpiRepo.Update(td);
                    }
                    //if Battery has been altered restore all Battery status to previous logs status
                    //oldBatteryLogs.Add(RestorePreviousBatteryStatus(r));
                    //r.fk_Battery = null;
                    //r.BatteryId = 0;
                    //r.fk_PreviousLog = null;
                    //r.PreviousLogId = null;
                }
                r.NetAmount = r.Rate = r.SubTotal = l.ReceiptAmount;
                r.DiscountAmount = r.OtherAmount = r.DiscountPercent = 0;
                r.IsRefurbish = rr.IsRefurbish;
                r.JobsheetId = l.JobSheetId;
                r.BatteryAge = 0;//l.ReceiptOutKm - rr.KmReading;//Calculate Difference
                r.MechanicId = l.MechanicId;
                r.CreditAccountId = rr.DebitAccountId;//l.OwnerId.GetValueOrDefault(0) == 0 ? rr.CreditAccountId : l.OwnerId.GetValueOrDefault(0);
                r.DebitAccountId = view.PrimaryDebitAccountId.Value;
                r.Remark = l.ReceiptRemark;
                r.BatteryId = l.ReceiptBatteryId;
                r.fk_Battery = rr.fk_Battery;
                r.ReasonId = l.ReasonId;
                r.NextUseId = l.NextUseId;
                r.BatteryLife = rr.BatteryLife;
                r.BatteryStatusId = 1203;
                r.BatterySerialNo = rr.fk_Battery.BatterySerialNo;
                r.VoucherTypeId = 51;
                r.DocDate = view.DocumentDate;
                r.DocNo = view.DocumentNo;
                r.VehicleId = l.VehicleId;
                r.NextUseId = l.NextUseId;
                r.fk_Battery.S_VoucherTypeId = r.VoucherTypeId;
                r.fk_Battery.ObjectState = ObjectState.Modified;
                r.fk_Battery.S_DocDate = r.DocDate;
                r.fk_Battery.S_CreditAccountId = r.CreditAccountId;
                r.fk_Battery.S_DebitAccountId = r.DebitAccountId;
                r.fk_Battery.S_Life = r.BatteryLife;
                r.fk_Battery.S_StatusId = r.BatteryStatusId;
                r.PreviousLogId = rr.Id;
                r.fk_PreviousLog = rr;
                //Set Issue Receipt Entry in Cross
                i.fk_IssueReceipt = r;
                if(r.Id>0)i.IssueReceiptId = r.Id;
                if (r.IssueReceiptId.GetValueOrDefault(0) == 0)
                {
                    r.fk_IssueReceipt = null;
                    r.IssueReceiptId = null;
                }
                if (!string.IsNullOrWhiteSpace(l.ReceiptRowVersionId))
                {
                    r.RowVersion = Encoding.UTF8.GetBytes(l.ReceiptRowVersionId);
                }
                //r.PreviousLogId = ir.Id;
                rr.NextLogId = r.Id;
                rr.fk_NextLog = r;
                #region BatteryCheck Receipt
                if (r.fk_BatteryCheck == null || r.fk_BatteryCheck.Id == 0)
                {
                    r.fk_BatteryCheck = new BatteryCheck();
                }
                r.fk_BatteryCheck.CheckDate = view.DocumentDate;
                r.fk_BatteryCheck.Remarks = l.ReceiptRemark;
                r.fk_BatteryCheck.GravityLevel = l.GravityLevel;
                r.fk_BatteryCheck.BatteryId = r.BatteryId;
                r.fk_BatteryCheck.VehicleId = r.VehicleId.Value;
                r.fk_BatteryCheck.IsTerminalCarbonChecked = l.IsTerminalCarbonChecked;
                r.fk_BatteryCheck.IsWaterLevelChecked = l.IsWaterLevelChecked;
                r.fk_BatteryCheck.fk_Battery = r.fk_Battery;
                r.fk_BatteryCheck.ObjectState = r.fk_BatteryCheck.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                #endregion
                if (r.Id > 0)
                {
                    r.ObjectState = ObjectState.Modified;
                    r.fk_Battery.S_BatteryLogId = r.Id;
                    
                }
                else
                {
                    r.ObjectState = ObjectState.Added;
                    r.fk_Battery.fk_S_BatteryLog = r;
                }
                newReceiptLogs.Add(r);
                #endregion
                
            }
            var BatteryRepo = _uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                
                
                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }
            if (cv)
            {

                #region Prepare Issue Voucher
                v.ConstCurTypeId = view.ConstCurTypeId;
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
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
                v.Account5Id = view.SGSTLedgerId;
                v.Amount5 = view.SGSTAmount;
                v.Account6Id = view.IGSTLedgerId;
                v.Amount6 = view.IGSTAmount;
                //v.Account4Id = view.OtherLedgerId;
                //v.Amount4 = view.OtherAmount;
                v.UserRemark = view.Narration;
                v.OfficeId = view.OfficeId;
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //TODO:Setup Account Narration from Template located with VoucherType
                v.AccountingRemark = "";
                
                if (v.Amount1 != issueNetamount)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery Total Net Value {issueNetamount} Does't match Voucher Primary Debit Amount {v.Amount1}");
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
                        .Where(x => x.Id == 50)
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
            tei = tei ?? new BatteryLogExtraInfo();
            tei.fk_Voucher = v;
            tei.OfficeId = view.OfficeId;
            tei.CalVat = view.CalVat;
            //tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherId = v.Id;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.Remark = view.Narration;
            tei.VoucherTypeId = 50;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newIssuedLogs)
            {
                if (cv)
                {
                    log.VoucherId = v?.Id;
                    log.fk_Voucher = v;
                    log.fk_Battery.S_ExtraInfoId = tei.Id;
                    log.fk_Battery.S_DocDate = tei.DocDate;
                    log.fk_Battery.fk_S_ExtraInfo = tei;
                    
                }
                //Only Create Battery Performance in case Battery is issued first time
                if (log.fk_PreviousLog.BatteryStatusId == 1202)
                {
                    var tpi = issueBatteryPerformance.FirstOrDefault(x => x.FirstIssueLogId == log.Id) ?? new BatteryLifePerformanceLog();
                    if (tpi.Id == 0) tpi.FirstIssueLogId = log.Id;
                    tpi.CurrentAge = 0;
                    tpi.Life = 0;
                    tpi.LifeAge = 0;
                    tpi.LifeStartDate = log.DocDate;
                    tpi.PreviousAge = 0;
                    tpi.PurchaseAmount = log.NetAmount;
                    tpi.SupplierId = log.DebitAccountId;
                    tpi.LifeEndDate = null;
                    tpi.fk_FirstIssueLog = log;
                    tpi.ObjectState = tpi.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    newBatteryPerformance.Add(tpi);
                }
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                log.ObjectState=log.Id > 0 ? ObjectState.Modified : ObjectState.Added; ;
                _repository.Update(log.fk_PreviousLog);
                if (log.fk_BatteryCheck.Id > 0)
                {
                    BatteryCheckRepo.Update(log.fk_BatteryCheck);
                }
                else
                {
                    BatteryCheckRepo.Insert(log.fk_BatteryCheck);
                }
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            foreach (var log in newReceiptLogs)
            {
                if (cv)
                {
                    log.VoucherId = v?.Id;
                    log.fk_Voucher = v;
                    log.fk_Battery.S_ExtraInfoId = tei.Id;
                    log.fk_Battery.S_DocDate = tei.DocDate;
                    log.fk_Battery.fk_S_ExtraInfo = tei;
                }
                //Extract Battery Performance for current record in loop and set LastReceiptLog as this
                var tpd=receiptTpData.FirstOrDefault(x => x.Life == log.BatteryLife && log.BatteryId == x.BatteryId);
                if (tpd != null&&((tpd.LastReceiptLogId.HasValue&& tpd.LastReceiptLogId<log.Id)||log.Id==0))
                {
                    tpd.LastReceiptLogId = log.Id;
                    tpd.fk_LastReceiptLog = log;
                    tpd.ObjectState=ObjectState.Modified;
                    newBatteryPerformance.Add(tpd);

                }
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                log.ObjectState =log.Id > 0 ? ObjectState.Modified : ObjectState.Added; ;
                _repository.Update(log.fk_PreviousLog);
                if (log.fk_BatteryCheck.Id > 0)
                {
                    BatteryCheckRepo.Update(log.fk_BatteryCheck);
                }
                else
                {
                    BatteryCheckRepo.Insert(log.fk_BatteryCheck);
                }
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            _uom.SaveChanges();
            if (view.Id > 0)
            {
                var newLogsIds = newIssuedLogs.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                newLogsIds.AddRange(newReceiptLogs.Where(x => x.Id > 0).Select(x => x.Id));
                newLogsIds = newLogsIds.Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    var td = receiptBatteryPerformance.FirstOrDefault(x => x.LastReceiptLogId == log.Id || x.FirstIssueLogId == log.Id);
                    if (td != default(BatteryLifePerformanceLog))
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
                                throw new BusinessException(ErrorCode.GLB106, "Cannot Delete Battery Performance Log as it is Locked");
                            }
                            //Find Last Log Other than current in loop
                            BatteryLog lstTl = _repository.GetLastBatteryLogByStatusAndLife(log.BatteryId, (td.FirstIssueLogId == log.Id ? new long[]{ 43,95, 45, 48, 57 } : new long[]{ 50 }), log.BatteryLife, log.Id);
                            td.LastReceiptLogId = lstTl?.Id;
                            td.ObjectState = ObjectState.Modified;
                            tpiRepo.Update(td);
                        }
                        
                    }

                    if (log.fk_BatteryCheck!=null&& log.fk_BatteryCheck.Id> 0)
                    {
                        log.fk_BatteryCheck.ObjectState=ObjectState.Deleted;
                        BatteryCheckRepo.Delete(log.fk_BatteryCheck);
                    }

                    log.IssueReceiptId = null;
                    log.fk_IssueReceipt = null;
                    _repository.Update(RestorePreviousBatteryStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }
            }
            _uom.SaveChanges();
            foreach (var log in newIssuedLogs)
            {
                log.fk_IssueReceipt.IssueReceiptId = log.Id;
                log.fk_IssueReceipt.ObjectState=ObjectState.Modified;
                _repository.Update(log.fk_IssueReceipt);
            }
            _uom.SaveChanges();
            var vehids =
                newIssuedLogs.Select(x => x.VehicleId)
                    .ToList();
            vehids.AddRange(newReceiptLogs.Select(x => x.VehicleId));
            vehids = vehids.Distinct().ToList();
            //var modelcount=
            //_repository.Queryable().Count(x=>x.VehicleId==)
            foreach (var log in newBatteryPerformance)
            {
                log.BatteryId = log.fk_FirstIssueLog.BatteryId;
                log.fk_Battery = log.fk_FirstIssueLog.fk_Battery;
                if (log.Id > 0) tpiRepo.Update(log);
                else tpiRepo.Insert(log);
            }
            _uom.SaveChanges();
        }
        private BatteryLog RestorePreviousBatteryStatus(BatteryLog current)
        {
            //if Battery has been altered restore all Battery status to previous logs status
            if (current.VoucherTypeId == 58) return current;
            var p = current.fk_PreviousLog;
            
            if (p == null)
            {
                _repository.Queryable().Include(x => x.fk_Battery).Where(x => x.Id == current.PreviousLogId).Load();
                p = current.fk_PreviousLog;
                if (p == null)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Unable to Restore Battery Status.");
                }
            }
            if (p.ObjectState != ObjectState.Deleted)
            {
                p.NextLogId = null;
                p.fk_NextLog = null;
                p.ScrapCost = 0;//added by sanjay

                p.fk_Battery.S_StatusId = p.BatteryStatusId;
                p.fk_Battery.S_DebitAccountId = p.DebitAccountId;
                p.fk_Battery.S_BatteryLogId = p.Id;
                p.fk_Battery.S_DocDate = p.DocDate;
                p.fk_Battery.S_ExtraInfoId = p.ExtraInfoId;
                p.fk_Battery.S_Life = p.BatteryLife;
                p.fk_Battery.S_VoucherTypeId = p.VoucherTypeId;

                p.fk_Battery.fk_S_BatteryLog = null;
                p.fk_Battery.fk_S_OtherAccount = null;
                p.fk_Battery.fk_S_Status = null;
                p.fk_Battery.fk_S_DebitAccount = null;
                p.fk_Battery.fk_S_ExtraInfo = null;
                p.fk_Battery.fk_S_VoucherType = null;

                current.PreviousLogId = null;
                current.fk_PreviousLog = null;
            
                p.ObjectState = ObjectState.Modified;
                p.fk_Battery.ObjectState = ObjectState.Modified;
            }
            return p;
        }

        public BatteryLogExtraInfo InsertUpdateChasisBatteryBill(vwBatteryChassisBill view, IUnitOfWorkAsync _uom)
        {
            if (view.BatteryLogs.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
            }
            //view.PrimaryDebitAccountId consider as VehicleId
            if (string.IsNullOrWhiteSpace(view.DocumentNumber))
            {
                throw new BusinessException(ErrorCode.GLB106, "Document Number is Required");
            }
            var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            var oldBatteryLogList= _repository.Queryable().Include(x=>x.fk_Battery).Where(x => x.ExtraInfoId == view.Id&&x.VoucherTypeId== 58).ToList();
            var oldtpiIdlist = oldBatteryLogList.Select(x => x.Id).ToList();
            var oldBatteryPerformance = tpiRepo.Queryable().Where(x => oldtpiIdlist.Contains(x.FirstIssueLogId.Value)).ToList();
            //var newlogList=new List<BatteryLog>();
            var newBatteryPerformance = new List<BatteryLifePerformanceLog>();
            var newissuelogs=new List<BatteryLog>();
            foreach (var log in view.BatteryLogs)
            {
                var i=log.Id>0?oldBatteryLogList.FirstOrDefault(x=>x.Id== log.Id) :new BatteryLog();
                if (i==null)
                {
                    throw new BusinessException(ErrorCode.GLB106,"One of Battery Log Transaction didn't found for update");
                }
                var tpi = oldBatteryPerformance.FirstOrDefault(x => x.FirstIssueLogId == i.Id) ?? new BatteryLifePerformanceLog();
                if (i.NextLogId>0)
                {
                    i.ObjectState = ObjectState.Unchanged;
                    newissuelogs.Add(i);
                    if (tpi == null)
                    {
                        if (tpi.Id == 0) tpi.FirstIssueLogId = i.Id;
                        tpi.CurrentAge = 0;
                        tpi.Life = 0;
                        tpi.LifeAge = 0;
                        tpi.LifeStartDate = log.LogDate ?? view.IssueDate;
                        tpi.PreviousAge = 0;
                        tpi.PurchaseAmount = i.NetAmount;
                        tpi.SupplierId = i.DebitAccountId;
                        tpi.LifeEndDate = null;
                        tpi.fk_FirstIssueLog = i;
                    }
                    tpi.ObjectState = tpi.Id > 0 ? ObjectState.Unchanged : ObjectState.Added;
                    newBatteryPerformance.Add(tpi);
                    continue;
                    //if (i.BatterySerialNo != log.BatterySerialNo || i.BatteryId != log.BatteryId)
                    //{

                    //throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Battery No:{log.BatterySerialNo}");
                    //}
                }

                var duplicateBattery =
                    _repository.Queryable().FirstOrDefault(x => x.BatterySerialNo == log.BatterySerialNo && x.BatteryId != log.BatteryId && (x.VoucherTypeId == 43|| x.VoucherTypeId == 95 || x.VoucherTypeId == 48 || x.VoucherTypeId == 58));
                if (duplicateBattery != null)
                {
                    throw new BusinessException(ErrorCode.GLB105, $"Battery No [  { log.BatterySerialNo }  ] already Exists");
                }

                i.CreditAccountId = i.DebitAccountId = view.StoreId;
                if (log.OwnerId.HasValue&&log.OwnerId > 0)
                {
                    i.DebitAccountId = log.OwnerId.Value;
                }
                i.ObjectState = i.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                #region Battery Issue Code
                i.TSLId = log.TSLId;
                i.VehicleId = log.VehicleId;
                i.VoucherTypeId = 58;
                
                i.BatteryLife = 0;
                i.BatterySerialNo = log.BatterySerialNo;
               
                i.BatteryAge = 0;
                i.BatteryStatusId = 1205;
                i.Remark = log.Remark;
                i.DiscountAmount = i.DiscountPercent = i.CGSTAmount = i.SGSTAmount =i.IGSTAmount = i.CGSTPercent = i.SGSTPercent = i.IGSTPercent = i.OtherAmount = i.ScrapCost = i.TransferPrice = 0;
                i.Rate = i.SubTotal = i.NetAmount = log.NetAmount;
                i.WarrantyDays = log.WarrantyDays;
                i.DocDate = log.LogDate?? view.IssueDate;
                i.DocNo = view.DocumentNumber;
                //if (i.Id == 0) t.fk_NextLog = i;
                
                if (tpi.Id == 0) tpi.FirstIssueLogId = i.Id;
                tpi.CurrentAge = 0;
                tpi.Life = 0;
                tpi.LifeAge = 0;
                tpi.LifeStartDate = log.LogDate ?? view.IssueDate;
                tpi.PreviousAge = 0;
                tpi.PurchaseAmount = i.NetAmount;
                tpi.SupplierId = i.DebitAccountId;
                tpi.LifeEndDate = null;
                tpi.fk_FirstIssueLog = i;
                tpi.ObjectState = tpi.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                newBatteryPerformance.Add(tpi);
                #endregion

                if (i.Id == 0)
                {
                    var Battery = new BatteryMaster()
                    {
                        BrandId = log.BrandId.GetValueOrDefault(0),
                        IsAnalysis = true,
                        ObjectState = ObjectState.Added,
                        OpeningAge = 0,
                        S_Life = 0,
                        S_CreditAccountId = i.CreditAccountId,
                        S_StatusId = 1205,
                        S_DebitAccountId = i.DebitAccountId,
                        S_DocDate = log.LogDate ?? view.IssueDate,
                        BatterySerialNo = i.BatterySerialNo,
                        S_VoucherTypeId = 58,
                    };
                    i.fk_Battery = Battery;
                }
                else
                {
                    i.fk_Battery.BrandId = log.BrandId.GetValueOrDefault(0);
                    i.fk_Battery.IsAnalysis = true;
                    i.fk_Battery.ObjectState = ObjectState.Modified;
                    i.fk_Battery.OpeningAge = 0;
                    i.fk_Battery.S_Life = 0;
                    i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                    i.fk_Battery.S_StatusId = 1205;
                    i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                    i.fk_Battery.S_DocDate = log.LogDate ?? view.IssueDate;
                    i.fk_Battery.BatterySerialNo = i.BatterySerialNo;
                    i.fk_Battery.S_VoucherTypeId = 58;
                }
                //if (t.NextLogId == 0) t.fk_NextLog.fk_Battery = t.fk_Battery;
                newissuelogs.Add(i);
            }
            
            var newLogsIds = newissuelogs.Where(x => x.Id > 0).Select(x => x.Id).ToList();
            var deletedLogs = oldBatteryLogList.Where(x => !newLogsIds.Contains(x.Id)).ToList();
            if (deletedLogs.Any())
            {
                if (deletedLogs.Any(x =>  x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.NextLogId>0).Select(x => x.BatterySerialNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    //RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }
            }
            var newperfLogsIds = newBatteryPerformance.Where(x => x.Id > 0).Select(x => x.Id).ToList();
            var deletedperfLogs = oldBatteryPerformance.Where(x => !newperfLogsIds.Contains(x.Id)).ToList();
            if (deletedperfLogs.Any())
            {
                foreach (var log in deletedperfLogs)
                {
                    log.ObjectState=ObjectState.Deleted;
                    tpiRepo.Delete(log);
                }
            }
            var BatteryRepo = _uom.Repository<BatteryMaster>();
            foreach (var log in newissuelogs)
            {
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    //_repository.Update(log.fk_NextLog);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    //_repository.Insert(log.fk_NextLog);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            
            foreach (var log in newBatteryPerformance)
            {
                log.BatteryId = log.fk_FirstIssueLog.BatteryId;
                log.fk_Battery = log.fk_FirstIssueLog.fk_Battery;
                if (log.Id > 0) tpiRepo.Update(log);
                else tpiRepo.Insert(log);
            }
            
            var teiRepo = _uom.Repository<BatteryLogExtraInfo>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id) ?? new BatteryLogExtraInfo();
            tei.DocNo = view.DocumentNumber;
            tei.PageId = view.PageId;
            tei.VoucherTypeId = 58;
            tei.OfficeId = view.OfficeId;
            tei.DocDate = view.IssueDate;
            tei.CrAccountId = view.StoreId;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;


            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);

            _uom.SaveChanges();

            foreach (var x in newissuelogs)
            {
                x.fk_Battery.PurchaseLogId = x.Id;
                if (x.fk_Battery.S_BatteryLogId.GetValueOrDefault(x.Id) == x.Id)
                {
                    x.fk_Battery.S_BatteryLogId = x.Id;
                }
                x.fk_Battery.ObjectState = ObjectState.Modified;
                x.ObjectState=ObjectState.Modified;
                x.ExtraInfo = tei;
                BatteryRepo.Update(x.fk_Battery);
            }
            _uom.SaveChanges();
            return tei;
        }
        public void InsertOrUpdatePurchaseBillMRNView(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            if (view.Batterys.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
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

            if (view.VoucherTypeId == 136)
            {
                view.ProvisionalAcId = view.PrimaryCreditAccountId;
                view.PrimaryCreditAccountId = InventoryControlAcId;
            }
            var teiRepo = _repository.GetRepository<BatteryLogExtraInfo>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id) ?? new BatteryLogExtraInfo();
            if (tei == default(BatteryLogExtraInfo) && view.Id > 0)
            {
                //Through error if voucher not found with provided vouchertypeid
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }

            Voucher v = new Voucher();
            var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            if (view.Id > 0)
            {
                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = _repository.GetRepository<Voucher>().Query(x => x.Id == tei.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (v == null)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Parent Transaction[Voucher] Not Found");
                }
            }
            #region Battery Log Preparation

            //Collect Distincs ReferenceId's from Posted SpareLogs
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (view.Id > 0)
            {
                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var oldbpiIdlist = existingBatteryLogs.Select(x => x.Id).ToList();
            var oldBatteryPerformance = tpiRepo.Queryable().Where(x => oldbpiIdlist.Contains(x.FirstIssueLogId.Value)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var newBatteryPerformance = new List<BatteryLifePerformanceLog>();
            #region Loop
            foreach (var l in view.Batterys)
            {
                var t = new BatteryLog();
                if (view.Id > 0 && l.Id > 0)
                {
                    t = existingBatteryLogs.Find(x => x.Id == l.Id);
                    if (t.NextLogId != null && t.NextLogId > 0)//&&t.fk_ChildLog.ParentLogId==t.Id)
                    {
                        newBatteryLogList.Add(t);
                        continue;
                        //throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{t.fk_NextLog.DocNo}]");
                    }
                }
                var duplicateBattery =
                    _repository.Queryable().FirstOrDefault(x => x.BatterySerialNo == l.BatterySerialNo && x.BatteryId != l.BatteryId && (x.VoucherTypeId == 43 || x.VoucherTypeId == 95 || x.VoucherTypeId == 48 || x.VoucherTypeId == 58));
                if (duplicateBattery != null)
                {
                    throw new BusinessException(ErrorCode.GLB105, $"Battery No [  {l.BatterySerialNo}  ] already Exists");
                }

                t.OtherAmount = l.OtherAmount;
                t.VehicleId = l.VehicleId;
                t.Remark = l.Remark;
                t.BatteryId = l.BatteryId;
                t.CGSTAmount = l.CGSTAmount;
                t.SGSTAmount = l.SGSTAmount;
                t.IGSTAmount = l.IGSTAmount;
                t.Rate = l.Rate;
                t.SubTotal = l.Rate - l.DiscountAmount + t.OtherAmount;
                t.DiscountAmount = l.DiscountAmount;
                t.DiscountPercent = l.DiscountPercent;
                t.RoundAmount = l.RoundAmount;
                t.NetAmount = l.NetAmount;
                t.POLogId = l.PurchaseOrderId;
                t.TaxServiceTypeId = l.ServiceTaxTypeId;
                t.CGSTPercent = l.CGSTPercent;
                t.SGSTPercent = l.SGSTPercent;
                t.IGSTPercent = l.IGSTPercent;
                t.WarrantyDays = l.WarrantyDays;

                t.DocDate = view.DocumentDate;
                t.DocNo = view.DocumentNo;
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


                #region Voucher Battery
                
                t.IsRefurbish = false;
                t.CreditAccountId = view.PrimaryCreditAccountId;
                t.DebitAccountId = view.PrimaryDebitAccountId.GetValueOrDefault();
                t.IssueReceiptId = null;
                t.JobsheetId = null;
                t.POLogId = l.PurchaseOrderId;

                t.BatteryAge = 0;
                //t.ParentLogId = null;
                t.ReasonId = null;
                t.BatteryLife = 0;
                t.BatteryStatusId = 1202;
                t.BatterySerialNo = l.BatterySerialNo;
                t.CalOthAmt = view.CalOthAmt;
                t.CalVat = view.CalVat;
                if (t.Id == 0)
                {
                    var Battery = new BatteryMaster()
                    {
                        BrandId = l.BrandId.GetValueOrDefault(0),
                        IsAnalysis = true,
                        ObjectState = ObjectState.Added,
                        OpeningAge = 0,

                        //fk_PurchaseBatteryLog = t,
                        //PurchaseExtraInfoId = tei.Id,
                        //fk_PurchaseExtraInfo = tei,
                        S_Life = 0,
                        S_CreditAccountId = t.CreditAccountId,
                        S_StatusId = 1202,
                        S_DebitAccountId = t.DebitAccountId,
                        S_DocDate = view.DocumentDate,
                        BatterySerialNo = t.BatterySerialNo,
                        S_VoucherTypeId = t.VoucherTypeId
                    };
                    t.fk_Battery = Battery;
                    // newBatterys.Add(Battery);
                }
                else
                {
                    t.fk_Battery.BrandId = l.BrandId.GetValueOrDefault(0);
                    t.fk_Battery.IsAnalysis = true;
                    t.fk_Battery.ObjectState = ObjectState.Modified;
                    t.fk_Battery.OpeningAge = 0;
                    t.fk_Battery.S_Life = 0;
                    t.fk_Battery.S_CreditAccountId = t.CreditAccountId;
                    t.fk_Battery.S_StatusId = 1202;
                    t.fk_Battery.S_DebitAccountId = t.DebitAccountId;
                    t.fk_Battery.S_DocDate = view.DocumentDate;
                    t.fk_Battery.BatterySerialNo = t.BatterySerialNo;
                    t.fk_Battery.S_VoucherTypeId = view.VoucherTypeId.Value;
                }
                
                #endregion

                t.fk_Voucher = v;

                newBatteryLogList.Add(t);
                var tpi = t.Id > 0 ? oldBatteryPerformance.FirstOrDefault(x => x.FirstIssueLogId == t.Id) : null;// new BatteryLifePerformanceLog();
                if (t.VehicleId > 0)
                {

                    if (tpi == null)
                    {
                        tpi = new BatteryLifePerformanceLog();
                        if (tpi.Id == 0)
                        {
                            tpi.FirstIssueLogId = t.Id;
                        }
                        tpi.CurrentAge = 0;
                        tpi.Life = 0;
                        tpi.LifeAge = 0;
                        tpi.LifeStartDate = t.DocDate;
                        tpi.PreviousAge = 0;
                        tpi.PurchaseAmount = t.NetAmount;
                        tpi.SupplierId = t.DebitAccountId;
                        tpi.LifeEndDate = null;
                        tpi.fk_FirstIssueLog = t;
                        tpi.BatteryId = t.BatteryId;
                        tpiRepo.Insert(tpi);
                    }
                    else
                    {
                        if (tpi.Id == 0) tpi.FirstIssueLogId = t.Id;
                        tpi.CurrentAge = 0;
                        tpi.Life = 0;
                        tpi.LifeAge = 0;
                        tpi.LifeStartDate = l.LogDate ?? view.DocumentDate;
                        tpi.PreviousAge = 0;
                        tpi.PurchaseAmount = t.NetAmount;
                        tpi.SupplierId = t.DebitAccountId;
                        tpi.LifeEndDate = null;
                        tpi.BatteryId = t.BatteryId;
                        tpi.fk_FirstIssueLog = t;
                        tpi.ObjectState = tpi.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        newBatteryPerformance.Add(tpi);
                        tpiRepo.Update(tpi);
                    }
                    tpi.ObjectState = tpi.Id > 0 ? ObjectState.Unchanged : ObjectState.Added;
                    newBatteryPerformance.Add(tpi);
                    t.BatteryStatusId = 1205; /*OnVehicle*/
                    //t.AirPressure = l.AirPressure;
                    //t.KmReading = l.KmReading;
                    if (l.OwnerId.HasValue && l.OwnerId > 0)
                    {
                        t.DebitAccountId = l.OwnerId.Value;
                    }
                    //t.IsStepney = l.IsStepney;

                    t.fk_Battery.S_StatusId = 1205/*OnVehicle in case VehicleNo was assigned to purchase entry*/;
                }
                else if (tpi != null)
                {
                    tpi.ObjectState = ObjectState.Deleted;
                    tpiRepo.Delete(tpi);
                }

                t.fk_Battery.S_Life = 0;
                t.fk_Battery.S_CreditAccountId = t.CreditAccountId;
                t.fk_Battery.S_DebitAccountId = t.DebitAccountId;
                t.fk_Battery.S_DocDate = l.LogDate ?? view.DocumentDate;
                t.fk_Battery.S_VoucherTypeId = t.VoucherTypeId;
            }
            #endregion
            #endregion

            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id);
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.fk_NextLog != null))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    var oldperf = oldBatteryPerformance.FirstOrDefault(x => x.FirstIssueLogId == log.Id);
                    if (oldperf != null)
                    {
                        oldperf.ObjectState = ObjectState.Deleted;
                        tpiRepo.Delete(oldperf);
                    }
                    //RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }
            }

              #region Prepare Voucher
            v.ConstCurTypeId = view.ConstCurTypeId;
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault(0);
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount;
            v.Account1Id = view.PrimaryDebitAccountId;
            v.Account2Id = view.PrimaryCreditAccountId;
            v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
           
            v.Amount3 = 0;
            v.Account4Id = view.OtherLedgerId;
            v.Amount4 = view.OtherAmount;
            v.Amount5 = 0;
            v.Amount6 = 0;
            v.Amount7 = view.TCSAmount;
            v.Account7Id = view.TCSAccountId;
            v.Account8Id = view.RoundOffAcId;
            v.Amount8 = view.RoundOffAmount;
            v.Account9Id = view.PostDiscountAcId;
            v.Amount9 = -Math.Abs(view.PostDiscountAmount);
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";
            PrepareVoucherDetails(_repository, v);

            #endregion
            

            tei.fk_Voucher = v;
            tei.VoucherId = v.Id;
            
            var BatteryRepo = _repository.GetRepository<BatteryMaster>();
            foreach (var log in newBatteryLogList)
            {
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }



            tei.CalVat = view.CalVat;
            tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.OfficeId = view.OfficeId;

            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
            tei.TaxServiceTypeId = view.ServiceTaxTypeId;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
           
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.RoundOffAcId = view.RoundOffAcId;
            tei.RoundOffAmount = view.RoundOffAmount;
            tei.PostDiscountAcId = view.PostDiscountAcId;
            tei.PostDiscountAmount = view.PostDiscountAmount;
            tei.OtherLedgerId = view.OtherLedgerId;
            tei.OtherAmount = view.OtherAmount;
            tei.ProvisionalAcId = view.ProvisionalAcId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            //if (view.JsonData != null)
            //{
            //    foreach (var entity in view.JsonData)
            //    {
            //        tei.DeleteAndAdd(entity);
            //    }
            //}
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            _uom.SaveChanges();

            foreach (var x in newBatteryLogList)
            {
                x.fk_Battery.PurchaseLogId = x.Id;
                x.fk_Battery.S_BatteryLogId = x.Id;
                x.fk_Battery.S_ExtraInfoId = tei.Id;
                x.fk_Battery.PurchaseExtraInfoId = tei.Id;
                x.fk_Battery.fk_PurchaseExtraInfo = tei;
                x.fk_Battery.ObjectState = ObjectState.Modified;
                x.ObjectState = ObjectState.Modified;
                x.ExtraInfoId = tei.Id;
                x.ExtraInfo = tei;
                BatteryRepo.Update(x.fk_Battery);
            }
            _uom.SaveChanges();
        }
        
        public void InsertOrUpdatePurchaseBillMRNSettlementView(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            if (view.Batterys.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
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

            /*forcily debit accountis control account*/
            view.ProvisionalAcId = view.PrimaryDebitAccountId;
            view.PrimaryDebitAccountId = InventoryControlAcId;

            var teiRepo = _repository.GetRepository<BatteryLogExtraInfo>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id) ?? new BatteryLogExtraInfo();
            if (tei == default(BatteryLogExtraInfo) && view.Id > 0)
            {
                //Through error if voucher not found with provided vouchertypeid
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }

            Voucher v = new Voucher();
            var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            if (view.Id > 0 && view.VoucherTypeId == 138 /*Battery MRN*/)
            {
                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = _repository.GetRepository<Voucher>().Query(x => x.Id == tei.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (v == null)
                {
                    v = new Voucher();
                }
            }
            #region Battery Log Preparation

            //Collect Distincs ReferenceId's from Posted SpareLogs
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (view.Id > 0)
            {
                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var oldbpiIdlist = existingBatteryLogs.Select(x => x.Id).ToList();
            var newBatteryLogList = view.Batterys.Where(x => x.Id > 0).Select(x => x.Id);

            #endregion

            if (view.Id > 0)
            {                
                var deletedLogs = existingBatteryLogs.Where(x => !newBatteryLogList.Contains(x.Id)).ToList();
                foreach (var log in deletedLogs)
                {
                    log.BillExtraInfoId = null;
                    log.fk_Bill = null;
                }
            }

            var amount = view.Batterys.Sum(x => x.SubTotal);
            //var cgst = newBatteryLogList.Sum(x => x.CGSTAmount);
            //var sgst = newBatteryLogList.Sum(x => x.SGSTAmount);
            var igst = view.Batterys.Sum(x => x.IGSTAmount);
            var roundup = view.Batterys.Sum(x => x.RoundAmount);

            var subtotalamount = amount + (view.CalVat ? 0 : igst) + roundup;
            var vatamount = view.CalVat ? +igst : 0;

            if (view.VoucherTypeId == 138)
            {
                #region Prepare Voucher
                v.ConstCurTypeId = view.ConstCurTypeId;
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
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
                v.Amount7 = view.TCSAmount;
                v.Account7Id = view.TCSAccountId;
                v.Account8Id = view.RoundOffAcId;
                v.Amount8 = view.RoundOffAmount;
                v.Account9Id = view.PostDiscountAcId;
                v.Amount9 = -Math.Abs(view.PostDiscountAmount);
                v.UserRemark = view.Narration;
                v.OfficeId = view.OfficeId;
                v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //TODO:Setup Account Narration from Template located with VoucherType
                v.AccountingRemark = "";
                

                #endregion                

                tei.fk_Voucher = v;
                tei.VoucherId = v.Id;
            }

            tei.CalVat = view.CalVat;
            tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.OfficeId = view.OfficeId;

            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
            tei.TaxServiceTypeId = view.ServiceTaxTypeId;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.RoundOffAcId = view.RoundOffAcId;
            tei.RoundOffAmount = view.RoundOffAmount;
            tei.PostDiscountAcId = view.PostDiscountAcId;
            tei.PostDiscountAmount = view.PostDiscountAmount;
            tei.OtherLedgerId = view.OtherLedgerId;
            tei.OtherAmount = view.OtherAmount;
            tei.ProvisionalAcId = view.ProvisionalAcId;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            _uom.SaveChanges();

            var existinglogs = _repository.Queryable().Where(x => newBatteryLogList.Contains(x.Id)).ToList();
            List<VoucherDetailReference> vdrids = new List<VoucherDetailReference>();
            
            foreach (var x in existinglogs.GroupBy(p => p.VoucherId))
            {
                try
                {
                    var _vdrId = _repository.GetRepository<VoucherDetailReference>().Queryable().Where(k => k.fk_VoucherDetail.VoucherId == x.Key && k.fk_VoucherDetail.AccountId == InventoryControlAcId).FirstOrDefault();
                    if (_vdrId != null)
                    {
                        _vdrId.Amount = x.Sum(y => y.SubTotal);
                        vdrids.Add(_vdrId);
                    }
                }
                catch { }
            }


            foreach (var x in existinglogs)
            {
                x.ObjectState = ObjectState.Modified;
                x.BillExtraInfoId = tei.Id;
                x.fk_Bill = tei;
            }

            PrepareVoucherDetails(_repository, v,vdrids);
            _uom.SaveChanges();
        }
        public void InsertOrUpdatePurchaseBillView(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            if (view.Batterys.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106,"Battery Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account {view.PrimaryCreditAccountId} or Primary Credit Ammount {view.PrimaryCreditAmount} has Invalid Value.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 || view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account {view.PrimaryDebitAccountId} or Primary Debit Ammount {view.PrimaryDebitAmount} has Invalid Value.");
            }
            var teiRepo = _repository.GetRepository<BatteryLogExtraInfo>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id) ?? new BatteryLogExtraInfo();
            if (tei ==default(BatteryLogExtraInfo)&&view.Id>0)
            {
                //Through error if voucher not found with provided vouchertypeid
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }
            
            Voucher v = new Voucher();
            var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            if (view.Id > 0 && view.VoucherTypeId != 136 /*Battery MRN*/)
            {
                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = _repository.GetRepository<Voucher>().Query(x => x.Id == tei.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (v == null)
                {
                    throw new BusinessException(ErrorCode.GLB106,$"Parent Transaction[Voucher] Not Found");
                }
            }
            #region Battery Log Preparation

            //Collect Distincs ReferenceId's from Posted SpareLogs
            List<BatteryLog> existingBatteryLogs=new List<BatteryLog>();
            if (view.Id > 0)
            {
                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var oldbpiIdlist = existingBatteryLogs.Select(x => x.Id).ToList();
            var oldBatteryPerformance = tpiRepo.Queryable().Where(x => oldbpiIdlist.Contains(x.FirstIssueLogId.Value)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var newBatteryPerformance = new List<BatteryLifePerformanceLog>();
            #region Loop
            foreach (var l in view.Batterys)
            {
                var t = new BatteryLog();
                if (view.Id > 0 && l.Id > 0)
                {
                    t = existingBatteryLogs.Find(x => x.Id == l.Id);
                    if (t.NextLogId != null && t.NextLogId > 0)//&&t.fk_ChildLog.ParentLogId==t.Id)
                    {
                        newBatteryLogList.Add(t);
                        continue;
                        //throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{t.fk_NextLog.DocNo}]");
                    }
                }
                var duplicateBattery =
                    _repository.Queryable().FirstOrDefault(x => x.BatterySerialNo == l.BatterySerialNo && x.BatteryId != l.BatteryId && (x.VoucherTypeId == 43|| x.VoucherTypeId == 95 || x.VoucherTypeId == 48 || x.VoucherTypeId == 58));
                if (duplicateBattery != null)
                {
                    throw new BusinessException(ErrorCode.GLB105, $"Battery No [  { l.BatterySerialNo }  ] already Exists");
                }

                t.OtherAmount = l.OtherAmount;
                t.VehicleId = l.VehicleId;
                t.Remark = l.Remark;
                t.BatteryId = l.BatteryId;
                t.CGSTAmount = l.CGSTAmount;
                t.SGSTAmount = l.SGSTAmount;
                t.IGSTAmount = l.IGSTAmount;
                t.Rate = l.Rate;
                t.SubTotal = l.Rate - l.DiscountAmount +t.OtherAmount;
                t.DiscountAmount = l.DiscountAmount;
                t.DiscountPercent = l.DiscountPercent;
                t.RoundAmount = l.RoundAmount;
                t.NetAmount = l.NetAmount;
                t.POLogId = l.PurchaseOrderId;
                t.TaxServiceTypeId = l.ServiceTaxTypeId;
                t.CGSTPercent = l.CGSTPercent;
                t.SGSTPercent = l.SGSTPercent;
                t.IGSTPercent = l.IGSTPercent;
                t.WarrantyDays = l.WarrantyDays;

                t.DocDate = view.DocumentDate;
                t.DocNo = view.DocumentNo;
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


                #region Voucher Battery 43 => Inward of Purchase Battery
                if (view.VoucherTypeId == 43|| view.VoucherTypeId == 95 || view.VoucherTypeId==136) //Inward of Purchased Batterys and Inward of Old Purchased Batterys
                {
                    t.IsRefurbish = false;
                    t.CreditAccountId = view.PrimaryCreditAccountId;
                    t.DebitAccountId = view.PrimaryDebitAccountId.GetValueOrDefault();
                    t.IssueReceiptId = null;
                    t.JobsheetId = null;
                    t.POLogId = l.PurchaseOrderId;

                    t.BatteryAge = 0;
                    //t.ParentLogId = null;
                    t.ReasonId = null;
                    t.BatteryLife = 0;
                    t.BatteryStatusId = 1202;
                    t.BatterySerialNo = l.BatterySerialNo;
                    t.CalOthAmt = view.CalOthAmt;
                    t.CalVat = view.CalVat;
                    if (t.Id == 0)
                    {
                        var Battery = new BatteryMaster()
                        {
                            BrandId = l.BrandId.GetValueOrDefault(0),
                            IsAnalysis = true,
                            ObjectState = ObjectState.Added,
                            OpeningAge = 0,

                            //fk_PurchaseBatteryLog = t,
                            //PurchaseExtraInfoId = tei.Id,
                            //fk_PurchaseExtraInfo = tei,
                            S_Life = 0,
                            S_CreditAccountId = t.CreditAccountId,
                            S_StatusId = 1202,
                            S_DebitAccountId = t.DebitAccountId,
                            S_DocDate = view.DocumentDate,
                            BatterySerialNo = t.BatterySerialNo,
                            S_VoucherTypeId = t.VoucherTypeId
                        };
                        t.fk_Battery = Battery;
                        // newBatterys.Add(Battery);
                    }
                    else
                    {
                        t.fk_Battery.BrandId = l.BrandId.GetValueOrDefault(0);
                        t.fk_Battery.IsAnalysis = true;
                        t.fk_Battery.ObjectState = ObjectState.Modified;
                        t.fk_Battery.OpeningAge = 0;
                        t.fk_Battery.S_Life = 0;
                        t.fk_Battery.S_CreditAccountId = t.CreditAccountId;
                        t.fk_Battery.S_StatusId = 1202;
                        t.fk_Battery.S_DebitAccountId = t.DebitAccountId;
                        t.fk_Battery.S_DocDate = view.DocumentDate;
                        t.fk_Battery.BatterySerialNo = t.BatterySerialNo;
                        t.fk_Battery.S_VoucherTypeId = view.VoucherTypeId.Value;
                    }
                }
                #endregion

                t.fk_Voucher = (view.VoucherTypeId == 136 /*Battery MRN*/? null : v);

                newBatteryLogList.Add(t);
                var tpi = t.Id > 0 ? oldBatteryPerformance.FirstOrDefault(x => x.FirstIssueLogId == t.Id) : null;// new BatteryLifePerformanceLog();
                if (t.VehicleId > 0)
                {

                    if (tpi == null)
                    {
                        tpi = new BatteryLifePerformanceLog();
                        if (tpi.Id == 0)
                        {
                            tpi.FirstIssueLogId = t.Id;
                        }
                        tpi.CurrentAge = 0;
                        tpi.Life = 0;
                        tpi.LifeAge = 0;
                        tpi.LifeStartDate = t.DocDate;
                        tpi.PreviousAge = 0;
                        tpi.PurchaseAmount = t.NetAmount;
                        tpi.SupplierId = t.DebitAccountId;
                        tpi.LifeEndDate = null;
                        tpi.fk_FirstIssueLog = t;
                        tpi.BatteryId = t.BatteryId;
                        tpiRepo.Insert(tpi);
                    }
                    else
                    {
                        if (tpi.Id == 0) tpi.FirstIssueLogId = t.Id;
                        tpi.CurrentAge = 0;
                        tpi.Life = 0;
                        tpi.LifeAge = 0;
                        tpi.LifeStartDate = l.LogDate ?? view.DocumentDate;
                        tpi.PreviousAge = 0;
                        tpi.PurchaseAmount = t.NetAmount;
                        tpi.SupplierId = t.DebitAccountId;
                        tpi.LifeEndDate = null;
                        tpi.BatteryId = t.BatteryId;
                        tpi.fk_FirstIssueLog = t;
                        tpi.ObjectState = tpi.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        newBatteryPerformance.Add(tpi);
                        tpiRepo.Update(tpi);
                    }
                    tpi.ObjectState = tpi.Id > 0 ? ObjectState.Unchanged : ObjectState.Added;
                    newBatteryPerformance.Add(tpi);
                    t.BatteryStatusId = 1205; /*OnVehicle*/
                    //t.AirPressure = l.AirPressure;
                    //t.KmReading = l.KmReading;
                    if (l.OwnerId.HasValue && l.OwnerId > 0)
                    {
                        t.DebitAccountId = l.OwnerId.Value;
                    }
                    //t.IsStepney = l.IsStepney;

                    t.fk_Battery.S_StatusId = 1205/*OnVehicle in case VehicleNo was assigned to purchase entry*/;
                }
                else if (tpi != null)
                {
                    tpi.ObjectState = ObjectState.Deleted;
                    tpiRepo.Delete(tpi);
                }
               
                t.fk_Battery.S_Life = 0;
                t.fk_Battery.S_CreditAccountId = t.CreditAccountId;
                t.fk_Battery.S_DebitAccountId = t.DebitAccountId;
                t.fk_Battery.S_DocDate = l.LogDate ?? view.DocumentDate;
                t.fk_Battery.S_VoucherTypeId = t.VoucherTypeId;
            }
            #endregion
            #endregion

            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id);
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.fk_NextLog != null))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    var oldperf = oldBatteryPerformance.FirstOrDefault(x => x.FirstIssueLogId == log.Id);
                    if(oldperf != null)
                    {
                        oldperf.ObjectState = ObjectState.Deleted;
                        tpiRepo.Delete(oldperf);
                    }
                    //RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }
            }

            if (view.VoucherTypeId != 136)
            {
                #region Prepare Voucher
                v.ConstCurTypeId = view.ConstCurTypeId;
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
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
                v.Amount7 = view.TCSAmount;
                v.Account7Id = view.TCSAccountId;
                v.Account8Id = view.RoundOffAcId;
                v.Amount8 = view.RoundOffAmount;
                v.Account9Id = view.PostDiscountAcId;
                v.Amount9 = -Math.Abs(view.PostDiscountAmount);
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

                tei.fk_Voucher = v;
                tei.VoucherId = v.Id;
            }
            var BatteryRepo = _repository.GetRepository<BatteryMaster>();
            foreach (var log in newBatteryLogList)
            {
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            
            
            
            tei.CalVat = view.CalVat;
            tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.OfficeId = view.OfficeId;
            
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
            tei.TaxServiceTypeId = view.ServiceTaxTypeId;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.RoundOffAcId = view.RoundOffAcId;
            tei.RoundOffAmount = view.RoundOffAmount;
            tei.PostDiscountAcId = view.PostDiscountAcId;
            tei.PostDiscountAmount = view.PostDiscountAmount;
            tei.OtherLedgerId = view.OtherLedgerId;
            tei.OtherAmount = view.OtherAmount;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;

            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            //if (view.JsonData != null)
            //{
            //    foreach (var entity in view.JsonData)
            //    {
            //        tei.DeleteAndAdd(entity);
            //    }
            //}
            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            _uom.SaveChanges();

            foreach (var x in newBatteryLogList)
            {
                x.fk_Battery.PurchaseLogId = x.Id;
                x.fk_Battery.S_BatteryLogId = x.Id;
                x.fk_Battery.S_ExtraInfoId = tei.Id;
                x.fk_Battery.PurchaseExtraInfoId = tei.Id;
                x.fk_Battery.fk_PurchaseExtraInfo = tei;
                x.fk_Battery.ObjectState = ObjectState.Modified;
                x.ObjectState=ObjectState.Modified;
                x.ExtraInfoId = tei.Id;
                x.ExtraInfo = tei;
                BatteryRepo.Update(x.fk_Battery);
            }
            _uom.SaveChanges();
        }

        private static void PrepareVoucherDetails(IRepository<BatteryLog> repository, Voucher v, List<VoucherDetailReference> againstrefvdrs = null)
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
            var offices = ledgerRepo.Where(x => x.Id == v.Account1Id || x.Id == v.Account2Id || x.Id == v.Account3Id || x.Id == v.Account4Id || x.Id == v.Account5Id || x.Id == v.Account6Id || x.Id == v.Account7Id || x.Id == v.Account8Id || x.Id == v.Account9Id)
                .Select(x => new { x.Id, x.OfficeId, x.ReferenceFlag }).ToList();
            if (v.Account1Id.HasValue && v.Amount1 != 0)
            {
                var a1 = new VoucherDetail() { };
                a1.AccountId = v.Account1Id.Value;
                a1.Amount = v.Amount1;
                a1.OrderId = 1;
                a1.CurRate = v.CurRate;
                a1.CurTypeId = v.CurTypeId;
                var ledger = offices.Where(x => x.Id == v.Account1Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account1Id}");
                }
                a1.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a1.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a1);
                if (ledger.ReferenceFlag || v.VoucherTypeId == 138) { PrepareVDR(a1, v.VoucherNo,againstrefvdrs); }
            }
            if (v.Account2Id.HasValue && v.Amount2 != 0)
            {
                var a2 = new VoucherDetail() { };
                a2.AccountId = v.Account2Id.Value;
                a2.Amount = v.Amount2;
                a2.CurRate = v.CurRate;
                a2.CurTypeId = v.CurTypeId;
                a2.OrderId = 2;
                var ledger = offices.Where(x => x.Id == v.Account2Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account2Id}");
                }
                a2.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a2.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a2);
                if (ledger.ReferenceFlag || v.VoucherTypeId == 136 || v.VoucherTypeId == 138) { PrepareVDR(a2, v.VoucherNo); }
            }
            if (v.Account3Id > 0 && v.Amount3!=0)
            {
                var a3 = new VoucherDetail() { };
                a3.AccountId = v.Account3Id.GetValueOrDefault(0);
                a3.Amount = v.Amount3;
                a3.CurRate = v.CurRate;
                a3.CurTypeId = v.CurTypeId;
                a3.OrderId = 3;
                var ledger = offices.Where(x => x.Id == v.Account3Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account3Id}");
                }
                a3.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a3.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a3);
                if (ledger.ReferenceFlag) { PrepareVDR(a3, v.VoucherNo); }
            }
            if (v.Account4Id > 0 && v.Amount4!=0)
            {
                var a4 = new VoucherDetail() { };
                a4.AccountId = v.Account4Id.GetValueOrDefault(0);
                a4.Amount = v.Amount4;
                a4.OrderId = 4;
                a4.CurRate = v.CurRate;
                a4.CurTypeId = v.CurTypeId;
                var ledger = offices.Where(x => x.Id == v.Account4Id)
                     .Select(x => new { x.OfficeId, x.ReferenceFlag })
                     .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account4Id}");
                }
                a4.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
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
                a5.CurRate = v.CurRate;
                a5.CurTypeId = v.CurTypeId;
                var ledger = offices.Where(x => x.Id == v.Account5Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account5Id}");
                }
                a5.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
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
                a6.CurRate = v.CurRate;
                a6.CurTypeId = v.CurTypeId;
                var ledger = offices.Where(x => x.Id == v.Account6Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account6Id}");
                }
                a6.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
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
                a7.CurRate = v.CurRate;
                a7.CurTypeId = v.CurTypeId;
                var ledger = offices.Where(x => x.Id == v.Account7Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account7Id}");
                }
                a7.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
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
                a8.CurRate = v.CurRate;
                a8.CurTypeId = v.CurTypeId;
                var ledger = offices.Where(x => x.Id == v.Account8Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account8Id}");
                }
                a8.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
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
                a9.CurRate = v.CurRate;
                a9.CurTypeId = v.CurTypeId;
                var ledger = offices.Where(x => x.Id == v.Account9Id)
                    .Select(x => new { x.OfficeId, x.ReferenceFlag })
                    .FirstOrDefault();
                if (ledger == null)
                {
                    throw new BusinessException(ErrorCode.VCH110, $"Invalid Ledger identification :{v.Account9Id}");
                }
                a9.OfficeId = ledger.OfficeId.Value == 0 ? v.OfficeId : ledger.OfficeId.Value;
                a9.ObjectState = ObjectState.Added;
                v.VoucherDetails.Add(a9);
                if (ledger.ReferenceFlag) { PrepareVDR(a9, v.VoucherNo); }
            }
        }

        private static void PrepareVDR(VoucherDetail vd, string voucherNo, List<VoucherDetailReference> againstrefvdrs = null)
        {
            if (againstrefvdrs != null && againstrefvdrs.Any())
            {
                foreach (var avdr in againstrefvdrs.GroupBy(x => x.Id))
                {
                    var vdr = new VoucherDetailReference()
                    {
                        Amount = avdr.Sum(x => x.Amount),
                        ObjectState = ObjectState.Added,
                        ReferenceNo = avdr.FirstOrDefault().ReferenceNo,
                        VDRTypeId = 1014,
                        RefId = avdr.Key,
                        CurTypeId = vd.CurTypeId,
                        CurRate = vd.CurRate,
                        ConstCurTypeId = vd.ConstCurTypeId,
                        IsCCRequired = true
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
                    ConstCurTypeId = vd.ConstCurTypeId,
                    IsCCRequired = true
                };
                vd.VoucherDetailReferences = new List<VoucherDetailReference>() { vdr };
            }
        }
        /// <exception cref="BusinessException">Invalid VoucherId.</exception>
        public void DeleteGraph(long key,IUnitOfWorkAsync uow)
        {
            var teiRepo = uow.RepositoryAsync<BatteryLogExtraInfo>();
            var btcheckrepo= uow.RepositoryAsync<BatteryCheck>();
            var settingids = new [] { "VoucherVisiblityFlag" };
            var settings = uow.RepositoryAsync<ApiConfiguration>().Queryable().Where(x => settingids.Contains(x.Value)).ToList();
            var tei = teiRepo.Find(key);
            if(tei==null)throw new BusinessException(ErrorCode.GLB109,$"The selected transaction is not existing.");
            var typeCanBeDeleted = new List<long>() { 138,136,43,95, 44, 45, 46, 47, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57 ,58};
            if (!typeCanBeDeleted.Contains(tei.VoucherTypeId))
            {
                throw new BusinessException(ErrorCode.GLB106, "Only Battery transactions can be deleted via this gateway");
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

                // Marking voucher & its related vd & vdrs for deletion
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
                tei.VoucherId = null; //Remove Voucher link from Battery Extra Info
                
            }
            #endregion VoucherEnd

            #region Battery Deletion Process Checks
            var list = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Include(x => x.fk_PreviousLog).Where(x => (x.ExtraInfoId == tei.Id) || (x.VoucherTypeId==136 && x.BillExtraInfoId == tei.Id)).ToList();
            
            //Deletion Case 1: The Whole Transaction shall be deleted if & only if nextlogid(s) of all Battery(s) within a transaction is null
            if (list.Any(x =>x.NextLogId > 0))
            {
                var invalidrows = list.Where(x => x.NextLogId > 0).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine);
                throw new BusinessException(ErrorCode.GLB108, $"Transaction cannot delete some of the Battery(s) has been referenced.{Environment.NewLine} Battery Nos=>Child VoucherNo:{invalidrows}");
            }
            #endregion Battery Deletion Process Checks end

            var tpRepo = uow.RepositoryAsync<BatteryLifePerformanceLog>();
            List<BatteryLifePerformanceLog> tplist = null;
            
            if (tei.VoucherTypeId == 138)
            {
                foreach (var log in list)
                {
                    log.BillExtraInfoId = null;
                    log.fk_Bill = null;
                    log.ObjectState = ObjectState.Modified;
                }
            }
            else {

                if (tei.VoucherTypeId == 50 || tei.VoucherTypeId == 51)
                {
                    tplist = (from t in tpRepo.Queryable()
                              join p in _repository.Queryable().Where(x => x.ExtraInfoId == tei.Id)
                              on (t.BatteryId + "" + t.Life) equals (p.BatteryId + "" + p.BatteryLife)
                              where (t.FirstIssueLogId == p.Id || t.LastReceiptLogId == p.Id) && t.Life == p.BatteryLife
                              select t
                              ).ToList();
                    //tplist = (from t in tpRepo.Queryable()
                    //    from p in list
                    //    where
                    //        t.BatteryId == p.BatteryId && t.Life == p.BatteryLife &&
                    //        (t.FirstIssueLogId == p.Id || t.LastReceiptLogId == p.Id)
                    //    select t).ToList();
                }
                if (tei.VoucherTypeId == 136 || tei.VoucherTypeId == 43 || tei.VoucherTypeId == 58 || tei.VoucherTypeId == 95)
                {
                    foreach (var log in list)
                    {
                        if (log.VoucherTypeId == 136 || log.VoucherTypeId == 43 || log.VoucherTypeId == 58 || log.VoucherTypeId == 95)
                        //New Battery Purchase Inward//Chasis Battery
                        {
                            log.ObjectState = ObjectState.Modified;
                            log.fk_Battery.ObjectState = ObjectState.Modified;
                            log.fk_Battery.PurchaseExtraInfoId = null;
                            log.fk_Battery.PurchaseLogId = null;
                            log.fk_Battery.fk_PurchaseBatteryLog = null;
                            log.fk_Battery.fk_PurchaseExtraInfo = null;
                            log.fk_Battery.S_BatteryLogId = null;
                            log.fk_Battery.S_ExtraInfoId = null;
                            log.fk_Battery.fk_S_BatteryLog = null;
                            log.fk_Battery.fk_S_ExtraInfo = null;

                        }
                    }
                    _repository.UOW.SaveChanges();
                }

                foreach (var log in list)
                {
                    if (log.BatteryCheckId > 0)
                    {
                        if (log.fk_BatteryCheck == null)
                        {
                            log.fk_BatteryCheck = btcheckrepo.Find(log.BatteryCheckId);
                        }
                        log.fk_BatteryCheck.ObjectState = ObjectState.Deleted;
                    }
                    if (log.VoucherTypeId == 136 || log.VoucherTypeId == 43 || log.VoucherTypeId == 58 || log.VoucherTypeId == 95) //New Battery Purchase Inward//Chasis Battery
                    {
                        log.fk_Battery.ObjectState = ObjectState.Deleted;
                        log.ObjectState = ObjectState.Deleted;
                    }
                    else
                    {
                        if (log.VoucherTypeId == 50 && tplist != null)//Issue Log: Restore Last issue in Battery Performance Log
                        {
                            var lastissuelog = tplist.FirstOrDefault(x => x.FirstIssueLogId == log.Id);
                            if (lastissuelog != null) //Performance log shall be deleted only in case of first issue.
                            {
                                lastissuelog.ObjectState = ObjectState.Deleted;
                                tpRepo.Delete(lastissuelog);
                            }
                        }
                        if (log.VoucherTypeId == 51 && tplist != null)
                        //Receipt Voucher: Restore Last Receipt in Battery Performance Log
                        {
                            var lastreceiptlog = tplist.FirstOrDefault(x => x.LastReceiptLogId == log.Id);
                            if (lastreceiptlog == null) // Only last receipt can be deleted.
                            {
                                throw new BusinessException(ErrorCode.GLB106, "Only recent transaction can be deleted.");
                            }

                            //Find Last Log Other than current in loop
                            BatteryLog lstTl = _repository.GetLastBatteryLogByStatusAndLife(log.BatteryId, new long[] { 51 }, log.BatteryLife, log.Id);
                            lastreceiptlog.LastReceiptLogId = lstTl?.Id;
                            lastreceiptlog.ObjectState = ObjectState.Modified;
                            tpRepo.Update(lastreceiptlog);
                        }

                        var p = RestorePreviousBatteryStatus(log);
                        if (p.ObjectState != ObjectState.Deleted)
                        {
                            _repository.Update(p);
                        }
                        else
                        {
                            _repository.Delete(p);
                        }

                        log.ObjectState = ObjectState.Deleted;
                    }
                    _repository.Delete(log);
                }
            }

            tei.ObjectState = ObjectState.Deleted;
            teiRepo.Delete(tei);
            //_repository.UOW.SaveChanges();
            //foreach (var log in checkentries)
            //{
            //    log.ObjectState=ObjectState.Deleted;
            //}
            //_repository.UOW.SaveChanges();
        }

        public vwBatteryBillView GetBatteryResaleBill(long key)
        {
            return _repository.GetBatteryResaleBillView(key);
        }
        public void InsertUpdateBatteryScrap(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            view.VoucherTypeId = 53;//Battery Scrap
            if (view.ScrapLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
            }

            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Account {view.PrimaryCreditAccountId}");
            }

            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 || view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Buyer Account {view.PrimaryDebitAccountId} or Buyer Ammount {view.PrimaryDebitAmount} is required.");
            }

            if (view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106,
                    $" Battery Income Ammount {view.PrimaryCreditAmount} is required.");
            }

            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<BatteryLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            Voucher v = new Voucher();

            //Collect Distince ReferenceId's from Posted SpareLogs
            BatteryLogExtraInfo tei = null;
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = vRepo.Query(x => x.Id == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                if (v == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var issuerefids = view.ScrapLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var newBatterystatus = new long[] { 1203 };
            List<BatteryLog> scrapReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => issuerefids.Contains(x.Id)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var oldBatteryLogs = new List<BatteryLog>();
            foreach (var l in view.ScrapLog)
            {
                /************************************************************
                *************||Battery Scrap Logics Start||*********************
                *************************************************************/
                #region Battery Scrap Logic
                var i = new BatteryLog();//Scrap Log
                var ir = scrapReferenceLogs.FirstOrDefault(x => x.Id == l.ReferenceId);//Scrap Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Battery No {l.BatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.BatterySerialNo}");
                }
                if (!newBatterystatus.Contains(ir.BatteryStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {l.BatterySerialNo} can't be scrap");
                }
                if (l.Id > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.Id);//Issued Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Battery has been altered restore all Battery status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                    //oldBatteryLogs.Add(RestorePreviousBatteryStatus(i));
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }
                i.TSLId = l.TSLId;
                i.Rate = i.SubTotal = i.NetAmount = l.BatteryCost;
                i.DiscountAmount = i.DiscountPercent = i.OtherAmount = 0;
                i.BatteryAge = 0;
                
                i.CreditAccountId = view.PrimaryCreditAccountId;//StoreId
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;//VendorId
                i.Remark = l.Remark;
                i.BatteryId = ir.BatteryId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = ir.BatteryLife;
                i.BatteryStatusId = 1204;//Scrap
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = ir.BatteryLife;
                i.fk_Battery.S_StatusId = i.BatteryStatusId;//Scrap
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
                    i.fk_Battery.S_BatteryLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newBatteryLogList.Add(i);
                #endregion
            }
            var BatteryRepo = _uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }
            #region Prepare Issue Voucher


            var totalScrap = view.ScrapLog.Sum(x => x.BatteryCost);
            v.ConstCurTypeId = view.ConstCurTypeId;
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.Value;
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount;
            v.Account1Id = view.PrimaryDebitAccountId;//VendorId
            v.Account2Id = view.PrimaryCreditAccountId;//Income Id
            v.Amount2 = -Math.Abs(view.PrimaryCreditAmount);
            v.Account7Id = view.TCSAccountId;
            v.Amount7 = view.TCSAmount;
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";

            if (v.Amount1 != totalScrap)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Buyer Total Amount {totalScrap} Does't match Voucher Primary Debit Amount {v.Amount1}");
            }
            if (v.Amount2 != -totalScrap)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Income Total Amount {-totalScrap} Does't match Voucher Primary Credit Amount {v.Amount2}");
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
            tei = tei ?? new BatteryLogExtraInfo();
            tei.fk_Voucher = v;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherId = v.Id;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;//StoreId
            tei.DrAccountId = view.PrimaryDebitAccountId;//VendorId
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.TCSAccountId = view.TCSAccountId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;


            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);

            foreach (var log in newBatteryLogList)
            {
                log.VoucherId = v.Id;
                log.fk_Voucher = v;
                log.fk_Battery.S_ExtraInfoId = tei.Id;
                log.fk_Battery.S_DocDate = tei.DocDate;
                log.fk_Battery.fk_S_ExtraInfo = tei;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            _uom.SaveChanges();
        }

        public vwBatteryBillView GetBatteryScrapBillView(long key)
        {
            return _repository.GetBatteryScrapBillView(key);
        }
        public void InsertUpdateBatteryStocktransferOutBillView(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            view.VoucherTypeId = 52;//Outward of Transfered Batterys
            if (view.StoreTransferLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
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

            if (transitStore == null || !long.TryParse(transitStore.Value, out transitStoreId)) throw new BusinessException(ErrorCode.GLB103, "Transit Store need to be configured.");


            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<BatteryLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            Voucher v = new Voucher();
            //Collect Distincs ReferenceId's from Posted SpareLogs
            BatteryLogExtraInfo tei = null;
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
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
                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var StoreTransferrefids = view.StoreTransferLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var newBatterystatus = new long[] { 1202, 1203 };
            List<BatteryLog> StoreTransferReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => StoreTransferrefids.Contains(x.Id)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var oldBatteryLogs = new List<BatteryLog>();
            foreach (var l in view.StoreTransferLog)
            {
                /************************************************************
                *************||Battery StoreTransfer Logics Start||*********************
                *************************************************************/
                #region Battery StoreTransfer Logic
                var i = new BatteryLog();//StoreTransfer Log
                var ir = StoreTransferReferenceLogs.Find(x => x.Id == l.ReferenceId);//StoreTransfer Reference Log

                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Battery No {l.BatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.BatterySerialNo}");
                }
                if (!newBatterystatus.Contains(ir.BatteryStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {l.BatterySerialNo} can't be StoreTransfer");
                }
                if (l.Id > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.Id);//StoreTransfer Log
                    if (i.fk_NextLog?.Id > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Battery has been altered restore all Battery status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                    //oldBatteryLogs.Add(RestorePreviousBatteryStatus(i));
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }
                i.TSLId = l.TSLId;
                i.Rate = i.SubTotal = i.NetAmount = l.BatteryCost;
                i.OtherAmount = 0;
                i.DiscountAmount = i.DiscountPercent = 0;
                i.BatteryAge = 0;


                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = transitStoreId;
                i.Remark = l.Remark;
                i.BatteryId = ir.BatteryId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = ir.BatteryLife;
                i.BatteryStatusId = ir.BatteryStatusId;// Shall retain old Battery status in case of StoreTransfer
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = ir.BatteryLife;
                i.fk_Battery.S_StatusId = i.BatteryStatusId;
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
                    i.fk_Battery.S_BatteryLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newBatteryLogList.Add(i);
                #endregion
            }
            var BatteryRepo = _uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                //var deletedids = deletedLogs.Select(x => x.Id).ToList();
                //var parents = _repository.Queryable().Where(x => deletedids.Contains(x.NextLogId.Value)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;


                    _repository.Delete(log);
                }
                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }
            #region Prepare StoreTransfer Voucher
            var totalVendorAmt = StoreTransferReferenceLogs.Sum(x => x.NetAmount);



            if (totalVendorAmt > 0)
            {
                v.ConstCurTypeId = view.ConstCurTypeId;
                v.CurTypeId = view.CurTypeId;
                v.ViewId = view.ViewId;

                v.CurRate = view.CurRate;
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


            tei = tei ?? new BatteryLogExtraInfo();
            tei.fk_Voucher = v;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherId = v?.Id;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = transitStoreId;
            tei.TransitStoreId = transitStoreId;
            tei.ProvisionalAcId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);

            foreach (var log in newBatteryLogList)
            {
                log.VoucherId = v?.Id;
                log.fk_Voucher = v;
                log.fk_Battery.S_ExtraInfoId = tei.Id;

                log.fk_Battery.S_DocDate = tei.DocDate;
                log.fk_Battery.fk_S_ExtraInfo = tei;

                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            _uom.SaveChanges();
        }
        public void InsertUpdateBatteryStocktransferInBillView(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            view.VoucherTypeId = 44;//Inward of Transfered Batterys
            if (view.StoreTransferLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Sender Store Account {view.PrimaryCreditAccountId} is required.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Receiver Store Account {view.PrimaryDebitAccountId} is required.");
            }


            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<BatteryLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            Voucher v = new Voucher();
            //Collect Distincs ReferenceId's from Posted SpareLogs
            BatteryLogExtraInfo tei = null;
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
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
                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var StoreTransferrefids = view.StoreTransferLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            
            List<BatteryLog> StoreTransferReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => StoreTransferrefids.Contains(x.Id)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var oldBatteryLogs = new List<BatteryLog>();
            foreach (var l in view.StoreTransferLog)
            {
                /************************************************************
                *************||Battery StoreTransfer Logics Start||*********************
                *************************************************************/
                #region Battery StoreTransfer Logic
                var i = new BatteryLog();//StoreTransfer Log
                var ir = StoreTransferReferenceLogs.Find(x => x.Id == l.ReferenceId);//StoreTransfer Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Battery No {l.BatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.BatterySerialNo}");
                }
                if (ir.VoucherTypeId != 52)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {l.BatterySerialNo} can't be Inwarded into Store. Only stock transfered Battery(s) can be inwarded through this transaction");
                }
                if (l.Id > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.Id);//StoreTransfer Log
                    if (i.fk_NextLog?.Id > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Battery has been altered restore all Battery status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                    //oldBatteryLogs.Add(RestorePreviousBatteryStatus(i));
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }
                i.TSLId = l.TSLId;
                i.Rate = i.SubTotal = l.BatteryCost;
                i.OtherAmount = 0;
                i.NetAmount = l.BatteryCost;
                i.DiscountAmount = i.DiscountPercent = 0;
                i.BatteryAge = 0;
                

                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.BatteryId = ir.BatteryId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = ir.BatteryLife;
                i.BatteryStatusId = ir.BatteryStatusId;// Shall retain old Battery status in case of StoreTransfer
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = ir.BatteryLife;
                i.fk_Battery.S_StatusId = i.BatteryStatusId;
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
                    i.fk_Battery.S_BatteryLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newBatteryLogList.Add(i);
                #endregion
            }
            var BatteryRepo = _uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();

                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                foreach (var log in deletedLogs)
                {
                    RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }
            #region Prepare StoreTransfer Voucher
            var totalVendorAmt = StoreTransferReferenceLogs.Sum(x => x.NetAmount);
            var transitStore = _repository.GetRepository<ApiConfiguration>().Find("TransitStoreId");
            long transitStoreId = 0;
            if (transitStore == null || !long.TryParse(transitStore.Value, out transitStoreId)) throw new BusinessException(ErrorCode.GLB103, "Transit Store need to be configured.");
            if (totalVendorAmt > 0)
            {
                v.ConstCurTypeId = view.ConstCurTypeId;
                v.CurTypeId = view.CurTypeId;
                v.ViewId = view.ViewId;
                v.CurRate = view.CurRate;
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


            tei = tei ?? new BatteryLogExtraInfo();
            tei.fk_Voucher = v;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherId = v?.Id;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.TransitStoreId = transitStoreId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);

            foreach (var log in newBatteryLogList)
            {
                log.VoucherId = v.Id;
                log.fk_Voucher = v;
                log.fk_Battery.S_ExtraInfoId = tei.Id;
                log.fk_Battery.S_DocDate = tei.DocDate;
                log.fk_Battery.fk_S_ExtraInfo = tei;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            _uom.SaveChanges();
        }
        public vwBatteryBillView GetBatteryStoretransferOutBillView(long key)
        {
            return _repository.GetBatteryStoretransferOutBillView(key);
        }
        public vwBatteryBillView GetBatteryStoretransferInBillView(long key)
        {
            return _repository.GetBatteryStoretransferInBillView(key);
        }
        public void InsertUpdateBatteryReject(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            if (view.RejectLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Battery Vendor Account {view.PrimaryCreditAccountId} is required.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Account {view.PrimaryDebitAccountId} is required");
            }
            
            var teiRepo = _uom.RepositoryAsync<BatteryLogExtraInfo>();
            if (teiRepo.Queryable().Any(x => x.DocNo == view.DocumentNo && view.Id <= 0))
            {
                throw new BusinessException(ErrorCode.GLB106, $"Document No {view.DocumentNo} already exists.");
            }
            if(view.Id<=0 && view.Batterys.Any(x=>x.Id>0)) throw new BusinessException(ErrorCode.GLB106, $"Incomplete Transaction.");
            //TODO:Implement Document No change restriction validation
            //Collect Distincs ReferenceId's from Posted SpareLogs
            BatteryLogExtraInfo tei = null;
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");


                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            var Rejectrefids = view.RejectLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var allowedStatus = view.VoucherTypeId == 46 ? new long[] { 1207 } : new long[] { 1208 };
            List<BatteryLog> RejectReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => Rejectrefids.Contains(x.Id)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var oldBatteryLogs = new List<BatteryLog>();
            foreach (var l in view.RejectLog)
            {
                /************************************************************
                *************||Battery Reject Logics Start||*********************
                *************************************************************/
                #region Battery Reject Logic
                var i = new BatteryLog();//Reject Log
                var ir = RejectReferenceLogs.Find(x => x.Id == l.ReferenceId);//Issue Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Battery No {l.BatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.BatterySerialNo}");
                }
                if (!allowedStatus.Contains(ir.BatteryStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery Status has already been changed. So the Battery No {l.BatterySerialNo} can't be inwarded.");
                }
                if (l.Id > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.Id);//Reject Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Battery has been altered restore all Battery status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                    //oldBatteryLogs.Add(RestorePreviousBatteryStatus(i));
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                    //i.fk_Battery = null;
                    //i.BatteryId = 0;
                    //i.fk_PreviousLog = null;
                    //i.PreviousLogId = null;
                }

                //46  Inward of Retreat Rejected Batterys
                //47  Inward of Claim Rejected Batterys
                //1203 Old Stock
                i.TSLId = l.TSLId;
                i.Rate =
                    i.SubTotal =
                        i.OtherAmount = i.NetAmount = i.DiscountAmount = i.DiscountPercent = 0;
                        i.BatteryAge = 0;

                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.BatteryId = ir.BatteryId;
                i.ReasonId = ir.ReasonId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = ir.BatteryLife;
                i.BatteryStatusId = 1203;
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;

                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = ir.BatteryLife;
                i.fk_Battery.S_StatusId = i.BatteryStatusId;

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
                    i.fk_Battery.S_BatteryLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newBatteryLogList.Add(i);
                #endregion
            }
            var BatteryRepo = _uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();

                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                foreach (var log in deletedLogs)
                {
                    RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }


            tei = tei ?? new BatteryLogExtraInfo();
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newBatteryLogList)
            {
                log.fk_Battery.S_DocDate = tei.DocDate;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            _uom.SaveChanges();
        }
        public vwBatteryBillView GetBatteryRejectBillView(long key)
        {
            return _repository.GetBatteryRejectBillView(key);
        }
        public void InsertUpdateBatteryClaimReceiptBillView(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            view.VoucherTypeId = 48;//Claim Received
            if (view.Batterys.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
            }
            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryCreditAmount >= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Credit Account {view.PrimaryCreditAccountId} or Primary Credit Ammount {view.PrimaryCreditAmount} has Invalid Value.");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0 || view.PrimaryDebitAmount <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Primary Debit Account {view.PrimaryDebitAccountId} or Primary Debit Ammount {view.PrimaryDebitAmount} has Invalid Value.");
            }
            var teiRepo = _repository.GetRepository<BatteryLogExtraInfo>();
            var tei = teiRepo.Queryable().FirstOrDefault(x => x.Id == view.Id) ?? new BatteryLogExtraInfo();
            if (tei == default(BatteryLogExtraInfo) && view.Id > 0)
            {
                //Through error if voucher not found with provided vouchertypeid
                throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
            }
            Voucher v = new Voucher();
            #region Battery Log Preparation

            //Collect Distincs ReferenceId's from Posted SpareLogs
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (view.Id > 0)
            {
                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == view.Id && x.BatteryStatusId == 1202).ToList();
            }
            var refIds = view.Batterys.Select(x => x.ReferenceId).ToList();
            var referencelogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => refIds.Contains(x.Id)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var claimReceiptLog = new List<BatteryLog>();
            var BatteryRepo = _repository.GetRepository<BatteryMaster>();
            foreach (var l in view.Batterys)
            {
                var t = new BatteryLog();
                if (view.Id > 0 && l.Id > 0)
                {
                    t = existingBatteryLogs.Find(x => x.Id == l.Id);
                    if (t.NextLogId != null && t.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{t.fk_NextLog.DocNo}]");
                    }
                }

                var duplicateBattery =
                    _repository.Queryable().FirstOrDefault(x => x.BatterySerialNo == l.BatterySerialNo && x.BatteryId != l.BatteryId && (x.VoucherTypeId == 43|| x.VoucherTypeId == 95 || x.VoucherTypeId == 48 || x.VoucherTypeId == 58));
                if (duplicateBattery != null)
                {
                    throw new BusinessException(ErrorCode.GLB105, $"Battery No [  { l.BatterySerialNo }  ] already Exists");
                }

                #region//Claim Section start
                var crt = new BatteryLog();

                var xx = referencelogs.FirstOrDefault(x => x.Id == l.ReferenceId);

                if (view.Id > 0 && l.Id > 0)
                {
                    var previouslogids = existingBatteryLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                    _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();

                    var oldcrt =
                        existingBatteryLogs.FirstOrDefault(x => x.fk_PreviousLog.PreviousLogId == l.ReferenceId);
                    if (oldcrt == default(BatteryLog))
                    {
                        throw new BusinessException(ErrorCode.GLB106, $"Change of Battery No ' { l.BatterySerialNo } ' Not allowed.Use Delete Option in-case of Battery Number Change.");
                    }
                }

                if (xx == default(BatteryLog) || xx.BatteryStatusId != 1208 || (xx.NextLogId != null && t.Id == 0))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery {l.BatterySerialNo} has invalid reference. Because Ref Battery No '{xx.BatterySerialNo}' is out of stock");
                }

                #endregion

                #region New Battery Entry of ClaimPassed
                t.OtherAmount = l.OtherAmount;
                t.VehicleId = l.VehicleId;
                t.Remark = l.Remark;
                t.BatteryId = l.BatteryId;
                t.CGSTAmount = l.CGSTAmount;
                t.SGSTAmount = l.SGSTAmount;
                t.IGSTAmount = l.IGSTAmount;
                t.Rate = l.Rate;
                t.SubTotal = l.Rate;
                t.DiscountAmount = 0;
                t.DiscountPercent = 0;
                t.RoundAmount = l.RoundAmount;
                t.NetAmount = l.NetAmount;
                t.POLogId = l.PurchaseOrderId;
                t.TaxServiceTypeId = l.ServiceTaxTypeId;
                t.CGSTPercent = l.CGSTPercent;
                t.SGSTPercent = l.SGSTPercent;
                t.IGSTPercent = l.IGSTPercent;
                t.WarrantyDays = l.WarrantyDays;
                t.DocDate = view.DocumentDate;
                t.DocNo = view.DocumentNo;
                t.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
                t.CreditAccountId = view.PrimaryCreditAccountId;
                t.DebitAccountId = view.PrimaryDebitAccountId.GetValueOrDefault();
                t.TransferPrice = l.CarriedCost;// added by sanjay
                t.BatteryLife = 0;
                t.BatteryStatusId = 1202;
                t.BatterySerialNo = l.BatterySerialNo;
                //t.CalOthAmt = view.CalOthAmt;
                t.CalVat = view.CalVat;

                //For New Battery
                if (t.Id == 0)
                {

                    var Battery = new BatteryMaster()
                    {
                        BrandId = l.BrandId.GetValueOrDefault(0),
                        IsAnalysis = true,
                        ObjectState = ObjectState.Added,
                        OpeningAge = 0,
                        S_Life = 0,
                        S_CreditAccountId = t.CreditAccountId,
                        S_StatusId = 1202,
                        S_DebitAccountId = t.DebitAccountId,
                        S_DocDate = view.DocumentDate,
                        BatterySerialNo = t.BatterySerialNo,
                        S_VoucherTypeId = t.VoucherTypeId
                    };
                    t.fk_Battery = Battery;
                }
                else
                {
                    t.fk_Battery.BrandId = l.BrandId.GetValueOrDefault(0);
                    t.fk_Battery.IsAnalysis = true;
                    t.TaxServiceTypeId = t.TaxServiceTypeId;
                    t.fk_Battery.ObjectState = ObjectState.Modified;
                    t.fk_Battery.S_Life = 0;
                    t.fk_Battery.S_CreditAccountId = t.CreditAccountId;
                    t.fk_Battery.S_StatusId = 1202;
                    t.fk_Battery.S_DebitAccountId = t.DebitAccountId;
                    t.fk_Battery.S_DocDate = view.DocumentDate;
                    t.fk_Battery.BatterySerialNo = t.BatterySerialNo;
                    t.fk_Battery.S_VoucherTypeId = view.VoucherTypeId.Value;

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
                crt.BatteryId = xx.BatteryId;
                crt.BatterySerialNo = xx.BatterySerialNo;
                crt.BatteryLife = xx.BatteryLife;
                crt.DocDate = view.DocumentDate;
                crt.DocNo = view.DocumentNo;
                crt.VoucherTypeId = view.VoucherTypeId.GetValueOrDefault();
                crt.ScrapCost = l.CarriedCost;
                crt.BatteryStatusId = 1210;//Claim Passed
                crt.TaxServiceTypeId = l.ServiceTaxTypeId;
                crt.PreviousLogId = xx.Id;
                crt.fk_PreviousLog = xx;
                crt.CreditAccountId = view.PrimaryCreditAccountId;
                crt.DebitAccountId = view.PrimaryDebitAccountId.GetValueOrDefault();

                crt.ObjectState = crt.Id > 0 ? ObjectState.Modified : ObjectState.Added;

                crt.fk_Battery = xx.fk_Battery;

                crt.fk_Battery.S_Life = crt.BatteryLife;
                crt.fk_Battery.S_CreditAccountId = crt.CreditAccountId;
                crt.fk_Battery.S_StatusId = crt.BatteryStatusId;
                crt.fk_Battery.S_DebitAccountId = crt.DebitAccountId;
                crt.fk_Battery.S_DocDate = crt.DocDate;
                // crt.fk_Battery.S_VoucherId = v.Id;
                crt.fk_Battery.S_VoucherTypeId = crt.VoucherTypeId;


                crt.fk_Battery.ObjectState = ObjectState.Modified;


                xx.fk_NextLog = crt;
                xx.NextLogId = crt.Id;
                xx.ObjectState = ObjectState.Modified;

                claimReceiptLog.Add(crt);

                t.PreviousLogId = crt.Id;
                t.fk_PreviousLog = crt;

                newBatteryLogList.Add(t);
                #endregion
            }
            #endregion


            #region Prepare Voucher
            if (view.Id > 0)
            {
                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = _repository.GetRepository<Voucher>().Query(x => x.Id == tei.VoucherId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
            }
            v = v ?? new Voucher();
            v.ConstCurTypeId = view.ConstCurTypeId;
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
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
            v.Account4Id = view.SGSTLedgerId;
            v.Amount4 = view.SGSTAmount;
            v.Account5Id = view.IGSTLedgerId;
            v.Amount5 = view.IGSTAmount;
            v.Account7Id = view.TCSAccountId;
            v.Amount7 = view.TCSAmount;
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";
            PrepareVoucherDetails(_repository, v);

            #endregion

            foreach (var log in newBatteryLogList)
            {
                log.VoucherId = v.Id;
                log.fk_Voucher = v;

                _repository.Insert(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }

            var netamount = newBatteryLogList.Sum(x => x.NetAmount);
            var cgst = view.CalVat ? newBatteryLogList.Sum(x => x.CGSTAmount) : 0;
            var sgst = view.CalVat ? newBatteryLogList.Sum(x => x.SGSTAmount) : 0;
            var igst = view.CalVat ? newBatteryLogList.Sum(x => x.IGSTAmount) : 0;
            var vatamount = cgst + sgst + igst;
            var othamount = newBatteryLogList.Sum(x => x.OtherAmount);
            if (v.Amount1 != netamount)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Battery Total Net Value {netamount} Does't match Voucher Primary Debit Amount {v.Amount1}");
            }
            if (v.Amount2 != -(netamount + vatamount + othamount+view.TCSAmount))
            {
                throw new BusinessException(ErrorCode.GLB106, $"Bill Total Amount {-(netamount + vatamount + othamount+ view.TCSAmount)} Does't match Voucher Primary Credit Amount {v.Amount2}");
            }
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
            _uom.SaveChanges();

            if (v.Id > 0) tei.fk_Voucher = v;
            tei.CalVat = view.CalVat;
            //tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.OfficeId = view.OfficeId;
            if (v.Id > 0) tei.VoucherId = v.Id;
            if (v.Id > 0) tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.TaxServiceTypeId = view.ServiceTaxTypeId;
            tei.CGSTACId = view.CGSTLedgerId;
            tei.SGSTACId = view.SGSTLedgerId;
            tei.IGSTACId = view.IGSTLedgerId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var x in newBatteryLogList)
            {
                //Updating Voucher info in New Logs
                x.VoucherId = v?.Id;
                x.fk_Voucher = v;

                //Updating Battery Master Info
                x.fk_Battery.PurchaseExtraInfoId = tei.Id;
                x.fk_Battery.fk_PurchaseExtraInfo = tei;
                x.fk_Battery.PurchaseLogId = x.Id;

                x.fk_Battery.S_BatteryLogId = x.Id;
                x.fk_Battery.S_ExtraInfoId = tei.Id;
                x.fk_Battery.fk_S_ExtraInfo = tei;
                x.fk_Battery.ObjectState = ObjectState.Modified;

                x.ExtraInfoId = tei.Id;
                x.ExtraInfo = tei;
                x.ObjectState = ObjectState.Modified;

                //Updating tei in claimed passed Battery
                x.fk_PreviousLog.ExtraInfoId = tei.Id;
                x.fk_PreviousLog.ExtraInfo = tei;
                x.fk_PreviousLog.fk_Battery.S_ExtraInfoId = tei.Id;
                x.fk_PreviousLog.fk_Battery.S_BatteryLogId = x.fk_PreviousLog.Id;
                x.fk_PreviousLog.fk_Battery.fk_S_BatteryLog = x.fk_PreviousLog;

                x.fk_PreviousLog.ObjectState = ObjectState.Modified;
                x.fk_PreviousLog.fk_Battery.ObjectState = ObjectState.Modified;
                BatteryRepo.Update(x.fk_Battery);
                _repository.Update(x);
            }

            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id);
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();

                if (deletedLogs.Any(x => x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child DocNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                if (previouslogids.Any())
                {
                    _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                }
                var deletedBatterys = new List<BatteryMaster>();
                foreach (var log in deletedLogs)
                {
                    //restoring Battery record for sent for claimed Battery
                    _repository.Update(RestorePreviousBatteryStatus(log.fk_PreviousLog));

                    //Deleting Claim Battery Log
                    log.fk_PreviousLog.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);

                    //Deleting Current Log
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);

                    //Deleting Battery
                    //log.fk_Battery.ObjectState = ObjectState.Deleted;
                    //BatteryRepo.Delete(log.fk_Battery);
                    log.fk_Battery.S_BatteryLogId = null;
                    log.fk_Battery.PurchaseLogId = null;
                    log.fk_Battery.fk_S_BatteryLog = null;
                    log.fk_Battery.fk_PurchaseBatteryLog = null;
                    log.fk_Battery.ObjectState = ObjectState.Modified;
                    BatteryRepo.Update(log.fk_Battery);
                    deletedBatterys.Add(log.fk_Battery);
                }
                _uom.SaveChanges();
                
                foreach (var t in deletedBatterys)
                {
                    //Deleting Battery
                    t.ObjectState = ObjectState.Deleted;
                    t.S_BatteryLogId = null;
                    t.PurchaseLogId = null;
                    t.fk_S_BatteryLog = null;
                    t.fk_PurchaseBatteryLog = null;
                    BatteryRepo.Delete(t);
                }
            }
            _uom.SaveChanges();
        }

        public vwBatteryBillView GetBatteryRefurbishReceiptBillView(long key)
        {
            return _repository.GetBatteryRefurbishReceiptBillView(key);
        }
        public void InsertUpdateBatteryRefurbishReceipt(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            view.VoucherTypeId = 45;
            if (view.RefurbishReceiptLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
            }

            if (view.PrimaryCreditAccountId <= 0 || view.PrimaryDebitAmount<=0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Battery Vendor Account {view.PrimaryCreditAccountName} & Retraiting Cost is required.");
            }

            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0||view.PrimaryCreditAmount>=0)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Account {view.PrimaryDebitAccountName} & Retraiting Cost is required");
            }

            var vRepo = _uom.RepositoryAsync<Voucher>();
            var teiRepo = _uom.RepositoryAsync<BatteryLogExtraInfo>();
            //var tpiRepo = _uom.RepositoryAsync<BatteryLifePerformanceLog>();
            Voucher v = new Voucher();
            //Collect Distincs ReferenceId's from Posted SpareLogs
            BatteryLogExtraInfo tei = null;
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");
                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = vRepo.Query(x => x.Id == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            if (v == null) throw new BusinessException(ErrorCode.VCH108, $"Voucher: The Transaction you are trying to update, doesn't exist");


            var refurbishReceiptrefids = view.RefurbishReceiptLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var allowedStatus = new long[] {1207};
            List<BatteryLog> RefurbishReceiptReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => refurbishReceiptrefids.Contains(x.Id)).ToList();
            var Batterylist = view.RefurbishReceiptLog.Select(x => x.BatteryId).ToList();
            var purchaseCosts = _uom.RepositoryAsync<BatteryMaster>().Queryable().Where(x=> Batterylist.Contains(x.Id)).Select(x=> new
            {
                x.Id,
                PurchaseCost= x.fk_PurchaseBatteryLog.NetAmount
            }).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var oldBatteryLogs = new List<BatteryLog>();
            foreach (var l in view.RefurbishReceiptLog)
            {
                /************************************************************
                *************||Battery RefurbishReceipt Logics Start||*********************
                *************************************************************/
                #region Battery RefurbishReceipt Logic
                var i = new BatteryLog();//RefurbishReceipt Log
                var ir = RefurbishReceiptReferenceLogs.Find(x => x.Id == l.ReferenceId);//Issue Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Battery No {l.BatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.BatterySerialNo}");
                }
                if (!allowedStatus.Contains(ir.BatteryStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery Status has already been changed. So the Battery No {l.BatterySerialNo} can't be inwarded.");
                }
                if (l.Id > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.Id);//RefurbishReceipt Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                var _BatteryPurchaseCost = purchaseCosts.FirstOrDefault(x => x.Id == l.BatteryId);
                if (_BatteryPurchaseCost != null && l.CarriedCost >= _BatteryPurchaseCost.PurchaseCost)
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"CarriedCost for the Battery No {ir.BatterySerialNo} should be less than purchase cost");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //if Battery has been altered restore all Battery status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                }

                //45  Remoudl received
                //1202 New Battery Stock
                //1207 Sent for remould
                i.TSLId = l.TSLId;
                i.Rate = i.SubTotal = i.NetAmount = l.BatteryCost;
                i.TransferPrice = l.CarriedCost;//added by sanjay
                i.RoundAmount = l.RoundAmount;
                i.OtherAmount = i.DiscountAmount = i.DiscountPercent = 0;
                i.BatteryAge = 0;
               
                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.BatteryId = ir.BatteryId;
                i.ReasonId = ir.ReasonId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = (ir.BatteryLife + 1);
                i.BatteryStatusId = 1202;
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;

                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = ir.BatteryLife;
                i.fk_Battery.S_StatusId = i.BatteryStatusId;

                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                ir.ScrapCost = l.CarriedCost; //added by sanjay//adding carried cost(TP) in scrap value of old record in case of remoulding

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
                    i.fk_Battery.S_BatteryLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newBatteryLogList.Add(i);
                #endregion
            }
            var BatteryRepo = _uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();

                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                foreach (var log in deletedLogs)
                {
                    RestorePreviousBatteryStatus(log);
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }
            #region Prepare Issue Voucher


            var totalRemoudCost = view.RefurbishReceiptLog.Sum(x => x.BatteryCost);
            v.ConstCurTypeId = view.ConstCurTypeId;
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.Value;
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount;
            v.Account1Id = view.PrimaryDebitAccountId;//StoreId
            v.Account2Id = view.PrimaryCreditAccountId;//VendorId
            v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;
            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.Amount7 = view.TCSAmount;
            v.Account7Id = view.TCSAccountId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";

            if (v.Amount1 != totalRemoudCost)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Total Amount {totalRemoudCost} Does't match Voucher Primary Debit Amount {v.Amount1}");
            }
            if (v.Amount2 != -(totalRemoudCost+view.TCSAmount))
            {
                throw new BusinessException(ErrorCode.GLB106, $"Vendor Total Amount {-totalRemoudCost} Does't match Voucher Primary Credit Amount {v.Amount2}");
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

            tei = tei ?? new BatteryLogExtraInfo();
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.TCSAccountId = view.TCSAccountId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.VoucherId= v?.Id;
            tei.fk_Voucher = v;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;


            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newBatteryLogList)
            {
                log.VoucherId = v?.Id;
                log.fk_Voucher = v;
                log.fk_Battery.S_ExtraInfoId = tei.Id;
                log.fk_Battery.S_DocDate = tei.DocDate;
                log.fk_Battery.fk_S_ExtraInfo = tei;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            _uom.SaveChanges();
        }
        public void InsertUpdateBatteryClaimSettlement(vwBatteryBillView view, IUnitOfWorkAsync _uom)
        {
            view.VoucherTypeId = 49;
            if (view.BatteryClaimSettlementLog.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Details is Missing");
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
            var teiRepo = _uom.RepositoryAsync<BatteryLogExtraInfo>();
            Voucher v = new Voucher();

            //Collect Distincs ReferenceId's from Posted SpareLogs
            BatteryLogExtraInfo tei = null;
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (view.Id > 0)
            {
                tei =
                    teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == view.VoucherTypeId);
                if (tei == null) throw new BusinessException(ErrorCode.VCH108, $"The Transaction you are trying to update, doesn't exist");

                //If it is existing transaction try to fatch v/vd/vdr and SpareLabour info from database
                v = vRepo.Query(x => x.Id == tei.VoucherId && x.VoucherTypeId == tei.VoucherTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();

                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_NextLog).Where(x => x.ExtraInfoId == tei.Id && x.VoucherTypeId == tei.VoucherTypeId).ToList();
            }
            if (v == null) throw new BusinessException(ErrorCode.VCH108, $"Voucher: The Transaction you are trying to update, doesn't exist");

            var issuerefids = view.BatteryClaimSettlementLog.Where(x => x.ReferenceId > 0).Select(x => x.ReferenceId).ToList();
            var newBatterystatus = new long[] { 1208 };
            List<BatteryLog> issueReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => issuerefids.Contains(x.Id)).ToList();
            var newBatteryLogList = new List<BatteryLog>();
            var oldBatteryLogs = new List<BatteryLog>();
            foreach (var l in view.BatteryClaimSettlementLog)
            {
                /************************************************************
                *************||Battery Claim Settlement||*********************
                *************************************************************/
                #region Battery Claim Settlement
                var i = new BatteryLog();//Claim Settlement Log
                var ir = issueReferenceLogs.Find(x => x.Id == l.ReferenceId);//Claim Settlement Reference Log
                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log din't found for Battery No {l.BatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.BatterySerialNo}");
                }
                if (!newBatterystatus.Contains(ir.BatteryStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {l.BatterySerialNo} can't be ClaimSettlementd");
                }

                if (l.Id > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.Id);//Issued Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105, $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.BatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                }
                i.TSLId = l.TSLId;
                i.Rate = i.SubTotal = l.BatteryCost;
                i.OtherAmount = 0;
                i.RoundAmount = l.RoundAmount;
                i.NetAmount = l.BatteryCost;
                i.DiscountAmount = i.DiscountPercent = 0;
                i.CreditAccountId = view.PrimaryCreditAccountId;
                i.DebitAccountId = view.PrimaryDebitAccountId.Value;
                i.Remark = l.Remark;
                i.BatteryId = ir.BatteryId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = ir.BatteryLife;
                i.BatteryStatusId = 1223;//ClaimSettlementd
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;
                i.VoucherTypeId = view.VoucherTypeId.Value;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = i.BatteryLife;
                i.fk_Battery.S_StatusId = i.BatteryStatusId;//ClaimSettlementd
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
                    i.fk_Battery.S_BatteryLogId = i.Id;
                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newBatteryLogList.Add(i);
                #endregion
            }
            var BatteryRepo = _uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                var newLogsIds = newBatteryLogList.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId.GetValueOrDefault(0) > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var previouslogids = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => previouslogids.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    _repository.Update(RestorePreviousBatteryStatus(log));
                    log.ObjectState = ObjectState.Deleted;
                    _repository.Delete(log);
                }

                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }
            #region Prepare Issue Voucher
            var totalAmt = view.BatteryClaimSettlementLog.Sum(x => x.BatteryCost);
            v.ConstCurTypeId = view.ConstCurTypeId;
            v.CurTypeId = view.CurTypeId;
            v.CurRate = view.CurRate;
            v.VoucherDate = view.DocumentDate;
            v.VoucherDateTime = view.DocumentDate;
            v.VoucherTypeId = view.VoucherTypeId.Value;
            v.VoucherNo = view.DocumentNo;
            v.Amount1 = view.PrimaryDebitAmount;
            v.Account1Id = view.PrimaryDebitAccountId;
            v.Account2Id = view.PrimaryCreditAccountId;
            v.Amount2 = view.PrimaryCreditAmount > 0 ? -view.PrimaryCreditAmount : view.PrimaryCreditAmount;

            v.UserRemark = view.Narration;
            v.OfficeId = view.OfficeId;
            v.ObjectState = v.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //TODO:Setup Account Narration from Template located with VoucherType
            v.AccountingRemark = "";

            if (v.Amount1 != totalAmt)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Battery Total Net Value {totalAmt} Does't match Voucher Primary Debit Amount {v.Amount1}");
            }
            if (v.Amount2 != -totalAmt)
            {
                throw new BusinessException(ErrorCode.GLB106, $"Store Credit Amount {-totalAmt} Does't match Voucher Primary Credit Amount {v.Amount2}");
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
            tei = tei ?? new BatteryLogExtraInfo();
            tei.fk_Voucher = v;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.VoucherId = v.Id;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = view.VoucherTypeId.Value;
            tei.OfficeId = view.OfficeId;
            tei.Remark = view.Narration;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            foreach (var log in newBatteryLogList)
            {
                log.VoucherId = v.Id;
                log.fk_Voucher = v;
                log.fk_Battery.S_ExtraInfoId = tei.Id;
                log.fk_Battery.S_DocDate = tei.DocDate;
                log.fk_Battery.fk_S_ExtraInfo = tei;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                _repository.Update(log.fk_PreviousLog);
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            _uom.SaveChanges();
        }

        public void InsertUpdateReceipt(vwBatteryBillView view, IUnitOfWorkAsync uom)
        {
            if (view.ReceiptLogs.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Receipt Details is Missing");
            }
            if (view.PrimaryDebitAccountId.GetValueOrDefault() <= 0)
            {
                throw new BusinessException(ErrorCode.GLB106,
                    $"Primary Debit Account {view.PrimaryDebitAccountId} has Invalid Value.");
            }

            #region Check Circuler Reference for Batterys

            //var duplicatecheckgroup = view.IssueReceiptLogs.GroupBy(x => new { x.IssueBatteryId, x.ReceiptBatteryId });

            var groupbyvehicle = view.ReceiptLogs.GroupBy(x => x.VehicleId).ToList();
            if (
                groupbyvehicle.Select(grouping => grouping.Select(x => x.ReceiptBatteryId).ToList())
                    .Any(receiptlist => receiptlist.GroupBy(x => x).Any(x => x.Count() > 1)))
            {
                throw new BusinessException(ErrorCode.GLB106,
                    "Same Battery can't be received more than one in Single Transaction.");
            }

            #endregion

            var teiRepo = uom.RepositoryAsync<BatteryLogExtraInfo>();
            var tpiRepo = uom.RepositoryAsync<BatteryLifePerformanceLog>();
            //var BatteryCheckRepo = uom.RepositoryAsync<BatteryCheck>();
            BatteryLogExtraInfo tei = new BatteryLogExtraInfo();
            if (view.Id > 0)
            {
//Try to find existing Battery extra info record
                tei = teiRepo.Queryable()
                    .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == 51);
            }
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (tei != null && tei.Id > 0)
            {
                //In-case updating existing record find all existing attached Battery Logs
                existingBatteryLogs =
                    _repository.Queryable()
                        .Include(x => x.fk_Battery)
                        .Include(x => x.fk_BatteryCheck)
                        .Include(x => x.fk_PreviousLog)
                        .Include(x => x.fk_NextLog)
                        .Where(x => (x.ExtraInfoId == tei.Id) && x.VoucherTypeId == 51)
                        .ToList();
            }
            //Extract Ids of Primary Key
            var oldreceiptids =
                existingBatteryLogs.Where(x => x.VoucherTypeId == 51 && x.Id > 0).Select(x => x.Id).ToList();

            var receptrefids = view.ReceiptLogs.Select(x => x.ReceiptReferenceId).ToList();

            //Fatch Battery Performance Logs in case updating record
            var receiptBatteryPerformance =
                tpiRepo.Queryable().Where(x => oldreceiptids.Contains(x.LastReceiptLogId.Value)).ToList();

            List<BatteryLog> receiptReferenceLogs =
                _repository.Queryable().Include(x => x.fk_Battery).Where(x => receptrefids.Contains(x.Id)).ToList();

            //Fatch Battery Performance Logs for fresh receipt so that we could update LastReceiptLogId
            var receiptLogBatteryPerformanceIds =
                receiptReferenceLogs.Select(x => x.BatteryId + "-" + x.BatteryLife).ToList();
            //TODO:Check if It works
            var receiptTpData =
                tpiRepo.Queryable().Include(x=>x.fk_FirstIssueLog).Include(x=>x.fk_LastReceiptLog)
                    .Where(x => receiptLogBatteryPerformanceIds.Contains(x.BatteryId + "-" + x.Life))
                    .ToList();

            var oldBatteryLogs = new List<BatteryLog>();
            var newReceiptLogs = new List<BatteryLog>();
            var newBatteryPerformance = new List<BatteryLifePerformanceLog>();
            foreach (var l in view.ReceiptLogs)
            {

                /************************************************************
                *************||Battery Receipt Logics Start||*******************
                *************************************************************/

                #region Battery Receipt Logic

                var r = new BatteryLog();
                var rr = receiptReferenceLogs.Find(x => x.Id == l.ReceiptReferenceId);

                if (rr == null || rr.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101,
                        $"Previous Log din't found for Battery No {l.ReceiptBatterySerialNo}");
                }

                if (rr.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.ReceiptBatterySerialNo}");
                }

                if (l.ReceiptLogId > 0)
                {
                    r = existingBatteryLogs.Find(x => x.Id == l.ReceiptLogId);
                    if (r.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.ReceiptBatterySerialNo}[Referenced Transaction No :{r.fk_NextLog.DocNo}]");
                    }
                }

                if (rr != null && rr.NextLogId > 0 && r.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106,
                        $"Invalid Reference for Battery No {rr.BatterySerialNo}.");
                }

                //Check if Battery No has been altered
                if (r.Id > 0 && r.PreviousLogId != rr.Id)
                {
                    //Restore Last Receipt Log Id when Receipt Battery No Changed
                    var td = receiptBatteryPerformance.FirstOrDefault(x => x.LastReceiptLogId == r.Id);
                    if (td != default(BatteryLifePerformanceLog))
                    {
                        var lastReceipt = _repository.GetLastBatteryLogByStatusAndLife(r.BatteryId, new long[] { 51 }, r.BatteryLife,
                            r.Id);
                        td.LastReceiptLogId = lastReceipt?.Id;
                        td.ObjectState = ObjectState.Modified;
                        tpiRepo.Update(td);
                    }
                    //if Battery has been altered restore all Battery status to previous logs status
                    //oldBatteryLogs.Add(RestorePreviousBatteryStatus(r));
                    //r.fk_Battery = null;
                    //r.BatteryId = 0;
                    //r.fk_PreviousLog = null;
                    //r.PreviousLogId = null;
                }

                r.CreditAccountId = rr.DebitAccountId;
                r.IgnoreValidation = true;
                r.NetAmount = r.Rate = r.SubTotal = l.ReceiptAmount;
                r.DiscountAmount = r.OtherAmount = r.DiscountPercent = 0;
                r.JobsheetId = l.JobSheetId;
                r.BatteryAge = view.DocumentDate.Subtract(rr.DocDate).Days; //Calculate Difference
                r.MechanicId = l.MechanicId;
                r.CreditAccountId = rr.DebitAccountId;
                    //l.OwnerId.GetValueOrDefault(0) == 0 ? rr.CreditAccountId : l.OwnerId.GetValueOrDefault(0);
                r.DebitAccountId = view.PrimaryDebitAccountId.Value;
                r.Remark = l.ReceiptRemark;
                r.BatteryId = l.ReceiptBatteryId;
                r.fk_Battery = rr.fk_Battery;
                r.ReasonId = l.ReasonId;
                r.NextUseId = l.NextUseId;
                r.BatteryLife = rr.BatteryLife;
                r.BatteryStatusId = 1203;
                r.BatterySerialNo = rr.fk_Battery.BatterySerialNo;
                r.VoucherTypeId = 51;
                r.DocDate = view.DocumentDate;
                r.DocNo = view.DocumentNo;
                r.VehicleId = l.VehicleId;
                r.NextUseId = l.NextUseId;
                r.fk_Battery.S_VoucherTypeId = r.VoucherTypeId;
                r.fk_Battery.ObjectState = ObjectState.Modified;
                r.fk_Battery.S_DocDate = r.DocDate;
                r.fk_Battery.S_CreditAccountId = r.CreditAccountId;
                r.fk_Battery.S_DebitAccountId = r.DebitAccountId;
                r.fk_Battery.S_Life = rr.BatteryLife;
                r.fk_Battery.S_StatusId = r.BatteryStatusId;
                r.PreviousLogId = rr.Id;
                r.fk_PreviousLog = rr;
                rr.NextLogId = r.Id;
                rr.fk_NextLog = r;
                //#region BatteryCheck Receipt
                //if (r.fk_BatteryCheck == null || r.fk_BatteryCheck.Id == 0)
                //{
                //    r.fk_BatteryCheck = new BatteryCheck();
                //}
                //r.fk_BatteryCheck.AirPressure = 0;
                //r.fk_BatteryCheck.CheckDate = view.DocumentDate;
                //r.fk_BatteryCheck.KmRun = l.ReceiptKmRun;
                //r.fk_BatteryCheck.Remarks = l.ReceiptRemark;
                //r.fk_BatteryCheck.TreadDepth = l.ReceiptTreadWear;
                //r.fk_BatteryCheck.BatteryId = r.BatteryId;
                //r.fk_BatteryCheck.VehicleId = r.VehicleId.Value;
                //r.fk_BatteryCheck.fk_Battery = r.fk_Battery;
                //r.fk_BatteryCheck.ObjectState = r.fk_BatteryCheck.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //#endregion
                if (r.Id > 0)
                {
                    r.ObjectState = ObjectState.Modified;
                    r.fk_Battery.S_BatteryLogId = r.Id;

                }
                else
                {
                    r.ObjectState = ObjectState.Added;
                    r.fk_Battery.fk_S_BatteryLog = r;
                }
                newReceiptLogs.Add(r);

                #endregion

            }
            var BatteryRepo = uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {


                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }

            tei = tei ?? new BatteryLogExtraInfo();
            tei.OfficeId = view.OfficeId;
            tei.CalVat = view.CalVat;
            //tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = null;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = 51;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.Remark = view.Narration;
            tei.ViewId = view.ViewId;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);

            foreach (var log in newReceiptLogs)
            {
                //Extract Battery Performance for current record in loop and set LastReceiptLog as this
                var tpd = receiptTpData.FirstOrDefault(x => x.Life == log.BatteryLife && log.BatteryId == x.BatteryId);
                if (tpd != null && ((tpd.LastReceiptLogId.HasValue && tpd.LastReceiptLogId < log.Id) || log.Id == 0))
                {
                    tpd.LastReceiptLogId = log.Id;
                    tpd.fk_LastReceiptLog = log;
                    tpd.ObjectState = ObjectState.Modified;
                    newBatteryPerformance.Add(tpd);

                }
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                log.ObjectState = log.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                ;
                _repository.Update(log.fk_PreviousLog);
                //if (log.fk_BatteryCheck.Id > 0)
                //{
                //    BatteryCheckRepo.Update(log.fk_BatteryCheck);
                //}
                //else
                //{
                //    BatteryCheckRepo.Insert(log.fk_BatteryCheck);
                //}
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            uom.SaveChanges();
            if (view.Id > 0)
            {
                var newLogsIds = newReceiptLogs.Where(x => x.Id > 0).Select(x => x.Id).Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108,
                        $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child DocNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                if (deletedIds.Any())
                {
                    _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                }
                
                foreach (var log in deletedLogs)
                {
                    var td =
                        receiptBatteryPerformance.FirstOrDefault(
                            x => x.LastReceiptLogId == log.Id || x.FirstIssueLogId == log.Id);
                    if (td != default(BatteryLifePerformanceLog))
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
                                throw new BusinessException(ErrorCode.GLB106,
                                    "Cannot Delete Battery Performance Log as it is Locked");
                            }
                            //Find Last Log Other than current in loop
                            BatteryLog lstTl = _repository.GetLastBatteryLogByStatusAndLife(log.BatteryId, (td.FirstIssueLogId == log.Id ? new long[] { 43, 95, 45, 48, 57 } : new long[] { 50 }), log.BatteryLife, log.Id);
                            td.LastReceiptLogId = lstTl?.Id;
                            td.ObjectState = ObjectState.Modified;
                            tpiRepo.Update(td);
                        }

                    }

                    //if (log.fk_BatteryCheck != null && log.fk_BatteryCheck.Id > 0)
                    //{
                    //    log.fk_BatteryCheck.ObjectState = ObjectState.Deleted;
                    //    BatteryCheckRepo.Delete(log.fk_BatteryCheck);
                    //}
                    log.IssueReceiptId = null;
                    log.fk_IssueReceipt = null;
                    _repository.Update(RestorePreviousBatteryStatus(log));
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
            foreach (var log in newBatteryPerformance)
            {
                log.BatteryId = log.fk_FirstIssueLog.BatteryId;
                log.fk_Battery = log.fk_FirstIssueLog.fk_Battery;
                if (log.Id > 0) tpiRepo.Update(log);
                else tpiRepo.Insert(log);
            }
            uom.SaveChanges();

        }

        public void InsertUpdateIssue(vwBatteryBillView view, IUnitOfWorkAsync uom)
        {
            if (view.IssueLogs.Count == 0)
            {
                throw new BusinessException(ErrorCode.GLB106, "Battery Issue Log Details is Missing");
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

            #region Check Circuler Reference for Batterys
            //var duplicatecheckgroup = view.IssueReceiptLogs.GroupBy(x => new { x.IssueBatteryId, x.ReceiptBatteryId });

            var groupbyvehicle = view.IssueLogs.GroupBy(x => x.VehicleId).ToList();
            if (groupbyvehicle.Select(grouping => grouping.Select(x => x.IssueBatteryId).ToList()).Any(issuelist => issuelist.GroupBy(x => x).Any(x => x.Count() > 1)))
            {
                throw new BusinessException(ErrorCode.GLB106, "Same Battery can't be issued more than one in Single Transaction.");
            }

            long PricipalOwnerId = 0;
            long VehicleOwnerId = 0;
            if (view.VoucherTypeId == 50)
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


            #endregion
            var vRepo = uom.RepositoryAsync<Voucher>();
            var teiRepo = uom.RepositoryAsync<BatteryLogExtraInfo>();
            var tpiRepo = uom.RepositoryAsync<BatteryLifePerformanceLog>();
            var BatteryCheckRepo = uom.RepositoryAsync<BatteryCheck>();
            BatteryLogExtraInfo tei = new BatteryLogExtraInfo();
            
            if (view.Id > 0)
            {//Try to find existing Battery extra info record
                tei = teiRepo.Queryable()
                        .FirstOrDefault(x => x.Id == view.Id && x.VoucherTypeId == 50);
            }
            if (view.Id > 0 && tei != null && vRepo.Queryable().Any(x => x.Id == tei.VoucherId && x.VoucherTypeId == 50))
            {
                //Try to find existing voucher record
                v = vRepo.Queryable().Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).FirstOrDefault(x => x.Id == tei.VoucherId && x.VoucherTypeId == 50);
            }
            List<BatteryLog> existingBatteryLogs = new List<BatteryLog>();
            if (tei != null && tei.Id > 0)
            {
                //In-case updating existing record find all existing attached Battery Logs
                existingBatteryLogs = _repository.Queryable().Include(x => x.fk_Battery).Include(x => x.fk_BatteryCheck).Include(x => x.fk_PreviousLog).Include(x => x.fk_NextLog).Where(x => (x.ExtraInfoId == tei.Id) && x.VoucherTypeId == 50).ToList();
            }

            //Extract Ids of Primary Key
            var oldissueids = existingBatteryLogs.Where(x => x.VoucherTypeId == 50 && x.Id > 0).Select(x => x.Id).ToList();

            var issuerefids = view.IssueLogs.Select(x => x.IssueReferenceId).ToList();

            //Fatch Battery Performance Logs in case updating record
            var issueBatteryPerformance = tpiRepo.Queryable().Where(x => oldissueids.Contains(x.FirstIssueLogId.Value)).ToList();

            List<BatteryLog> issueReferenceLogs = _repository.Queryable().Include(x => x.fk_Battery).Where(x => issuerefids.Contains(x.Id)).ToList();

            var newBatteryStatus = new long[] { 1202,1203 };//New & Old Batteries can be issued
            decimal issueNetamount = 0;//issueReferenceLogs.Where(x => newBatteryStatus.Contains(x.BatteryStatusId)).Sum(x => x.SubTotal);

            var cv = issueReferenceLogs.Any(x => newBatteryStatus.Contains(x.BatteryStatusId) && x.NetAmount > 0)&& view.PrimaryDebitAmount != 0;
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
            var oldBatteryLogs = new List<BatteryLog>();
            var newIssuedLogs = new List<BatteryLog>();
            var newBatteryPerformance = new List<BatteryLifePerformanceLog>();
            foreach (var l in view.IssueLogs)
            {
                /************************************************************
                *************||Battery Issue Logics Start||*********************
                *************************************************************/
                #region Battery Issue Logic
                issueNetamount += l.IssueAmount;

                var i = new BatteryLog(); //Issued Log
                var ir = issueReferenceLogs.Find(x => x.Id == l.IssueReferenceId);//Issue Reference Log

                if (ir == null || ir.Id == 0)
                {
                    throw new BusinessException(ErrorCode.TYR101, $"Previous Log didn't found for Battery No {l.IssueBatterySerialNo}");
                }
                if (ir.VoucherTypeId == view.VoucherTypeId)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Previous Log voucher type and current Voucher Type should be different for Battery No {l.IssueBatterySerialNo}");
                }
                if (l.IssueLogId > 0)
                {
                    i = existingBatteryLogs.Find(x => x.Id == l.IssueLogId);//Issued Log
                    if (i.NextLogId > 0)
                    {
                        throw new BusinessException(ErrorCode.GLB105,
                            $"Cannot Modify Battery Log Information that has been referenced/issued.Ref Battery No:{l.IssueBatterySerialNo}[Referenced Transaction No :{i.fk_NextLog.DocNo}]");
                    }
                }
                if (ir.NextLogId > 0 && i.Id == 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery No {ir.BatterySerialNo} is out of stock");
                }
                //Check if Battery No has been altered
                if (i.Id > 0 && i.PreviousLogId != ir.Id)
                {
                    //var tp = issueBatteryPerformance.FirstOrDefault(x => x.FirstIssueLogId == i.Id);
                    //tp.ObjectState=ObjectState.Deleted;
                    //tpiRepo.Delete(tp);
                    //if Battery has been altered restore all Battery status to previous logs status
                    throw new BusinessException(ErrorCode.GLB106, "Change of Battery Number Not allowed.Use Delete Option in-case of Battery Number Change.");
                }
                i.TSLId = l.TSLId;
                i.IssueReceiptId = l.ReceiptLogId;
                i.IgnoreValidation = true;
                i.NetAmount = i.Rate = i.SubTotal = l.IssueAmount;
                i.DiscountAmount = i.OtherAmount = i.DiscountPercent = 0;
                i.JobsheetId = l.JobSheetId;
                i.BatteryAge = 0;
                i.MechanicId = l.MechanicId;
                i.CreditAccountId = ir.DebitAccountId;//view.PrimaryDebitAccountId.GetValueOrDefault(0);
                i.DebitAccountId = view.PrimaryCreditAccountId;
                i.Remark = l.IssueRemark;
                i.BatteryId = ir.BatteryId;
                i.fk_Battery = ir.fk_Battery;
                i.BatteryLife = ir.BatteryLife;
                i.BatteryStatusId = 1205;//OnVehicle
                i.BatterySerialNo = i.fk_Battery.BatterySerialNo;
                i.VoucherTypeId = 50;
                i.DocDate = view.DocumentDate;
                i.DocNo = view.DocumentNo;
                i.VehicleId = l.VehicleId;
                i.fk_Battery.S_VoucherTypeId = i.VoucherTypeId;
                i.fk_Battery.ObjectState = ObjectState.Modified;
                i.fk_Battery.S_DocDate = i.DocDate;
                i.fk_Battery.S_CreditAccountId = i.CreditAccountId;
                i.fk_Battery.S_DebitAccountId = i.DebitAccountId;
                i.fk_Battery.S_Life = ir.BatteryLife;
                i.fk_Battery.S_StatusId = 1205;//OnVehicle
                i.PreviousLogId = ir.Id;
                i.fk_PreviousLog = ir;
                ir.fk_NextLog = i;
                ir.NextLogId = i.Id;
                //if (!string.IsNullOrWhiteSpace(l.IssueRowVersionId))
                //{
                //    i.RowVersion = Encoding.UTF8.GetBytes(l.IssueRowVersionId);
                //}
                #region BatteryCheck Issue
                if (i.fk_BatteryCheck == null || i.fk_BatteryCheck.Id == 0)
                {
                    i.fk_BatteryCheck = new BatteryCheck();
                }
                i.fk_BatteryCheck.IsTerminalCarbonChecked = l.IsTerminalCarbonChecked;
                i.fk_BatteryCheck.IsWaterLevelChecked = l.IsWaterLevelChecked;
                i.fk_BatteryCheck.GravityLevel = l.GravityLevel;
                i.fk_BatteryCheck.CheckDate = view.DocumentDate;
                i.fk_BatteryCheck.Remarks = l.IssueRemark;
                i.fk_BatteryCheck.BatteryId = i.BatteryId;
                i.fk_BatteryCheck.VehicleId = i.VehicleId.Value;
                i.fk_BatteryCheck.fk_Battery = i.fk_Battery;
                i.fk_BatteryCheck.ObjectState = i.fk_BatteryCheck.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                #endregion
                if (i.Id > 0)
                {

                    i.ObjectState = ObjectState.Modified;
                    i.fk_Battery.S_BatteryLogId = i.Id;

                }
                else
                {
                    i.ObjectState = ObjectState.Added;
                    i.fk_Battery.fk_S_BatteryLog = i;
                }
                newIssuedLogs.Add(i);
                #endregion
            }
            var BatteryRepo = uom.RepositoryAsync<BatteryMaster>();
            if (view.Id > 0)
            {
                if (oldBatteryLogs.Any())
                {
                    foreach (var log in oldBatteryLogs)
                    {
                        log.ObjectState = ObjectState.Modified;
                        log.fk_Battery.ObjectState = ObjectState.Modified;
                        _repository.Update(log);
                        BatteryRepo.Update(log.fk_Battery);
                    }
                }
            }
            if (cv)
            {

                #region Prepare Issue Voucher
                v.ConstCurTypeId = view.ConstCurTypeId;
                v.CurTypeId = view.CurTypeId;
                v.CurRate = view.CurRate;
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
                //v.Account4Id = view.OtherLedgerId;
                //v.Amount4 = view.OtherAmount;
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

                if (v.Amount1 != issueNetamount)
                {
                    throw new BusinessException(ErrorCode.GLB106, $"Battery Total Net Value {issueNetamount} Does't match Voucher Primary Debit Amount {v.Amount1}");
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
                        .Where(x => x.Id == 50)
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
            tei = tei ?? new BatteryLogExtraInfo();
            if (cv) tei.fk_Voucher = v;
            tei.OfficeId = view.OfficeId;
            tei.CalVat = view.CalVat;
            //tei.CalOthAmt = view.CalOthAmt;
            tei.VendorReferenceNo = view.VendorReferenceNo;
            if (cv) tei.VoucherId = v.Id;
            tei.DocDate = view.DocumentDate;
            tei.CrAccountId = view.PrimaryCreditAccountId;
            tei.DrAccountId = view.PrimaryDebitAccountId;
            tei.VoucherTypeId = 50;
            tei.DocNo = view.DocumentNo;
            tei.PageId = view.PageId;
            tei.Remark = view.Narration;
            tei.ObjectState = tei.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            tei.ViewId = view.ViewId;
            tei.TCSAmount = view.TCSAmount;
            tei.TCSRate = view.TCSRate;
            tei.TCSAccountId = view.TCSAccountId;
            tei.ConstCurTypeId = view.ConstCurTypeId;
            tei.CurTypeId = view.CurTypeId;
            tei.CurRate = view.CurRate;

            if (tei.Id > 0) teiRepo.Update(tei);
            else teiRepo.Insert(tei);
            var mlids = newIssuedLogs.Select(x => $"{(x.BatteryLife - 1)}-{x.BatteryId}").ToList();
            var ageList = tpiRepo.Queryable().Where(x => mlids.Contains((x.Life + "-" + x.BatteryId))).Select(x => new { Mileage = x.PreviousAge + x.LifeAge, x.BatteryId }).ToList();
            foreach (var log in newIssuedLogs)
            {
                log.fk_Battery.S_ExtraInfoId = tei.Id;
                log.fk_Battery.S_DocDate = tei.DocDate;
                log.fk_Battery.fk_S_ExtraInfo = tei;
                log.ExtraInfoId = tei.Id;
                log.ExtraInfo = tei;
                
                if (cv)
                {
                    log.VoucherId = v.Id;
                    log.fk_Voucher = v;
                }
                else
                {//If Voucher is not applicable set voucher values as null
                    log.VoucherId = null;
                    log.fk_Voucher = null;
                }
                //Only Create Battery Performance in case Battery is issued first time
                if (log.fk_PreviousLog.BatteryStatusId == 1202)
                {
                    var tpi = issueBatteryPerformance.FirstOrDefault(x => x.FirstIssueLogId == log.Id) ?? new BatteryLifePerformanceLog();
                    if (tpi.Id == 0) tpi.FirstIssueLogId = log.Id;
                    tpi.CurrentAge = 0;
                    tpi.Life = log.BatteryLife;
                    tpi.LifeAge = 0;
                    tpi.LifeStartDate = log.DocDate;
                    var batteryage = ageList.FirstOrDefault(x => x.BatteryId == log.BatteryId);
                    tpi.PreviousAge = batteryage?.Mileage ?? 0;
                    tpi.PurchaseAmount = log.NetAmount;
                    tpi.SupplierId = log.DebitAccountId;
                    tpi.LifeEndDate = null;
                    tpi.fk_FirstIssueLog = log;
                    tpi.ObjectState = tpi.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    newBatteryPerformance.Add(tpi);
                }
                
                log.fk_PreviousLog.ObjectState = ObjectState.Modified;
                log.ObjectState = log.Id > 0 ? ObjectState.Modified : ObjectState.Added; ;
                _repository.Update(log.fk_PreviousLog);
                if (log.fk_BatteryCheck.Id > 0)
                {
                    BatteryCheckRepo.Update(log.fk_BatteryCheck);
                }
                else
                {
                    BatteryCheckRepo.Insert(log.fk_BatteryCheck);
                }
                if (log.Id > 0)
                {
                    _repository.Update(log);
                    BatteryRepo.Update(log.fk_Battery);
                }
                else
                {
                    _repository.Insert(log);
                    BatteryRepo.Insert(log.fk_Battery);
                }
            }
            uom.SaveChanges();
            if (view.Id > 0)
            {
                var newLogsIds = newIssuedLogs.Where(x => x.Id > 0).Select(x => x.Id).ToList();
                newLogsIds = newLogsIds.Distinct().ToList();
                var deletedLogs = existingBatteryLogs.Where(x => !newLogsIds.Contains(x.Id)).ToList();
                if (deletedLogs.Any(x => x.NextLogId > 0))
                {
                    throw new BusinessException(ErrorCode.GLB108, $"Cannot Delete Battery Log Information that has been referenced/issued.{Environment.NewLine}Ref Battery Nos=>Child VoucherNo:{deletedLogs.Where(x => x.fk_NextLog != null).Select(x => x.BatterySerialNo + "=>" + x.fk_NextLog.DocNo).JoinStrings("," + Environment.NewLine)}");
                }
                var deletedIds = deletedLogs.Where(x => x.fk_PreviousLog == null).Select(x => x.PreviousLogId).ToList();
                _repository.Queryable().Where(x => deletedIds.Contains(x.Id)).Load();
                foreach (var log in deletedLogs)
                {
                    if (log.fk_BatteryCheck != null && log.fk_BatteryCheck.Id > 0)
                    {
                        log.fk_BatteryCheck.ObjectState = ObjectState.Deleted;
                        BatteryCheckRepo.Delete(log.fk_BatteryCheck);
                    }
                    log.IssueReceiptId = null;
                    log.fk_IssueReceipt = null;
                    _repository.Update(RestorePreviousBatteryStatus(log));
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
            foreach (var log in newBatteryPerformance)
            {
                log.BatteryId = log.fk_FirstIssueLog.BatteryId;
                log.fk_Battery = log.fk_FirstIssueLog.fk_Battery;
                if (log.Id > 0) tpiRepo.Update(log);
                else tpiRepo.Insert(log);
            }

            var tpt = uom.RepositoryAsync<TPTRequestPool>();
            try
            {
                if (view.Id == 0 && PricipalOwnerId > 0 && VehicleOwnerId > 0 && (PricipalOwnerId != VehicleOwnerId))
                {
                    TPTRequestPool tpr = new TPTRequestPool();
                    tpr.ObjectState = ObjectState.Added;
                    tpr.RequestId = Guid.NewGuid().ToString();
                    tpr.ViewId = tei.ViewId.GetValueOrDefault();
                    tpr.RecordId = tei.Id;
                    tpr.DocNo = tei.DocNo;
                    tpr.BatchId = tpr.RequestId;
                    tpr.IsProceeded = false;
                    tpr.CreatedTime = DateTime.Now;

                    tpr.TypeKey = "ZRA_BAT_ISSUE_SALE";
                    tpt.Insert(tpr);
                }
            }
            catch (SqlException ex)
            {
                throw new Exception(ex.Message);
            }

            uom.SaveChanges();
        }
    }
}
