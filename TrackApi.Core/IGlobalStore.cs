using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;

namespace TrackoApi.Core
{
    public interface IGlobalStore
    {
        ConcurrentDictionary<string, List<string>> AccessTokens { get; }
        ConcurrentDictionary<string, string> ConnectionStringCache { get; }
        ConcurrentDictionary<string, object> Constants { get; }
        Func<string, string> DbConnectionCallback { get; set; }
        ConcurrentDictionary<Type, MappingAPI> MappingCache { get; }
        ConcurrentDictionary<string, List<ConnectedUser>> SignalRUsers { get; }
        ConcurrentDictionary<string, TenantViewModel> Tenants { get; }
        ConcurrentDictionary<string, TPTokenViewModel> Tokens { get; }

        void AddToken(string tenantid, string refreshTokenId, TimeSpan expiry);
        void AddUser(string tenantid, ConnectedUser user);
        void CleanRadisCache(string loggedInTenantId = "");
        List<ConnectedUser> GetAllConnectedUsers(string tenantid);
        IDbConnection CreateDbConnection(string tenantId);
        string GetOrAddConnectionString(string tenantId);
        string GetOrAddConnectionString(string tenantId, Func<string, string> callback);
        TenantViewModel GetOrAddTenant(string clienkKey);
        TPTokenViewModel GetOrAddToken(string token);
        TenantViewModel GetOrAddTenant(string clienkKey, Func<string, TenantViewModel> callback);
        TPTokenViewModel GetOrAddToken(string token, Func<string, TPTokenViewModel> callback);
        void ClearThirdPartyTokens();
        TenantViewModel GetTenant(string clientKey = "", string tenantId = "");
        TPTokenViewModel GetToken(string token = "");
        TenantViewModel GetTenantByClientKey(string clientKey);
        TenantViewModel GetTenantByTenantId(string tenantId);
        bool IsTokenExists(string tenantid, string refreshTokenId);
        void LogOutAllUsers(string tenantid);
        void RefreshRadisCache(string tenantId);
        void RemoveToken(string tenantid, string refreshTokenId);
        void RemoveUser(string tenantid, long userId);
        void UpdateTokenAccessTime(string token, DateTime timestamp);
    }
}