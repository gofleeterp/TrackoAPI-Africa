using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IVehicleRepairJobService : IService<VehicleRepairJob>
    {
    }
    public class VehicleRepairJobService : Service<VehicleRepairJob>, IVehicleRepairJobService
    {
        private readonly IRepositoryAsync<VehicleRepairJob> _repository;
        public VehicleRepairJobService(IRepositoryAsync<VehicleRepairJob> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
