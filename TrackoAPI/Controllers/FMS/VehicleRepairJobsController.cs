using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleRepairJobsController : ODataController
    {
        private readonly IVehicleRepairJobService _service;

        public VehicleRepairJobsController(IVehicleRepairJobService service)
        {
            _service = service;
        }
        // GET: odata/PMMasters
        [HttpGet, EnableQuery]
        public IQueryable<VehicleRepairJob> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/PMMasters(5)
        [EnableQuery]
        public SingleResult<VehicleRepairJob> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PMMasters(5)
        public async Task<IHttpActionResult> Put(long key, VehicleRepairJob objPmMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objPmMaster.Id)
            {
                return BadRequest();
            }
            objPmMaster.ObjectState = ObjectState.Modified;
            _service.Update(objPmMaster);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!PMMasterExists(key))
                //{
                //    return NotFound();
                //}
                throw;
            }

            return Updated(objPmMaster);
        }
        // POST: odata/PMMasters
        public async Task<IHttpActionResult> Post(VehicleRepairJob objPmMaster)
        {
            objPmMaster.ObjectState = ObjectState.Added;
            _service.Insert(objPmMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                //if (PMMasterExists(objPmMaster.Name))
                //{
                //    return Conflict();
                //}
                throw;
            }
            return Created(objPmMaster);
        }
        //// PATCH: odata/PMMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleRepairJob> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleRepairJob objPmMaster = await _service.FindAsync(key);
            if (objPmMaster == null)
            {
                return NotFound();
            }
            objPmMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objPmMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!PMMasterExists(key))
                //{
                //    return NotFound();
                //}
                throw;
            }

            return Updated(objPmMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objPmMaster = await _service.FindAsync(key);
            if (objPmMaster == null)
            {
                return NotFound();
            }
            objPmMaster.ObjectState = ObjectState.Deleted;
            _service.Delete(objPmMaster);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("POST", "PUT")]//For Single Navigation Properties
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty,
           [FromBody] Uri link)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var _uow = Request.GetContext();
            VehicleRepairJob pmlog = await _service.FindAsync(key);
            if (pmlog == null)
            {
                return NotFound();
            }
            long navigationkey = Request.GetKeyFromUri<long>(link);

            switch (navigationProperty)
            {
                case "fk_JobCard":
                    VehicleMovementLog jobcard = await _uow.RepositoryAsync<VehicleMovementLog>().FindAsync(navigationkey);
                    if (jobcard == null)
                    {
                        return NotFound();
                    }
                    pmlog.JobCardId = jobcard.Id;
                    pmlog.fk_JobCard = jobcard;
                    pmlog.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    break;
                case "fk_Driver":
                    DriverMaster driver = await _uow.RepositoryAsync<DriverMaster>().FindAsync(navigationkey);
                    if (driver == null)
                    {
                        return NotFound();
                    }
                    pmlog.DriverId = driver.Id;
                    pmlog.fk_Driver = driver;
                    pmlog.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    break;
                case "fk_Mechanic":
                    GenericMaster mechanic = await _uow.RepositoryAsync<GenericMaster>().FindAsync(navigationkey);
                    if (mechanic == null)
                    {
                        return NotFound();
                    }
                    pmlog.MechanicId = mechanic.Id;
                    pmlog.fk_Mechanic = mechanic;
                    pmlog.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    break;
                case "fk_JobGroup":
                    GenericMaster jobgroup = await _uow.RepositoryAsync<GenericMaster>().FindAsync(navigationkey);
                    if (jobgroup == null)
                    {
                        return NotFound();
                    }
                    pmlog.JobGroupId = jobgroup.Id;
                    pmlog.fk_JobGroup = jobgroup;
                    pmlog.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    break;
                default:
                    return NotFound();
            }
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