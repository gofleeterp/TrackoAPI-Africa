using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoAPI.Repository;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Tyres;
using TrackoApi.Models.Global;

namespace TrackoApi.Service
{
    public interface ITyreMillageLogService : IService<TyreMillageLog>
    {
    }
    public class TyreMillageLogService : Service<TyreMillageLog>, ITyreMillageLogService
    {
        private readonly IRepositoryAsync<TyreMillageLog> _repository;
        public TyreMillageLogService(IRepositoryAsync<TyreMillageLog> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
