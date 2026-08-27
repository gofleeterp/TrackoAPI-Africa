using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class LedgerOfficesController : ODataController
    //ODataController
    {
        private readonly ILedgerOfficeService _LedgerOfficeService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly IRepositoryAsync<Ledger> _ledgerRepo;

        public LedgerOfficesController(IUnitOfWorkAsync unitOfWorkAsync, ILedgerOfficeService service,IRepositoryAsync<Ledger> ledgerRepo)
        {
            _LedgerOfficeService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
            _ledgerRepo = ledgerRepo;
        }
        // GET: odata/Vouchers
        [HttpGet, EnableQuery]
        public IQueryable<LedgerOffice> Get()
        {
            return _LedgerOfficeService.Queryable();
        }
        // GET: odata/Vouchers(5)
        [EnableQuery]
        public SingleResult<LedgerOffice> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_LedgerOfficeService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/Vouchers(5)
        public async Task<IHttpActionResult> Put(long key, LedgerOffice LedgerOffice)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != LedgerOffice.Id)
            {
                return BadRequest();
            }
            LedgerOffice.ObjectState = ObjectState.Modified;
            _LedgerOfficeService.Update(LedgerOffice);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (!LedgerOfficeExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(LedgerOffice);
        }
        // POST: odata/Vouchers
        public async Task<IHttpActionResult> Post(LedgerOffice LedgerOffice)
        {
            LedgerOffice.ObjectState = ObjectState.Added;
            _LedgerOfficeService.Insert(LedgerOffice);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (LedgerOfficeExists(LedgerOffice.LedgerId, LedgerOffice.PlantName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Plant already mapped with this Ledger.");
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Created(LedgerOffice);
        }
        //// PATCH: odata/Vouchers(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<LedgerOffice> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            LedgerOffice LedgerOffice = await _LedgerOfficeService.FindAsync(key);
            if (LedgerOffice == null)
            {
                return NotFound();
            }
            
            LedgerOffice.ObjectState = ObjectState.Modified;
            patch.Patch(LedgerOffice);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
               await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (!LedgerOfficeExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(LedgerOffice);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var LedgerOffice = await _LedgerOfficeService.FindAsync(key);
            if (LedgerOffice == null)
            {
                return NotFound();
            }
            LedgerOffice.ObjectState = ObjectState.Deleted;
            _LedgerOfficeService.Delete(LedgerOffice);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                await uow.SaveChangesAsync();
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

        private bool LedgerOfficeExists(long ledgerId,string plantName)
        {
            return _LedgerOfficeService.Query(e => e.LedgerId == ledgerId && e.PlantName== plantName).Select().Any();
        }
        private bool LedgerOfficeExists(long key)
        {
            return _LedgerOfficeService.Query(e => e.Id == key).Select().Any();
        }
        
    }
}