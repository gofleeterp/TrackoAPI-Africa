using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class OfficesController : ODataController
    //ODataController
    {
        private readonly IOfficeMasterService _objOfficeMasterService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public OfficesController(IUnitOfWorkAsync unitOfWorkAsync, IOfficeMasterService service)
        {
            _objOfficeMasterService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/OfficeMasters
        [HttpGet, EnableQuery]
        public IQueryable<OfficeMaster> Get()
        {
            return _objOfficeMasterService.Queryable();
        }
        // GET: odata/OfficeMasters(5)
        [EnableQuery]
        public SingleResult<OfficeMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objOfficeMasterService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/OfficeMasters(5)
        public async Task<IHttpActionResult> Put(long key, OfficeMaster objOfficeMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objOfficeMaster.Id)
            {
                return BadRequest();
            }
            objOfficeMaster.ObjectState = ObjectState.Modified;
            _objOfficeMasterService.Update(objOfficeMaster);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OfficeMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objOfficeMaster);
        }
        // POST: odata/OfficeMasters
        public async Task<IHttpActionResult> Post(OfficeMaster objOfficeMaster)
        {
            if (!Request.IsBatchRequest())
            {
                _unitOfWorkAsync.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            objOfficeMaster.ObjectState = ObjectState.Added;
            _objOfficeMasterService.Insert(objOfficeMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
                await _objOfficeMasterService.MapOfficeToDefaultClass(objOfficeMaster);
               

            }
            catch (DbUpdateException)
            {
                if (!Request.IsBatchRequest())
                {
                    _unitOfWorkAsync.Rollback();
                }
                if (OfficeMasterExists(objOfficeMaster.OfficeName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                _unitOfWorkAsync.Commit();
            }
            return Created(objOfficeMaster);
        }
        //// PATCH: odata/OfficeMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<OfficeMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            OfficeMaster objOfficeMaster = await _objOfficeMasterService.FindAsync(key);
            if (objOfficeMaster == null)
            {
                return NotFound();
            }
            objOfficeMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objOfficeMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OfficeMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objOfficeMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objOfficeMaster = await _objOfficeMasterService.FindAsync(key);
            if (objOfficeMaster == null)
            {
                return NotFound();
            }
            objOfficeMaster.ObjectState = ObjectState.Deleted;
            _objOfficeMasterService.Delete(objOfficeMaster);
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

        private bool OfficeMasterExists(string name)
        {
            return _objOfficeMasterService.Query(e => e.OfficeName == name).Select().Any();
        }
        private bool OfficeMasterExists(long key)
        {
            return _objOfficeMasterService.Query(e => e.Id == key).Select().Any();
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            OfficeMaster office = await _objOfficeMasterService.FindAsync(key);
            if (office == null)
            {
                return NotFound();
            }
            switch (navigationProperty)
            {
                case "fk_Address":
                    var navigationkey = Request.GetKeyFromUri<long>(link);
                    PostalAddress curAddress = await uow.RepositoryAsync<PostalAddress>().FindAsync(navigationkey);
                    if (curAddress == null)
                    {
                        return NotFound();
                    }
                    office.fk_Address = curAddress;
                    office.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);
                default:
                    return NotFound();
            }
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateLink([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var _uow = Request.GetContext();
            OfficeMaster office = await _objOfficeMasterService.FindAsync(key);
            if (office == null)
            {
                return NotFound();
            }
            long navigationkey = 0;
            switch (navigationProperty)
            {
                case "fk_Address":
                    navigationkey = Request.GetKeyValue<long>(link);
                    PostalAddress curAddress = await _uow.RepositoryAsync<PostalAddress>().FindAsync(navigationkey);
                    if (curAddress == null)
                    {
                        return NotFound();
                    }
                    office.fk_Address = curAddress;
                    office.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    return StatusCode(HttpStatusCode.NoContent);
                default:
                    return NotFound();
            }
        }
    }
}