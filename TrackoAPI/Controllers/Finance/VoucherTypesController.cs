using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    public class VoucherTypesController:ODataController
    {
        private readonly IVoucherTypeService _repo;

        public VoucherTypesController(IVoucherTypeService voucherTypeService)
        {
            _repo = voucherTypeService;
        }
        // GET: odata/VoucherTypes
        [HttpGet, EnableQuery]
        public IQueryable<VoucherType> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/VoucherTypes(5)/GetDefaultMappings
        [HttpGet]
        public IQueryable<ViewField> GetDefaultMappings(long key)
        {
            return Request.GetContext().RepositoryAsync<ViewField>()
                .Queryable()
                .Where(x => x.ViewId == key&&x.ShowInVTG);
        }
        // GET: odata/VoucherTypes(5)
        [EnableQuery]
        public SingleResult<VoucherType> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        [AcceptVerbs("PUT","REPLACE")]
        public async Task<IHttpActionResult> Put(long key, VoucherType entity)
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
            _repo.Update(entity);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(entity);
        }
        // POST: odata/Vouchers
        public async Task<IHttpActionResult> Post(VoucherType entity)
        {
            entity.ObjectState = ObjectState.Added;
            _repo.Insert(entity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(entity);
        }
        //// PATCH: odata/Vouchers(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VoucherType> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VoucherType entity = await _repo.FindAsync(key);
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
                throw;
            }

            return Updated(entity);
        }
        // DELETE: odata/Vouchers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var entity = await _repo.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }

            if (entity.IsReserved)
            {
                return BadRequest("You cannot delete reserved Voucher Types");
            }
            entity.ObjectState = ObjectState.Deleted;
            _repo.Delete(entity);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
            }
            base.Dispose(disposing);
        }
    }
}