using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Service.Finance;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers.Finance
{
    [AuthorizeEx]
    public class GSTConfigurationController : ODataController
    {
        private readonly IGSTConfigurationService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public GSTConfigurationController(IUnitOfWorkAsync unitOfWorkAsync, IGSTConfigurationService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }

        // GET: odata/GSTConfiguration
        [HttpGet, EnableQuery]
        public IQueryable<GSTConfiguration> Get() => _repo.Queryable();

        // GET: odata/GSTConfiguration(5)
        [EnableQuery]
        public SingleResult<GSTConfiguration> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));

        // PUT: odata/GSTConfiguration(5)
        public async Task<IHttpActionResult> Put(long key, GSTConfiguration objgstconfig)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objgstconfig.Id)
            {
                return BadRequest();
            }
            objgstconfig.ObjectState = ObjectState.Modified;
            _repo.Update(objgstconfig);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
               
            }

            return Updated(objgstconfig);
        }

        // POST: odata/GSTConfiguration
        public async Task<IHttpActionResult> Post(GSTConfiguration objgstconfig)
        {
            objgstconfig.ObjectState = ObjectState.Added;
            _repo.Insert(objgstconfig);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
            }
            return Created(objgstconfig);
        }

        // PATCH: odata/GSTConfiguration(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<GSTConfiguration> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GSTConfiguration objGstConfig = await _repo.FindAsync(key);
            if (objGstConfig == null)
            {
                return NotFound();
            }
            objGstConfig.ObjectState = ObjectState.Modified;
            patch.Patch(objGstConfig);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
            }

            return Updated(objGstConfig);
        }
        
        // DELETE: odata/GSTConfiguration(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objContactBook = await _repo.FindAsync(key);
            if (objContactBook == null)
            {
                return NotFound();
            }
            objContactBook.ObjectState = ObjectState.Deleted;
            _repo.Delete(objContactBook);
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