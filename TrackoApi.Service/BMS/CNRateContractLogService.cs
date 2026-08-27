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
    public interface ICNRateContractLogService : IService<CNRateContractLog>
    {
      
    }
    public class CNRateContractLogService : Service<CNRateContractLog>, ICNRateContractLogService
    {
        private readonly IRepositoryAsync<CNRateContractLog> _repository;
        public CNRateContractLogService(IRepositoryAsync<CNRateContractLog> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
