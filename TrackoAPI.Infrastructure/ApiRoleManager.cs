using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using TrackoApi.Core;
using TrackoApi.Data;
using TrackoApi.Models.Global;

namespace TrackoAPI.Infrastructure
{
    public class ApiRoleManager: RoleManager<ApiRole,long>
    {
        public ApiRoleManager(IRoleStore<ApiRole, long> roleStore)
            : base(roleStore)
        {
        }

        public static ApiRoleManager Create(IdentityFactoryOptions<ApiRoleManager> options, IOwinContext context,IGlobalStore gs)
        {
            var appRoleManager = new ApiRoleManager(new RoleStore<ApiRole,long,ApiUserRole>(new TrackoApiDbContext(gs)));
            return appRoleManager;
        }
    }
}
