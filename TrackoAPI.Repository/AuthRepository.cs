using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TrackoApi.Data;
using TrackoApi.Models;
using TrackoApi.Models.Global;
using TrackoAPI.Infrastructure;
using TrackoAPI.ViewModels;

namespace TrackoAPI.Repository
{
    public interface IAuthRepository
    {
        void AddAccessControls(ApiRoleAclBridge acls);
        IQueryable<ApiRole> GetRoles(long userid);
        Task<IdentityResult> RegisterUser(RegisterUser userModel);
        Task<Tuple<ApiUser,ApiSession>> FindUser(string userName,string password,ApiSession session);
        Task<ApiUser> FindUser(string userName, string password);
        ApiAppClient FindClient(string appName,string screte,string key);
        Task<bool> AddRefreshToken(ApiRefreshToken token);
        Task<bool> RemoveRefreshToken(string refreshTokenId);
        Task<ApiRefreshToken> FindRefreshToken(string refreshTokenId);
        List<ApiRefreshToken> GetAllRefreshTokens();
        void Dispose();
    }

    public class AuthRepository:IDisposable, IAuthRepository
    {
        //private readonly TrackoApiDbContext _db;
        private readonly TrackoApiDbContext _db;
        //private readonly UserManager<ApiUser,long> _um;
        private readonly ApiUserManager _um;
        private readonly UserStore<ApiUser, ApiRole, long, ApiUserLogin, ApiUserRole, ApiUserClaim> _us;

        public AuthRepository(ITrackoApiDbContext _context)
        {
            _db= (TrackoApiDbContext) _context;
            //_um=new UserManager<ApiUser,long>();
            _us = new UserStore<ApiUser, ApiRole, long, ApiUserLogin, ApiUserRole, ApiUserClaim>(_db);
            _um = new ApiUserManager(_us);

        }

        //public AuthRepository(string connectionString)
        //{
        //    _db = new TrackoApiDbContext();
        //    //_um = new UserManager<ApiUser, long>(new UserStore<ApiUser, ApiRole, long, ApiUserLogin, ApiUserRole, ApiUserClaim>(_db));
        //    _us = new UserStore<ApiUser, ApiRole, long, ApiUserLogin, ApiUserRole, ApiUserClaim>(_db);
        //    _um = new ApiUserManager(_us);
        //}
        public void AddAccessControls(ApiRoleAclBridge acls)
        {
            try
            {
                _db.ApiAccessControls.Add(acls);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }

        }
        public IQueryable<ApiRole> GetRoles(long userid) => _db.Roles.Include(c => c.AccessList).Where(x => x.Users.Any(y => y.UserId == userid)).AsQueryable();//Users.Include(x => x.Roles).Where(y=>y.Id==userid).AsQueryable();
        public async Task<IdentityResult> RegisterUser(RegisterUser userModel)
        {
            var user = new ApiUser(userModel.UserName);
            var result = await _um.CreateAsync(user, userModel.ConfirmPassword);
            return result;
        }

        public async Task<Tuple<ApiUser,ApiSession>> FindUser(string userName,string password,ApiSession session)
        {
            var user = await _um.FindAsync(userName, password);
            if (user == null) return new Tuple<ApiUser, ApiSession>(user,session);
            var ssn =_db.ApiSessions.Where(x => x.ApplicationId == session.ApplicationId && x.UserId == session.UserId).AsQueryable();
            if (await ssn.AnyAsync())
            {
                await ssn.ForEachAsync(i => i.EndDateTime = DateTime.UtcNow);
            }
            var s=_db.ApiSessions.Add(session);
            await _db.SaveChangesAsync();
            return new Tuple<ApiUser, ApiSession>(user, s);
        }

        public async Task<ApiUser> FindUser(string userName, string password)
        {
            var user = await _um.FindAsync(userName, password);
            return user;
        }
        #region TokenRefresh
        public ApiAppClient FindClient(string appName,string screte,string key)
        {
            var client = _db.Clients.FirstOrDefault(x=>x.ApplicationId==appName && x.Secret==screte&&x.ClientKey==key);
            return client;
        }
        public async Task<bool> AddRefreshToken(ApiRefreshToken token)
        {
            var existingToken = _db.RefreshTokens.SingleOrDefault(r => r.Subject == token.Subject && r.ClientKey == token.ClientKey);
            if (existingToken != null)
            {
                var result = await RemoveRefreshToken(existingToken.Id);
            }
            _db.RefreshTokens.Add(token);
            return await _db.SaveChangesAsync() > 0;
        }
        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(x=>x.Id==refreshTokenId);
            if (refreshToken == null) return false;
            _db.RefreshTokens.Remove(refreshToken);
            return await _db.SaveChangesAsync() > 0;
        }
        public async Task<ApiRefreshToken> FindRefreshToken(string refreshTokenId)
        {
            var refreshToken = await _db.RefreshTokens.FindAsync(refreshTokenId);

            return refreshToken;
        }

        public List<ApiRefreshToken> GetAllRefreshTokens() => _db.RefreshTokens.ToList();
        #endregion
        public void Dispose()
        {
            _db.Dispose();
            _um.Dispose();
        }
    }
}
