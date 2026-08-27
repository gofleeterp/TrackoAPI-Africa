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
    public interface IAPLLogAnxService : IService<APLLogAnx>
    {
        IQueryable<APLLogAnx> GetAllAPLLogAnxList(int id);
    }
    public class APLLogAnxService : Service<APLLogAnx>, IAPLLogAnxService
    {
        private readonly IRepositoryAsync<APLLogAnx> _repository;
        public APLLogAnxService(IRepositoryAsync<APLLogAnx> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<APLLogAnx> GetAllAPLLogAnxList(int brandid)
        {
            return _repository.GetAllAPLLogAnxList(brandid);
        }
    }
}
