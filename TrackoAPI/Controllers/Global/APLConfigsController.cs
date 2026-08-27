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
using System.Data.Entity;
using Hangfire;
using System.Data.SqlClient;
using TrackoApi.Service.Global;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class APLConfigsController : ODataController
    {
        private readonly IAPLConfigService _objAPLConfigService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        public APLConfigsController(IUnitOfWorkAsync unitOfWorkAsync, IAPLConfigService service)
        {
            _objAPLConfigService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/APLConfigs
        [HttpGet, EnableQuery]
        public IQueryable<APLConfig> Get()
        {
            return _objAPLConfigService.Queryable();
        }
        // GET: odata/APLConfigs(5)
        [EnableQuery]
        public SingleResult<APLConfig> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objAPLConfigService.Queryable().Where(t => t.Id == key));
        }        
        
        // PUT: odata/APLConfigs(5)
        public async Task<IHttpActionResult> Put([FromODataUri] long key, APLConfig objAPLConfig)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return BadRequest("Update not allowed");
            if (key != objAPLConfig.Id)
            {
                return BadRequest();
            }
            objAPLConfig.ObjectState = ObjectState.Modified;
            _objAPLConfigService.Update(objAPLConfig);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLConfig);
        }
        // POST: odata/APLConfigs
        public async Task<IHttpActionResult> Post(APLConfig objAPLConfig)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (await _objAPLConfigService.Queryable().AnyAsync(x => x.ViewId == objAPLConfig.ViewId && x.IsItemLevelAPL != objAPLConfig.IsItemLevelAPL))
            {
                return BadRequest("[Item Level APL] should be same for one kind of Transaction");
            }

            objAPLConfig.ObjectState = ObjectState.Added;
            _objAPLConfigService.Insert(objAPLConfig);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateException)
            {
                throw;
            }
            try
            {
                BackgroundJob.Enqueue<IHangfireJobProcessor>(x => x.SyncAPLConfigInAPLAnnexureLevel(objAPLConfig.ViewId, Helper.LoggedInTenantId, Helper.SessionId(), 0, null));
            }
            catch (SqlException e)
            {
                return BadRequest(e.Message);
            }
            return Created(objAPLConfig);
        }
        //// PATCH: odata/APLConfigs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<APLConfig> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return BadRequest("Update not allowed");

            APLConfig objAPLConfig = await _objAPLConfigService.FindAsync(key);
            if (objAPLConfig == null)
            {
                return NotFound();
            }
            objAPLConfig.ObjectState = ObjectState.Modified;
            patch.Patch(objAPLConfig);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objAPLConfig);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            //var objAPLConfig = await _objAPLConfigService.FindAsync(key);
            //if (objAPLConfig == null)
            //{
            //    return NotFound();
            //}
            //objAPLConfig.ObjectState = ObjectState.Deleted;
            //_objAPLConfigService.Delete(objAPLConfig);
            //await _unitOfWorkAsync.SaveChangesAsync();
            //return StatusCode(HttpStatusCode.NoContent);

            var obj = await _objAPLConfigService.FindAsync(key);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != obj.Id)
            {
                return BadRequest();
            }
            obj.IsActive = false;
            obj.ObjectState = ObjectState.Modified;
            _objAPLConfigService.Update(obj);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            try
            {
                BackgroundJob.Enqueue<IHangfireJobProcessor>(x => x.SyncAPLConfigInAPLAnnexureLevel(obj.ViewId, Helper.LoggedInTenantId, Helper.SessionId(), 0, null));
            }
            catch (SqlException e)
            {
                return BadRequest(e.Message);
            }
            return Updated(obj);
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