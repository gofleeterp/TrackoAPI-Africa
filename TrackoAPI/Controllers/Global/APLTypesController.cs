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
    public class APLTypesController : ODataController
    {
        private readonly IAPLTypeService _objAPLTypeService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        public APLTypesController(IUnitOfWorkAsync unitOfWorkAsync, IAPLTypeService service)
        {
            _objAPLTypeService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/APLTypes
        [HttpGet, EnableQuery]
        public IQueryable<APLType> Get()
        {
            return _objAPLTypeService.Queryable();
        }
        // GET: odata/APLTypes(5)
        [EnableQuery]
        public SingleResult<APLType> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objAPLTypeService.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/APLTypes(5)
        public async Task<IHttpActionResult> Put([FromODataUri] long key, APLType objAPLType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objAPLType.Id)
            {
                return BadRequest();
            }
            objAPLType.ObjectState = ObjectState.Modified;
            _objAPLTypeService.Update(objAPLType);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLType);
        }
        // POST: odata/APLTypes
        public async Task<IHttpActionResult> Post(APLType objAPLType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            objAPLType.ObjectState = ObjectState.Added;
            _objAPLTypeService.Insert(objAPLType);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objAPLType);
        }
        //// PATCH: odata/APLTypes(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<APLType> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            APLType objAPLType = await _objAPLTypeService.FindAsync(key);
            if (objAPLType == null)
            {
                return NotFound();
            }
            objAPLType.ObjectState = ObjectState.Modified;
            patch.Patch(objAPLType);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLType);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objAPLType = await _objAPLTypeService.FindAsync(key);
            if (objAPLType == null)
            {
                return NotFound();
            }
            objAPLType.ObjectState = ObjectState.Deleted;
            _objAPLTypeService.Delete(objAPLType);
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