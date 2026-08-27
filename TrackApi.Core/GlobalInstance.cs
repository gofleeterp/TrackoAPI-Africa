using Newtonsoft.Json;
using RestSharp;
using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.SqlClient;
using System.Linq;
using TrackoApi.Core.Helpers;
using TrackoAPI.Models.Shared;
using Unity;

namespace TrackoApi.Core
{
    public class ConnectedUser
    {
        public ConnectedUser()
        {
            ConnectedTime = DateTime.Now;
            Groups = new List<string>();
        }
        public DateTime ConnectedTime { get; set; }
        public string ConnectionId { get; set; }
        public string DisplayName { get; set; }
        public List<string> Groups { get; set; }
        public long SessionId { get; set; }
        public string TenantId { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
    }

    public class GlobalStore : IGlobalStore
    {
        //public GlobalStore()
        //{
        //}
        private ICacheClient _radis;
        public ICacheClient RedisCache => _radis;

        [InjectionConstructor]
        public GlobalStore(ICacheClient cacheClient) //: this()
        {
            _radis = cacheClient;
            //Instance = this;
        }
        //public static GlobalStore Instance { get; internal set; }
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
                if (user == null || string.IsNullOrWhiteSpace(tenantid)) return;
                //RemoveToken(tenantid, refreshTokenId);
                var key = $"ActiveUsers_{tenantid}";
                if (!SignalRUsers.ContainsKey(tenantid))
                {
                    if (_radis.Exists(key))
                    {
                        var rtokens = _radis.Get<List<ConnectedUser>>(key);
                        if (rtokens != null && rtokens.Any())
                        {
                            SignalRUsers.TryAdd(tenantid, rtokens);
                        }
                    }
                }
                SignalRUsers.AddOrUpdate(tenantid, new List<ConnectedUser>(), (s, list) =>
                {
                    if (list == null) list = new List<ConnectedUser>();
                    if (list.Any(x => x.UserId == user.UserId))
                    {
                        list.RemoveAll(x => x?.UserId == user?.UserId);
                    }
                    list.Add(user);
                    _radis.Replace(key, list, TimeSpan.FromDays(1));
                    return list;
                });
            }
            catch(Exception ex)
            {
                //ignore
            }
        }

        public List<ConnectedUser> GetAllConnectedUsers(string tenantid)
        {
            try
            {
                var key = $"ActiveUsers_{tenantid}";
            if (!SignalRUsers.ContainsKey(tenantid))
            {
                if (_radis.Exists(key))
                {
                    var rtokens = _radis.Get<List<ConnectedUser>>(key);
                    if (rtokens != null && rtokens.Any())
                    {
                        SignalRUsers.TryAdd(tenantid, rtokens);
                    }
                }
            }
            SignalRUsers.TryGetValue(tenantid, out var users);
            return users;
            }
            catch (Exception ex)
            {
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
                    if (_radis.Exists(key))
                    {
                        var rtokens = _radis.Get<List<ConnectedUser>>(key);
                        if (rtokens != null && rtokens.Any())
                        {
                            SignalRUsers.TryAdd(tenantid, rtokens);
                        }
                    }
                }
                SignalRUsers.AddOrUpdate(tenantid, new List<ConnectedUser>(), (s, list) =>
                {
                    if (list == null) list = new List<ConnectedUser>();
                    var isremoved = list.RemoveAll(x => x?.UserId == userId);
                    if (isremoved > 0)
                    {
                        _radis.Replace(key, list, TimeSpan.FromDays(1));
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
                throw;
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
            //RemoveToken(tenantid, refreshTokenId);
            var key = $"AccessTokens_{tenantid}";
            if (!AccessTokens.ContainsKey(tenantid))
            {
                if (_radis.Exists(key))
                {
                    var rtokens = _radis.Get<List<string>>(key);
                    if (rtokens != null && rtokens.Any())
                    {
                        AccessTokens.TryAdd(tenantid, rtokens);
                    }
                }
            }
            AccessTokens.AddOrUpdate(tenantid, new List<string>(), (s, list) =>
            {
                if (list == null) list = new List<string>();
                list.Add(refreshTokenId);
                _radis.Replace(key, list, expiry);
                return list;
            });
        }
        public void LogOutAllUsers(string tenantid)
        {
            try
            {
                if (!AccessTokens.ContainsKey(tenantid))
                {
                    var key = $"AccessTokens_{tenantid}";
                    if (_radis.Exists(key))
                    {
                        _radis.Remove(key);
                    }
                }
                AccessTokens.TryUpdate(tenantid, new List<string>(), new List<string>());
            }
            catch{
                //Ignore
            }
        }
        public bool IsTokenExists(string tenantid, string refreshTokenId)
        {
            if (!AccessTokens.ContainsKey(tenantid))
            {
                var key = $"AccessTokens_{tenantid}";
                if (_radis.Exists(key))
                {
                    var rtokens = _radis.Get<List<string>>(key);
                    if (rtokens != null && rtokens.Any())
                    {
                        AccessTokens.TryAdd(tenantid, rtokens);
                    }
                }
            }
            AccessTokens.TryGetValue(tenantid, out var tokens);
            return tokens != null && tokens.Any(x => x == refreshTokenId);
        }

        public void RemoveToken(string tenantid, string refreshTokenId)
        {
            try
            {
                var key = $"AccessTokens_{tenantid}";
                if (!AccessTokens.ContainsKey(tenantid))
                {
                    if (_radis.Exists(key))
                    {
                        var rtokens = _radis.Get<List<string>>(key);
                        if (rtokens != null && rtokens.Any())
                        {
                            AccessTokens.TryAdd(tenantid, rtokens);
                        }
                    }
                }
                AccessTokens.AddOrUpdate(tenantid, new List<string>(), (s, list) =>
                {
                    if (list == null) list = new List<string>();
                    var isremoved = list?.Remove(refreshTokenId) ?? false;
                    if (isremoved)
                    {
                        _radis.Replace(key, list, TimeSpan.FromDays(1));
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
                        _radis.RemoveAll(keys);
                    }
                    Tenants?.Clear();
                    var ckeys = Tenants.Select(x => $"ConnectionStrings_{x.Value.Id}").ToList();
                    if (ckeys != null && ckeys.Any())
                    {
                        _radis.RemoveAll(ckeys);
                    }
                    ConnectionStringCache?.Clear();
                }
                else
                {
                    var key = Tenants.Where(x => x.Value.Id == loggedInTenantId).Select(x => x.Key).FirstOrDefault();
                    _radis.Remove($"TenantInfo_{key}");
                    Tenants.TryRemove(key, out var _);
                    _radis.Remove($"ConnectionStrings_{loggedInTenantId}");
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

        public string GetOrAddConnectionString(string tenantId, Func<string, string> callback)
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

            if (_radis.Exists(key))
            {
                var result = _radis.Get<string>(key);
                ConnectionStringCache.TryAdd(tenantId, result);
                return result;
            }
            else
            {
                var connectionString = callback(tenantId);
                if (string.IsNullOrWhiteSpace(connectionString)) throw new BusinessException(ErrorCode.GLB103, "Storage configuration not found");
                ConnectionStringCache.TryAdd(tenantId, connectionString);
                _radis.Add(key, connectionString, TimeSpan.FromDays(1));
                return connectionString;
            }
        }
        public TPTokenViewModel GetOrAddToken(string token)
        {
            return GetOrAddToken(token, GetToken);
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
            if (_radis.Exists(key))
            {
                var ti = _radis.Get<TPTokenViewModel>(key);
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
                _radis.Add(key, tokenInfo, TimeSpan.FromHours(5));
                Tokens.TryAdd(token, tokenInfo);
            }
            return tokenInfo;
        }
        public void ClearThirdPartyTokens()
        {
            try
            {
                foreach (var token in this.Tokens)
                {
                    var key = $"TokenInfo_{token.Key}";
                    if (_radis.Exists(key))
                    {
                        _radis.Remove(key);
                    }
                }
                this.Tokens.Clear();
            }
            catch
            {
                //Ignore
            }
        }
        public void UpdateTokenAccessTime(string token,DateTime timestamp)
        {
            try
            {
                var key = $"TokenInfo_{token}";
                if (Tokens.TryGetValue(key, out var tokeninfo) && tokeninfo != null)
                {
                    tokeninfo.LastCalledTime = timestamp;
                    Tokens.TryRemove(key, out _);
                    Tokens.TryAdd(key, tokeninfo);
                    if (_radis.Exists(key))
                    {
                        _radis.Remove(key);
                        _radis.Add(key, tokeninfo);
                    }
                }
            }
            catch
            {
                //Ignore
            }
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
            if (_radis.Exists(key))
            {
                var ti = _radis.Get<TenantViewModel>(key);
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
                _radis.Add(key, tenantInfo, TimeSpan.FromDays(1));
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
            if (Helper.HostedOnPremise && response.Data != null)
            {
                response.Data.IsHostedOnPremise = true;
            }
#endif
            return response.IsSuccessful ? response.Data : null;
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
                    _radis.Replace(key, tenantInfo, TimeSpan.FromDays(1));
                    Tenants.TryRemove(tenantInfo.ClientKey, out var _);
                    Tenants.TryAdd(tenantInfo.ClientKey, tenantInfo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public class InnerSelect
    {
        public InnerSelect()
        {
            Parameters = new List<DbParameter>();
        }

        public List<DbParameter> Parameters { get; set; }
        public string Sql { get; set; }
    }

    public class MappingAPI
    {
        public EntitySet EntitySet { get; set; }
        public ICollection<EdmMember> KeyMembers { get; set; }
        public Type SetType { get; set; }
        public string TableName { get; set; }
    }
    public class TenantAppViewModel
    {
        public string ApplicationId { get; set; }
        public ApplicationCategory ApplicationType { get; set; }
        public string AppName { get; set; }
        public bool IsActive { get; set; }
        public int NoOfUsers { get; set; }
        public string UpdateUrl { get; set; }
        public string SetupUrl { get; set; }
        public string FormatUrl { get; set; }
    }
    public  class JsonGLLog{
        public string KeyPrefix { get; set; }
        public string JsonKey { get; set; }
        public string JsonData { get; set; }
    }
    public class TPTokenViewModel
    {
        public string Token { get; set; }
        public string Appidentity { get; set; }
        public int Interval { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string AllowedPath { get; set; }
        public string JsonMetaData { get; set; }
        public bool IsDeactivated { get; set; }
        public DateTime? LastCalledTime { get; set; }
        public string TenantId { get; set; }
        public bool IsValidCall(out string ErrorMessage, string controller = null, string action = null)
        {
            ErrorMessage = "";
            if (Interval>0 && LastCalledTime != null && ((LastCalledTime.Value.AddMinutes(Interval) > DateTime.Now) || (DateTime.Now.Subtract(LastCalledTime.Value).TotalMinutes < Interval)))
            {
                ErrorMessage = $"Next Call would be allowed after {LastCalledTime.Value.AddMinutes(Interval):dd-MMM-yyyy HH:mm:ss}";
                return false;
            }
            if (ExpiryDate != null && ExpiryDate < DateTime.Now)
            {
                ErrorMessage = $"Token has been Expired";
                return false;
            }
            if (IsDeactivated)
            {
                ErrorMessage = $"Token has been revoked";
                return false;
            }
            if (string.IsNullOrWhiteSpace(AllowedPath))
            {
                AllowedPath = "*";
            }
            if (AllowedPath != "*")
            {
                if (string.IsNullOrWhiteSpace(controller))
                {
                    ErrorMessage = $"The resource({controller}) you are looking for is not available. ErrorCode:304";
                    return false;
                }
                try
                {
                    var dics = JsonConvert.DeserializeObject<Dictionary<string, string>>(AllowedPath);
                    if (dics != null)
                    {
                        if (!dics.ContainsKey(controller))
                        {
                            ErrorMessage = $"The resource({controller}) you are looking for is not available.. ErrorCode:314";
                            return false;
                        }
                        if (!string.IsNullOrWhiteSpace(action) && action != "*")
                        {
                            if (dics.TryGetValue(controller, out string actionName) && !action.Equals(actionName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                ErrorMessage = $"The resource({controller}/{action}) you are looking for is not available. ErrorCode:319";
                                return false;
                            }
                        }
                    }
                    else
                    {
                        ErrorMessage = $"The resource({controller}/{action}) you are looking for is not available. ErrorCode:326";
                        return false;
                    }
                }
                catch(Exception ex)
                {
                    ErrorMessage = ex.GetBaseException().Message;
                    return false;
                }
            }
            if (string.IsNullOrWhiteSpace(TenantId))
            {
                ErrorMessage = "Token is not configured";
                return false;
            }
            return true;
        }
    }
    public class TenantViewModel
    {
        public int AccessCode { get; set; }
        public List<TenantAppViewModel> Apps { get; set; }
        public string ClientKey { get; set; }
        public string ConnectionString { get; set; }
        public string EmailAddress { get; set; }
        public string Id { get; set; }

        public bool IsActive { get; set; }
        public bool IsHostedOnPremise { get; set; } = false;
        public bool IsSingleUserMode { get; set; } = false;
        public LogType LogType { get; set; }
        public string Name { get; set; }
        public string PANNo { get; set; }
        public string PhoneNumber { get; set; }
        public string PostalAddress { get; set; }
        public string RemoteBackupPath { get; set; }
        public string Secret { get; set; }
        public string ServerUrl { get; set; }
        public string ShortName { get; set; }
        public string WebAddress { get; set; }
        public int ConstCurTypeId { get; set; }
    }
}