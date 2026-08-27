using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IMaterialLocationMappingService : IService<MaterialLocationMap>
    {
        
    }
    public class MaterialLocationMappingService : Service<MaterialLocationMap>, IMaterialLocationMappingService
    {
        private readonly IRepositoryAsync<MaterialLocationMap> _repository;
        public MaterialLocationMappingService(IRepositoryAsync<MaterialLocationMap> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
