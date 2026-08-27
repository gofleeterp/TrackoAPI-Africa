using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DriverPaymentsController : ODataController
    //ODataController
    {
        private readonly IDriverPaymentService _objDriverPaymentService;

        public DriverPaymentsController(IDriverPaymentService service)
        {
            _objDriverPaymentService = service;
        }
        // GET: odata/DriverPayments
        [HttpGet, EnableQuery]
        public IQueryable<DriverPayment> Get()
        {
            return _objDriverPaymentService.Queryable();
        }
        // GET: odata/DriverPayments(5)
        [EnableQuery]
        public SingleResult<DriverPayment> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objDriverPaymentService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/DriverPayments(5)
        public async Task<IHttpActionResult> Put(long key, DriverPayment objDriverPayment)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDriverPayment.Id)
            {
                return BadRequest();
            }
            objDriverPayment.ObjectState = ObjectState.Modified;
            _objDriverPaymentService.Update(objDriverPayment);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DriverPaymentExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDriverPayment);
        }
        // POST: odata/DriverPayments
        public async Task<IHttpActionResult> Post(DriverPayment objDriverPayment)
        {
            objDriverPayment.ObjectState = ObjectState.Added;
            _objDriverPaymentService.Insert(objDriverPayment);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DriverPaymentExists(objDriverPayment.ReferenceNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objDriverPayment);
        }
        //// PATCH: odata/DriverPayments(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DriverPayment> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DriverPayment objDriverPayment = await _objDriverPaymentService.FindAsync(key);
            if (objDriverPayment == null)
            {
                return NotFound();
            }
            objDriverPayment.ObjectState = ObjectState.Modified;
            patch.Patch(objDriverPayment);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DriverPaymentExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDriverPayment);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDriverPayment = await _objDriverPaymentService.FindAsync(key);
            if (objDriverPayment == null)
            {
                return NotFound();
            }
            objDriverPayment.ObjectState = ObjectState.Deleted;
            _objDriverPaymentService.Delete(objDriverPayment);
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

        private bool DriverPaymentExists(string referenceNo)
        {
            return _objDriverPaymentService.Query(e => e.ReferenceNo == referenceNo).Select().Any();
        }
        private bool DriverPaymentExists(long key)
        {
            return _objDriverPaymentService.Query(e => e.Id == key).Select().Any();
        }
    }
}