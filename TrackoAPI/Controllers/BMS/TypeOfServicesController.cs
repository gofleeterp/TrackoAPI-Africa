
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TaxTypeServicesController : ODataController
    //ODataController
    {
        private readonly ITypeOfServiceService _repo;

        public TaxTypeServicesController(ITypeOfServiceService service)
        {
            _repo = service;
        }
        // GET: odata/TypeOfServices
        [HttpGet, EnableQuery]
        public IQueryable<TaxTypeService> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/TypeOfServices(5)
        [EnableQuery]
        public SingleResult<TaxTypeService> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/TypeOfServices(5)
        public async Task<IHttpActionResult> Put(long key, TaxTypeService objTaxTypeService)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTaxTypeService.Id)
            {
                return BadRequest();
            }
            objTaxTypeService.ObjectState = ObjectState.Modified;
            _repo.Update(objTaxTypeService);

            try
            {
              //  await Request.GetContext().SaveChangesAsync();
                await Request.GetContext().SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeOfServiceExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTaxTypeService);
        }
        // POST: odata/TypeOfServices
        public async Task<IHttpActionResult> Post(TaxTypeService objTaxTypeService)
        {
            objTaxTypeService.ObjectState = ObjectState.Added;
            _repo.Insert(objTaxTypeService);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TypeOfServiceExists(objTaxTypeService.Name, objTaxTypeService.Code))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name or Code should be unique");
                }
                throw;
            }
            return Created(objTaxTypeService);
        }
        //// PATCH: odata/TypeOfServices(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TaxTypeService> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TaxTypeService objTaxTypeService = await _repo.FindAsync(key);
            if (objTaxTypeService == null)
            {
                return NotFound();
            }
            objTaxTypeService.ObjectState = ObjectState.Modified;
            patch.Patch(objTaxTypeService);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeOfServiceExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTaxTypeService);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTypeOfService = await _repo.FindAsync(key);
            if (objTypeOfService == null)
            {
                return NotFound();
            }
            objTypeOfService.ObjectState = ObjectState.Deleted;
            _repo.Delete(objTypeOfService);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TypeOfServiceExists(string name,string code) => _repo.Query(e => (e.Name == name)|| (e.Code== code)).Select().Any();
        private bool TypeOfServiceExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}