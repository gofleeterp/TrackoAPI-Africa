using Microsoft.TeamFoundation.SourceControl.WebApi.Legacy;

using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.vw.ts;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VoucherDetailReferencesController : ODataController
    //ODataController
    {
        private readonly IVoucherDetailReferenceService _service;
        public VoucherDetailReferencesController(IVoucherDetailReferenceService service)
        {
            _service = service;
        }
        // GET: odata/VoucherDetailReferences
        [HttpGet, EnableQuery]
        public IQueryable<VoucherDetailReference> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/VoucherDetailReferences(5)
        [EnableQuery]
        public SingleResult<VoucherDetailReference> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VoucherDetailReferences(5)
        public async Task<IHttpActionResult> Put(long key, VoucherDetailReference entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            entity.CurTypeId = entity.fk_VoucherDetail.CurTypeId;
            entity.CurRate = entity.fk_VoucherDetail.CurRate;

            entity.ObjectState = ObjectState.Modified;
            _service.Update(entity);
            await Request.GetContext().SaveChangesAsync();

            return Updated(entity);
        }
        // POST: odata/VoucherDetailReferences
        public async Task<IHttpActionResult> Post(VoucherDetailReference entity)
        {
            entity.ObjectState = ObjectState.Added;

            entity.CurTypeId = entity.fk_VoucherDetail.CurTypeId;
            entity.CurRate = entity.fk_VoucherDetail.CurRate;

            #region Currency Conversion
            try
            {
                entity.Amount_MNC = entity.Amount * (1 * entity.CurRate);
            }
            catch { }
            #endregion

            _service.Insert(entity);
            await Request.GetContext().SaveChangesAsync();
            return Created(entity);
        }
        //// PATCH: odata/VoucherDetailReferences(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VoucherDetailReference> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VoucherDetailReference entity = await _service.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.CurTypeId = entity.fk_VoucherDetail.CurTypeId;
            entity.CurRate = entity.fk_VoucherDetail.CurRate;

            entity.ObjectState = ObjectState.Modified;
            patch.Patch(entity);
            await Request.GetContext().SaveChangesAsync();

            return Updated(entity);
        }
        //POST:odata/VehicleMovementLogs(key)/TripExpenses
        [ODataRoute("VoucherDetailReferences({key})/AgainstReferences")]
        public async Task<IHttpActionResult> PostAgainstReferences([FromODataUri] long key, [FromBody] VoucherDetailReference child)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!await _service.Queryable().AnyAsync(x => x.Id == key))
            {
                return BadRequest("Parent VDR Not Found");
            }
            var uow = Request.GetContext();
            child.ObjectState = ObjectState.Added;
            child.RefId = key;
            _service.Insert(child);
            await uow.SaveChangesAsync();
            return Created(child);
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] long key, string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var vdr = await _service.FindAsync(key);
            if (vdr == null)
            {
                return NotFound();
            }
            var nrecordid = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("DELETE")]
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var vdr = await _service.FindAsync(key);
            if (vdr == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_ParentReference":
                    vdr.RefId = null;
                    vdr.fk_ParentReference = null;
                    vdr.ObjectState = ObjectState.Modified;
                    break;                    
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVoucherDetailReference = await _service.FindAsync(key);
            if (objVoucherDetailReference == null)
            {
                return NotFound();
            }
            objVoucherDetailReference.ObjectState = ObjectState.Deleted;
            _service.Delete(objVoucherDetailReference);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        [HttpGet, EnableQuery]
        public IQueryable<VDRBalance> GetPendingReferences()
        {
            return Request.GetContext().Context.VDRBalances.AsQueryable();
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