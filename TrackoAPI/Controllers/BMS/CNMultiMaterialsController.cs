using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service.BMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNMultiMaterialsController : ODataController
    //ODataController
    {
        private readonly ICNMultiMaterialService _repo;

        public CNMultiMaterialsController(ICNMultiMaterialService service)
        {
            _repo = service;
        }
        // GET: odata/CNMultiMaterials
        [HttpGet, EnableQuery]
        public IQueryable<CNMultiMaterial> Get() => _repo.Queryable();

        // GET: odata/CNMultiMaterials(5)
        [EnableQuery]
        public SingleResult<CNMultiMaterial> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/CNMultiMaterials(5)
        public async Task<IHttpActionResult> Put(long key, CNMultiMaterial objCNMultiMaterial)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objCNMultiMaterial.Id)
            {
                return BadRequest();
            }
            objCNMultiMaterial.ObjectState = ObjectState.Modified;
            _repo.Update(objCNMultiMaterial);

            try
            {
                //  await _unitOfWorkAsync.SaveChangesAsync();
                await Request.GetContext().SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objCNMultiMaterial);
        }
        // POST: odata/CNMultiMaterials
        public async Task<IHttpActionResult> Post(CNMultiMaterial objMaterialMaster)
        {
            objMaterialMaster.ObjectState = ObjectState.Added;
            _repo.Insert(objMaterialMaster);
            await Request.GetContext().SaveChangesAsync();
            return Created(objMaterialMaster);
        }
        //// PATCH: odata/MaterialMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNMultiMaterial> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CNMultiMaterial objMaterialMaster = await _repo.FindAsync(key);
            if (objMaterialMaster == null)
            {
                return NotFound();
            }
            objMaterialMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objMaterialMaster);
            try
            {
                _repo.Update(objMaterialMaster);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objMaterialMaster = await _repo.FindAsync(key);
            if (objMaterialMaster == null)
            {
                return NotFound();
            }
            var stockRepo = Request.GetContext()
                .RepositoryAsync<CNStockLog>();
            
            if (
                await
                    stockRepo.Queryable()
                        .AnyAsync(
                            x =>
                                (x.LogTypeId == 1423 || x.LogTypeId == 1451 || x.LogTypeId == 1472) &&
                                x.CNId == objMaterialMaster.CnId))
            {
                return BadRequest("Shipped Consignment are not allowed to modify.");
            }
            var stockLog = await stockRepo.Queryable().Include(x => x.StockMMLogs).FirstOrDefaultAsync(x=>x.CNId== objMaterialMaster.CnId&&x.LogTypeId== 1422&&x.RefStockId==null);
            stockLog.ObjectState=ObjectState.Modified;
           var stockMm= stockLog.StockMMLogs.FirstOrDefault(x => x.CNMMId == objMaterialMaster.Id);
            if(stockMm!=null)stockMm.ObjectState =ObjectState.Deleted;
            stockLog.InQty = stockLog.StockMMLogs.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.InQty);
            await Request.GetContext().SaveChangesAsync();
            objMaterialMaster.ObjectState = ObjectState.Deleted;
            _repo.Delete(objMaterialMaster);
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
        
        private bool InvoiceExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}