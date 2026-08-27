using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackoApi.Models.AMS;

namespace TrackoApi.Service.Finance
{
    public interface IGSTConfigurationService : IService<GSTConfiguration>
    {      
    }

    public class GSTConfigurationService : Service<GSTConfiguration>,IGSTConfigurationService
    {
        private readonly IRepositoryAsync<GSTConfiguration> _repository;

        public GSTConfigurationService(IRepositoryAsync<GSTConfiguration> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
