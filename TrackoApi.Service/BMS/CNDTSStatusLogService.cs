using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System;
using System.Linq;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global.DTS;

namespace TrackoApi.Service.TMS
{
    public interface ICNDTSStatusLogService : IService<CNDTSStatusLog>
    {
    }
    public class CNDTSStatusLogService : Service<CNDTSStatusLog>, ICNDTSStatusLogService
    {
        private readonly IRepositoryAsync<CNDTSStatusLog> _repository;
        public CNDTSStatusLogService(IRepositoryAsync<CNDTSStatusLog> repository) : base(repository)
        {
            _repository = repository;
        }
        public override CNDTSStatusLog Insert(CNDTSStatusLog entity)
        {
            var previousLog = entity.fk_PreviousLog ?? _repository.Local.OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(
                                  x =>
                                      x.CNId == entity.CNId &&
                                      x.StartDate <= entity.StartDate && x.StatusId != entity.StatusId) ??//&& ( x.Id == 0||x.Id != entity.Id ) && (x.Id == 0||x.Id != nextlogid) 
                              _repository.Queryable()
                                  .OrderByDescending(x => x.StartDate)
                                  .ThenByDescending(x => x.Id)
                                  //.Include(x => x.fk_NextLog)
                                  .FirstOrDefault(
                                      x =>
                                          x.CNId == entity.CNId &&
                                          x.StartDate <= entity.StartDate && x.Id != entity.Id&& x.StatusId != entity.StatusId);
            
            entity.PreviousLogId = previousLog?.Id;
            entity.fk_PreviousLog = previousLog;
            if (previousLog != null)
            {
                previousLog.NextLogId = entity.Id;
                previousLog.fk_NextLog = entity;
                previousLog.ObjectState=ObjectState.Modified;
                previousLog.EndDate = entity.StartDate;
                previousLog.ConsumedMinutes =
                    previousLog.EndDate.GetValueOrDefault(DateTime.Now).Subtract(previousLog.StartDate).Minutes;
            }
            
            return base.Insert(entity);
            //_repository.UOW.SaveChanges();
            //CreateNextStatusAuto(entity);
            //return l;
        }

        public override void Update(CNDTSStatusLog entity)
        {
            base.Update(entity);
        }

        public override void Patch(CNDTSStatusLog entity)
        {
            base.Patch(entity);
        }
        public void CreateNextStatusAuto(CNDTSStatusLog entity)
        {
            if (_repository.GetConfigValue<int>("IsCNTrackEnabled") == 0) return;
            if (entity.fk_NextLog != null) return;
            var dtsrepo = _repository.GetRepository<DTSStatus>().Queryable();
            var statusid = dtsrepo.Where(x => x.Id == entity.StatusId)
                               .Select(x => new { x.NextStatusId })
                               .FirstOrDefault()?.NextStatusId ?? 0;
            if (statusid == 0) return;
            var cndts = _repository.Queryable().OrderByDescending(x => x.StartDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.CNId == entity.CNId && x.StatusId == statusid && x.StockLogId == entity.Id) ?? new CNDTSStatusLog();
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
                _repository.Insert(cndts);
            }

        }
    }
}
