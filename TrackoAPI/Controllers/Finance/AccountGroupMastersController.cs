using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class AccountGroupsController : ODataController
    //ODataController
    {
        private readonly IAccountGroupService _objAccountGroupService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public AccountGroupsController(IUnitOfWorkAsync unitOfWorkAsync, IAccountGroupService service)
        {
            _objAccountGroupService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/AccountGroupMasters
        [HttpGet, EnableQuery]
        public IQueryable<AccountGroup> Get()
        {
            return _objAccountGroupService.Queryable();
        }
        // GET: odata/AccountGroupMasters(5)
        [EnableQuery]
        public SingleResult<AccountGroup> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objAccountGroupService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/AccountGroupMasters(5)
        public async Task<IHttpActionResult> Put(long key, AccountGroup objAccountGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != objAccountGroup.Id)
            {
                return BadRequest();
            }
            objAccountGroup.ObjectState = ObjectState.Modified;
            _objAccountGroupService.Update(objAccountGroup);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountGroupMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            return Updated(objAccountGroup);
        }
        // POST: odata/AccountGroupMasters
        public async Task<IHttpActionResult> Post(AccountGroup objAccountGroup)
        {
            objAccountGroup.ObjectState = ObjectState.Added;
            _objAccountGroupService.Insert(objAccountGroup);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (AccountGroupMasterExists(objAccountGroup.GroupName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objAccountGroup);
        }
        //// PATCH: odata/AccountGroupMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<AccountGroup> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            AccountGroup objAccountGroup = await _objAccountGroupService.FindAsync(key);
            if (objAccountGroup == null)
            {
                return NotFound();
            }
            objAccountGroup.ObjectState = ObjectState.Modified;
            patch.Patch(objAccountGroup);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AccountGroupMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            return Updated(objAccountGroup);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objAccountGroupMaster = await _objAccountGroupService.FindAsync(key);
            if (objAccountGroupMaster == null)
            {
                return NotFound();
            }
            objAccountGroupMaster.ObjectState = ObjectState.Deleted;
            _objAccountGroupService.Delete(objAccountGroupMaster);
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

        private bool AccountGroupMasterExists(string groupName)
        {
            return _objAccountGroupService.Query(e => e.GroupName == groupName).Select().Any();
        }
        private bool AccountGroupMasterExists(long key)
        {
            return _objAccountGroupService.Query(e => e.Id == key).Select().Any();
        }
    }
}