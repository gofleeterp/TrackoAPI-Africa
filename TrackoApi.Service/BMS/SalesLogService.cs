using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface ISalesLogService : IService<SalesLog>
    {
      
    }
    public class SalesLogService : Service<SalesLog>, ISalesLogService
    {
        private readonly IRepositoryAsync<SalesLog> _repo;
        public SalesLogService(IRepositoryAsync<SalesLog> repository) : base(repository)
        {
            _repo = repository;
        }
    }
}
