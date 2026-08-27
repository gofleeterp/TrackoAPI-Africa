using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoAPI.Repository;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;

namespace TrackoApi.Service
{
    public interface IDriverNextStatusMappingService : IService<DriverNextStatusMapping>
    {
      
    }
    public class DriverNextStatusMappingService : Service<DriverNextStatusMapping>, IDriverNextStatusMappingService
    {
        private readonly IRepositoryAsync<DriverNextStatusMapping> _repository;
        public DriverNextStatusMappingService(IRepositoryAsync<DriverNextStatusMapping> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
