using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS.GPS;

namespace TrackoApi.Service.FMS.GPS
{
    public interface IGPSKmLogService : IService<GPSKmLog>
    {
    }
    public class GPSKmLogService : Service<GPSKmLog>, IGPSKmLogService
    {
        private readonly IRepositoryAsync<GPSKmLog> _repository;
        public GPSKmLogService(IRepositoryAsync<GPSKmLog> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
