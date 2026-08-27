using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class HSAdvancesController : ODataController
    //ODataController
    {
        private readonly IHSAdvanceService _repo;

        public HSAdvancesController(IHSAdvanceService service)
        {
            _repo = service;
        }
        // GET: odata/SpareAliass
        [HttpGet, EnableQuery]
        public IQueryable<HSAdvance> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/SpareAliass(5)
        [EnableQuery]
        public SingleResult<HSAdvance> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        [HttpGet,EnableQuery]
        public IQueryable<HSAdvance> GetUnsettledHSAdvances()
        {
            return _repo.Queryable().Where(x=> (x.Amount - (x.Settlements.Sum(y => (decimal?)y.Amount) == null ? 0 : x.Settlements.Sum(y => (decimal?)y.Amount))) > 0);
        }
        // PUT: odata/SpareAliass(5)
        public async Task<IHttpActionResult> Put(long key, HSAdvance objHSAdvance)
        {
            var uow = Request.GetContext();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objHSAdvance.Id)
            {
                return BadRequest();
            }
            if(objHSAdvance.Amount<=0) return BadRequest("Advance Amount is Zero which is not allowed.");
            if (objHSAdvance.OfficeId.GetValueOrDefault(0) <= 0)
            {
                var officeid =
                uow.RepositoryAsync<Ledger>()
                    .Queryable()
                    .Where(x => x.Id == objHSAdvance.CrAccountId)
                    .Select(x => x.OfficeId)
                    .FirstOrDefault();
                objHSAdvance.OfficeId = officeid;
            }
            objHSAdvance.ObjectState = ObjectState.Modified;
            _repo.Update(objHSAdvance);
            await uow.SaveChangesAsync();
            return Updated(objHSAdvance);
        }
        // POST: odata/SpareAliass
        public async Task<IHttpActionResult> Post(HSAdvance objHSAdvance)
        {
            var uow = Request.GetContext();
            if (objHSAdvance.Amount <= 0) return BadRequest("Advance Amount is Zero which is not allowed.");
            if (objHSAdvance.OfficeId.GetValueOrDefault(0) <= 0)
            {
                var officeid =
                uow.RepositoryAsync<Ledger>()
                    .Queryable()
                    .Where(x => x.Id == objHSAdvance.CrAccountId)
                    .Select(x => x.OfficeId)
                    .FirstOrDefault();
                objHSAdvance.OfficeId = officeid;
            }
            objHSAdvance.ObjectState = ObjectState.Added;
            _repo.Insert(objHSAdvance);
            await uow.SaveChangesAsync();
            return Created(objHSAdvance);
        }
        //// PATCH: odata/SpareAliass(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<HSAdvance> patch)
        {
            var uow = Request.GetContext();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            HSAdvance objHSAdvance = await _repo.FindAsync(key);
            if (objHSAdvance == null)
            {
                return NotFound();
            }
            if (objHSAdvance.OfficeId.GetValueOrDefault(0) <= 0)
            {
                var officeid =
                uow.RepositoryAsync<Ledger>()
                    .Queryable()
                    .Where(x => x.Id == objHSAdvance.CrAccountId)
                    .Select(x => x.OfficeId)
                    .FirstOrDefault();
                objHSAdvance.OfficeId = officeid;
            }
            objHSAdvance.ObjectState = ObjectState.Modified;
            patch.Patch(objHSAdvance);
            if (objHSAdvance.Amount <= 0) return BadRequest("Advance Amount is Zero which is not allowed.");
            await uow.SaveChangesAsync();
            return Updated(objHSAdvance);
        }
        
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objSpareAlias = await _repo.FindAsync(key);
            if (objSpareAlias == null)
            {
                return NotFound();
            }
            objSpareAlias.ObjectState = ObjectState.Deleted;
            _repo.Delete(objSpareAlias);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
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

        [AcceptVerbs("DELETE")]
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
            string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var advance = await _repo.FindAsync(key);
            if (advance == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_Voucher":
                    advance.VoucherId = null;
                    advance.fk_Voucher = null;
                    advance.ObjectState = ObjectState.Modified;
                    break;
                case "fk_VDR":
                    advance.VDRId = null;
                    advance.fk_VDR = null;
                    advance.ObjectState = ObjectState.Modified;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var advance = await _repo.FindAsync(key);
            if (advance == null)
            {
                return NotFound();
            }
            var newrecordid = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_Voucher":
                    if (!uow.RepositoryAsync<Voucher>().Queryable().Any(x => x.Id == newrecordid))
                    {
                        return NotFound();
                    }
                    advance.VoucherId = newrecordid;
                    advance.ObjectState = ObjectState.Modified;
                    break;
                case "fk_VDR":
                    if (!uow.RepositoryAsync<VoucherDetailReference>().Queryable().Any(x => x.Id == newrecordid))
                    {
                        return NotFound();
                    }
                    advance.VDRId = newrecordid;
                    advance.ObjectState = ObjectState.Modified;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}