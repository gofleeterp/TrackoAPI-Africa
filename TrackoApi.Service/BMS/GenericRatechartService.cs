using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service.TMS
{
    public interface IGenericRatechartService : IService<VehicleConfigurationLog>
    {
    }
    public class GenericRatechartService : Service<VehicleConfigurationLog>, IGenericRatechartService
    {
        private readonly IRepositoryAsync<VehicleConfigurationLog> _repository;
        public GenericRatechartService(IRepositoryAsync<VehicleConfigurationLog> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
