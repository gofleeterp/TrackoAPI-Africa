
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TaxRateMastersController : ODataController
    //ODataController
    {
        private readonly ITaxRateMasterService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public TaxRateMastersController(IUnitOfWorkAsync unitOfWorkAsync, ITaxRateMasterService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/TaxRateMasters
        [HttpGet, EnableQuery]
        public IQueryable<TaxRateMaster> Get() => _repo.Queryable();

        // GET: odata/TaxRateMasters(5)
        [EnableQuery]
        public SingleResult<TaxRateMaster> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/TaxRateMasters(5)
        public async Task<IHttpActionResult> Put(long key, TaxRateMaster objTaxRateMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTaxRateMaster.Id)
            {
                return BadRequest();
            }
            objTaxRateMaster.ObjectState = ObjectState.Modified;
            _repo.Update(objTaxRateMaster);

            try
            {
             
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxRateMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTaxRateMaster);
        }
        // POST: odata/TaxRateMasters
        public async Task<IHttpActionResult> Post(TaxRateMaster objTaxRateMaster)
        {
            objTaxRateMaster.ObjectState = ObjectState.Added;
            _repo.Insert(objTaxRateMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                //if (TaxRateMasterExists(objTaxRateMaster.Name, objTaxRateMaster.Code))
                //{
                //    throw new BusinessException(ErrorCode.GLB104, "Name or Code should be unique");
                //}
                throw;
            }
            return Created(objTaxRateMaster);
        }
        //// PATCH: odata/TaxRateMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TaxRateMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TaxRateMaster objTaxRateMaster = await _repo.FindAsync(key);
            if (objTaxRateMaster == null)
            {
                return NotFound();
            }
            objTaxRateMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objTaxRateMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxRateMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTaxRateMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTaxRateMaster = await _repo.FindAsync(key);
            if (objTaxRateMaster == null)
            {
                return NotFound();
            }
            objTaxRateMaster.ObjectState = ObjectState.Deleted;
            _repo.Delete(objTaxRateMaster);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

       // private bool TaxRateMasterExists(string name,string code) => _repo.Query(e => (e.Name == name)|| (e.Code== code)).Select().Any();
        private bool TaxRateMasterExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}