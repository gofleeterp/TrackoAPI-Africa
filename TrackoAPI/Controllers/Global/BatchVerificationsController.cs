using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class BatchVerificationsController : ODataController
    {
        private readonly IRepositoryAsync<ReportProcedure> _procRepo;
        private readonly IUnitOfWorkAsync _uow;

        public BatchVerificationsController(IUnitOfWorkAsync uow, IRepositoryAsync<ReportProcedure> procRepo)
        {
            _uow = uow;
            _procRepo = procRepo;
        }
        [HttpGet, EnableQuery]
        public IQueryable<BatchVerification> Get()
        {
            return new[] { new BatchVerification()}.AsQueryable();
        }
        // GET: odata/DriverPayments(5)
        [EnableQuery]
        public SingleResult<BatchVerification> Get([FromODataUri] long key)
        {
            return SingleResult.Create(new[] { new BatchVerification() }.AsQueryable());
        }
        // POST: odata/BatchVerifications
        public async Task<IHttpActionResult> Post(BatchVerification entity)
        {
            var uow = Request.GetContext();
            var spname = await _procRepo.FindAsync(entity.ProcId);
            if (spname == null)
            {
                return BadRequest("Batch Verification Not Configured");
            }
            if(string.IsNullOrWhiteSpace(spname.StoredProcedureName))
            {
                return BadRequest("Procedure Name was blank");
            }
            await uow.ExecuteProcedureAsync(spname.StoredProcedureName, new SqlParameter("TransactionId", entity.TransactionId), new SqlParameter("TransactionNumber", entity.TransactionNumber), new SqlParameter("TransactionType", entity.TransactionType), new SqlParameter("JsonData", entity.JsonData));
            return Created(entity);
        }
        

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
                
            }
            base.Dispose(disposing);
        }
    }
}