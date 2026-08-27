using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Results;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleAccessoryLogsController : ODataController
    //ODataController
    {
        private readonly IVehicleAccessoryLogService _repo;
        private readonly IUnitOfWorkAsync _uow;

        public VehicleAccessoryLogsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleAccessoryLogService service)
        {
            _repo = service;
            _uow = unitOfWorkAsync;
        }
        // GET: odata/VehicleAccessoryLogs
        [HttpGet, EnableQuery]
        public IQueryable<VehicleAccessoryLog> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/VehicleAccessoryLogs(5)
        [EnableQuery]
        public SingleResult<VehicleAccessoryLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleAccessoryLogs(5)
        public async Task<IHttpActionResult> Put(long key, VehicleAccessoryLog objVehicleAccessoryLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleAccessoryLog.Id)
            {
                return BadRequest();
            }
            objVehicleAccessoryLog.ObjectState = ObjectState.Modified;
            _repo.Update(objVehicleAccessoryLog);
            await _uow.SaveChangesAsync();

            return Updated(objVehicleAccessoryLog);
        }
        // POST: odata/VehicleAccessoryLogs
        public async Task<IHttpActionResult> Post(VehicleAccessoryLog objVehicleAccessoryLog)
        {
            objVehicleAccessoryLog.ObjectState = ObjectState.Added;
            _repo.Insert(objVehicleAccessoryLog);
            await _uow.SaveChangesAsync();
            return Created(objVehicleAccessoryLog);
        }
        //// PATCH: odata/VehicleAccessoryLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleAccessoryLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleAccessoryLog objVehicleAccessoryLog = await _repo.FindAsync(key);
            if (objVehicleAccessoryLog == null)
            {
                return NotFound();
            }
            objVehicleAccessoryLog.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleAccessoryLog);
            await _uow.SaveChangesAsync();

            return Updated(objVehicleAccessoryLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleAccessoryLog = await _repo.FindAsync(key);
            if (objVehicleAccessoryLog == null)
            {
                return NotFound();
            }
            objVehicleAccessoryLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(objVehicleAccessoryLog);
            await _uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _uow.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}