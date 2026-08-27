using Microsoft.Extensions.Caching.Memory;
using RestSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TrackoApi.Core.Helpers;
using Unity;

namespace TrackoApi.Core
{


    public class GlobalStoreOnPremises : IGlobalStore
    {
        //public GlobalStoreOnPremises()
        //{
        //}
        private readonly IMemoryCache _cache;
        private readonly MemoryCacheEntryOptions _cacheTransOpt;

        [InjectionConstructor]
        public GlobalStoreOnPremises(IMemoryCache cacheClient) //: this()
        {
            _cache = cacheClient;
            _cacheTransOpt= new MemoryCacheEntryOptions()
             // Keep in cache for this time, reset time if accessed.
             .SetSlidingExpiration(TimeSpan.FromMinutes(30)).SetSize(100000);
            //Instance = this;
        }
        //public static GlobalStoreOnPremises Instance { get; internal set; }
        public ConcurrentDictionary<string, string> ConnectionStringCache { get; } = new ConcurrentDictionary<string, string>();

        public ConcurrentDictionary<string, object> Constants => new ConcurrentDictionary<string, object>();
        public Func<string, string> DbConnectionCallback { get; set; }
        public ConcurrentDictionary<Type, MappingAPI> MappingCache { get; } = new ConcurrentDictionary<Type, MappingAPI>();

        #region Active Users

        public ConcurrentDictionary<string, List<ConnectedUser>> SignalRUsers { get; } = new ConcurrentDictionary<string, List<ConnectedUser>>();
        
        public void AddUser(string tenantid, ConnectedUser user)
        {
            try
            {
                //RemoveToken(tenantid, refreshTokenId);
                var key = $"ActiveUsers_{tenantid}";
                if (!SignalRUsers.ContainsKey(tenantid))
                {
                    if (_cache.TryGetValue<List<ConnectedUser>>(key, out var existingusers))
                    {
                        if (existingusers != null && existingusers.Any())
                        {
                            SignalRUsers.TryAdd(tenantid, existingusers);
                        }
                    }
                }
                SignalRUsers.AddOrUpdate(tenantid, new List<ConnectedUser>(), (s, list) =>
                {
                    if (list == null) list = new List<ConnectedUser>();
                    list.RemoveAll(x => x.UserId == user.UserId);
                    list.Add(user);
                    _cache.Remove(key);
                    _cache.Set(key, list, _cacheTransOpt);
                    return list;
                });
            }
            catch
            {//Ignore

            }
        }

        public List<ConnectedUser> GetAllConnectedUsers(string tenantid)
        {
            try
            {
                var key = $"ActiveUsers_{tenantid}";
                if (!SignalRUsers.ContainsKey(tenantid))
                {
                    if (_cache.TryGetValue<List<ConnectedUser>>(key, out var existingusers))
                    {
                        if (existingusers != null && existingusers.Any())
                        {
                            SignalRUsers.TryAdd(tenantid, existingusers);
                            return existingusers;
                        }
                    }
                }
                SignalRUsers.TryGetValue(tenantid, out var users);
                return users;
            }
            catch
            {//Ignore
                return new List<ConnectedUser>();
            }
        }

        public void RemoveUser(string tenantid, long userId)
        {
            try
            {
                var key = $"ActiveUsers_{tenantid}";
                if (!SignalRUsers.ContainsKey(tenantid))
                {
                    if (_cache.TryGetValue<List<ConnectedUser>>(key, out var existingusers))
                    {
                        if (existingusers != null && existingusers.Any())
                        {
                            SignalRUsers.TryAdd(tenantid, existingusers);
                        }
                    }
                }
                SignalRUsers.AddOrUpdate(tenantid, new List<ConnectedUser>(), (s, list) =>
                {
                    if (list == null) list = new List<ConnectedUser>();
                    var isremoved = list?.RemoveAll(x => x?.UserId == userId);
                    if (isremoved > 0)
                    {
                        _cache.Remove(key);
                        _cache.Set(key, list, _cacheTransOpt);
                    }
                    return list;
                });
            }
            catch (StackExchange.Redis.RedisServerException ex)
            {
                throw new BusinessException(ErrorCode.GLB107, $"Cache Service is down.\n{ex.GetBaseException().Message}");
            }
            catch (Exception ex)
            {
                //Ignore
            }
        }
        #endregion Active Users

        #region Access Token

        /// <summary>
        /// <example>TenantKey,Tokens</example>
        /// </summary>
        public ConcurrentDictionary<string, List<string>> AccessTokens { get; } = new ConcurrentDictionary<string, List<string>>();

        public void AddToken(string tenantid, string refreshTokenId, TimeSpan expiry)
        {
            try
            {
                //RemoveToken(tenantid, refreshTokenId);
                var key = $"AccessTokens_{tenantid}";
                if (!AccessTokens.ContainsKey(tenantid))
                {
                    if (_cache.TryGetValue<List<string>>(key, out var existingtokens))
                    {
                        if (existingtokens != null && existingtokens.Any())
                        {
                            AccessTokens.TryAdd(tenantid, existingtokens);
                        }
                    }
                }
                AccessTokens.AddOrUpdate(tenantid, new List<string>(), (s, list) =>
                {
                    if (list == null) list = new List<string>();
                    list?.Add(refreshTokenId);
                    if(_cache.TryGetValue(key,out var exobj))
                    {
                        _cache.Remove(key);
                    }          
                    _cache.Set(key, list, new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiry, Size = 1, Priority = CacheItemPriority.High });
                    return list;
                });
            }
            catch
            {//Ignore

            }
        }
        public void LogOutAllUsers(string tenantid)
        {
            try
            {
                AccessTokens.TryUpdate(tenantid, new List<string>(), new List<string>());
            }
            catch
            {
                //Ignore
            }
        }
        public bool IsTokenExists(string tenantid, string refreshTokenId)
        {
            try
            { 
            if (!AccessTokens.ContainsKey(tenantid))
            {
                var key = $"AccessTokens_{tenantid}";
                if (_cache.TryGetValue<List<string>>(key, out var existingtokens))
                {
                    if (existingtokens != null && existingtokens.Any())
                    {
                        AccessTokens.TryAdd(tenantid, existingtokens);
                    }
                }
            }
            AccessTokens.TryGetValue(tenantid, out var tokens);
            return tokens != null && tokens.Any(x => x == refreshTokenId);
            }
            catch
            {
                return false;
            }
        }

        public void RemoveToken(string tenantid, string refreshTokenId)
        {
            try
            {
                var key = $"AccessTokens_{tenantid}";
                if (_cache.TryGetValue<List<string>>(key, out var existingtokens))
                {
                    if (existingtokens != null && existingtokens.Any())
                    {
                        AccessTokens.TryAdd(tenantid, existingtokens);
                    }
                }
                AccessTokens.AddOrUpdate(tenantid, new List<string>(), (s, list) =>
                {
                    if (list == null) list = new List<string>();
                    var isremoved = list?.Remove(refreshTokenId) ?? false;
                    if (isremoved)
                    {
                        _cache.Remove(key);
                        _cache.Set(key, list, _cacheTransOpt);
                    }
                    return list;
                });
            }
            catch (Exception)
            {
            }
        }
        #endregion Access Token

        public ConcurrentDictionary<string, TenantViewModel> Tenants { get; } = new ConcurrentDictionary<string, TenantViewModel>();

        public ConcurrentDictionary<string, TPTokenViewModel> Tokens { get; } = new ConcurrentDictionary<string, TPTokenViewModel>();

        public void CleanRadisCache(string loggedInTenantId = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(loggedInTenantId))
                {
                    var keys = Tenants.Select(x => $"TenantInfo_{x.Key}").ToList();
                    if (keys != null && keys.Any())
                    {
                        foreach (var key in keys)
                        {
                            _cache.Remove(key);
                        }
                    }
                    Tenants?.Clear();
                    var ckeys = Tenants.Select(x => $"ConnectionStrings_{x.Value.Id}").ToList();
                    if (ckeys != null && ckeys.Any())
                    {
                        foreach (var key in ckeys)
                        {
                            _cache.Remove(key);
                        }
                    }
                    ConnectionStringCache?.Clear();
                }
                else
                {
                    var key = Tenants.Where(x => x.Value.Id == loggedInTenantId).Select(x => x.Key).FirstOrDefault();
                    _cache.Remove($"TenantInfo_{key}");
                    Tenants.TryRemove(key, out var _);
                    _cache.Remove($"ConnectionStrings_{loggedInTenantId}");
                    ConnectionStringCache.TryRemove(loggedInTenantId, out var _);
                }
            }
            catch
            {
                //Ignore
            }
        }
        public IDbConnection CreateDbConnection(string tenantId)
        {
            var connString = GetOrAddConnectionString(tenantId);
            var connection = new SqlConnection(connString);
            return connection;
        }
        public string GetOrAddConnectionString(string tenantId)
        {
            try
            {
                if (ConnectionStringCache.ContainsKey(tenantId))
                {
                    if (ConnectionStringCache.TryGetValue(tenantId, out var connectionString) && !string.IsNullOrWhiteSpace(connectionString)) return connectionString;
                }
                if (DbConnectionCallback == null)
                {
                    if (Helper.HostedOnPremise)
                    {
                        var connection = ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
                        if (string.IsNullOrWhiteSpace(connection)) throw new BusinessException(ErrorCode.GLB103, "Hosted OnPremise configuration not found");
                        ConnectionStringCache.TryAdd(tenantId, connection);
                        return connection;
                    }
                }
                return GetOrAddConnectionString(tenantId, DbConnectionCallback);
            }
            catch
            {
                return "";
            }
        }

        public string GetOrAddConnectionString(string tenantId, Func<string, string> callback)
        {
            try
            {
                if (ConnectionStringCache.ContainsKey(tenantId))
                {
                    if (ConnectionStringCache.TryGetValue(tenantId, out var connectionString) && !string.IsNullOrWhiteSpace(connectionString)) return connectionString;
                }
                if (callback != null && DbConnectionCallback == null && !Helper.HostedOnPremise)
                {
                    DbConnectionCallback = callback;
                }
                if (callback == null && DbConnectionCallback != null && !Helper.HostedOnPremise)
                {
                    callback = DbConnectionCallback;
                }
                if (Helper.HostedOnPremise)
                {
                    var connection = ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
                    if (string.IsNullOrWhiteSpace(connection)) throw new BusinessException(ErrorCode.GLB103, "Hosted OnPremise configuration not found");
                    ConnectionStringCache.TryAdd(tenantId, connection);
                    return connection;
                }
                var key = $"ConnectionStrings_{tenantId}";

                if (_cache.TryGetValue<string>(key, out var cs))
                {
                    ConnectionStringCache.TryAdd(tenantId, cs);
                    return cs;
                }
                else
                {
                    var connectionString = callback(tenantId);
                    if (string.IsNullOrWhiteSpace(connectionString)) throw new BusinessException(ErrorCode.GLB103, "Storage configuration not found");
                    ConnectionStringCache.TryAdd(tenantId, connectionString);
                    _cache.Set(key, connectionString, _cacheTransOpt);
                    return connectionString;
                }            
            }
            catch
            {
                return "";
            }
        }
        public void ClearThirdPartyTokens()
        {
            try
            {
                foreach (var token in this.Tokens)
                {
                    _cache.Remove(token.Key);
                }
                this.Tokens.Clear();
            }
            catch
            {
                //Ignore
            }
        }
        public TenantViewModel GetOrAddTenant(string clienkKey)
        {
            return GetOrAddTenant(clienkKey, GetTenantByClientKey);
        }
        public TPTokenViewModel GetOrAddToken(string token, Func<string, TPTokenViewModel> callback)
        {
            var key = $"TokenInfo_{token}";
            if (Tokens != null && Tokens.ContainsKey(token))
            {
                Tokens.TryGetValue(token, out var tei);
                if (tei != null)
                {
                    return tei;
                }
                Tokens.TryRemove(token, out var _);
            }
            if (_cache.TryGetValue<TPTokenViewModel>(key, out var ti))
            {
                if (ti != null)
                {
                    if (!Tokens.ContainsKey(token))
                    {
                        Tokens.TryAdd(token, ti);
                    }
                    return ti;
                }
            }
            var tokenInfo = callback(token);
            if (tokenInfo != null)
            {
                _cache.Set(key, tokenInfo, _cacheTransOpt);
                Tokens.TryAdd(token, tokenInfo);
            }
            return tokenInfo;
        }

        public TenantViewModel GetOrAddTenant(string clienkKey, Func<string, TenantViewModel> callback)
        {
            var key = $"TenantInfo_{clienkKey}";
            if (Tenants != null && Tenants.ContainsKey(clienkKey))
            {
                Tenants.TryGetValue(clienkKey, out var tei);
                if (tei != null)
                {
                    return tei;
                }
                Tenants.TryRemove(clienkKey, out var _);
            }
            if (_cache.TryGetValue<TenantViewModel>(key,out var ti))
            {
                if (ti != null)
                {
                    if (string.IsNullOrWhiteSpace(ti.ConnectionString))
                    {
                        if (Helper.HostedOnPremise)
                        {
                            if (!ti.IsHostedOnPremise)
                            {
                                throw new BusinessException(ErrorCode.GLB103, $"{ti.Name} is not Hosted on selected server");
                            }
                            var connection = ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
                            if (string.IsNullOrWhiteSpace(connection)) throw new BusinessException(ErrorCode.GLB103, "Hosted OnPremise configuration not found");
                            ti.ConnectionString = connection;
                        }
                        else
                        {
                            if (ti.IsHostedOnPremise)
                            {
                                throw new BusinessException(ErrorCode.GLB103, $"{ti.Name} is not Hosted on selected server");
                            }
                            ti.ConnectionString = this.GetOrAddConnectionString(ti.Id);
                        }
                    }
                    if (!Tenants.ContainsKey(clienkKey))
                    {
                        Tenants.TryAdd(clienkKey, ti);
                    }
                    return ti;
                }
            }
            var tenantInfo = callback(clienkKey);
            if (tenantInfo != null)
            {
                if (Helper.HostedOnPremise)
                {
                    if (!tenantInfo.IsHostedOnPremise)
                    {
                        throw new BusinessException(ErrorCode.GLB103, $"{tenantInfo.Name} is not Hosted on selected server");
                    }
                    var connection = ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
                    if (string.IsNullOrWhiteSpace(connection)) throw new BusinessException(ErrorCode.GLB103, "Hosted OnPremise configuration not found");
                    tenantInfo.ConnectionString = connection;
                }
                else if (string.IsNullOrWhiteSpace(tenantInfo.ConnectionString))
                {
                    if (tenantInfo.IsHostedOnPremise)
                    {
                        throw new BusinessException(ErrorCode.GLB103, $"{tenantInfo.Name} is not Hosted on selected server");
                    }
                    tenantInfo.ConnectionString = this.GetOrAddConnectionString(tenantInfo.Id);
                }
//#if DEBUG
//                tenantInfo.ConnectionString= ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
//#endif
                _cache.Set(key, tenantInfo,_cacheTransOpt);
                Tenants.TryAdd(clienkKey, tenantInfo);
            }
            return tenantInfo;
        }
        public TenantViewModel GetTenant(string clientKey = "", string tenantId = "")
        {
            var client = new RestClient(Helper.GatewayUrl + "/Tenant/GetTenantInfo");
            var request = new RestRequest(Method.POST);
            request.AddHeader("godkey", "B41B582F-7B78-4370-A0BD-519E24F8D9B6");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("tenant_client_key", clientKey);
            request.AddHeader("tenant_id", tenantId);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            IRestResponse<TenantViewModel> response = client.ExecuteAsPost<TenantViewModel>(request, "POST");
#if DEBUG
            if (Helper.HostedOnPremise&& response.Data!=null)
            {
                response.Data.IsHostedOnPremise = true;
            }
#endif
            return response.IsSuccessful ? response.Data : null;
        }

        public TenantViewModel GetTenantByClientKey(string clientKey) => GetTenant(clientKey: clientKey);

        public TenantViewModel GetTenantByTenantId(string tenantId) => GetTenant(tenantId: tenantId);

        public void RefreshRadisCache(string tenantId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenantId)) return;
                var tenantInfo = GetTenant(tenantId: tenantId);
                if (tenantInfo != null)
                {
                    if (Helper.HostedOnPremise)
                    {
                        var connection = ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
                        if (string.IsNullOrWhiteSpace(connection)) throw new BusinessException(ErrorCode.GLB103, "Hosted OnPremise configuration not found");
                        tenantInfo.ConnectionString = connection;
                    }
                    else if (string.IsNullOrWhiteSpace(tenantInfo.ConnectionString))
                    {
                        try
                        {
                            tenantInfo.ConnectionString = this.GetOrAddConnectionString(tenantInfo.Id);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    var key = $"TenantInfo_{tenantInfo.ClientKey}";
                    _cache.Remove(key);
                    _cache.Set(key, tenantInfo,new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) ,Size=1,Priority=CacheItemPriority.High});
                    Tenants.TryRemove(tenantInfo.ClientKey, out var _);
                    Tenants.TryAdd(tenantInfo.ClientKey, tenantInfo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public TPTokenViewModel GetOrAddToken(string token)
        {
            return GetOrAddToken(token, GetToken);
        }
        public void UpdateTokenAccessTime(string token, DateTime timestamp)
        {
            try { 
            var key = $"TokenInfo_{token}";
            if (Tokens.TryGetValue(key, out var tokeninfo) && tokeninfo != null)
            {
                tokeninfo.LastCalledTime = timestamp;
                Tokens.TryRemove(key, out _);
                Tokens.TryAdd(key, tokeninfo);                
                if (_cache.TryGetValue(key,out _))
                {
                    _cache.Remove(key);
                    _cache.Set(key, tokeninfo, _cacheTransOpt);
                }
            }
            }
            catch
            {
                //Ignore
            }
        }

        public TPTokenViewModel GetToken(string token = "")
        {
            var client = new RestClient(Helper.GatewayUrl + "/Tenant/GetTokenInfo");
            var request = new RestRequest(Method.POST);
            request.AddHeader("godkey", "B41B582F-7B78-4370-A0BD-519E24F8D9B6");
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("token", token);
            request.AddHeader("Accept", "application/json");
            request.AddHeader("Content-Type", "application/json");
            IRestResponse<TPTokenViewModel> response = client.ExecuteAsPost<TPTokenViewModel>(request, "POST");
            return response.IsSuccessful ? response.Data : null;
        }
    }

}