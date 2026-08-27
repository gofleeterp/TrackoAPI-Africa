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
    public interface IBrandMasterService : IService<BrandMaster>
    {
    }
    public class BrandMasterService : Service<BrandMaster>, IBrandMasterService
    {
        private readonly IRepositoryAsync<BrandMaster> _repository;
        public BrandMasterService(IRepositoryAsync<BrandMaster> repository) : base(repository)
        {
            _repository = repository;
        }

        public override void Delete(BrandMaster entity)
        {
            if (_repository.GetRepository<TyreMaster>().Queryable().Any(x => x.BrandId == entity.Id))
            {
                throw new BusinessException(ErrorCode.GLB108);
            }
            base.Delete(entity);
        }
    }
}
