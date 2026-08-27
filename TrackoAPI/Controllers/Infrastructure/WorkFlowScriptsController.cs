using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Models.Shared;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class WorkFlowScriptsController : ODataController
    //ODataController
    {
        private readonly IWorkFlowScriptService _entityService;

        public WorkFlowScriptsController(IWorkFlowScriptService service)
        {
            _entityService = service;
        }
        // GET: odata/WorkFlowScripts
        [HttpGet, EnableQuery]
        public IQueryable<ApiWorkFlowScript> Get()
        {
            return _entityService.Queryable();
        }
        
        // GET: odata/WorkFlowScripts(5)
        [EnableQuery]
        public SingleResult<ApiWorkFlowScript> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_entityService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/WorkFlowScripts(5)
        public async Task<IHttpActionResult> Put(long key, ApiWorkFlowScript entity)
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
            _entityService.Update(entity);
            await Request.GetContext().SaveChangesAsync();
            return Updated(entity);
        }
        // POST: odata/WorkFlowScripts
        public async Task<IHttpActionResult> Post(ApiWorkFlowScript entity)
        {
            entity.ObjectState = ObjectState.Added;
            _entityService.Insert(entity);
            await Request.GetContext().SaveChangesAsync();
            return Created(entity);
        }
        //// PATCH: odata/WorkFlowScripts(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ApiWorkFlowScript> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApiWorkFlowScript entity = await _entityService.FindAsync(key);
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
            var entity = await _entityService.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Modified;
            entity.Status=MasterStatus.Suspended;
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //if (!Request.IsBatchRequest())
                //{
                //    Request.GetCo
                //}
            }
            base.Dispose(disposing);
        }
    }
}