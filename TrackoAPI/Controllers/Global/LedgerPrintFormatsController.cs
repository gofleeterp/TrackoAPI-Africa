using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class LedgerPrintFormatsController : ODataController
    {
        private readonly ILedgerPrintFormatService _service;

        public LedgerPrintFormatsController(ILedgerPrintFormatService service)
        {
            _service = service;
        }
        // GET: odata/LedgerPrintFormats
        [HttpGet, EnableQuery]
        public IQueryable<LedgerPrintFormat> Get()
        {
            return _service.Queryable();
        }

        // GET: odata/LedgerPrintFormats(5)
        [EnableQuery]
        public SingleResult<LedgerPrintFormat> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/LedgerPrintFormats(5)
        public async Task<IHttpActionResult> Put(long key, LedgerPrintFormat objLedgerPrintFormat)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objLedgerPrintFormat.Id)
            {
                return BadRequest();
            }
            objLedgerPrintFormat.ObjectState = ObjectState.Modified;
            _service.Update(objLedgerPrintFormat);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LedgerPrintFormatExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLedgerPrintFormat);
        }
        // POST: odata/LedgerPrintFormats
        public async Task<IHttpActionResult> Post(LedgerPrintFormat objLedgerPrintFormat)
        {
            objLedgerPrintFormat.ObjectState = ObjectState.Added;
            _service.Insert(objLedgerPrintFormat);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LedgerPrintFormatExists(objLedgerPrintFormat.PrintFormatId, objLedgerPrintFormat.LedgerId, objLedgerPrintFormat.OfficeId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objLedgerPrintFormat);
        }

        

        //// PATCH: odata/LedgerPrintFormats(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<LedgerPrintFormat> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            LedgerPrintFormat objLedgerPrintFormat = await _service.FindAsync(key);
            if (objLedgerPrintFormat == null)
            {
                return NotFound();
            }
            objLedgerPrintFormat.ObjectState = ObjectState.Modified;
            patch.Patch(objLedgerPrintFormat);
            _service.Patch(objLedgerPrintFormat);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LedgerPrintFormatExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLedgerPrintFormat);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objLedgerPrintFormat = await _service.FindAsync(key);
            if (objLedgerPrintFormat == null)
            {
                return NotFound();
            }
            objLedgerPrintFormat.ObjectState = ObjectState.Deleted;
            _service.Delete(objLedgerPrintFormat);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LedgerPrintFormatExists(long printFormatId, long? ledgerId, long? officeId)
        {
            return _service.Query(e => e.PrintFormatId == printFormatId && e.LedgerId == ledgerId && e.OfficeId == officeId).Select().Any();
        }
        private bool LedgerPrintFormatExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
    }
}