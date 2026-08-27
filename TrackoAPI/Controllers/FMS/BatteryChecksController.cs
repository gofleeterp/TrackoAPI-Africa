using System;
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
    public class BatteryCheckingsController : ODataController
    //ODataController
    {
        private readonly IBatteryCheckService _objBatteryCheckService;

        public BatteryCheckingsController(IBatteryCheckService service)
        {
            _objBatteryCheckService = service;
        }
        // GET: odata/BatteryChecks
        [HttpGet, EnableQuery]
        public IQueryable<BatteryCheck> Get()
        {
            return _objBatteryCheckService.Queryable();
        }
        // GET: odata/BatteryChecks(5)
        [EnableQuery]
        public SingleResult<BatteryCheck> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objBatteryCheckService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/BatteryChecks(5)
        public async Task<IHttpActionResult> Put(long key, BatteryCheck objBatteryCheck)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objBatteryCheck.Id)
            {
                return BadRequest();
            }
            objBatteryCheck.ObjectState = ObjectState.Modified;
            _objBatteryCheckService.Update(objBatteryCheck);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objBatteryCheck);
        }
        //[ODataRoute("BatteryCheckings({key})/JobCard/$ref"),HttpPost]
        //public IHttpActionResult LinkJobCard()
        //{

        //}
        [AcceptVerbs("POST", "PUT")]//For Single Navigation Properties
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty,
            [FromBody] Uri link)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var _uow = Request.GetContext();
            BatteryCheck Batterycheck = await _objBatteryCheckService.FindAsync(key);
            if (Batterycheck == null)
            {
                return NotFound();
            }
            long navigationkey = Request.GetKeyFromUri<long>(link);
            
            switch (navigationProperty)
            {
                case "JobCard":
                    VehicleMovementLog jobcard = await _uow.RepositoryAsync<VehicleMovementLog>().FindAsync(navigationkey);
                    if (jobcard == null)
                    {
                        return NotFound();
                    }
                    Batterycheck.JobCard = jobcard;
                    Batterycheck.JobCardId = jobcard.Id;
                    Batterycheck.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    break;
                default:
                    return NotFound();
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("POST", "PUT")]//For Multiple Navigation Properties
        public async Task<IHttpActionResult> CreateLink([FromODataUri] int key, string navigationProperty,
            [FromBody] Uri link)
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        // POST: odata/BatteryChecks
        public async Task<IHttpActionResult> Post(BatteryCheck objBatteryCheck)
        {
            objBatteryCheck.ObjectState = ObjectState.Added;
            _objBatteryCheckService.Insert(objBatteryCheck);
            await Request.GetContext().SaveChangesAsync();
            return Created(objBatteryCheck);
        }
        //// PATCH: odata/BatteryChecks(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<BatteryCheck> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            BatteryCheck objBatteryCheck = await _objBatteryCheckService.FindAsync(key);
            if (objBatteryCheck == null)
            {
                return NotFound();
            }
            objBatteryCheck.ObjectState = ObjectState.Modified;
            patch.Patch(objBatteryCheck);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objBatteryCheck);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objBatteryCheck = await _objBatteryCheckService.FindAsync(key);
            if (objBatteryCheck == null)
            {
                return NotFound();
            }
            objBatteryCheck.ObjectState = ObjectState.Deleted;
            _objBatteryCheckService.Delete(objBatteryCheck);
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
        
    }
}