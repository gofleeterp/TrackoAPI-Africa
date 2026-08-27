using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DriverGuarantorsController : ODataController
    //ODataController
    {
        private readonly IDriverGuarantorService _objDriverGuarantorService;

        public DriverGuarantorsController(IDriverGuarantorService service)
        {
            _objDriverGuarantorService = service;
        }
        // GET: odata/DriverGuarantors
        [HttpGet, EnableQuery]
        public IQueryable<DriverGuarantor> Get()
        {
            return _objDriverGuarantorService.Queryable();
        }
        // GET: odata/DriverGuarantors(5)
        [EnableQuery]
        public SingleResult<DriverGuarantor> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objDriverGuarantorService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/DriverGuarantors(5)
        public async Task<IHttpActionResult> Put(long key, DriverGuarantor objDriverGuarantor)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDriverGuarantor.Id)
            {
                return BadRequest();
            }
            objDriverGuarantor.ObjectState = ObjectState.Modified;
            _objDriverGuarantorService.Update(objDriverGuarantor);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objDriverGuarantor);
        }
        // POST: odata/DriverGuarantors
        public async Task<IHttpActionResult> Post(DriverGuarantor objDriverGuarantor)
        {
            objDriverGuarantor.ObjectState = ObjectState.Added;
            _objDriverGuarantorService.Insert(objDriverGuarantor);
            await Request.GetContext().SaveChangesAsync();
            return Created(objDriverGuarantor);
        }
        //// PATCH: odata/DriverGuarantors(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DriverGuarantor> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DriverGuarantor objDriverGuarantor = await _objDriverGuarantorService.FindAsync(key);
            if (objDriverGuarantor == null)
            {
                return NotFound();
            }
            objDriverGuarantor.ObjectState = ObjectState.Modified;
            patch.Patch(objDriverGuarantor);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objDriverGuarantor);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDriverGuarantor = await _objDriverGuarantorService.FindAsync(key);
            if (objDriverGuarantor == null)
            {
                return NotFound();
            }
            objDriverGuarantor.ObjectState = ObjectState.Deleted;
            _objDriverGuarantorService.Delete(objDriverGuarantor);
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
    }
}