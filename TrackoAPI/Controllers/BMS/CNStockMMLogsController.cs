using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNStockMMLogsController : ODataController
    {
        private readonly ICNStockMMLogService _repo;

        public CNStockMMLogsController(ICNStockMMLogService service)
        {
            _repo = service;
        }

        // DELETE: odata/CNStockMMLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            var cnStockMmLog = await _repo.Queryable().Include(x => x.Outwards).FirstOrDefaultAsync(x => x.Id == key);
            if (cnStockMmLog == null)
            {
                return NotFound();
            }

            var sllog = cnStockMmLog.LogTypeId;
            var outqty = cnStockMmLog.OutQty;
            var tlid = cnStockMmLog.TriplogId;
            var chcnid = cnStockMmLog.ChallanCNId;
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction();
            }
            try
            {
                cnStockMmLog.ObjectState = ObjectState.Deleted;
                cnStockMmLog.Outwards?.ForEach(x => x.ObjectState = ObjectState.Deleted);
                await uow.SaveChangesAsync();
                var chcnqty = await uow.RepositoryAsync<CnChallan>().Queryable().AsNoTracking().Where(x => x.Id == chcnid).Select(x => x.Qty).FirstOrDefaultAsync();
                if (/*chcn!=null&&*/(sllog == 1423 || sllog == 1451 || sllog == 1454 || sllog == 1455))
                {
                    if (tlid > 0)
                    {
                        await _repo.ExecuteSqlAsync($"UPDATE tVehicleMovementLog SET LoadingQty=(CASE WHEN LoadingQty>0 THEN LoadingQty-{outqty} ELSE 0 END) WHERE Id={tlid}");
                    }
                    chcnqty -= outqty;
                    var chcnstate = chcnqty <= 0 ? ObjectState.Deleted : ObjectState.Modified;
                    if (chcnstate == ObjectState.Deleted)
                    {
                        uow.DeleteStockByChallanId(chcnid ?? 0);
                        await _repo.ExecuteSqlAsync($"DELETE tCNChallan WHERE Id={chcnid}");
                    }
                    else
                    {
                        await _repo.ExecuteSqlAsync($"UPDATE tCNChallan SET Qty=Qty-{outqty} WHERE Id={chcnid}");
                        await _repo.ExecuteSqlAsync($"UPDATE tCNStockLog SET OutQty=OutQty-{outqty} WHERE ChallanCNId={chcnid}");
                    }
                }
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/CNStockMMLogs
        [HttpGet, EnableQuery]
        public IQueryable<CNStockMMLog> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/CNStockMMLogs(5)
        [EnableQuery]
        public SingleResult<CNStockMMLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        //// PATCH: odata/CNStockMMLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNStockMMLog> patch)
        {
            var uom = Request.GetContext();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CNStockMMLog ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            ch.ObjectState = ObjectState.Modified;
            var oldstatus = ch.LogTypeId;
            patch.Patch(ch);
            if (!Request.IsBatchRequest())
            {
                uom.BeginTransaction();
            }
            try
            {
                if (ch.LogTypeId == 1425 || ch.LogTypeId == 1455 || ch.LogTypeId == 1451)
                {
                    var count =
                      new
                      {
                          Delivered = await
                              _repo.Queryable()
                                  .CountAsync(x => x.CNId == ch.CNId && (x.LogTypeId == 1425 || x.LogTypeId == 1455) && x.ChallanCNId == ch.ChallanCNId),

                          Total = await
                              _repo.Queryable()
                                  .CountAsync(
                                      x =>
                                          x.CNId == ch.CNId &&
                                          x.ChallanCNId == ch.ChallanCNId)
                      };
                    if (count.Delivered + (ch.LogTypeId == 1422 && oldstatus != 1422 ? ch.InQty : 0) == count.Total)
                    {
                        var tl = await uom.RepositoryAsync<VehicleMovementLog>().FindAsync(ch.TriplogId);
                        tl.UnloadingReachDate = ch.LogDate;
                        tl.UnloadingDate = ch.LogDate;
                        tl.ObjectState = ObjectState.Modified;
                    }
                }

                await uom.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uom.Rollback();
                }
                throw;
            }
            return Updated(ch);
        }

        // POST: odata/CNStockMMLogs
        public async Task<IHttpActionResult> Post(CNStockMMLog CNStockMMLog)
        {
            CNStockMMLog.ObjectState = ObjectState.Added;

            var ch = _repo.Insert(CNStockMMLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(ch);
        }

        // PUT: odata/CNStockMMLogs(5)
        public async Task<IHttpActionResult> Put(long key, CNStockMMLog CNStockMMLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != CNStockMMLog.Id)
            {
                return BadRequest();
            }
            CNStockMMLog.ObjectState = ObjectState.Modified;
            _repo.Update(CNStockMMLog);
            await Request.GetContext().SaveChangesAsync();

            return Updated(CNStockMMLog);
        }

        [HttpGet, EnableQuery]
        public IQueryable<CNStockMMLog> SearchTop10CNStockMM([FromODataUri] long stockOfficeId, [FromODataUri] DateTime stockDate, [FromODataUri] string serachTerm)
        {
            var stockInStatus = new List<long> { 1422, 1455 };
            var query =
                _repo.Queryable()
                    .Where(
                        x =>
                            x.OfficeId == stockOfficeId && x.LogDate <= stockDate && (x.InQty - (x.Outwards.Sum(y => (decimal?)y.OutQty) == null ? 0 : x.Outwards.Sum(y => (decimal?)y.OutQty))) > 0 && stockInStatus.Contains(x.LogTypeId));
            if (!string.IsNullOrWhiteSpace(serachTerm))
            {
                query = query.Where(x => x.fk_CNMaster.CNNo.Contains(serachTerm) ||
                                         x.fk_CNMaterial.MaterialName.Contains(serachTerm) ||
                                         x.fk_CNMaterial.Abbreviation.Contains(serachTerm) ||
                                         x.fk_CNMM.InvoiceNo.Contains(serachTerm));
            }
            return query;
        }
        [HttpPost]
        public async Task<IHttpActionResult> SRVUpdate(ODataActionParameters parameters)
        {
            var uow = Request.GetContext();
            try
            {
                var srv = parameters["srvlist"] as IEnumerator<VW_DispatchAcknowledgment>;
                if (srv == null) return BadRequest("No records found to acknowledge.");
                var srvlist = srv.ToList();
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
                foreach (var record in srvlist)
                {
                    await uow.ExecSqlQueryAsync($"EXEC [dbo].[Proc_TRANS_1523_UpdateAndCreate]{record.Id},'{record.AcknowledgmentNo}',{record.LogType},{record.Qty}");
                }
                uow.Commit();
                // return Ok(result);
                return Ok();
            }
            catch (Exception ex)
            {
                uow.Rollback();
                throw;
            }
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