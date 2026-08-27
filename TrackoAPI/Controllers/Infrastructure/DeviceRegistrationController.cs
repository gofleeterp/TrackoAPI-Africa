using Microsoft.AspNet.Identity.Owin;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoAPI.Infrastructure;
using TrackoAPI.ViewModels;
using Unity;

namespace TrackoAPI.Controllers
{
    [RoutePrefix("api/ApiSecurity")]
    public class DeviceRegistrationController : ApiController
    {
        private readonly IGlobalStore _gStore;
        private readonly IUnityContainer _unity;
        public DeviceRegistrationController(IUnityContainer container, IGlobalStore globalStore)
        {
            _unity = container;
            _gStore = globalStore;
        }

        [AllowAnonymous]
        [Route("RegisterDevice"), HttpPost]
        public async Task<IHttpActionResult> RegisterDevice(vwApiDevice device)
        {
            try
            {
                var oc = Request.GetOwinContext();
                var email = string.Empty;
                var temp = Helper.HostedOnPremise ? _gStore.GetOrAddTenant(device.ClientId) : _gStore.GetOrAddTenant(device.ClientId, GetTenantFromDb);
                if (temp == null)
                {
                    return BadRequest("Unable to find Tenant Information");
                }

                oc.Set("as:tenantConnection", temp.ConnectionString);
                email = temp.EmailAddress;
                //using (var context = oc.Get<TenantDbContext>())
                //{
                //    var temp =
                //        await
                //            context.Tenants.Where(x => x.IsActive && x.ClientKey == device.ClientId)
                //                .Select(x => new
                //                {
                //                    x.ConnectionString,
                //                    x.EmailAddress
                //                }).FirstOrDefaultAsync();
                //    if (temp == null)
                //    {
                //        return StatusCode(HttpStatusCode.Forbidden);
                //    }
                //    oc.Set("as:tenantConnection", temp.ConnectionString);
                //    email = temp.EmailAddress;
                //}
                if (string.IsNullOrWhiteSpace(email))
                {
                    email = "support@gofleet.co.in";
                }
                using (AuthRepository repo = (AuthRepository)_unity.Resolve<IAuthRepository>())
                {
                    var isCreated = await repo.RegisterDeviceAsync(device, email, temp.Name, temp.PhoneNumber);
                    if (isCreated)
                    {
                        return Ok();
                    }
                }
                return StatusCode(HttpStatusCode.BadRequest);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        [Route("AuthorizeDevice({key},{otp},{clientId})"), HttpGet]
        public async Task<IHttpActionResult> VerifyDevice([FromUri]string key, [FromUri]string otp, [FromUri]string clientId)
        {
            if (!Helper.HostedOnPremise)
            {
                var oc = Request.GetOwinContext();
                using (var context = oc.Get<TenantDbContext>())
                {
                    var temp =
                        await
                            context.Tenants.Where(x => x.IsActive && x.ClientKey == clientId)
                                .Select(x => new
                                {
                                    x.ConnectionString
                                }).FirstOrDefaultAsync();
                    if (temp == null)
                    {
                        return StatusCode(HttpStatusCode.Forbidden);
                    }
                    oc.Set("as:tenantConnection", temp.ConnectionString);
                }
            }
            using (AuthRepository repo = (AuthRepository)_unity.Resolve<IAuthRepository>())
            {
                var isVerified = await repo.AuthorizeDevice(key, otp);
                if (isVerified)
                {
                    return Ok();
                }
            }

            return BadRequest("Invalid OTP");
        }
        private TenantViewModel GetTenantFromDb(string clientKey)
        {
            using (var dbcontext = new TenantDbContext())
            {
                var vm = dbcontext.Tenants.Where(x => x.ClientKey == clientKey).Select(x =>
                    new TenantViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        PostalAddress = x.PostalAddress,
                        EmailAddress = x.EmailAddress,
                        Apps = x.Apps.Select(y => new TenantAppViewModel
                        {
                            ApplicationId = y.ApplicationId,
                            IsActive = y.IsActive,
                            AppName = y.fk_Application.ApplicationName,
                            ApplicationType = y.fk_Application.ApplicationType,
                            UpdateUrl = y.UpdateUrl,
                            NoOfUsers = y.NoOfActiveUsers,
                            FormatUrl = y.FormatUrl,
                            SetupUrl = y.SetupUrl
                        }).ToList(),
                        ConnectionString = x.ConnectionString,
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
                        ConstCurTypeId = x.ConstCurTypeId
                    }).FirstOrDefault();
                return vm;
            }
        }
    }
}