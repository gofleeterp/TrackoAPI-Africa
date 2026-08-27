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
    public interface IAPLLogService : IService<APLLog>
    {
        IQueryable<APLLog> GetAllAPLLogList(int id);
    }
    public class APLLogService : Service<APLLog>, IAPLLogService
    {
        private readonly IRepositoryAsync<APLLog> _repository;
        public APLLogService(IRepositoryAsync<APLLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<APLLog> GetAllAPLLogList(int brandid)
        {
            return _repository.GetAllAPLLogList(brandid);
        }
    }
}
