using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleMonthlyBudgetsController : ODataController
    //ODataController
    {
        private readonly IVehicleMonthlyBudgetService _objVehicleMonthlyBudgetService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public VehicleMonthlyBudgetsController(IUnitOfWorkAsync unitOfWorkAsync, IVehicleMonthlyBudgetService service)
        {
            _objVehicleMonthlyBudgetService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/VehicleMonthlyBudgets
        [HttpGet, EnableQuery]
        public IQueryable<VehicleMonthlyBudget> Get()
        {
            return _objVehicleMonthlyBudgetService.Queryable();
        }
        // GET: odata/VehicleMonthlyBudgets(5)
        [EnableQuery]
        public SingleResult<VehicleMonthlyBudget> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objVehicleMonthlyBudgetService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleMonthlyBudgets(5)
        public async Task<IHttpActionResult> Put(long key, VehicleMonthlyBudget objVehicleMonthlyBudget)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleMonthlyBudget.Id)
            {
                return BadRequest();
            }
            objVehicleMonthlyBudget.ObjectState = ObjectState.Modified;
            _objVehicleMonthlyBudgetService.Update(objVehicleMonthlyBudget);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleMonthlyBudgetExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVehicleMonthlyBudget);
        }
        // POST: odata/VehicleMonthlyBudgets
        public async Task<IHttpActionResult> Post(VehicleMonthlyBudget objVehicleMonthlyBudget)
        {
            objVehicleMonthlyBudget.ObjectState = ObjectState.Added;
            _objVehicleMonthlyBudgetService.Insert(objVehicleMonthlyBudget);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (VehicleMonthlyBudgetExists(objVehicleMonthlyBudget.VehicleId,objVehicleMonthlyBudget.RefDate))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objVehicleMonthlyBudget);
        }
        //// PATCH: odata/VehicleMonthlyBudgets(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleMonthlyBudget> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            VehicleMonthlyBudget objVehicleMonthlyBudget = await _objVehicleMonthlyBudgetService.FindAsync(key);
            if (objVehicleMonthlyBudget == null)
            {
                return NotFound();
            }
            objVehicleMonthlyBudget.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleMonthlyBudget);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleMonthlyBudgetExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVehicleMonthlyBudget);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objVehicleMonthlyBudget = await _objVehicleMonthlyBudgetService.FindAsync(key);
            if (objVehicleMonthlyBudget == null)
            {
                return NotFound();
            }
            objVehicleMonthlyBudget.ObjectState = ObjectState.Deleted;
            _objVehicleMonthlyBudgetService.Delete(objVehicleMonthlyBudget);
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

        private bool VehicleMonthlyBudgetExists(long vehicleId,DateTime refDate)
        {
            return _objVehicleMonthlyBudgetService.Query(e => e.VehicleId == vehicleId && e.RefDate==refDate).Select().Any();
        }
        private bool VehicleMonthlyBudgetExists(long key)
        {
            return _objVehicleMonthlyBudgetService.Query(e => e.Id == key).Select().Any();
        }
    }
}