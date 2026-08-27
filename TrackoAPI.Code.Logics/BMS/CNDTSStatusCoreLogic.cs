using EntityFramework.Extensions;
using Repository.Pattern.DataContext;
using System;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Linq;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global.DTS;

namespace TrackoAPI.Code.Logics.BMS
{
    public class CNDTSStatusCoreLogic : IBaseLogic
    {
        //protected static CNDTSStatusCoreLogic _Instance;
        //public static CNDTSStatusCoreLogic Instance => _Instance ?? (_Instance = new CNDTSStatusCoreLogic());

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

        public bool IsPostLogicCall { get; set; }

        public void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            IsPostLogicCall = isPostLogicCall;
            if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0) return;
            SaveAfterPostLogic = false;
            var entity = entry.Entity as CNDTSStatusLog;
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
                CreateNextStatusAuto(entity);
            }
        }

        public bool SaveAfterPostLogic { get; private set; }

        public void MapNextPreviousLog(CNDTSStatusLog entity)
        {
            if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0 || entity.ObjectState == ObjectState.Unchanged) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var maprepo = _db.Set<DTSStatusMapping>();
            var previousStatusIds = maprepo.Where(x => x.NextStatusId == entity.StatusId)
                .Select(x => new { x.CurrentStatusId })
                .FromCache().Select(x => x.CurrentStatusId)
                .ToList();
            var nextlogid = entity.NextLogId.GetValueOrDefault();
            var previousLog = entity.fk_PreviousLog ?? repo.Local.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(
                x =>
                    x.CNId == entity.CNId &&
                    x.StartDate <= entity.StartDate && previousStatusIds.Contains(x.StatusId) && x.StatusId != entity.StatusId) ??//&& ( x.Id == 0||x.Id != entity.Id ) && (x.Id == 0||x.Id != nextlogid)
                                        repo
                                            .OrderByDescending(x => x.StartDate)
                                            .ThenByDescending(x => x.Id)
                                            //.Include(x => x.fk_NextLog)
                                            .FirstOrDefault(
                                                x =>
                                                    x.CNId == entity.CNId &&
                                                    x.StartDate <= entity.StartDate && x.Id != entity.Id && x.Id != nextlogid && x.StatusId != entity.StatusId);

            if (previousLog != null)
            {
                if (previousLog.NextLogId > 0 && entity.Id != previousLog.NextLogId && entity.StatusId != 59)
                {
                    var exp = new BusinessException(ErrorCode.GLB106, "Next Status is Already Exists.");
                    //ExceptionlessClient.Default.CreateException(exp).AddObject(entity).AddTags(Helper.LoggedInTenantId).Submit();
                    throw exp;
                }
                if (_db.GetApiConfig<int>("InForceCNStatusOrder") == 1 &&
                    !maprepo.Any(x => x.CurrentStatusId == previousLog.StatusId && x.NextStatusId == entity.StatusId))
                {
                    var exp = new BusinessException(ErrorCode.GLB106, "Invalid Consignment Traking Status");
                    //ExceptionlessClient.Default.CreateException(exp).AddObject(entity).AddTags(Helper.LoggedInTenantId).Submit();
                    throw exp;
                }
                //    var pnexttriplog = previousLog.fk_NextLog;
                //if (pnexttriplog != null&&pnexttriplog.Id!=entity.Id)
                //{
                //    pnexttriplog.PreviousLogId = entity.Id;
                //    pnexttriplog.fk_PreviousLog = entity;
                //    entity.fk_NextLog = pnexttriplog;
                //    entity.NextLogId = pnexttriplog.Id;
                //    pnexttriplog.ObjectState = ObjectState.Modified;
                //    repo.AddOrUpdate(pnexttriplog);
                //}
                entity.PreviousLogId = previousLog.Id;
                //TODO:Write Mapping Logic
                entity.fk_PreviousLog = previousLog;
                //previousLog.NextLogId = entity.Id;
                //previousLog.fk_NextLog = entity;
                previousLog.EndDate = entity.StartDate;
                previousLog.ConsumedMinutes =
                    previousLog.EndDate.GetValueOrDefault(DateTime.Now).Subtract(previousLog.StartDate).Minutes;
                previousLog.ObjectState = previousLog.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                repo.AddOrUpdate(previousLog);
                //CreateNextStatusAuto(entity);
                //SaveAfterPostLogic = true;
            }
        }

        public void CreateNextStatusAuto(CNDTSStatusLog entity)
        {
            if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0) return;
            if (entity.fk_NextLog != null && default(CNDTSStatusLog) != entity.fk_NextLog) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var dtsrepo = _db.Set<DTSStatus>();
            var statusid = dtsrepo.Where(x => x.Id == entity.StatusId)
                .Select(x => new { x.NextStatusId })
                .FirstOrDefault()?.NextStatusId ?? 0;
            if (statusid == 0) return;
            var cndts = repo.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.CNId == entity.CNId && x.StatusId == statusid && x.StockLogId == entity.Id) ?? new CNDTSStatusLog();
            if (cndts.NextLogId.GetValueOrDefault() <= 0)
            {
                cndts.CNDTSStatusId = entity.CNDTSStatusId;
                cndts.CNId = entity.CNId;
                cndts.IsAuto = true;
                cndts.StartDate = cndts.Id == 0 ? entity.StartDate.AddSeconds(1) : entity.StartDate;
                cndts.OfficeId1 = entity.OfficeId1;
                cndts.ObjectState = cndts.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                cndts.Qty = entity.Qty;
                cndts.StatusId = statusid;
                cndts.StockLogId = entity.StockLogId;
                cndts.fk_StockLog = entity.StockLogId > 0 ? entity.fk_StockLog : null;
                cndts.PreviousLogId = entity.Id;
                cndts.fk_PreviousLog = entity;
                entity.NextLogId = cndts.Id;
                entity.fk_NextLog = cndts;
                entity.EndDate = cndts.Id == 0 ? entity.StartDate.AddSeconds(1) : entity.StartDate;
                entity.ConsumedMinutes =
                    entity.EndDate.GetValueOrDefault(DateTime.Now).Subtract(entity.StartDate).Minutes;
                entity.ObjectState = entity.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                repo.AddOrUpdate(cndts);
                SaveAfterPostLogic = true;
            }
        }

        public void UnMapNextPreviousLog(CNDTSStatusLog entity)
        {
            if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var dtsrepo = _db.Set<DTSStatus>();
            var statusid = dtsrepo.Where(x => x.Id == entity.StatusId)
                               .Select(x => new { x.NextStatusId })
                               .FirstOrDefault()?.NextStatusId ?? 0;
            if (statusid > 0)
            {
                if (entity.fk_NextLog == null)
                {
                    entity.fk_NextLog = repo.FirstOrDefault(x => x.Id == entity.NextLogId);
                }
                if (entity.fk_NextLog != null)
                {
                    entity.fk_NextLog.ObjectState = ObjectState.Deleted;
                    repo.Remove(entity.fk_NextLog);
                }
            }
            if (entity.NextLogId > 0 && !repo.Local.Any(x => x.Id == entity.NextLogId && entity.ObjectState == ObjectState.Deleted))
            {
                throw new BusinessException(ErrorCode.GLB106, "Cannot delete this status as next status has exists for this status.");
            }
            CNDTSStatusLog previoustrip1 = entity.fk_PreviousLog ??
                                                                       repo.OrderByDescending(
                                                                           x => x.StartDate)
                                                                           .ThenByDescending(x => x.Id)
                                                                           .FirstOrDefault(
                                                                               x => x.NextLogId == entity.Id);

            if (previoustrip1 != null)
            {
                //if (entity.fk_NextLog == null)
                //{
                //    entity.fk_NextLog =
                //        repo.OrderByDescending(x => x.StartDate)
                //            .ThenByDescending(x => x.Id)
                //            .FirstOrDefault(x => x.Id == entity.NextLogId);
                //}

                repo.Attach(previoustrip1);
                previoustrip1.NextLogId = null;
                previoustrip1.fk_NextLog = null;
                previoustrip1.EndDate = null;
                previoustrip1.ConsumedMinutes = 0;
                previoustrip1.ObjectState = ObjectState.Modified;
                //if (previoustrip1.fk_NextLog != null)
                //{
                //    repo.Attach(previoustrip1.fk_NextLog);
                //    previoustrip1.fk_NextLog.ObjectState = ObjectState.Modified;
                //    previoustrip1.fk_NextLog.fk_PreviousLog = previoustrip1;
                //    previoustrip1.fk_NextLog.PreviousLogId = previoustrip1.Id;
                //}
                entity.PreviousLogId = null;
                entity.fk_PreviousLog = null;
                entity.fk_NextLog = null;
                entity.NextLogId = null;
                SaveAfterPostLogic = true;
            }
        }
    }
}