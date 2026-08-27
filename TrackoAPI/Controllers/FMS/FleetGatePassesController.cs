using System;
using System.Data;
using System.Data.Entity;
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
    public class FleetGatePassesController : ODataController
    //ODataController
    {
        private readonly IFleetGatePassService _service;

        public FleetGatePassesController(IFleetGatePassService service)
        {
            _service = service;
        }
        // GET: odata/FleetGatePasss
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<FleetGatePass> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/FleetGatePasss(5)
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<FleetGatePass> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/FleetGatePasss(5)
        public async Task<IHttpActionResult> Put(long key, FleetGatePass objFleetGatePass)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objFleetGatePass.Id)
            {
                return BadRequest();
            }
            objFleetGatePass.ObjectState = ObjectState.Modified;
            _service.Update(objFleetGatePass);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objFleetGatePass);
        }

        // POST: odata/FleetGatePasss
        public async Task<IHttpActionResult> Post(FleetGatePass objFleetGatePass)
        {
            objFleetGatePass.ObjectState = ObjectState.Added;
            _service.Insert(objFleetGatePass);
            await Request.GetContext().SaveChangesAsync();
            return Created(objFleetGatePass);
        }
        //// PATCH: odata/FleetGatePasss(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<FleetGatePass> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            FleetGatePass objFleetGatePass = await _service.FindAsync(key);
            if (objFleetGatePass == null)
            {
                return NotFound();
            }
            objFleetGatePass.ObjectState = ObjectState.Modified;
            patch.Patch(objFleetGatePass);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objFleetGatePass);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            if(!Request.IsBatchRequest())Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
            var objFleetGatePass = await _service.FindAsync(key);
            if (objFleetGatePass == null)
            {
                return NotFound();
            }
            await
                Request.GetContext()
                                .Context.Database.ExecuteSqlCommandAsync(
                                    $"UPDATE dbo.tTyreLog SET GatePassId=NULL WHERE GatePassId IS NOT NULL AND GatePassId={key}");
            await
                Request.GetContext()
                                .Context.Database.ExecuteSqlCommandAsync(
                                    $"UPDATE dbo.tBatteryLog SET GatePassId=NULL WHERE GatePassId IS NOT NULL AND GatePassId={key}");
            await
                Request.GetContext()
                                .Context.Database.ExecuteSqlCommandAsync(
                                    $"UPDATE dbo.tSpareLog SET GatePassId=NULL WHERE GatePassId IS NOT NULL AND GatePassId={key}");
            objFleetGatePass.ObjectState = ObjectState.Deleted;
            _service.Delete(objFleetGatePass);
            await Request.GetContext().SaveChangesAsync();
            if (!Request.IsBatchRequest())
                Request.GetContext().Commit();
            return StatusCode(HttpStatusCode.NoContent);
        }
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        [FromODataUri] string relatedKey, string navigationProperty)
        {
            var supplier = await _service.Queryable().AnyAsync(p => p.Id == key);
            if (!supplier)
            {
                return StatusCode(HttpStatusCode.NotFound);
            }
            var id = Convert.ToInt32(relatedKey);
            switch (navigationProperty)
            {
                case "Tyres":

                    var tyreResult =
                        await
                            Request.GetContext()
                                .Context.Database.ExecuteSqlCommandAsync(
                                    $"UPDATE dbo.tTyreLog SET GatePassId=NULL WHERE Id={id}");
                    return StatusCode(tyreResult > 0 ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
                case "Spares":

                    var spareResult =
                        await
                            Request.GetContext()
                                .Context.Database.ExecuteSqlCommandAsync(
                                    $"UPDATE dbo.tSpareLog SET GatePassId=NULL WHERE Id={id}");
                    return StatusCode(spareResult > 0 ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
                case "Batteries":

                    var batteryResult =
                        await
                            Request.GetContext()
                                .Context.Database.ExecuteSqlCommandAsync(
                                    $"UPDATE dbo.tBatteryLog SET GatePassId=NULL WHERE Id={id}");
                    return StatusCode(batteryResult > 0 ? HttpStatusCode.NoContent : HttpStatusCode.NotFound);
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);

            }
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