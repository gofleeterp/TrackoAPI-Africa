using Newtonsoft.Json;
using Repository.Pattern.Core.UnitOfWork;

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.vw.ts;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleTripSettlementsController : ODataController
    //ODataController
    {
        private readonly IVehicleTripSettlementService _service;

        public VehicleTripSettlementsController(IVehicleTripSettlementService service)
        {
            _service = service;
        }
        // GET: odata/VehicleTripSettlements
        [HttpGet, EnableQuery]
        public IQueryable<VehicleTripSettlement> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/VehicleTripSettlements(5)
        [EnableQuery]
        public SingleResult<VehicleTripSettlement> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/VehicleTripSettlements(5)
        public async Task<IHttpActionResult> Put(long key, VehicleTripSettlement objVehicleTripSettlement)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objVehicleTripSettlement.Id)
            {
                return BadRequest();
            }
            objVehicleTripSettlement.ObjectState = ObjectState.Modified;
            _service.Update(objVehicleTripSettlement);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VehicleTripSettlementExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objVehicleTripSettlement);
        }
        // POST: odata/VehicleTripSettlements
        public async Task<IHttpActionResult> Post(VehicleTripSettlement entity)
        {
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction();
            }
            entity.ObjectState = ObjectState.Added;
            _service.Insert(entity);
            var settlement=_service.PrepareSettlement(entity.Id,entity);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                if (VehicleTripSettlementExists(entity.TripSheetNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().Commit();
            }
            return Created(settlement);
        }
        //// PATCH: odata/VehicleTripSettlements(5)
        /// <summary>
        /// Patches the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="patch">The patch.</param>
        /// <returns>Task&lt;IHttpActionResult&gt;.</returns>
        /// PATCH performs a partial update. The client specifies just the properties to update.
        /// <exception cref="DbUpdateConcurrencyException">Condition.</exception>
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleTripSettlement> patch)
        {
            
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().BeginTransaction();
            }
            VehicleTripSettlement objVehicleTripSettlement = await _service.Queryable().Where(x => x.Id == key)
                      .Include(x => x.TripAdvances.Select(v => v.fk_DebitAccount))
                      .Include(x => x.TripExpenses.Select(z => z.fk_ExpenseType.fk_Ledger))
                      .Include(x => x.TripExpenses.Select(z => z.fk_TripAdvanceLog.fk_DebitAccount))
                      .Include(x => x.TripLogs)
                      .Include(x => x.fk_Voucher)
                      .FirstOrDefaultAsync();
            if (objVehicleTripSettlement == null)
            {
                return NotFound();
            }
            objVehicleTripSettlement.ObjectState = ObjectState.Modified;
            patch.Patch(objVehicleTripSettlement);
            var settlement = _service.PrepareSettlement(objVehicleTripSettlement.Id, objVehicleTripSettlement);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                if (!VehicleTripSettlementExists(key))
                {
                    return NotFound();
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().Commit();
            }
            return Updated(settlement);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction();
            }
            try
            {
                
                var tladvmappingflag = uow.RepositoryAsync<VehicleTripSettlement>().GetClientConfigValue<long>("ShowAutoTripOnAdvance");
                var ts = await _service.Queryable()
                .Include(x => x.TripAdvances)
                .Include(x => x.TripExpenses)
                .Include(x => x.TripLogs).Where(x => x.Id == key).FirstOrDefaultAsync();
                if (ts == null)
                {
                    return NotFound();
                }
                var tsbaladvid = ts.CashPaidAdvId;
                long? tsbaladv_vchid=null;
                var advRepo = uow.RepositoryAsync<TripAdvanceLog>();
                
                if (tsbaladvid.GetValueOrDefault() > 0)
                {
                    var tsbaladv = await advRepo.Queryable().Where(x => x.Id == tsbaladvid).Select(x =>new {x.VoucherId,x.RequestStatusId }).FirstOrDefaultAsync();
                    tsbaladv_vchid = tsbaladv.VoucherId;
                    if (tsbaladv.RequestStatusId == 1597)
                    {
                        throw new BusinessException(ErrorCode.TADV108, "The Balance TripAdvance for this Trip Settlement has been Disburshed.");
                    }
                }
                if (ts.TripAdvances != null && ts.TripAdvances.Any())
                {
                    ts.TripAdvances.ForEach(x =>
                    {
                        x.SettlementId = null;
                        x.fk_Settlement = null;
                        if (tladvmappingflag == 0&&x.HireVehicleId.GetValueOrDefault(0)==0)
                        {
                            x.TripLogId = null;
                            x.fk_Triplog = null;
                        }
                        x.ObjectState = ObjectState.Modified;
                    });
                }

                var fuelids = ts.TripExpenses.Where(x => x.TripAdvanceLogId > 0).Select(y => y.TripAdvanceLogId).ToList();
                if (ts.TripExpenses != null && ts.TripExpenses.Any())
                {
                    ts.TripExpenses.ForEach(x => {
                        x.SettlementId = null;
                        x.fk_Settlement = null;
                        x.ObjectState =x.IsBudgeted? ObjectState.Modified: ObjectState.Deleted;
                    });
                }
                if (ts.TripLogs != null && ts.TripLogs.Any())
                {
                    ts.TripLogs.ForEach(x => {
                        x.SettlementId = null;
                        x.fk_TripSettlement = null;
                        x.ObjectState = ObjectState.Modified;
                    });
                }
                
                var voucherid = new List<long>() { ts.VoucherId.GetValueOrDefault(), ts.SetlBalFuelVoucherId.GetValueOrDefault(), ts.SetlBalVoucherId.GetValueOrDefault(), ts.NetBalVoucherId.GetValueOrDefault()}.Where(x=>x>0);
                var advoucherid = new List<long>() { ts.SetlBalFuelVoucherId.GetValueOrDefault(), ts.SetlBalVoucherId.GetValueOrDefault() }.Where(x => x > 0);
                ts.ObjectState = ObjectState.Deleted;
                
                _service.Delete(ts);

                if (fuelids != null && fuelids.Any())
                {
                    await uow.ExecSqlQueryAsync($"update tal set tal.BalanceQty= (tal.FuelQty-ISNULL(xtal.ConsumedFuel,0)) from dbo.tTripAdvanceLog as tal left join (select TripAdvancelogId,ConsumedFuel=SUM(isnull(te.FuelQty,0)) from dbo.tTripExpenseLog as te group by te.TripAdvancelogId) as xtal on tal.Id=xtal.TripAdvanceLogId where tal.AdvanceTypeId=3 and tal.Id IN({(fuelids.JoinStrings(","))})");
                }

                if (ts.HVPId > 0&& voucherid.Any())
                {
                    await _service.ExecuteSqlAsync($"delete [dbo].[tTripAdvanceLog] WHERE VoucherId IN({(voucherid.JoinStrings(","))})");
                }
                await uow.SaveChangesAsync();
                if (tsbaladvid > 0)
                {
                    await uow.ExecSqlQueryAsync($"DELETE [dbo].[tTripAdvanceLog] WHERE Id=@p0", tsbaladvid);
                    if (tsbaladv_vchid > 0)
                    {
                        await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVouchers] WHERE Id=@p0", tsbaladv_vchid);
                    }
                }
                if (advoucherid.Any())
                {
                    await _service.ExecuteSqlAsync($"delete [dbo].[tTripAdvanceLog] WHERE VoucherId IN({(advoucherid.JoinStrings(","))})");
                }
                if (voucherid.Any())
                {
                    await _service.ExecuteSqlAsync($"delete [dbo].[tVouchers] WHERE Id IN ({(voucherid.JoinStrings(","))})");
                }

                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch (Exception)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }
            
        }
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
            string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            VehicleTripSettlement t = null;
            if (!VehicleTripSettlementExists(key))
            {
                return NotFound();
            }
            var childkey = Request.GetKeyValue<long>(link);
            switch (navigationProperty)
            {
                case "TripAdvances":
                    var advance = await uow.RepositoryAsync<TripAdvanceLog>().FindAsync(childkey);
                    t = await _service.Queryable().Where(x => x.Id == key).Include(x => x.TripAdvances.Select(y=>y.CashAmount+y.FuelAmount)).FirstOrDefaultAsync();
                    if (advance == null)
                    {
                        return NotFound();
                    }
                    t.TripAdvances.Add(advance);
                    t.TripAdvanceAmt = t.TripAdvances.Sum(y => y.CashAmount + y.FuelAmount);
                    break;
                case "TripExpenses":
                    t = await _service.Queryable().Where(x => x.Id == key).Include(x => x.TripExpenses.Select(y => y.SettledAmount)).FirstOrDefaultAsync();
                    var expanse = await uow.RepositoryAsync<TripExpenseLog>().FindAsync(childkey);
                    if (expanse == null)
                    {
                        return NotFound();
                    }
                    t.TripExpenses.Add(expanse);
                    t.TripExpenseAmt = t.TripExpenses.Sum(x => x.SettledAmount);
                    break;
                case "TripLogs":
                    
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("DELETE")]
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var triplog = await _service.FindAsync(key);
            if (triplog == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_TripSettlement":
                    //triplog.fk_TripSettlement = null;
                    //triplog.SettlementId = null;
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

        private bool VehicleTripSettlementExists(string tripSheetNo)
        {
            return _service.Query(e => e.TripSheetNo == tripSheetNo).Select().Any();
        }
        private bool VehicleTripSettlementExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }
        private string GetLiveDbLevelValidation(long _Id, IUnitOfWorkAsync _uow)
        {
            var livevalidationerr = _uow.SqlQueryAsync(
            "[dbo].[Proc_GBL_TS_LiveValidationV1]",
            new SqlParameter() { Value = _Id, ParameterName = "parameter1" },
            new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter9" }/*SessionId*/
            ).Result;

            if (livevalidationerr != null && livevalidationerr?.Rows?.Count > 0)
            {
                return Utilities.To<string>(livevalidationerr.Rows[0]["ErrorMessage"]);
            }
            return "";
        }

        [HttpPost]
        public async Task<IHttpActionResult> PostV2(ODataActionParameters param)
        {
            if (!(param["entity"] is VehicleTripSettlement entity))
            {
                return BadRequest("Null Parameter Not Allowed");
            }

            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction();
            }
            entity.ObjectState = entity.Id > 0 ? ObjectState.Modified : ObjectState.Added;

            #region Step1:- Validations
            if (entity.Id > 0)
            {
                var err = GetLiveDbLevelValidation(entity.Id, uow);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
            }
            #endregion
            try
            {
                await _service.CreateSettlementV4(entity, uow);

                //var methodtype = _service.GetConfigValue<int>("UseNewSettlementMethod");
                //if (methodtype == 0)
                //{
                //    await _service.CreateSettlementV2(entity, uow);
                //}
                //else if (methodtype == 4)
                //{
                //    await _service.CreateSettlementV4(entity, uow);
                //}
                //else
                //{
                //    await _service.CreateSettlementV3(entity, uow);
                //}

                await Request.GetContext().SaveChangesAsync();                

                var spname = await uow.RepositoryAsync<ReportProcedure>().FindAsync(540);
                if (spname != null)
                {
                    try
                    {
                        await uow.ExecuteProcedureAsync(spname.StoredProcedureName, new SqlParameter("TransactionId", entity.Id), new SqlParameter("TransactionNumber", entity.TripSheetNo), new SqlParameter("TransactionType", entity.ViewId));
                    }
                    catch (SqlException ex)
                    {
                        throw new BusinessException(ex);
                    }
                    
                }

                
            }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                if ((ex is DbUpdateConcurrencyException) && VehicleTripSettlementExists(entity.TripSheetNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().Commit();
            }           

            return Ok(entity.Id);           
        }
        [HttpPost]
        public async Task<IHttpActionResult> PostHireSettlementV1(ODataActionParameters param)
        {
            if (!(param["entity"] is VehicleTripSettlement entity))
            {
                return BadRequest("Null Parameter Not Allowed");
            }

            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction();
            }
            entity.ObjectState = entity.Id > 0 ? ObjectState.Modified : ObjectState.Added;

            try
            {
                await _service.HireSettlementV1(entity, uow);
                await Request.GetContext().SaveChangesAsync();
                var spname = await uow.RepositoryAsync<ReportProcedure>().FindAsync(540);
                if (spname != null)
                {
                    try
                    {
                        await uow.ExecuteProcedureAsync(spname.StoredProcedureName, new SqlParameter("TransactionId", entity.Id), new SqlParameter("TransactionNumber", entity.TripSheetNo), new SqlParameter("TransactionType", entity.ViewId));
                    }
                    catch (SqlException ex)
                    {
                        throw new BusinessException(ex);
                    }

                }

            }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                if ((ex is DbUpdateConcurrencyException) && VehicleTripSettlementExists(entity.TripSheetNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            if (!Request.IsBatchRequest())
            {
                Request.GetContext().Commit();
            }
            return Ok(entity.Id);
        }
    }
}