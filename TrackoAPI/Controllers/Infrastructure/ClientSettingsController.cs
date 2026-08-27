using CronExpressionDescriptor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using Repository.Pattern.Core.UnitOfWork;
using RestSharp;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoAPI.Infrastructure;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.Global;
using Unity;
using System.Web.Routing;

namespace TrackoAPI.Controllers
{
    [RoutePrefix("api/ClientSettings")]
    public class ClientSettingsController : ApiController
    {
        private readonly IGlobalStore _gStore;
        private readonly IUnityContainer _unity;

        public ClientSettingsController(IUnityContainer container, IGlobalStore globalStore)
        {
            _unity = container;
            _gStore = globalStore;
        }
        [HttpGet]
        [Route("api/routes")]
        public IHttpActionResult GetRoutes()
        {
            var routes = RouteTable.Routes;
            var routeData = new List<RouteData>();

            foreach (RouteBase routeBase in routes)
            {
                var route = routeBase as Route;
                if (route != null && route.Url != "api/{controller}/{action}/{id}")
                {
                    var r = new RouteData();
                    r.Url = route.Url;
                    r.Defaults = route.Defaults;
                    r.DataTokens = route.DataTokens;

                    routeData.Add(r);
                }
            }

            return Ok(routeData);
        }

        public class RouteData
        {
            public string Url { get; set; }
            public RouteValueDictionary Defaults { get; set; }
            public RouteValueDictionary DataTokens { get; set; }
        }

        [HttpGet, Route("InitializeClient({accessCode},{applicationId})")]
        public async Task<IHttpActionResult> InitializeClient([FromUri] int accessCode, [FromUri] string applicationId)
        {
#if !DEBUG
            if (Helper.HostedOnPremise) return BadRequest("Action Not Allowed");
#endif
            using (var db = new TenantDbContext())
            {
                //var client =await
                //    db.Tenants.Where(x => x.AccessCode == accessCode && x.Applications.Any(y=>y.Id==applicationId))
                //        //.Select(x => new {x.ClientKey, x.IsActive, x.ServerUrl, TenantId=x.Id, ClientSecret=x.Secret, TenantName=x.Name,x.ShortName,UpdateUrl=x.Applications.FirstOrDefault(y=>y.Id==applicationId).UpdateUrl})
                //        .SelectMany(x=>x.Applications.Where(y=>y.Id==applicationId), (p, c) =>
                //            new
                //            {
                //                p.ClientKey,
                //                p.IsActive,
                //                p.ServerUrl,
                //                TenantId = p.Id,
                //                ClientSecret = p.Secret,
                //                TenantName = p.Name,
                //                p.ShortName,
                //                UpdateUrl = c.UpdateUrl,
                //                IsAppActive =c.IsActive
                //            })
                //        .FirstOrDefaultAsync();
                var client = await db.TenantApplications
                    .Where(x => x.fk_Tenant.AccessCode == accessCode && x.ApplicationId == applicationId)
                    .Select(x => new
                    {
                        x.fk_Tenant.ClientKey,
                        x.fk_Tenant.IsActive,
                        x.fk_Tenant.ServerUrl,
                        x.TenantId,
                        ClientSecret = x.fk_Tenant.Secret,
                        TenantName = x.fk_Tenant.Name,
                        x.fk_Tenant.ShortName,
                        UpdateUrl = x.UpdateUrl,
                        SetupUrl = x.SetupUrl,
                        FormatUrl = x.FormatUrl,
                        IsAppActive = x.IsActive && x.fk_Application.IsActive
                    }).FirstOrDefaultAsync().ConfigureAwait(true);
                if (client == null || !client.IsActive)
                {
                    return StatusCode(HttpStatusCode.Unauthorized);
                }

                if (!client.IsAppActive)
                {
                    return BadRequest("This Software has been deactivated by IWLT");
                }

                return Ok(client);
            }
        }

        [HttpGet, Route("AppUpdateUrl"), AuthorizeEx]
        public string WinAppUpdateUrl()
        {
            var updateUrl = this.GetClaimByKey<string>("AppUpdateUrl");
            return updateUrl;
        }

        [HttpGet, Route("Ip2Location({ip})")]
        public IHttpActionResult Ip2Location([FromUri] string ip)
        {
            try
            {
                using (WebClient webClient = new WebClient())
                {
                    var result = webClient.DownloadString(
                        $"http://africa.ipinfodb.com/v3/ip-city/?key=8bd134f132fd0713be0880c6ada4c23a65b8d26b695f231971e0574314fb6afb&ip={ip}&format=json");
                    var geoInfo = JsonConvert.DeserializeObject<IPToGeoLocation>(result);
                    return Ok(geoInfo);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet, Route("ServerTime")]
        public IHttpActionResult GetServerDateTime()
        {
            return Ok(DateTime.Now);
        }

        [HttpPost, Route("CronDescription")]
        public IHttpActionResult GetCronDescription([FromBody] CronQuery query)
        {
            return Ok(MyCron.GetDescription(query.Cron));
        }

        [HttpPost, Route("CronNextExecutions")]
        public IHttpActionResult GetCronNextExecutions([FromBody] CronQuery query)
        {
            if (query.StartDate == null) return BadRequest("Start Date is Required");
            return Ok(MyCron.GetNextExecutions(query.StartDate.Value, query.EndDate, query.Cron));
        }

        [Route("GetJsonLogs"), ResponseType(typeof(List<JsonGlobalLog>)), HttpGet, AuthorizeEx]
        public async Task<IHttpActionResult> GetJsonLogsAsync()
        {
            try
            {
                var logs = new List<JsonGlobalLog>();
                var prefix = HttpContext.Current.Request.Headers.Get("prefix");
                var jsonKey = HttpContext.Current.Request.Headers.Get("jsonkey");
                if (string.IsNullOrWhiteSpace(prefix)) return BadRequest("KeyPrefix is required");
                if (!Helper.HostedOnPremise)
                {
                    using (var db = new TenantDbContext())
                    {
                        IQueryable<JsonGlobalLog> query;

                        if (!string.IsNullOrWhiteSpace(jsonKey))
                        {
                            query = db.JsonLog
                                .Where(x => prefix == x.KeyPrefix && jsonKey == x.JsonKey);
                        }
                        else
                        {
                            query = db.JsonLog
                                .Where(x => prefix == x.KeyPrefix);
                        }

                        if (await query.AnyAsync())
                        {
                            logs.AddRange(await query.ToListAsync());
                        }
                    }

                    if (prefix.ToLower() != "gstin_info" || logs.Any()) return Ok(logs);
                    if (string.IsNullOrWhiteSpace(jsonKey) && prefix.ToLower() == "gstin_info")
                        return BadRequest("GSTIN Number is required");
                    DataTable dt = null;
                    using (var uow = _unity.Resolve<IUnitOfWorkAsync>())
                    {
                        var spName = uow.Context.GetApiConfig("gstininfo_procedurename");
                        if (string.IsNullOrWhiteSpace(spName))
                        {
                            spName = "Proc_EWB_Adaequare_GetGSTINDetail";
                        }

                        dt = await uow.SqlQueryAsync(spName,
                            new SqlParameter("parameter1", jsonKey));
                        if (dt == null) return Ok(logs);
                    }

                    var log = new JsonGlobalLog()
                    {
                        JsonKey = jsonKey,
                        KeyPrefix = prefix,
                        JsonData = JsonConvert.SerializeObject(dt)
                    };
                    using (var db = new TenantDbContext())
                    {
                        db.JsonLog.Add(log);
                        await db.SaveChangesAsync();
                    }

                    logs.Add(log);
                    return Ok(logs);
                }
                else
                {
                    using (var db = new CoreSettingDb())
                    {
                        IQueryable<JsonGlobalLog> query;

                        if (!string.IsNullOrWhiteSpace(jsonKey))
                        {
                            query = db.JsonLog
                                .Where(x => prefix == x.KeyPrefix && jsonKey == x.JsonKey);
                        }
                        else
                        {
                            query = db.JsonLog
                                .Where(x => prefix == x.KeyPrefix);
                        }

                        if (await query.AnyAsync() && prefix.ToLower() == "gstin_info")
                        {
                            logs.AddRange(await query.ToListAsync());
                        }
                    }

                    if (logs.Any())
                    {
                        return Ok(logs);
                    }

                    if (prefix.ToLower() != "gstin_info") return Ok(logs);

                    if (string.IsNullOrWhiteSpace(jsonKey) && prefix.ToLower() == "gstin_info")
                        return BadRequest("GSTIN Number is required");
                    try
                    {
                        var client = new RestClient(Helper.GatewayUrl + "/Tenant/GetJsonLogs");
                        var request = new RestRequest(Method.GET);
                        request.AddHeader("godkey", "B41B582F-7B78-4370-A0BD-519E24F8D9B6");
                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Accept", "application/json");
                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("prefix", prefix);
                        request.AddHeader("jsonkey", jsonKey);
                        var res = client.ExecuteAsGet<List<JsonGlobalLog>>(request, "GET");
                        if (res.Data != null && res.Data.All(x => !string.IsNullOrWhiteSpace(x.KeyPrefix) && !string.IsNullOrWhiteSpace(x.JsonKey) && !string.IsNullOrWhiteSpace(x.JsonData)))
                        {
                            logs.AddRange(res.Data);
                            using (var db = new CoreSettingDb())
                            {
                                db.JsonLog.AddRange(res.Data);
                                await db.SaveChangesAsync();
                            }
                        }
                    }
                    catch
                    {
                        //Ignore
                    }

                    if (logs.Any()) return Ok(logs);

                    DataTable dt = null;
                    try
                    {
                        using var uow = _unity.Resolve<IUnitOfWorkAsync>();
                        var spName = uow.Context.GetApiConfig("gstininfo_procedurename");
                        if (string.IsNullOrWhiteSpace(spName))
                        {
                            spName = "Proc_EWB_Adaequare_GetGSTINDetail";
                        }

                        dt = await uow.SqlQueryAsync(spName,
                            new SqlParameter("parameter1", jsonKey));
                    }
                    catch
                    {
                        //Ignore
                    }

                    if (dt == null) return Ok(logs);
                    var log = new JsonGlobalLog()
                    {
                        JsonKey = jsonKey,
                        KeyPrefix = prefix,
                        JsonData = JsonConvert.SerializeObject(dt)
                    };
                    logs.Add(log);
                    try
                    {
                        using var db = new CoreSettingDb();
                        db.JsonLog.Add(log);
                        await db.SaveChangesAsync();
                    }
                    catch
                    {
                        //ignore
                    }

                    try
                    {
                        var client = new RestClient(Helper.GatewayUrl + "/Tenant/PostJsonLogs");
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("godkey", "B41B582F-7B78-4370-A0BD-519E24F8D9B6");
                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Accept", "application/json");
                        request.AddHeader("Content-Type", "application/json");
                        request.AddJsonBody(logs);
                        _ = client.ExecuteAsPost<TPTokenViewModel>(request, "POST");
                    }
                    catch
                    {
                        //ignore
                    }
                }

                return Ok(logs);
            }
            catch(Exception ex)
            {
                return BadRequest($"Unable to Process Reques.{ex.GetBaseException().Message}");
            }
        }
        //private TenantViewModel GetTenantFromDb(string clientKey)
        //{
        //    using (var dbcontext = new TenantDbContext())
        //    {
        //        var vm = dbcontext.Tenants.Where(x => x.ClientKey == clientKey).Select(x =>
        //            new TenantViewModel
        //            {
        //                Id = x.Id,
        //                Name = x.Name,
        //                PostalAddress = x.PostalAddress,
        //                EmailAddress = x.EmailAddress,
        //                Apps = x.Apps.Select(y => new TenantAppViewModel
        //                {
        //                    ApplicationId = y.ApplicationId,
        //                    IsActive = y.IsActive,
        //                    AppName = y.fk_Application.ApplicationName,
        //                    ApplicationType = y.fk_Application.ApplicationType,
        //                    UpdateUrl = y.UpdateUrl,
        //                    NoOfUsers = y.NoOfActiveUsers
        //                }).ToList(),
        //                ConnectionString = x.ConnectionString,
        //                LogType = x.LogType,
        //                IsActive = x.IsActive,
        //                IsSingleUserMode = x.IsSingleUserMode,
        //                ClientKey = x.ClientKey,
        //                PANNo = x.PANNo,
        //                Secret = x.Secret,
        //                AccessCode = x.AccessCode,
        //                ShortName = x.ShortName,
        //                ServerUrl = x.ServerUrl,
        //                PhoneNumber = x.PhoneNumber,
        //                RemoteBackupPath = x.RemoteBackupPath,
        //                WebAddress = x.WebAddress
        //            }).FirstOrDefault();
        //        return vm;
        //    }

        //}
    }
}