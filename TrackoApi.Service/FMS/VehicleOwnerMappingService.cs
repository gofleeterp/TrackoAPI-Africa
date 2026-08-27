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
    public interface IVehicleOwnerMappingService : IService<VehicleOwnerMapping>
    {
        IQueryable<VehicleOwnerMapping> GetAllVehicleOwnerMappingList(int id);
    }
    public class VehicleOwnerMappingService : Service<VehicleOwnerMapping>, IVehicleOwnerMappingService
    {
        private readonly IRepositoryAsync<VehicleOwnerMapping> _repository;
        public VehicleOwnerMappingService(IRepositoryAsync<VehicleOwnerMapping> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleOwnerMapping> GetAllVehicleOwnerMappingList(int brandid)
        {
            return _repository.GetAllVehicleOwnerMappingList(brandid);
        }
    }
}
