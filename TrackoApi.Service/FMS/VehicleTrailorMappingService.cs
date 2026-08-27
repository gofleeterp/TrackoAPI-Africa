using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service
{
    public interface IVehicleTrailorMappingService : IService<VehicleTrailorMapping>
    {
    }
    public class VehicleTrailorMappingService : Service<VehicleTrailorMapping>, IVehicleTrailorMappingService
    {
        private IRepositoryAsync<VehicleTrailorMapping> _repository;
        public VehicleTrailorMappingService(IRepositoryAsync<VehicleTrailorMapping> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}