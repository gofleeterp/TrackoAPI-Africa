using AutoMapper;
using EntityFramework.Extensions;
using Hangfire;
using Hangfire.States;

using Newtonsoft.Json;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using System.Web.Http;
using System.Web.Management;
using System.Web.OData;
using System.Web.OData.Routing;
using System.Web.UI.WebControls;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global.CronJobs;
using TrackoApi.Service;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.AMS;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.ViewModels.FMS;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.vw.ts;
using TrackoAPI.WebUtilities.Helper;
using IsolationLevel = System.Data.IsolationLevel;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class VehicleMovementLogsController : ODataController
    //ODataController
    {
        private readonly IVehicleMovementLogService _tlRepo;
        private readonly ITripAdvanceLogService _tripAdvanceLogService;
        private readonly bool IsNewGPSBatchTripUploadEnabled;
        public VehicleMovementLogsController(IVehicleMovementLogService service, ITripAdvanceLogService advance)
        {
            _tlRepo = service;
            _tripAdvanceLogService = advance;
            IsNewGPSBatchTripUploadEnabled = _tlRepo.GetConfigValue<int>("IsNewGPSBatchTripUploadEnabled") == 1;
        }

        [HttpPost]
        public async Task<IHttpActionResult> BulkPostTripLog(ODataActionParameters parameters)
        {
            var batchId = Guid.NewGuid().ToString("N");
            //var dataSource = new Tortuga.Chain.SqlServerDataSource("TS", Tenant.TenantConnection.GetConnection());
            try
            {
                var itriplog = parameters["trps"] as IEnumerator<VehicleMovementLog>;
                if (itriplog == null) return BadRequest("No TripLog found to upload");
                var trps = itriplog.ToList();
                var uow = Request.GetContext();
                var sessionid = Helper.SessionId();
                var timestamp = DateTime.Now;
                Parallel.ForEach(trps.AsParallel(), entity =>
                {
                    entity.ObjectState = ObjectState.Added;
                    entity.CreatedDOE = timestamp;
                    entity.CreatedSessionId = sessionid;
                    entity.BatchId = batchId;
                });

                //using (var transaction = await dataSource.BeginTransactionAsync(batchId, IsolationLevel.ReadCommitted))
                //{
                //    dataSource.InsertBulk("tVehicleMovementLog", trps, SqlBulkCopyOptions.CheckConstraints);
                //    transaction.Commit();
                //}

                using (var transaction = new TransactionScope())
                {
                    uow.BulkInsert(trps);
                    transaction.Complete();
                }
                var item = new vwBatch { BatchId = batchId, BatchSize = trps.Count };
                return Ok(item);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] long key, string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var triplog = await _tlRepo.FindAsync(key);
            if (triplog == null)
            {
                return NotFound();
            }
            var newrecordid = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_Voucher":
                    if (!uow.RepositoryAsync<Voucher>().Queryable().Any(x => x.Id == newrecordid))
                    {
                        return NotFound();
                    }
                    triplog.VoucherId = newrecordid;
                    triplog.ObjectState = ObjectState.Modified;
                    break;

                case "fk_TDSVoucher":
                    if (!uow.RepositoryAsync<Voucher>().Queryable().Any(x => x.Id == newrecordid))
                    {
                        return NotFound();
                    }
                    triplog.HSTDSVoucherId = newrecordid;
                    triplog.ObjectState = ObjectState.Modified;
                    break;

                case "fk_VDR":
                    if (!uow.RepositoryAsync<VoucherDetailReference>().Queryable().Any(x => x.Id == newrecordid))
                    {
                        return NotFound();
                    }
                    triplog.VDRId = newrecordid;
                    triplog.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpPost]
        public async Task<IHttpActionResult> DeepPost(ODataActionParameters param)
        {
            var json = param["entity"]?.ToString();
            if (string.IsNullOrWhiteSpace(json))
            {
                return BadRequest("Required Entity Not Found");
            }
            var tl = JsonConvert.DeserializeObject<VehicleMovementLog>(json);
            var uow = Request.GetContext();
            if (tl.TripTypeId != 1159 && _tlRepo.Queryable().Any(x => x.VehicleId == tl.VehicleId && x.HireVehicleId == tl.HireVehicleId && tl.TripTypeId == 1664 && tl.UnloadingDate == null))
            {
                return BadRequest(
                    "Vehicle already have pending schedule. so cannot create new trip or schedule before either closing it or rejecting it.");
            }
            await CheckDateOverlap(tl);
            if (tl.TripStartDate.Date > DateTime.Now.Date.AddDays(30))
            {
                return BadRequest("Future Date is not Allowed.");
            }
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction(IsolationLevel.ReadCommitted);
            }

            try
            {
                List<VehicleMovementLogPickupDrop> wps = null;
                tl.ObjectState = ObjectState.Added;
                _tlRepo.Insert(tl);

                #region Waypoints

                tl.WayPoints?.ForEach(x => x.ObjectState = ObjectState.Added);
                tl.TripAdvances?.ForEach(x => x.ObjectState = ObjectState.Added);
                if ((tl.CreateWayPointOnServer || tl.FormId == "5001") && tl.VehicleId > 0 /*Run only for Own Vehicles*/ && tl.RouteId > 0)
                {
                    try
                    {
                        var wpRepo = uow.RepositoryAsync<RouteWayPoint>();
                        var waypoints = await wpRepo.Queryable().Where(x => x.RouteId == tl.RouteId).Select(x =>
                            new
                            {
                                RouteId = x.RouteId,
                                CityId = x.CityId,
                                GeographyPoint = x.GeographyPoint,
                                KM = x.Distance,
                                Latitude = x.Latitude,
                                Longitude = x.Longitude,
                                Order = x.OrderId,
                                TravalTime = x.TransitTime,
                                TypeId = x.TypeId
                            }).ToListAsync();
                        wps = waypoints.Select(x => new VehicleMovementLogPickupDrop
                        {
                            RouteId = x.RouteId,
                            CityId = x.CityId,
                            GeographyPoint = x.GeographyPoint,
                            KM = (int)x.KM,
                            Latitude = (decimal)x.Latitude,
                            Longitude = (decimal)x.Longitude,
                            Order = x.Order,
                            OriginLocationId = tl.FromPlaceId ?? waypoints.OrderBy(y => y.Order).FirstOrDefault()?.CityId ?? 0,
                            StopageTime = 0,
                            TravalTime = x.TravalTime,
                            TriplogId = tl.Id,
                            fk_Triplog = tl,
                            TypeId = x.TypeId.GetValueOrDefault(),
                            ObjectState = ObjectState.Added
                        }).ToList();
                        uow.RepositoryAsync<VehicleMovementLogPickupDrop>().InsertGraphRange(wps);
                        tl.KmRun = wps.Sum(x => x.KM);
                        tl.TotalKmRun = tl.KmRun + tl.AdditionalKmRun;
                        //await uow.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        return BadRequest("Unable to Created RouteWay Points from server side when trip posted from Mobile app using View Id 5001");
                    }
                }

                #endregion Waypoints

                #region Budgeted Expense

                if ((tl.BookBudgetingOnServer || tl.FormId == "5001" || tl.RefreshBudgetingOnServer) &&
                    tl.VehicleId > 0 /*Run only for Own Vehicles*/ && tl.RouteId > 0)
                {
                    try
                    {
                        if (tl.RefreshBudgetingOnServer)
                        {
                            await uow.ExecSqlQueryAsync("DELETE FROM [dbo].[tTripExpenseLog] WHERE TripLogId=@tlid", new SqlParameter("tlid", tl.Id));
                        }
                        var expenseRaw = await uow.SqlQueryAsync(
                            "[dbo].[Proc_TRNS_TripBdgtV2_Show]",
                            new SqlParameter() { Value = tl.RouteId, ParameterName = "parameter1" }/*RouteId*/,
                            new SqlParameter() { Value = tl.VehicleId, ParameterName = "parameter2" }/*VehicleId*/,
                            new SqlParameter() { Value = tl.TripStartDate, ParameterName = "parameter3" }/*TripDate*/,
                            new SqlParameter() { Value = tl.TripNatureId, ParameterName = "parameter4" }/*TripNature*/);
                        var expRepo = uow.RepositoryAsync<TripExpenseLog>();
                        if (expenseRaw != null)
                        {
                            foreach (DataRow row in expenseRaw.Rows)
                            {
                                var exp = new TripExpenseLog
                                {
                                    SettledAmount = Utilities.To<decimal>(row["PaidAmount"]),
                                    ClaimAmount = Utilities.To<decimal>(row["BudgetedAmount"]),
                                    ExpenseTypeId = Utilities.To<long>(row["ExpenseId"]),
                                    FuelQty = 0,
                                    FuelRate = 0,
                                    BudgetedQty = Utilities.To<decimal>(row["BudgetedQty"]),
                                    TripLogId = tl.Id,
                                    ViewId = string.IsNullOrWhiteSpace(tl.FormId) ? 1576 : long.Parse(tl.FormId),
                                    IsAuto = true,//changes done sanjay
                                    IsBudgeted = true,//changes done sanjay
                                    ObjectState = ObjectState.Added
                                };
                                expRepo.Insert(exp);
                                tl.TripExpenses?.Add(exp);
                            }
                            tl.BdgtFuelQty = tl.TripExpenses.Where(x => x.IsBudgeted && x.BudgetedQty > 0).Sum(x => x.BudgetedQty);
                            tl.BdgtAdvance = tl.BdgtTripExpense = tl.TripExpenses.Where(x => x.IsBudgeted && x.BudgetedQty == 0).Sum(x => x.ClaimAmount);
                            if (tl.ObjectState != ObjectState.Added)
                            {
                                tl.ObjectState = ObjectState.Modified;
                            }
                            await uow.SaveChangesAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        return BadRequest("Unable to Created Budgeted Trip Expense from server side when trip posted from Mobile app using View Id 5001");
                    }
                }

                #endregion Budgeted Expense

                tl.ChallanCNs?.ForEach(x =>
                {
                    x.ObjectState = ObjectState.Added;
                    if (x.fk_CNMaster != null)
                    {
                        x.fk_CNMaster.ObjectState = ObjectState.Added;
                        x.fk_CNMaster.StockLogs?.ForEach(y =>
                        {
                            y.ObjectState = ObjectState.Added;
                            y.StockMMLogs?.ForEach(z =>
                            {
                                z.ObjectState = ObjectState.Added;
                            });
                        });
                    }

                    x.CnStockLogs?.ForEach(y =>
                    {
                        y.ObjectState = ObjectState.Added;
                        y.StockMMLogs?.ForEach(z =>
                        {
                            z.ObjectState = ObjectState.Added;
                        });
                    });
                });
                await Request.GetContext().SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                if (!IsNewGPSBatchTripUploadEnabled)
                {
                    if (wps != null && wps.Any())
                    {
                        foreach (var log in wps)
                        {
                            await _tlRepo.PushToGpsProviderAsync(log);
                        }
                    }
                    if (tl.WayPoints != null && tl.WayPoints.Any())
                    {
                        foreach (var log in tl.WayPoints)
                        {
                            await _tlRepo.PushToGpsProviderAsync(log);
                        }
                    }
                }
                else
                {
                    await _tlRepo.ScheduleTripPushToGPSAsync(tl.Id, tl.RouteId);
                }
                if (tl.UnloadingDate != null)
                {
                    if (_tlRepo.Queryable().Any(x => x.ParentTLId == tl.Id))
                    {
                        var childtlid = await _tlRepo.Queryable().Where(x => x.ParentTLId == tl.Id).Select(x => x.Id).FirstAsync();
                        BackgroundJob.Enqueue<IHangfireJobProcessor>(x => x.PushChildTrip(childtlid, Helper.LoggedInTenantId, 0, null));
                    }
                }                

                return Created(tl);
            }
            catch
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }

                throw;
            }
        }

        // DELETE: odata/VehicleMovementLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction();
            }
            try
            {
                if (_tlRepo.GetConfigValue<int>("IsVTSEnabled") != 0)
                {
                    await uow.ExecSqlQueryAsync($"EXEC [dbo].[Proc_TRANS_1624_RemoveTLVTS]{key}");
                }

                var cnStockRepo = uow.RepositoryAsync<CNStockLog>();
                var cnno =
                    await cnStockRepo.Queryable()
                        .Where(
                            x => x.TriplogId == key && x.LogTypeId == 1423 && x.Outwards.Any(y => y.Outwards.Any())).Select(x => x.fk_CNMaster.CNNo).ToListAsync();
                if (cnno.Any())
                {
                    throw new BusinessException(ErrorCode.GLB106, $"These [{Utilities.JoinStrings(cnno, ",")}] Consignments are delivered or attached with next Trip");
                }
                await uow.ExecSqlQueryAsync($"DELETE [dbo].[tPickDroplog] WHERE TriplogId={key}");
                var query = uow.RepositoryAsync<HSAdvance>()
                    .Queryable()
                    .Where(x => x.HireSlipId == key);
                var hsvchids = await query.Select(x => x.VoucherId).ToListAsync();
                await query.DeleteAsync();
                if (hsvchids != null && hsvchids.Any())
                {
                    foreach (var hsvchid in hsvchids)
                    {
                        await uow.ExecSqlQueryAsync($"DELETE [dbo].[tVouchers] WHERE Id={hsvchid}");
                    }
                }

                //await uow.RepositoryAsync<Voucher>().Queryable().Where(x => hsvchids.Contains(x.Id)).DeleteAsync();
                var voucherid = await _tlRepo.Queryable()
                    .Where(x => x.Id == key)
                    .Select(x => new { x.VoucherId, x.HSTDSVoucherId })
                    .FirstOrDefaultAsync();
                await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tVehicleMovementLog] SET VoucherId=NULL,HSTDSVoucherId=NULL, VDRId=NULL WHERE Id={key};  DELETE V FROM [dbo].[tVouchers] V WHERE V.Id in({voucherid?.VoucherId ?? 0},{voucherid?.HSTDSVoucherId ?? 0}) ");
                var tl = await _tlRepo.Queryable().Where(x => x.Id == key).Include(x => x.TripExpenses)
                    .FirstOrDefaultAsync();
                if (tl == null)
                {
                    return NotFound();
                }
                if (tl.SettlementId > 0)
                {
                    throw new BusinessException(ErrorCode.GLB106, "Settled Trip cannot be deleted.");
                }
                if (tl.TripTypeId == 1159)
                {
                    await uow.ExecSqlQueryAsync("UPDATE tRepairLabourLog SET JobCardId=NULL WHERE JobCardId=@p0", key);

                    await uow.ExecSqlQueryAsync("UPDATE tVehiclePMLog SET NextLogId=NULL WHERE NextLogId in (select j.Id from dbo.tVehiclePMLog as j where j.JobCardId=@p0)", key);
                    await uow.ExecSqlQueryAsync("UPDATE tVehiclePMLog SET PreviousLogId=NULL WHERE PreviousLogId in (select j.Id from dbo.tVehiclePMLog as j where j.JobCardId=@p0)", key);

                    //await uow.RepositoryAsync<RepairLabourLog>().Queryable().Where(x => x.JobCardId == key).DeleteAsync();
                    await uow.RepositoryAsync<TyreCheck>().Queryable().Where(x => x.JobCardId == key).DeleteAsync();
                    await uow.RepositoryAsync<BatteryCheck>().Queryable().Where(x => x.JobCardId == key).DeleteAsync();
                    await uow.RepositoryAsync<VehiclePreventiveLog>().Queryable().Where(x => x.JobCardId == key).DeleteAsync();
                    await uow.RepositoryAsync<VehicleRepairJob>().Queryable().Where(x => x.JobCardId == key).DeleteAsync();
                }
                var cnrepo = uow.RepositoryAsync<CNMaster>();
                var tarepo = uow.RepositoryAsync<TripAdvanceLog>();
                var cns = await
                    cnrepo.Queryable()
                        .Where(x => x.TripLogId == tl.Id).Select(x => new
                        {
                            x.Id,
                            x.CNNo,
                            IsBillExists = x.BillLogs.Any()
                        }).ToListAsync();
                if (cns.Any())
                {
                    await cnrepo.Queryable().Where(x => x.TripLogId == tl.Id).UpdateAsync(x => new CNMaster()
                    {
                        TLLoadQty = 0,
                        TripLogId = null
                    });
                }

                tl.TripExpenses?.ForEach(x =>
                {
                    x.ObjectState = ObjectState.Deleted;
                });
                await uow.SaveChangesAsync();

                var tals = await
                    tarepo.Queryable()
                        .Where(x => x.TripLogId == tl.Id).Select(x => new
                        {
                            x.Id
                        }).ToListAsync();
                if (tals.Any())
                {
                    if (uow.Context.GetApiConfig<int>("DeniedTLDeleteIfTALMapped") == 1)
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                                $"API Response:Delete Tripadvance(s) before deleting the Trip");
                    }
                    else if (uow.Context.GetApiConfig<int>("TLDeleteActionOnSameSourceAdvance") == 1)
                    {
                        var vchIds =
                            await
                                tarepo.Queryable()
                                    .Where(x => x.TripLogId == tl.Id && x.VoucherId != null)
                                    .Select(x => x.VoucherId)
                                    .ToListAsync();
                        await tarepo.Queryable().Where(x => x.TripLogId == tl.Id).DeleteAsync();
                        var vouchers =
                            await uow.RepositoryAsync<Voucher>().Queryable().Where(x => vchIds.Contains(x.Id)).ToListAsync();

                        vouchers?.ForEach(x => x.ObjectState = ObjectState.Deleted);
                    }
                    else
                    {
                        await tarepo.Queryable().Where(x => x.TripLogId == tl.Id).UpdateAsync(x => new TripAdvanceLog
                        {
                            TripLogId = null
                        });
                    }
                }

                await uow.RepositoryAsync<CnChallan>().Queryable().Where(x => x.TriplogId == tl.Id).Include(x => x.fk_Challan).LoadAsync();
                //var deletedTripExpenses = uow.Context.Delete<TripExpenseLog>(x => x.TripLogId == tl.Id&&x.TripAdvanceLogId==null);
                //var deletedTripAdvances = uow.Context.Delete<TripAdvanceLog>(x => x.TripLogId == tl.Id);
                //var deletedChallan = uow.Context.Delete<TripAdvanceLog>(x => x.TripLogId == tl.Id);
                //_tlRepo.Delete(tl);
                tl.Challans?.ForEach(x => x.ObjectState = ObjectState.Deleted);
                tl.ChallanCNs?.ForEach(x => x.ObjectState = ObjectState.Deleted);
                await uow.Context.MaterialDispatchOrders.Where(x => x.DispatchId == tl.Id).DeleteAsync();
                //uow.Context.Delete<MaterialDispatchOrder>(x => x.DispatchId == tl.Id);
                await uow.SaveChangesAsync();
                
                if (cns.Any())
                {
                    if (uow.Context.GetApiConfig<int>("DeniedTLDeleteIfCNMapped") == 1)
                    {
                        throw new BusinessException(ErrorCode.GLB106,
                                $"API Response:Delete Consignment(s) before deleting the Trip");
                    }
                    else if (uow.Context.GetApiConfig<int>("TLDeleteActionOnSameSourceLR") == 1 && cns.Any())
                    {
                        if (cns.Any(x => x.IsBillExists))
                        {
                            throw new BusinessException(ErrorCode.GLB106,
                                $"Unable to delete attached consignments as these {Utilities.JoinStrings(cns.Where(x => x.IsBillExists), ",")} are billed.");
                        }
                        var cnids = cns.Select(x => x.Id).ToList();

                        var cnNos =
                            await uow.RepositoryAsync<CNStockLog>().Queryable().Where(x => cnids.Contains(x.CNId) && x.ChallanCNId > 0).Select(x => new { x.fk_CNMaster.CNNo, TLNo = x.fk_Triplog != null ? x.fk_Triplog.TriplogNo : null }).Distinct().ToListAsync();
                        if (cnNos.Any())
                        {
                            throw new BusinessException(ErrorCode.GLB106, $"Consignment/LR cannot be deleted as they are attached in another TripLog.\n Details: {Utilities.JoinStrings(cnNos.Select(x => x.CNNo + " in " + x.TLNo), ",\n")}");
                        }
                        cns.ForEach(x =>
                        {
                            var cn = new CNMaster
                            {
                                Id = x.Id
                            };
                            cnrepo.Attach(cn);
                            cn.ObjectState = ObjectState.Deleted;
                        });
                        var salesvouchers = await uow.RepositoryAsync<SalesLog>().Queryable().Where(x => x.CNId != null && cnids.Contains((long)x.CNId)).Select(x => new { x.Id, x.CNId, x.SalesVoucherId, x.VDRId }).ToListAsync();
                        if (salesvouchers.Any())
                        {
                            await uow.ExecSqlQueryAsync($"UPDATE [dbo].[tSalesLog] SET SalesVoucherId=NULL,VDRId=NULL WHERE Id in({Utilities.JoinStrings(salesvouchers.Select(x => x.Id), ",")});" +
                                $"DELETE [dbo].[tVouchers] WHERE Id in({Utilities.JoinStrings(salesvouchers.Select(x => x.SalesVoucherId), ",")});" +
                                $"DELETE [dbo].[tSalesLog] WHERE Id in({Utilities.JoinStrings(salesvouchers.Select(x => x.Id), ",")});");
                        }
                        await uow.SaveChangesAsync();
                    }
                }

                tl.ObjectState = ObjectState.Deleted;
                
                if (tl.TripTypeId == 1159)
                {
                    try
                    {
                        var v1 = await uow.SqlQueryAsync(
                        "[dbo].[Proc_GBL_TSL_Delete]",
                        new SqlParameter() { Value = 0, ParameterName = "parameter1" },//TSLID
                        new SqlParameter() { Value = tl.Id, ParameterName = "parameter2" },//TransactionId
                        new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter3" },//CSId
                        new SqlParameter() { Value = tl.FormId, ParameterName = "parameter4" }//ViewId
                        );
                    }
                    catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
                }

                await uow.SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
            }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("DELETE")]
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] long key,
        string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var triplog = await _tlRepo.FindAsync(key);
            if (triplog == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_TripSettlement":
                    triplog.fk_TripSettlement = null;
                    triplog.SettlementId = null;
                    break;

                case "fk_Voucher":
                    triplog.VoucherId = null;
                    triplog.fk_Voucher = null;
                    triplog.ObjectState = ObjectState.Modified;
                    break;

                case "fk_TDSVoucher":
                    triplog.HSTDSVoucherId = null;
                    triplog.fk_TDSVoucher = null;
                    triplog.ObjectState = ObjectState.Modified;
                    break;

                case "fk_VDR":
                    triplog.VDRId = null;
                    triplog.fk_VDR = null;
                    triplog.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        public async Task<IHttpActionResult> DeleteRef([FromODataUri] long key, [FromODataUri] string relatedKey, string navigationProperty)
        {
            var uow = Request.GetContext();
            var triplog = await _tlRepo.FindAsync(key);
            if (triplog == null)
            {
                return StatusCode(HttpStatusCode.NotFound);
            }
            switch (navigationProperty)
            {
                case "TripAdvances":
                    var advanceid = Convert.ToInt32(relatedKey);
                    var advance = await uow.RepositoryAsync<TripAdvanceLog>().FindAsync(advanceid);
                    if (advance == null)
                    {
                        return NotFound();
                    }
                    if (advance.SettlementId.HasValue && advance.SettlementId.Value > 0)
                    {
                        return BadRequest($"Settled Advance {advance.VoucherNo} Cannot be Unmapped");
                    }
                    advance.TripLogId = null;
                    advance.fk_Triplog = null;
                    advance.VoucherId = advance.VoucherId;
                    advance.fk_Voucher = null;
                    advance.ObjectState = ObjectState.Modified;
                    break;

                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // GET: odata/VehicleMovementLogs
        [HttpGet, EnableQuery(MaxExpansionDepth = 3)]
        public IQueryable<VehicleMovementLog> Get()
        {
            return _tlRepo.Queryable();
        }

        // GET: odata/VehicleMovementLogs(5)
        [EnableQuery(MaxExpansionDepth = 3)]
        public SingleResult<VehicleMovementLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_tlRepo.Queryable().Where(t => t.Id == key));
        }

        //// PATCH: odata/VehicleMovementLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<VehicleMovementLog> patch)
        {
            var uow = Request.GetContext();
            VehicleMovementLog vml = await _tlRepo.FindAsync(key);
            if (vml == null)
            {
                return NotFound();
            }
            patch.TryGetPropertyValue("Data", out var dv);
            patch.Patch(vml);
            if (vml.FormId == "1576")
            {
                vml.SHRT_VoucherDate = vml.UnloadingDate;
            }
            try
            {
                var futuredatetriptypes = new List<long?> { 1453/*Local Dispatch*/, 1664/*Trip Schedule*/ };
                var alloweddate = (futuredatetriptypes.Contains(vml.TripTypeId) ? DateTime.Now.AddDays(5) : DateTime.Now.AddHours(1));
                if (vml.TripStartDate.Date > alloweddate)
                {
                    ModelState.AddModelError("TripStartDate", "Future Date is not Allowed.");
                }
                if (vml.LoadingReachDate != null && vml.LoadingReachDate > alloweddate)
                {
                    ModelState.AddModelError("LoadingReachDate", "Future Date is not Allowed.");
                }
                if (vml.LoadingDate != null && vml.LoadingDate > alloweddate)
                {
                    ModelState.AddModelError("LoadingDate", "Future Date is not Allowed.");
                }
                if (vml.UnloadingReachDate != null && vml.UnloadingReachDate > alloweddate)
                {
                    ModelState.AddModelError("UnloadingReachDate", "Future Date is not Allowed.");
                }
                if (vml.UnloadingDate != null && vml.UnloadingDate > alloweddate)
                {
                    ModelState.AddModelError("UnloadingDate", "Future Date is not Allowed.");
                }

                if (vml.LoadingReachDate != null && vml.TripStartDate > vml.LoadingReachDate)
                {
                    ModelState.AddModelError("LoadingReachDate", $"Trip No :{vml.TriplogNo} Loading ReachDate has to be greater than Trip Start Date");
                }
                if (vml.LoadingDate != null && vml.LoadingReachDate.GetValueOrDefault(vml.TripStartDate) > vml.LoadingDate)
                {
                    ModelState.AddModelError("LoadingDate", $"Trip No :{vml.TriplogNo} Loading Date has to be greater than Loading ReachDate");
                }
                if (vml.UnloadingReachDate != null && vml.LoadingDate.GetValueOrDefault(vml.LoadingReachDate.GetValueOrDefault(vml.TripStartDate)) > vml.UnloadingReachDate)
                {
                    ModelState.AddModelError("UnloadingReachDate", $"Trip No :{vml.TriplogNo} Unloading Reach Date has to be greater than Loading Date");
                }
                if (vml.TripTypeId == 1158 || (vml.TripTypeId == 1160 && vml.VehicleId != null))
                {
                    if (vml.UnloadingDate != null && vml.UnloadingReachDate.GetValueOrDefault(vml.LoadingDate.GetValueOrDefault(vml.LoadingReachDate.GetValueOrDefault(vml.TripStartDate))) > vml.UnloadingDate)
                    {
                        ModelState.AddModelError("UnloadingDate", $"Trip No :{vml.TriplogNo} UnloadingDate {vml.UnloadingDate:dd-MMM-yyyy HH:mm:ss tt} has to be greater than Unloading Reach Date  {vml.UnloadingReachDate.GetValueOrDefault(vml.LoadingDate.GetValueOrDefault(vml.LoadingReachDate.GetValueOrDefault(vml.TripStartDate))):dd-MMM-yyyy HH:mm:ss tt} AND Loading Date");
                    }
                }
                var err = GetLiveDbLevelValidation(vml, uow);
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
            }
            catch (Exception e)
            {
                //Ignore
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            vml.ObjectState = ObjectState.Modified;

            if (dv is List<JsonDataEntity> dataview && dataview.Any())
            {
                foreach (var entity in dataview)
                {
                    vml.DeleteAndAdd(entity);
                }
            }
            await CheckDateOverlap(vml);

            try
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().BeginTransaction(IsolationLevel.ReadCommitted);
                }
                await uow.SaveChangesAsync();
                List<VehicleMovementLogPickupDrop> wps = null;
                if ((vml.CreateWayPointOnServer || vml.FormId == "5001") && vml.VehicleId > 0/*Run only for Own Vehicles*/&& vml.RouteId > 0)
                {
                    try
                    {
                        var wpRepo = uow.RepositoryAsync<RouteWayPoint>();
                        var waypoints = await wpRepo.Queryable().Where(x => x.RouteId == vml.RouteId).Select(x =>
                            new
                            {
                                RouteId = x.RouteId,
                                CityId = x.CityId,
                                GeographyPoint = x.GeographyPoint,
                                KM = x.Distance,
                                Latitude = x.Latitude,
                                Longitude = x.Longitude,
                                Order = x.OrderId,
                                TravalTime = x.TransitTime,
                                TypeId = x.TypeId
                            }).ToListAsync();
                        wps = waypoints.Select(x => new VehicleMovementLogPickupDrop
                        {
                            RouteId = x.RouteId,
                            CityId = x.CityId,
                            GeographyPoint = x.GeographyPoint,
                            KM = (int)x.KM,
                            Latitude = (decimal)x.Latitude,
                            Longitude = (decimal)x.Longitude,
                            Order = x.Order,
                            OriginLocationId = vml.FromPlaceId ?? waypoints.OrderBy(y => y.Order).FirstOrDefault()?.CityId ?? 0,
                            StopageTime = 0,
                            TravalTime = x.TravalTime,
                            TriplogId = vml.Id,
                            fk_Triplog = vml,
                            TypeId = x.TypeId.GetValueOrDefault(),
                            ObjectState = ObjectState.Added
                        }).ToList();
                        uow.RepositoryAsync<VehicleMovementLogPickupDrop>().InsertGraphRange(wps);
                    }
                    catch (Exception e)
                    {
                        return BadRequest("Unable to Created RouteWay Points from server side when trip posted from Mobile app using View Id 5001");
                    }

                    vml.KmRun = wps.Sum(x => x.KM);
                    vml.TotalKmRun = vml.KmRun + vml.AdditionalKmRun;
                }

                

                if ((vml.BookBudgetingOnServer || vml.FormId == "5001" || vml.RefreshBudgetingOnServer) && vml.VehicleId > 0/*Run only for Own Vehicles*/&& (vml.RouteId > 0 && vml.TripTypeId == 1158 || vml.TripTypeId == 1160) && (!await uow.RepositoryAsync<TripExpenseLog>().Queryable()
                        .AnyAsync(x => x.TripLogId == vml.Id && x.IsBudgeted) || vml.RefreshBudgetingOnServer) && vml.SettlementId.GetValueOrDefault() == 0)
                {
                    try
                    {
                        if (vml.RefreshBudgetingOnServer)
                        {
                            await uow.ExecSqlQueryAsync("DELETE FROM [dbo].[tTripExpenseLog] WHERE TripLogId=@tlid", new SqlParameter("tlid", vml.Id));
                        }
                        var expenseRaw = await uow.SqlQueryAsync(
                            "[dbo].[Proc_TRNS_TripBdgtV2_Show]",
                            new SqlParameter() { Value = vml.RouteId, ParameterName = "parameter1" }/*RouteId*/,
                            new SqlParameter() { Value = vml.VehicleId, ParameterName = "parameter2" }/*VehicleId*/,
                            new SqlParameter() { Value = vml.TripStartDate, ParameterName = "parameter3" }/*TripDate*/,
                            new SqlParameter() { Value = vml.TripNatureId, ParameterName = "parameter4" }/*TripNature*/);
                        var expRepo = uow.RepositoryAsync<TripExpenseLog>();
                        if (expenseRaw != null)
                        {
                            foreach (DataRow row in expenseRaw.Rows)
                            {
                                var exp = new TripExpenseLog
                                {
                                    SettledAmount = Utilities.To<decimal>(row["PaidAmount"]),
                                    ClaimAmount = Utilities.To<decimal>(row["BudgetedAmount"]),
                                    ExpenseTypeId = Utilities.To<long>(row["ExpenseId"]),
                                    FuelQty = 0,
                                    FuelRate = 0,
                                    BudgetedQty = Utilities.To<decimal>(row["BudgetedQty"]),
                                    TripLogId = vml.Id,
                                    ViewId = string.IsNullOrWhiteSpace(vml.FormId) ? 1576 : long.Parse(vml.FormId),
                                    IsAuto = true,//changes done sanjay
                                    IsBudgeted = true,//changes done sanjay
                                    ObjectState = ObjectState.Added
                                };
                                expRepo.Insert(exp);
                                vml.TripExpenses?.Add(exp);
                            }
                            //vml.BdgtFuelQty = vml.TripExpenses.Where(x => x.IsBudgeted && x.BudgetedQty > 0).Sum(x => x.BudgetedQty);
                            if ((vml.TripExpenses?.Count ?? 0) > 0)
                            {
                                var budgetedQty = vml.TripExpenses.Where(x => x.IsBudgeted).Sum(x => x.FuelQty);
                                if (budgetedQty > 0)
                                {
                                    vml.BdgtFuelQty = budgetedQty;
                                }
                                vml.BdgtAdvance = vml.BdgtTripExpense = vml.TripExpenses.Where(x => x.IsBudgeted && x.BudgetedQty == 0).Sum(x => x.ClaimAmount);
                            }
                            vml.ObjectState = ObjectState.Modified;
                        }
                    }
                    catch (Exception e)
                    {
                        // var signal=Request.GetHeader("SignalRConnectionId");

                        vml.Data?.Add(new JsonDataEntity
                        {
                            DataName = "Errors",
                            RawJson = JsonConvert.SerializeObject(new
                            {
                                BudgetError = $"{e.GetBaseException().ToString()}"
                            })
                        });
                    }
                }

                if (!string.IsNullOrWhiteSpace(vml.CnChallanJson))
                {
                    try
                    {
                        var cnchallans = JsonConvert.DeserializeObject<List<CnChallan>>(vml.CnChallanJson);
                        if (cnchallans.Any())
                        {
                            var repo = uow.RepositoryAsync<CnChallan>();
                            foreach (var chcn in cnchallans)
                            {
                                if (chcn.Id > 0)
                                {
                                    //var delta = new Delta<CnChallan>();
                                    //delta.CopyChangedValues(chcn);
                                    var chcnorg = await repo.FindAsync(chcn.Id);
                                    Mapper.Map(chcn, chcnorg);
                                    //delta.Patch(chcnorg);
                                    chcnorg.TriplogId = vml.Id;
                                    chcnorg.fk_Triplog = vml;
                                    chcnorg.ObjectState = ObjectState.Modified;
                                }
                                else
                                {
                                    chcn.TriplogId = vml.Id;
                                    chcn.fk_Triplog = vml;
                                    chcn.ObjectState = ObjectState.Added;
                                    var item = repo.Insert(chcn);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return BadRequest($"Malformed data found in CnChallan section \n Details:{ex.GetBaseException().Message}");
                    }
                }
                if (vml.VehicleId.GetValueOrDefault()>0 && vml.Driver1stId.GetValueOrDefault()>0)
                {
                    if (!string.IsNullOrWhiteSpace(vml.DriverPhone) && vml.DriverPhone.Length >= 10)
                    {
                        var drivercontact = await uow.RepositoryAsync<DriverMaster>().Queryable()
                            .Where(x => x.Id == vml.Driver1stId).FirstOrDefaultAsync();
                        if (vml.DriverPhone != drivercontact.DriverContactNo1)
                        {
                            drivercontact.DriverContactNo1 = vml.DriverPhone;
                            drivercontact.ObjectState = ObjectState.Modified;
                        }
                    }
                }

                await uow.SaveChangesAsync();
                await RunDateLogic(vml);
                if (vml.VehicleId.GetValueOrDefault() > 0 && (vml.TripTypeId == 1158 || vml.TripTypeId == 1453 || (vml.TripTypeId == 1160 && vml.VehicleId != null)))
                {
                    await _tlRepo.Attach_eTolls(vml.VehicleId, vml.HireVehicleId, vml.Driver1stId, vml.Id, vml.TripStartDate,
                        vml.UnloadingDate ?? DateTime.Now);
                }
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                string tripprocessJobId = "";
                if (_tlRepo.GetConfigValue<int>("RunTripPostProcess") == 1)
                {
                    var differTime = _tlRepo.GetConfigValue<double>("TripPostProcessTriggerInterval");
                    if (differTime < 2)
                    {
                        differTime = 2;
                    }
                    tripprocessJobId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunTripPostProcess(null, vml.Id, Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(differTime));
                    //tripprocessJobId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunTripPostProcess(null,vml.Id, Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(1));
                }
                if (_tlRepo.GetConfigValue<int>("RunFuelAutomationProcess") == 1)
                {
                    if (!string.IsNullOrWhiteSpace(tripprocessJobId) && _tlRepo.GetConfigValue<int>("RunFuelAutomationAfterTripPostTask") == 1)
                    {
                        BackgroundJob.ContinueJobWith<IHangfireJobProcessor>(tripprocessJobId, x => x.RunFuelAutomation(vml.Id, Helper.SessionId(), Helper.LoggedInTenantId));
                    }
                    else
                    {
                        var differTime = _tlRepo.GetConfigValue<double>("FuelAutomationTriggerInterval");
                        if (differTime < 2)
                        {
                            differTime = 2;
                        }
                        BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunFuelAutomation(vml.Id, Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(differTime));
                    }
                }
                if (!IsNewGPSBatchTripUploadEnabled)
                {
                    if (wps != null && wps.Any())
                    {
                        foreach (var log in wps)
                        {
                            await _tlRepo.PushToGpsProviderAsync(log);
                        }
                    }
                }
                else
                {
                    await _tlRepo.ScheduleTripPushToGPSAsync(vml.Id, vml.RouteId);
                }
                if (vml.UnloadingDate != null)
                {
                    if (_tlRepo.Queryable().Any(x => x.ParentTLId == vml.Id))
                    {
                        var childtlid = await _tlRepo.Queryable().Where(x => x.ParentTLId == vml.Id).Select(x => x.Id).FirstAsync();
                        BackgroundJob.Enqueue<IHangfireJobProcessor>(x => x.PushChildTrip(childtlid, Helper.LoggedInTenantId, 0, null));
                    }
                }

                if (vml.FormId == "1571")
                {
                    try
                    {
                        var v1 = await uow.SqlQueryAsync(
                    "[dbo].[Proc_TRANS_1571_SaveShortage]",
                    new SqlParameter() { Value = vml.Id, ParameterName = "parameter1" }/*Id*/,
                    new SqlParameter() { Value = vml.CreatedSessionId, ParameterName = "parameter2" }/*CSID*/);
                    }
                    catch (SqlException ex) { throw new BusinessException(ErrorCode.GLB104, ex.Message, ex); }
                    catch (SqlExecutionException ex) { throw new BusinessException(ErrorCode.GLB104, ex.Message, ex); }
                }
                if (vml.TripTypeId == 1159)
                {
                    try
                    {
                        var v1 = await uow.SqlQueryAsync(
                        "[dbo].[Proc_GBL_TSL_Save]",
                        new SqlParameter() { Value = vml.Id, ParameterName = "parameter1" },
                        new SqlParameter() { Value = vml.CreatedSessionId, ParameterName = "parameter2" },
                        new SqlParameter() { Value = vml.FormId, ParameterName = "parameter3" },
                        new SqlParameter() { Value =JsonConvert.SerializeObject(vml.TSLs), ParameterName = "parameter4" }
                        );
                    }
                    catch (SqlException ex) { throw new BusinessException(ErrorCode.GLB112, ex.Message, ex); }
                    catch (SqlExecutionException ex) { throw new BusinessException(ErrorCode.GLB112, ex.Message, ex); }
                    
                    try
                    {
                        var v1 = await uow.SqlQueryAsync(
                        "[dbo].[Proc_GBL_CreateAPLData]",
                        new SqlParameter() { Value = vml.Id, ParameterName = "parameter1" },
                        new SqlParameter() { Value = vml.TriplogNo, ParameterName = "parameter3" },
                        new SqlParameter() { Value = vml.FormId, ParameterName = "parameter2" }
                        );
                    }
                    catch (SqlException ex) { throw new BusinessException(ErrorCode.GLB104, ex.Message, ex); }
                    catch (SqlExecutionException ex) { throw new BusinessException(ErrorCode.GLB104, ex.Message, ex); ; }
                }
                return Updated(vml);
            }
            catch (DbUpdateException ex)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                return BadRequest(ex.Message);
            }
            catch (SqlException ex)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                return BadRequest(ex.Message);
            }
            catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw new BusinessException(ErrorCode.GLB104, ex.Message, ex);
            }
        }

        // POST: odata/VehicleMovementLogs
        public async Task<IHttpActionResult> Post(VehicleMovementLog tl)
        {
            if (tl == null)
            {
                return BadRequest("Entity Null is not Allowed");
            }

            if (tl.FormId == "1576") {
                tl.SHRT_VoucherDate = tl.UnloadingDate;
            }

            var futuredatetriptypes = new List<long?> { 1453/*Local Dispatch*/, 1664/*Trip Schedule*/ };
            var alloweddate = (futuredatetriptypes.Contains(tl.TripTypeId) ? DateTime.Now.AddDays(5) : DateTime.Now.AddHours(1));
            if (tl.TripStartDate.Date > alloweddate)
            {
                ModelState.AddModelError("TripStartDate", "Future Date is not Allowed.");
            }
            if (tl.LoadingReachDate != null && tl.LoadingReachDate > alloweddate)
            {
                ModelState.AddModelError("LoadingReachDate", "Future Date is not Allowed.");
            }
            if (tl.LoadingDate != null && tl.LoadingDate > alloweddate)
            {
                ModelState.AddModelError("LoadingDate", "Future Date is not Allowed.");
            }
            if (tl.UnloadingReachDate != null && tl.UnloadingReachDate > alloweddate)
            {
                ModelState.AddModelError("UnloadingReachDate", "Future Date is not Allowed.");
            }
            if (tl.UnloadingDate != null && tl.UnloadingDate > alloweddate)
            {
                ModelState.AddModelError("UnloadingDate", "Future Date is not Allowed.");
            }

            if (tl.LoadingReachDate != null && tl.TripStartDate > tl.LoadingReachDate)
            {
                ModelState.AddModelError("LoadingReachDate", $"Trip No :{tl.TriplogNo} Loading ReachDate has to be greater than Trip Start Date");
            }
            if (tl.LoadingDate != null && tl.LoadingReachDate.GetValueOrDefault(tl.TripStartDate) > tl.LoadingDate)
            {
                ModelState.AddModelError("LoadingDate", $"Trip No :{tl.TriplogNo} Loading Date has to be greater than Loading ReachDate");
            }
            if (tl.UnloadingReachDate != null && tl.LoadingDate.GetValueOrDefault(tl.LoadingReachDate.GetValueOrDefault(tl.TripStartDate)) > tl.UnloadingReachDate)
            {
                ModelState.AddModelError("UnloadingReachDate", $"Trip No :{tl.TriplogNo} Unloading Reach Date has to be greater than Loading Date");
            }
            if (tl.UnloadingDate != null && tl.UnloadingReachDate.GetValueOrDefault(tl.LoadingDate.GetValueOrDefault(tl.LoadingReachDate.GetValueOrDefault(tl.TripStartDate))) > tl.UnloadingDate)
            {
                ModelState.AddModelError("UnloadingDate", $"Trip No :{tl.TriplogNo} UnloadingDate {tl.UnloadingDate:dd-MMM-yyyy HH:mm:ss tt} has to be greater than Unloading Reach Date  {tl.UnloadingReachDate.GetValueOrDefault(tl.LoadingDate.GetValueOrDefault(tl.LoadingReachDate.GetValueOrDefault(tl.TripStartDate))):dd-MMM-yyyy HH:mm:ss tt} AND Loading Date");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var uow = Request.GetContext();
            var err = GetLiveDbLevelValidation(tl, uow);
            if (!string.IsNullOrWhiteSpace(err))
            {
                return BadRequest(err);
            }

            if (tl.TripTypeId != 1159 && _tlRepo.Queryable().Any(x => x.VehicleId == tl.VehicleId && x.HireVehicleId == tl.HireVehicleId && x.TripTypeId == 1664 && x.UnloadingDate == null))
            {
                return BadRequest(
                    "Vehicle already have pending schedule. so cannot create new trip or schedule before either closing it or rejecting it.");
            }
            await CheckDateOverlap(tl);
            if (tl.TripStartDate.Date > DateTime.Now.Date.AddDays(30))
            {
                ModelState.AddModelError("TripStartDate", "Future Date is not Allowed.");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            tl.ObjectState = ObjectState.Added;
            var dt = tl.Data ?? new List<JsonDataEntity>();
            if (dt.Any())
            {
                tl.ExtraProperties = JsonConvert.SerializeObject(dt);
            }
            List<VehicleMovementLogPickupDrop> wps = null;
            try
            {
                if (!Request.IsBatchRequest())
                {
                    uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                _tlRepo.Insert(tl);
                await uow.SaveChangesAsync();
                if ((tl.CreateWayPointOnServer || tl.FormId == "5001") && tl.VehicleId > 0 /*Run only for Own Vehicles*/ && tl.RouteId > 0)
                {
                    try
                    {
                        var wpRepo = uow.RepositoryAsync<RouteWayPoint>();
                        var waypoints = await wpRepo.Queryable().Where(x => x.RouteId == tl.RouteId).Select(x =>
                            new
                            {
                                RouteId = x.RouteId,
                                CityId = x.CityId,
                                GeographyPoint = x.GeographyPoint,
                                KM = x.Distance,
                                Latitude = x.Latitude,
                                Longitude = x.Longitude,
                                Order = x.OrderId,
                                TravalTime = x.TransitTime,
                                TypeId = x.TypeId
                            }).ToListAsync();
                        wps = waypoints.Select(x => new VehicleMovementLogPickupDrop
                        {
                            RouteId = x.RouteId,
                            CityId = x.CityId,
                            GeographyPoint = x.GeographyPoint,
                            KM = (int)x.KM,
                            Latitude = (decimal)x.Latitude,
                            Longitude = (decimal)x.Longitude,
                            Order = x.Order,
                            OriginLocationId = tl.FromPlaceId ?? waypoints.OrderBy(y => y.Order).FirstOrDefault()?.CityId ?? 0,
                            StopageTime = 0,
                            TravalTime = x.TravalTime,
                            TriplogId = tl.Id,
                            fk_Triplog = tl,
                            TypeId = x.TypeId.GetValueOrDefault(),
                            ObjectState = ObjectState.Added
                        }).ToList();
                        uow.RepositoryAsync<VehicleMovementLogPickupDrop>().InsertGraphRange(wps);
                        tl.TotalKmRun = tl.KmRun = wps.Sum(x => x.KM);
                        tl.ObjectState = ObjectState.Modified;
                        //await uow.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        return BadRequest("Unable to Created RouteWay Points from server side when trip posted from Mobile app using View Id 5001");
                    }
                    tl.KmRun = wps.Sum(x => x.KM);
                }

                if ((tl.BookBudgetingOnServer || tl.FormId == "5001") && tl.VehicleId > 0 /*Run only for Own Vehicles*/ && tl.RouteId > 0)
                {
                    try
                    {
                        var expenseRaw = await uow.SqlQueryAsync(
                            "[dbo].[Proc_TRNS_TripBdgtV2_Show]",
                            new SqlParameter() { Value = tl.RouteId, ParameterName = "parameter1" }/*RouteId*/,
                            new SqlParameter() { Value = tl.VehicleId, ParameterName = "parameter2" }/*VehicleId*/,
                            new SqlParameter() { Value = tl.TripStartDate, ParameterName = "parameter3" }/*TripDate*/,
                            new SqlParameter() { Value = tl.TripNatureId, ParameterName = "parameter4" }/*TripNature*/);
                        var expRepo = uow.RepositoryAsync<TripExpenseLog>();
                        if (expenseRaw != null)
                        {
                            foreach (DataRow row in expenseRaw.Rows)
                            {
                                var exp = new TripExpenseLog
                                {
                                    SettledAmount = Utilities.To<decimal>(row["PaidAmount"]),
                                    ClaimAmount = Utilities.To<decimal>(row["BudgetedAmount"]),
                                    ExpenseTypeId = Utilities.To<long>(row["ExpenseId"]),
                                    FuelQty = 0,
                                    FuelRate = 0,
                                    BudgetedQty = Utilities.To<decimal>(row["BudgetedQty"]),
                                    TripLogId = tl.Id,
                                    ViewId = string.IsNullOrWhiteSpace(tl.FormId) ? 1576 : long.Parse(tl.FormId),
                                    IsAuto = true,//changes done sanjay
                                    IsBudgeted = true,//changes done sanjay
                                    ObjectState = ObjectState.Added
                                };
                                expRepo.Insert(exp);
                                tl.TripExpenses?.Add(exp);
                            }
                            if ((tl.TripExpenses?.Count ?? 0) > 0)
                            {
                                var budgetedQty = tl.TripExpenses.Where(x => x.IsBudgeted).Sum(x => x.FuelQty);
                                if (budgetedQty > 0)
                                {
                                    tl.BdgtFuelQty = budgetedQty;
                                }
                                tl.BdgtAdvance = tl.BdgtTripExpense = tl.TripExpenses.Where(x => x.IsBudgeted && x.BudgetedQty == 0).Sum(x => x.ClaimAmount);
                            }
                            if (tl.ObjectState != ObjectState.Added)
                            {
                                tl.ObjectState = ObjectState.Modified;
                            }
                            await uow.SaveChangesAsync();
                        }
                    }
                    catch (Exception e)
                    {
                        tl.Data?.Add(new JsonDataEntity
                        {
                            DataName = "Errors",
                            RawJson = JsonConvert.SerializeObject(new
                            {
                                BudgetError = $"{e.GetBaseException().ToString()}"
                            })
                        });
                    }
                }

                if (!string.IsNullOrWhiteSpace(tl.CnChallanJson))
                {
                    try
                    {
                        var cnchallans = JsonConvert.DeserializeObject<List<CnChallan>>(tl.CnChallanJson);
                        if (cnchallans.Any())
                        {
                            var repo = uow.RepositoryAsync<CnChallan>();
                            foreach (var chcn in cnchallans)
                            {
                                if (chcn.Id > 0)
                                {
                                    var chcnorg = await repo.FindAsync(chcn.Id);
                                    Mapper.Map(chcn, chcnorg);
                                    chcnorg.TriplogId = tl.Id;
                                    chcnorg.fk_Triplog = tl;
                                    chcnorg.ObjectState = ObjectState.Modified;
                                }
                                else
                                {
                                    chcn.TriplogId = tl.Id;
                                    chcn.fk_Triplog = tl;
                                    chcn.ObjectState = ObjectState.Added;
                                    repo.Insert(chcn);
                                }
                            }
                        }
                    }
                    catch
                    {
                        return BadRequest("Malformed data found in CnChallan section");
                    }
                }
                if (tl.VehicleId.GetValueOrDefault() > 0 && tl.Driver1stId.GetValueOrDefault() > 0)
                {
                    if (!string.IsNullOrWhiteSpace(tl.DriverPhone) && tl.DriverPhone.Length >= 10 && tl.Driver1stId.GetValueOrDefault() != 0)
                    {
                        var drivercontact = await uow.RepositoryAsync<DriverMaster>().Queryable()
                            .Where(x => x.Id == tl.Driver1stId).FirstOrDefaultAsync();
                        if (tl.DriverPhone != drivercontact?.DriverContactNo1)
                        {
                            drivercontact.DriverContactNo1 = tl.DriverPhone;
                            drivercontact.ObjectState = ObjectState.Modified;
                        }
                    }
                }


                await uow.SaveChangesAsync();
                await RunDateLogic(tl);
                if (tl.VehicleId.GetValueOrDefault() > 0 && (tl.TripTypeId == 1158 || tl.TripTypeId == 1453 || (tl.TripTypeId == 1160 && tl.VehicleId != null)))
                {
                    await _tlRepo.Attach_eTolls(tl.VehicleId, tl.HireVehicleId, tl.Driver1stId, tl.Id, tl.TripStartDate,
                        tl.UnloadingDate ?? DateTime.Now);
                }
                
                tl.TotalKmRun = tl.KmRun + tl.AdditionalKmRun;

                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                string tripprocessJobId = "";
                if (_tlRepo.GetConfigValue<int>("RunTripPostProcess") == 1)
                {
                    var differTime = _tlRepo.GetConfigValue<double>("TripPostProcessTriggerInterval");
                    if (differTime < 2)
                    {
                        differTime = 2;
                    }
                    tripprocessJobId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunTripPostProcess(null, tl.Id, Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(differTime));
                    //tripprocessJobId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunTripPostProcess(null,vml.Id, Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(1));
                }
                if (_tlRepo.GetConfigValue<int>("RunFuelAutomationProcess") == 1)
                {
                    if (!string.IsNullOrWhiteSpace(tripprocessJobId) && _tlRepo.GetConfigValue<int>("RunFuelAutomationAfterTripPostTask") == 1)
                    {
                        BackgroundJob.ContinueJobWith<IHangfireJobProcessor>(tripprocessJobId, x => x.RunFuelAutomation(tl.Id, Helper.SessionId(), Helper.LoggedInTenantId));
                    }
                    else
                    {
                        var differTime = _tlRepo.GetConfigValue<double>("FuelAutomationTriggerInterval");
                        if (differTime < 2)
                        {
                            differTime = 2;
                        }
                        BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.RunFuelAutomation(tl.Id, Helper.SessionId(), Helper.LoggedInTenantId), TimeSpan.FromMinutes(differTime));
                    }
                }
                if (!IsNewGPSBatchTripUploadEnabled)
                {
                    if (wps != null && wps.Any())
                    {
                        foreach (var log in wps)
                        {
                            await _tlRepo.PushToGpsProviderAsync(log);
                        }
                    }
                }
                else
                {
                    await _tlRepo.ScheduleTripPushToGPSAsync(tl.Id, tl.RouteId);
                }
                if (tl.UnloadingDate != null)
                {
                    if (_tlRepo.Queryable().Any(x => x.ParentTLId == tl.Id))
                    {
                        var childtlid = await _tlRepo.Queryable().Where(x => x.ParentTLId == tl.Id).Select(x => x.Id).FirstAsync();
                        BackgroundJob.Enqueue<IHangfireJobProcessor>(x => x.PushChildTrip(childtlid, Helper.LoggedInTenantId, 0, null));
                    }
                }

                if (tl.FormId == "1571")
                {
                    var v1 = await uow.SqlQueryAsync(
                    "[dbo].[Proc_TRANS_1571_SaveShortage]",
                    new SqlParameter() { Value = tl.Id, ParameterName = "parameter1" }/*Id*/,
                    new SqlParameter() { Value = tl.CreatedSessionId, ParameterName = "parameter2" }/*CSID*/);
                }

                if (tl.TripTypeId == 1159)
                {
                    try
                    {
                        var v1 = await uow.SqlQueryAsync(
                        "[dbo].[Proc_GBL_TSL_Save]",
                        new SqlParameter() { Value = tl.Id, ParameterName = "parameter1" },
                        new SqlParameter() { Value = tl.CreatedSessionId, ParameterName = "parameter2" },
                        new SqlParameter() { Value = tl.FormId, ParameterName = "parameter3" },
                        new SqlParameter() { Value = JsonConvert.SerializeObject(tl.TSLs), ParameterName = "parameter4" }
                        );
                    }
                    catch (SqlExecutionException ex) { return BadRequest(ex.Message); }

                    try
                    {
                        var v1 = await uow.SqlQueryAsync(
                        "[dbo].[Proc_GBL_CreateAPLData]",
                        new SqlParameter() { Value = tl.Id, ParameterName = "parameter1" },
                        new SqlParameter() { Value = tl.TriplogNo, ParameterName = "parameter3" },
                        new SqlParameter() { Value = tl.FormId, ParameterName = "parameter2" }
                        );
                    }
                    catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
                }

            }
            catch (DbUpdateException ex)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw new BusinessException(ex);
            }
            catch (SqlException ex)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw new BusinessException(ex);
            }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }
                throw;
            }

            return Created(tl);
        }

        // POST: odata/VehicleMovementLogs(key)/Challans
        [ODataRoute("VehicleMovementLogs({key})/Challans")]
        public async Task<IHttpActionResult> PostChallans([FromODataUri] long key, [FromBody] ChallanMaster challan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var tl = await _tlRepo.FindAsync(key);
            challan.ChallanDate = tl.LoadingDate.GetValueOrDefault(tl.TripStartDate);
            challan.ChallanNo = tl.TriplogNo + "-" + challan.RouteId;
            challan.DriverId = tl.Driver1stId.GetValueOrDefault(challan.DriverId.GetValueOrDefault(0));
            challan.OfficeID = tl.OfficeId.GetValueOrDefault(0);
            challan.VehicleId = tl.VehicleId;
            challan.ViewId = 1008;
            challan.ChallanTypeId = 1017;
            var uow = Request.GetContext();
            challan.ObjectState = ObjectState.Added;
            challan.TriplogId = key;
            if (challan.ChallanCNView != null && challan.ChallanCNView.Any())
            {
                foreach (var cn in challan.ChallanCNView)
                {
                    //var dto = new MapperConfiguration(c => c.CreateMap<vwChallanCN, CnChallan>()).CreateMapper().Map<CnChallan>(cn);
                    var cnf = new MapperConfiguration(x =>
                      {
                          x.CreateMap<vwChallanCN, CnChallan>();
                          x.CreateMap<vwCnChallanCharges, CnChallanCharges>();
                      });
                    var dto = cnf.CreateMapper().Map<CnChallan>(cn);
                    challan.CNChallans.Add(dto);
                }
            }
            var item = uow.RepositoryAsync<ChallanMaster>().Insert(challan);
            await uow.SaveChangesAsync();
            return Created(item);
        }

        // POST: odata/VehicleMovementLogs(key)/Challans
        [ODataRoute("VehicleMovementLogs({key})/ChallanCNs")]
        public async Task<IHttpActionResult> PostCnChallans([FromODataUri] long key, [FromBody] CnChallan chcn)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var tl = await _tlRepo.FindAsync(key);
            if (tl == null)
            {
                return NotFound();
            }
            chcn.TriplogId = key;
            chcn.fk_Triplog = tl;
            var uow = Request.GetContext();
            chcn.ObjectState = ObjectState.Added;
            var item = uow.RepositoryAsync<CnChallan>().Insert(chcn);
            await uow.SaveChangesAsync();
            return Created(item);
        }

        // POST: odata/VehicleMovementLogs(key)/Challans
        [ODataRoute("VehicleMovementLogs({key})/HSAdvances")]
        public async Task<IHttpActionResult> PostHSAdvances([FromODataUri] long key, [FromBody] HSAdvance adv)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var tl = await _tlRepo.FindAsync(key);
            if (tl == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            if (adv.OfficeId.GetValueOrDefault(0) <= 0)
            {
                var officeid =
                uow.RepositoryAsync<Ledger>()
                    .Queryable()
                    .Where(x => x.Id == adv.CrAccountId)
                    .Select(x => x.OfficeId)
                    .FirstOrDefault();
                adv.OfficeId = officeid;
            }
            adv.HireSlipId = key;
            adv.fk_HireSlip = tl;
            adv.ObjectState = ObjectState.Added;
            var item = uow.RepositoryAsync<HSAdvance>().Insert(adv);
            await uow.SaveChangesAsync();
            return Created(item);
        }

        [ODataRoute("VehicleMovementLogs({key})/TripAdvances")]
        public async Task<IHttpActionResult> PostTripAdvances([FromODataUri] long key, [FromBody] TripAdvanceLog advance)
        {
            advance.TripLogId = key;
            if (advance.Amount <= 0 && (advance.RequestStatusId != 1596/*New Request*/ && advance.RequestStatusId != 1598/*Reject Request*/)) return BadRequest("Advance Amount is Zero which is not allowed.");
            var unitOfWorkAsync = Request.GetContext();

            #region Advance Logic

            advance.ObjectState = ObjectState.Added;
            //TODO:Fuel Rate should be fetched from Database
            if (advance.AdvanceTypeId == 10)
            {
                try
                {
                    advance.FuelQty = advance.FuelAmount / advance.FuelRate;
                }
                catch (DivideByZeroException)
                {
                    return BadRequest("TADV104:Either Fuel Amount or Fuel Rate was Zero. which is not allowed");
                }
            }
            else
            {
                advance.FuelAmount = advance.FuelQty * advance.FuelRate;
            }

            //advance.Amount = advance.FuelQty > 0 ? advance.FuelAmount : advance.CashAmount;

            #endregion Advance Logic

            #region Voucher Logic

            if (advance.RequestStatusId != 1596 && advance.RequestStatusId != 1598/*New Request*/)
            {
                if (advance.VoucherId > 0)
                {
                    advance.fk_Voucher = unitOfWorkAsync.RepositoryAsync<Voucher>().Query(x => x.Id == advance.VoucherId && x.VoucherTypeId == advance.AdvanceTypeId).Include(x => x.VoucherDetails.Select(y => y.VoucherDetailReferences)).Select(x => x).FirstOrDefault();
                }
                if (advance.fk_Voucher == null) { advance.fk_Voucher = new Voucher { ObjectState = ObjectState.Added }; }
                _tripAdvanceLogService.PrepareV(advance);

                #endregion Voucher Logic

                #region Voucher Detail Logic

                _tripAdvanceLogService.PrepareVD(advance);

                #endregion Voucher Detail Logic

                #region Voucher Detail Refrence

                advance.fk_Voucher.VoucherDetails.ForEach(x => new Action<VoucherDetail, TripAdvanceLog,List<FakeVDRs>>(_tripAdvanceLogService.PrepareVDR).Invoke(x, advance,null));

                #endregion Voucher Detail Refrence

                #region Validations

                if (advance.fk_Voucher.Amount1 + advance.fk_Voucher.Amount2 != 0 || (advance.fk_Voucher.Amount1 > 0 ? advance.fk_Voucher.Amount1 : advance.fk_Voucher.Amount2) != advance.Amount || advance.Amount <= 0 || advance.fk_Voucher.VoucherDetails.Sum(x => x.Amount) != 0)
                {
                    return BadRequest("TADV100: credit and debit amount does not tally.");//Amount Validation Failed
                }
                if (advance.fk_Voucher.VoucherDetails.Count(x => x.ObjectState == ObjectState.Added) <= 1)
                {
                    return BadRequest("TADV101: There should be 2 Accounts in Cash or Fuel Entry");//Atleast two VD are required in Advance Transaction Voucher
                }
                if (advance.fk_Voucher.VoucherDetails.Count(x => x.VoucherDetailReferences.Count != 0 && x.ObjectState == ObjectState.Added) == 0 && (advance.AdvanceTypeId == 1 || advance.AdvanceTypeId == 2 || advance.AdvanceTypeId == 3))
                {
                    return BadRequest("TADV102:Expense or Control Account in case of Cash or Fuel Entry, should have [Bill by Bill] Flag On.");//Atlead one VDR is Required in Advance Transaction
                }
                foreach (var voucherDetail in advance.fk_Voucher.VoucherDetails.Where(voucherDetail => voucherDetail.VoucherDetailReferences.Count > 0 && voucherDetail.Amount != voucherDetail.VoucherDetailReferences.Sum(x => x.Amount)))
                {
                    return BadRequest("TADV103: Bills total does not match in either Credit or Debit side" + voucherDetail.Id);//VD and VDR Amount Doesn't Tally
                }

                #endregion Validations
            }
            if (advance.RequestStatusId == 1596/*New Request*/ || advance.RequestStatusId == 1598 /*Reject Request*/)
            {
                var voucher = unitOfWorkAsync.RepositoryAsync<Voucher>().Queryable().FirstOrDefault(x => x.Id == advance.VoucherId && x.VoucherTypeId == advance.AdvanceTypeId);
                if (voucher != null)
                {
                    voucher.ObjectState = ObjectState.Deleted;
                }
                advance.VoucherId = null;
                advance.fk_Voucher = null;
            }
            _tripAdvanceLogService.Insert(advance);
            await unitOfWorkAsync.SaveChangesAsync();
            //_audit.Insert(new ApiRecordAccessLog() { ObjectState = ObjectState.Added, RecordId = advance.Id, UserId = this.GetClaimByKey<long>("UserId"), SessionId = this.GetClaimByKey<long>("SessionId"), Type = AccessType.Created, ViewId = 1005, RecordName = advance.VoucherNo });
            //await _unitOfWorkAsync.SaveChangesAsync();
            return Created(advance);
        }

        //POST:odata/VehicleMovementLogs(key)/TripExpenses
        [ODataRoute("VehicleMovementLogs({key})/TripExpenses")]
        public async Task<IHttpActionResult> PostTripExpenses([FromODataUri] long key, [FromBody] TripExpenseLog expense)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var tl = await _tlRepo.Queryable().Include(x => x.TripExpenses).FirstOrDefaultAsync(x => x.Id == key);

            if (tl == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            expense.ObjectState = ObjectState.Added;
            tl.TripExpenses.Add(expense);
            tl.BdgtTripExpense = tl.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) == 0).Sum(x => x.ClaimAmount);
            //if (expense.TripAdvanceLogId > 0)
            //{
            //    var r =
            //        uow.RepositoryAsync<TripExpenseLog>().Queryable().Include(x=>x.fk_TripAdvanceLog)
            //            .Where(x => x.TripAdvanceLogId == expense.TripAdvanceLogId);
            //    decimal sum = 0;
            //    if (r.Any())
            //    {
            //        sum = r.Sum(x => x.FuelQty + x.ShortFuelQty);

            //        if (r.First().fk_TripAdvanceLog.FuelQty < (sum + expense.FuelQty + expense.ShortFuelQty))
            //        {
            //            return BadRequest($"Maximum Allocation Fuel Qty {r.First().fk_TripAdvanceLog.FuelQty - (sum + expense.FuelQty + expense.ShortFuelQty)} has been exceeded for Fuel Expense No {r.First().fk_TripAdvanceLog.ReferenceNo}");
            //        }
            //        r.First().fk_TripAdvanceLog.BalanceQty = r.First().fk_TripAdvanceLog.FuelQty - (sum + expense.FuelQty + expense.ShortFuelQty);

            //        r.First().fk_TripAdvanceLog.ObjectState = ObjectState.Modified;
            //    }
            //    tl.ConsumedFuelAmt = tl.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) >0).Sum(x => x.SettledAmount);
            //    tl.ConsumedFuelQty = tl.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0).Sum(x => x.FuelQty);
            //    tl.ShortFuelAmt = tl.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0).Sum(x => x.ShortFuelAmt);
            //    tl.ShortFuelQty = tl.TripExpenses.Where(x => x.TripAdvanceLogId.GetValueOrDefault(0) > 0).Sum(x => x.ShortFuelQty);

            //}
            tl.ObjectState = ObjectState.Modified;
            await uow.SaveChangesAsync();
            return Created(expense);
        }

        // POST: odata/VehicleMovementLogs(key)/WayPoints
        [ODataRoute("VehicleMovementLogs({key})/WayPoints")]
        public async Task<IHttpActionResult> PostWayPoints([FromODataUri] long key, [FromBody] VehicleMovementLogPickupDrop point)
        {
            var uow = Request.GetContext();
            try
            {
                if (!Request.IsBatchRequest())
                {
                    uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var tl = await _tlRepo.FindAsync(key);
                if (tl == null)
                {
                    return NotFound();
                }

                point.TriplogId = key;
                point.fk_Triplog = tl;

                point.ObjectState = ObjectState.Added;
                var item = uow.RepositoryAsync<VehicleMovementLogPickupDrop>().Insert(point);
                await uow.SaveChangesAsync();
                if (!IsNewGPSBatchTripUploadEnabled)
                {
                    await _tlRepo.PushToGpsProviderAsync(point);
                }
                else
                {
                    await _tlRepo.ScheduleTripPushToGPSAsync(point.TriplogId, point.RouteId);
                }
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return Created(item);
            }
            catch (Exception e)
            {
                if (!Request.IsBatchRequest())
                {
                    uow.Rollback();
                }

                throw;
            }
        }

        // PUT: odata/VehicleMovementLogs(5)
        public async Task<IHttpActionResult> Put(long key, VehicleMovementLog tl)
        {
            return BadRequest("Put Request Not Supported");
            await CheckDateOverlap(tl);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != tl.Id)
            {
                return BadRequest();
            }
            tl.ObjectState = ObjectState.Modified;

            try
            {
                var uow = Request.GetContext();
                var err = GetLiveDbLevelValidation(tl, uow);
                
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }

                if (!Request.IsBatchRequest())
                {
                    uow.BeginTransaction(IsolationLevel.ReadCommitted);
                }
                if (tl.FormId == "1576")
                {
                    tl.SHRT_VoucherDate = tl.UnloadingDate;
                }
                _tlRepo.Update(tl);
                await uow.SaveChangesAsync();
                await RunDateLogic(tl);
                if (tl.VehicleId.GetValueOrDefault() > 0 && (tl.TripTypeId == 1158 || tl.TripTypeId == 1453 || (tl.TripTypeId == 1160 && tl.VehicleId != null)))
                {
                    await _tlRepo.Attach_eTolls(tl.VehicleId, tl.HireVehicleId, tl.Driver1stId.GetValueOrDefault(), tl.Id, tl.TripStartDate,
                        tl.UnloadingDate ?? DateTime.Now);
                }

                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                //await _tlRepo.PushToGpsProviderAsync(tl);
                if (tl.UnloadingDate != null)
                {
                    if (_tlRepo.Queryable().Any(x => x.ParentTLId == tl.Id))
                    {
                        var childtlid = await _tlRepo.Queryable().Where(x => x.ParentTLId == tl.Id).Select(x => x.Id).FirstAsync();
                        BackgroundJob.Enqueue<IHangfireJobProcessor>(x => x.PushChildTrip(childtlid, Helper.LoggedInTenantId, 0, null));
                    }
                }
                if (tl.FormId == "1571")
                {
                    var v1 = await uow.SqlQueryAsync(
                    "[dbo].[Proc_TRANS_1571_SaveShortage]",
                    new SqlParameter() { Value = tl.Id, ParameterName = "parameter1" }/*Id*/,
                    new SqlParameter() { Value = tl.CreatedSessionId, ParameterName = "parameter2" }/*CSID*/);
                }
                if (tl.TripTypeId == 1159)
                {
                    try
                    {
                        var v1 = await uow.SqlQueryAsync(
                        "[dbo].[Proc_GBL_TSL_Save]",
                        new SqlParameter() { Value = tl.Id, ParameterName = "parameter1" },
                        new SqlParameter() { Value = tl.CreatedSessionId, ParameterName = "parameter2" },
                        new SqlParameter() { Value = tl.FormId, ParameterName = "parameter3" },
                        new SqlParameter() { Value = JsonConvert.SerializeObject(tl.TSLs), ParameterName = "parameter4" }
                        );
                    }
                    catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
                    try
                    {
                        var v1 = await uow.SqlQueryAsync(
                        "[dbo].[Proc_GBL_CreateAPLData]",
                        new SqlParameter() { Value = tl.Id, ParameterName = "parameter1" },
                        new SqlParameter() { Value = tl.TriplogNo, ParameterName = "parameter3" },
                        new SqlParameter() { Value = tl.FormId, ParameterName = "parameter2" }
                        );
                    }
                    catch (SqlExecutionException ex) { return BadRequest(ex.Message); }
                }
                return Updated(tl);
            }
            catch (DbUpdateException ex)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw new BusinessException(ex);
            }
            catch (SqlException ex)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw new BusinessException(ex);
            }
            catch (Exception ex)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Rollback();
                }
                throw;
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> RecalculateTLFreight([FromODataUri] long key)
        {
            await Request.GetContext().ExecSqlQueryAsync($"EXEC [dbo].[Proc_TRANS_1499_ReCalculateTLFreight]{key}");
            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private async Task CheckDateOverlap(VehicleMovementLog log)
        {
            var message = await _tlRepo.ValidateTLDateRangeOverlapAsync(log.TripStartDate, log.UnloadingDate, log.VehicleId ?? 0, log.HireVehicleId ?? 0, log.Id, log.TripTypeId ?? 0, log.TripNatureId ?? 0);
            if (!string.IsNullOrWhiteSpace(message))
            {
                throw new BusinessException(ErrorCode.TAL101, message);
            }
        }

        private async Task RunDateLogic(VehicleMovementLog entity)
        {
            var allowedVtsTrips = new List<long?> { 1159, 1158, 1453 };

            if (_tlRepo.GetConfigValue<int>("IsHSVTSActivated") == 1)
            {
                allowedVtsTrips.Add(1160);
            }
            if (!allowedVtsTrips.Any(x => x == 1160) && entity.VehicleId != null && entity.TripTypeId == 1160)
            {
                allowedVtsTrips.Add(1160);
            }
            var allowedVtsTripNatures = new List<long?> { 1076, 1075 };
            var allowVTSFormORM = _tlRepo.GetConfigValue<int>("AllowVTSFormORM");
            if (allowVTSFormORM == 1)
            {
                allowedVtsTripNatures.Add(1520);
            }
            if (_tlRepo.GetConfigValue<int>("IsVTSEnabled") == 1 && allowedVtsTrips.Contains(entity.TripTypeId) && allowedVtsTripNatures.Contains(entity.TripNatureId))
            {
                try
                {
                    await Request.GetContext().ExecSqlQueryAsync($"[dbo].[Proc_TRANS_1624_CreateVTS] @parameter1={entity.Id},@parameter2=''");
                }
                catch (SqlException ex)
                {
                    throw new BusinessException(ex);
                }
            }
            ///*Pushing Driver allocation from triplog*/
            //if (_tlRepo.GetConfigValue<int>("EnableDriverMappingFromTL") == 1 && entity.Driver1stId > 0)
            //{
            //    try
            //    {
            //        await Request.GetContext().ExecSqlQueryAsync($"[dbo].[Proc_TRANS_1576_DriverAllocation] @parameter1={entity.Id}");
            //    }
            //    catch (SqlException ex)
            //    {
            //        throw new BusinessException(ex);
            //    }
            //}
        }

        private string GetLiveDbLevelValidation(VehicleMovementLog _record, IUnitOfWorkAsync _uow)
        {
            try
            {
                var obj = new
                {
                    _record.OfficeId,
                    _record.PartyId,
                    _record.TripStartDate,
                    _record.LoadingReachDate,
                    _record.LoadingDate,
                    _record.UnloadingReachDate,
                    _record.UnloadingDate,
                    _record.TotalKmRun,
                    _record.AdditionalKmRun,
                    _record.Driver1stId,
                    _record.Driver2ndId,
                    _record.DriverPhone,
                    _record.TripModeId,
                    _record.FromPlaceId,
                    _record.RouteId,
                    _record.ExpTime,
                    _record.ExpectedDeliveryDate,
                    _record.Remarks,
                    _record.HVPId,
                    _record.HVPayableId,
                    _record.HSTDSRate,
                    _record.HSTDSAmount,
                    _record.VoucherId,
                    _record.HSTDSVoucherId,
                    _record.PANStatusId,
                    _record.PANNo,
                    _record.PartyRefNo,
                    _record.EWayBillTL,
                    _record.eWayBillValidity,
                };

                var livevalidationerr = _uow.SqlQueryAsync(
                "[dbo].[Proc_GBL_TL_LiveValidationV1]",
                new SqlParameter() { Value = _record.Id, ParameterName = "parameter1" },
                new SqlParameter() { Value = _record.TripTypeId, ParameterName = "parameter2" },
                new SqlParameter() { Value = _record.VehicleId, ParameterName = "parameter3" },
                new SqlParameter() { Value = _record.HireVehicleId, ParameterName = "parameter4" },
                new SqlParameter() { Value = _record.TripNatureId, ParameterName = "parameter5" },
                new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter6" },
                new SqlParameter() { Value = JsonConvert.SerializeObject(obj), ParameterName = "parameter7" }
                ).Result;

                if (livevalidationerr != null && livevalidationerr?.Rows?.Count > 0)
                {
                    return Utilities.To<string>(livevalidationerr.Rows[0]["ErrorMessage"]);
                }
                return "";
            }
            catch (Exception ex)
            {
                return $"Live Validation Error:{ex.GetBaseException().Message}";
            }
        }

    }
}