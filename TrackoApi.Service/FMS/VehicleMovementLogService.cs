using EntityFramework.Caching;
using EntityFramework.Extensions;
using Hangfire;
using Newtonsoft.Json;
using Repository.Pattern.Core.Repositories;
using Service.Pattern;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.Models.AMS;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.GPS;
using TrackoApi.Models.Global;
using TrackoApi.Service.Global;
using TrackoAPI.Reports.ViewModels;
using TrackoAPI.Repository;

namespace TrackoApi.Service
{
    public interface IVehicleMovementLogService : IService<VehicleMovementLog>
    {
        Task Attach_eTolls(long? vehicleid, long? hirevehicleid, long? driverId, long tripLogId, DateTime tripstartdate, DateTime? unloaddate);

        IQueryable<VehicleMovementLog> GetAllVehicleMovementLogList(int id);

        Task PushToGpsProviderAsync(VehicleMovementLog log);

        Task PushToGpsProviderAsync(VehicleMovementLogPickupDrop point);
        Task ScheduleTripPushToGPSAsync(long triplogid,long? routeid);
    }

    public class VehicleMovementLogService : Service<VehicleMovementLog>, IVehicleMovementLogService
    {
        private readonly IRepositoryAsync<VehicleMovementLog> _repository;
        public VehicleMovementLogService(IRepositoryAsync<VehicleMovementLog> repository) : base(repository)
        {
            _repository = repository;
        }

        public async Task Attach_eTolls(long? vehicleid, long? hirevehicleid, long? driverId, long tripLogId, DateTime tripstartdate, DateTime? unloaddate)
        {
            try
            {
                if (vehicleid.GetValueOrDefault(0) > 0)
                {
                    const string etollquery= @"
                    UPDATE [dbo].[tTripAdvanceLog] SET TripLogId=@tripid WHERE AdvanceTypeId=88 AND ISNULL(VehicleId,0)=@vehicleid AND VoucherDate between @fromdate and @enddate and ISNULL(SettlementId,0)=0 AND ISNULL(TripLogId,0)<>@tripid;
                    UPDATE [dbo].[tTripAdvanceLog] SET TripLogId=NULL WHERE AdvanceTypeId=88 AND ISNULL(VehicleId,0)=@vehicleid AND VoucherDate not between @fromdate and @enddate and ISNULL(SettlementId,0)=0 AND ISNULL(TripLogId,0)=@tripid;
                    ";
                    /*Map Required eTolls*/
                    await this._repository.ExecuteSqlAsync(etollquery
                   ,
                   new SqlParameter("tripid", tripLogId), new SqlParameter("fromdate", tripstartdate),
                   new SqlParameter("enddate", unloaddate), new SqlParameter("vehicleid", vehicleid)).ConfigureAwait(true);
                    
                    if (_repository.GetConfigValue<int>("AutoPickTLForAdvance") == 1)
                    {
                        await this._repository.ExecuteSqlAsync(
                            "UPDATE [dbo].[tTripAdvanceLog] SET TripLogId=@tripid,DriverId=(CASE WHEN DriverId IS NULL THEN @driverId ELSE DriverId END) WHERE AdvanceTypeId in(1,2,3) AND ISNULL(VehicleId,0)=@vehicleid AND AdvanceDate between @fromdate and @enddate and ISNULL(SettlementId,0)=0 AND (ISNULL(TripLogId,0)=0 OR ISNULL(DriverId,0)=0)",
                            new SqlParameter("tripid", tripLogId), new SqlParameter("fromdate", tripstartdate),
                            new SqlParameter("enddate", unloaddate), new SqlParameter("vehicleid", vehicleid), new SqlParameter("driverId", (object)driverId ?? DBNull.Value)).ConfigureAwait(true);
                    }
                }
                await this._repository.ExecuteSqlAsync(
                    "UPDATE [dbo].[tGPSStatusLog] SET TripLogId=@tripid,VTSId=NULL WHERE VehicleId=@vehicleid AND HireVehicleId=@hirevehicleid AND GPSTime between @fromdate and @enddate AND ISNULL(TripLogId,0)<>@tripid",
                    new SqlParameter("tripid", tripLogId), new SqlParameter("fromdate", tripstartdate),
                    new SqlParameter("enddate", unloaddate), new SqlParameter("vehicleid", vehicleid), new SqlParameter("hirevehicleid", vehicleid));
                //await this._repository.ExecuteSqlAsync(
                //   "UPDATE [dbo].[tVehicleMovementLog] SET TripLogId=@tripid,VTSId=NULL WHERE ISNULL(VehicleId,0)=@vehicleid AND GPSTime between @fromdate and @enddate AND ISNULL(TripLogId,0)<>@tripid",
                //   new SqlParameter("tripid", tripLogId), new SqlParameter("fromdate", tripstartdate),
                //   new SqlParameter("enddate", unloaddate), new SqlParameter("vehicleid", vehicleid)).ConfigureAwait(true);
            }
            catch
            {
                //Ignore
            }
        }

        public IQueryable<VehicleMovementLog> GetAllVehicleMovementLogList(int brandid)
        {
            return _repository.GetAllVehicleMovementLogList(brandid);
        }
        public async Task PushToGpsProviderAsync(VehicleMovementLog log)
        {
            return;
            if (!log.LoadingDate.HasValue) return;
            try
            {
                var vehicleInfo = await (log.HireVehicleId > 0 ? _repository.GetRepository<HireVehicle>().Queryable().Where(x => x.Id == log.HireVehicleId && x.GPSVendorId > 0).Select(x => new { x.GPSVendorId, x.VehicleNo }).FromCacheFirstOrDefaultAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromDays(8))) : _repository.GetRepository<VehicleMaster>().Queryable().Where(x => x.Id == log.VehicleId && x.GPSVendorId > 0).Select(x => new { x.GPSVendorId, x.VehicleNo }).FromCacheFirstOrDefaultAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromDays(8))));
                if (vehicleInfo == null || vehicleInfo.GPSVendorId.GetValueOrDefault() == 0) return;
                var record = new GPSTripUploadViewModel
                {
                    TripStartDate = log.TripStartDate,
                    LoadingDate = log.LoadingDate,
                    TripNo = log.TriplogNo,
                    Remark = log.Remarks,
                    ETA = log.ExpectedDeliveryDate,
                    KM = log.TotalKmRun,
                    Qty = log.LoadingQty,
                    TripId = log.Id,
                    VehicleNo = vehicleInfo.VehicleNo,
                    TripNature = log.TripNatureId == 1076 ? "Empty" : log.TripNatureId == 1645 ? "Empty -> Loaded" : log.TripNatureId == 1075 ? "Loaded" : log.TripNatureId == 1646 ? "Loaded -> Empty" : log.TripNatureId == 1647 ? "Loaded -> Loaded" : log.TripNatureId == 1520 ? "ORM" : "None",
                    DriverMobile=log.DriverPhone
                };
                var endpoint = await _repository.GetRepository<GpsEndPoint>().Queryable().Where(x => x.VendorId == vehicleInfo.GPSVendorId && x.ServiceTypeId == 1595).FromCacheFirstOrDefaultAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromDays(8)));
                if (endpoint == null) return;
                var parameters = (endpoint.ParameterMapping ?? "").Split('^');
                var requestbody = endpoint.ParameterTemplate;
                if (parameters.Length > 0)
                {
                    foreach (var pr in parameters)
                    {
                        switch (pr)
                        {
                            case "_Consignor":
                                if (log.PartyId > 0)
                                {
                                    var party = await _repository.GetRepository<Ledger>().Queryable().Where(x => x.Id == log.PartyId).Select(x => new { x.fk_Address.FullAddress, x.AccountName }).FromCacheFirstOrDefaultAsync();
                                    if (party != null)
                                    {
                                        record.Consignee = party.AccountName;
                                        record.ConsigneeAddress = party.FullAddress;
                                    }
                                }
                                break;

                            case "_Consignee":
                                if (log.ConsigneeId > 0)
                                {
                                    var consignee = await _repository.GetRepository<Ledger>().Queryable().Where(x => x.Id == log.ConsigneeId).Select(x => new { x.fk_Address.FullAddress, x.AccountName }).FromCacheFirstOrDefaultAsync();
                                    if (consignee != null)
                                    {
                                        record.Consignee = consignee.AccountName;
                                        record.ConsigneeAddress = consignee.FullAddress;
                                    }
                                }
                                break;

                            case "_ToCity":
                            case "_FromCity":
                                if (log.RouteId > 0 && string.IsNullOrWhiteSpace(record.ToCity))
                                {
                                    var route = await _repository.GetRepository<RouteMaster>().Queryable().Where(x => x.Id == log.RouteId).Select(x => new { FromCity = x.fk_FromPlace.CityName, ToCity = x.fk_ToPlace.CityName, RouteKM = x.TransitKm }).FromCacheFirstOrDefaultAsync();
                                    if (route != null)
                                    {
                                        record.FromCity = route.FromCity;
                                        record.ToCity = route.ToCity;
                                        if (record.KM < 1)
                                        {
                                            record.KM = route.RouteKM;
                                        }
                                    }
                                }
                                break;
                            case "_DriverName":
                            case "_Driver":
                                var driverName = await _repository.GetRepository<DriverMaster>().Queryable()
                                    .Where(x => x.Id == log.Driver1stId).Select(x => new
                                    {
                                        x.DriverName,
                                        x.DriverCode
                                    }).FirstOrDefaultAsync().ConfigureAwait(true);
                                record.DriverName = driverName != null ? $"{driverName?.DriverName}[{driverName?.DriverCode}]" : "";
                                break;
                        }
                        var propValue = record.GetPropertyValue(pr.Replace("_", "")) ?? "";
                        string value;
                        if (propValue is DateTime time) value = time.ToString(string.IsNullOrWhiteSpace(endpoint.DateFormat) ? "yyyy-MM-dd HH:mm:ss" : endpoint.DateFormat);
                        else
                        {
                            value = propValue.ToString();
                        }
                        requestbody = requestbody.Replace(pr, value);
                    }
                }

                BackgroundJob.Enqueue<IHangfireJobProcessor>(
                    x => x.CallGpsVendor(endpoint, requestbody, 0, record));
            }
            catch (Exception e)
            {
                using (var db = new TenantDbContext())
                {
                    db.ApiLog.Add(new WebApiUsage()
                    {
                        RequestContent = $"TripLogNo:{log.TriplogNo},TripDate:{log.TripStartDate:f}",
                        ResponseContent = e.GetBaseException().StackTrace,
                        RequestMethod = "",
                        ResponseTimestamp = DateTime.Now,
                        RequestTimestamp = DateTime.Now,
                        ResponseStatusCode = 0
                    });
                    await db.SaveChangesAsync();
                }
            }
        }
        public async Task ScheduleTripPushToGPSAsync(long triplogid,long? routeid)
        {
            try
            {
                if (routeid.GetValueOrDefault() == 0) return;
                var rwrepo = this._repository.GetRepository<RouteWayPoint>();
                var twrepo = this._repository.GetRepository<VehicleMovementLogPickupDrop>();
                if (await rwrepo.Queryable().CountAsync(x=>x.RouteId== routeid) != await twrepo.Queryable().CountAsync(x=>x.TriplogId== triplogid)&&await twrepo.Queryable().AnyAsync(x=>x.TriplogId== triplogid&&(x.HangfireJobId!=null||x.HangfireJobId!="")))
                {
                    return;
                }
                var log = await this.FindAsync(triplogid);
                if (log.UnloadingDate != null) return;
                double interval = _repository.GetConfigValue<double>("PushScheduledTripInterval");/*Minutes*/
                if (Math.Abs(interval) < 1) interval = -1;
                var delayed = log.ScheduledPlacementDate==null
                    ? 5
                    : interval;
                var jobid = BackgroundJob.Schedule<IHangfireJobProcessor>(
                    x => x.PushToGPSProvider(log.Id,Helper.LoggedInTenantId,null), TimeSpan.FromMinutes(delayed));
                try
                {
                    await _repository.ExecuteSqlAsync("UPDATE [dbo].[tPickDroplog] SET HangfireJobId=@jobid WHERE TripLogId=@triplogid",
                        new SqlParameter("triplogid", log.Id), new SqlParameter("jobid", jobid)).ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    //Ignore
                }
            }
            catch
            {
                //Ignore
            }
        }
        public async Task PushToGpsProviderAsync(VehicleMovementLogPickupDrop point)
        {
            var doNotPostIt = base.GetConfigValue<int>("PostPassThroughWayPointOnGPS") == 0 && point.TypeId == 1616;
            if (point.CityId == point.OriginLocationId || doNotPostIt) return;
            if (point.fk_Triplog == null && point.TriplogId > 0)
            {
                point.fk_Triplog = _repository.Find(point.TriplogId);
            }

            var log = point.fk_Triplog;
            if (log == null) throw new BusinessException(ErrorCode.GLB106, "Trip Missing on Tripway point");
            var nextTrip = await _repository.Queryable()
                .Where(x => (x.TripTypeId == 1158 || x.TripTypeId == 1160 || x.TripTypeId == 1453) && x.VehicleId == log.VehicleId && x.HireVehicleId == log.HireVehicleId &&
                            x.TripStartDate > log.TripStartDate && x.Id != log.Id).Select(x => new { x.Id }).FromCacheFirstOrDefaultAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromSeconds(30)));
            if (nextTrip != null) return;
            if (log.WayPoints.All(x => x.Id != point.Id))
            {
                log.WayPoints.Add(point);
            }
            try
            {
                var vehicleInfo = await (log.HireVehicleId > 0 ? _repository.GetRepository<HireVehicle>().Queryable().Where(x => x.Id == log.HireVehicleId && x.GPSVendorId > 0).Select(x => new { x.GPSVendorId, x.VehicleNo, x.RegistrationNo }).FirstOrDefaultAsync() : _repository.GetRepository<VehicleMaster>().Queryable().Where(x => x.Id == log.VehicleId && x.GPSVendorId > 0 && !x.IsDeactive && x.SoldDate == null).Select(x => new { x.GPSVendorId, x.VehicleNo, RegistrationNo = x.VehicleRegNo }).FirstOrDefaultAsync());
                if (vehicleInfo == null || vehicleInfo.GPSVendorId.GetValueOrDefault() == 0) return;
                var totalkm = log.WayPoints.Where(x => x.Order <= point.Order).Sum(x => x.KM);
                var record = new GPSTripUploadViewModel
                {
                    TripStartDate = log.TripStartDate.AddMinutes(point.Order == 1 ? 0 : point.Order),
                    ScheduledPlacementDate = log.ScheduledPlacementDate,
                    ScheduledDepartureDate = log.ScheduledDepartureDate,
                    LoadingReportDate = log.TripNatureId == 1076/*Empty*/? log.TripStartDate : (log.LoadingReachDate ?? log.TripStartDate),
                    LoadingDate = log.TripNatureId == 1076/*Empty*/ ? log.TripStartDate : (log.LoadingDate ?? log.TripStartDate),
                    TripNo = log.TriplogNo,
                    Remark = log.Remarks,
                    ETAHour = log.ExpTime,
                    ETA = log.ExpectedDeliveryDate,
                    KM = totalkm > 0 ? totalkm : log.TotalKmRun,
                    Qty = log.LoadingQty,
                    TripId = log.Id,
                    VehicleNo = vehicleInfo.VehicleNo,
                    Id = point.Id,
                    TenantId = Helper.LoggedInTenantId,
                    RegistrationNo = vehicleInfo.RegistrationNo,
                    Order = point.Order,
                    PointKM = point.KM,
                    StopageTime = point.StopageTime,
                    TravalTime = point.TravalTime,
                    TripNature = log.TripNatureId == 1076 ? "Empty" : log.TripNatureId == 1645 ? "Empty -> Loaded" : log.TripNatureId == 1075 ? "Loaded" : log.TripNatureId == 1646 ? "Loaded -> Empty" : log.TripNatureId == 1647 ? "Loaded -> Loaded" : log.TripNatureId == 1520 ? "ORM" :"None",
                    DriverMobile=log.DriverPhone,
                    RouteName =  log.fk_Route?.Name
                };
                var cnlist = (await _repository.GetRepository<CnChallan>().Queryable().Where(x => x.TriplogId == point.TriplogId && x.CNId > 0).Select(x => x.fk_CNMaster.CNNo).ToListAsync());
                if (cnlist != null && cnlist.Any())
                {
                    record.CNNos = cnlist.JoinStrings(",");
                }
                var endpoints = await _repository.GetRepository<GpsEndPoint>().Queryable().Where(x => (x.VendorId == vehicleInfo.GPSVendorId || x.fk_Vendor.Alias == "GOFLEETGIS") && x.ServiceTypeId == 1595).ToListAsync();
                if (endpoints == null || !endpoints.Any()) return;
                foreach (var endpoint in endpoints)
                {
                    var parameters = (endpoint.ParameterMapping ?? "").Split('^');
                    var requestbody = endpoint.ParameterTemplate;
                    if (parameters.Length > 0)
                    {
                        foreach (var pr in parameters)
                        {
                            switch (pr)
                            {
                                case "_Consignor":
                                    if (log.PartyId > 0)
                                    {
                                        var party = await _repository.GetRepository<Ledger>().Queryable().Where(x => x.Id == log.PartyId).Select(x => new { x.fk_Address.FullAddress, x.AccountName }).FromCacheFirstOrDefaultAsync();
                                        if (party != null)
                                        {
                                            record.Consignor = party.AccountName;
                                            record.ConsignoreAddress = party.FullAddress;
                                        }
                                    }
                                    break;

                                case "_Consignee":
                                    if (log.ConsigneeId > 0)
                                    {
                                        var consignee = await _repository.GetRepository<Ledger>().Queryable().Where(x => x.Id == log.ConsigneeId).Select(x => new { x.fk_Address.FullAddress, x.AccountName }).FromCacheFirstOrDefaultAsync().ConfigureAwait(true);
                                        if (consignee != null)
                                        {
                                            record.Consignee = consignee.AccountName;
                                            record.ConsigneeAddress = consignee.FullAddress;
                                        }
                                    }
                                    break;

                                case "_ToCity":
                                    var tocityname = await _repository.GetRepository<CityMaster>().Queryable()
                                        .Where(x => x.Id == point.CityId).Select(x => new
                                        {
                                            x.CityName,
                                            StateName = x.fk_State == null ? null : x.fk_State.Name,
                                            District = x.fk_District == null ? null : x.fk_District.CityName,
                                            x.PostalCode
                                        }).FromCacheFirstOrDefaultAsync().ConfigureAwait(true);
                                    record.ToCity = tocityname?.CityName;
                                    record.ToCityStateName = tocityname?.StateName;
                                    record.PostalCode = tocityname?.PostalCode;
                                    break;

                                case "_FromCity":
                                    var fromcityname = await _repository.GetRepository<CityMaster>().Queryable()
                                        .Where(x => x.Id == point.OriginLocationId).Select(x => new
                                        {
                                            x.CityName
                                        }).FromCacheFirstOrDefaultAsync().ConfigureAwait(true);
                                    record.FromCity = fromcityname?.CityName;
                                    break;
                                case "_DriverName":
                                case "_Driver":
                                    var driverName = await _repository.GetRepository<DriverMaster>().Queryable()
                                        .Where(x => x.Id == log.Driver1stId).Select(x => new
                                        {
                                            x.DriverName,
                                            x.DriverCode
                                        }).FirstOrDefaultAsync().ConfigureAwait(true);
                                    record.DriverName =driverName!=null? $"{driverName?.DriverName}[{driverName?.DriverCode}]":"";
                                    break;
                            }
                            var propValue = record.GetPropertyValue(pr.Replace("_", "")) ?? "";
                            string value;
                            if (propValue is DateTime time) value = time.ToString(string.IsNullOrWhiteSpace(endpoint.DateFormat) ? "yyyy-MM-dd HH:mm:ss" : endpoint.DateFormat);
                            else
                            {
                                value = propValue.ToString();
                            }
                            try
                            {
                                while (requestbody.Contains(pr))
                                {
                                    requestbody = requestbody.Replace(pr, value);
                                }
                            }
                            catch (Exception)
                            {
                                requestbody = requestbody.Replace(pr, value);
                            }
                        }
                    }

                    var interval = _repository.GetConfigValue<int>("PushScheduledTripInterval");/*Minutes*/
                    if (interval == 0) interval = -1;
                    var delayed = (log.ScheduledPlacementDate ?? log.TripStartDate) > DateTime.Now.AddMinutes(interval)
                        ? log.TripStartDate.Subtract(DateTime.Now.AddMinutes(interval)).TotalMinutes + point.Order - 1
                        : point.Order - 1;
                    var jobid = BackgroundJob.Schedule<IHangfireJobProcessor>(
                        x => x.CallGpsVendor(endpoint, requestbody, 0, record), TimeSpan.FromMinutes(delayed));
                    try
                    {
                        await _repository.ExecuteSqlAsync("UPDATE [dbo].[tPickDroplog] SET HangfireJobId=@jobid WHERE Id=@id",
                            new SqlParameter("jobid", jobid), new SqlParameter("id", point.Id)).ConfigureAwait(true);
                    }
                    catch (Exception e)
                    {
                        //Ignore
                    }
                }
            }
            catch (Exception e)
            {
                if (!Helper.HostedOnPremise)
                {
                    using (var db = new TenantDbContext())
                    {
                        db.ApiLog.Add(new WebApiUsage()
                        {
                            RequestContent = $"TripLogNo:{log.TriplogNo},TripDate:{log.TripStartDate:f}",
                            ResponseContent = e.GetBaseException().StackTrace,
                            RequestMethod = "",
                            ResponseTimestamp = DateTime.Now,
                            RequestTimestamp = DateTime.Now,
                            ResponseStatusCode = 0
                        });
                        await db.SaveChangesAsync().ConfigureAwait(true);
                    }
                }
                else
                {
                    using (var db=new CoreSettingDb())
                    {
                        db.ApiLog.Add(new WebApiUsage()
                        {
                            RequestContent = $"TripLogNo:{log.TriplogNo},TripDate:{log.TripStartDate:f}",
                            ResponseContent = e.GetBaseException().StackTrace,
                            RequestMethod = "",
                            ResponseTimestamp = DateTime.Now,
                            RequestTimestamp = DateTime.Now,
                            ResponseStatusCode = 0
                        });
                        await db.SaveChangesAsync().ConfigureAwait(true);
                    }
                }
            }
        }
        private async Task CallGpsVendorAsync(GpsEndPoint endpoint, string requestbody, int count = 0, GPSTripUploadViewModel record = null)
        {
            if (count > 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(15));
            }
            count++;
            try
            {
                var client = new RestSharp.RestClient(endpoint.Url);
                var request = new RestSharp.RestRequest((RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), endpoint.Method.ToUpper()));
                if (endpoint.Method == "GET")
                {
                    request.Resource = requestbody.Trim().Replace('\n', ' ');
                }
                else
                {
                    request.AddJsonBody(requestbody);
                }

                var response = await client.ExecuteGetTaskAsync(request);
                if (response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK/*200*/&& (response.Content ?? "").Contains('1')) return;
                if (count == 3)
                {
                    if (!Helper.HostedOnPremise)
                    {
                        using (var db = new TenantDbContext())
                        {
                            db.ApiLog.Add(new WebApiUsage()
                            {
                                IP = response.ResponseUri.ToString(),
                                RequestContent = endpoint.ParameterTemplate,
                                ResponseContent = response.Content,
                                RequestMethod = endpoint.Method,
                                ResponseTimestamp = DateTime.Now,
                                RequestTimestamp = DateTime.Now,
                                ResponseStatusCode = (int)response.StatusCode,
                                RequestHeaders = JsonConvert.SerializeObject(record)
                            });
                            await db.SaveChangesAsync();
                        }
                    }
                }
                if (count < 3)
                {
                    await CallGpsVendorAsync(endpoint, requestbody, count, record);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    if (!Helper.HostedOnPremise)
                    {
                        using (var db = new TenantDbContext())
                        {
                            db.ApiLog.Add(new WebApiUsage()
                            {
                                IP = endpoint.Url,
                                RequestContent = endpoint.ParameterTemplate,
                                ResponseContent = ex.GetBaseException().Message + "\n" + ex.StackTrace,
                                RequestMethod = endpoint.Method,
                                ResponseTimestamp = DateTime.Now,
                                RequestTimestamp = DateTime.Now,
                                RequestHeaders = JsonConvert.SerializeObject(record)
                            });
                            await db.SaveChangesAsync();
                        }
                    }
                }
                catch (Exception)
                {
                    //Ignore
                }

                if (count < 3)
                {
                    await CallGpsVendorAsync(endpoint, requestbody, count, record);
                }
            }
        }
    }
}