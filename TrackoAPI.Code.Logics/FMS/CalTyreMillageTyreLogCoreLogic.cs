using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using EntityFramework.Extensions;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Tyres;

namespace TrackoAPI.Code.Logics.FMS
{
    public class CalTyreMillageTyreLogCoreLogic : BaseLogic<TyreLog>
    {
        //protected IDataContextAsync _db;

        //public override IBaseLogic Bind(IDataContextAsync db)
        //{
        //    _db = db;
        //    return this;
        //}

        //public override void Execute(DbEntityEntry entry)
        //{
        //    Execute(entry, false);
        //    SaveAfterPostLogic = false;
        //}

        public override void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = false;
            var entity = entry.Entity as TyreLog;
            if (entity == null) return;
            if (!isPostLogicCall) PreLogic(entity);
        }

        //public override bool SaveAfterPostLogic { get; set; }
        //public override DbSet<TyreLog> DbSet => _db.Set<TyreLog>();
        private void PreLogic(TyreLog entity)
        {
            if (entity.VoucherTypeId == 35) //Receipt
            {
                try
                {
                    var tmldb = _db.Set<TyreMillageLog>();
                    switch (entity.ObjectState)
                    {
                        case ObjectState.Added:
                        case ObjectState.Modified:
                            tmldb.Where(
                                x => x.Life == entity.TyreLife && x.TransactionId == entity.Id && x.SourceTypeId == 1483)
                                .Delete();
                            var millageLog = new TyreMillageLog
                            {
                                TyreId = entity.TyreId,
                                Life = entity.TyreLife,
                                KMRun = entity.KmRun,
                                ObjectState = ObjectState.Added,
                                OnDate = (entity.fk_IssueReceipt?.VoucherDate).GetValueOrDefault(entity.VoucherDate),
                                OutDate = entity.VoucherDate,
                                TransactionId = entity.Id,
                                SourceTypeId = 1483,
                                VehicleId = entity.VehicleId.GetValueOrDefault(),
                                CreatedDOE = DateTime.Now,
                                CreatedSessionId = Helper.SessionId()
                            };
                            tmldb.Add(millageLog);
                            break;
                        case ObjectState.Deleted:
                            tmldb.Where(
                                x => x.Life == entity.TyreLife && x.TransactionId == entity.Id && x.SourceTypeId == 1483)
                                .Delete();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //ex.ToExceptionless().AddObject(entity).Submit();
                    //Ignore & Report It
                }
            }

            if (_db.GetApiConfig<int>("AllowServerSideTyreLogDateValidation") == 1)
            {


                switch (entity.ObjectState)
                {
                    case ObjectState.Added:
                    case ObjectState.Modified:
                        //if (!entity.IgnoreValidation)
                        //{
                        var localtransactions = DbSet.Local.Where(x => x.ObjectState == ObjectState.Deleted).Select(x => x.VoucherNo).ToList();
                        if (entity.TyreId > 0 && entity.Id == 0)
                        {
                            var currenttransactionno = DbSet.Where(x =>
                                    x.TyreId == entity.TyreId &&
                                    entity.VoucherDate <= x.VoucherDate)
                                .Select(x => x.VoucherNo).FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(currenttransactionno) && !localtransactions.Contains(currenttransactionno))
                            {
                                throw new BusinessException(ErrorCode.TYR103,
                                    $"Tyre Number {entity.TyreNo} has another transaction with Doc Number {currenttransactionno} before Current Transaction Date i.e. {entity.VoucherDate:dd-MMM-yyyy HH:mm}");
                            }
                        }

                        if (entity.TyreId > 0 && entity.Id > 0)
                        {
                            var previoustransactionNo = DbSet.Where(x =>
                                    x.TyreId == entity.TyreId && x.Id != entity.Id && x.Id < entity.Id &&
                                    DbFunctions.TruncateTime(x.VoucherDate) > entity.VoucherDate.Date)
                                .Select(x => x.VoucherNo).FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(previoustransactionNo) && !localtransactions.Contains(previoustransactionNo))
                            {
                                throw new BusinessException(ErrorCode.TYR103,
                                    $"Tyre Number {entity.TyreNo} has another transaction with Doc Number {previoustransactionNo} after Current Transaction Date i.e. {entity.VoucherDate:dd-MMM-yyyy HH:mm}");
                            }

                            var nextTransactionNo = DbSet.Where(x =>
                                    x.TyreId == entity.TyreId && x.Id != entity.Id && x.Id > entity.Id &&
                                    DbFunctions.TruncateTime(x.VoucherDate) < entity.VoucherDate.Date)
                                .Select(x => x.VoucherNo).FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(nextTransactionNo) && !localtransactions.Contains(nextTransactionNo))
                            {
                                throw new BusinessException(ErrorCode.TYR103,
                                    $"Tyre Number {entity.TyreNo} has another transaction with Doc Number {nextTransactionNo} before Current Transaction Date i.e. {entity.VoucherDate:dd-MMM-yyyy HH:mm}");
                            }
                        }
                        break;
                }
            }
        }
    }
}
