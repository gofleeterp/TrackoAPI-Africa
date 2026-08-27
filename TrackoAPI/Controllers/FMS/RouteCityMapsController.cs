using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class RouteCityMapsController : ODataController
    //ODataController
    {
        private readonly IRouteCityMapService _objRouteCityMapService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public RouteCityMapsController(IUnitOfWorkAsync unitOfWorkAsync, IRouteCityMapService service)
        {
            _objRouteCityMapService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/RouteCityMaps
        [HttpGet, EnableQuery]
        public IQueryable<RouteCityMap> Get()
        {
            return _objRouteCityMapService.Queryable();
        }
        // GET: odata/RouteCityMaps(5)
        [EnableQuery]
        public SingleResult<RouteCityMap> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objRouteCityMapService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/RouteCityMaps(5)
        public async Task<IHttpActionResult> Put(long key, RouteCityMap objRouteCityMap)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objRouteCityMap.Id)
            {
                return BadRequest();
            }
            objRouteCityMap.ObjectState = ObjectState.Modified;
            _objRouteCityMapService.Update(objRouteCityMap);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objRouteCityMap);
        }
        // POST: odata/RouteCityMaps
        public async Task<IHttpActionResult> Post(RouteCityMap objRouteCityMap)
        {
            objRouteCityMap.ObjectState = ObjectState.Added;
            _objRouteCityMapService.Insert(objRouteCityMap);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objRouteCityMap);
        }
        //// PATCH: odata/RouteCityMaps(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<RouteCityMap> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RouteCityMap objRouteCityMap = await _objRouteCityMapService.FindAsync(key);
            if (objRouteCityMap == null)
            {
                return NotFound();
            }
            objRouteCityMap.ObjectState = ObjectState.Modified;
            patch.Patch(objRouteCityMap);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objRouteCityMap);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objRouteCityMap = await _objRouteCityMapService.FindAsync(key);
            if (objRouteCityMap == null)
            {
                return NotFound();
            }
            objRouteCityMap.ObjectState = ObjectState.Deleted;
            _objRouteCityMapService.Delete(objRouteCityMap);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }
        
    }
}