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
    public interface IVehicleTailorMappingService : IService<VehicleTailorMapping>
    {
        IQueryable<VehicleTailorMapping> GetAllVehicleTailorMappingList(int id);
    }
    public class VehicleTailorMappingService : Service<VehicleTailorMapping>, IVehicleTailorMappingService
    {
        private readonly IRepositoryAsync<VehicleTailorMapping> _repository;
        public VehicleTailorMappingService(IRepositoryAsync<VehicleTailorMapping> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<VehicleTailorMapping> GetAllVehicleTailorMappingList(int brandid)
        {
            return _repository.GetAllVehicleTailorMappingList(brandid);
        }
    }
}
