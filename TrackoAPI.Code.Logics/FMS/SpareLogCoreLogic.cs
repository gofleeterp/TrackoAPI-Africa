using Repository.Pattern.DataContext;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Linq.Dynamic.Core;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using Z.EntityFramework.Plus;

namespace TrackoAPI.Code.Logics.FMS
{
    public class SpareLogCoreLogic : BaseLogic<SpareLog>
    {
        public override void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = false;
            var entity = entry.Entity as SpareLog;
            if (entity == null) return;
            if (!isPostLogicCall) PreLogic(entity, entry);
        }

        private void PreLogic(SpareLog entity, DbEntityEntry entry)
        {
            switch (entity.ObjectState)
            {
                case ObjectState.Added:
                    CheckIfItIsAccesory(entity);
                    break;
                case ObjectState.Modified:
                    CheckIfItIsAccesory(entity);
                    break;
                case ObjectState.Deleted:
                    DeleteAccesory(entity);
                    break;
            }
        }
        readonly long?[] VehicleAccessoryVoucherTypes = new long?[] { 22/*Direct Consumption*/, 24 /*Spare Part Issue*/};
        private void DeleteAccesory(SpareLog entity)
        {
           if (!VehicleAccessoryVoucherTypes.Contains(entity.VoucherTypeId)) return;
            var repo = _db.Set<VehicleAccessoryLog>();
            if (repo.Any(x => x.SpareLogId == entity.Id))
            {
                repo.Where(x => x.SpareLogId == entity.Id).Delete();
            }
        }
        private void CheckIfItIsAccesory(SpareLog entity)
        {
            
            if (!VehicleAccessoryVoucherTypes.Contains(entity.VoucherTypeId)|| entity.VehicleId.GetValueOrDefault()==0) return;
            if (entity.fk_Spare == null)
            {
                if (entity.SparePartId > 0)
                {
                    entity.fk_Spare = _db.Set<SpareMaster>().Find(entity.SparePartId);
                }
                if(entity.fk_Spare == null)
                {
                    return;
                }
            }
            if (entity.fk_Spare.Monitoring)
            {
                var repo = _db.Set<VehicleAccessoryLog>();
                var vs = entity.Id > 0 ? repo.FirstOrDefault(x => x.SpareLogId == entity.Id)??new VehicleAccessoryLog() : new VehicleAccessoryLog();
                vs.SpareLogId = entity.Id;
                vs.SparePartId = entity.SparePartId;
                vs.fk_SpareLog = entity;
                vs.AssetId = entity.VehicleId.GetValueOrDefault();
                vs.DepositedQty = entity.DepositedQty;
                vs.LogDate = entity.VoucherDate;
                vs.ObjectState = vs.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                vs.Qty = entity.Qty;
                vs.Remark = entity.Remark;
                vs.ScrapQty = 0;
                vs.BalanceQty = entity.Qty;
                //vs.StatusId = 0;/*Issued*/
                repo.AddOrUpdate(vs);
            }

        }
    }
}
