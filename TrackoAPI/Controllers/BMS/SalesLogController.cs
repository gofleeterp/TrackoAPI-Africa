using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
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
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.BMS
{
    [AuthorizeEx]
    public class SalesLogController : ODataController
    {
        private readonly ISalesLogService _repo;
        public SalesLogController(ISalesLogService service)
        {
            _repo = service;
        }
        // GET: odata/SalesLog
        [HttpGet, EnableQuery]
        public IQueryable<SalesLog> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/SalesLog
        [EnableQuery]
        public SingleResult<SalesLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/SalesLog
        public async Task<IHttpActionResult> Put(long key, SalesLog dispatchbill)
        { 
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != dispatchbill.Id)
            {
                return BadRequest();
            }
            dispatchbill.ObjectState = ObjectState.Modified;
            _repo.Update(dispatchbill);
            await Request.GetContext().SaveChangesAsync();

            return Updated(dispatchbill);
        }


        private bool SalesLogExists(long key)
        {
            return _repo.Query(e => e.TripLogId == key).Select().Any();
        }

        public async Task<IHttpActionResult> Post(SalesLog salesLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            salesLog.ObjectState = ObjectState.Added;
            _repo.Insert(salesLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(salesLog);
        }

        //// PATCH: odata/SalesLog(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<SalesLog> patch)
        {
            var uow = Request.GetContext();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SalesLog saleslog = await _repo.FindAsync(key);
            if (saleslog == null)
            {
                return NotFound();
            }
            saleslog.ObjectState = ObjectState.Modified;
            patch.Patch(saleslog);
            try
            {
                await uow.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SalesLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(saleslog);
        }

        public async Task<IHttpActionResult> Delete(long key)
        {
            var objSalesLog = await _repo.FindAsync(key);
            if (objSalesLog == null)
            {
                return NotFound();
            }
            objSalesLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(objSalesLog);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var dispatch = await _repo.Queryable().FirstOrDefaultAsync(p => p.Id == key);
            if (dispatch == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_CN":
                    dispatch.CNId = null;
                    dispatch.fk_CN = null;
                    break;
                case "fk_TripLog":
                    dispatch.TripLogId = null;
                    dispatch.fk_TripLog = null;
                    break;
                case "fk_ChallanCN":
                    dispatch.ChallanCNId = null;
                    dispatch.fk_ChallanCN = null;
                    break;
                //case "fk_Bill":
                //    dispatch.BillId = null;
                //    dispatch.fk_Bill = null;
                //    break;
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
                case "fk_CN":

                    var cnrepo = uow.RepositoryAsync<CNMaster>();
                    var iscnexist = await cnrepo.Queryable().AnyAsync(x => x.Id == id);
                    if (!iscnexist)
                    {
                        return NotFound();
                    }
                    log.CNId = id;
                    log.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;
                case "fk_TripLog":
                    var trrepo = uow.RepositoryAsync<VehicleMovementLog>();
                    var advance = await
                        trrepo.Queryable().AnyAsync(x => x.Id == id);
                    if (!advance)
                    {
                        return NotFound();
                    }
                    log.TripLogId = id;
                    log.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;
                case "fk_ChallanCN":
                    var cnid = uow.RepositoryAsync<CnChallan>();
                    var billcnid = await cnid.Queryable().AnyAsync(x => x.Id == id);
                    if (!billcnid)
                    {
                        return NotFound();
                    }
                    log.ChallanCNId = id;
                    log.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;
                //case "fk_Bill":
                //    var billid = uow.RepositoryAsync<CNBill>();
                //    var tripbillid = await billid.Queryable().AnyAsync(x => x.Id == id);
                //    if (!tripbillid)
                //    {
                //        return NotFound();
                //    }
                //    log.BillId = id;
                //    log.ObjectState = ObjectState.Modified;
                //    await uow.SaveChangesAsync();
                //    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
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