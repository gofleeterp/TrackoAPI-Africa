using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using Repository.Pattern.DataContext;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Tyres;

namespace TrackoAPI.Code.Logics.FMS
{
    public class CalTyreMillageVMLCoreLogic:IBaseLogic
    {
        //protected static CalTyreMillageVMLCoreLogic _Instance;
        //public static CalTyreMillageVMLCoreLogic Instance => _Instance ?? (_Instance = new CalTyreMillageVMLCoreLogic());

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
            var entity = entry.Entity as VehicleMovementLog;
            if (entity == null) return;
            if (!isPostLogicCall) PreLogic(entity, entry);
        }

        public bool SaveAfterPostLogic { get; private set; }

        private void PreLogic(VehicleMovementLog entity, DbEntityEntry entry)
        {
            try
            {
                
                if (entity.TotalKmRun == 0)
                    entity.TotalKmRun = entity.KmRun + entity.AdditionalKmRun;
                var tmldb = _db.Set<TyreMillageLog>();
                var tpldb = _db.Set<TyreLifePerformanceLog>();
                var Sourcetypes = new List<long>() { 1484, 1485 };
                switch (entity.ObjectState)
                {
                    case ObjectState.Modified:
                        if (entity.VehicleId > 0 && ((entity.RouteId > 0 && (entity.TripTypeId == 1158 || (entity.TripTypeId == 1160 && entity.VehicleId != null))) || entity.TripTypeId == 1159))
                        {
                            var prop = entry.Property("UnloadingDate");

                            if (((DateTime?)prop.OriginalValue) == null && (DateTime?)prop.CurrentValue != null)//Add the KM
                            {
                                var tlogs =
                                   _db.Set<TyreLog>()
                                       .Where(
                                           x =>
                                               x.TyreStatusId == 1103 && x.VehicleId == entity.VehicleId && x.VoucherDate <= entity.TripStartDate && (x.fk_IssueReceipt == null ? DateTime.Now : x.fk_IssueReceipt.VoucherDate) >= entity.TripStartDate)
                                       .Select(x => new { x.TyreId, x.TyreLife }).Distinct()
                                       .ToList();
                                if (!tlogs.Any()) break;
                                var tlist = tlogs.Select(x => x.TyreId + "-" + x.TyreLife).ToList();
                                try
                                {

                                    var plogs = tpldb
                                            .Where(x => tlist.Contains(x.TyreId + "-" + x.Life))
                                            .ToList();
                                    foreach (var log in plogs)
                                    {
                                        _db.Entry(log).State = EntityState.Unchanged;
                                        switch (entity.TripTypeId)
                                        {
                                            case 1158:
                                            case 1160:
                                                log.TLLifeMileage += entity.TotalKmRun;                                                
                                                log.ObjectState = ObjectState.Modified;
                                                break;
                                            case 1159:
                                                log.JSLifeMileage += entity.TotalKmRun;
                                                log.ObjectState = ObjectState.Modified;
                                                break;
                                        }
                                        //if(log.ObjectState==ObjectState.Modified) tpldb.AddOrUpdate(log);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    //Ignore
                                }
                                try
                                {
                                    var tmillageLogs = tlogs.Select(x => new TyreMillageLog
                                    {
                                        TyreId = x.TyreId,
                                        Life = x.TyreLife,
                                        KMRun = entity.TotalKmRun,
                                        ObjectState = ObjectState.Added,
                                        OnDate = entity.TripStartDate,
                                        OutDate = entity.UnloadingDate.GetValueOrDefault(),
                                        TransactionId = entity.Id,
                                        SourceTypeId = (entity.TripTypeId.GetValueOrDefault() == 1158|| entity.TripTypeId.GetValueOrDefault() == 1160) ? 1484 : 1485,
                                        VehicleId = entity.VehicleId.GetValueOrDefault(),
                                        CreatedDOE = DateTime.Now,
                                        CreatedSessionId = Helper.SessionId()
                                    });
                                    tmldb.AddRange(tmillageLogs);
                                }
                                catch (Exception ex)
                                {
                                    //ex.ToExceptionless().AddObject(entity).AddObject(tlogs).SetMessage("Unable to push TyreMillage Record in TyreMillage Pool.").Submit();
                                }

                            }
                            else if (((DateTime?)prop.OriginalValue) != null && ((DateTime?)prop.CurrentValue) == null)//Less the KM
                            {

                                var tlmlogs = tmldb
                                        .Where(x => x.TransactionId == entity.Id && Sourcetypes.Contains(x.SourceTypeId)).Select(x => new { x.TyreId, x.Life, x.Id })
                                        .ToList();
                                if (!tlmlogs.Any()) break;
                                var tlist = tlmlogs.Select(x => x.TyreId + "-" + x.Life).ToList();
                                var plogs = tpldb
                                        .Where(x => tlist.Contains(x.TyreId + "-" + x.Life))
                                        .ToList();
                                foreach (var log in plogs)
                                {
                                    _db.Entry(log).State = EntityState.Unchanged;
                                    if ((entity.TripTypeId == 1158 || (entity.TripTypeId == 1160 && entity.VehicleId != null)) && log.TLLifeMileage > 0)//VehicleMovementLog
                                    {
                                        log.TLLifeMileage -= entity.TotalKmRun;
                                        log.ObjectState = ObjectState.Modified;
                                    }
                                    else if (entity.TripTypeId == 1159 && log.JSLifeMileage > 0)//JobSheet
                                    {
                                        log.JSLifeMileage -= entity.TotalKmRun;
                                        log.ObjectState = ObjectState.Modified;
                                    }

                                    //if (log.ObjectState == ObjectState.Modified)
                                    //{
                                    //    _db.Entry(log).State=EntityState.Modified;
                                    //}

                                }
                                var tmillageLogs = tlmlogs.Select(x => new TyreMillageLog
                                {
                                    Id = x.Id,
                                    ObjectState = ObjectState.Deleted
                                });
                                foreach (var log in tmillageLogs)
                                {
                                    var l = _db.Entry(log);
                                    if (l.State == EntityState.Detached)
                                        tmldb.Attach(log);
                                    tmldb.Remove(log);
                                }
                                //tmldb.RemoveRange(tmillageLogs);
                            }
                            else if (((DateTime?)prop.CurrentValue) != null && (DateTime?)prop.OriginalValue == (DateTime?)prop.CurrentValue && !tmldb.Any(x => x.TransactionId == entity.Id && Sourcetypes.Contains(x.SourceTypeId)))
                            {
                                var tlogs =
                                   _db.Set<TyreLog>()
                                       .Where(
                                           x =>
                                               x.TyreStatusId == 1103 && x.VehicleId == entity.VehicleId && x.VoucherDate <= entity.TripStartDate && (x.fk_IssueReceipt == null ? DateTime.Now : x.fk_IssueReceipt.VoucherDate) >= entity.TripStartDate)
                                       .Select(x => new { x.TyreId, x.TyreLife }).Distinct()
                                       .ToList();
                                if (!tlogs.Any()) break;
                                try
                                {

                                    var tmillageLogs = tlogs.Select(x => new TyreMillageLog
                                    {
                                        TyreId = x.TyreId,
                                        Life = x.TyreLife,
                                        KMRun = entity.TotalKmRun,
                                        ObjectState = ObjectState.Added,
                                        OnDate = entity.TripStartDate,
                                        OutDate = entity.UnloadingDate.GetValueOrDefault(),
                                        TransactionId = entity.Id,
                                        SourceTypeId = entity.TripTypeId.GetValueOrDefault() == 1158 || (entity.TripTypeId == 1160 && entity.VehicleId != null) ? 1484 : 1485,
                                        VehicleId = entity.VehicleId.GetValueOrDefault(),
                                        CreatedDOE = DateTime.Now,
                                        CreatedSessionId = Helper.SessionId()
                                    });
                                    tmldb.AddRange(tmillageLogs);
                                }
                                catch (Exception ex)
                                {
                                    //ex.ToExceptionless().AddObject(entity).AddObject(tlogs).SetMessage("Unable to push TyreMillage Record in TyreMillage Pool.").Submit();
                                }
                            }
                        }

                        break;
                    case ObjectState.Deleted:
                        if (entity.VehicleId > 0 && ((entity.RouteId > 0 && (entity.TripTypeId == 1158 || (entity.TripTypeId == 1160 && entity.VehicleId != null))) || entity.TripTypeId == 1159))
                        {
                            var types = new List<long>() { 1484, 1485 };
                            var tlmlogs = tmldb
                                    .Where(x => x.TransactionId == entity.Id && types.Contains(x.SourceTypeId)).Select(x => new { x.TyreId, x.Life, x.Id })
                                    .ToList();
                            if (!tlmlogs.Any()) break;
                            var tlist = tlmlogs.Select(x => x.TyreId + "-" + x.Life).ToList();
                            var plogs = tpldb
                                    .Where(x => tlist.Contains(x.TyreId + "-" + x.Life))
                                    .ToList();
                            foreach (var log in plogs)
                            {
                                _db.Entry(log).State = EntityState.Unchanged;
                                if ((entity.TripTypeId == 1158|| (entity.TripTypeId == 1160 && entity.VehicleId != null)) && log.TLLifeMileage > 0)//VehicleMovementLog
                                {
                                    log.TLLifeMileage -= entity.TotalKmRun;
                                    log.ObjectState = ObjectState.Modified;
                                }
                                else if (entity.TripTypeId == 1159 && log.JSLifeMileage > 0)//JobSheet
                                {
                                    log.JSLifeMileage -= entity.TotalKmRun;
                                    log.ObjectState = ObjectState.Modified;
                                }
                                //if (log.ObjectState == ObjectState.Modified) tpldb.AddOrUpdate(log);
                            }
                            var tmillageLogs = tlmlogs.Select(x => new TyreMillageLog
                            {
                                Id = x.Id,
                                ObjectState = ObjectState.Deleted
                            });
                            foreach (var log in tmillageLogs)
                            {
                                var l = _db.Entry(log);
                                if (entry.State == EntityState.Detached)
                                    tmldb.Attach(log);
                                tmldb.Remove(log);
                            }
                        }
                        break;
                }

                
            }
            catch (Exception ex)
            {
                //ex.ToExceptionless().Submit();
            }
        }

       
    }
}
