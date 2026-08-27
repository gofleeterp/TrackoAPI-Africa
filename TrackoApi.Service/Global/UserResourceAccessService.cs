using Repository.Pattern.Core;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IUserResourceAccessService : IService<ApiResourceAccessLog>
    {
        void AddOrUpdateResourceLog(ApiResourceAccessLog log);
        long GetResourceId(string resourceName, AclType resourceType);
    }
    public class UserResourceAccessService : Service<ApiResourceAccessLog>, IUserResourceAccessService
    {
        private readonly IRepositoryAsync<ApiResourceAccessLog> _repository;
        public UserResourceAccessService(IRepositoryAsync<ApiResourceAccessLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public void AddOrUpdateResourceLog(ApiResourceAccessLog log)
        {
            _repository.AddOrUpdateLog(log);
        }

        public long GetResourceId(string resourceName, AclType resourceType)
        {
           return _repository.GetResourceId(resourceName, resourceType);
        }
    }
}
