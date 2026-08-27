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
    public interface ILoadTypeService : IService<LoadType>
    {
        
    }
    public class LoadTypeService : Service<LoadType>, ILoadTypeService
    {
        private readonly IRepositoryAsync<LoadType> _repository;
        public LoadTypeService(IRepositoryAsync<LoadType> repository) : base(repository)
        {
            _repository = repository;
        }
        
    }
}
