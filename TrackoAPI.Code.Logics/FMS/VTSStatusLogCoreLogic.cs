using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.DataContext;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global.DTS;

namespace TrackoAPI.Code.Logics.FMS
{
    public class VTSStatusLogCoreLogic : IBaseLogic
    {
        //protected static VTSStatusLogCoreLogic _Instance;
        //public static VTSStatusLogCoreLogic Instance => _Instance ?? (_Instance = new VTSStatusLogCoreLogic());

        protected IDataContextAsync _db;
        public IBaseLogic Bind(IDataContextAsync db)
        {
            _db = db;
            return this;
        }

        public void Execute(DbEntityEntry entry)
        {
            Execute(entry, false);
            SaveAfterPostLogic = false;
        }

        public void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = false;
            var entity = entry.Entity as VTSStatusLog;
            if (entity == null) return;
            if (!isPostLogicCall)
            {
                switch (entity.ObjectState)
                {
                    case ObjectState.Added:
                        MapNextPreviousLog(entity);
                        break;
                    case ObjectState.Modified:
                        MapNextPreviousLog(entity);
                        break;
                    case ObjectState.Deleted:
                        UnMapNextPreviousLog(entity);
                        break;
                }
            }
            else
            {
                
            }
        }
        
        public bool SaveAfterPostLogic { get; private set; }
        private void MapNextPreviousLog(VTSStatusLog entity)
        {
            
            var repo = _db.Set<VTSStatusLog>();
            var previousLog =entity.PreviousLogId>0?repo.AsQueryable().Include(x=>x.fk_NextLog).FirstOrDefault(x=>x.Id== entity.PreviousLogId):
                                        repo
                                            .OrderByDescending(x => x.StartDate)
                                            .ThenByDescending(x => x.Id)
                                            .Include(x => x.fk_NextLog)
                                            .FirstOrDefault(
                                                x =>
                                                    (x.VehicleId == entity.VehicleId &&x.HireVehicleId==entity.HireVehicleId)&&
                                                    x.StartDate < entity.StartDate && x.Id != entity.Id);
            

            if (previousLog != null)
            {
                if (previousLog.NextLogId > 0 && entity.Id != previousLog.NextLogId)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Next Status is Already Exists.");
                }
                if (
                    !_db.Set<DTSStatusMapping>()
                        .Any(x => x.CurrentStatusId == previousLog.DTSStatusId && x.NextStatusId == entity.DTSStatusId))
                {
                    throw new BusinessException(ErrorCode.GLB106,"Invalid Status Selected");
                }
                var pnexttriplog = previousLog.fk_NextLog;
                if (pnexttriplog != null && pnexttriplog.Id != entity.Id)
                {
                    pnexttriplog.PreviousLogId = entity.Id;
                    pnexttriplog.fk_PreviousLog = entity;
                    entity.fk_NextLog = pnexttriplog;
                    entity.NextLogId = pnexttriplog.Id;
                    pnexttriplog.ObjectState = ObjectState.Modified;
                    repo.AddOrUpdate(pnexttriplog);
                }
                entity.ConsumedMinutes= (long)Math.Round(entity.EndDate.GetValueOrDefault(entity.StartDate).Subtract(entity.StartDate).TotalMinutes, 0);
                entity.PreviousLogId = previousLog.Id;
                entity.fk_PreviousLog = previousLog;
                previousLog.NextLogId = entity.Id;
                previousLog.fk_NextLog = entity;
                previousLog.EndDate = entity.StartDate;
                previousLog.ConsumedMinutes =
                (long) Math.Round(previousLog.EndDate.GetValueOrDefault(previousLog.StartDate).Subtract(previousLog.StartDate).TotalMinutes,0);
                previousLog.ObjectState = ObjectState.Modified;
                repo.AddOrUpdate(previousLog);
            }
        }

        private void UnMapNextPreviousLog(VTSStatusLog entity)
        {
            var repo = _db.Set<VTSStatusLog>();

            //if (entity.NextLogId > 0)
            //{
            //    throw new BusinessException(ErrorCode.GLB106,"Cannot delete this status as next status has exists for this status.");
            //}
            
            VTSStatusLog previoustrip1 = entity.fk_PreviousLog ??
                                                                       repo.OrderByDescending(
                                                                           x => x.StartDate)
                                                                           .ThenByDescending(x => x.Id)
                                                                           .FirstOrDefault(
                                                                               x => x.NextLogId == entity.Id);

            if (previoustrip1 != null)
            {
                if (entity.fk_NextLog == null&&entity.NextLogId>0)
                {
                    entity.fk_NextLog =
                        repo.OrderByDescending(x => x.StartDate)
                            .ThenByDescending(x => x.Id)
                            .FirstOrDefault(x => x.Id == entity.NextLogId);
                }

                repo.Attach(previoustrip1);
                previoustrip1.NextLogId = null;
                previoustrip1.fk_NextLog = null;
                previoustrip1.EndDate = null;
                previoustrip1.ConsumedMinutes = 0;
                previoustrip1.DelayMinutes = 0;
                previoustrip1.ObjectState = ObjectState.Modified;
                if (previoustrip1.fk_NextLog != null)
                {
                    repo.Attach(previoustrip1.fk_NextLog);
                    if (previoustrip1.fk_NextLog.ObjectState == ObjectState.Unchanged)
                    {
                        previoustrip1.fk_NextLog.ObjectState = ObjectState.Modified;
                    }
                    previoustrip1.fk_NextLog.fk_PreviousLog = previoustrip1;
                    previoustrip1.fk_NextLog.PreviousLogId = previoustrip1.Id;
                }
                entity.PreviousLogId = null;
                entity.fk_PreviousLog = null;
                entity.fk_NextLog = null;
                entity.NextLogId = null;
            }
        }
    }
}
