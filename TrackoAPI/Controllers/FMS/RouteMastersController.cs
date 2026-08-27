using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class RouteMastersController : ODataController
    //ODataController
    {
        private readonly IRouteMasterService _service;

        public RouteMastersController(IRouteMasterService service)
        {
            _service = service;
        }
        // GET: odata/RouteMasters
        [HttpGet, EnableQuery]
        public IQueryable<RouteMaster> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/RouteMasters(5)
        [EnableQuery]
        public SingleResult<RouteMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/RouteMasters(5)
        public async Task<IHttpActionResult> Put(long key, RouteMaster objRouteMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objRouteMaster.Id)
            {
                return BadRequest();
            }
            objRouteMaster.ObjectState = ObjectState.Modified;
            _service.Update(objRouteMaster);

            try
            {
                var dt = objRouteMaster.Data ?? new List<JsonDataEntity>();
                if (dt.Any())
                {
                    objRouteMaster.ExtraProperties = JsonConvert.SerializeObject(dt);
                }

                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RouteMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            
            return Updated(objRouteMaster);
        }
        // POST: odata/RouteMasters
        public async Task<IHttpActionResult> Post(RouteMaster objRouteMaster)
        {
            if (RouteMasterExists(objRouteMaster.Name))
            {
                throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
            }

            objRouteMaster.ObjectState = ObjectState.Added;
            
            _service.Insert(objRouteMaster);
            try
            {
                var dt = objRouteMaster.Data ?? new List<JsonDataEntity>();
                if (dt.Any())
                {
                    objRouteMaster.ExtraProperties = JsonConvert.SerializeObject(dt);
                }
                await Request.GetContext().SaveChangesAsync();
            }
            catch (Exception ex)
            {
                    throw new BusinessException(ErrorCode.GLB104, ex.InnerException.Message);               
            }
            
            return Created(objRouteMaster);
        }
        //// PATCH: odata/RouteMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<RouteMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            RouteMaster objRouteMaster = await _service.FindAsync(key);
            if (objRouteMaster == null)
            {
                return NotFound();
            }
            objRouteMaster.ObjectState = ObjectState.Modified;            
            patch.Patch(objRouteMaster);
            try
            {
                var dt = objRouteMaster.Data ?? new List<JsonDataEntity>();
                if (dt.Any())
                {
                    objRouteMaster.ExtraProperties = JsonConvert.SerializeObject(dt);
                }

                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RouteMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            
            return Updated(objRouteMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objRouteMaster = await _service.FindAsync(key);
            if (objRouteMaster == null)
            {
                return NotFound();
            }
            objRouteMaster.ObjectState = ObjectState.Deleted;
            _service.Delete(objRouteMaster);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing&&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool RouteMasterExists(string routeName)
        {
            return _service.Query(e => e.Name == routeName).Select().Any();
        }
        private bool RouteMasterExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
        // POST: odata/RouteMasters(key)/AllowedVehicleTypes
        [AcceptVerbs("POST")]
        [ODataRoute("RouteMasters({key})/AllowedVehicleTypes")]
        public async Task<IHttpActionResult> PostAllowedVehicleTypes([FromODataUri]long key, [FromBody] RouteVehicleType vehicletype)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var route = await _service.FindAsync(key);
            if (route == null)
            {
                return NotFound();
            }
            vehicletype.RouteId = key;
            route.ObjectState = ObjectState.Modified;
            vehicletype.ObjectState = ObjectState.Added;
            route.AllowedVehicleTypes.Add(vehicletype);
            await uow.SaveChangesAsync();

            return Created(vehicletype);
        }
        // POST: odata/RouteMasters(key)/WayPoints
        [AcceptVerbs("POST")]
        [ODataRoute("RouteMasters({key})/ChildRoutes")]
        public async Task<IHttpActionResult> PostChildRoutes([FromODataUri]long key, [FromBody] ChildParentRoute child)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var route = await _service.FindAsync(key);
            if (route == null)
            {
                return NotFound();
            }
            
            route.ObjectState = ObjectState.Modified;
            child.ObjectState = ObjectState.Added;
            route.ChildRoutes.Add(child);
            await uow.SaveChangesAsync();

            return Created(child);
        }
        // POST: odata/RouteMasters(key)/WayPoints
        [AcceptVerbs("POST")]
        [ODataRoute("RouteMasters({key})/WayPoints")]
        public async Task<IHttpActionResult> PostWayPoints([FromODataUri]long key, [FromBody] RouteWayPoint routeWayPoint)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var route = await _service.Queryable().Include(x=>x.WayPoints).FirstOrDefaultAsync(x=>x.Id==key);
            if (route == null)
            {
                return NotFound();
            }
            
            routeWayPoint.RouteId = key;
            route.ObjectState=ObjectState.Modified;
            routeWayPoint.ObjectState=ObjectState.Added;
            
            try
            {
                route.WayPoints.Add(routeWayPoint);
                await uow.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (await WayPointExists(routeWayPoint.RouteId, routeWayPoint.CityId))
                {
                    return BadRequest("City Already Mapped to this Route");
                }
            }
            
            return Created(routeWayPoint);
        }
        // POST: odata/RouteMasters(key)/WayPoints
        [AcceptVerbs("POST")]
        [ODataRoute("RouteMasters({key})/Budgets")]
        public async Task<IHttpActionResult> PostBudgets([FromODataUri]long key, [FromBody] TripExpenseBudget budget)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var route = await _service.Queryable().AnyAsync(x => x.Id == key);
            if (!route)
            {
                return NotFound();
            }

            budget.RouteId = key;
            budget.ObjectState = ObjectState.Added;
            uow.Repository<TripExpenseBudget>().Insert(budget);
            await uow.SaveChangesAsync();

            return Created(budget);
        }
        private async Task<bool> WayPointExists(long routeId, long cityId)
        {
            return await Request.GetContext().RepositoryAsync<RouteWayPoint>().Queryable().AnyAsync(e => e.RouteId == routeId && e.CityId == cityId);
        }

    }
}