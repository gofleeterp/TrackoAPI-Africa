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
    public class APLLogAnxLevelsController : ODataController
    {
        private readonly IAPLLogAnxLevelService _objAPLLogAnxLevelService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        public APLLogAnxLevelsController(IUnitOfWorkAsync unitOfWorkAsync, IAPLLogAnxLevelService service)
        {
            _objAPLLogAnxLevelService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/APLLogAnxLevels
        [HttpGet, EnableQuery]
        public IQueryable<APLLogAnxLevel> Get()
        {
            return _objAPLLogAnxLevelService.Queryable();
        }
        // GET: odata/APLLogAnxLevels(5)
        [EnableQuery]
        public SingleResult<APLLogAnxLevel> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objAPLLogAnxLevelService.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/APLLogAnxLevels(5)
        public async Task<IHttpActionResult> Put([FromODataUri] long key, APLLogAnxLevel objAPLLogAnxLevel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objAPLLogAnxLevel.Id)
            {
                return BadRequest();
            }
            objAPLLogAnxLevel.ObjectState = ObjectState.Modified;
            _objAPLLogAnxLevelService.Update(objAPLLogAnxLevel);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLLogAnxLevel);
        }
        // POST: odata/APLLogAnxLevels
        public async Task<IHttpActionResult> Post(APLLogAnxLevel objAPLLogAnxLevel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            objAPLLogAnxLevel.ObjectState = ObjectState.Added;
            _objAPLLogAnxLevelService.Insert(objAPLLogAnxLevel);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objAPLLogAnxLevel);
        }
        //// PATCH: odata/APLLogAnxLevels(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<APLLogAnxLevel> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            APLLogAnxLevel objAPLLogAnxLevel = await _objAPLLogAnxLevelService.FindAsync(key);
            if (objAPLLogAnxLevel == null)
            {
                return NotFound();
            }
            objAPLLogAnxLevel.ObjectState = ObjectState.Modified;
            patch.Patch(objAPLLogAnxLevel);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLLogAnxLevel);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objAPLLogAnxLevel = await _objAPLLogAnxLevelService.FindAsync(key);
            if (objAPLLogAnxLevel == null)
            {
                return NotFound();
            }
            objAPLLogAnxLevel.ObjectState = ObjectState.Deleted;
            _objAPLLogAnxLevelService.Delete(objAPLLogAnxLevel);
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