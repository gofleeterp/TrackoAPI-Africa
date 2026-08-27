using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS.Repairs;

namespace TrackoApi.Service
{
    public interface IStoreBINMasterService : IService<StoreBinMaster>
    {
    }
    public class StoreBINMasterService : Service<StoreBinMaster>, IStoreBINMasterService
    {
        private readonly IRepositoryAsync<StoreBinMaster> _repository;
        public StoreBINMasterService(IRepositoryAsync<StoreBinMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public override StoreBinMaster Insert(StoreBinMaster entity)
        {
            return base.Insert(entity);
        }
    }
}
