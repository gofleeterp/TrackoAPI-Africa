using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class RouteVehicleTypesController : ODataController
    //ODataController
    {
        private readonly IRouteVehicleTypeService _repo;

        public RouteVehicleTypesController(IRouteVehicleTypeService service)
        {
            _repo = service;
        }
        // GET: odata/RouteVehicleTypes
        [HttpGet, EnableQuery]
        public IQueryable<RouteVehicleType> Get() => _repo.Queryable();

        // GET: odata/RouteVehicleTypes(5)
        [EnableQuery]
        public SingleResult<RouteVehicleType> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));

        // PUT: odata/RouteVehicleTypes(5)
        public async Task<IHttpActionResult> Put(long key, RouteVehicleType objRouteVehicleType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objRouteVehicleType.Id)
            {
                return BadRequest();
            }
            objRouteVehicleType.ObjectState = ObjectState.Modified;
            _repo.Update(objRouteVehicleType);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {               
                throw;
            }

            return Updated(objRouteVehicleType);
        }
        // POST: odata/RouteVehicleTypes
        public async Task<IHttpActionResult> Post(RouteVehicleType objRouteVehicleType)
        {
            objRouteVehicleType.ObjectState = ObjectState.Added;
            _repo.Insert(objRouteVehicleType);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objRouteVehicleType);
        }
        //// PATCH: odata/RouteVehicleTypes(5)
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<RouteVehicleType> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RouteVehicleType objRouteVehicleType = await _repo.FindAsync(key);
            if (objRouteVehicleType == null)
            {
                return NotFound();
            }
            objRouteVehicleType.ObjectState = ObjectState.Modified;
            patch.Patch(objRouteVehicleType);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objRouteVehicleType);
        }
        // DELETE: odata/RouteVehicleTypes(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objRouteVehicleType = await _repo.FindAsync(key);
            if (objRouteVehicleType == null)
            {
                return NotFound();
            }
            objRouteVehicleType.ObjectState = ObjectState.Deleted;
            _repo.Delete(objRouteVehicleType);
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
        private bool VehicleTypeExists(long key)
        {
            return _repo.Query(e => e.Id == key).Select().Any();
        }
    }
}