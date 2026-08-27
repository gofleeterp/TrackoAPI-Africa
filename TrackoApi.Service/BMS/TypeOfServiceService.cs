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
    public interface ITypeOfServiceService : IService<TaxTypeService>
    {
        
    }
    public class TypeOfServiceService : Service<TaxTypeService>, ITypeOfServiceService
    {
        private readonly IRepositoryAsync<TaxTypeService> _repository;
        public TypeOfServiceService(IRepositoryAsync<TaxTypeService> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
