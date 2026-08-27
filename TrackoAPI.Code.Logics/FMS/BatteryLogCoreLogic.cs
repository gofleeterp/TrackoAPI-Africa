using Repository.Pattern.DataContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;

namespace TrackoAPI.Code.Logics.FMS
{
    public class BatteryLogCoreLogic : BaseLogic<BatteryLog>
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
        //public override bool SaveAfterPostLogic { get; set; }
        //public override DbSet<BatteryLog> DbSet => _db.Set<BatteryLog>();
        public override void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = false;
            var entity = entry.Entity as BatteryLog;
            if (entity == null) return;
            if (!isPostLogicCall) PreLogic(entity);
        }

        private void PreLogic(BatteryLog entity)
        {
            if (_db.GetApiConfig<int>("AllowServerSideBatteryLogDateValidation") == 1)
            {


                switch (entity.ObjectState)
                {
                    case ObjectState.Added:
                    case ObjectState.Modified:
                        //if (!entity.IgnoreValidation)
                        //{
                        var localtransactions = DbSet.Local.Where(x => x.ObjectState == ObjectState.Deleted).Select(x => x.DocNo).ToList();
                        if (entity.BatteryId > 0 && entity.Id == 0)
                        {
                            var currenttransactionno = DbSet.Where(x =>
                                    x.BatteryId == entity.BatteryId &&
                                    entity.DocDate <= x.DocDate)
                                .Select(x => x.DocNo).FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(currenttransactionno) && !localtransactions.Contains(currenttransactionno))
                            {
                                throw new BusinessException(ErrorCode.TYR103,
                                    $"Battery Number {entity.BatterySerialNo} has another transaction with Doc Number {currenttransactionno} before Current Transaction Date i.e. {entity.DocDate:dd-MMM-yyyy HH:mm}");
                            }
                        }

                        if (entity.BatteryId > 0 && entity.Id > 0)
                        {
                            var previoustransactionNo = DbSet.Where(x =>
                                    x.BatteryId == entity.BatteryId && x.Id != entity.Id && x.Id < entity.Id &&
                                    DbFunctions.TruncateTime(x.DocDate) > entity.DocDate.Date)
                                .Select(x => x.DocNo).FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(previoustransactionNo) && !localtransactions.Contains(previoustransactionNo))
                            {
                                throw new BusinessException(ErrorCode.TYR103,
                                    $"Battery Number {entity.BatterySerialNo} has another transaction with Doc Number {previoustransactionNo} after Current Transaction Date i.e. {entity.DocDate:dd-MMM-yyyy HH:mm}");
                            }

                            var nextTransactionNo = DbSet.Where(x =>
                                    x.BatteryId == entity.BatteryId && x.Id != entity.Id && x.Id > entity.Id &&
                                    DbFunctions.TruncateTime(x.DocDate) < entity.DocDate.Date)
                                .Select(x => x.DocNo).FirstOrDefault();
                            if (!string.IsNullOrWhiteSpace(nextTransactionNo) && !localtransactions.Contains(nextTransactionNo))
                            {
                                throw new BusinessException(ErrorCode.TYR103,
                                    $"Battery Number {entity.BatterySerialNo} has another transaction with Doc Number {nextTransactionNo} before Current Transaction Date i.e. {entity.DocDate:dd-MMM-yyyy HH:mm}");
                            }
                        }
                        break;
                }
            }
        }
    }
}
