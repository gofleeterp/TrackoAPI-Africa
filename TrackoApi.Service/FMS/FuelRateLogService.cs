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
    public interface IFuelRateLogService : IService<FuelRateLog>
    {
    }
    public class FuelRateLogService : Service<FuelRateLog>, IFuelRateLogService
    {
        private readonly IRepositoryAsync<FuelRateLog> _repository;
        public FuelRateLogService(IRepositoryAsync<FuelRateLog> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
