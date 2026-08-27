using System.Linq;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IBatteryCheckService : IService<BatteryCheck>
    {
    }
    public class BatteryCheckService : Service<BatteryCheck>, IBatteryCheckService
    {
        private readonly IRepositoryAsync<BatteryCheck> _repository;
        public BatteryCheckService(IRepositoryAsync<BatteryCheck> repository) : base(repository)
        {
            _repository = repository;
        }

        public override void Delete(BatteryCheck entity)
        {
            if (_repository.GetRepository<BatteryLog>().Queryable().Any(x => x.BatteryCheckId == entity.Id)) throw new BusinessException(ErrorCode.GLB106, "Cannot Delete this Inspection Transaction as it was Created from Log.");
            base.Delete(entity);
        }
    }
}
