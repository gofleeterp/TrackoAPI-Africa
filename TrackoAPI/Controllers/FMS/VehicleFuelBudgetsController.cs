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
    public class VehicleFuelBudgetsController : ODataController
    //ODataController
    {
        private readonly IVehicleFuelBudgetService _objVehicleFuelBudgetService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public VehicleFuelBudgetsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleFuelBudgetService service)
        {
            _objVehicleFuelBudgetService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/VehicleFuelBudgets
        [HttpGet, EnableQuery]
        public IQueryable<VehicleFuelBudget> Get()
        {
            return _objVehicleFuelBudgetService.Queryable();
        }
        // GET: odata/VehicleFuelBudgets(5)
        [EnableQuery]
        public SingleResult<VehicleFuelBudget> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleFuelBudgetService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleFuelBudgets(5)
        public async Task<IHttpActionResult> Put(long key, VehicleFuelBudget objVehicleFuelBudget)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleFuelBudget.Id)
            {
                return BadRequest();
            }
            objVehicleFuelBudget.ObjectState = ObjectState.Modified;
            _objVehicleFuelBudgetService.Update(objVehicleFuelBudget);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objVehicleFuelBudget);
        }
        // POST: odata/VehicleFuelBudgets
        public async Task<IHttpActionResult> Post(VehicleFuelBudget objVehicleFuelBudget)
        {
            objVehicleFuelBudget.ObjectState = ObjectState.Added;
            _objVehicleFuelBudgetService.Insert(objVehicleFuelBudget);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objVehicleFuelBudget);
        }
        //// PATCH: odata/VehicleFuelBudgets(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleFuelBudget> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleFuelBudget objVehicleFuelBudget = await _objVehicleFuelBudgetService.FindAsync(key);
            if (objVehicleFuelBudget == null)
            {
                return NotFound();
            }
            objVehicleFuelBudget.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleFuelBudget);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objVehicleFuelBudget);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleFuelBudget = await _objVehicleFuelBudgetService.FindAsync(key);
            if (objVehicleFuelBudget == null)
            {
                return NotFound();
            }
            objVehicleFuelBudget.ObjectState = ObjectState.Deleted;
            _objVehicleFuelBudgetService.Delete(objVehicleFuelBudget);
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