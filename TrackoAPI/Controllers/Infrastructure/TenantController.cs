using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.ModelBinding;
using Tenant.Models;
using TrackoApi.Core;
using TrackoAPI.Infrastructure;
using TrackoAPI.ViewModels;

namespace TrackoAPI.Controllers
{
    [RoutePrefix("Tenant")]
    public class TenantController : ApiController
    {
        private readonly TenantRepository _repo;
        

        public TenantController(ITenantRepository tenant)
        {
            try
            {
                var clientKey = HttpContext.Current.Request.Headers.Get("client_key");
                var supperKey = HttpContext.Current.Request.Headers.Get("godkey");
                _repo = (TenantRepository) tenant;
                _repo.Intialize(clientKey,supperKey);
                
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message,ex);
            }
            
        }
        [Route("CreateTenant"),ResponseType(typeof(List<TenantResult>)),HttpPost]
        public async Task<IHttpActionResult> Register(RegisterTenant tenant)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _repo.RegisterTenant(tenant);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("DeactivateTenant"), ResponseType(typeof(bool)),HttpGet]
        public IHttpActionResult DeactivateTenant()
        {
            try
            {
                var result=_repo.ActivateDeactiveTenant(false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("ActivateTenant"), ResponseType(typeof (bool)), HttpGet]
        public IHttpActionResult ActivateTenant()
        {
            try
            {
                var result = _repo.ActivateDeactiveTenant(true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }
        [Route("AssignApp"),ResponseType(typeof(TenantResult)),HttpPost]
        public async Task<IHttpActionResult> RegisterApp(ApiApplication application)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _repo.AssignApplication(application);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [Route("ActivateApp"), ResponseType(typeof(bool)), HttpGet]
        public IHttpActionResult ActivateApp(string applicationName)
        {
            try
            {
                var result = _repo.ActivateDeactiveApp(applicationName,true);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("DeactivateApp"), ResponseType(typeof(bool)), HttpGet]
        public IHttpActionResult DeactivateApp(string applicationName)
        {
            try
            {
                var result = _repo.ActivateDeactiveApp(applicationName, false);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Route("CreateApp"), ResponseType(typeof(string)), HttpPost]
        public async Task<IHttpActionResult> CreateApp(Application app)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _repo.CreateNewApp(app);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _repo.Dispose();
            }
            base.Dispose(disposing);
        }
        [Route("GetTenantInfo"), ResponseType(typeof(TenantViewModel)), HttpPost]
        public async Task<TenantViewModel> GetTenantInfo()
        {
            var tenant_clientKey = HttpContext.Current.Request.Headers.Get("tenant_client_key");
            var tenant_id = HttpContext.Current.Request.Headers.Get("tenant_id");

            return await _repo.GetTenantInfoAsync(tenant_clientKey, tenant_id);
        }
        [Route("GetTokenInfo"), ResponseType(typeof(TPTokenViewModel)), HttpPost]
        public async Task<TPTokenViewModel> GetTokenInfo()
        {
            var token = HttpContext.Current.Request.Headers.Get("token");
            return await _repo.GetTokenInfoAsync(token);
        }
        [Route("GetJsonLogs"), ResponseType(typeof(List<JsonGLLog>)), HttpPost]
        public async Task<List<JsonGLLog>> GetJsonLogsAsync()
        {
            var prefix = HttpContext.Current.Request.Headers.Get("prefix");
            var jsonKey = HttpContext.Current.Request.Headers.Get("jsonkey");
            return await _repo.GetJsonLogsAsync(prefix,jsonKey);
        }
        [Route("PostJsonLogs"), HttpPost]
        public async Task<IHttpActionResult> PostJsonLogsAsync([FromBody]List<JsonGLLog> logs)
        {
            if (logs.Any(x => string.IsNullOrWhiteSpace(x.JsonKey) || string.IsNullOrWhiteSpace(x.KeyPrefix)))
            {
                return BadRequest("JsonKey and KeyPrefix, both are required values for each log record. please verify each log record are having these values.");
            }
             await _repo.PostJsonLogsAsync(logs);
             return Ok();
        }
    }
}
