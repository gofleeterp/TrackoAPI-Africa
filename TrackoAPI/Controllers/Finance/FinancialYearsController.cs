using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class FinancialYearsController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<FinancialYear> _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public FinancialYearsController(IUnitOfWorkAsync unitOfWorkAsync, IRepositoryAsync<FinancialYear> service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/FinancialYearBooks
        [HttpGet, EnableQuery]
        public IQueryable<FinancialYear> Get() => _repo.Queryable();

        // GET: odata/FinancialYearBooks(5)
        [EnableQuery]
        public SingleResult<FinancialYear> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/FinancialYearBooks(5)
        public async Task<IHttpActionResult> Put(long key, FinancialYear objFinancialYear)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objFinancialYear.Id)
            {
                return BadRequest();
            }
            objFinancialYear.ObjectState = ObjectState.Modified;
            _repo.Update(objFinancialYear);

            try
            {
              //  await _unitOfWorkAsync.SaveChangesAsync();
                
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!FinancialYearBookExists(key))
                //{
                //    return NotFound();
                //}
                //throw;
            }

            return Updated(objFinancialYear);
        }
        // POST: odata/FinancialYearBooks
        public async Task<IHttpActionResult> Post(FinancialYear objFinancialYear)
        {
            objFinancialYear.ObjectState = ObjectState.Added;
            _repo.Insert(objFinancialYear);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (FinancialYearExists(objFinancialYear))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objFinancialYear);
        }
        //// PATCH: odata/FinancialYearBooks(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<FinancialYear> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            FinancialYear objFinancialYear = await _repo.FindAsync(key);
            if (objFinancialYear == null)
            {
                return NotFound();
            }
            objFinancialYear.ObjectState = ObjectState.Modified;
            patch.Patch(objFinancialYear);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FinancialYearExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objFinancialYear);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objFinancialYearBook = await _repo.FindAsync(key);
            if (objFinancialYearBook == null)
            {
                return NotFound();
            }
            objFinancialYearBook.ObjectState = ObjectState.Deleted;
            _repo.Delete(objFinancialYearBook);
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

        private bool FinancialYearExists(FinancialYear fy) => _repo.Query(e => e.Abbreviation == fy.Abbreviation||e.Name==fy.Name).Select().Any();
        private bool FinancialYearExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}