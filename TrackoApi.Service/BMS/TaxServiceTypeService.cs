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
    public interface ITaxServiceTypeService : IService<TaxServiceType>
    {
            }
    public class TaxServiceTypeService : Service<TaxServiceType>, ITaxServiceTypeService
    {
        private readonly IRepositoryAsync<TaxServiceType> _repository;
        public TaxServiceTypeService(IRepositoryAsync<TaxServiceType> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
