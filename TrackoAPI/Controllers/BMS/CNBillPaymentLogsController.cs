
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNBillPaymentLogsController : ODataController
    //ODataController
    {
        private readonly ICNBillPaymentLogService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public CNBillPaymentLogsController(IUnitOfWorkAsync unitOfWorkAsync, ICNBillPaymentLogService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/CNBillPaymentLogs
        [HttpGet, EnableQuery]
        public IQueryable<CNBillPaymentLog> Get() => _repo.Queryable();

        // GET: odata/CNBillPaymentLogs(5)
        [EnableQuery]
        public SingleResult<CNBillPaymentLog> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/CNBillPaymentLogs(5)
        public async Task<IHttpActionResult> Put(long key, CNBillPaymentLog log)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != log.Id)
            {
                return BadRequest();
            }
            log.ObjectState = ObjectState.Modified;
            _repo.Update(log);

            try
            {
              await _unitOfWorkAsync.SaveChangesAsync();
                await _repo.UpdateBalanceAsync(log.BillLogId);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CNBillPaymentLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(log);
        }
        // POST: odata/CNBillPaymentLogs
        public async Task<IHttpActionResult> Post(CNBillPaymentLog log)
        {
            log.ObjectState = ObjectState.Added;
            _repo.Insert(log);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
                if (log.BillLogId.GetValueOrDefault() > 0)
                {
                    await _repo.UpdateBalanceAsync(log.BillLogId);
                }
                if (log.OnAccountRefId.GetValueOrDefault() > 0)
                {
                    await _repo.UpdateOnAccountBalanceAsync(log.OnAccountRefId);
                }
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(log);
        }
        //// PATCH: odata/CNBillPaymentLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNBillPaymentLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CNBillPaymentLog log = await _repo.FindAsync(key);
            if (log == null)
            {
                return NotFound();
            }

            log.ObjectState = ObjectState.Modified;
            patch.Patch(log);
            try
            {
                if (log.OnAccountRefId > 0)
                {
                    //var plog = await _unitOfWorkAsync.RepositoryAsync<VoucherDetailReference>().Queryable().FirstOrDefaultAsync(x=>x.RefId==log.On)

                }
                await _unitOfWorkAsync.SaveChangesAsync();
                if (log.BillLogId.GetValueOrDefault() > 0)
                {
                    await _repo.UpdateBalanceAsync(log.BillLogId);
                }
                if (log.OnAccountRefId.GetValueOrDefault() > 0)
                {
                    await _repo.UpdateOnAccountBalanceAsync(log.OnAccountRefId);
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CNBillPaymentLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(log);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var log = await _repo.FindAsync(key);
            if (log == null)
            {
                return NotFound();
            }
            log.ObjectState = ObjectState.Deleted;
            _repo.Delete(log);
            var billlogid = log.BillLogId;
            var onaccountRefId = log.OnAccountRefId;
            await _unitOfWorkAsync.SaveChangesAsync();
            if (billlogid > 0)
            {
                await _repo.UpdateBalanceAsync(billlogid);                
            }
            if (onaccountRefId > 0)
            {
                await _repo.UpdateOnAccountBalanceAsync(onaccountRefId);
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

        private bool CNBillPaymentLogExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var cn = _repo.Queryable().SingleOrDefault(p => p.Id == key);
            if (cn == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_VDR":
                    cn.fk_VDR = null;
                    cn.VDRId = null;
                    break;
                case "fk_TripAdvance":
                    cn.fk_TripAdvance = null;
                    cn.TripAdvanceId = null;
                    break;
                case "fk_CN":
                    cn.fk_CN = null;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var log = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (log == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_VDR":

                    var vdrepo = uow.RepositoryAsync<VoucherDetailReference>();
                    var vdr = await
                            vdrepo.Queryable().FirstOrDefaultAsync(x => x.Id == id);
                    if (vdr == null)
                    {
                        return NotFound();
                    }
                    log.VDRId = id;
                    vdr.TransactionId = key;
                    vdr.ObjectState = ObjectState.Modified;
                    log.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;
                case "fk_TripAdvance":
                    var trrepo = uow.RepositoryAsync<TripAdvanceLog>();
                    var advance = await
                        trrepo.Queryable().FirstOrDefaultAsync(x => x.Id == id);
                    if (advance == null)
                    {
                        return NotFound();
                    }
                    log.TripAdvanceId = id;
                    log.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;
                case "fk_CN":
                    var cnid = uow.RepositoryAsync<CNMaster>();
                    var billcnid = await cnid.Queryable().FirstOrDefaultAsync(x => x.Id == id);
                    if(billcnid==null)
                    {
                        return NotFound();
                    }
                    log.CNId = id;
                    log.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}