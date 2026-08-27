using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DriverRelativesController : ODataController
    //ODataController
    {
        private readonly IDriverRelativeService _objDriverRelativeService;

        public DriverRelativesController(IDriverRelativeService service)
        {
            _objDriverRelativeService = service;
        }
        // GET: odata/DriverRelatives
        [HttpGet, EnableQuery]
        public IQueryable<DriverRelative> Get()
        {
            return _objDriverRelativeService.Queryable();
        }
        // GET: odata/DriverRelatives(5)
        [EnableQuery]
        public SingleResult<DriverRelative> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objDriverRelativeService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/DriverRelatives(5)
        public async Task<IHttpActionResult> Put(long key, DriverRelative objDriverRelative)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDriverRelative.Id)
            {
                return BadRequest();
            }
            objDriverRelative.ObjectState = ObjectState.Modified;
            _objDriverRelativeService.Update(objDriverRelative);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objDriverRelative);
        }
        // POST: odata/DriverRelatives
        public async Task<IHttpActionResult> Post(DriverRelative objDriverRelative)
        {
            objDriverRelative.ObjectState = ObjectState.Added;
            _objDriverRelativeService.Insert(objDriverRelative);
            await Request.GetContext().SaveChangesAsync();
            return Created(objDriverRelative);
        }
        //// PATCH: odata/DriverRelatives(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DriverRelative> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DriverRelative objDriverRelative = await _objDriverRelativeService.FindAsync(key);
            if (objDriverRelative == null)
            {
                return NotFound();
            }
            objDriverRelative.ObjectState = ObjectState.Modified;
            patch.Patch(objDriverRelative);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objDriverRelative);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDriverRelative = await _objDriverRelativeService.FindAsync(key);
            if (objDriverRelative == null)
            {
                return NotFound();
            }
            objDriverRelative.ObjectState = ObjectState.Deleted;
            _objDriverRelativeService.Delete(objDriverRelative);
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