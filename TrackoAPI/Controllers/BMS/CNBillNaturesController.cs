
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
    public class CNBillNaturesController : ODataController
    //ODataController
    {
        private readonly ICNBillNatureService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public CNBillNaturesController(IUnitOfWorkAsync unitOfWorkAsync, ICNBillNatureService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/CNBillNatures
        [HttpGet, EnableQuery]
        public IQueryable<CNBillNature> Get() => _repo.Queryable();

        // GET: odata/CNBillNatures(5)
        [EnableQuery]
        public SingleResult<CNBillNature> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/CNBillNatures(5)
        public async Task<IHttpActionResult> Put(long key, CNBillNature objCNBillNature)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objCNBillNature.Id)
            {
                return BadRequest();
            }
            objCNBillNature.ObjectState = ObjectState.Modified;
            _repo.Update(objCNBillNature);

            try
            {
              await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CNBillNatureExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objCNBillNature);
        }
        // POST: odata/CNBillNatures
        public async Task<IHttpActionResult> Post(CNBillNature objCNBillNature)
        {
            objCNBillNature.ObjectState = ObjectState.Added;
            _repo.Insert(objCNBillNature);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (CNBillNatureExists(objCNBillNature.Name))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name or Code should be unique");
                }
                throw;
            }
            return Created(objCNBillNature);
        }
        //// PATCH: odata/CNBillNatures(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNBillNature> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CNBillNature objCNBillNature = await _repo.FindAsync(key);
            if (objCNBillNature == null)
            {
                return NotFound();
            }
            objCNBillNature.ObjectState = ObjectState.Modified;
            patch.Patch(objCNBillNature);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CNBillNatureExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objCNBillNature);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objCNBillNature = await _repo.FindAsync(key);
            if (objCNBillNature == null)
            {
                return NotFound();
            }
            objCNBillNature.ObjectState = ObjectState.Deleted;
            _repo.Delete(objCNBillNature);
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

        private bool CNBillNatureExists(string name) => _repo.Query(e => e.Name == name).Select().Any();
        private bool CNBillNatureExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}