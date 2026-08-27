using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IVehiclePMService : IService<VehiclePreventiveLog>
    {
    }
    public class VehiclePMService : Service<VehiclePreventiveLog>, IVehiclePMService
    {
        private readonly IRepositoryAsync<VehiclePreventiveLog> _repository;
        public VehiclePMService(IRepositoryAsync<VehiclePreventiveLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public override VehiclePreventiveLog Insert(VehiclePreventiveLog entity)
        {
            var pmid = entity.NewPMId ?? entity.PMId;
            if (entity.PreviousLogId.GetValueOrDefault(0) == 0 ||
                !_repository.Queryable().Any(x => x.NewPMId == pmid && x.VehicleId == entity.VehicleId))
                return base.Insert(entity);
            {
                var lastLog = _repository.Queryable().Where(x => x.NewPMId == pmid && x.VehicleId == entity.VehicleId).OrderByDescending(x=>x.JobDate).FirstOrDefault();
                if (lastLog == null)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Previous Log Reference is Required");
                }
                entity.fk_PreviousLog = lastLog;
                entity.PreviousLogId = lastLog.Id;
                lastLog.NextLogId = entity.Id;
                lastLog.fk_NextLog = entity;
                lastLog.ObjectState=ObjectState.Modified;
                base.Update(lastLog);
            }
            return base.Insert(entity);
        }
    }
}
