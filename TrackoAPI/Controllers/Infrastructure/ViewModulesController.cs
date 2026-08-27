using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ViewModulesController : ODataController
    //ODataController
    {
        private readonly IViewModuleService _objCityMasterService;

        public ViewModulesController(IViewModuleService service)
        {
            _objCityMasterService = service;
        }
        // GET: odata/CityMasters
        [HttpGet, EnableQuery]
        public IQueryable<ApiViewModule> Get()
        {
            return _objCityMasterService.Queryable();
        }
        
        // GET: odata/CityMasters(5)
        [EnableQuery]
        public SingleResult<ApiViewModule> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objCityMasterService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/CityMasters(5)
        public async Task<IHttpActionResult> Put(long key, ApiViewModule objCityMaster)
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }
        // POST: odata/CityMasters
        public async Task<IHttpActionResult> Post(ApiViewModule objCityMaster)
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }
        //// PATCH: odata/CityMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ApiViewModule> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApiViewModule entity = await _objCityMasterService.FindAsync(key);
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
            return StatusCode(HttpStatusCode.Forbidden);
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