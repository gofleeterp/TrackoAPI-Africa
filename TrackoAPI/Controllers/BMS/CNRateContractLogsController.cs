using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.OData;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNRateContractLogsController : ODataController
    {
        private readonly ICNRateContractLogService _repo;

        public CNRateContractLogsController(ICNRateContractLogService service)
        {
            _repo = service;
        }

        [HttpPost]
        public IHttpActionResult BulkPost(ODataActionParameters parameters)
        {
            try
            {
                var batchId = Guid.NewGuid().ToString("N");
                var icontractlogs = parameters["contractlogs"] as IEnumerator<CNRateContractLog>;
                if (icontractlogs == null) return BadRequest("No Contract log found to upload");
                var contractlogs = icontractlogs.ToList();
                if (contractlogs.Any(x => x.RateContractId <= 0))
                {
                    return BadRequest("One of rate contract record doesn't have valid contract name.");
                }
                var uow = Request.GetContext();
                Parallel.ForEach(contractlogs.AsParallel(), entity =>
                {
                    entity.ObjectState = ObjectState.Added;
                    entity.CreatedDOE = DateTime.Now;
                    entity.CreatedSessionId = Helper.SessionId();
                    entity.BatchId = batchId;
                });

                using (var transaction = new TransactionScope())
                {
                    uow.BulkInsert(contractlogs);
                    transaction.Complete();
                }
                var item = new vwBatch { BatchId = batchId, BatchSize = contractlogs.Count };
                return Ok(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // DELETE: odata/CNBillLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var contractLog = await _repo.FindAsync(key);
            if (contractLog == null)
            {
                return NotFound();
            }
            contractLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(contractLog);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/CNRateContractLogs
        [HttpGet, EnableQuery]
        public IQueryable<CNRateContractLog> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/CNRateContractLogs(5)
        [EnableQuery]
        public SingleResult<CNRateContractLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        [HttpGet, EnableQuery]
        public IQueryable<LoadType> GetDistinctLoadTypes([FromODataUri] long key)
        {
            return _repo.Queryable().Where(y => y.RateContractId == key).Select(x => x.fk_LoadType).Distinct();
        }

        //// PATCH: odata/CNBillLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNRateContractLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CNRateContractLog ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            ch.ObjectState = ObjectState.Modified;
            patch.Patch(ch);
            await Request.GetContext().SaveChangesAsync();
            return Updated(ch);
        }

        // POST: odata/CNBillLogs
        public async Task<IHttpActionResult> Post(CNRateContractLog rateContractLog)
        {
            rateContractLog.ObjectState = ObjectState.Added;

            var ch = _repo.Insert(rateContractLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(ch);
        }

        // PUT: odata/CNRateContractLogs(5)
        public async Task<IHttpActionResult> Put(long key, CNRateContractLog cNRateContractLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != cNRateContractLog.Id)
            {
                return BadRequest();
            }
            cNRateContractLog.ObjectState = ObjectState.Modified;
            _repo.Update(cNRateContractLog);
            await Request.GetContext().SaveChangesAsync();

            return Updated(cNRateContractLog);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing && !Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}