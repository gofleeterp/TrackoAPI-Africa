using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS.Repairs;

namespace TrackoApi.Service
{
    public interface ISpareInventoryLevelService : IService<SpareInventoryLevel>
    {
    }
    public class SpareInventoryLevelService : Service<SpareInventoryLevel>, ISpareInventoryLevelService
    {
        private readonly IRepositoryAsync<SpareInventoryLevel> _repository;
        public SpareInventoryLevelService(IRepositoryAsync<SpareInventoryLevel> repository) : base(repository)
        {
            _repository = repository;
        }

        public override SpareInventoryLevel Insert(SpareInventoryLevel entity)
        {
            return base.Insert(entity);
        }
    }
}
