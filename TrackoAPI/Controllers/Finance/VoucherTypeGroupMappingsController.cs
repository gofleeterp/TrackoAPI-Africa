using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    public class VoucherTypeGroupMappingsController : ODataController
    {
        private readonly IVoucherTypeGroupMappingService _repo;

        public VoucherTypeGroupMappingsController(IVoucherTypeGroupMappingService voucherTypeService)
        {
            _repo = voucherTypeService;
        }
        //GET: odata/VoucherTypeGroupMappings(5)/Include
        [EnableQuery,HttpGet]
        public IQueryable<Ledger> GetInclude([FromODataUri]long key)
        {
            var includes = _repo.Queryable().Where(x => x.Id == key).Select(x => x.Include).FirstOrDefault();
            var query =
                Request.GetContext().RepositoryAsync<Ledger>()
                    .SelectQuery($"SELECT [Id],[AccountAbbr] AS Alias,[AccountName] FROM[dbo].[mLedger] WHERE Id in ({includes})")
                    .AsQueryable();
            return query;
        }
            //public IQueryable<Ledger>
        //    GET: odata/VoucherTypeGroupMappings/GetIncluded
        // GET: odata/SearchLedgerByVoucherType
        [EnableQuery,HttpGet,ODataRoute("SearchLedgerByVoucherType(voucherTypeId={voucherTypeId},fieldId={fieldId},viewId={viewId})")]
        public IQueryable<Ledger> SearchLedgerByVoucherType([FromODataUri]long? voucherTypeId, [FromODataUri]long fieldId, [FromODataUri]long? viewId, ODataQueryOptions<Ledger> query)
        {
            var data = _repo.GetLedgersByVoucherTypeId(voucherTypeId, fieldId, viewId);
            //query.ApplyTo(data);
            return data ?? new Queue<Ledger>().AsQueryable();
        }
        // GET: odata/VoucherTypeGroupMappings
        [HttpGet, EnableQuery]
        public IQueryable<VoucherTypeGroupMapping> Get() => _repo.Queryable();

        // GET: odata/VoucherTypeGroupMappings(5)
        [EnableQuery]
        public SingleResult<VoucherTypeGroupMapping> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/VoucherTypeGroupMappings(5)
        public async Task<IHttpActionResult> Put(long key, VoucherTypeGroupMapping entity)
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
            _repo.VerifyExistanceOfMapping(entity);
            _repo.Update(entity);

            await Request.GetContext().SaveChangesAsync();
            return Updated(entity);
        }
        // POST: odata/VoucherTypeGroupMappings
        public async Task<IHttpActionResult> Post(VoucherTypeGroupMapping entity)
        {
            entity.ObjectState = ObjectState.Added;
            _repo.Insert(entity);
            await Request.GetContext().SaveChangesAsync();
            return Created(entity);
        }
        //// PATCH: odata/VoucherTypeGroupMappings(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VoucherTypeGroupMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var entity = await _repo.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Modified;
            patch.Patch(entity);
            _repo.VerifyExistanceOfMapping(entity);
            await Request.GetContext().SaveChangesAsync();
            return Updated(entity);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var entity = await _repo.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
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