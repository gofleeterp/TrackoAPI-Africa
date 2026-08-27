using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoAPI.Code.Logics.FMS;

namespace TrackoAPI.Code.Logics.BMS
{
    public class CNBillPaymentLogCoreLogic : BaseLogic<CNBillPaymentLog>
    {
        //protected static CNBillPaymentLogCoreLogic _Instance;
        //public static CNBillPaymentLogCoreLogic Instance => _Instance ?? (_Instance = new CNBillPaymentLogCoreLogic());

        //protected IDataContextAsync _db;
        //public override IBaseLogic Bind(IDataContextAsync db)
        //{
        //    _db = db;
        //    return this;
        //}

        //public override void Execute(DbEntityEntry entry)
        //{
        //    Execute(entry, false);
        //}

        /// <summary>
        /// Executes the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="isPostLogicCall">if set to <c>true</c> [is post logic call].</param>
        public override void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {/*
          1432	Payment
          1433	Deduction
          1434	OnAccount
          1443	CN Advance
          */
            if (!(entry.Entity is CNBillPaymentLog cnBillPaymentLog)) return;
            if (cnBillPaymentLog.ObjectState == ObjectState.Modified ||
                 cnBillPaymentLog.ObjectState == ObjectState.Added)
            {
                if (cnBillPaymentLog.TypeId == 1432/*Payment*/&& cnBillPaymentLog.BillLogId.GetValueOrDefault(0)>0)
                {
                    var billlog =
                        _db.Set<CNBillLog>().FirstOrDefault(x => x.Id == cnBillPaymentLog.BillLogId);
                    if (billlog == null)
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                            "CE:Invalid Bill Selected for Money Receipt");
                    }


                    var paidamt =
                        DbSet
                            .Where(x => x.BillLogId == cnBillPaymentLog.BillLogId && cnBillPaymentLog.Id != x.Id && x.TypeId != 1433)
                            .Sum(x => (decimal?)x.Amount);
                    if (billlog.TotalBillAmount < (paidamt.GetValueOrDefault() + cnBillPaymentLog.Amount))
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                            "CE:Insufficient Balance Amount for One of CN");
                    }
                    /*balance logic changed by sanjay on dated: 2020-05-12*/
                    billlog.BalanceAmount = billlog.TotalBillAmount - (paidamt.GetValueOrDefault() + cnBillPaymentLog.Amount);
                    billlog.ObjectState = ObjectState.Modified;
                    _db.Set<CNBillLog>().AddOrUpdate(billlog);
                }
                else if (cnBillPaymentLog.TypeId == 1432/*Payment*/&& cnBillPaymentLog.OnAccountRefId.GetValueOrDefault(0) > 0)
                {
                    var mrsInCuntext = _db.ChangeTracker.Entries<CNBillPaymentLog>()
                        .Select(x => x.Entity)
                        .Where(x => x.OnAccountRefId == cnBillPaymentLog.OnAccountRefId)
                        .Select(x => new
                        {
                            x.Amount,
                            x.Id
                        }).ToList();
                    var totalofContext = mrsInCuntext.Sum(x => x.Amount);
                    var idsincontext = mrsInCuntext.Select(x => x.Id).Distinct().ToList();
                    if (cnBillPaymentLog.fk_OnAccountRef == null)
                    {
                        cnBillPaymentLog.fk_OnAccountRef =
                            DbSet.Find(cnBillPaymentLog.OnAccountRefId);
                    }
                    if (cnBillPaymentLog.fk_OnAccountRef == null)
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                            "Invalid OnAccount MR Selected for Settlement");
                    }
                    var onAccountMrLog = DbSet
                        .Where(x => x.Id == cnBillPaymentLog.OnAccountRefId)
                        .Select(
                            x =>
                                x.OnAcSettlements.Where(y => !idsincontext.Contains(y.Id))
                                    .Sum(z => (decimal?)z.Amount)).FirstOrDefault();

                    if (cnBillPaymentLog.fk_OnAccountRef.Amount <
                        (onAccountMrLog.GetValueOrDefault() + totalofContext))
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                            "Insufficient OnAccount Balance Amount.");
                    }
                    cnBillPaymentLog.fk_OnAccountRef.OnAccountBalanceAmount = cnBillPaymentLog.fk_OnAccountRef.Amount - (onAccountMrLog.GetValueOrDefault() + totalofContext);
                    cnBillPaymentLog.fk_OnAccountRef.ObjectState = ObjectState.Modified;
                }
                else if (cnBillPaymentLog.TypeId == 1433/*Deduction(Against DebitNote)*/&& cnBillPaymentLog.OnAccountRefId.GetValueOrDefault(0) > 0)
                {
                    var mrsInCuntext = _db.ChangeTracker.Entries<CNBillPaymentLog>()
                        .Select(x => x.Entity)
                        .Where(x => x.OnAccountRefId == cnBillPaymentLog.OnAccountRefId)
                        .Select(x => new
                        {
                            x.Amount,
                            x.Id
                        }).ToList();
                    var totalofContext = mrsInCuntext.Sum(x => x.Amount);
                    var idsincontext = mrsInCuntext.Select(x => x.Id).Distinct().ToList();
                    if (cnBillPaymentLog.fk_OnAccountRef == null)
                    {
                        cnBillPaymentLog.fk_OnAccountRef =
                            DbSet.Find(cnBillPaymentLog.OnAccountRefId);
                    }
                    if (cnBillPaymentLog.fk_OnAccountRef == null)
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                            "Invalid Deduction Reference Selected for Settlement");
                    }
                    var onAccountMrLog = DbSet
                        .Where(x => x.Id == cnBillPaymentLog.OnAccountRefId)
                        .Select(
                            x =>
                                x.OnAcSettlements.Where(y => !idsincontext.Contains(y.Id))
                                    .Sum(z => (decimal?)z.Amount)).FirstOrDefault();

                    if (cnBillPaymentLog.fk_OnAccountRef.Amount <
                        (onAccountMrLog.GetValueOrDefault() + totalofContext))
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                            "Insufficient Deduction Reference Balance Amount.");
                    }
                    cnBillPaymentLog.fk_OnAccountRef.OnAccountBalanceAmount = cnBillPaymentLog.fk_OnAccountRef.Amount - (onAccountMrLog.GetValueOrDefault() + totalofContext);
                    cnBillPaymentLog.fk_OnAccountRef.ObjectState = ObjectState.Modified;
                }
                AddStatusMap(entry);
            }
            
            if (cnBillPaymentLog.ObjectState != ObjectState.Unchanged && cnBillPaymentLog.TypeId == 1434/*OnAccount*/)
            {
                decimal? existing = cnBillPaymentLog.Amount;
                if (cnBillPaymentLog.Id > 0)
                {
                    existing =
                        DbSet
                            .Where(x => x.OnAccountRefId == cnBillPaymentLog.Id)
                            .Sum(x => (decimal?)x.Amount);
                }
                cnBillPaymentLog.OnAccountBalanceAmount = existing ?? cnBillPaymentLog.Amount;
                RemoveStatusMap(cnBillPaymentLog);
            }

            if (cnBillPaymentLog.TypeId == 1443/*CN Advance*/&& cnBillPaymentLog.ObjectState == ObjectState.Deleted)
            {
                if (cnBillPaymentLog.TripAdvanceId > 0)
                {
                    cnBillPaymentLog.fk_TripAdvance =_db.Set<TripAdvanceLog>().Find(cnBillPaymentLog.TripAdvanceId);
                    cnBillPaymentLog.ObjectState = ObjectState.Deleted;
                    new TripAdvanceCoreLogic().Bind(_db).Execute(_db.Entry(cnBillPaymentLog.fk_TripAdvance));
                }
                RemoveStatusMap(cnBillPaymentLog);
            }

        }
        private void RemoveStatusMap(CNBillPaymentLog entity)
        {
            //if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var statusid = _db.GetDTSStatusIdByDateId(1561);
            if (statusid == 0) return;

            var cndts = repo.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.CNId == entity.CNId && x.StatusId == statusid);
            if (cndts != null)
            {
                cndts.ObjectState = ObjectState.Deleted;
                new CNDTSStatusCoreLogic().Bind(_db).Execute(_db.Entry(cndts));
            }

        }
        //public override bool SaveAfterPostLogic { get; set; }
        //public override DbSet<CNBillPaymentLog> DbSet => _db.Set<CNBillPaymentLog>();
        private void AddStatusMap(DbEntityEntry entry)
        {
            var entity = entry.Entity as CNBillPaymentLog;
            if (entity == null || _db.GetApiConfig<int>("IsCNTrackEnabled") == 0 || entity.CNId.GetValueOrDefault()<=0) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var statusid = _db.GetDTSStatusIdByDateId(1561);
            //_db.Set<DTSStatus>()
            //               .Where(x => x.DateId == 1561)
            //               .Select(x => new { x.Id })
            //               .FromCacheFirstOrDefault()
            //               ?.Id ?? 0; 
            if (statusid == 0) return;
            var cndts = repo.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.CNId == entity.CNId && x.StatusId == statusid) ?? new CNDTSStatusLog();
            if (cndts.NextLogId.GetValueOrDefault() <= 0)
            {
                if ((entity.fk_Bill?.Id).GetValueOrDefault(0) == 0)
                {
                    entity.fk_Bill = _db.Set<CNBill>().FirstOrDefault(x => x.Id == entity.BillId);
                }
                if (entity.fk_Bill == null) return;
                cndts.CNId = entity.CNId.GetValueOrDefault();
                cndts.IsAuto = true;
                cndts.StartDate = entity.fk_Payment.DocumentDate;
                cndts.OfficeId1 = entity.fk_Payment.OfficeId;
                cndts.ObjectState = cndts.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                cndts.Qty = 0;
                cndts.StatusId = statusid;
                cndts.Remark = $"Money Receipt No:{entity.fk_Payment?.DocumentNo}, Payment Received on :{entity.fk_Payment?.DocumentDate:D} against Bill No:{entity.fk_Bill?.BillNo}, Dated :{entity.fk_Bill?.BillDate:D}";
                if (cndts.PreviousLogId <= 0)
                {
                    var previousLog = repo.Local.OrderByDescending(x => x.StartDate.Date).ThenByDescending(x => x.Id).FirstOrDefault(
                                          x =>
                                              x.CNId == entity.CNId &&
                                              x.StartDate.Date <= cndts.StartDate.Date && x.StatusId != cndts.StatusId) ??//&& ( x.Id == 0||x.Id != entity.Id ) && (x.Id == 0||x.Id != nextlogid) 
                                      repo
                                          .OrderByDescending(x => DbFunctions.TruncateTime(x.StartDate))
                                          .ThenByDescending(x => x.Id)
                                          //.Include(x => x.fk_NextLog)
                                          .FirstOrDefault(
                                              x =>
                                                  x.CNId == entity.CNId &&
                                                  DbFunctions.TruncateTime(x.StartDate) <= DbFunctions.TruncateTime(cndts.StartDate) && x.Id != entity.Id && x.StatusId != cndts.StatusId);
                    
                    cndts.PreviousLogId = previousLog?.Id;
                    cndts.fk_PreviousLog = previousLog;
                    if (previousLog != null)
                    {
                        previousLog.NextLogId = cndts.Id;
                        previousLog.fk_NextLog = cndts;
                        previousLog.EndDate = cndts.StartDate;
                        previousLog.ConsumedMinutes =
                            previousLog.EndDate.GetValueOrDefault(DateTime.Now).Subtract(previousLog.StartDate).Minutes;
                        previousLog.ObjectState = previousLog.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        repo.AddOrUpdate(previousLog);
                    }
                }

                repo.AddOrUpdate(cndts);

                var dts = (CNDTSStatusCoreLogic)new CNDTSStatusCoreLogic().Bind(_db);
                dts.CreateNextStatusAuto(cndts);
            }

        }
    }
}
