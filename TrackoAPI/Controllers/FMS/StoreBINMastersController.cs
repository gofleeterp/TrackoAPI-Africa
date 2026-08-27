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
    public class StoreBINMastersController : ODataController
    //ODataController
    {
        private readonly IStoreBINMasterService _sc;

        public StoreBINMastersController(IStoreBINMasterService service)
        {
            _sc = service;
        }
        // GET: odata/StoreBINMasters
        [HttpGet, EnableQuery]
        public IQueryable<StoreBinMaster> Get()
        {
            return _sc.Queryable();
        }
        // GET: odata/StoreBINMasters(5)
        [EnableQuery]
        public SingleResult<StoreBinMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_sc.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/StoreBINMasters(5)
        public async Task<IHttpActionResult> Put(long key, StoreBinMaster entity)
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
        // POST: odata/StoreBINMasters
        public async Task<IHttpActionResult> Post(StoreBinMaster entity)
        {
            if (CheckExists(entity.StoreId,entity.RoomId, entity.BinName))
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
                if (CheckExists(entity.StoreId,entity.RoomId, entity.BinName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(entity);
        }
        //// PATCH: odata/StoreBINMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<StoreBinMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StoreBinMaster entity = await _sc.FindAsync(key);
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

        private bool CheckExists(long StoreId,long RoomId, string BINNo)
        {
            return _sc.Query(e => StoreId == e.StoreId && BINNo == e.BinName && e.RoomId==RoomId).Select().Any();
        }
        private bool CheckExists(long key)
        {
            return _sc.Query(e => e.Id == key).Select().Any();
        }
    }
}