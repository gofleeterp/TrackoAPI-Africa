using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoAPI.Infrastructure.Filters;
using System.Web.OData.Routing;
using System.Data.Entity;
using TrackoAPI.WebUtilities.Helper;
using TrackoApi.Service;
using TrackoApi.Models.FMS;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Management;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class PurchaseRequisitionsController : ODataController
    //ODataController
    {
        private readonly IPurchaseRequisitionService _objPurchaseRequisitionService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public PurchaseRequisitionsController(IUnitOfWorkAsync unitOfWorkAsync, IPurchaseRequisitionService service)
        {
            _objPurchaseRequisitionService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/PurchaseRequisitions
        [HttpGet, EnableQuery]
        public IQueryable<PurchaseRequisition> Get()
        {
            return _objPurchaseRequisitionService.Queryable();
        }
        // GET: odata/PurchaseRequisitions(5)
        [EnableQuery]
        public SingleResult<PurchaseRequisition> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objPurchaseRequisitionService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PurchaseRequisitions(5)
        public async Task<IHttpActionResult> Put(long key, PurchaseRequisition objPurchaseRequisition)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            return BadRequest("Not allowed");
            if (key != objPurchaseRequisition.Id)
            {
                return BadRequest();
            }
            objPurchaseRequisition.ObjectState = ObjectState.Modified;
            _objPurchaseRequisitionService.Update(objPurchaseRequisition);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseRequisitionExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            return Updated(objPurchaseRequisition);
        }
        // POST: odata/PurchaseRequisitions
        public async Task<IHttpActionResult> Post(PurchaseRequisition objPurchaseRequisition)
        {
            var jsonLogs = objPurchaseRequisition.LogsJson;
            objPurchaseRequisition.ObjectState = ObjectState.Added;
            _objPurchaseRequisitionService.Insert(objPurchaseRequisition);
            if(!string.IsNullOrWhiteSpace(jsonLogs))
            {
                var repo = _unitOfWorkAsync.RepositoryAsync<PurchaseRequisitionLog>();
                var logs = JsonConvert.DeserializeObject<List<PurchaseRequisitionLog>>(jsonLogs);
                foreach (var l in logs)
                {
                    l.fk_PR = objPurchaseRequisition;
                    l.PRId = objPurchaseRequisition.Id;
                    l.ObjectState = ObjectState.Added;
                    repo.Insert(l);
                }
            }
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PurchaseRequisitionExists(objPurchaseRequisition.DocNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            //try
            //{
            //    var v1 = await _unitOfWorkAsync.SqlQueryAsync(
            //    "[dbo].[Proc_GBL_CreateAPLData]",
            //    new SqlParameter() { Value = objPurchaseRequisition.Id, ParameterName = "parameter1" },
            //    new SqlParameter() { Value = objPurchaseRequisition.DocNo, ParameterName = "parameter3" },
            //    new SqlParameter() { Value = objPurchaseRequisition.ViewId, ParameterName = "parameter2" }
            //    );
            //}
            //catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            return Created(objPurchaseRequisition);
        }
        //// PATCH: odata/PurchaseRequisitions(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PurchaseRequisition> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PurchaseRequisition objPurchaseRequisition = await _objPurchaseRequisitionService.FindAsync(key);
            if (objPurchaseRequisition == null)
            {
                return NotFound();
            }
            var jsonLogs = patch.GetEntity().LogsJson;
            objPurchaseRequisition.ObjectState = ObjectState.Modified;
            patch.Patch(objPurchaseRequisition);
            if (!string.IsNullOrWhiteSpace(jsonLogs))
            {
                var repo = _unitOfWorkAsync.RepositoryAsync<PurchaseRequisitionLog>();
                var logs = JsonConvert.DeserializeObject<List<PurchaseRequisitionLog>>(jsonLogs);
                foreach (var l in logs)
                {
                    l.fk_PR = objPurchaseRequisition;
                    l.PRId = objPurchaseRequisition.Id;
                    if (l.ObjectState == ObjectState.Unchanged)
                    {
                        l.ObjectState = l.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    }
                    switch (l.ObjectState)
                    {
                        case ObjectState.Added:
                            repo.Insert(l);
                            break;
                        case ObjectState.Modified:
                            repo.Update(l);
                            break;
                        case ObjectState.Deleted:
                            repo.Delete(l);
                            break;
                    }
                    
                }
            }
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseRequisitionExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            return Updated(objPurchaseRequisition);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objPurchaseRequisition = await _objPurchaseRequisitionService.FindAsync(key);
            if (objPurchaseRequisition == null)
            {
                return NotFound();
            }
            objPurchaseRequisition.ObjectState = ObjectState.Deleted;
            _objPurchaseRequisitionService.Delete(objPurchaseRequisition);
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

        private bool PurchaseRequisitionExists(string DocNo)
        {
            return _objPurchaseRequisitionService.Query(e => e.DocNo == DocNo).Select().Any();
        }
        private bool PurchaseRequisitionExists(long key)
        {
            return _objPurchaseRequisitionService.Query(e => e.Id == key).Select().Any();
        }
        //POST:odata/PurchaseRequisitions(key)/Logs
        [ODataRoute("PurchaseRequisitions({key})/Logs")]
        public async Task<IHttpActionResult> PostPurchaseRequisitionLogs([FromODataUri] long key, [FromBody] PurchaseRequisitionLog log)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var po = await _objPurchaseRequisitionService.Queryable().Include(x => x.Logs).FirstOrDefaultAsync(x => x.Id == key);

            if (po == null)
            {
                return NotFound();
            }
            log.PRId = key;
            var uow = Request.GetContext();
            log.ObjectState = ObjectState.Added;
            po.Logs.Add(log);
            po.ObjectState = ObjectState.Modified;
            await uow.SaveChangesAsync();

            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = po.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = po.DocNo, ParameterName = "parameter3" },
                new SqlParameter() { Value = po.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            return Created(log);
        }
    }
}