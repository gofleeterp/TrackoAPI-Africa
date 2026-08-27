using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IVehicleAccessoryLogService : IService<VehicleAccessoryLog>
    {
        IQueryable<VehicleAccessoryLog> GetAllVehicleAccessoryLogList(int id);
    }
    public class VehicleAccessoryLogService : Service<VehicleAccessoryLog>, IVehicleAccessoryLogService
    {
        private readonly IRepositoryAsync<VehicleAccessoryLog> _repository;
        public VehicleAccessoryLogService(IRepositoryAsync<VehicleAccessoryLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleAccessoryLog> GetAllVehicleAccessoryLogList(int brandid)
        {
            return _repository.GetAllVehicleAccessoryLogList(brandid);
        }
    }
}
