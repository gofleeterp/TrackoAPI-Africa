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
    public interface ICNBillNatureService : IService<CNBillNature>
    {
        
    }
    public class CNBillNatureService : Service<CNBillNature>, ICNBillNatureService
    {
        private readonly IRepositoryAsync<CNBillNature> _repository;
        public CNBillNatureService(IRepositoryAsync<CNBillNature> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
