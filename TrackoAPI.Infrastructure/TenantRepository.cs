using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels;
using Unity;

namespace TrackoAPI.Infrastructure
{
    public interface ITenantRepository
    {
        string ClientKey { get; }
        string TenantManagerKey { get; }
        void Intialize(string _clientkey,string _supperKey);
        bool ActivateDeactiveTenant(bool isactive);
        bool ActivateDeactiveApp(string appName,bool isActive);
        Task<TenantResult> AssignApplication(ApiApplication app);
        Task<List<TenantResult>> RegisterTenant(RegisterTenant tenant);
        Task<string> CreateNewApp(Application app);
        Task<TPTokenViewModel> GetTokenInfoAsync(string token);
        Task<List<JsonGLLog>> GetJsonLogsAsync(string prefix, string jsonKey);
        Task PostJsonLogsAsync(List<JsonGLLog> glLogs);
        void Dispose();
    }

    public class TenantRepository : ITenantRepository
    {
        protected ITrackoApiDbContext TrackoApiDb;
        protected TenantDbContext TenantDb;
        private IUnityContainer _unity;
        private IGlobalStore _gs;

        public string ClientKey { get; private set; }

        public string TenantManagerKey { get; private set; }

        public TenantRepository(IUnityContainer container)
        {
            _unity = container;
            _gs = _unity.Resolve<IGlobalStore>();
        }

        public void Intialize(string _clientkey,string _supperKey)
        {
            if (string.IsNullOrWhiteSpace(_supperKey) || _supperKey != "B41B582F-7B78-4370-A0BD-519E24F8D9B6")
            {
                throw new AccessViolationException("Bad Attempt to access restricted resource.");
            }
            TenantManagerKey = _supperKey;
            ClientKey = _clientkey;
            TenantDb = new TenantDbContext();
            var connection = TenantDb.Tenants.FirstOrDefault(x => x.ClientKey == _clientkey);
            if (connection != null)
            {
                HttpContext.Current.GetOwinContext().Set("as:tenantConnection", connection.ConnectionString);
                TrackoApiDb = _unity.Resolve<ITrackoApiDbContext>();
            }
        }

        public Task<TenantViewModel> GetTenantInfoAsync(string clientKey,string tenantId)
        {
            //TODO:Allowed Origin Logic need to be implemented
            if (string.IsNullOrWhiteSpace(clientKey) && string.IsNullOrWhiteSpace(tenantId)) return null;
            var query = TenantDb.Tenants;
            return TenantDb.Tenants.Where(x => x.ClientKey == clientKey || x.Id == tenantId).Select(x => new TenantViewModel
            {
                Id = x.Id,
                Name = x.Name,
                PostalAddress = x.PostalAddress,
                EmailAddress = x.EmailAddress,
                Apps = x.Apps.Select(y => new TenantAppViewModel
                {
                    ApplicationId = y.ApplicationId,
                    IsActive = y.IsActive,
                    ApplicationType = y.fk_Application.ApplicationType,
                    UpdateUrl = y.UpdateUrl,
                    NoOfUsers = y.NoOfActiveUsers,
                    AppName = y.fk_Application.ApplicationName,
                    SetupUrl = y.SetupUrl,
                    FormatUrl = y.FormatUrl
                }).ToList(),
                LogType = x.LogType,
                IsActive = x.IsActive,
                IsSingleUserMode = x.IsSingleUserMode,
                IsHostedOnPremise = x.IsHostedOnPremise,
                ClientKey = x.ClientKey,
                PANNo = x.PANNo,
                Secret = x.Secret,
                AccessCode = x.AccessCode,
                ShortName = x.ShortName,
                ServerUrl = x.ServerUrl,
                PhoneNumber = x.PhoneNumber,
                RemoteBackupPath = x.RemoteBackupPath,
                WebAddress = x.WebAddress,
                ConstCurTypeId=x.ConstCurTypeId
            }).FirstOrDefaultAsync();
        }
        public bool ActivateDeactiveTenant(bool isactive)
        {
            var tenant=TenantDb.Tenants.FirstOrDefault(x => x.ClientKey == ClientKey);
            if (tenant != null) tenant.IsActive = isactive;
            TenantDb.Tenants.AddOrUpdate(tenant);
            var count=TenantDb.SaveChanges();
            return count > 0;
        }

        public bool ActivateDeactiveApp(string appName,bool isActive)
        {
            var app = TrackoApiDb.Clients.FirstOrDefault(x => x.ApplicationId == appName && x.ClientKey==ClientKey);
            if (app != null) app.IsActive = isActive;
            TrackoApiDb.Clients.AddOrUpdate(app);
            var count=TrackoApiDb.SaveChanges();
            return count > 0;
        }

        public async Task<TenantResult> AssignApplication(ApiApplication app)
        {
            try
            {
                var existingApp = TrackoApiDb.Clients.FirstOrDefault(x => x.ApplicationId == app.ApplicationId);
                if (existingApp != null)
                    return new TenantResult
                    {
                        ClientKey = existingApp.ClientKey,
                        ApplicationId = existingApp.ApplicationId,
                        ClientSecret = existingApp.Secret
                    };
                var tenant = TenantDb.Tenants.FirstOrDefault(x => x.ClientKey == ClientKey);
                var tApp = await TenantDb.Applications.FindAsync(app.ApplicationId);
                if (tenant == null) return new TenantResult();

                tenant.Applications.Add(tApp);
                
                var result = TrackoApiDb.Clients.Add(new ApiAppClient
                {
                    ClientKey = tenant.ClientKey,
                    Secret = tenant.Secret,
                    ApplicationId = app.ApplicationId,
                    IsActive = true,
                    AllowedOrigin = app.AllowedOrigin,
                    RefreshTokenLifeTime = app.RefreshTokenLifeTime
                });
                var tcount = await TenantDb.SaveChangesAsync();
                if (tcount == 0) return null;
                var trcount= await TrackoApiDb.SaveChangesAsync();
                if (trcount != 0)
                    return new TenantResult
                    {
                        ClientKey = result.ClientKey,
                        ApplicationId = result.ApplicationId,
                        ClientSecret = result.Secret
                    };
                tenant.Applications.Remove(tApp);
                await TenantDb.SaveChangesAsync();
                return null;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<TenantResult>> RegisterTenant(RegisterTenant tenant)
        {
            try
            {
                var key = Guid.NewGuid().ToString("D").ToLower();
                var result = new List<TenantResult>();
                var tm = new TenantMaster()
                {
                    ConnectionString = tenant.DatabaseConnectionString,
                    IsActive = true,
                    Name = tenant.TenantName,
                    ClientKey = Helper.GetHash(key),
                    Secret = Helper.GetHash(tenant.SecretPhrase),
                    EmailAddress = tenant.EmailAddress,
                    PhoneNumber = tenant.PhoneNumber,
                    PostalAddress = tenant.PostalAddress,
                    PANNo = tenant.PAN,
                    Id = key,
                    AccessCode = tenant.AccessCode,
                    LogType = LogType.All,
                    ServerUrl = tenant.ServerUrl,
                    IsHostedOnPremise = false,
                    IsSingleUserMode = false
                };
                foreach (var app in tenant.Applications)
                {
                    tm.Applications.Add(TenantDb.Applications.Find(app.ApplicationId));
                    tm.Apps.Add(new TenantApplicationMapping()
                    {
                        fk_Tenant = tm,
                        ApplicationId = app.ApplicationId,
                        IsActive = true,
                        TenantId = tm.Id,
                        UpdateUrl = $"{tenant.ServerUrl}/applications/updates/GOF/saas/UpdateManifest.xml",
                        NoOfActiveUsers = app.NoOfActiveUsers,
                        FormatUrl = tenant.FormatUrl,
                        SetupUrl = tenant.SetupUrl
                    });
                }

                var apps = tenant.Applications.Select(application => new ApiAppClient
                {
                    ClientKey = Helper.GetHash(key),
                    Secret = Helper.GetHash(tenant.SecretPhrase),
                    ApplicationId = application.ApplicationId,
                    AllowedOrigin = application.AllowedOrigin,
                    RefreshTokenLifeTime = application.RefreshTokenLifeTime,
                    ObjectState = ObjectState.Added,
                    IsActive = true
                }).ToList();
                HttpContext.Current.GetOwinContext().Set("as:tenantConnection", tm.ConnectionString);
                TrackoApiDb = new TrackoApiDbContext(_gs);
                TrackoApiDb.Clients.AddRange(apps);
                var tcount = await TrackoApiDb.SaveChangesAsync();
                if (tcount == 0) return result;

                TenantDb.Tenants.Add(tm);
                var tmcount = await TenantDb.SaveChangesAsync();
                if (tmcount == 0)
                {
                    TrackoApiDb.Clients.RemoveRange(apps);
                    TrackoApiDb.SaveChanges();
                    return result;
                }
                var repo = _unity.Resolve<IAuthRepository>();
                var p = await repo.CreateUpdateUser(new RegisterUser()
                {
                    UserName = tenant.AdminUserName,
                    Password = tenant.Password,
                    ConfirmPassword = tenant.ConfirmedPassword,
                    UserType = 200,
                    IsRoaming = true
                });
                if (p.Succeeded)
                {
                    result.AddRange(apps.Select(app => new TenantResult
                    {
                        ClientKey = tm.ClientKey,
                        ApplicationId = app.ApplicationId,
                        ClientSecret = app.Secret
                    }));
                    return result;
                }

                TrackoApiDb.Clients.RemoveRange(apps);
                TrackoApiDb.SaveChanges();
                TenantDb.Tenants.Remove(tm);
                TenantDb.SaveChanges();
                return result;
            }
            catch (Exception e)
            {
                return new List<TenantResult>(){new TenantResult()
                {
                    ApplicationId = e.Message,
                    ClientKey = e.StackTrace
                }};
            }
        }
      
      public async Task<string> CreateNewApp(Application app)
        {
            try
            {
                if (TenantDb.Applications.Any(x => x.ApplicationName == app.ApplicationName))
                {
                    throw new Exception("Application Already Exists.");
                }
               var entity=TenantDb.Applications.Add(new Application
                {
                    ApplicationName = app.ApplicationName,
                    ApplicationType = app.ApplicationType,
                    IsActive = true
                });
               await TenantDb.SaveChangesAsync();
                return entity.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Task<TPTokenViewModel> GetTokenInfoAsync(string token)
        {
            return TenantDb.ThirdPartyTokens
                .Where(x => token == x.Token)
                .Select(x => new TPTokenViewModel
                {
                    Token=x.Token,
                    AllowedPath=x.AllowedPath,
                    Appidentity=x.Appidentity,
                    ExpiryDate=x.ExpiryDate,
                    Interval=x.Interval,
                    IsDeactivated=x.IsDeactivated,
                    JsonMetaData=x.JsonMetaData,
                    LastCalledTime=x.LastCalledTime,
                    TenantId=x.TenantId                    
                }).FirstOrDefaultAsync();
        }
        public Task<List<JsonGLLog>> GetJsonLogsAsync(string prefix,string jsonKey)
        {
            IQueryable<JsonGlobalLog> query;
            
            if (!string.IsNullOrWhiteSpace(jsonKey))
            {
                query = TenantDb.JsonLog
                    .Where(x => prefix == x.KeyPrefix&&jsonKey==x.JsonKey);
            }
            else
            {
                query = TenantDb.JsonLog
                    .Where(x => prefix == x.KeyPrefix);
            }
            return query
                .Select(x => new JsonGLLog
                {
                    JsonData            = x.JsonData,
                    JsonKey = x.JsonKey,
                    KeyPrefix = x.KeyPrefix
                }).ToListAsync();
        }
        public Task PostJsonLogsAsync(List<JsonGLLog> glLogs)
        {
            return Task.CompletedTask;
        }
        public void Dispose()
        {
            TenantDb?.Dispose();
            TrackoApiDb?.Dispose();
        }
    }
}
