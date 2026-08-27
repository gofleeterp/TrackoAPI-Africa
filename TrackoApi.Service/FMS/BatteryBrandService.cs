using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoAPI.Repository;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Service
{
    public interface IBatteryBrandService : IService<BatteryBrand>
    {
    }
    public class BatteryBrandService : Service<BatteryBrand>, IBatteryBrandService
    {
        private readonly IRepositoryAsync<BatteryBrand> _repository;
        public BatteryBrandService(IRepositoryAsync<BatteryBrand> repository) : base(repository)
        {
            _repository = repository;
        }

        public override void Delete(BatteryBrand entity)
        {
            if (_repository.GetRepository<BatteryMaster>().Queryable().Any(x => x.BrandId == entity.Id))
            {
                throw new BusinessException(ErrorCode.GLB108);
            }
            base.Delete(entity);
        }
    }
}
