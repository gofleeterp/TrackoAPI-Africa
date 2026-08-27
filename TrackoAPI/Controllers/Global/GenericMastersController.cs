using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class GenericMastersController : ODataController
    //ODataController
    {
        private readonly IGenericMasterService _objGenericMasterService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public GenericMastersController(IUnitOfWorkAsync unitOfWorkAsync, IGenericMasterService service)
        {
            _objGenericMasterService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }

        [HttpPost]
        public async Task<IHttpActionResult> BulkPostGeneric(ODataActionParameters parameters)
        {
            var batchId = Guid.NewGuid().ToString("N");
            var igenerics = parameters["masters"] as IEnumerator<GenericMaster>;
            if (igenerics == null) return BadRequest("No Records found to upload");
            var mesters = igenerics.ToList();
            var uow = Request.GetContext();
            var constants = await
                uow.RepositoryAsync<ConstantValue>()
                    .Queryable()
                    .Where(x => x.ConstantTypeId == 44)
                    .Select(x => x.Id)
                    .ToListAsync();
            var invalidtypes = mesters.Where(x => !constants.Contains(x.ConstantId)).Select(x => x.Name).ToList();
            if (invalidtypes.Any()) return BadRequest($"Following Master has invalid Type Defined.\n{invalidtypes.Take(10).JoinStrings(",")}");
            Parallel.ForEach(mesters.AsParallel(), entity =>
            {
                entity.CreatedDOE = DateTime.Now;
                entity.CreatedSessionId = Helper.SessionId();
                entity.BatchId = batchId;
            });
            try
            {
                using (var transaction = new TransactionScope())
                {
                    uow.BulkInsert(mesters);
                    transaction.Complete();
                }
                var item = new vwBatch { BatchId = batchId, BatchSize = mesters.Count };
                return Ok(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objGenericMaster = await _objGenericMasterService.FindAsync(key);
            if (objGenericMaster == null)
            {
                return NotFound();
            }
            objGenericMaster.ObjectState = ObjectState.Deleted;
            _objGenericMasterService.Delete(objGenericMaster);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/GenericMasters
        [HttpGet, EnableQuery]
        public IQueryable<GenericMaster> Get()
        {
            return _objGenericMasterService.Queryable();
        }

        // GET: odata/GenericMasters(5)
        [EnableQuery]
        public SingleResult<GenericMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objGenericMasterService.Queryable().Where(t => t.Id == key));
        }

        //// PATCH: odata/GenericMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<GenericMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GenericMaster objGenericMaster = await _objGenericMasterService.FindAsync(key);
            if (objGenericMaster == null)
            {
                return NotFound();
            }
            objGenericMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objGenericMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenericMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objGenericMaster);
        }

        // POST: odata/GenericMasters
        public async Task<IHttpActionResult> Post(GenericMaster objGenericMaster)
        {
            objGenericMaster.ObjectState = ObjectState.Added;
            _objGenericMasterService.Insert(objGenericMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (GenericMasterExists(objGenericMaster.Name, objGenericMaster.ConstantId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
            }
            return Created(objGenericMaster);
        }

        // PUT: odata/GenericMasters(5)
        public async Task<IHttpActionResult> Put(long key, GenericMaster objGenericMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objGenericMaster.Id)
            {
                return BadRequest();
            }
            objGenericMaster.ObjectState = ObjectState.Modified;
            _objGenericMasterService.Update(objGenericMaster);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GenericMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objGenericMaster);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool GenericMasterExists(string name, long constantValue)
        {
            return _objGenericMasterService.Query(e => e.Name == name && e.ConstantId == constantValue).Select().Any();
        }

        private bool GenericMasterExists(long key)
        {
            return _objGenericMasterService.Query(e => e.Id == key).Select().Any();
        }
    }
}