using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System.Collections.Generic;
using System.Linq;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IAPLConfigService : IService<APLConfig>
    {
        IQueryable<APLConfig> GetAllAPLConfigList(int id);
    }
    public class APLConfigService : Service<APLConfig>, IAPLConfigService
    {
        private readonly IRepositoryAsync<APLConfig> _repository;
        public APLConfigService(IRepositoryAsync<APLConfig> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<APLConfig> GetAllAPLConfigList(int brandid)
        {
            return _repository.GetAllAPLConfigList(brandid);
        }
    }
}