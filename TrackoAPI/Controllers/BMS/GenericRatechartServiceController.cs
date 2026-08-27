using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service.TMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleConfigurationLogsController : ODataController
    //ODataController
    {
        private readonly IGenericRatechartService _objGenericRatechart;

        public VehicleConfigurationLogsController(IGenericRatechartService service)
        {
            _objGenericRatechart = service;
        }
        // GET: odata/VehicleConfigurationLog
        [HttpGet, EnableQuery]
        public IQueryable<VehicleConfigurationLog> Get()
        {
            return _objGenericRatechart.Queryable();
        }
        // GET: odata/VehicleConfigurationLog(5)
        [EnableQuery]
        public SingleResult<VehicleConfigurationLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objGenericRatechart.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/VehicleConfigurationLog(5)
        public async Task<IHttpActionResult> Put(long key, VehicleConfigurationLog objVehicleConfigurationLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleConfigurationLog.Id)
            {
                return BadRequest();
            }
            objVehicleConfigurationLog.ObjectState = ObjectState.Modified;
            _objGenericRatechart.Update(objVehicleConfigurationLog);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objVehicleConfigurationLog);
        }

        // POST: odata/VehicleConfigurationLog
        public async Task<IHttpActionResult> Post(VehicleConfigurationLog objVehicleConfigurationLog)
        {
            objVehicleConfigurationLog.ObjectState = ObjectState.Added;
            objVehicleConfigurationLog.IsActive = true;
            var cnextra= _objGenericRatechart.Insert(objVehicleConfigurationLog);

            await Request.GetContext().SaveChangesAsync();
            return Created(cnextra);
        }

        //// PATCH: odata/VehicleConfigurationLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleConfigurationLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            VehicleConfigurationLog cnextra = await _objGenericRatechart.FindAsync(key);
            if (cnextra == null)
            {
                return NotFound();
            }
            cnextra.ObjectState = ObjectState.Modified;
            patch.Patch(cnextra);
            await Request.GetContext().SaveChangesAsync();
            return Updated(cnextra);
        }

        // DELETE: odata/VehicleConfigurationLog(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleConfigurationLog = await _objGenericRatechart.FindAsync(key);
            if (objVehicleConfigurationLog == null)
            {
                return NotFound();
            }
            objVehicleConfigurationLog.IsActive = false;
            objVehicleConfigurationLog.ObjectState = ObjectState.Modified;
            _objGenericRatechart.Update(objVehicleConfigurationLog);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objVehicleConfigurationLog);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}