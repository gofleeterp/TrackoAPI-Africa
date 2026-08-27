using EntityFramework.Caching;
using EntityFramework.Extensions;

using Hangfire;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OfficeOpenXml.FormulaParsing.Excel.Functions.Numeric;

using RestSharp;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using System.Web.Http;
using System.Web.Http.Description;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.MessageService;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Infrastructure.Services;
using TrackoAPI.ViewModels.Integration;
using TrackoAPI.WebUtilities.Helper;
using Unity;

using static Microsoft.TeamFoundation.Client.CommandLine.Options;

namespace TrackoAPI.Controllers.Infrastructure
{
    [RoutePrefix("api/v2/integration")]
    public class IntegrationController : ApiController
    {
        public IntegrationController()
        {
        }
        [HttpGet,AuthorizeEx,Route(""),ResponseType(typeof(List<JobTrack>))]
        public async Task<IHttpActionResult> GetAsync()
        {
            if (Helper.HostedOnPremise) return BadRequest();
            using (var db=new TenantDbContext())
            {
                var result= await db.Jobs.Where(x => x.TenantId == Helper.LoggedInTenantId).ToListAsync();
                return Ok(result);
            }
        }
        [HttpGet,Route("clearcache")]
#if !DEBUG
        [AuthorizeEx]
#endif
        public IHttpActionResult ClearCache()
        {
            CacheManager.Current.Expire("Global");
            return Ok();
        }
        [HttpPost, Route("event")]
        public async Task<IHttpActionResult> PostEvent([FromBody]EventNotification eventData)
        {
            try
            {
                var receiver = this.Request.GetHeader("receiver");
                var sender = this.Request.GetHeader("sender");
                if (string.IsNullOrWhiteSpace(receiver)) return BadRequest("Receiver identity is required");
                if (string.IsNullOrWhiteSpace(sender)) return BadRequest("Sender identity is required");
                var innerEvent = new InnerEvent
                {
                    //Event = eventData,
                    EventReceivedOn = DateTimeOffset.Now,
                    Receiver = receiver,
                    Sender = sender,
                    EventLogId = Guid.NewGuid().ToString("D")
                };
                string jobid = string.Empty;
                using (var _ctx = new TenantDbContext())
                {
                    var senderrecord = await _ctx.Integrations.Where(x => x.Id == innerEvent.Sender).Select(x => new { x.Id, x.OriginHost, x.Token, Events = x.Events.Select(y => y.EventCode) }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (senderrecord == null) return Unauthorized();
                    var receiverrecord = await _ctx.Tenants.Where(x => x.Id == innerEvent.Receiver).Select(x => new { x.Id,x.IsHostedOnPremise,x.IsActive,x.ServerUrl }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (receiverrecord == null|| !receiverrecord.IsActive) return Unauthorized();
                    if (receiverrecord.IsHostedOnPremise) return BadRequest($"The services you are looking has been moved to this url {receiverrecord.ServerUrl}");
                    if (senderrecord.Events==null ||!senderrecord.Events.Contains(eventData.EventCode)) return BadRequest($"Either Event Code is invalid or You are not subscribed to this event '{eventData.EventCode}'");

                    jobid = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ProcessEvent(innerEvent), TimeSpan.FromMilliseconds(3000));
                    _ctx.Jobs.Add(new JobTrack()
                    {
                        JobLogId = jobid,
                        TenantId = innerEvent.Receiver,
                        SenderId=innerEvent.Sender,
                        EventLogId = innerEvent.EventLogId,
                        EventBody = JsonConvert.SerializeObject(eventData),
                        IsProcessed = false,
                        EventCode=eventData.EventCode,
                        CreatedAt = innerEvent.EventReceivedOn
                    });
                    try
                    {
                        await _ctx.SaveChangesAsync().ConfigureAwait(true);
                    }
                    catch (DbUpdateException ex)
                    {
                        if(!string.IsNullOrWhiteSpace(jobid)) BackgroundJob.Delete(jobid);
                        if (ex.GetBaseException().Message.Contains("statement conflicted with the FOREIGN KEY constraint"))
                        {
                            return BadRequest("Bad Data Provided");
                        }
                        throw new BusinessException(ex);
                    }
                }
                return Ok(new { JobLogId = jobid, EventLogId = innerEvent.EventLogId });
            }
            catch (Exception ex)
            {
                throw new BusinessException(ErrorCode.EventFailed,ex.GetBaseException().Message);
            }
            
        }
        [HttpPost, Route("events")]
        public async Task<IHttpActionResult> PostEvents([FromBody]List<EventNotification> events)
        {
            try
            {
                if (events.Count > 1000)
                {
                    return BadRequest($"In single Request you can only send maximum 1000 Events. And you have sent {events.Count}");
                }

                var eventTime = DateTimeOffset.Now;
                List<dynamic> results = new List<dynamic>();
                var receiver = this.Request.GetHeader("receiver");
                var sender = this.Request.GetHeader("sender");
                if (string.IsNullOrWhiteSpace(receiver)) return BadRequest("Receiver identity is required");
                if (string.IsNullOrWhiteSpace(sender)) return BadRequest("Sender identity is required");

                using (var ctx = new TenantDbContext())
                {
                    var eventCodes = events.Select(x => x.EventCode).Distinct();
                    var senderrecord = await ctx.Integrations.Where(x => x.Id == sender).Select(x => new { x.Id, x.OriginHost, x.Token, Events = x.Events.Select(y => y.EventCode) }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (senderrecord == null) return Unauthorized();
                    var receiverrecord = await ctx.Tenants.Where(x => x.Id == receiver).Select(x => new { x.Id }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (receiverrecord == null) return Unauthorized();
                    if (senderrecord.Events == null || !eventCodes.All(x=> senderrecord.Events.Contains(x))) return BadRequest($"Either one of event Code is invalid or You are not subscribed to that event event");
                    foreach (var eventd in events.GroupBy(x => x.EventCode))
                    {
                        var eventCode = eventd.FirstOrDefault()?.EventCode ?? 0;
                        if (eventCode == 0)
                        {
                            return BadRequest("Event Code not defined");
                        }
                        var innerEvent = new InnerEvent
                        {
                            //Events = eventd.ToList(),
                            EventReceivedOn = eventTime,
                            Receiver = receiver,
                            Sender = sender,
                            EventLogId = Guid.NewGuid().ToString("D"),
                            HasMultipleEvent = true
                        };
                        string jobid = string.Empty;
                        jobid = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ProcessEvent(innerEvent), TimeSpan.FromMilliseconds(3000));
                        ctx.Jobs.Add(new JobTrack()
                        {
                            JobLogId = jobid,
                            TenantId = innerEvent.Receiver,
                            SenderId = innerEvent.Sender,
                            EventLogId = innerEvent.EventLogId,
                            EventBody = JsonConvert.SerializeObject(eventd),
                            IsProcessed = false,
                            EventCode = eventCode,
                            CreatedAt = innerEvent.EventReceivedOn
                        });
                        try
                        {
                            await ctx.SaveChangesAsync().ConfigureAwait(true);
                        }
                        catch (DbUpdateException ex)
                        {
                            if (!string.IsNullOrWhiteSpace(jobid)) BackgroundJob.Delete(jobid);
                            if (ex.GetBaseException().Message.Contains("statement conflicted with the FOREIGN KEY constraint"))
                            {
                                return BadRequest("Bad Data Provided");
                            }
                            throw new BusinessException(ex);
                        }
                        results.Add(new { JobLogId = jobid, EventLogId = innerEvent.EventLogId });
                    }

                }
                return Ok(results);

            }
            catch (Exception ex)
            {
                throw new BusinessException(ErrorCode.EventFailed, ex.GetBaseException().Message);
            }

        }
    }
    [RoutePrefix("api/integration")]
    public class IntegrationPointController : ApiController
    {
        private readonly IGlobalStore _gc;
        private SMTPMailService _emailService;
        public IntegrationPointController(IGlobalStore globalStore)
        {
            _gc=globalStore;
        }
        [HttpGet, AuthorizeEx, Route(""), ResponseType(typeof(List<JobTrack>))]
        public async Task<IHttpActionResult> GetAsync()
        {
            using (var db = new TenantDbContext())
            {
                var result = await db.Jobs.Where(x => x.TenantId == Helper.LoggedInTenantId).ToListAsync();
                return Ok(result);
            }
        }
        [HttpGet, Route("clearcache")]
#if !DEBUG
        [AuthorizeEx]
#endif
        public IHttpActionResult ClearCache()
        {
            CacheManager.Current.Expire("Global");
            _gc.ClearThirdPartyTokens();
            return Ok();
        }
        [HttpPost, Route("v3/event")]
        public async Task<IHttpActionResult> PostEvent([FromBody]EventNotification eventData)
        {
            try
            {
                var receiver = this.Request.GetHeader("receiver");
                var sender = this.Request.GetHeader("sender");
                if (string.IsNullOrWhiteSpace(receiver)) return BadRequest("Receiver identity is required");
                if (string.IsNullOrWhiteSpace(sender)) return BadRequest("Sender identity is required");
                if (eventData.Properties.Count > 2000)
                {
                    return BadRequest($"In single Event you can only send maximum 2000 Properties. And you have sent {eventData.Properties.Count}");
                }

                var job = new JobTrack()
                {
                    TenantId = receiver,
                    SenderId = sender,
                    EventLogId = Guid.NewGuid().ToString("D"),
                    EventBody = JsonConvert.SerializeObject(eventData),
                    IsProcessed = false,
                    EventCode = eventData.EventCode,
                    CreatedAt = DateTimeOffset.Now
                };
                using (var _ctx = new TenantDbContext())
                {
                    var senderrecord = await _ctx.Integrations.Where(x => x.Id == job.SenderId).Select(x => new { x.Id, x.OriginHost, x.Token, Events = x.Events.Select(y => new { y.EventCode, y.AllowConcurrent}) }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (senderrecord == null) return Unauthorized();
                    var receiverrecord = await _ctx.Tenants.Where(x => x.Id == job.TenantId).Select(x => new { x.Id, x.IsHostedOnPremise, x.IsActive, x.ServerUrl }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (receiverrecord == null || !receiverrecord.IsActive) return Unauthorized();
                    if (receiverrecord.IsHostedOnPremise) return BadRequest($"The services you are looking has been moved to this url {receiverrecord.ServerUrl}");
                    if (senderrecord.Events == null || !senderrecord.Events.Any(x=>x.EventCode==eventData.EventCode)) return BadRequest($"Either Event Code is invalid or You are not subscribed to this event '{eventData.EventCode}'");
                    if(senderrecord.Events.Any(x=>x.EventCode== eventData.EventCode && x.AllowConcurrent))
                    {
                        job.JobLogId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ProcessEventById(job.EventLogId, false, null), TimeSpan.FromSeconds(15));
                    }
                    else
                    {
                        job.JobLogId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ProcessFIFOEventById(job.EventLogId, false, null), TimeSpan.FromSeconds(15));
                    }

                    _ctx.Jobs.Add(job);
                    try
                    {
                        await _ctx.SaveChangesAsync().ConfigureAwait(true);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (!string.IsNullOrWhiteSpace(job.JobLogId)) BackgroundJob.Delete(job.JobLogId);
                        if (ex.GetBaseException().Message.Contains("statement conflicted with the FOREIGN KEY constraint"))
                        {
                            return BadRequest("Bad Data Provided");
                        }
                        throw new BusinessException(ex);
                    }
                }
                return Ok(new { JobLogId = job.JobLogId, EventLogId = job.EventLogId });
            }
            catch (Exception ex)
            {
                throw new BusinessException(ErrorCode.EventFailed, ex.GetBaseException().Message);
            }

        }
        [HttpPost, Route("v3/events")]
        public async Task<IHttpActionResult> PostEvents([FromBody]List<EventNotification> events)
        {
            try
            {
                if (events.Count > 2000)
                {
                    return BadRequest($"In single Request you can only send maximum 2000 Events. And you have sent {events.Count}");
                }

                var eventTime = DateTimeOffset.Now;
                List<dynamic> results = new List<dynamic>();
                var receiver = this.Request.GetHeader("receiver");
                var sender = this.Request.GetHeader("sender");
                if (string.IsNullOrWhiteSpace(receiver)) return BadRequest("Receiver identity is required");
                if (string.IsNullOrWhiteSpace(sender)) return BadRequest("Sender identity is required");

                using (var ctx = new TenantDbContext())
                {
                    var eventCodes = events.Select(x => x.EventCode).Distinct();
                    var senderrecord = await ctx.Integrations.Where(x => x.Id == sender).Select(x => new { x.Id, x.OriginHost, x.Token, Events = x.Events.Select(y => new { y.EventCode, y.AllowConcurrent }) }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    //var senderrecord = await ctx.Integrations.Where(x => x.Id == sender).Select(x => new { x.Id, x.OriginHost, x.Token, Events = x.Events.Select(y => y.EventCode) }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (senderrecord == null) return Unauthorized();
                    var receiverrecord = await ctx.Tenants.Where(x => x.Id == receiver).Select(x => new { x.Id }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (receiverrecord == null) return Unauthorized();
                    if (senderrecord.Events == null || !eventCodes.All(x => senderrecord.Events.Any(y=>y.EventCode==x))) return BadRequest($"Either one of event Code is invalid or You are not subscribed to that event event");

                    foreach (var eventd in events.GroupBy(x => x.EventCode))
                    {
                        var eventCode = eventd.FirstOrDefault()?.EventCode ?? 0;
                        if (eventCode == 0)
                        {
                            return BadRequest("Event Code not defined");
                        }

                        var job = new JobTrack()
                        {
                            TenantId = receiver,
                            SenderId = sender,
                            EventLogId = Guid.NewGuid().ToString("D"),
                            EventBody = JsonConvert.SerializeObject(eventd),
                            IsProcessed = false,
                            EventCode = eventd.Key,
                            CreatedAt = eventTime
                        };
                        if (senderrecord.Events.Any(x => x.EventCode == eventd.Key && x.AllowConcurrent))
                        {
                            job.JobLogId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ProcessEventById(job.EventLogId, true, null), TimeSpan.FromSeconds(15));
                        }
                        else
                        {
                            job.JobLogId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ProcessFIFOEventById(job.EventLogId, true, null), TimeSpan.FromSeconds(15));
                        }
                        ctx.Jobs.Add(job);
                        try
                        {
                            await ctx.SaveChangesAsync().ConfigureAwait(true);
                        }
                        catch (DbUpdateException ex)
                        {
                            if (!string.IsNullOrWhiteSpace(job.JobLogId)) BackgroundJob.Delete(job.JobLogId);
                            if (ex.GetBaseException().Message.Contains("statement conflicted with the FOREIGN KEY constraint"))
                            {
                                return BadRequest("Bad Data Provided");
                            }
                            throw new BusinessException(ex);
                        }
                        results.Add(new { JobLogId = job.JobLogId, EventLogId = job.EventLogId });
                    }

                }
                return Ok(results);

            }
            catch (Exception ex)
            {
                throw new BusinessException(ErrorCode.EventFailed, ex.GetBaseException().Message);
            }

        }
        [HttpPost, Route("v3/event/{sender}/{receiver}/{eventcode}")]        
        public async Task<IHttpActionResult> SimpleEvent([FromUri]string sender,[FromUri]string receiver, [FromUri]int eventcode, [FromBody]string body)
        {
            try
            {
                
                if (string.IsNullOrWhiteSpace(receiver)) return BadRequest("Receiver identity is required");
                if (string.IsNullOrWhiteSpace(sender)) return BadRequest("Sender identity is required");
                var job = new JobTrack()
                {
                    TenantId = receiver,
                    SenderId = sender,
                    EventLogId = Guid.NewGuid().ToString("D"),
                    EventBody = body,
                    IsProcessed = false,
                    EventCode = eventcode,
                    CreatedAt = DateTimeOffset.Now
                };
                using (var _ctx = new TenantDbContext())
                {
                    var senderrecord = await _ctx.Integrations.Where(x => x.Id == job.SenderId).Select(x => new { x.Id, x.OriginHost, x.Token, Events = x.Events.Select(y => new { y.EventCode, y.AllowConcurrent }) }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (senderrecord == null) return Unauthorized();
                    var receiverrecord = await _ctx.Tenants.Where(x => x.Id == job.TenantId).Select(x => new { x.Id, x.IsHostedOnPremise, x.IsActive, x.ServerUrl }).FromCacheFirstOrDefaultAsync(tags: new[] { "Global" }).ConfigureAwait(true);
                    if (receiverrecord == null || !receiverrecord.IsActive) return Unauthorized();
                    if (receiverrecord.IsHostedOnPremise) return BadRequest($"The services you are looking has been moved to this url {receiverrecord.ServerUrl}");
                    if (senderrecord.Events == null || !senderrecord.Events.Any(x => x.EventCode == eventcode)) return BadRequest($"Either Event Code is invalid or You are not subscribed to this event '{eventcode}'");
                    if (senderrecord.Events.Any(x => x.EventCode == eventcode && x.AllowConcurrent))
                    {
                        job.JobLogId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ProcessEventById(job.EventLogId, false, null), TimeSpan.FromSeconds(15));
                    }
                    else
                    {
                        job.JobLogId = BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ProcessFIFOEventById(job.EventLogId, false, null), TimeSpan.FromSeconds(15));
                    }

                    _ctx.Jobs.Add(job);
                    try
                    {
                        await _ctx.SaveChangesAsync().ConfigureAwait(true);
                    }
                    catch (DbUpdateException ex)
                    {
                        if (!string.IsNullOrWhiteSpace(job.JobLogId)) BackgroundJob.Delete(job.JobLogId);
                        if (ex.GetBaseException().Message.Contains("statement conflicted with the FOREIGN KEY constraint"))
                        {
                            return BadRequest("Bad Data Provided");
                        }
                        throw new BusinessException(ex);
                    }
                }
                return Ok(new { JobLogId = job.JobLogId, EventLogId = job.EventLogId });
            }
            catch (Exception ex)
            {
                throw new BusinessException(ErrorCode.EventFailed, ex.GetBaseException().Message);
            }

        }
        [HttpPost, Route("v3/get/{procId}")]
        public async Task<IHttpActionResult> GetData([FromUri]long procId, [FromBody]IDictionary<string,object> data)
        {
            var enablediagnosis = !string.IsNullOrEmpty(this.Request.GetHeader("verbose"));
            try
            {
                var token = this.Request.GetHeader("token");
                if (string.IsNullOrWhiteSpace(token))
                {
                    return new TextResult("Token should be provided", Request, HttpStatusCode.Unauthorized);
                }
                if (procId <= 0)
                {
                    return new TextResult("InetegrationId is Required", Request, HttpStatusCode.BadRequest);
                }
                TPTokenViewModel tokenInfo = null;
                tokenInfo = !Helper.HostedOnPremise ? _gc.GetOrAddToken(token, GetTokenInfoFromDb) : _gc.GetOrAddToken(token);
                if (tokenInfo == null) {
                    return new TextResult("Invalid token", Request, HttpStatusCode.Unauthorized);
                }
                
                var isvalid = tokenInfo.IsValidCall(out var errormessage, "integration", $"v3/get/{procId}");
                if (!isvalid) {
                    return new TextResult(errormessage, Request, HttpStatusCode.Unauthorized);
                }
                _gc.UpdateTokenAccessTime(token, DateTime.Now);
                tokenInfo.LastCalledTime = DateTime.Now;
                if (string.IsNullOrWhiteSpace(tokenInfo.JsonMetaData)) {
                    return new TextResult("Token not configured to use this resource", Request, HttpStatusCode.BadRequest);
                }

                var metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(tokenInfo.JsonMetaData);
                if (!metadata.TryGetValue("ProcId", out object intprocid)|| ((long)intprocid)!= procId)
                { 
                    return new TextResult($"Token not configured to use this resource. Hint:{procId}!= configuredValue", Request, HttpStatusCode.BadRequest);
                }
                using (var db=new TrackoApiDbContext(new TenantConnection() {TenantId= tokenInfo.TenantId},_gc))
                {
                    var proc = await db.ReportProcedures.Where(x=>x.Id==procId).FromCacheFirstOrDefaultAsync(tags:new[] { tokenInfo.TenantId, procId.ToString() });
                    if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                    {
                        return BadRequest("Source is Not Configured.");
                    }
                    proc.UsaseCount++;
                    proc.ObjectState = ObjectState.Modified;
                    object[] parameters = null;
                    if (data != null && data.Any())
                    {
                        BuildCallable(proc.StoredProcedureName, data, ref parameters);
                    }
                    var resultdt = await this.SqlQueryAsync(db, proc.StoredProcedureName, parameters);
                    await db.SaveChangesAsync();                    
                    return Ok(resultdt);
                }
            }
            catch (Exception ex)
            {
                if(ex is BusinessException)
                {
                    throw ex;
                }
                if (enablediagnosis)
                {
                    return new TextResult($"{ex.Message}\r\n{ex.StackTrace}", Request, HttpStatusCode.InternalServerError);
                }
                else
                {
                    return new TextResult("internal Error in Processing Request", Request, HttpStatusCode.InternalServerError);
                }
            }
        }
        private void BuildCallable(string proc, IDictionary<string,object> req, ref object[] parameters)
        {
            var list = new List<object>();

            foreach (var field in req.AsEnumerable())
            {
                if (!proc.ToLower().Contains($"@{field.Key.ToLower()}") || proc.ToLower().Contains($"@{field.Key.ToLower()}=")) continue;
                if (parameters == null)
                {
                    parameters = new object[] { };
                }

                var value = field.Value?.ToString();
                list.Add(string.IsNullOrWhiteSpace(value)
                    ? new SqlParameter(field.Key.ToLower(), DBNull.Value)
                    : new SqlParameter(field.Key.ToLower(), value));
            }
            if (list.Any())
            {
                parameters = list.ToArray();
            }
        }
        private TPTokenViewModel GetTokenInfoFromDb(string token)
        {
            using (var db=new TenantDbContext())
            {
                return db.ThirdPartyTokens
                .Where(x => token == x.Token)
                .Select(x => new TPTokenViewModel
                {
                    Token = x.Token,
                    AllowedPath = x.AllowedPath,
                    Appidentity = x.Appidentity,
                    ExpiryDate = x.ExpiryDate,
                    Interval = x.Interval,
                    IsDeactivated = x.IsDeactivated,
                    JsonMetaData = x.JsonMetaData,
                    LastCalledTime = x.LastCalledTime,
                    TenantId = x.TenantId
                }).FirstOrDefault();
            }
        }
        private async Task<DataTable> SqlQueryAsync(TrackoApiDbContext _dataContext, string sql, params object[] parameters)
        {
            var existingconnection = _dataContext.Database.CurrentTransaction != null || _dataContext.Database.Connection.State == ConnectionState.Open;
            var connection = _dataContext.Database.CurrentTransaction?.UnderlyingTransaction?.Connection ?? _dataContext.Database.Connection;
            var dt = new DataTable();
            using (System.Data.IDbCommand command = connection.CreateCommand())
            {
                try
                {
                    if (!existingconnection)
                    {
                        await connection.OpenAsync();
                    }
                    else
                    {
                        command.Transaction = _dataContext.Database.CurrentTransaction?.UnderlyingTransaction;
                    }

                    command.CommandText = sql.Replace(" ", "").Split('@')[0];
                    command.CommandTimeout = command.Connection.ConnectionTimeout;
                    command.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }
                    }

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
                finally
                {
                    if (!existingconnection)
                        connection.Close();
                    command.Parameters.Clear();
                }
            }
            return dt;
        }
        [AllowAnonymous]
        [Route("Formaters"), HttpGet]
        public IHttpActionResult GetAllFormatProviders()
        {
            var formaters =Startup.config.Formatters.Select(x => new {x.GetType().Name, SupportedMediaTypes=x.SupportedMediaTypes.Select(y=>y.MediaType), x.MediaTypeMappings, x.SupportedEncodings });
            return Ok(formaters);
        }
        [AllowAnonymous]
        [Route("GenerateToken"), HttpGet]
        public IHttpActionResult GenerateToken()
        {
            var randomText = TrackoApi.Core.Helpers.Helper.RandomString(30);
            return Ok(TrackoApi.Core.Helpers.Helper.GetHash(randomText));
        }
        [Route("v3/get/schedulebatchhttp/{sender}/{procid}/{delay}/{batchId}"), HttpGet]
        public IHttpActionResult ScheduleHttpBatchRequests([FromUri] string sender, [FromUri] long procid, [FromUri] string batchId,[FromUri] int delay/*millisecond*/)
        {
            if (string.IsNullOrWhiteSpace(sender)) return BadRequest("Sender is required");
            if (string.IsNullOrWhiteSpace(batchId)) return BadRequest("BatchId is required");
            if (delay <= 0) delay = 10000;
           var hangfirejobid= Hangfire.BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ScheduleHttpCall(batchId, sender, procid, null), TimeSpan.FromMilliseconds(delay));
            return Ok(hangfirejobid);
        }
        /*Multi thread with delay added- single call at a time*/
        [Route("v3/get/batchhttpsingle/{sender}/{batchId}"), HttpGet]
        public async Task<IHttpActionResult> ProcessSingleHttpBatchRequests([FromUri] string sender,[FromUri]string batchId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sender)) return BadRequest("Sender is required");
                if (string.IsNullOrWhiteSpace(batchId)) return BadRequest("BatchId is required");
                var ctx = new TrackoApiDbContext(new TenantConnection { TenantId= sender },_gc);
                var requests=ctx.HttpRequestPools.Where(x=>x.BatchId==batchId).ToList();
                var urls = requests.Select(x => x.Uri).Distinct().ToList();
                var tasks = new List<Task>();
                RestClient client = new RestClient();                
                int ctr = 0;
                foreach (var req in requests)
                {
                    tasks.Add(client.AddRequestWithDelay(req, ctr++));
                }
                await Task.WhenAll(tasks);
                await ctx.SaveChangesAsync();
                ctx.Dispose();
                return Ok();
            }catch (Exception ex)
            {
                return await Task.FromResult(BadRequest(ex.GetBaseException().Message));
            }
        }

        /*Multi threading to get data from happay and other vendors without delay*/
        [Route("v3/get/batchhttp/{sender}/{batchId}"), HttpGet]
        public async Task<IHttpActionResult> ProcessHttpBatchRequests([FromUri] string sender, [FromUri] string batchId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(sender)) return BadRequest("Sender is required");
                if (string.IsNullOrWhiteSpace(batchId)) return BadRequest("BatchId is required");
                var ctx = new TrackoApiDbContext(new TenantConnection { TenantId = sender }, _gc);
                var requests = ctx.HttpRequestPools.Where(x => x.BatchId == batchId).ToList();
                var urls = requests.Select(x => x.Uri).Distinct().ToList();
                var tasks = new List<Task>();
                RestClient client = new RestClient();

                foreach (var req in requests)
                {
                    tasks.Add(client.AddRequest(req));
                }
                await Task.WhenAll(tasks);
                await ctx.SaveChangesAsync();
                ctx.Dispose();
                return Ok();
            }
            catch (Exception ex)
            {
                return await Task.FromResult(BadRequest(ex.GetBaseException().Message));
            }
        }

        [Route("v3/send/emailV2/{sender}/{batchId}"), HttpGet, ResponseType(typeof(EmailResponse))]
        public async Task<IHttpActionResult> SendEmailV2Async([FromUri] string sender, [FromUri] string batchId)
        {
            HttpRequestPool req = null; // <-- Declare req early
            TrackoApiDbContext ctx = null;

            try
            {
                var emailservice = Unity.Config.UnityCore.Container.Resolve<ISendGridEmailService>();
                if (string.IsNullOrWhiteSpace(sender)) return BadRequest("Sender is required");
                if (string.IsNullOrWhiteSpace(batchId)) return BadRequest("BatchId is required");

                ctx = new TrackoApiDbContext(new TenantConnection { TenantId = sender }, _gc);
                req = ctx.HttpRequestPools.FirstOrDefault(x => x.BatchId == batchId);
                req.Result = "Preparing";

                if (req == null)
                    return BadRequest("No request found with the provided batchId");

                var email = JsonConvert.DeserializeObject<SendGridEmailViewModel>(req.RequestBody);

                if (string.IsNullOrWhiteSpace(email.HtmlBody) && string.IsNullOrWhiteSpace(email.PlanTextBody))
                {
                    req.Result = "Email Body is required";
                }
                if (string.IsNullOrWhiteSpace(email.Subject))
                {
                    req.Result = "Email Subject is required";
                }
                if (email.Tos == null || !email.Tos.Any())
                {
                    req.Result = "Email Recipients are required";
                }
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await emailservice.SendAsync(email, req);
                req.Result = JsonConvert.SerializeObject(response);

                await ctx.SaveChangesAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                if (req != null)
                {
                    req.Result = "Exception: " + ex.GetBaseException().Message;
                    if (ctx != null)
                        await ctx.SaveChangesAsync();
                }

                return await Task.FromResult(BadRequest(ex.GetBaseException().Message));
            }
            finally
            {
                ctx?.Dispose();
            }
        }



        //private async Task SingleHttpCall(IRestClient client,HttpRequestPool req)
        //{
        //    try
        //    {
        //        var contentType = "application/json";
        //        var watch = new Stopwatch();

        //        var method = (RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), req.Method.ToUpper());

        //        var request = new RestRequest(req.Uri, method);
        //        foreach (var item in req._headers)
        //        {
        //            if (item.Key.ToLower() == "Content-Type"&&!string.IsNullOrWhiteSpace(item.Value.ToString())) contentType = item.Value.ToString();
        //            request.AddHeader(item.Key, item.Value.ToString());
        //        }
        //        if (req.Timeout <= 0)
        //        {
        //            req.Timeout = 18000;
        //        }
        //        if (contentType.Contains("json"))
        //        {
        //            request.RequestFormat=DataFormat.Json;
        //        }else if(contentType.Contains("xml"))
        //        {
        //            request.RequestFormat = DataFormat.Xml;
        //        }
        //        if (!string.IsNullOrWhiteSpace(req.RequestBody))
        //        {
        //            request.AddParameter(contentType, req.RequestBody, ParameterType.RequestBody);                  
        //        }
        //        watch.Start();
        //        var res = await client.ExecuteTaskAsync(request);
        //        watch.Stop();                
        //        req.Result = res.Content;
        //        if (string.IsNullOrWhiteSpace(req.Result))
        //        {
        //            req.Result = res.ErrorMessage;
        //        }
        //        if (string.IsNullOrWhiteSpace(req.Result)&& res.ErrorException!=null)
        //        {
        //            req.Result = res.ErrorException.GetBaseException().Message;
        //        }
        //        req.ExecutedTime=req.ProcessTime = DateTime.Now;
        //        req.IsProceeded = true;
        //        if (req.LogRequest)
        //        {
        //            var log = LogRequest(client, request, res, watch.ElapsedMilliseconds);
        //            req.LogData = JsonConvert.SerializeObject(log);
        //        }
        //    }
        //    catch(Exception ex)
        //    {
        //        req.Result=ex.GetBaseException()?.Message;
        //    }

        //}
        //private dynamic LogRequest(IRestClient _restClient, IRestRequest request, IRestResponse response, long durationMs)
        //{
        //    var requestToLog = new
        //    {
        //        resource = request.Resource,
        //        // Parameters are custom anonymous objects in order to have the parameter type as a nice string
        //        // otherwise it will just show the enum value
        //        parameters = request.Parameters.Select(parameter => new
        //        {
        //            name = parameter.Name,
        //            value = parameter.Value,
        //            type = parameter.Type.ToString()
        //        }),
        //        // ToString() here to have the method as a nice string otherwise it will just show the enum value
        //        method = request.Method.ToString(),
        //        // This will generate the actual Uri used in the request
        //        uri = _restClient.BuildUri(request)
        //    };

        //    var responseToLog = new
        //    {
        //        statusCode = response.StatusCode,
        //        content = response.Content,
        //        headers = response.Headers,
        //        // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
        //        responseUri = response.ResponseUri,
        //        errorMessage = response.ErrorMessage,
        //    };

        //    return new
        //    {
        //        requestToLog,
        //        responseToLog
        //    };
        //}
    }
}