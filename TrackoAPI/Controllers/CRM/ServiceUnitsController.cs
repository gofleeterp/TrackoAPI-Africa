using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.CRM;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ServiceUnitsController : ODataController
    //ODataController
    {
        private readonly IService<ServiceUnit> _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public ServiceUnitsController(IUnitOfWorkAsync unitOfWorkAsync, IService<ServiceUnit> service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/ServiceUnits
        [HttpGet, EnableQuery]
        public IQueryable<ServiceUnit> Get() => _repo.Queryable();
        // GET: odata/ServiceUnits(5)
        [EnableQuery]
        public SingleResult<ServiceUnit> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/ServiceUnits(5)
        public async Task<IHttpActionResult> Put(long key, ServiceUnit objServiceUnit)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objServiceUnit.Id)
            {
                return BadRequest();
            }
            objServiceUnit.ObjectState = ObjectState.Modified;
            _repo.Update(objServiceUnit);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceUnitExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objServiceUnit);
        }

        // POST: odata/ServiceUnits
        public async Task<IHttpActionResult> Post(ServiceUnit objServiceUnit)
        {
            objServiceUnit.ObjectState = ObjectState.Added;
            _repo.Insert(objServiceUnit);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ServiceUnitExists(objServiceUnit.UnitName,objServiceUnit.DataSourceId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name is already created");
                }
                throw;
            }
            return Created(objServiceUnit);
        }
        //// PATCH: odata/ServiceUnits(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ServiceUnit> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ServiceUnit objServiceUnit = await _repo.FindAsync(key);
            if (objServiceUnit == null)
            {
                return NotFound();
            }
            objServiceUnit.ObjectState = ObjectState.Modified;
            patch.Patch(objServiceUnit);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceUnitExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objServiceUnit);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objServiceUnit = await _repo.FindAsync(key);
            if (objServiceUnit == null)
            {
                return NotFound();
            }
            objServiceUnit.ObjectState = ObjectState.Deleted;
            _repo.Delete(objServiceUnit);
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

        private bool ServiceUnitExists(string name,long? datasourceid) => _repo.Query(e =>e.UnitName == name&&e.DataSourceId==datasourceid).Select().Any();
        private bool ServiceUnitExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}