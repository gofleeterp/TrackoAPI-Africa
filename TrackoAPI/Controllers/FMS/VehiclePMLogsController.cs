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
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehiclePMLogsController : ODataController
    {
        private readonly IVehiclePMService _service;

        public VehiclePMLogsController(IVehiclePMService service)
        {
            _service = service;
        }
        // GET: odata/PMMasters
        [HttpGet, EnableQuery]
        public IQueryable<VehiclePreventiveLog> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/PMMasters(5)
        [EnableQuery]
        public SingleResult<VehiclePreventiveLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PMMasters(5)
        public async Task<IHttpActionResult> Put(long key, VehiclePreventiveLog objPmMaster)
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
        public async Task<IHttpActionResult> Post(VehiclePreventiveLog objPmMaster)
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
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehiclePreventiveLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehiclePreventiveLog objPmMaster = await _service.FindAsync(key);
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
            VehiclePreventiveLog pmlog = await _service.FindAsync(key);
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
                case "fk_NextLog":
                    VehiclePreventiveLog nextpmlog = await _service.FindAsync(navigationkey);
                    if (nextpmlog == null)
                    {
                        return NotFound();
                    }
                    pmlog.NextLogId = nextpmlog.Id;
                    pmlog.fk_NextLog = nextpmlog;
                    pmlog.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    break;
                case "fk_PreviousLog":
                    VehiclePreventiveLog previouspmlog = await _service.FindAsync(navigationkey);
                    if (previouspmlog == null)
                    {
                        return NotFound();
                    }
                    pmlog.PreviousLogId = previouspmlog.Id;
                    pmlog.fk_PreviousLog = previouspmlog;
                    pmlog.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    break;
                case "fk_NewPMMaster":
                    PMMaster newpmmaster = await _uow.RepositoryAsync<PMMaster>().FindAsync(navigationkey);
                    if (newpmmaster == null)
                    {
                        return NotFound();
                    }
                    pmlog.NewPMId = newpmmaster.Id;
                    pmlog.fk_NewPMMaster = newpmmaster;
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