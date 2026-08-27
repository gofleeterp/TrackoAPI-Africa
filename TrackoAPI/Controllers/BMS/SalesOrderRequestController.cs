using System;
using System.Data;
using System.Data.Entity.Infrastructure;
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
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class SalesOrderRequestsController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<SalesOrderRequest> _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public SalesOrderRequestsController(IUnitOfWorkAsync unitOfWorkAsync, IRepositoryAsync<SalesOrderRequest> service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/SalesOrderRequest
        [HttpGet, EnableQuery]
        public IQueryable<SalesOrderRequest> Get() => _repo.Queryable();

        // GET: odata/SalesOrderRequest(5)
        [EnableQuery]
        public SingleResult<SalesOrderRequest> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/SalesOrderRequest(5)
        public async Task<IHttpActionResult> Put(long key, SalesOrderRequest objSalesOrderRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objSalesOrderRequest.Id)
            {
                return BadRequest();
            }
            objSalesOrderRequest.ObjectState = ObjectState.Modified;
            _repo.Update(objSalesOrderRequest);

            try
            {
              
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalesOrderRequestExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objSalesOrderRequest);
        }

        // POST: odata/SalesOrderRequest
        public async Task<IHttpActionResult> Post(SalesOrderRequest objSalesOrderRequest)
        {
            objSalesOrderRequest.ObjectState = ObjectState.Added;
            _repo.Insert(objSalesOrderRequest);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SalesOrderRequestExists(objSalesOrderRequest.RequestNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "RequestNo already exists");
                }
                throw;
            }
            return Created(objSalesOrderRequest);
        }
        //// PATCH: odata/SalesOrderRequest(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<SalesOrderRequest> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SalesOrderRequest objSalesOrderRequest = await _repo.FindAsync(key);
            if (objSalesOrderRequest == null)
            {
                return NotFound();
            }
            objSalesOrderRequest.ObjectState = ObjectState.Modified;
            patch.Patch(objSalesOrderRequest);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalesOrderRequestExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objSalesOrderRequest);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var objSalesOrderRequest = await _repo.FindAsync(key);
                if (objSalesOrderRequest == null)
                {
                    return NotFound();
                }
                objSalesOrderRequest.ObjectState = ObjectState.Deleted;
                _repo.Delete(objSalesOrderRequest);
                await uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SalesOrderRequestExists(string name) => _repo.Query(e => e.RequestNo == name).Select().Any();
        private bool SalesOrderRequestExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}