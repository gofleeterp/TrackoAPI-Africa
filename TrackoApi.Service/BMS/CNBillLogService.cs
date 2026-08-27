using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    
    public interface ICNBillLogService : IService<CNBillLog>
    {
      
    }
    public class CNBillLogService : Service<CNBillLog>, ICNBillLogService
    {
        private readonly IRepositoryAsync<CNBillLog> _repository;
        public CNBillLogService(IRepositoryAsync<CNBillLog> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
