using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IAPLLogAnxLevelService : IService<APLLogAnxLevel>
    {
        IQueryable<APLLogAnxLevel> GetAllAPLLogAnxLevelList(int id);
    }
    public class APLLogAnxLevelService : Service<APLLogAnxLevel>, IAPLLogAnxLevelService
    {
        private readonly IRepositoryAsync<APLLogAnxLevel> _repository;
        public APLLogAnxLevelService(IRepositoryAsync<APLLogAnxLevel> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<APLLogAnxLevel> GetAllAPLLogAnxLevelList(int brandid)
        {
            return _repository.GetAllAPLLogAnxLevelList(brandid);
        }
    }
}
