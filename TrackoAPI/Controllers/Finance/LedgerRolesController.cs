using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class LedgerRolesController : ODataController
    //ODataController
    {
        private readonly ILedgerRoleService _ledgerRoleService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly IRepositoryAsync<Ledger> _ledgerRepo;

        public LedgerRolesController(IUnitOfWorkAsync unitOfWorkAsync, ILedgerRoleService service,IRepositoryAsync<Ledger> ledgerRepo)
        {
            _ledgerRoleService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
            _ledgerRepo = ledgerRepo;
        }
        // GET: odata/Vouchers
        [HttpGet, EnableQuery]
        public IQueryable<LedgerRole> Get()
        {
            return _ledgerRoleService.Queryable();
        }
        // GET: odata/Vouchers(5)
        [EnableQuery]
        public SingleResult<LedgerRole> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_ledgerRoleService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/Vouchers(5)
        public async Task<IHttpActionResult> Put(long key, LedgerRole ledgerRole)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != ledgerRole.Id)
            {
                return BadRequest();
            }
            ledgerRole.ObjectState = ObjectState.Modified;
            _ledgerRoleService.Update(ledgerRole);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (ledgerRole.IsDefault)
                {
                    var ledger = await _ledgerRepo.FindAsync(ledgerRole.LedgerId);
                    ledger.AccountRoleId = ledgerRole.RoleId;
                    ledger.ObjectState = ObjectState.Modified;
                }
                await _unitOfWorkAsync.SaveChangesAsync();
                await _ledgerRepo.MapLedgerToDefaultRoleClass(ledgerRole.LedgerId, ledgerRole.RoleId, null);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (!LedgerRoleExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(ledgerRole);
        }
        // POST: odata/Vouchers
        public async Task<IHttpActionResult> Post(LedgerRole ledgerRole)
        {
            ledgerRole.ObjectState = ObjectState.Added;
            _ledgerRoleService.Insert(ledgerRole);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (ledgerRole.IsDefault)
                {
                    var ledger=await _ledgerRepo.FindAsync(ledgerRole.LedgerId);
                    ledger.AccountRoleId = ledgerRole.RoleId;
                    ledger.ObjectState=ObjectState.Modified;
                }
                await _unitOfWorkAsync.SaveChangesAsync();
                await _ledgerRepo.MapLedgerToDefaultRoleClass(ledgerRole.LedgerId, ledgerRole.RoleId, null);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (LedgerRoleExists(ledgerRole.LedgerId, ledgerRole.RoleId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Role already mapped with this Ledger.");
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Created(ledgerRole);
        }
        //// PATCH: odata/Vouchers(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<LedgerRole> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            LedgerRole ledgerRole = await _ledgerRoleService.FindAsync(key);
            if (ledgerRole == null)
            {
                return NotFound();
            }
            var oldRoleId = ledgerRole.RoleId;
            ledgerRole.ObjectState = ObjectState.Modified;
            patch.Patch(ledgerRole);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (ledgerRole.IsDefault)
                {
                    var ledger = await _ledgerRepo.FindAsync(ledgerRole.LedgerId);
                    ledger.AccountRoleId = ledgerRole.RoleId;
                    ledger.ObjectState = ObjectState.Modified;
                }
                await _unitOfWorkAsync.SaveChangesAsync();
               if(oldRoleId!=ledgerRole.RoleId) await _ledgerRepo.MapLedgerToDefaultRoleClass(ledgerRole.LedgerId, ledgerRole.RoleId, oldRoleId);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (!LedgerRoleExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(ledgerRole);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var ledgerRole = await _ledgerRoleService.FindAsync(key);
            if (ledgerRole == null)
            {
                return NotFound();
            }
            ledgerRole.ObjectState = ObjectState.Deleted;
            var oldRole = ledgerRole.RoleId;
            var ledgerId = ledgerRole.LedgerId;
            _ledgerRoleService.Delete(ledgerRole);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                await uow.SaveChangesAsync();
                await _ledgerRepo.MapLedgerToDefaultRoleClass(ledgerId, null, oldRole);
            }
            catch (Exception e)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
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

        private bool LedgerRoleExists(long ledgerId,long roleId)
        {
            return _ledgerRoleService.Query(e => e.LedgerId == ledgerId && e.RoleId==roleId).Select().Any();
        }
        private bool LedgerRoleExists(long key)
        {
            return _ledgerRoleService.Query(e => e.Id == key).Select().Any();
        }
        
    }
}