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
    public class RouteTollMapsController : ODataController
    //ODataController
    {
        private readonly IRouteTollMapService _objRouteTollMapService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public RouteTollMapsController(IUnitOfWorkAsync unitOfWorkAsync, IRouteTollMapService service)
        {
            _objRouteTollMapService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/RouteTollMaps
        [HttpGet, EnableQuery]
        public IQueryable<RouteTollMap> Get()
        {
            return _objRouteTollMapService.Queryable();
        }
        // GET: odata/RouteTollMaps(5)
        [EnableQuery]
        public SingleResult<RouteTollMap> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objRouteTollMapService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/RouteTollMaps(5)
        public async Task<IHttpActionResult> Put(long key, RouteTollMap objRouteTollMap)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objRouteTollMap.Id)
            {
                return BadRequest();
            }
            objRouteTollMap.ObjectState = ObjectState.Modified;
            _objRouteTollMapService.Update(objRouteTollMap);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objRouteTollMap);
        }
        // POST: odata/RouteTollMaps
        public async Task<IHttpActionResult> Post(RouteTollMap objRouteTollMap)
        {
            objRouteTollMap.ObjectState = ObjectState.Added;
            _objRouteTollMapService.Insert(objRouteTollMap);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objRouteTollMap);
        }
        //// PATCH: odata/RouteTollMaps(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<RouteTollMap> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RouteTollMap objRouteTollMap = await _objRouteTollMapService.FindAsync(key);
            if (objRouteTollMap == null)
            {
                return NotFound();
            }
            objRouteTollMap.ObjectState = ObjectState.Modified;
            patch.Patch(objRouteTollMap);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objRouteTollMap);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objRouteTollMap = await _objRouteTollMapService.FindAsync(key);
            if (objRouteTollMap == null)
            {
                return NotFound();
            }
            objRouteTollMap.ObjectState = ObjectState.Deleted;
            _objRouteTollMapService.Delete(objRouteTollMap);
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