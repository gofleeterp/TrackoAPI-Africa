using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service.BMS;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.BMS
{
    public class HMArrivalsController : ODataController
    {
        private readonly IHMArrivalService _repo;
        private readonly IHMArrivalLogService _logRepo;
        public HMArrivalsController(IHMArrivalService service, IHMArrivalLogService billLogService)
        {
            _repo = service;
            _logRepo = billLogService;
        }
        // GET: odata/HSArrivals
        [HttpGet, EnableQuery]
        public IQueryable<HMArrival> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/HSArrivals(5)
        [EnableQuery]
        public SingleResult<HMArrival> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/HSArrivals(5)
        public async Task<IHttpActionResult> Put(long key, HMArrival hmar)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != hmar.Id)
            {
                return BadRequest();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                hmar.ObjectState = ObjectState.Modified;
                _repo.Update(hmar);
                await uow.SaveChangesAsync();
            }
            catch
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(hmar);
        }
        // POST: odata/HMArrivals
        public async Task<IHttpActionResult> Post(HMArrival hmar)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                hmar.ObjectState = ObjectState.Added;
                _repo.Insert(hmar);
                await uow.SaveChangesAsync();
            }
            catch
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Created(hmar);
        }
        //// PATCH: odata/HSArrivals(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<HMArrival> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            HMArrival ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                patch.TryGetPropertyValue("Data", out var dv);
                patch.Patch(ch);
                ch.ObjectState = ObjectState.Modified;
                if (dv is List<JsonDataEntity> dataview && dataview.Any())
                {
                    foreach (var entity in dataview)
                    {
                        ch.DeleteAndAdd(entity);
                    }
                }
                await uow.SaveChangesAsync();
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                uow.Commit();
            }
            return Updated(ch);
        }
        
        // DELETE: odata/HSArrivals(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            var hmar = await _repo.FindAsync(key);
            if (hmar == null)
            {
                return NotFound();
            }
            hmar.ObjectState = ObjectState.Deleted;
            _repo.Delete(hmar);
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var tdsv = hmar.TDSVoucherId;
                var tdsamtv = hmar.TaxableAmtVoucherId;
                var ntdsamtv = hmar.NonTaxAmtVoucherId;
                _repo.ExecuteSql($"DELETE FROM tHMArrivalLog WHERE HMArrivalId={hmar.Id}");
                await uow.SaveChangesAsync();
                if (tdsv.GetValueOrDefault() > 0)
                {
                    _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={tdsv}");
                }
                if (tdsamtv.GetValueOrDefault() > 0)
                {
                    _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={tdsamtv}");
                }
                if (ntdsamtv.GetValueOrDefault() > 0)
                {
                    _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={ntdsamtv}");
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
        protected override void Dispose(bool disposing)
        {
            if (disposing && !Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }


        // POST: odata/HMArrivals(key)/BillLogs
        [AcceptVerbs("POST")]
        [ODataRoute("HMArrivals({key})/ArrivalLogs")]
        public async Task<IHttpActionResult> PostHMArrivalLogs([FromODataUri]long key, [FromBody] HMArrivalLog billlog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            if (!await _repo.Queryable().AnyAsync(x=>x.Id==key))
            {
                return NotFound();
            }
            billlog.HMArrivalId = key;
            billlog.ObjectState = ObjectState.Added;

            try
            {
                _logRepo.Insert(billlog);
                await uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
            }
            catch
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                throw;
            }

            return Created(billlog);
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var bill = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (bill == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var id = Request.GetKeyFromUri<long>(link);
                switch (navigationProperty)
                {
                    case "fk_TDSVoucher":
                        bill.TDSVoucherId = id;
                        bill.ObjectState = ObjectState.Modified;
                        break;
                    case "fk_TaxableAmtVoucher":
                        bill.TaxableAmtVoucherId = id;
                        bill.ObjectState = ObjectState.Modified;
                        break;
                    case "fk_NonTaxAmtVoucher":
                        bill.NonTaxAmtVoucherId = id;
                        bill.ObjectState = ObjectState.Modified;
                        break;
                    default:
                        if (!Request.IsBatchRequest())
                        {
                            uow.Rollback();
                        }
                        return StatusCode(HttpStatusCode.NotImplemented);
                }
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

        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            var bill = _repo.Queryable().SingleOrDefault(p => p.Id == key);
            if (bill == null)
            {
                return NotFound();
            }
            try
            {
                switch (navigationProperty)
                {
                    case "fk_TDSVoucher":
                        long? tdsvoucherid = 0;
                        tdsvoucherid = bill.TDSVoucherId;
                        bill.fk_TDSVoucher = null;
                        bill.TDSVoucherId = null;
                        await uow.SaveChangesAsync();
                        if (tdsvoucherid.GetValueOrDefault() > 0)
                        {
                            _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={tdsvoucherid}");
                        }

                        break;
                    case "fk_TaxableAmtVoucher":
                        long? taxAmtvoucherid = 0;
                        taxAmtvoucherid = bill.TaxableAmtVoucherId;
                        bill.fk_TaxableAmtVoucher = null;
                        bill.TaxableAmtVoucherId = null;
                        await uow.SaveChangesAsync();
                        if (taxAmtvoucherid.GetValueOrDefault() > 0)
                        {
                            _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={taxAmtvoucherid}");
                        }
                        break;
                    case "fk_NonTaxAmtVoucher":
                        long? NontaxAmtvoucherid = 0;
                        NontaxAmtvoucherid = bill.NonTaxAmtVoucherId;
                        bill.fk_NonTaxAmtVoucher = null;
                        bill.NonTaxAmtVoucherId = null;
                        await uow.SaveChangesAsync();
                        if (NontaxAmtvoucherid.GetValueOrDefault() > 0)
                        {
                            _repo.ExecuteSql($"DELETE FROM tVouchers WHERE Id={NontaxAmtvoucherid}");
                        }
                        break;
                    default:
                        if (!Request.IsBatchRequest())
                        {
                            uow.Rollback();
                        }
                        return StatusCode(HttpStatusCode.NotImplemented);
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
    }
}