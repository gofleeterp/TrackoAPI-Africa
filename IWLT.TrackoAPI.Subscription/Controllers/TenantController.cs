using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IWLT.TrackoAPI.Subscription.Models;
using IWLT.TrackoAPI.Subscription.ViewModels;
using Tenant.Models;

namespace IWLT.TrackoAPI.Subscription.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly TenantDbContext _context;

        public TenantController(TenantDbContext context)
        {
            _context = context;
        }

        // GET: api/ApplicationMapping
        [HttpGet("{accessCode}"), ResponseCache(Duration = 200, Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetClientInfo(int accessCode)
        {
            try
            {
                var list = await _context.TenantApplications.Where(x=>x.fk_Tenant.AccessCode==accessCode&&x.fk_Tenant.IsActive).Select(x=>new ClientAppInfoViewModel
                {
                    AccessCode = x.fk_Tenant.AccessCode,
                    TenantId = x.TenantId,
                    AppId = x.ApplicationId,
                    AppName = x.fk_Application.ApplicationName,
                    AppType = x.fk_Application.ApplicationType,
                    IsActive = x.IsActive,
                    IsSingleUserMode = x.fk_Tenant.IsSingleUserMode,
                    LogType = x.fk_Tenant.LogType,
                    NoOfActiveUsers = x.NoOfActiveUsers,
                    ServerUrl = x.fk_Tenant.ServerUrl,
                    TenantKey = x.fk_Tenant.ClientKey,
                    TenantName = x.fk_Tenant.Name,
                    TenantShortName = x.fk_Tenant.ShortName,
                    UpdateUrl = x.UpdateUrl
                }).ToListAsync().ConfigureAwait(true);
                return Ok(list);
            }
            catch (Exception e)
            {
                return BadRequest(new JsonResult(e));
            }
         
        }
        // GET: api/ApplicationMapping
        [HttpGet("{accessCode}/{applicationKey}"), ResponseCache(Duration = 200,Location = ResponseCacheLocation.Client)]
        public async Task<IActionResult> GetClientInfo(int accessCode,string applicationKey)
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
                                IsActive=x.fk_Tenant.IsActive,
                                IsSingleUserMode = x.fk_Tenant.IsSingleUserMode,
                                LogType = x.fk_Tenant.LogType,
                                NoOfActiveUsers = x.NoOfActiveUsers,
                                ServerUrl = x.fk_Tenant.ServerUrl,
                                ClientKey = x.fk_Tenant.ClientKey,
                                ClientSecret=x.fk_Tenant.Secret,
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
        //// GET: api/ApplicationMapping/5
        //[HttpGet("{id}")]
        //public async Task<IActionResult> Get([FromRoute] long id)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    var tenantApplicationMapping = await _context.TenantApplications.FindAsync(id);

        //    if (tenantApplicationMapping == null)
        //    {
        //        return NotFound();
        //    }

        //    return Ok(tenantApplicationMapping);
        //}

        // PUT: api/ApplicationMapping/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put([FromRoute] long id, [FromBody] TenantApplicationMapping tenantApplicationMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tenantApplicationMapping.Id)
            {
                return BadRequest();
            }

            _context.Entry(tenantApplicationMapping).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TenantApplicationMappingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ApplicationMapping
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TenantApplicationMapping tenantApplicationMapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.TenantApplications.Add(tenantApplicationMapping);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = tenantApplicationMapping.Id }, tenantApplicationMapping);
        }

        // DELETE: api/ApplicationMapping/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantApplicationMapping = await _context.TenantApplications.FindAsync(id);
            if (tenantApplicationMapping == null)
            {
                return NotFound();
            }

            _context.TenantApplications.Remove(tenantApplicationMapping);
            await _context.SaveChangesAsync();

            return Ok(tenantApplicationMapping);
        }

        private bool TenantApplicationMappingExists(long id)
        {
            return _context.TenantApplications.Any(e => e.Id == id);
        }

        public IActionResult Get(long id)
        {
            return Ok();
        }
    }
}