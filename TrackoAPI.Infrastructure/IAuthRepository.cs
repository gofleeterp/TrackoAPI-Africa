using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels;
using TrackoAPI.ViewModels.Global;

namespace TrackoAPI.Infrastructure
{
    public interface IAuthRepository
    {
        void AddAccessControls(ApiRolePermission acls);
        IQueryable<ApiRole> GetRolePermissionByUserId(long userid);//New
        IQueryable<ApiUserRole> GetRolesByUserId(long userid);
        IQueryable<ApiRolePermission> GetObjectsByRoleId(long roleId);
        IQueryable<ApiRolePermission> GetObjectsByRoleIds(List<long> roleIds);
        Task<IdentityResult> CreateUpdateUser(RegisterUser userModel);
        Task<IdentityResult> CreateUpdateRole(vwRole role);
        Task<IdentityResult> SuspenUser(long userId);
        Task CreateSession(ApiSession session);
        Task<ApiUser> FindUserAsync(string userName, string password);
        Task<ApiAppClient> FindClient(string appName, string screte, string key);
        Task<bool> AddRefreshToken(ApiRefreshToken token);
        Task<bool> RemoveRefreshToken(string refreshTokenId);
        Task<ApiRefreshToken> FindRefreshToken(string refreshTokenId);
        IQueryable<ApiView> Views();
        IQueryable<ApiViewModule> Modules();
        IQueryable<ApiRefreshToken> GetAllRefreshTokens();
        IQueryable<ApiRole> GetRoles();
        IQueryable<ApiUser> FindUserById(long id);
        List<UserResourceResult> UserPermissions(long userId);
        IQueryable<ApiConfiguration> ApiConfigurations { get; }
        IQueryable<ClientConfiguration> ClientConfigurations { get; }
        Task<Tuple<bool, string>> IsVersionBugFree(string version, long? viewid);
        void Dispose();
        IQueryable<ApiUser> Users();
        void Begin(IsolationLevel level = IsolationLevel.ReadCommitted);
        void Commit();
        void Rollback();
        Task<IdentityResult> ModifyRoleACL(long roleid, List<vwApiRolePermission> assigned);
        Task<IdentityResult> DeleteRole(long id);
        Task<int> GetFianaceStatus();
        Task<bool> IsIpAuthorized(long userId, string IpAddress);
        Task<bool> RegisterDeviceAsync(vwApiDevice vwDevice, string tenantEmail, string tenantName,string phoneNumber);
        Task<bool> IsDeviceAuthorized(string deviceIdentity, string oldDeviceId);
        Task<bool> AuthorizeDevice(string deviceIdentity, string otp);
        Task<bool> UnAuthorizeDevice(string deviceIdentity);

        IQueryable<UserDefinedReport> UserDefined();
        IQueryable<ApiSession> Sessions { get; }
        Task<IdentityResult> ChangePassword(ChangePassword password);
    }
}