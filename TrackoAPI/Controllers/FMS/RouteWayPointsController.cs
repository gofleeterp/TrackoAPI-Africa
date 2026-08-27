using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
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
    public class RouteWayPointsController : ODataController
    //ODataController
    {
        private readonly IRouteWayPointService _service;

        public RouteWayPointsController(IRouteWayPointService service)
        {
            _service = service;
        }
        // GET: odata/RouteWayPoints
        [HttpGet, EnableQuery]
        public IQueryable<RouteWayPoint> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/RouteWayPoints(5)
        [EnableQuery]
        public SingleResult<RouteWayPoint> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/RouteWayPoints(5)
        public async Task<IHttpActionResult> Put(long key, RouteWayPoint objRouteWayPoint)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objRouteWayPoint.Id)
            {
                return BadRequest();
            }
            objRouteWayPoint.ObjectState = ObjectState.Modified;
            _service.Update(objRouteWayPoint);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WayPointExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objRouteWayPoint);
        }
        // POST: odata/RouteWayPoints
        public async Task<IHttpActionResult> Post(RouteWayPoint objRouteWayPoint)
        {
            objRouteWayPoint.ObjectState = ObjectState.Added;
            _service.Insert(objRouteWayPoint);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (WayPointExists(objRouteWayPoint.RouteId,objRouteWayPoint.CityId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objRouteWayPoint);
        }
        //// PATCH: odata/RouteWayPoints(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<RouteWayPoint> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RouteWayPoint objRouteWayPoint = await _service.FindAsync(key);
            if (objRouteWayPoint == null)
            {
                return NotFound();
            }
            objRouteWayPoint.ObjectState = ObjectState.Modified;
            patch.Patch(objRouteWayPoint);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!WayPointExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objRouteWayPoint);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objWayPoint = await _service.FindAsync(key);
            if (objWayPoint == null)
            {
                return NotFound();
            }
            objWayPoint.ObjectState = ObjectState.Deleted;
            _service.Delete(objWayPoint);
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

        private bool WayPointExists(long routeId,long cityId)
        {
            return _service.Query(e => e.RouteId == routeId&&e.CityId==cityId).Select().Any();
        }
        private bool WayPointExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
    }
}