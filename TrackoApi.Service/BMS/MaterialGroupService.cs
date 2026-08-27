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
    public interface IMaterialGroupService : IService<MaterialGroup>
    {
        
    }
    public class MaterialGroupService : Service<MaterialGroup>, IMaterialGroupService
    {
        private readonly IRepositoryAsync<MaterialGroup> _repository;
        public MaterialGroupService(IRepositoryAsync<MaterialGroup> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
