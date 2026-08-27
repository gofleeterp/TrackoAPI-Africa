using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Global;

namespace TrackoApi.Service
{
    public interface IStationeryBookLogService : IService<StationeryBookLog>
    {
       
    }
    public class StationeryBookLogService : Service<StationeryBookLog>, IStationeryBookLogService
    {
        private readonly IRepositoryAsync<StationeryBookLog> _repository;
        public StationeryBookLogService(IRepositoryAsync<StationeryBookLog> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
