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
    public class SpareInventoryLevelsController : ODataController
    //ODataController
    {
        private readonly ISpareInventoryLevelService _sc;

        public SpareInventoryLevelsController(ISpareInventoryLevelService service)
        {
            _sc = service;
        }
        // GET: odata/SpareInventoryLevels
        [HttpGet, EnableQuery]
        public IQueryable<SpareInventoryLevel> Get()
        {
            return _sc.Queryable();
        }
        // GET: odata/SpareInventoryLevels(5)
        [EnableQuery]
        public SingleResult<SpareInventoryLevel> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_sc.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/SpareInventoryLevels(5)
        public async Task<IHttpActionResult> Put(long key, SpareInventoryLevel entity)
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
        // POST: odata/SpareInventoryLevels
        public async Task<IHttpActionResult> Post(SpareInventoryLevel entity)
        {
            if (CheckExists(entity.SpareItemId, entity.StoreId,entity.MakeId))
            {
                throw new BusinessException(ErrorCode.GLB104,"Duplicate Record.");
            }
            entity.ObjectState = ObjectState.Added;
            _sc.Insert(entity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CheckExists(entity.SpareItemId,entity.StoreId, entity.MakeId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(entity);
        }
        //// PATCH: odata/SpareInventoryLevels(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<SpareInventoryLevel> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SpareInventoryLevel entity = await _sc.FindAsync(key);
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

        private bool CheckExists(long SpreItemId, long StoreId,long? MakeId)
        {
            return _sc.Query(e => SpreItemId==e.SpareItemId && StoreId==e.StoreId && (MakeId>0 && e.MakeId==MakeId)).Select().Any();
        }
        private bool CheckExists(long key)
        {
            return _sc.Query(e => e.Id == key).Select().Any();
        }
    }
}