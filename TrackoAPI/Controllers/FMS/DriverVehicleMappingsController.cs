using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DriverVehicleMappingsController : ODataController
    //ODataController
    {
        private readonly IDriverVehicleMappingService _service;

        public DriverVehicleMappingsController(IDriverVehicleMappingService service)
        {
            _service = service;
        }
        // GET: odata/DriverVehicleMappings
        [HttpGet, EnableQuery]
        public IQueryable<DriverVehicleMapping> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/DriverVehicleMappings(5)
        [EnableQuery]
        public SingleResult<DriverVehicleMapping> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/DriverVehicleMappings(5)
        public async Task<IHttpActionResult> Put(long key, DriverVehicleMapping entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            if (_service.Queryable().Any(x => x.NextLogId.HasValue && x.Id == key))
            {
                throw new BusinessException(ErrorCode.GLB106, "Only Current Status can be updated.");
            }
            entity.ObjectState = ObjectState.Modified;
            await _service.UpdateAsync(entity);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DriverMapingExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(entity);
        }
        // POST: odata/DriverVehicleMappings
        public async Task<IHttpActionResult> Post(DriverVehicleMapping entity)
        {
            entity.ObjectState = ObjectState.Added;
            await _service.InsertAsync(entity);
            await Request.GetContext().SaveChangesAsync();
            return Created(entity);
        }
        //// PATCH: odata/DriverVehicleMappings(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DriverVehicleMapping> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DriverVehicleMapping entity = await _service.FindAsync(key);

            if (entity == null)
            {
                return NotFound();
            }
            if (entity.NextLogId.HasValue)
            {
                throw new BusinessException(ErrorCode.GLB106, "Only Current Status can be updated.");
            }
            entity.ObjectState = ObjectState.Modified;
            patch.Patch(entity);
            try
            {
                await _service.PatchAsync(entity);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DriverMapingExists(key))
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
            var uow = Request.GetContext();

            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                var entity = await _service.Queryable().Where(x => x.Id == key).FirstOrDefaultAsync();
                if (entity == null)
                {
                    return NotFound();
                }
                if (entity.PreviousLogId > 0)
                {
                    await uow.ExecSqlQueryAsync(
                        "UPDATE [dbo].[tDriverVehicleMapping] SET NextLogId=NULL WHERE NextLogId=@key",
                        new SqlParameter("@key", key));
                }
                if (entity.NextLogId.HasValue)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Only Current Status can be deleted.");
                }
                entity.ObjectState = ObjectState.Deleted;
                _service.Delete(entity);
                await Request.GetContext().SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception e)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            
        }
        [AcceptVerbs("DELETE")]
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] long key,
        string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var dvrmapping = await _service.FindAsync(key);
            if (dvrmapping == null)
            {
                return NotFound();
            }
            switch (navigationProperty)
            {
                case "fk_VTSLog":
                    dvrmapping.VTSLogId = null;
                    dvrmapping.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] long key, string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var dvrmapping = await _service.FindAsync(key);
            if (dvrmapping == null)
            {
                return NotFound();
            }
            var newrecordid = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_VTSLog":                    
                    dvrmapping.VTSLogId = newrecordid;
                    dvrmapping.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
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
        private bool DriverMapingExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
        
    }
}