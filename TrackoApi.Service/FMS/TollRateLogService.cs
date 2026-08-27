using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.FMS;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ITollRateLogService : IService<TollRateLog>
    {
        IQueryable<TollRateLog> GetAllTollRateLogList(int id);
    }
    public class TollRateLogService : Service<TollRateLog>, ITollRateLogService
    {
        private readonly IRepositoryAsync<TollRateLog> _repository;
        public TollRateLogService(IRepositoryAsync<TollRateLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public IQueryable<TollRateLog> GetAllTollRateLogList(int brandid)
        {
            return _repository.GetAllTollRateLogList(brandid);
        }
    }
}
