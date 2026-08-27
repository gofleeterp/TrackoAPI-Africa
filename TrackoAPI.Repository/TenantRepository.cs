using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Practices.Unity;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.Models.Global;
using TrackoAPI.ViewModels;

namespace TrackoAPI.Repository
{
    public class TenantRepository
    {
        protected ITrackoApiDbContext TrackoApiDb;
        protected readonly TenantDbContext TenantDb;
        private readonly string _clientKey;
        public string ClientKey { get; set; }
        public string TenantManagerKey { get; set; }
        public TenantRepository(IUnityContainer container)
        {
            if (string.IsNullOrWhiteSpace(TenantManagerKey) || TenantManagerKey != "B41B582F-7B78-4370-A0BD-519E24F8D9B6")
            {
                throw new AccessViolationException("Bad Attempt to access restricted resource.");
            }
            TenantDb = HttpContext.Current.GetOwinContext().Get<TenantDbContext>("TenantDbContext");
            var connection = TenantDb.Tenants.FirstOrDefault(x => x.ClientKey == _clientKey);
            if (connection != null)
            {
                HttpContext.Current.GetOwinContext().Set("as:tenantConnection", connection.ConnectionString);
                TrackoApiDb =container.Resolve<ITrackoApiDbContext>();
                AuthRepo = (AuthRepository) container.Resolve<IAuthRepository>();
            }
        }

        public AuthRepository AuthRepo { get; set; }

        public bool ActivateDeactiveTenant(bool isactive)
        {
            var tenant=TenantDb.Tenants.FirstOrDefault(x => x.ClientKey == _clientKey);
            if (tenant != null) tenant.IsActive = isactive;
            TenantDb.Tenants.AddOrUpdate(tenant);
            var count=TenantDb.SaveChanges();
            return count > 0;
        }

        public bool ActivateDeactiveApp(string applicationId,bool isActive)
        {
            var app = TrackoApiDb.Clients.FirstOrDefault(x => x.ApplicationId == applicationId && x.ClientKey==_clientKey);
            if (app != null) app.IsActive = isActive;
            TrackoApiDb.Clients.AddOrUpdate(app);
            var count=TrackoApiDb.SaveChanges();
            return count > 0;
        }

        public TenantResult RegisterApplication(ApiApplication app)
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
                var tenant = TenantDb.Tenants.FirstOrDefault(x => x.ClientKey == _clientKey);

                if (tenant == null) return new TenantResult();
                var result = TrackoApiDb.Clients.Add(new ApiAppClient
                {
                    ClientKey = tenant.ClientKey,
                    Secret = tenant.Secret,
                    ApplicationId = app.ApplicationId,
                    IsActive = true,
                    AllowedOrigin = app.AllowedOrigin,
                    RefreshTokenLifeTime = app.RefreshTokenLifeTime
                });
                TrackoApiDb.SaveChanges();
                return new TenantResult
                {
                    ClientKey = result.ClientKey,
                    ApplicationId = result.ApplicationId,
                    ClientSecret = result.Secret
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public async Task<List<TenantResult>> RegisterTenant(RegisterTenant tenant)
        {
            var key=Guid.NewGuid().ToString("D");
            var result=new List<TenantResult>();

            var tm = new TenantMaster()
            {
                ConnectionString = tenant.DatabaseConnectionString,
                IsActive = true,
                Name = tenant.TenantName,
                ClientKey = Helper.GetHash(key),
                Secret = Helper.GetHash(tenant.SecretPhrase),
                EmailAddress = tenant.EmailAddress,
                PhoneNumber = tenant.PhoneNumber,
                PostalAddress = tenant.PostalAddress
            };
            
            var apps = tenant.Applications.Select(application => new ApiAppClient
            {
                ClientKey = Helper.GetHash(key),
                Secret = Helper.GetHash(tenant.SecretPhrase),
                ApplicationId = application.ApplicationId,
                AllowedOrigin = application.AllowedOrigin,
                RefreshTokenLifeTime = application.RefreshTokenLifeTime,
                IsActive = true
            }).ToList();
            HttpContext.Current.GetOwinContext().Set("as:tenantConnection", tm.ConnectionString);
            TrackoApiDb = new TrackoApiDbContext();
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
            };

            
            var p = await AuthRepo.RegisterUser(new RegisterUser()
            {
                UserName = tenant.AdminUserName,
                Password = tenant.Password,
                ConfirmPassword = tenant.ConfirmedPassword
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
        public void Dispose()
        {
            TenantDb.Dispose();
            TrackoApiDb.Dispose();
        }
    }
}
