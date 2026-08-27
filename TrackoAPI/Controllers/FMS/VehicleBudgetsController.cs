using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleBudgetsController : ODataController
    //ODataController
    {
        private readonly IVehicleBudgetService _objVehicleBudgetService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public VehicleBudgetsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleBudgetService service)
        {
            _objVehicleBudgetService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/VehicleBudgets
        [HttpGet, EnableQuery]
        public IQueryable<VehicleBudget> Get()
        {
            return _objVehicleBudgetService.Queryable();
        }
        // GET: odata/VehicleBudgets(5)
        [EnableQuery]
        public SingleResult<VehicleBudget> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleBudgetService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleBudgets(5)
        public async Task<IHttpActionResult> Put(long key, VehicleBudget objVehicleBudget)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleBudget.Id)
            {
                return BadRequest();
            }
            objVehicleBudget.ObjectState = ObjectState.Modified;
            _objVehicleBudgetService.Update(objVehicleBudget);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objVehicleBudget);
        }
        // POST: odata/VehicleBudgets
        public async Task<IHttpActionResult> Post(VehicleBudget objVehicleBudget)
        {
            objVehicleBudget.ObjectState = ObjectState.Added;
            _objVehicleBudgetService.Insert(objVehicleBudget);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objVehicleBudget);
        }
        //// PATCH: odata/VehicleBudgets(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleBudget> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleBudget objVehicleBudget = await _objVehicleBudgetService.FindAsync(key);
            if (objVehicleBudget == null)
            {
                return NotFound();
            }
            objVehicleBudget.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleBudget);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objVehicleBudget);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleBudget = await _objVehicleBudgetService.FindAsync(key);
            if (objVehicleBudget == null)
            {
                return NotFound();
            }
            objVehicleBudget.ObjectState = ObjectState.Deleted;
            _objVehicleBudgetService.Delete(objVehicleBudget);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}