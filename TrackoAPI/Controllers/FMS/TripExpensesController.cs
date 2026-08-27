using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;
using TrackoApi.Core.Helpers;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx, EnableQuery(MaxNodeCount = 500)]
    public class TripExpensesController : ODataController
    //ODataController
    {
        private readonly ITripExpenseLogService _objTripExpenseLogService;

        public TripExpensesController(ITripExpenseLogService service)
        {
            _objTripExpenseLogService = service;
        }
        // GET: odata/TripExpenseLogs
        [HttpGet, EnableQuery]
        public IQueryable<TripExpenseLog> Get()
        {
            return _objTripExpenseLogService.Queryable();
        }
        // GET: odata/TripExpenseLogs(5)
        [EnableQuery]
        public SingleResult<TripExpenseLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objTripExpenseLogService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/TripExpenseLogs(5)
        public async Task<IHttpActionResult> Put(long key, TripExpenseLog objTripExpenseLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != objTripExpenseLog.Id)
            {
                return BadRequest();
            }
            objTripExpenseLog.ObjectState = ObjectState.Modified;
            _objTripExpenseLogService.Update(objTripExpenseLog);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objTripExpenseLog);
        }
        // POST: odata/TripExpenseLogs
        public async Task<IHttpActionResult> Post(TripExpenseLog objTripExpenseLog)
        {
            objTripExpenseLog.ObjectState = ObjectState.Added;
            _objTripExpenseLogService.Insert(objTripExpenseLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(objTripExpenseLog);
        }
        //// PATCH: odata/TripExpenseLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TripExpenseLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            TripExpenseLog obj = await _objTripExpenseLogService.Queryable().Include(x=>x.fk_TripAdvanceLog).Include(x=>x.fk_TripLog.TripExpenses).Include(x=>x.fk_Settlement).FirstOrDefaultAsync(x=>x.Id==key);
            if (obj == null)
            {
                return NotFound();
            }
            obj.ObjectState = ObjectState.Modified;
            patch.Patch(obj);
            if (obj.fk_TripLog != null && obj.fk_TripLog.TripExpenses.Any())
            {
                //Update Trip ExpansesTotal
                obj.fk_TripLog.BdgtTripExpense = obj.fk_TripLog.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) == 0).Sum(x => x.ClaimAmount);
                obj.fk_TripLog.ObjectState = ObjectState.Modified;
            }
            if (obj.fk_Settlement != null)
            {
                //Update Trip ExpansesTotal
                obj.fk_Settlement.TripExpenseAmt = obj.fk_Settlement.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) == 0).Sum(x => x.SettledAmount);
                obj.fk_Settlement.ObjectState = ObjectState.Modified;
            }
            if (obj.fk_TripLog !=null&&obj.TripLogId>0)
            {
                obj.fk_TripLog.ConsumedFuelAmt = obj.fk_TripLog.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0).Sum(x => x.SettledAmount);
                obj.fk_TripLog.ConsumedFuelQty = obj.fk_TripLog.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0).Sum(x => x.FuelQty);
                obj.fk_TripLog.ShortFuelAmt = obj.fk_TripLog.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0).Sum(x => x.ShortFuelAmt);
                obj.fk_TripLog.ShortFuelQty = obj.fk_TripLog.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0).Sum(x => x.ShortFuelQty);
            }
            /*Commented by Mukesh*///TODO:Write new logic for Fuel Budgeting
            //if (obj.TripAdvanceLogId > 0)
            //{
            //    var r =
            //        _objTripExpenseLogService.Queryable()
            //            .Where(x => x.TripAdvanceLogId == obj.TripAdvanceLogId && x.Id != obj.Id);
            //    decimal sum = 0;
            //    if (r.Any())
            //    {
            //        sum = r.Sum(x => x.FuelQty + x.ShortFuelQty);
            //    }
                
            //    if (obj.fk_TripAdvanceLog.FuelQty < (sum+obj.FuelQty+obj.ShortFuelQty))
            //    {
            //        return BadRequest($"Maximum Allocation Fuel Qty {obj.fk_TripAdvanceLog.FuelQty- (sum + obj.FuelQty + obj.ShortFuelQty)} has been exceeded for Fuel Expense No {obj.fk_TripAdvanceLog.ReferenceNo}");
            //    }
            //    obj.fk_TripAdvanceLog.BalanceQty = obj.fk_TripAdvanceLog.FuelQty-(sum + obj.FuelQty + obj.ShortFuelQty);
            //    obj.fk_TripAdvanceLog.ObjectState=ObjectState.Modified;
            //}
            await Request.GetContext().SaveChangesAsync();

            return Updated(obj);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            TripExpenseLog obj = await _objTripExpenseLogService.Queryable().Include(x => x.fk_TripLog.TripExpenses).Include(x => x.fk_Settlement).Include(x=>x.fk_TripAdvanceLog).FirstOrDefaultAsync(x => x.Id == key);
            if (obj == null)
            {
                return NotFound();
            }
            obj.ObjectState = ObjectState.Deleted;
            if (obj.fk_TripLog != null && obj.fk_TripLog.TripExpenses.Any())
            {
                //Update Trip ExpansesTotal
                obj.fk_TripLog.BdgtTripExpense = obj.fk_TripLog.TripExpenses.Where(x=>x.ObjectState!=ObjectState.Deleted&&x.TripAdvanceLogId.GetValueOrDefault(0) == 0).Sum(x => x.ClaimAmount);
                obj.fk_TripLog.ObjectState = ObjectState.Modified;
            }
            if (obj.fk_Settlement != null)
            {
                //Update Trip ExpansesTotal
                obj.fk_Settlement.TripExpenseAmt = obj.fk_Settlement.TripExpenses.Where(x => x.ObjectState != ObjectState.Deleted && x.TripAdvanceLogId.GetValueOrDefault(0) == 0).Sum(x => x.SettledAmount);
                obj.fk_Settlement.ObjectState = ObjectState.Modified;
            }
            if (obj.TripAdvanceLogId > 0)
            {
                var r =
                    _objTripExpenseLogService.Queryable()
                        .Where(x => x.TripAdvanceLogId == obj.TripAdvanceLogId && x.Id != obj.Id);
                decimal sum = 0;
                if (r.Any())
                {
                    sum = r.Sum(x => x.FuelQty + x.ShortFuelQty);
                }
                obj.fk_TripAdvanceLog.BalanceQty = obj.fk_TripAdvanceLog.FuelQty - (sum + obj.FuelQty + obj.ShortFuelQty);
                obj.fk_TripAdvanceLog.ObjectState = ObjectState.Modified;
            }
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }



        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var tripadv = _objTripExpenseLogService.Queryable().SingleOrDefault(p => p.Id == key);
            if (tripadv == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_TripAdvanceLog":
                    tripadv.TripAdvanceLogId = null;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }

            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var trpadvid = await _objTripExpenseLogService.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (trpadvid == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {

                case "fk_TripAdvanceLog":
                    var tlRepo = Request.GetContext()
                       .RepositoryAsync<TripAdvanceLog>();
                    if (!await tlRepo.Queryable().AnyAsync(x => x.Id == id))
                    {
                        return BadRequest("Invalid Trip Advance For Mapping with Trip Expense");
                    }
                    trpadvid.TripAdvanceLogId = id;
                    trpadvid.ObjectState = ObjectState.Modified;
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
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}