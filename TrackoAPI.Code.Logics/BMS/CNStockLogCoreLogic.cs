using EntityFramework.Extensions;
using Hangfire;
using Repository.Pattern.DataContext;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global.DTS;

namespace TrackoAPI.Code.Logics.BMS
{
    public class CNStockLogCoreLogic : IBaseLogic
    {
        //protected static CNStockLogCoreLogic _Instance;
        //public static CNStockLogCoreLogic Instance => _Instance ?? (_Instance = new CNStockLogCoreLogic());

        protected IDataContextAsync _db;
        public bool EnableStockMerge { get; set; }
        public IBaseLogic Bind(IDataContextAsync db)
        {
            _db = db;
            EnableStockMerge = _db.GetApiConfig<int>("EnableStockMerge") > 0;
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
            var entity = entry.Entity as CNStockLog;
            if (entity == null) return;
            if (!isPostLogicCall)
            {
                //Chain(entity);
                //UnChain(entity);
                switch (entity.ObjectState)
                {
                    case ObjectState.Added:
                    case ObjectState.Modified:
                        AddStatusMap(entity);
                        break;

                    case ObjectState.Deleted:
                        RemoveStatusMap(entity);
                        break;
                }
            }
            else
            {
                if (EnableStockMerge && (entity.LogTypeId == 1422 || entity.LogTypeId == 1866))
                {
                    var stockRepo = _db.Set<CNStockLog>();
                    var existingSTK = stockRepo.Where(x => x.CNId == entity.CNId&&x.LogDate<=entity.LogDate && x.Id != entity.Id && x.LogTypeId == 1422 && x.OfficeId == entity.OfficeId).Select(x => new { x.Id, x.OfficeId, x.CNId }).FirstOrDefault();
                    if (existingSTK != null)
                    {
                        Hangfire.BackgroundJob.Enqueue<IDbBackgroundJobs>(x => x.MergeCNStock(null,Helper.LoggedInTenantId,entity.CNId,entity.OfficeId, entity.Id, existingSTK.Id));
                    }
                }
            }
        }

        public bool SaveAfterPostLogic { get; private set; } = false;

        private void Chain(CNStockLog entity)
        {
            if (entity?.ObjectState == ObjectState.Added && entity.RefStockId.GetValueOrDefault() > 0)
            {
                var previousLog = _db.Set<CNStockLog>().Find(entity.RefStockId);
                if (previousLog != null)
                {
                    previousLog.NextLogId = entity.Id;
                    previousLog.fk_NextLog = entity;
                    var pEntry = _db.Entry(previousLog);
                    if (pEntry.State == EntityState.Detached)
                    {
                        _db.Set<CNStockLog>().Attach(previousLog);
                        pEntry.State = EntityState.Modified;
                    }
                }
            }
        }

        private void UnChain(CNStockLog entity)
        {
            if (entity?.ObjectState == ObjectState.Deleted && entity.RefStockId.GetValueOrDefault() > 0)
            {
                var previousLog = _db.Set<CNStockLog>().Find(entity.RefStockId);
                if (previousLog != null)
                {
                    previousLog.NextLogId = null;
                    previousLog.fk_NextLog = null;
                    var pEntry = _db.Entry(previousLog);
                    if (pEntry.State == EntityState.Detached)
                    {
                        _db.Set<CNStockLog>().Attach(previousLog);
                        pEntry.State = EntityState.Modified;
                    }
                }
            }
        }

        private void AddStatusMap(CNStockLog entity)
        {
            if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0) return;
            //AddStatusMap(item);
            try
            {
                _db.Database.ExecuteSqlCommand($"EXEC [dbo].[Proc_TRANS_1555_CreateDTSForStockLog]{entity.Id}");
            }
            catch (SqlException ex)
            {
                throw new BusinessException(ErrorCode.GLB106, ex.GetBaseException().Message);
            }
            //var repo = _db.Set<CNDTSStatusLog>();
            //var statusid = GetStatusId(entity.LogTypeId);
            //if (statusid == 0) return;
            //if (entity.LogTypeId == 1422 && entity.RefStockId > 0) statusid = 45;
            //var cndts = repo.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.CNId == entity.CNId && x.StatusId == statusid && x.StockLogId == entity.Id) ?? new CNDTSStatusLog();
            //if (cndts.NextLogId.GetValueOrDefault() <= 0)
            //{
            //    cndts.CNId = entity.CNId;
            //    cndts.IsAuto = true;
            //    cndts.StartDate = entity.LogDate;
            //    cndts.OfficeId1 = entity.OfficeId;
            //    cndts.ObjectState = cndts.Id > 0 ? ObjectState.Modified : ObjectState.Added;
            //    cndts.OfficeId1 = entity.OfficeId;
            //    cndts.Qty = entity.OutQty > 0 ? entity.OutQty : entity.InQty;
            //    cndts.StatusId = statusid;
            //    cndts.StockLogId = entity.Id;
            //    cndts.fk_StockLog = entity;
            //    repo.AddOrUpdate(cndts);
            //    CNDTSStatusCoreLogic.Instance.Bind(_db).Execute(_db.Entry(cndts));
            //}
        }

        private void RemoveStatusMap(CNStockLog entity)
        {
            if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var cndts = repo.Where(x => x.CNId == entity.CNId &&x.StockLogId == entity.Id).ToList();
            if (cndts?.Count > 0)
            {
                var dtspipe = new CNDTSStatusCoreLogic().Bind(_db);
                foreach (var cndt in cndts)
                {
                    cndt.ObjectState = ObjectState.Deleted;
                    dtspipe.Execute(_db.Entry(cndt));
                }
            }
        }
    }
}