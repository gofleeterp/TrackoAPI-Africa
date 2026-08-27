using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoAPI.Repository;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace TrackoApi.Service
{
    public interface IDriverVehicleMappingService : IService<DriverVehicleMapping>
    {

    }
    public class DriverVehicleMappingService : Service<DriverVehicleMapping>, IDriverVehicleMappingService
    {
        private readonly IRepositoryAsync<DriverVehicleMapping> _repository;
        public DriverVehicleMappingService(IRepositoryAsync<DriverVehicleMapping> repository) : base(repository)
        {
            _repository = repository;
        }

        public override async Task<DriverVehicleMapping> InsertAsync(DriverVehicleMapping entity)
        {
            entity.ObjectState = ObjectState.Added;
            var _query = _repository.Queryable()
                .OrderByDescending(x => x.StatusDate)
                .ThenByDescending(x => x.Id)
                .Where(x => x.DriverRoleId == entity.DriverRoleId);


            if (!((entity.DriverStatusId == 1657 || entity.DriverStatusId == 1658)) && entity.VehicleId.GetValueOrDefault(0) > 0)
            {
                _query = _query.Where(y => y.VehicleId == entity.VehicleId);
            }
            else {
                _query = _query.Where(y => y.DriverId == entity.DriverId);
            }

                var vehiclelog = await _query.Select(x => new
                {
                    x.StatusDate,
                    x.DriverStatusId,
                    x.DriverId,
                    x.DriverRoleId
                }).FirstOrDefaultAsync();

            if (vehiclelog != null)
            {
                if (vehiclelog.StatusDate > entity.StatusDate)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Status Date should be greater than or equal to Previous Status Date For the Vehicle");
                }
                if (vehiclelog.DriverStatusId == entity.DriverStatusId)
                {
                    throw new BusinessException(ErrorCode.GLB106, "There is already driver mapped to this vehicle with specified role and status");
                }
                if ((vehiclelog.DriverStatusId == 1225/*OnVehicle*/ && vehiclelog.DriverId != entity.DriverId))/*Next Status Should of Same Driver*/
                {
                    throw new BusinessException(ErrorCode.GLB106, "Invalid Driver Status Selected");
                }
                if ((vehiclelog.DriverStatusId == 1225/*OnVehicle*/  && vehiclelog.DriverRoleId != entity.DriverRoleId))
                {
                    throw new BusinessException(ErrorCode.GLB106, "Invalid Driver Role Selected");
                }
            }

            var previousLog = await _repository.Queryable().OrderByDescending(x => x.StatusDate).ThenByDescending(x => x.Id).FirstOrDefaultAsync(x => x.DriverId == entity.DriverId);
            if (previousLog == null) return await base.InsertAsync(entity);

            if ((!await _repository.GetRepository<DriverNextStatusMapping>().Queryable()
                .AnyAsync(x => x.CurrentStatusId == previousLog.DriverStatusId && x.NextStatusId == entity.DriverStatusId)) || (previousLog.DriverStatusId == 1225 && previousLog.DriverId != entity.DriverId))
            {
                throw new BusinessException(ErrorCode.GLB106, "Invalid Driver Status Selected");
            }
            if (previousLog.StatusDate > entity.StatusDate)
            {
                throw new BusinessException(ErrorCode.GLB106, "Status Date should be greater than or equal to Previous Status Date For the Driver");
            }
            if (previousLog.DriverStatusId == entity.DriverStatusId)
            {
                throw new BusinessException(ErrorCode.GLB106, "There is already driver mapped to this vehicle with specified role and status");
            }
            entity.PreviousLogId = previousLog.Id;
            entity.fk_PreviousLog = previousLog;
            previousLog.NextLogId = entity.Id;
            previousLog.fk_NextLog = entity;
            previousLog.ObjectState = ObjectState.Modified;
            return await base.InsertAsync(entity);
        }

        public override async Task PatchAsync(DriverVehicleMapping entity)
        {

            if (await _repository.Queryable().AnyAsync(x => x.PreviousLogId == entity.Id))
            {
                throw new BusinessException(ErrorCode.GLB106, "Only current status is allowed to update.");
            }
            var previousLog = _repository.Queryable().OrderByDescending(x => x.StatusDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.DriverId == entity.DriverId && x.NextLogId == entity.Id);
            if (previousLog != null)
            {
                if ((!await _repository.GetRepository<DriverNextStatusMapping>().Queryable()
                        .AnyAsync(x => x.CurrentStatusId == previousLog.DriverStatusId && x.NextStatusId == entity.DriverStatusId)) || (previousLog.DriverStatusId == 1225 && previousLog.DriverId != entity.DriverId))
                {
                    throw new BusinessException(ErrorCode.GLB106, "Invalid Driver Status Selected");
                }
                if (previousLog.StatusDate > entity.StatusDate)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Status Date should be greater than or equal to Previous Status Date For the Driver");
                }
                if (previousLog.DriverStatusId == entity.DriverStatusId)
                {
                    throw new BusinessException(ErrorCode.GLB106, "There is already driver mapped to this vehicle with specified role and status");
                }
                entity.PreviousLogId = previousLog.Id;
                entity.fk_PreviousLog = previousLog;
                previousLog.NextLogId = entity.Id;
                previousLog.fk_NextLog = entity;
                previousLog.ObjectState = ObjectState.Modified;
            }
            await base.PatchAsync(entity);
        }

        public override async Task UpdateAsync(DriverVehicleMapping entity)
        {
            if (await _repository.Queryable().AnyAsync(x => x.PreviousLogId == entity.Id))
            {
                throw new BusinessException(ErrorCode.GLB106, "Only current status is allowed to update.");
            }
            var previousLog = _repository.Queryable().OrderByDescending(x => x.StatusDate).ThenByDescending(x => x.Id).FirstOrDefault(x => x.DriverId == entity.DriverId && x.NextLogId == entity.Id);
            if (previousLog != null)
            {
                if ((!await _repository.GetRepository<DriverNextStatusMapping>().Queryable()
                        .AnyAsync(x => x.CurrentStatusId == previousLog.DriverStatusId && x.NextStatusId == entity.DriverStatusId)) || (previousLog.DriverStatusId == 1225 && previousLog.DriverId != entity.DriverId))
                {
                    throw new BusinessException(ErrorCode.GLB106, "Invalid Driver Status Selected");
                }
                if (previousLog.StatusDate > entity.StatusDate)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Status Date should be greater than or equal to Previous Status Date For the Driver");
                }
                if (previousLog.DriverStatusId == entity.DriverStatusId)
                {
                    throw new BusinessException(ErrorCode.GLB106, "There is already driver mapped to this vehicle with specified role and status");
                }
                entity.PreviousLogId = previousLog.Id;
                entity.fk_PreviousLog = previousLog;
                previousLog.NextLogId = entity.Id;
                previousLog.fk_NextLog = entity;
                previousLog.ObjectState = ObjectState.Modified;
            }
            await base.UpdateAsync(entity);
        }
    }
}
