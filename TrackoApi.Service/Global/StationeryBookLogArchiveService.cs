using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Global;

namespace TrackoApi.Service
{
    public interface IStationeryBookLogArchiveService : IService<StationeryBookLogArchive>
    {
       
    }
    public class StationeryBookLogArchiveService : Service<StationeryBookLogArchive>, IStationeryBookLogArchiveService
    {
        private readonly IRepositoryAsync<StationeryBookLogArchive> _repository;
        public StationeryBookLogArchiveService(IRepositoryAsync<StationeryBookLogArchive> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
