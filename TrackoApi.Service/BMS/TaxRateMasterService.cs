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
    public interface ITaxRateMasterService : IService<TaxRateMaster>
    {
        
    }
    public class TaxRateMasterService : Service<TaxRateMaster>, ITaxRateMasterService
    {
        private readonly IRepositoryAsync<TaxRateMaster> _repository;
        public TaxRateMasterService(IRepositoryAsync<TaxRateMaster> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
