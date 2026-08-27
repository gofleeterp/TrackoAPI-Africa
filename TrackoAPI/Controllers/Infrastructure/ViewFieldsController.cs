using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ViewFieldsController : ODataController
    //ODataController
    {
        private readonly IViewFieldService _service;
        public ViewFieldsController(IViewFieldService service)
        {
            _service = service;
        }
        // GET: odata/CityMasters
        [HttpGet, EnableQuery]
        public IQueryable<ViewField> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/CityMasters(5)
        [EnableQuery]
        public SingleResult<ViewField> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/CityMasters(5)
        public async Task<IHttpActionResult> Put(long key, ViewField entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            entity.ObjectState = ObjectState.Modified;
            _service.Update(entity);
            await Request.GetContext().SaveChangesAsync();

            return Updated(entity);
        }
        // POST: odata/CityMasters
        public async Task<IHttpActionResult> Post(ViewField entity)
        {
            entity.ObjectState = ObjectState.Added;
            _service.Insert(entity);
            await Request.GetContext().SaveChangesAsync();
            return Created(entity);
        }
        //// PATCH: odata/CityMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ViewField> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ViewField entity = await _service.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Modified;
            patch.Patch(entity);
            await Request.GetContext().SaveChangesAsync();

            return Updated(entity);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            throw new BusinessException(ErrorCode.GLB107,"View Field Delete Not Allowed.");
            var entity = await _service.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Deleted;
            _service.Delete(entity);
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
    }
}