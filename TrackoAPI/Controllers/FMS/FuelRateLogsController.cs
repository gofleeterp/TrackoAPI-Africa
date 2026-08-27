using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class FuelRateLogsController : ODataController
    //ODataController
    {
        private readonly IFuelRateLogService _repo;

        public FuelRateLogsController(IFuelRateLogService service)
        {
            _repo = service;
        }
        // GET: odata/FuelRateLogs
        [HttpGet, EnableQuery]
        public IQueryable<FuelRateLog> Get() => _repo.Queryable();

        // GET: odata/FuelRateLogs(5)
        [EnableQuery]
        public SingleResult<FuelRateLog> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/FuelRateLogs(5)
        public async Task<IHttpActionResult> Put(long key, FuelRateLog objFuelRateLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objFuelRateLog.Id)
            {
                return BadRequest();
            }
            objFuelRateLog.ObjectState = ObjectState.Modified;
            _repo.Update(objFuelRateLog);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FuelRateLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objFuelRateLog);
        }
        // POST: odata/FuelRateLogs
        public async Task<IHttpActionResult> Post(FuelRateLog objFuelRateLog)
        {
            objFuelRateLog.ObjectState = ObjectState.Added;
            _repo.Insert(objFuelRateLog);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                //if (FuelRateLogExists(objFuelRateLog.BrandName))
                //{
                //    throw new BusinessException(ErrorCode.GLB104,"Record Already Exists");
                //    //return Conflict();
                //}
                throw;
            }
            return Created(objFuelRateLog);
        }
        //// PATCH: odata/FuelRateLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<FuelRateLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            FuelRateLog objFuelRateLog = await _repo.FindAsync(key);
            if (objFuelRateLog == null)
            {
                return NotFound();
            }
            objFuelRateLog.ObjectState = ObjectState.Modified;
            patch.Patch(objFuelRateLog);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FuelRateLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objFuelRateLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objFuelRateLog = await _repo.FindAsync(key);
            if (objFuelRateLog == null)
            {
                return NotFound();
            }
            objFuelRateLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(objFuelRateLog);
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

//        private bool FuelRateLogExists(long fuelId,long pumpId,DateTime fromDate,DateTime toDate) => _repo.Query(e => e.FuelId == fuelId && e.PumpId == pumpId && e.FromDate).Select().Any();
        private bool FuelRateLogExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}