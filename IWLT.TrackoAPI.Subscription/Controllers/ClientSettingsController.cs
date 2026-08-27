using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IWLT.TrackoAPI.Subscription.Models;
using IWLT.TrackoAPI.Subscription.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace IWLT.TrackoAPI.Subscription.Controllers
{
    [Route("api/[controller]")]
    public class ClientSettingsController : ControllerBase
    {
        private readonly TenantDbContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        public ClientSettingsController(TenantDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        // GET: api/ApplicationMapping
        /// <summary>
        /// https://localhost:44372/api/ClientSettings/InitializeClient(1463,8643C14A-9F37-4364-A4A1-582C82BEA57C)
        /// </summary>
        /// <param name="accessCode"></param>
        /// <param name="applicationKey"></param>
        /// <returns></returns>
        [HttpGet("InitializeClient({accessCode},{applicationKey})"), ResponseCache(Duration = 200, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetClientInfo(int accessCode, string applicationKey)
        {
            try
            {
                var client = await
                    _context.TenantApplications.Where(x => x.fk_Tenant.AccessCode == accessCode)
                        .Select(x =>
                            new
                            {
                                TenantId = x.TenantId,
                                AppId = x.ApplicationId,
                                AppName = x.fk_Application.ApplicationName,
                                AppType = x.fk_Application.ApplicationType,
                                IsAppActive = x.IsActive,
                                IsActive = x.fk_Tenant.IsActive,
                                IsSingleUserMode = x.fk_Tenant.IsSingleUserMode,
                                LogType = x.fk_Tenant.LogType,
                                NoOfActiveUsers = x.NoOfActiveUsers,
                                ServerUrl = x.fk_Tenant.ServerUrl,
                                ClientKey = x.fk_Tenant.ClientKey,
                                ClientSecret = x.fk_Tenant.Secret,
                                TenantName = x.fk_Tenant.Name,
                                ShortName = x.fk_Tenant.ShortName,
                                UpdateUrl = x.UpdateUrl
                            })
                        .FirstOrDefaultAsync().ConfigureAwait(true);
                if (client == null || !client.IsActive)
                {
                    return Unauthorized();
                }

                if (!client.IsAppActive)
                {
                    return BadRequest("This Software has been deactivated by IWLT");
                }
                return Ok(client);
            }
            catch (Exception e)
            {
                return BadRequest(new JsonResult(e));
            }

        }
        [HttpGet, Route("Ip2Location({ip})")]
        public async Task<IActionResult> Ip2LocationAsync(string ip)
        {
            try
            {
                using (var client = _httpClientFactory.CreateClient())
                {
                    using (var result = await client.GetAsync($"http://api.ipinfodb.com/v3/ip-city/?key=8bd134f132fd0713be0880c6ada4c23a65b8d26b695f231971e0574314fb6afb&ip={ip}&format=json").ConfigureAwait(true))
                    {
                        if (!result.IsSuccessStatusCode) return StatusCode((int)result.StatusCode);

                        return Ok(await result.Content.ReadAsStringAsync().ConfigureAwait(true));
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }


        }

        [HttpGet, Route("ServerTime")]
        public IActionResult GetServerDateTime()
        {
            return Ok(DateTime.Now);
        }
    }
}
