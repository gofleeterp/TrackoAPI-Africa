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
    public interface IMaterialPartyService : IService<MaterialParty>
    {
        
    }
    public class MaterialPartyService : Service<MaterialParty>, IMaterialPartyService
    {
        private readonly IRepositoryAsync<MaterialParty> _repository;
        public MaterialPartyService(IRepositoryAsync<MaterialParty> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
