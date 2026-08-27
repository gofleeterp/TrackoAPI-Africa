using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IObjectClassMapService : IService<ObjectClassMap>
    {
        
    }
    public class ObjectClassMapService : Service<ObjectClassMap>, IObjectClassMapService
    {
        private readonly IRepositoryAsync<ObjectClassMap> _repository;
        public ObjectClassMapService(IRepositoryAsync<ObjectClassMap> repository) : base(repository)
        {
            _repository = repository;
        }

    }
}
