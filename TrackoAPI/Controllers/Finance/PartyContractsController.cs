using System;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class PartyContractsController : ODataController
    //ODataController
    {
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly IRepositoryAsync<PartyContractMap> _repo;

        public PartyContractsController(IUnitOfWorkAsync unitOfWorkAsync, IRepositoryAsync<PartyContractMap> service)
        {
            _unitOfWorkAsync = unitOfWorkAsync;
            _repo = service;
        }
        // GET: odata/PartyContracts
        [HttpGet, EnableQuery]
        public IQueryable<PartyContractMap> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/PartyContracts(5)
        [EnableQuery]
        public SingleResult<PartyContractMap> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PartyContracts(5)
        public async Task<IHttpActionResult> Put(long key, PartyContractMap mapping)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != mapping.Id)
            {
                return BadRequest();
            }
            mapping.ObjectState = ObjectState.Modified;
            _repo.Update(mapping);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (mapping.IsDefault)
                {
                    var ledger = await _repo.FindAsync(mapping.PartyId);
                    ledger.ContractId = mapping.ContractId;
                    ledger.ObjectState = ObjectState.Modified;
                }
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (!PartyContractMapExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(mapping);
        }
        // POST: odata/PartyContracts
        public async Task<IHttpActionResult> Post(PartyContractMap mapping)
        {
            mapping.ObjectState = ObjectState.Added;
            _repo.Insert(mapping);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (mapping.IsDefault)
                {
                    var ledger=await _repo.FindAsync(mapping.PartyId);
                    ledger.ContractId = mapping.ContractId;
                    ledger.ObjectState=ObjectState.Modified;
                }
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (PartyContractMapExists(mapping.PartyId, mapping.ContractId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Role already mapped with this Ledger.");
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Created(mapping);
        }
        //// PATCH: odata/PartyContracts(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PartyContractMap> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PartyContractMap PartyContractMap = await _repo.FindAsync(key);
            if (PartyContractMap == null)
            {
                return NotFound();
            }
            var oldContractId = PartyContractMap.ContractId;
            PartyContractMap.ObjectState = ObjectState.Modified;
            patch.Patch(PartyContractMap);
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                if (PartyContractMap.IsDefault)
                {
                    var ledger = await _repo.FindAsync(PartyContractMap.PartyId);
                    ledger.ContractId = PartyContractMap.ContractId;
                    ledger.ObjectState = ObjectState.Modified;
                }
                await _unitOfWorkAsync.SaveChangesAsync();               
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                if (!PartyContractMapExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(PartyContractMap);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var PartyContractMap = await _repo.FindAsync(key);
            if (PartyContractMap == null)
            {
                return NotFound();
            }
            PartyContractMap.ObjectState = ObjectState.Deleted;
            var oldRole = PartyContractMap.ContractId;
            var PartyId = PartyContractMap.PartyId;
            _repo.Delete(PartyContractMap);
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

        private bool PartyContractMapExists(long PartyId,long ContractId)
        {
            return _repo.Query(e => e.PartyId == PartyId && e.ContractId==ContractId).Select().Any();
        }
        private bool PartyContractMapExists(long key)
        {
            return _repo.Query(e => e.Id == key).Select().Any();
        }
        
    }
}