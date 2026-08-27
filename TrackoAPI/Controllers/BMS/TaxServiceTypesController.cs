using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TaxServiceTypesController : ODataController
    //ODataController
    {
        private readonly ITaxServiceTypeService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public TaxServiceTypesController(IUnitOfWorkAsync unitOfWorkAsync, ITaxServiceTypeService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/TaxServiceTypes
        [HttpGet, EnableQuery]
        public IQueryable<TaxServiceType> Get() => _repo.Queryable();

        // GET: odata/TaxServiceTypes(5)
        [EnableQuery]
        public SingleResult<TaxServiceType> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/TaxServiceTypes(5)
        public async Task<IHttpActionResult> Put(long key, TaxServiceType objTaxServiceType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTaxServiceType.Id)
            {
                return BadRequest();
            }
            if (objTaxServiceType.IsReserved) {
                return BadRequest("Pre-defined data cannot be updated");
            }

            objTaxServiceType.ObjectState = ObjectState.Modified;
            _repo.Update(objTaxServiceType);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxServiceTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTaxServiceType);
        }

        // POST: odata/TaxServiceTypes
        public async Task<IHttpActionResult> Post(TaxServiceType objTaxServiceType)
        {
            objTaxServiceType.ObjectState = ObjectState.Added;
            _repo.Insert(objTaxServiceType);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TaxServiceTypeExists(objTaxServiceType.Code))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name & Code are unique");
                }
                throw;
            }
            return Created(objTaxServiceType);
        }
        //// PATCH: odata/TaxServiceTypes(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TaxServiceType> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TaxServiceType objTaxServiceType = await _repo.FindAsync(key);
            if (objTaxServiceType == null)
            {
                return NotFound();
            }
            objTaxServiceType.ObjectState = ObjectState.Modified;
            patch.Patch(objTaxServiceType);
            if (objTaxServiceType.IsReserved)
            {
                return BadRequest("Pre-defined data cannot be updated");
            }
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaxServiceTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTaxServiceType);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTaxServiceType = await _repo.FindAsync(key);
            if (objTaxServiceType == null)
            {
                return NotFound();
            }
            objTaxServiceType.ObjectState = ObjectState.Deleted;
            _repo.Delete(objTaxServiceType);
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

        private bool TaxServiceTypeExists(string code) => _repo.Query(e =>e.Code == code).Select().Any();
        private bool TaxServiceTypeExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}