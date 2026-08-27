using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    public class PMSchedulesController : ODataController
    //ODataController
    {
        private readonly IPMScheduleService _sc;

        public PMSchedulesController(IPMScheduleService service)
        {
            _sc = service;
        }
        // GET: odata/PMSchedules
        [HttpGet, EnableQuery]
        public IQueryable<PMSchedule> Get()
        {
            return _sc.Queryable();
        }
        // GET: odata/PMSchedules(5)
        [EnableQuery]
        public SingleResult<PMSchedule> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_sc.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PMSchedules(5)
        public async Task<IHttpActionResult> Put(long key, PMSchedule entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            entity.ObjectState = ObjectState.Modified;
            _sc.Update(entity);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PMMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(entity);
        }
        // POST: odata/PMSchedules
        public async Task<IHttpActionResult> Post(PMSchedule entity)
        {
            if (PMMasterExists(entity.ScheduleDate, entity.ClassId, entity.SchedulePMId))
            {
                throw new BusinessException(ErrorCode.GLB104,"Duplicate Record.");
            }
            entity.ObjectState = ObjectState.Added;
            _sc.Insert(entity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PMMasterExists(entity.ScheduleDate,entity.ClassId,entity.SchedulePMId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(entity);
        }
        //// PATCH: odata/PMSchedules(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PMSchedule> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PMSchedule entity = await _sc.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Modified;
            patch.Patch(entity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PMMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(entity);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var entity = await _sc.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Deleted;
            _sc.Delete(entity);
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

        private bool PMMasterExists(DateTime scheduleDate, long classId, long pmId)
        {
            return _sc.Query(e => scheduleDate>= e.ScheduleDate && scheduleDate<=e.ExpiryDate &&e.ClassId==classId&&e.SchedulePMId==pmId).Select().Any();
        }
        private bool PMMasterExists(long key)
        {
            return _sc.Query(e => e.Id == key).Select().Any();
        }
    }
}