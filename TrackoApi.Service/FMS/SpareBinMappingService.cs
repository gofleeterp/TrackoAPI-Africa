using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS.Repairs;

namespace TrackoApi.Service
{
    public interface ISpareBinMappingService : IService<SpareBinMapping>
    {
    }
    public class SpareBinMappingService : Service<SpareBinMapping>, ISpareBinMappingService
    {
        private readonly IRepositoryAsync<SpareBinMapping> _repository;
        public SpareBinMappingService(IRepositoryAsync<SpareBinMapping> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
