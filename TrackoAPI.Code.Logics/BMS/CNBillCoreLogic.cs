using Newtonsoft.Json;

using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Linq;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;

namespace TrackoAPI.Code.Logics.BMS
{
    public class CNBillCoreLogic : BaseLogic<CNBill>
    {
        

        /// <summary>
        /// Executes the specified entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <param name="isPostLogicCall">if set to <c>true</c> [is post logic call].</param>
        public override void Execute(DbEntityEntry entry, bool isPostLogicCall)
        {
            var cnbillog = entry.Entity as CNBill;
            if (cnbillog != null)
            {
                switch (cnbillog.ObjectState)
                {
                    case ObjectState.Added:

                    case ObjectState.Modified:
                        //1566
                        //AddStatusMap(entry);
                        break;
                    case ObjectState.Deleted:
                        //RemoveStatusMap(cnbillog);
                        break;
                }
            }
            
            
        }
        
        private void RemoveStatusMap(CNBill entity)
        {
            if (_db.GetApiConfig<int>("IsCNTrackEnabled") == 0||entity.CoverNoteId.GetValueOrDefault(0)==0) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var statusid = _db.GetDTSStatusIdByDateId(1580);
            if (statusid == 0) return;
            var nextstatusid = _db.GetDTSStatusIdByDateId(1567);
            var cndts = repo.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).Where(x => x.fk_CN.BillId == entity.Id&& (x.StatusId == statusid|| x.StatusId == nextstatusid)).ToList();
            foreach (var cn in cndts)
            {
                cn.ObjectState = ObjectState.Deleted;
                new CNDTSStatusCoreLogic().Bind(_db).Execute(_db.Entry(cndts));
            }
            

        }
        private void AddStatusMap(DbEntityEntry entry)
        {
            var entity = entry.Entity as CNBill;
            if (entity==null||_db.GetApiConfig<int>("IsCNTrackEnabled") == 0||entity.CoverNoteId<=0) return;
            if ((entity.fk_CoverNote?.Id).GetValueOrDefault(0) == 0)
            {
                entity.fk_CoverNote = _db.Set<BillSubmission>().FirstOrDefault(x => x.Id == entity.CoverNoteId);
            }
            if (entity.fk_CoverNote == null) return;
            var repo = _db.Set<CNDTSStatusLog>();
            var statusid = _db.GetDTSStatusIdByDateId(1580);
            //_db.Set<DTSStatus>()
            //               .Where(x => x.DateId == 1580)
            //               .Select(x => new {x.Id})
            //               .FromCacheFirstOrDefault()
            //               ?.Id ?? 0;
            if (statusid==0)return;
            var nextstatusid = _db.GetDTSStatusIdByDateId(1567);
                //_db.Set<DTSStatus>()
                //                   .Where(x => x.DateId == 1567)
                //                   .Select(x => new { x.Id })
                //                   .FromCacheFirstOrDefault()
                //                   ?.Id ?? -1;
            var cnstatuses = repo.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).Where(x => x.fk_CN.BillId == entity.Id && (x.StatusId == statusid || x.StatusId == nextstatusid)).ToList();
            var cnlogs = DbSet.Where(x=>x.Id== entity.Id).SelectMany(x => x.BillLogs, (bill, log) => log).ToList();
            
            foreach (var log in cnlogs.Where(x=>x.CNId.GetValueOrDefault()>0))
            {
                var cndts = cnstatuses.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.CNId == log.CNId && x.StatusId == statusid) ?? new CNDTSStatusLog();
                if (cndts.NextLogId.GetValueOrDefault() <= 0)
                {
                    
                    cndts.CNId = log.CNId.GetValueOrDefault();
                    cndts.IsAuto = true;
                    cndts.StartDate = entity.fk_CoverNote.DocDate;
                    cndts.OfficeId1 = entity.fk_CoverNote.OfficeId;
                    cndts.ObjectState = cndts.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    cndts.Qty = 0;
                    cndts.StatusId = statusid;
                    cndts.Remark = $"Bill Submission Statement No:{entity.fk_CoverNote.DocNumber}, Submission Date :{entity.fk_CoverNote.DocDate:D}";
                    if (cndts.PreviousLogId.GetValueOrDefault() <= 0)
                    {
                        var previousLog = repo.Local.OrderByDescending(x => x.StartDate.Date).ThenByDescending(x => x.Id).FirstOrDefault(
                                              x =>
                                                  x.CNId == log.CNId &&
                                                  x.StartDate.Date <= entity.fk_CoverNote.DocDate.Date && x.StatusId != cndts.StatusId) ??
                                          repo
                                              .OrderByDescending(x => DbFunctions.TruncateTime(x.StartDate))
                                              .ThenByDescending(x => x.Id)
                                              //.Include(x => x.fk_NextLog)
                                              .FirstOrDefault(
                                                  x =>
                                                      x.CNId == log.CNId &&
                                                      DbFunctions.TruncateTime(x.StartDate) <= DbFunctions.TruncateTime(entity.fk_CoverNote.DocDate) && x.Id != entity.Id && x.StatusId != cndts.StatusId);
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
                }
                if (entity.fk_CoverNote.IsPODInclosed && nextstatusid > 0)
                {
                    var nextstatus = cnstatuses.OrderByDescending(x => x.StartDate)
                                         .ThenByDescending(x => x.Id)
                                         .FirstOrDefault(x => x.CNId == log.CNId && x.StatusId == nextstatusid) ??
                                     new CNDTSStatusLog();
                    if (nextstatus.NextLogId.GetValueOrDefault() <= 0)
                    {
                        nextstatus.CNId = log.CNId.GetValueOrDefault();
                        nextstatus.IsAuto = true;
                        nextstatus.StartDate = entity.fk_CoverNote.DocDate.AddSeconds(1);
                        nextstatus.OfficeId1 = entity.fk_CoverNote.OfficeId;
                        nextstatus.ObjectState = nextstatus.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        nextstatus.Qty = 0;
                        nextstatus.StatusId = nextstatusid;
                        nextstatus.PreviousLogId = cndts.Id;
                        nextstatus.fk_PreviousLog = cndts;
                        nextstatus.Remark =
                            $"POD Submited with Covernote No :{entity.fk_CoverNote.DocNumber}, Submission Date :{entity.fk_CoverNote.DocDate:D}";
                        cndts.EndDate = nextstatus.StartDate;
                        cndts.ConsumedMinutes =
                            cndts.EndDate.GetValueOrDefault(DateTime.Now).Subtract(cndts.StartDate).Minutes;
                        cndts.ObjectState = cndts.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        repo.AddOrUpdate(nextstatus);
                    }
                    var dts = (CNDTSStatusCoreLogic)new CNDTSStatusCoreLogic().Bind(_db);
                    dts.CreateNextStatusAuto(nextstatus);
                }
                else
                {
                    var dts = (CNDTSStatusCoreLogic)new CNDTSStatusCoreLogic().Bind(_db);
                    dts.CreateNextStatusAuto(cndts);
                }
            }
        }
    }
}