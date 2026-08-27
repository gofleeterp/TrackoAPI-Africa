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
    public interface IVehicleMovementLogPickupDropService : IService<VehicleMovementLogPickupDrop>
    {
        IQueryable<VehicleMovementLogPickupDrop> GetAllVehicleMovementLogPickupDropList(int id);
    }
    public class VehicleMovementLogPickupDropService : Service<VehicleMovementLogPickupDrop>, IVehicleMovementLogPickupDropService
    {
        private readonly IRepositoryAsync<VehicleMovementLogPickupDrop> _repository;
        public VehicleMovementLogPickupDropService(IRepositoryAsync<VehicleMovementLogPickupDrop> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleMovementLogPickupDrop> GetAllVehicleMovementLogPickupDropList(int brandid)
        {
            return _repository.GetAllVehicleMovementLogPickupDropList(brandid);
        }
    }
}
