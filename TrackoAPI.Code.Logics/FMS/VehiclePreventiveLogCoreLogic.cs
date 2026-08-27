using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.DataContext;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;

namespace TrackoAPI.Code.Logics.FMS
{
    public class VehiclePreventiveLogCoreLogic:IBaseLogic
    {
        //protected static VehiclePreventiveLogCoreLogic _Instance;
        //public static VehiclePreventiveLogCoreLogic Instance => _Instance ?? (_Instance = new VehiclePreventiveLogCoreLogic());

        protected IDataContextAsync _db;
        public IBaseLogic Bind(IDataContextAsync db)
        {
            _db = db;
            return this;
        }

        public void Execute(DbEntityEntry entry)
        {
            Execute(entry,false);
            SaveAfterPostLogic = false;
        }

        public void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            SaveAfterPostLogic = false;
            var entity = entry.Entity as VehiclePreventiveLog;
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

        }
        
        public bool SaveAfterPostLogic { get; private set; }

        private void MapNextPreviousLog(VehiclePreventiveLog entity)
        {
            var repo = _db.Set<VehiclePreventiveLog>();
            var previousLog =
                                        repo
                                            .OrderByDescending(x => x.JobDate)
                                            .ThenByDescending(x => x.Id)
                                            .Include(x => x.fk_NextLog)
                                            .FirstOrDefault(
                                                x =>
                                                    x.VehicleId == entity.VehicleId &&x.PMId==entity.PMId&&
                                                    x.JobDate < entity.JobDate&&x.Id!=entity.Id);

            if (previousLog != null)
            {
                var pnexttriplog = previousLog.fk_NextLog;
                if (pnexttriplog != null&&pnexttriplog.Id!=entity.Id)
                {
                    pnexttriplog.PreviousLogId = entity.Id;
                    pnexttriplog.fk_PreviousLog = entity;
                    entity.fk_NextLog = pnexttriplog;
                    entity.NextLogId = pnexttriplog.Id;
                    pnexttriplog.ObjectState = ObjectState.Modified;
                    repo.AddOrUpdate(pnexttriplog);
                }
                entity.PreviousLogId = previousLog.Id;
                entity.fk_PreviousLog = previousLog;
                previousLog.NextLogId = entity.Id;
                previousLog.fk_NextLog = entity;
                previousLog.ObjectState = ObjectState.Modified;
                repo.AddOrUpdate(previousLog);
            }
        }

        private void UnMapNextPreviousLog(VehiclePreventiveLog entity)
        {
            var repo = _db.Set<VehiclePreventiveLog>();
            VehiclePreventiveLog previoustrip1 = entity.fk_PreviousLog ??
                                                                       repo.OrderByDescending(
                                                                           x => x.JobDate)
                                                                           .ThenByDescending(x => x.Id)
                                                                           .FirstOrDefault(
                                                                               x => x.NextLogId == entity.Id);

            if (previoustrip1 != null)
            {
                if (entity.fk_NextLog == null)
                {
                    entity.fk_NextLog =
                        repo.OrderByDescending(x => x.JobDate)
                            .ThenByDescending(x => x.Id)
                            .FirstOrDefault(x => x.Id == entity.NextLogId);
                }
                repo.Attach(previoustrip1);
                previoustrip1.NextLogId = entity.NextLogId;
                previoustrip1.fk_NextLog = entity.fk_NextLog;
                previoustrip1.ObjectState = ObjectState.Modified;
                if (previoustrip1.fk_NextLog != null)
                {
                    repo.Attach(previoustrip1.fk_NextLog);
                    previoustrip1.fk_NextLog.ObjectState = ObjectState.Modified;
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
