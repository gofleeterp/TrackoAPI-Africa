using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoAPI.Code.Logics.BMS;

namespace TrackoApi.Service
{
    public interface ICNBillService : IService<CNBill>
    {
        bool VerifyDataIntegrity(string billid, string voucherId);
        void RevokeBillSubmission(CNBill entity);
        void ApplyBillSubmission(CNBill entity);
    }
    public class CNBillService : Service<CNBill>, ICNBillService
    {
        private readonly IRepositoryAsync<CNBill> _repository;
        public CNBillService(IRepositoryAsync<CNBill> repository) : base(repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Verifies the data integrity.
        /// </summary>
        /// <param name="billid">The billid.</param>
        /// <param name="voucherId">The voucher identifier.</param>
        /// <returns>System.Boolean.</returns>
        public bool VerifyDataIntegrity(string objBillid, string objVoucherId)
        {
            var billid = long.Parse(objBillid);
            return false;
        }

        public override void Delete(CNBill entity)
        {
            if (entity.CoverNoteId > 0)
            {
                throw new BusinessException(ErrorCode.GLB106,"Bills submited to client cannot be deleted.");
            }
            //var repo = _repository.GetRepository<DTSStatus>().Queryable();
            var statusid = _repository.UOW.Context.GetDTSStatusIdByDateId(1566);
            //repo
            //               .Where(x => x.DateId == 1566)
            //               .Select(x => new { x.Id })
            //               .FromCacheFirstOrDefault()
            //               ?.Id ?? 0;
            foreach (var status in _repository.GetRepository<CNDTSStatusLog>().Queryable().Where(x => x.fk_CN.BillId == entity.Id && x.StatusId == statusid).ToList())
            {
                status.ObjectState = ObjectState.Deleted;
                new CNDTSStatusCoreLogic().Bind(_repository.UOW.Context).Execute(_repository.UOW.Context.Entry(status));
            }
            base.Delete(entity);
        }

        public void RevokeBillSubmission(CNBill entity)
        {
            var statusid = _repository.UOW.Context.GetDTSStatusIdByDateId(1566);
            var nextstatusid = _repository.GetDTSStatusIdByDateId(1567);
            foreach (var status in _repository.GetRepository<CNDTSStatusLog>().Queryable().Where(x => x.fk_CN.BillId == entity.Id && (x.StatusId == statusid||x.StatusId== nextstatusid)).OrderByDescending(x=>x.Id).ToList())
            {
                status.ObjectState = ObjectState.Deleted;
                new CNDTSStatusCoreLogic().Bind(_repository.UOW.Context).Execute(_repository.UOW.Context.Entry(status));
            }
        }
        public void ApplyBillSubmission(CNBill entity)
        {
            if (entity == null ||_repository.GetConfigValue<int>("IsCNTrackEnabled") == 0 || entity.CoverNoteId <= 0) return;
            if ((entity.fk_CoverNote?.Id).GetValueOrDefault(0) == 0)
            {
                entity.fk_CoverNote = _repository.GetRepository<BillSubmission>().Find(entity.CoverNoteId);
            }
            if (entity.fk_CoverNote == null) return;
            var repo =_repository.GetRepository<CNDTSStatusLog>();

            var statusid = _repository.GetDTSStatusIdByDateId(1580);
            if (statusid == 0) return;
            var nextstatusid = _repository.GetDTSStatusIdByDateId(1567);
            var cnstatuses = repo.Queryable().OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).Where(x => x.fk_CN.BillId == entity.Id && (x.StatusId == statusid || x.StatusId == nextstatusid)).ToList();
            var cnlogs = _repository.GetRepository<CNBill>().Queryable().Where(x=>x.Id==entity.Id).SelectMany(x => x.BillLogs, (bill, log) => log).ToList();
            var logs = new List<CNDTSStatusLog>();
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
                        var previousLog = repo.Queryable()
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
                        }

                    }
                    if (cndts.Id > 0)
                    {
                        repo.Update(cndts);
                    }
                    else
                    {
                        repo.Insert(cndts);
                    }
                    logs.Add(cndts);
                }
            }
            _repository.UOW.SaveChangesAsync();
            if(entity.fk_CoverNote.IsPODInclosed && nextstatusid > 0)
            {
                foreach (var log in logs)
                {
                    var nextstatus = cnstatuses.OrderByDescending(x => x.StartDate)
                                         .ThenByDescending(x => x.Id)
                                         .FirstOrDefault(x => x.CNId == log.CNId && x.StatusId == nextstatusid) ??
                                     new CNDTSStatusLog();
                    if (nextstatus.NextLogId.GetValueOrDefault() <= 0)
                    {
                        nextstatus.CNId = log.CNId;
                        nextstatus.IsAuto = true;
                        nextstatus.StartDate = entity.fk_CoverNote.DocDate.AddSeconds(1);
                        nextstatus.OfficeId1 = entity.fk_CoverNote.OfficeId;
                        nextstatus.ObjectState = nextstatus.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        nextstatus.Qty = 0;
                        nextstatus.StatusId = nextstatusid;
                        nextstatus.PreviousLogId = log.Id;
                        nextstatus.fk_PreviousLog = log;
                        log.NextLogId = nextstatus.Id;
                        log.fk_NextLog = nextstatus;
                        nextstatus.Remark =
                            $"POD Submited with Covernote No :{entity.fk_CoverNote.DocNumber}, Submission Date :{entity.fk_CoverNote.DocDate:D}";
                        log.EndDate = nextstatus.StartDate;
                        log.ConsumedMinutes =
                            log.EndDate.GetValueOrDefault(DateTime.Now).Subtract(log.StartDate).Minutes;
                        log.ObjectState = log.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                        if (nextstatus.Id > 0)
                        {
                            repo.Update(nextstatus);
                        }
                        else
                        {
                            repo.Insert(nextstatus);
                        }
                    }
                }
            }
        }
    }
}
