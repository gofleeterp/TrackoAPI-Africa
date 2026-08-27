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
    public interface ICNRateContractService : IService<CNRateContract>
    {
      
    }
    public class CNRateContractService : Service<CNRateContract>, ICNRateContractService
    {
        private readonly IRepositoryAsync<CNRateContract> _repository;
        public CNRateContractService(IRepositoryAsync<CNRateContract> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
