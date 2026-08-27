using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNChallansController : ODataController
    //ODataController
    {
        
        private readonly ICNChallanService _objCnChallanService;
        

        public CNChallansController(ICNChallanService service)
        {
            _objCnChallanService = service;
        }
        // GET: odata/CNChallans
        [HttpGet, EnableQuery(MaxExpansionDepth = 4)]
        public IQueryable<CnChallan> Get()
        {
            return _objCnChallanService.Queryable();
        }
        // GET: odata/CNChallans(5)
        [EnableQuery(MaxExpansionDepth = 4)]
        public SingleResult<CnChallan> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objCnChallanService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/CNChallans(5)
        public async Task<IHttpActionResult> Put(long key, CnChallan objCnChallan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objCnChallan.Id)
            {
                return BadRequest();
            }
            objCnChallan.ObjectState = ObjectState.Modified;
            _objCnChallanService.Update(objCnChallan);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objCnChallan);
        }
        // POST: odata/CNChallans
        public async Task<IHttpActionResult> Post(CnChallan objCnChallan)
        {
            objCnChallan.ObjectState = ObjectState.Added;
            _objCnChallanService.Insert(objCnChallan);
            await Request.GetContext().SaveChangesAsync();
            return Created(objCnChallan);
        }
        //// PATCH: odata/CNChallans(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CnChallan> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var chcn = await _objCnChallanService.Queryable().Include(x=>x.CnStockLogs).FirstOrDefaultAsync(x=>x.Id==key);
            if (chcn == null)
            {
                return NotFound();
            }
            patch.Patch(chcn);
            if (chcn.ChallanId > 0)
            {
                var challan = await Request.GetContext().RepositoryAsync<ChallanMaster>().Queryable().Where(x => x.Id == chcn.ChallanId).Include(x => x.CNChallans).FirstOrDefaultAsync();
                if (challan == null)
                {
                    return NotFound();
                }
                challan.ObjectState = ObjectState.Modified;
                challan.Quantity = challan.CNChallans.Sum(x => x.Qty);
                challan.Weight = challan.CNChallans.Sum(x => x.Weight);
                chcn.TriplogId = challan.TriplogId;
            }
            //patch.TrySetPropertyValue("ArrivalDate", chcn.ArrivalDate);
            //patch.TrySetPropertyValue("ArrivalQty", chcn.ArrivalQty);

          
           
            chcn.ObjectState = ObjectState.Modified;
            
            await Request.GetContext().SaveChangesAsync();
            return Updated(chcn);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            
            var chcn = await _objCnChallanService.FindAsync(key);
            if (chcn == null)
            {
                return NotFound();
            }
            var cnid = chcn.CNId;
            var uow = Request.GetContext();
            if (
                await
                    uow.RepositoryAsync<CNStockLog>()
                        .Queryable()
                        .AnyAsync(x => x.ChallanCNId == chcn.Id&&x.LogTypeId==1423 &&x.Outwards.Any(y=>y.Outwards.Any())))
            {
                return BadRequest("LR cannot be removed as it has been arrived at Destination.");
            }
            
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }
            try
            {
                chcn.ObjectState=ObjectState.Deleted;
                if (chcn.ChallanId > 0)
                {
                    var chr = uow.RepositoryAsync<ChallanMaster>();
                    var chq = chr.Queryable().Include(x => x.CNChallans);
                    var ch = await chq.FirstOrDefaultAsync(x => x.Id == chcn.ChallanId);
                    if (ch.CNChallans.Count(x => x.ObjectState != ObjectState.Deleted) == 0)
                    {
                        ch.ObjectState = ObjectState.Deleted;
                        ch.TriplogId = null;
                        //await chr.DeleteAsync(ch);
                    }
                    else
                    {
                        ch.Quantity = ch.CNChallans.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.Qty);
                        ch.Weight = ch.CNChallans.Where(x => x.ObjectState != ObjectState.Deleted).Sum(x => x.Weight);
                    }
                }
                var count=await uow.SaveChangesAsync();
                if (count > 0)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tCNMaster] SET TripLogId=NULL WHERE Id={cnid}");
                    await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tSalesLog] SET TripLogId=NULL WHERE CNId={cnid}");
                }
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            
            return StatusCode(HttpStatusCode.NoContent);
        }
        [HttpPost]
        public async Task<IHttpActionResult> UpdateArrival([FromODataUri] long key, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var arrivalQty = (decimal) parameters["ArrivalQty"];
            var shortQty = (decimal) parameters["ShortQty"];
            var excessQty=(decimal)parameters["ExcessQty"];
            var arrivalDate = (DateTimeOffset?) parameters["ArrivalDate"];
             parameters.TryGetValue("ArrivalViewId", out var obj);
            long.TryParse(obj?.ToString(), out var arriveSource);
            var cnChallan = _objCnChallanService.Queryable().Include(x=>x.fk_Triplog).FirstOrDefault(x=>x.Id==key);
            if (cnChallan == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            cnChallan.ArrivalViewId = arriveSource;
            if (arrivalDate.HasValue&&(arrivalQty+shortQty) <= 0|| (arrivalQty + shortQty)>cnChallan.Qty)
            {
                throw new BusinessException(ErrorCode.GLB106,$"[Arrival Qty]+[Short Qty] should be greater than zero and should be equal to {cnChallan.Qty}");
            }
            if (arrivalDate.HasValue && arrivalDate.Value.DateTime < cnChallan.ShipmentDate)
            {
                throw new BusinessException(ErrorCode.GLB106,
                    $"Arrival Date should be greater than {cnChallan.ShipmentDate.GetValueOrDefault(cnChallan.fk_Triplog.TripStartDate):F}");
            }
            if(arrivalDate==null&&cnChallan.ArrivalDate!=null&&cnChallan.fk_Triplog.UnloadingDate!=null)
            {
                throw new BusinessException(ErrorCode.GLB106,
                    "Trip has ended so you cannot undo arrival of consignment from here.");
            }
            cnChallan.ArrivalQty = arrivalQty;
            cnChallan.Short = shortQty;
            cnChallan.Excess = excessQty;
            cnChallan.ArrivalDate = arrivalDate?.DateTime;
            cnChallan.ObjectState=ObjectState.Modified;
            _objCnChallanService.Update(cnChallan);
            await uow.SaveChangesAsync();
            if(cnChallan.TriplogId>0&&_objCnChallanService.Queryable().Where(x=>x.TriplogId==cnChallan.TriplogId).All(x=> x.ArrivalDate != null))
            {
                var maxdate = await _objCnChallanService.Queryable().Where(x => x.TriplogId == cnChallan.TriplogId && x.ArrivalDate != null).MaxAsync(x=>x.ArrivalDate);
                if (maxdate != null)
                {
                    await uow.ExecSqlQueryAsync($"UPDATE tVehicleMovementLog SET UnloadingDate=CASE WHEN UnloadingDate IS NULL THEN @p0 ELSE UnloadingDate END,UnloadingReachDate=CASE WHEN UnloadingReachDate IS NULL THEN @p0 ELSE UnloadingReachDate END WHERE Id=@p1",new SqlParameter("p0",maxdate), new SqlParameter("p1", cnChallan.TriplogId));
                }
            }
            return Ok();
        }
        [HttpPost]
        public async Task<IHttpActionResult> UpdateDeliveryFailed([FromODataUri] long key, ODataActionParameters parameters)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            
            var deliveryattemptDate = (DateTimeOffset?)parameters["DeliveryFailedDate"];
            parameters.TryGetValue("ArrivalViewId", out var obj);
            long.TryParse(obj?.ToString(), out var arriveSource);
            var cnChallan = _objCnChallanService.Queryable().Include(x => x.fk_Triplog).FirstOrDefault(x => x.Id == key);
            if (cnChallan == null)
            {
                return NotFound();
            }
            cnChallan.ArrivalViewId = arriveSource;
            if (deliveryattemptDate.HasValue&& deliveryattemptDate.Value.DateTime < cnChallan.ShipmentDate)
            {
                throw new BusinessException(ErrorCode.GLB106,
                    $"Delivery Failed Date should be greater than {cnChallan.ShipmentDate.GetValueOrDefault(cnChallan.fk_Triplog.TripStartDate).ToString("F")}");
            }
            
            
            cnChallan.DeliveryFailedDate = deliveryattemptDate?.DateTime;
            cnChallan.IsDeliveryFailed = deliveryattemptDate != null;
            cnChallan.ObjectState = ObjectState.Modified;
            _objCnChallanService.Update(cnChallan);
            await Request.GetContext().SaveChangesAsync();
            return Ok();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing&&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        [AcceptVerbs("POST", "PUT")]

        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            var cnch = await _objCnChallanService.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (cnch == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);

            var triplogid = uow.RepositoryAsync<VehicleMovementLog>();
            var tripid =
                await
                    triplogid.Queryable().AnyAsync(x => x.Id == id);
            if (!tripid)
            {
                return NotFound();
            }
            cnch.TriplogId = id;
            cnch.ObjectState = ObjectState.Modified;
            await uow.SaveChangesAsync();

            //switch (navigationProperty)
            //{
            //    case "fk_TripLog":               

            //        break;               
            //    default:
            //        return StatusCode(HttpStatusCode.NotImplemented);
            //}
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}