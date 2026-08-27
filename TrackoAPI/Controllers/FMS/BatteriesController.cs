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
    public class BatteriesController : ODataController
    //ODataController
    {
        private readonly IBatteryMasterService _objBatteryMasterService;

        public BatteriesController(IBatteryMasterService service)
        {
            _objBatteryMasterService = service;
        }
        // GET: odata/BatteryMasters
        [HttpGet, EnableQuery]
        public IQueryable<BatteryMaster> Get()
        {
            return _objBatteryMasterService.Queryable();
        }
        // GET: odata/BatteryMasters(5)
        [EnableQuery]
        public SingleResult<BatteryMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objBatteryMasterService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/BatteryMasters(5)
        public async Task<IHttpActionResult> Put(long key, BatteryMaster objBatteryMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objBatteryMaster.Id)
            {
                return BadRequest();
            }
            objBatteryMaster.ObjectState = ObjectState.Modified;
            _objBatteryMasterService.Update(objBatteryMaster);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BatteryMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objBatteryMaster);
        }
        // POST: odata/BatteryMasters
        public async Task<IHttpActionResult> Post(BatteryMaster objBatteryMaster)
        {
            objBatteryMaster.ObjectState = ObjectState.Added;
            _objBatteryMasterService.Insert(objBatteryMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (BatteryMasterExists(objBatteryMaster.BatterySerialNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objBatteryMaster);
        }
        //// PATCH: odata/BatteryMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<BatteryMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BatteryMaster objBatteryMaster = await _objBatteryMasterService.FindAsync(key);
            if (objBatteryMaster == null)
            {
                return NotFound();
            }
            objBatteryMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objBatteryMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BatteryMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objBatteryMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objBatteryMaster = await _objBatteryMasterService.FindAsync(key);
            if (objBatteryMaster == null)
            {
                return NotFound();
            }
            objBatteryMaster.ObjectState = ObjectState.Deleted;
            _objBatteryMasterService.Delete(objBatteryMaster);
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

        private bool BatteryMasterExists(string batterySerialNo)
        {
            return _objBatteryMasterService.Query(e => e.BatterySerialNo == batterySerialNo).Select().Any();
        }
        private bool BatteryMasterExists(long key)
        {
            return _objBatteryMasterService.Query(e => e.Id == key).Select().Any();
        }
    }
}