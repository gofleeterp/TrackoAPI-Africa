using System.Linq;
using System.Net;
using System.Net.Http;
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
    [RoutePrefix("api/ApiRolePermissions")]
    public class RolePermissionsController : ApiController
    //ODataController
    {
        private readonly IRolePermissionService _service;

        public RolePermissionsController(IRolePermissionService service)
        {
            _service = service;
        }
        // GET: odata/CityMasters
        [HttpGet, EnableQuery]
        public IQueryable<ApiRolePermission> Get()
        {
            return _service.Queryable();
        }
        
        // GET: odata/CityMasters(5)
        [EnableQuery]
        public SingleResult<ApiRolePermission> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/CityMasters(5)
        public async Task<IHttpActionResult> Put(long key, ApiRolePermission objCityMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objCityMaster.Id)
            {
                return BadRequest();
            }
            objCityMaster.ObjectState = ObjectState.Modified;
            _service.Update(objCityMaster);
            await Request.GetContext().SaveChangesAsync();

            return Ok(objCityMaster);
        }
        // POST: odata/CityMasters
        public async Task<IHttpActionResult> Post(ApiRolePermission objCityMaster)
        {
            objCityMaster.ObjectState = ObjectState.Added;
            _service.Insert(objCityMaster);
            await Request.GetContext().SaveChangesAsync();
            return Ok(objCityMaster);
        }
        //// PATCH: odata/CityMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ApiRolePermission> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApiRolePermission objCityMaster = await _service.FindAsync(key);
            if (objCityMaster == null)
            {
                return NotFound();
            }
            objCityMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objCityMaster);
            await Request.GetContext().SaveChangesAsync();
            return Ok(objCityMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objCityMaster = await _service.FindAsync(key);
            if (objCityMaster == null)
            {
                return NotFound();
            }
            objCityMaster.ObjectState = ObjectState.Deleted;
            _service.Delete(objCityMaster);
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