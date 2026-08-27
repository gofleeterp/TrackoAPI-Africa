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
using TrackoApi.Models.BMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class BillSubmissionsController : ODataController
    //ODataController
    {
        private readonly IBillSubmissionService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public BillSubmissionsController(IUnitOfWorkAsync unitOfWorkAsync, IBillSubmissionService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/BillSubmissions
        [HttpGet, EnableQuery]
        public IQueryable<BillSubmission> Get() => _repo.Queryable();

        // GET: odata/BillSubmissions(5)
        [EnableQuery]
        public SingleResult<BillSubmission> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/BillSubmissions(5)
        public async Task<IHttpActionResult> Put(long key, BillSubmission objBillSubmission)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objBillSubmission.Id)
            {
                return BadRequest();
            }
            objBillSubmission.ObjectState = ObjectState.Modified;
            _repo.Update(objBillSubmission);

            try
            {
              
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillSubmissionExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objBillSubmission);
        }

        // POST: odata/BillSubmissions
        public async Task<IHttpActionResult> Post(BillSubmission objBillSubmission)
        {
            objBillSubmission.ObjectState = ObjectState.Added;
            _repo.Insert(objBillSubmission);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BillSubmissionExists(objBillSubmission.DocNumber))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Doc Number already exists");
                }
                throw;
            }
            return Created(objBillSubmission);
        }
        //// PATCH: odata/BillSubmissions(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<BillSubmission> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BillSubmission objBillSubmission = await _repo.FindAsync(key);
            if (objBillSubmission == null)
            {
                return NotFound();
            }
            objBillSubmission.ObjectState = ObjectState.Modified;
            patch.Patch(objBillSubmission);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BillSubmissionExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objBillSubmission);
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
                var objBillSubmission = await _repo.FindAsync(key);
                if (objBillSubmission == null)
                {
                    return NotFound();
                }
                objBillSubmission.ObjectState = ObjectState.Deleted;
                _repo.Delete(objBillSubmission);
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

        private bool BillSubmissionExists(string name) => _repo.Query(e => e.DocNumber == name).Select().Any();
        private bool BillSubmissionExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}