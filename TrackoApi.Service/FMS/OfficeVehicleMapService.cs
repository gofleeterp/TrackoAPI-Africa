using System.Linq;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.FMS;

namespace TrackoApi.Service.FMS
{
    public interface IOfficeVehicleMapService : IService<OfficeVehicleMap>
    {
    }
    public class OfficeVehicleMapService : Service<OfficeVehicleMap>, IOfficeVehicleMapService
    {
        private readonly IRepositoryAsync<OfficeVehicleMap> _repository;
        public OfficeVehicleMapService(IRepositoryAsync<OfficeVehicleMap> repository) : base(repository)
        {
            _repository = repository;
        }

        public override void Delete(OfficeVehicleMap entity)
        {
            //if (_repository.GetRepository<BatteryMaster>().Queryable().Any(x => x.BrandId == entity.Id))
            //{
            //    throw new BusinessException(ErrorCode.GLB108);
            //}
            //base.Delete(entity);
        }
    }
}
