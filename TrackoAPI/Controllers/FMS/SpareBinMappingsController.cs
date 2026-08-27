using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;
using TrackoApi.Models.FMS.Repairs;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class SpareBinMappingsController : ODataController
    //ODataController
    {
        private readonly ISpareBinMappingService _sc;

        public SpareBinMappingsController(ISpareBinMappingService service)
        {
            _sc = service;
        }
        // GET: odata/SpareBinMappings
        [HttpGet, EnableQuery]
        public IQueryable<SpareBinMapping> Get()
        {
            return _sc.Queryable();
        }
        // GET: odata/SpareBinMappings(5)
        [EnableQuery]
        public SingleResult<SpareBinMapping> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_sc.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/SpareBinMappings(5)
        public async Task<IHttpActionResult> Put(long key, SpareBinMapping entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            entity.ObjectState = ObjectState.Modified;
            _sc.Update(entity);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CheckExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(entity);
        }
        // POST: odata/SpareBinMappings
        public async Task<IHttpActionResult> Post(SpareBinMapping entity)
        {
            
            entity.ObjectState = ObjectState.Added;
            _sc.Insert(entity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CheckExists(entity.SpareItemId,entity.StoreId, entity.BinId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Duplicate mapping.");
                }
                throw;
            }
            return Created(entity);
        }
        //// PATCH: odata/SpareBinMappings(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<SpareBinMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SpareBinMapping entity = await _sc.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Modified;
            patch.Patch(entity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CheckExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(entity);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var entity = await _sc.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Deleted;
            _sc.Delete(entity);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing&&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CheckExists(long SpreItemId, long StoreId,long? BinId)
        {
            return _sc.Query(e => SpreItemId==e.SpareItemId && StoreId==e.StoreId && e.BinId == BinId).Select().Any();
        }
        private bool CheckExists(long key)
        {
            return _sc.Query(e => e.Id == key).Select().Any();
        }
    }
}