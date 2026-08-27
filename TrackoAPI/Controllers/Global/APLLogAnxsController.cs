using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using System.Web.OData.Routing;
using TrackoApi.Models.Global;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class APLLogAnxsController : ODataController
    {
        private readonly IAPLLogAnxService _objAPLLogAnxService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        public APLLogAnxsController(IUnitOfWorkAsync unitOfWorkAsync, IAPLLogAnxService service)
        {
            _objAPLLogAnxService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/APLLogAnxs
        [HttpGet, EnableQuery]
        public IQueryable<APLLogAnx> Get()
        {
            return _objAPLLogAnxService.Queryable();
        }
        // GET: odata/APLLogAnxs(5)
        [EnableQuery]
        public SingleResult<APLLogAnx> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objAPLLogAnxService.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/APLLogAnxs(5)
        public async Task<IHttpActionResult> Put([FromODataUri] long key, APLLogAnx objAPLLogAnx)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objAPLLogAnx.Id)
            {
                return BadRequest();
            }
            objAPLLogAnx.ObjectState = ObjectState.Modified;
            _objAPLLogAnxService.Update(objAPLLogAnx);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLLogAnx);
        }
        // POST: odata/APLLogAnxs
        public async Task<IHttpActionResult> Post(APLLogAnx objAPLLogAnx)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            objAPLLogAnx.ObjectState = ObjectState.Added;
            _objAPLLogAnxService.Insert(objAPLLogAnx);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objAPLLogAnx);
        }
        //// PATCH: odata/APLLogAnxs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<APLLogAnx> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            APLLogAnx objAPLLogAnx = await _objAPLLogAnxService.FindAsync(key);
            if (objAPLLogAnx == null)
            {
                return NotFound();
            }
            objAPLLogAnx.ObjectState = ObjectState.Modified;
            patch.Patch(objAPLLogAnx);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLLogAnx);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objAPLLogAnx = await _objAPLLogAnxService.FindAsync(key);
            if (objAPLLogAnx == null)
            {
                return NotFound();
            }
            objAPLLogAnx.ObjectState = ObjectState.Deleted;
            _objAPLLogAnxService.Delete(objAPLLogAnx);
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
    }
}