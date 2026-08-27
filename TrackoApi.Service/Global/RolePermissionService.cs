using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.Pattern.Core;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Global;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IRolePermissionService : IService<ApiRolePermission>
    {
    }
    public class RolePermissionService : Service<ApiRolePermission>, IRolePermissionService
    {
        private readonly IRepositoryAsync<ApiRolePermission> _repository;
        public RolePermissionService(IRepositoryAsync<ApiRolePermission> repository) : base(repository)
        {
            _repository = repository;
        }
    }
}
