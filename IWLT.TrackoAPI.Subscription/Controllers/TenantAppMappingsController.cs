using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IWLT.TrackoAPI.Subscription.Models;
using Tenant.Models;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Formatter;

namespace IWLT.TrackoAPI.Subscription.Controllers
{
    [Route("AppMappings")]
    public class TenantAppMappingsController : ODataController
    {
        private readonly TenantDbContext _context;

        public TenantAppMappingsController(TenantDbContext context)
        {
            _context = context;
        }

        // GET: api/TenantApplicationMappings
        [EnableQuery(PageSize = 20, AllowedQueryOptions = AllowedQueryOptions.All)]
        public async Task<ActionResult<IEnumerable<TenantApplicationMapping>>> Get()
        {
            return await _context.TenantApplications.ToListAsync();
        }

        // GET: api/TenantApplicationMappings/5
        [HttpGet("({key})")]
        [EnableQuery(PageSize = 20, AllowedQueryOptions = AllowedQueryOptions.All)]
        public async Task<ActionResult<TenantApplicationMapping>> Get([FromODataUri] int key)
        {
            var tenantApplicationMapping = await _context.TenantApplications.FindAsync(key);

            if (tenantApplicationMapping == null)
            {
                return NotFound();
            }

            return tenantApplicationMapping;
        }

        // PUT: api/TenantApplicationMappings/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, TenantApplicationMapping tenantApplicationMapping)
        {
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

        // POST: api/TenantApplicationMappings
        [HttpPost]
        public async Task<ActionResult<TenantApplicationMapping>> Post(TenantApplicationMapping tenantApplicationMapping)
        {
            _context.TenantApplications.Add(tenantApplicationMapping);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = tenantApplicationMapping.Id }, tenantApplicationMapping);
        }

        // DELETE: api/TenantApplicationMappings/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<TenantApplicationMapping>> Delete(long id)
        {
            var tenantApplicationMapping = await _context.TenantApplications.FindAsync(id);
            if (tenantApplicationMapping == null)
            {
                return NotFound();
            }

            _context.TenantApplications.Remove(tenantApplicationMapping);
            await _context.SaveChangesAsync();

            return tenantApplicationMapping;
        }

        private bool TenantApplicationMappingExists(long id)
        {
            return _context.TenantApplications.Any(e => e.Id == id);
        }
    }
}
