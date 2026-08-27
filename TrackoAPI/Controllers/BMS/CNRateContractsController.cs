using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNRateContractsController : ODataController
    {
        private readonly ICNRateContractService _repo;
        private ICNRateContractLogService _logRepo;

        public CNRateContractsController(ICNRateContractService contractService, ICNRateContractLogService contractLogService)
        {
            _repo = contractService;
            _logRepo = contractLogService;
        }
        // GET: odata/CNRateContract
        [HttpGet, EnableQuery]
        public IQueryable<CNRateContract> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/CNRateContract(5)
        [EnableQuery]
        public SingleResult<CNRateContract> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/CNRateContract(5)
        public async Task<IHttpActionResult> Put(long key, CNRateContract rateContract)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != rateContract.Id)
            {
                return BadRequest();
            }
            rateContract.ObjectState = ObjectState.Modified;
            _repo.Update(rateContract);
            await Request.GetContext().SaveChangesAsync();

            return Updated(rateContract);
        }
        // POST: odata/CNRateContract
        public async Task<IHttpActionResult> Post(CNRateContract rateContract)
        {
            rateContract.ObjectState = ObjectState.Added;

            var ch = _repo.Insert(rateContract);
            await Request.GetContext().SaveChangesAsync();
            return Created(ch);
        }
        // PATCH: odata/CNRateContract(5)
        // PATCH performs a partial update.The client specifies just the properties to update.

       [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNRateContract> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CNRateContract ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            ch.ObjectState = ObjectState.Modified;
            patch.Patch(ch);
            await Request.GetContext().SaveChangesAsync();
            return Updated(ch);
        }
        // DELETE: odata/RateContract(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var rateContract = await _repo.FindAsync(key);
            if (rateContract == null)
            {
                return NotFound();
            }
            rateContract.ObjectState = ObjectState.Deleted;
            _repo.Delete(rateContract);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing&&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }


        // POST: odata/CNRateContracts(key)/CNRateContractLogs
        [AcceptVerbs("POST")]
        [ODataRoute("CNRateContracts({key})/RateContractLogs")]
        public async Task<IHttpActionResult> PostRateContractLogs([FromODataUri]long key, [FromBody] CNRateContractLog rateContractLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();

            var contract = await uow.RepositoryAsync<CNRateContract>().FindAsync(rateContractLog.RateContractId);
            contract.Id = key;
            contract.ObjectState = ObjectState.Modified;

            rateContractLog.RateContractId = key;
            rateContractLog.ObjectState = ObjectState.Added;

            try
            {
                _logRepo.Insert(rateContractLog);
                await uow.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (await RateContractLogExists(rateContractLog.RateContractId))
                {
                    return BadRequest("CN Already Mapped to this Bill");
                }
            }
            return Created(rateContractLog);
        }
        private async Task<bool> RateContractLogExists(long contractId)
        {
            return await Request.GetContext().RepositoryAsync<CNRateContractLog>().Queryable().AnyAsync(e => e.RateContractId == contractId);
        }
    }
}