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
    public class PrintFormatsController : ODataController
    {
        private readonly IPrintFormatService _service;

        public PrintFormatsController(IPrintFormatService service)
        {
            _service = service;
        }
        // GET: odata/PrintFormatMasters
        [HttpGet, EnableQuery]
        public IQueryable<PrintFormatMaster> Get()
        {
            return _service.Queryable();
        }

        [HttpGet]
        public IQueryable<PrintFormatDataSource> GetDataSources([FromODataUri] long key)
        {
            return _service.Queryable().Where(x => x.Id == key).Select(x => x.DataSources).SelectMany(x => x, ((list, source) => source));
        }
        [HttpGet]
        public IQueryable<LedgerPrintFormat> GetLedgerPrintFormats([FromODataUri] long key)
        {
            return _service.Queryable().Where(x => x.Id == key).Select(x => x.LedgerPrintFormats).SelectMany(x => x, ((list, source) => source));
        }
        // GET: odata/PrintFormatMasters(5)
        [EnableQuery]
        public SingleResult<PrintFormatMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PrintFormatMasters(5)
        public async Task<IHttpActionResult> Put(long key, PrintFormatMaster objPrintFormatMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objPrintFormatMaster.Id)
            {
                return BadRequest();
            }
            objPrintFormatMaster.ObjectState = ObjectState.Modified;
            _service.Update(objPrintFormatMaster);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PrintFormatMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objPrintFormatMaster);
        }
        // POST: odata/PrintFormatMasters
        public async Task<IHttpActionResult> Post(PrintFormatMaster objPrintFormatMaster)
        {
            objPrintFormatMaster.ObjectState = ObjectState.Added;
            _service.Insert(objPrintFormatMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PrintFormatMasterExists(objPrintFormatMaster.DisplayText))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objPrintFormatMaster);
        }
        //// PATCH: odata/PrintFormatMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PrintFormatMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PrintFormatMaster objPrintFormatMaster = await _service.FindAsync(key);
            if (objPrintFormatMaster == null)
            {
                return NotFound();
            }
            objPrintFormatMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objPrintFormatMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PrintFormatMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objPrintFormatMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objPrintFormatMaster = await _service.FindAsync(key);
            if (objPrintFormatMaster == null)
            {
                return NotFound();
            }
            objPrintFormatMaster.ObjectState = ObjectState.Deleted;
            _service.Delete(objPrintFormatMaster);
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

        private bool PrintFormatMasterExists(string formatName)
        {
            return _service.Query(e => e.DisplayText == formatName).Select().Any();
        }
        private bool PrintFormatMasterExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
    }
}