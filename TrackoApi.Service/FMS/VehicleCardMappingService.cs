using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service
{
    public interface IVehicleCardMappingService : IService<VehicleCardMapping>
    {
    }
    public class VehicleCardMappingService : Service<VehicleCardMapping>, IVehicleCardMappingService
    {
        private IRepositoryAsync<VehicleCardMapping> _repository;
        public VehicleCardMappingService(IRepositoryAsync<VehicleCardMapping> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}