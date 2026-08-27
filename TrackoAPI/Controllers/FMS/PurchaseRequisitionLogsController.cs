using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Management;
using System.Web.OData;
using System.Web.UI.WebControls;

using Repository.Pattern.Core.UnitOfWork;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

using static Microsoft.TeamFoundation.Client.CommandLine.Options;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class PurchaseRequisitionLogsController : ODataController
    //ODataController
    {
        private readonly IPurchaseRequisitionLogService _objlogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public PurchaseRequisitionLogsController(IUnitOfWorkAsync unitOfWorkAsync, IPurchaseRequisitionLogService service)
        {
            _objlogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/PurchaseRequisitionLogs
        [HttpGet, EnableQuery]
        public IQueryable<PurchaseRequisitionLog> Get()
        {
            return _objlogService.Queryable();
        }
        // GET: odata/PurchaseRequisitionLogs(5)
        [EnableQuery]
        public SingleResult<PurchaseRequisitionLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objlogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PurchaseRequisitionLogs(5)
        public async Task<IHttpActionResult> Put(long key, PurchaseRequisitionLog objlog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objlog.Id)
            {
                return BadRequest();
            }
            if (objlog.RequestQty <= 0) return BadRequest("Requested Qty should be greater than Zero.");

            objlog.ObjectState = ObjectState.Modified;
            _objlogService.Update(objlog);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseRequisitionLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = objlog.PRId, ParameterName = "parameter1" },
                new SqlParameter() { Value = objlog.fk_PR.DocNo, ParameterName = "parameter3" },
                new SqlParameter() { Value = objlog.fk_PR.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex)
            {
                return BadRequest(ex.Message);
            }
            return Updated(objlog);
        }
        // POST: odata/PurchaseRequisitionLogs
        public async Task<IHttpActionResult> Post(PurchaseRequisitionLog objlog)
        {
            if (objlog.RequestQty <= 0) return BadRequest("Requested Qty should be greater than Zero.");
            objlog.ObjectState = ObjectState.Added;
            _objlogService.Insert(objlog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {

                throw;
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = objlog.PRId, ParameterName = "parameter1" },
                new SqlParameter() { Value = objlog.fk_PR.DocNo, ParameterName = "parameter3" },
                new SqlParameter() { Value = objlog.fk_PR.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex)
            {
                return BadRequest(ex.Message);
            }
            return Created(objlog);

        }
        //// PATCH: odata/PurchaseRequisitionLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PurchaseRequisitionLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            PurchaseRequisitionLog objlog = await _objlogService.FindAsync(key);
            if (objlog == null)
            {
                return NotFound();
            }
            objlog.ObjectState = ObjectState.Modified;
            patch.Patch(objlog);

            if (objlog.RequestQty <= 0) return BadRequest("Indent Qty should be greater than Zero.");
            
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PurchaseRequisitionLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_CreateAPLData]",
                new SqlParameter() { Value = objlog.PRId, ParameterName = "parameter1" },
                new SqlParameter() { Value = objlog.fk_PR.DocNo, ParameterName = "parameter3" },
                new SqlParameter() { Value = objlog.fk_PR.ViewId, ParameterName = "parameter2" }
                );
            }
            catch (SqlExecutionException ex)
            {
                return BadRequest(ex.Message);
            }
            return Updated(objlog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objlog = await _objlogService.FindAsync(key);
            if (objlog == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }
            try
            {
                var v1 = await _unitOfWorkAsync.SqlQueryAsync(
                "[dbo].[Proc_GBL_TSL_Delete]",
                new SqlParameter() { Value = objlog.Id, ParameterName = "parameter1" },//TSLID
                new SqlParameter() { Value = objlog.PRId, ParameterName = "parameter2" },//TransactionId
                new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter3" },//CSId
                new SqlParameter() { Value = objlog.fk_PR.ViewId, ParameterName = "parameter4" }//ViewId
                );
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }

            var linkedPO = await _unitOfWorkAsync.RepositoryAsync<PurchaseOrderLog>().Queryable()
                   .Where(x => x.PRLId == objlog.Id)
                   .Select(y => new { y.fk_PurchaseOrder.PONo, y.fk_PurchaseOrder.PODate })
                   .FirstOrDefaultAsync();
            if (linkedPO!=null)
            {
               
                var IndentSpareName = await _unitOfWorkAsync.RepositoryAsync<SpareMaster>().Queryable()
                    .Where(x => x.Id == objlog.SpareId)
                    .Select(y => y.SpareName)
                    .FirstOrDefaultAsync();

                return BadRequest($"Item='{ IndentSpareName }' has been linked in PO: {linkedPO.PONo} dt: {linkedPO.PODate:dd-MMM-yyyy}");
            }
            

            objlog.ObjectState = ObjectState.Deleted;

            _objlogService.Delete(objlog);
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

        private bool PurchaseRequisitionLogExists(long key)
        {
            return _objlogService.Query(e => e.Id == key).Select().Any();
        }
    }
}