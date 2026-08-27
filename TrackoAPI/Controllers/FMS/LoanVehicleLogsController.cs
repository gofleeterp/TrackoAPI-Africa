using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.ModelBinding;
using Repository.Pattern.Core;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.Inventory;
using TrackoApi.Models.FMS.Loan;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class LoanVehicleLogsController : ODataController
    //ODataController
    {
        private readonly ILoanVehicleLogService _objLoanVehicleLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public LoanVehicleLogsController(IUnitOfWorkAsync unitOfWorkAsync, ILoanVehicleLogService service)
        {
            _objLoanVehicleLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/LoanVehicleLogs
        [HttpGet, EnableQuery]
        public IQueryable<LoanVehicleLog> Get()
        {
            return _objLoanVehicleLogService.Queryable();
        }
        // GET: odata/LoanVehicleLogs(5)
        [EnableQuery]
        public SingleResult<LoanVehicleLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objLoanVehicleLogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/LoanVehicleLogs(5)
        public async Task<IHttpActionResult> Put(long key, LoanVehicleLog objLoanVehicleLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objLoanVehicleLog.Id)
            {
                return BadRequest();
            }
            objLoanVehicleLog.ObjectState = ObjectState.Modified;
            _objLoanVehicleLogService.Update(objLoanVehicleLog);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanVehicleLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLoanVehicleLog);
        }
        // POST: odata/LoanVehicleLogs
        public async Task<IHttpActionResult> Post(LoanVehicleLog objLoanVehicleLog)
        {
            objLoanVehicleLog.ObjectState = ObjectState.Added;
            _objLoanVehicleLogService.Insert(objLoanVehicleLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LoanVehicleLogExists("", objLoanVehicleLog.VehicleId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objLoanVehicleLog);
        }
        //// PATCH: odata/LoanVehicleLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<LoanVehicleLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            LoanVehicleLog objLoanVehicleLog = await _objLoanVehicleLogService.FindAsync(key);
            if (objLoanVehicleLog == null)
            {
                return NotFound();
            }
            objLoanVehicleLog.ObjectState = ObjectState.Modified;
            patch.Patch(objLoanVehicleLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoanVehicleLogExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLoanVehicleLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objLoanVehicleLog = await _objLoanVehicleLogService.FindAsync(key);
            if (objLoanVehicleLog == null)
            {
                return NotFound();
            }
            objLoanVehicleLog.ObjectState = ObjectState.Deleted;
            _objLoanVehicleLogService.Delete(objLoanVehicleLog);
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

        private bool LoanVehicleLogExists(string h,long? vehicleId)
        {
            return _objLoanVehicleLogService.Query(e => e.VehicleId == vehicleId).Select().Any();
        }
        private bool LoanVehicleLogExists(long key)
        {
            return _objLoanVehicleLogService.Query(e => e.Id == key).Select().Any();
        }
    }
}