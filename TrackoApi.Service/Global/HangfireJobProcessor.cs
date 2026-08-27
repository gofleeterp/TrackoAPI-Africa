using EntityFramework.BulkInsert.Extensions;
using EntityFramework.Caching;
using EntityFramework.Extensions;
using Hangfire;
using Hangfire.States;
using Hangfire.Console;
using Hangfire.Server;
using MoreLinq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using OfficeOpenXml.Table;
using RestSharp;
using Stubble.Core;
using Stubble.Core.Builders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Migrations;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.MessageService;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.FMS.GPS;
using TrackoApi.Models.Global;
using TrackoApi.Models.Global.CronJobs;
using TrackoAPI.Code.Logics;
using TrackoAPI.Infrastructure.Services;
using TrackoAPI.Reporting.Models;
using TrackoAPI.Reports.ViewModels;
using TrackoAPI.Reports.ViewModels.FMS;
using TrackoAPI.Reports.ViewModels.Global.Integration;
using TrackoAPI.ViewModels.Integration;
using Unity;
using SqlParameter = System.Data.SqlClient.SqlParameter;
using System.Threading.Tasks;
using System.Data.Entity;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;

namespace TrackoApi.Service.Global
{
    public interface IHangfireJobProcessor
    {
        [Queue("fifo_event_automation"), DisableConcurrentExecution(60)]
        void RunFuelAutomation(long triplogId, string tenantId);
        
        [Queue("fifo_event_automation"), DisableConcurrentExecution(60)]
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        void RunFuelAutomation(long triplogId, long sessionId, string tenantId);
        
        [Queue("fifo_event_automation"), DisableConcurrentExecution(60)]
        void RunFuelAutomationByVehicle(long vehicleid, string tenantId);
        
        [Queue("fifo_event_automation"), DisableConcurrentExecution(60)]
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        void RunFuelAutomationByVehicle(long vehicleid, long sessionId, string tenantId);

        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(2)]
        void ProcessEvent(InnerEvent innerEvent);
        [Queue("fifo_event_processing"),AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(2),DisableConcurrentExecution(60)]
        void ProcessFIFOEventById(string eventId, bool isbatchRequest, PerformContext context = null);
        [Queue("event_processing"), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(2)]
        void ProcessEventById(string eventId, bool isbatchRequest, PerformContext context = null);
        void ReRunAllCustomSchedule(PerformContext context = null);
        void CleanOldJobLogs();
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        void PushToGPSProvider(long tripId, string tenantId,PerformContext context = null);
        [ProlongExpirationTime]
        void CallGpsVendor(GpsEndPoint endpoint, string requestbody, int count = 0,
            GPSTripUploadViewModel record = null);
        void ScheduleHttpCall(string batchId, string senderId, long procId = 0, PerformContext context = null);
        void CreateThumbnail(string fileName);
        [Queue("business_queue"), DisableConcurrentExecution(60)]
        void RunBusinessSchedule(PerformContext context, long? scheduleId, string tenantId);
        void TopupEmailFreeBalance();
        void SyncGPSStatusLog(PerformContext context);
        [Queue("fifo_event_processing"), DisableConcurrentExecution(60), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        void PushChildTrip(long tripId, string tenantId, int retry = 0, PerformContext pcontext = null);
        [Queue("fifo_post_transaction"), DisableConcurrentExecution(60), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        void RunTripPostProcess(PerformContext context, long triplogId, long sessionId, string tenantId);
        [Queue("fifo_post_transaction"), DisableConcurrentExecution(60), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        void RunCNPostProcess(PerformContext context, long cnId, long sessionId, string tenantId);

        [Queue("fifo_event_processing"), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(2), DisableConcurrentExecution(60)]
        void SyncAPLConfigInAPLAnnexureLevel(long _viewid, string tenantId, long _sessionId, int retry = 0, PerformContext pcontext = null);
    }
    public class HangfireJobProcessor : IHangfireJobProcessor
    {
        private readonly IGlobalStore _gs;
        public HangfireJobProcessor(IGlobalStore globalStore)
        {
            _gs = globalStore;
        }
        public void TopupEmailFreeBalance()
        {
            try
            {
                if (!Helper.HostedOnPremise)
                {
                    using (var ctx = new TenantDbContext())
                    {
                        var toBeDeleted = ctx.NotificationPurchaseLog.Where(x => x.PaymentStatus == TrackoAPI.Models.Shared.PurchaseType.FreeThreshold).ToList();
                        toBeDeleted.ForEach(x =>
                        {
                            x.ExpiryTime = DateTimeOffset.Now;
                        });
                        //foreach (var d in toBeDeleted)
                        //{
                        //    ctx.Entry(d).State = EntityState.Modified;
                        //}
                        var newentries = ctx.Tenants.Where(x => x.IsActive).Select(x => x.Id).ToList().Select(x => new NotificationPurchase
                        {
                            Balance = 100,
                            NoOfNotification = 100,
                            NotificationType = TrackoAPI.Models.Shared.NotificationType.Email,
                            PaymentStatus = TrackoAPI.Models.Shared.PurchaseType.FreeThreshold,
                            PurchaseRate = 0.10M,
                            PurchaseTime = DateTimeOffset.Now,
                            TenantId = x
                        });
                        foreach (var d in newentries)
                        {
                            ctx.NotificationPurchaseLog.Add(d);
                        }
                        ctx.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                //PerformContext context = null;
                //context.WriteLine(ConsoleTextColor.Red, ex.GetBaseException().Message);
            }
        }
        [AutomaticRetry(Attempts = 2, LogEvents =true,OnAttemptsExceeded =AttemptsExceededAction.Delete),ProlongExpirationTime]
        public void CreateThumbnail(string fileName)
        {

            try
            {
                var file = new FileInfo(fileName);
                if (!HasImageExtension(fileName) || file.Directory == null || !file.Exists) return;
                var thumdir = Path.Combine(file.Directory.FullName, "thumbnails");
                using (Image image = Image.FromFile(fileName))
                {
                    var smalldir = Path.Combine(thumdir, "small");
                    if (!Directory.Exists(smalldir))
                    {
                        Directory.CreateDirectory(smalldir);
                    }
                    using (Image small = image.GetThumbnailImage(75, 75, () => false, IntPtr.Zero))
                    {
                        small.Save(Path.ChangeExtension(Path.Combine(smalldir, file.Name), file.Extension));
                    }


                    var smallx2dir = Path.Combine(thumdir, "smallx2");
                    if (!Directory.Exists(smallx2dir))
                    {
                        Directory.CreateDirectory(smallx2dir);
                    }
                    using (Image small = image.GetThumbnailImage(150, 150, () => false, IntPtr.Zero))
                    {
                        small.Save(Path.ChangeExtension(Path.Combine(smallx2dir, file.Name), file.Extension));
                    }


                    var mediumdir = Path.Combine(thumdir, "medium");
                    if (!Directory.Exists(mediumdir))
                    {
                        Directory.CreateDirectory(mediumdir);
                    }
                    using (Image small = image.GetThumbnailImage(480, 320, () => false, IntPtr.Zero))
                    {
                        small.Save(Path.ChangeExtension(Path.Combine(mediumdir, file.Name), file.Extension));
                    }
                }
            }
            catch (Exception ex)
            {
                //PerformContext context = null;
                //context.WriteLine(ConsoleTextColor.Red, ex.GetBaseException().Message);
            }
        }

        private static bool HasImageExtension(string source)
        {
            return (source.EndsWith(".png") || source.EndsWith(".jpg") || source.EndsWith(".jpeg") || source.EndsWith(".jfif") || source.EndsWith(".bmp") || source.EndsWith(".tif") || source.EndsWith(".tiff") || source.EndsWith(".gif"));
        }



        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]

        public void ProcessEvent(InnerEvent innerEvent)
        {
            try
            {
                object job = null;
                string dbname;
                string eventBody = String.Empty;
                long EventCode = 0;
                using (var ctx = new TenantDbContext())
                {
                    dbname = ctx.Database.Connection.Database;
                    var jobi = ctx.Jobs.Where(x => x.EventLogId == innerEvent.EventLogId).Select(x => new
                    {
                        EventName = x.fk_Event.Name,
                        SenderName = x.fk_Sender.Name,
                        x.JobLogId,
                        x.EventBody,
                        x.EventCode,
                        x.fk_Tenant.IsHostedOnPremise,
                        x.fk_Tenant.IsActive
                    }).FirstOrDefault();
                    if (jobi == null) throw new BusinessException(ErrorCode.JobFailed, $"Job with id {innerEvent.EventLogId} not found.");
                    if (!jobi.IsActive || jobi.IsHostedOnPremise) return;
                    eventBody = jobi.EventBody;
                    job = jobi;
                    EventCode = jobi.EventCode;
                }

                var isArray = innerEvent.HasMultipleEvent;
                if (isArray)
                {
                    innerEvent.Events =
                        JsonConvert.DeserializeObject<List<EventNotification>>(eventBody);
                }
                else
                {
                    innerEvent.Event =
                        JsonConvert.DeserializeObject<EventNotification>(eventBody);
                }
                var evtd = isArray ? innerEvent.Events.FirstOrDefault() : innerEvent.Event;
                if (evtd == null) return;
                using (ITrackoApiDbContext tdc = new TrackoApiDbContext(new TenantConnection { TenantId = innerEvent.Receiver }, _gs))
                {
                    if (!tdc.EventStorage.Any(x => x.EventLogId == innerEvent.EventLogId))
                    {
                        var evt = new EventStorage()
                        {
                            EventDataIsListObject = isArray,
                            EventCode = evtd.EventCode,
                            EventData = !isArray ? evtd.Properties : null,
                            EventDataArray = isArray ? innerEvent.Events.Select(x => x.Properties).ToList() : null,
                            EventLogId = innerEvent.EventLogId,
                            EventName = job.GetPropertyValue<string>("EventName"),
                            IsProcessed = false,
                            JobLogId = job.GetPropertyValue<string>("JobLogId"),
                            EventTime = evtd.EventTime,
                            EventReceivedTime = innerEvent.EventReceivedOn,
                            SenderId = innerEvent.Sender,
                            SenderName = job.GetPropertyValue<string>("SenderName")
                        };
                        tdc.EventStorage.Add(evt);
                        var count = tdc.SaveChanges();
                        if (count > 0)
                        {
                            tdc.Database.ExecuteSqlCommand(
                                $"UPDATE [{dbname}].[dbo].[JobTracks] SET [IsProcessed]=1,[ProcessedTime]=SYSDATETIMEOFFSET() WHERE [EventLogId]=@id",
                                new SqlParameter("id", innerEvent.EventLogId));
                        }
                    }

                    var transaction = tdc.Database.CurrentTransaction ?? tdc.Database.BeginTransaction(IsolationLevel.ReadCommitted);
                    try
                    {
                        var procName = tdc.GetApiConfig($"EventProcessingprocName_{EventCode}");
                        if (string.IsNullOrWhiteSpace(procName))
                        {
                            procName = "[dbo].[Proc_GLB_HandleIntegration]@EventLogId";
                        }
                        using (var cmd = tdc.Database.Connection.CreateCommand())
                        {
                            cmd.Transaction = transaction.UnderlyingTransaction;
                            cmd.CommandText = procName.Replace(" ", "").Split('@')[0];
                            cmd.CommandTimeout = cmd.Connection.ConnectionTimeout;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("EventLogId", innerEvent.EventLogId));
                            cmd.ExecuteNonQuery();
                        }
                        //tdc.ExecuteProcedure(procName, new SqlParameter("EventLogId", innerEvent.EventLogId));
                        //tdc.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, procName, new SqlParameter("EventLogId",innerEvent.EventLogId));
                        transaction.Commit();

                    }
                    catch (Exception e)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                //PerformContext context = null;
                //context.WriteLine(ConsoleTextColor.Red, ex.GetBaseException().Message);
            }
        }

        [Queue("fifo_event_processing"), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(2), DisableConcurrentExecution(60)]
        public void ProcessFIFOEventById(string eventId, bool isbatchRequest, PerformContext context = null)
        {
            try
            {
                ProcessEvent(eventId, isbatchRequest, context);
            }
            catch (Exception ex)
            {
                //context.WriteLine(ConsoleTextColor.Red, ex.GetBaseException().Message);
            }
        }

        [Queue("event_processing"), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        public void ProcessEventById(string eventId, bool isbatchRequest, PerformContext context = null)
        {
            try
            {
                ProcessEvent(eventId, isbatchRequest, context);
            }
            catch (Exception ex)
            {
                //context.WriteLine(ConsoleTextColor.Red, ex.GetBaseException().Message);
            }
        }
        const string eventUpdateQuery = "UPDATE dbo.EventStorages SET IsProcessed=@IsProcessed,ProcessedTime=GETDATE(),Error=ISNULL(@error,Error) WHERE EventLogId=@eventLogId";
        private void ProcessEvent(string eventId, bool isbatchRequest, PerformContext context = null)
        {
            context.WriteLine($"Processing EventLogId {eventId}");
            object job = null;
            string dbname;
            string eventBody = String.Empty,tenantid,senderid;
            DateTimeOffset? eventReceivedOn;
            long EventCode = 0;
            using (var ctx = new TenantDbContext())
            {
                dbname = ctx.Database.Connection.Database;
                context.WriteLine($"Processing EventLogId {eventId} for database {dbname}");
                var jobi = ctx.Jobs.Where(x => x.EventLogId == eventId).Select(x => new
                {
                    EventName = x.fk_Event.Name,
                    SenderName = x.fk_Sender.Name,
                    x.TenantId,
                    x.SenderId,
                    x.JobLogId,
                    x.EventBody,
                    x.CreatedAt,
                    x.EventCode,
                x.fk_Tenant.IsHostedOnPremise,
                    x.fk_Tenant.IsActive
                }).FirstOrDefault();
                if (jobi == null) throw new BusinessException(ErrorCode.JobFailed, $"Job with id {eventId} not found.");
                if (!jobi.IsActive || jobi.IsHostedOnPremise) return;
                eventBody = jobi.EventBody;
                tenantid = jobi.TenantId;
                senderid = jobi.SenderId;
                eventReceivedOn = jobi.CreatedAt;
                job = jobi;
                EventCode = jobi.EventCode;
            }
            var events =isbatchRequest? JsonConvert.DeserializeObject<List<EventNotification>>(eventBody):null;
            var singleEvent =!isbatchRequest?
                JsonConvert.DeserializeObject<EventNotification>(eventBody):null;
            var evtd = isbatchRequest ? events?.FirstOrDefault() : singleEvent;
            if (evtd == null) return;
            context.WriteLine($"Opening tenant databse for TenantId {tenantid}");
            using (ITrackoApiDbContext tdc = new TrackoApiDbContext(new TenantConnection { TenantId = tenantid }, _gs))
            {
                context.WriteLine($"Database intialized");
                if (!tdc.EventStorage.Any(x => x.EventLogId == eventId))
                {
                    context.WriteLine($"Adding Event to Event Storage");
                    var evt = new EventStorage()
                    {
                        EventDataIsListObject = isbatchRequest,
                        EventCode = evtd.EventCode,
                        EventData = !isbatchRequest ? evtd.Properties : null,
                        EventDataArray = isbatchRequest ? events?.Select(x => x.Properties).ToList() : null,
                        EventLogId = eventId,
                        EventName = job.GetPropertyValue<string>("EventName"),
                        IsProcessed = false,
                        JobLogId = job.GetPropertyValue<string>("JobLogId"),
                        EventTime = evtd.EventTime,
                        EventReceivedTime = eventReceivedOn??DateTimeOffset.Now,
                        SenderId = senderid,
                        SenderName = job.GetPropertyValue<string>("SenderName")
                    };
                    tdc.EventStorage.Add(evt);
                    var count = tdc.SaveChanges();
                    if (count > 0)
                    {
                        context.WriteLine($"Event Added in Event Storage now marking event in global storage as Processed");
                        tdc.Database.ExecuteSqlCommand(
                            $"UPDATE [{dbname}].[dbo].[JobTracks] SET [IsProcessed]=1,[ProcessedTime]=SYSDATETIMEOFFSET() WHERE [EventLogId]=@id",
                            new SqlParameter("id", eventId));
                    }
                }
                context.WriteLine($"Processing Event on Tenant Database{(tdc.Database.CurrentTransaction==null?" Using New Transaction":"Using Existing Transaction")}");
                string ErrorMessage = string.Empty;
                bool isfaulty = false;
                using (var transaction = tdc.Database.CurrentTransaction ?? tdc.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    try
                    {
                        var procName = tdc.GetApiConfig($"EventProcessingprocName_{EventCode}");
                        if (string.IsNullOrWhiteSpace(procName))
                        {
                            procName = "[dbo].[Proc_GLB_HandleIntegration]@EventLogId";
                        }
                        context.WriteLine($"Using {procName} as Stored Procedure for EventLogId {eventId}");
                        //tdc.ExecuteProcedure(procName, new SqlParameter("EventLogId", eventId));
                        using (var cmd=tdc.Database.Connection.CreateCommand())
                        {
                            cmd.Transaction = transaction.UnderlyingTransaction;
                            cmd.CommandText = procName.Replace(" ", "").Split('@')[0];
                            cmd.CommandTimeout = cmd.Connection.ConnectionTimeout;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("EventLogId", eventId));
                            cmd.ExecuteNonQuery();
                        }
                        context.WriteLine($"Commiting Transaction");
                        //tdc.Database.ExecuteSqlCommand(TransactionalBehavior.EnsureTransaction, procName, new SqlParameter("EventLogId", eventId));
                        transaction.Commit();
                        context.WriteLine($"Transaction Commited");

                    }
                    catch (Exception e)
                    {
                        context.WriteLine($"Error was thrown while processing event at Tenant Database. Rolling back Transaction");
                        isfaulty = true;
                        ErrorMessage = e.GetBaseException().Message;
                        context.WriteLine($"Transaction Rolled Back");
                    }
                    context.WriteLine($"Transaction Completed and Commited");
                }
                context.WriteLine($"Marking Event as Status as {(isfaulty?"UnProcessed":"Processed")}");
                if (tdc.Database.Connection.State != ConnectionState.Open)
                {
                    tdc.Database.Connection.Open();
                }
                using (var cmd = tdc.Database.Connection.CreateCommand())
                {
                    cmd.CommandText = eventUpdateQuery;
                    cmd.CommandType = CommandType.Text;
                    cmd.Parameters.Add(new SqlParameter("eventLogId", eventId));
                    cmd.Parameters.Add(new SqlParameter("IsProcessed", isfaulty ? 0 : 1));
                    cmd.Parameters.Add(new SqlParameter("error", isfaulty ? (object)ErrorMessage : DBNull.Value));                  
                    cmd.ExecuteNonQuery();
                }
                if (isfaulty)
                {
                    throw new BusinessException(ErrorCode.EventFailed, ErrorMessage);
                }
            }
        }

        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        public void CallGpsVendor(GpsEndPoint endpoint, string requestbody,int count,GPSTripUploadViewModel record)
        {
            var stopwatch = new Stopwatch();
            
            if (count > 0)
            {
                Thread.Sleep(TimeSpan.FromSeconds(5));//.Delay(TimeSpan.FromSeconds(5));
            }
            count++;
            try
            {
                if (requestbody.Contains("&docnum=&"))
                {
                    using (ITrackoApiDbContext tdc = new TrackoApiDbContext(new TenantConnection { TenantId = record.TenantId }, _gs))
                    {
                        var cnlist = (tdc.CNChallans.Where(x => x.TriplogId == record.Id && x.CNId > 0).Select(x => x.fk_CNMaster.CNNo).ToList());
                        if (cnlist?.Count > 0)
                        {
                            requestbody = requestbody.Replace("&docnum=&", $"&docnum={cnlist.JoinStrings(",")}&");
                        }
                    }
                }
            }
            catch
            {
                //Ignore
            }
            try
            {
                var client = new RestSharp.RestClient(endpoint.Url);
                var request = new RestSharp.RestRequest((RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), endpoint.Method.ToUpper()));
                if (endpoint.Headers==null&&!string.IsNullOrWhiteSpace(endpoint._Headers))
                {
                    endpoint.Headers = JsonConvert.DeserializeObject<IDictionary<string, object>>(endpoint._Headers);
                }

                if (endpoint.Headers != null && endpoint.Headers.Count > 0)
                {
                    endpoint.Headers.Keys.ToList().ForEach(x => {
                        request.AddHeader(x, endpoint.Headers[x].ToString());
                    });
                }

                
                IRestResponse response = null;
                if (endpoint.Method == "GET")
                {
                    request.Resource = requestbody.Trim().Replace('\n', ' ');
                    stopwatch.Start();
                    response = client.ExecuteAsGet(request, endpoint.Method.ToUpper());
                }
                else
                {
                    if (endpoint.IsParameterInArray && !requestbody.StartsWith("["))
                    {
                        requestbody = "[" + requestbody + "]";
                    }
                    //request.AddJsonBody(requestbody);
                    request.AddParameter("application/json; charset=utf-8", requestbody, ParameterType.RequestBody);
                    stopwatch.Start();
                    response = client.Execute(request);
                }
                
                if (!string.IsNullOrWhiteSpace(record.TenantId))
                {
                    var elapsedTime = stopwatch.ElapsedMilliseconds;
                    stopwatch.Stop();
                    using (ITrackoApiDbContext tdc = new TrackoApiDbContext(new TenantConnection { TenantId = record.TenantId }, _gs))
                    {
                        string tracknum = "";
                        if (response.IsSuccessful && (response.Content ?? "").Contains("1|"))
                        {
                            tracknum = response.Content?.Split('|')[1];
                        }
                        tdc.Database.ExecuteSqlCommand(
                            $"UPDATE [dbo].[tPickDroplog] SET [Ref1]=@ref1,[Ref2]=@ref2 WHERE [Id]=@id",
                            new SqlParameter("ref1", (string.IsNullOrWhiteSpace(tracknum) ? $"Url:{client.BaseUrl}\nResponseContent:{response.Content}\n{ response.ErrorMessage}\nStatusCode{response.StatusCode}\nException:{response.ErrorException?.StackTrace}" : "") + $". Took {elapsedTime} Millisecond"),new SqlParameter("ref2", tracknum),new SqlParameter("id", record.Id));
                        //var point = await tdc.VehicleMovementLogPickupDrops.FindAsync(record.Id);
                        //if (point != null)
                        //{
                        //    point.Ref1 = (response.IsSuccessful ? response.Content : response.ErrorMessage)+$". Took {elapsedTime} Millisecond";
                        //    if (response.IsSuccessful && (response.Content ?? "").Contains("1|"))
                        //    {
                        //        point.Ref2 = response?.Content?.Split('|')[1];
                        //    }
                        //    point.ObjectState = ObjectState.Modified;
                        //    await tdc.SaveChangesAsync();
                        //}
                    }
                }
                
                if ((response.IsSuccessful && response.StatusCode == System.Net.HttpStatusCode.OK /*200*/ &&
                     (response.Content ?? "").Contains('1')) ||
                    (!string.IsNullOrWhiteSpace(response.Content) && response.Content.Contains("0|Duplicate")))
                {

                    return;
                }
                if (count == 3&&!Helper.HostedOnPremise)
                {
                    using (var db = new TenantDbContext())
                    {
                        db.ApiLog.Add(new WebApiUsage()
                        {
                            IP = response.ResponseUri.ToString(),
                            RequestContent = JsonConvert.SerializeObject(request.Parameters.Select(parameter => new
                            {
                                name = parameter.Name,
                                value = parameter.Value,
                                type = parameter.Type.ToString()
                            })),
                            RequestHeaders = endpoint._Headers,
                            ResponseContent = response.Content,
                            RequestMethod = endpoint.Method,
                            ResponseTimestamp = DateTime.Now,
                            RequestTimestamp = DateTime.Now,
                            ResponseStatusCode = (int)response.StatusCode
                        });
                        db.SaveChanges();
                    }
                }
                if (count < 3)
                {
                    CallGpsVendor(endpoint, requestbody, count,record);
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
                                RequestHeaders = JsonConvert.SerializeObject(record),
                                Uri = requestbody
                            });
                            db.SaveChanges();
                        }

                    }
                }
                catch (Exception)
                {
                    //Ignore
                }
                
                if (count < 3)
                {
                    CallGpsVendor(endpoint, requestbody, count,record);
                }

                throw;
            }
        }

        private const string _tenantEventStorageClear = "DELETE FROM [dbo].[EventStorages] WHERE (IsProcessed=1 AND ProcessedTime<=DATEADD(DAY,-7,SYSDATETIMEOFFSET())) OR (IsProcessed=0 AND [EventReceivedTime]<=DATEADD(DAY,-10,SYSDATETIMEOFFSET()));";

        private const string _jobtracks = "DELETE FROM [dbo].[JobTracks] WHERE (IsProcessed=1 AND ProcessedTime<=DATEADD(DAY,-3,SYSDATETIMEOFFSET())) OR (IsProcessed=0 AND CreatedAt<=DATEADD(DAY,-20,SYSDATETIMEOFFSET()))";

        public void CleanOldJobLogs()
        {
            List<string> list = new List<string>();
            if (!Helper.HostedOnPremise)
            {
                using (var ctx = new TenantDbContext())
                {
                    ctx.Database.ExecuteSqlCommand(_jobtracks);
                    list = ctx.Tenants.Select(x => x.ConnectionString).ToList();
                }
            }
            else
            {
                list.Add(Helper.OnPremiseHostedConnectionString);
            }
            if (list.Any())
            {

                foreach (var item in list)
                {
                    try
                    {
                        using (var con = new SqlConnection(item))
                        {
                            con.Open();
                            using (var cmd = con.CreateCommand())
                            {
                                cmd.CommandTimeout =(int) TimeSpan.FromMinutes(3).TotalSeconds;
                                cmd.CommandText = _tenantEventStorageClear;
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Ignore
                    }
                }
            }
        }
        [Queue("fifo_event_processing"), DisableConcurrentExecution(60), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        public async void SyncAPLConfigInAPLAnnexureLevel(long _viewid, string tenantId,long _sessionid,int retry = 0, PerformContext pcontext = null)
        {
            if (retry > 5)
            {
                pcontext.WriteLine("Retry count reached at max level of 5. So terminating process");
                return;
            }
            var context = new TrackoApiDbContext(new TenantConnection { TenantId = tenantId }, _gs);
            try
            {
                await context.ExecuteProcedureAsync("Proc_TRANS_1898_CreateAPL", new SqlParameter("parameter1", _viewid), new SqlParameter("parameter2", _sessionid), new SqlParameter("parameter3", 0));
            }
            catch (Exception ex)
            {
                try
                {
                    if (!Helper.HostedOnPremise)
                        using (var db = new TenantDbContext())
                        {
                            db.ApiLog.Add(new WebApiUsage()
                            {
                                IP = _viewid.ToString(),
                                ResponseContent = ex.GetBaseException().Message + "\n" + ex.StackTrace,
                                RequestMethod = _sessionid.ToString(),
                                ResponseTimestamp = DateTime.Now,
                                RequestTimestamp = DateTime.Now,
                                RequestContent = "",
                                TenantKey = tenantId
                            });
                            db.SaveChanges();
                        }
                }
                catch (Exception)
                {
                    //Ignore
                }
                throw;
            }
            finally
            {
                if (context != null && !context.Disposed)
                {
                    context.Dispose();
                }
            }
        }


        [Queue("fifo_event_processing"), DisableConcurrentExecution(60), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        public void PushChildTrip(long tripId, string tenantId,int retry=0, PerformContext pcontext = null)
        {
            if (retry > 5)
            {
                pcontext.WriteLine("Retry count reached at max of 5 for this trip. So terminating process");
                return;
            }
            var context = new TrackoApiDbContext(new TenantConnection { TenantId = tenantId }, _gs);
            try
            {

                var onlyTrip = context.GetApiConfig<int>("PushOnlyTripOnGPS",1)==1;
                if (onlyTrip&& context.VehicleMovementLogs.Any(x => x.Id == tripId))
                {
                    pcontext.WriteLine("Entred in onlytrip if  statement");
                    if (context.VehicleMovementLogs.Any(x => x.Id == tripId && (x.TripTypeId == 1158 || (x.TripTypeId == 1160 && x.VehicleId != null))))
                    {
                        pcontext.WriteLine("Entred in 1158 trip condition if body");
                        PushToGPSProvider(tripId, tenantId, pcontext);
                    }
                    else
                    {
                        pcontext.WriteLine("Entred in non 1158 else body and rescheduling gps push after 15 minutes");
                        _ = Hangfire.BackgroundJob.Schedule(() => PushChildTrip(tripId, tenantId,retry,null), TimeSpan.FromMinutes(10));
                    }
                }
                else
                {
                    pcontext.WriteLine("Entred in non onlytrip else  statement");
                    PushToGPSProvider(tripId, tenantId, pcontext);
                }
            }
            finally
            {
                if (context != null && !context.Disposed)
                {
                    context.Dispose();
                }
            }
        }
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        public void PushToGPSProvider(long tripId, string tenantId, PerformContext context = null)
        {
            try
            {


                if (tripId == 0)
                {
                    context.WriteLine($"TripLogId was {tripId}. Terminating Process.");
                    return;
                }
                using (TrackoApiDbContext tdc = new TrackoApiDbContext(new TenantConnection { TenantId = tenantId }, _gs))
                {

                    var log = tdc.VehicleMovementLogs.Where(x => x.Id == tripId && x.UnloadingDate == null).Select(x => new
                    {
                        x.Id,
                        x.VehicleId,
                        x.HireVehicleId,
                        x.TripStartDate,
                        x.ScheduledPlacementDate,
                        x.ScheduledDepartureDate,
                        x.TripNatureId,
                        x.LoadingReachDate,
                        x.LoadingDate,
                        x.TriplogNo,
                        x.Remarks,
                        x.ExpTime,
                        x.ExpectedDeliveryDate,
                        x.TotalKmRun,
                        x.LoadingQty,
                        Consignor = x.fk_Party != null ? x.fk_Party.AccountName : null,
                        ConsignoreAddress = x.fk_Party.fk_Address != null ? x.fk_Party.fk_Address.FullAddress : null,
                        Consignee = x.fk_Consignee == null ? null : x.fk_Consignee.AccountName,
                        ConsigneeAddress = x.fk_Consignee.fk_Address != null ? x.fk_Consignee.fk_Address.FullAddress : null,
                        DriverInfo = x.fk_DriverI == null ? null :new{x.fk_DriverI.DriverName,x.fk_DriverI.DriverCode,x.fk_DriverI.DriverContactNo1},
                        x.DriverPhone,
                        x.RouteId,
                        RouteName= x.RouteId!=null?x.fk_Route.Name:null,
                        x.FromPlaceId
                    }).FirstOrDefault();
                    if (log == null)
                    {
                        context.WriteLine("Triplog not or trip has already unloaded found so terminating process at line number 928");
                        return;
                    }

                    var nextTrip = tdc.VehicleMovementLogs
                        .Where(x => x.VehicleId == log.VehicleId && x.HireVehicleId == log.HireVehicleId &&
                                    x.TripStartDate > log.TripStartDate && x.Id != log.Id).Select(x => new { x.Id }).FirstOrDefault();
                    if (nextTrip != null)
                    {
                        context.WriteLine("Next Trip found  so terminating process at line number 991");
                        return;
                    }


                    var vehicleInfo = (log.HireVehicleId > 0 ? tdc.HireVehicles.Where(x => x.Id == log.HireVehicleId && x.GPSVendorId > 0).Select(x => new { x.GPSVendorId, x.VehicleNo, x.RegistrationNo }).FromCacheFirstOrDefault(CachePolicy.WithDurationExpiration(TimeSpan.FromDays(8))) : tdc.VehicleMasters.Where(x => x.Id == log.VehicleId && x.GPSVendorId > 0).Select(x => new { x.GPSVendorId, x.VehicleNo, RegistrationNo = x.VehicleRegNo }).FromCacheFirstOrDefault(CachePolicy.WithDurationExpiration(TimeSpan.FromDays(8))));
                    if (vehicleInfo == null || vehicleInfo.GPSVendorId.GetValueOrDefault() == 0) return;

                    var endpoint = tdc.IntegrationEndPoints.Where(x => x.VendorId == vehicleInfo.GPSVendorId && x.ServiceTypeId == 1595).FromCacheFirstOrDefault(CachePolicy.WithDurationExpiration(TimeSpan.FromDays(8)));
                    if (endpoint == null)
                    {
                        context.WriteLine("Integration EndPoint Entry not found with service typeid 1595, so terminating process at line number 991");
                        return;
                    }
                    var parameters = (endpoint.ParameterMapping ?? "").Split('^');
                    if(!tdc.VehicleMovementLogPickupDrops.Any(x => x.TriplogId == tripId))
                    {
                        try
                        {
                            var wpRepo = tdc.RouteWayPoints;
                            var waypointlist = wpRepo.Where(x => x.RouteId == log.RouteId).Select(x =>
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
                                }).ToList();
                            context.WriteLine($"Found {waypointlist.Count} waypoints for insreting into tPickDropLog");
                            var wps = waypointlist.Select(x => new VehicleMovementLogPickupDrop
                            {
                                RouteId = x.RouteId,
                                CityId = x.CityId,
                                GeographyPoint = x.GeographyPoint,
                                KM = (int)x.KM,
                                Latitude = (decimal)x.Latitude,
                                Longitude = (decimal)x.Longitude,
                                Order = x.Order,
                                OriginLocationId = log.FromPlaceId ?? waypointlist.OrderBy(y => y.Order).FirstOrDefault()?.CityId ?? 0,
                                StopageTime = 0,
                                TravalTime = x.TravalTime,
                                TriplogId = log.Id,
                                TypeId = x.TypeId.GetValueOrDefault(),
                                ObjectState = ObjectState.Added
                            });
                            tdc.VehicleMovementLogPickupDrops.AddRange(wps);
                            tdc.SaveChanges();
                            //await uow.SaveChangesAsync();
                        }
                        catch(Exception e)
                        {
                            context.WriteLine($"Occured error while inserting waypoint for triplog");
                            context.WriteLine(e);
                            //return BadRequest("Unable to Created RouteWay Points from server side when trip posted from Mobile app using View Id 5001");
                        }
                    }
                    var waypoints = tdc.VehicleMovementLogPickupDrops.Where(x => x.TriplogId == tripId&&(x.Ref2==null || x.Ref2.Trim()=="")&&x.CityId!=x.OriginLocationId).Select(point => new GPSTripUploadViewModel
                    {
                        Id = point.Id,
                        PointKM = point.KM,
                        Order = point.Order,
                        StopageTime = point.StopageTime,
                        TravalTime = point.TravalTime,
                        ToCity = point.fk_City == null ? null : point.fk_City.CityName,
                        ToCityStateName = point.fk_City.fk_State == null ? null : point.fk_City.fk_State.Name,
                        PostalCode = point.fk_City == null ? null : point.fk_City.PostalCode,
                        FromCity = point.fk_OriginLocation == null ? null : point.fk_OriginLocation.CityName
                    }).ToList();
                    var doNotPostIt = tdc.GetApiConfig<int>("PostPassThroughWayPointOnGPS") == 0;
                    
                    context.WriteLine($"Found {waypoints.Count} waypoints to be sent on GPS Provider");
                    var cnlist = (tdc.CNChallans.Where(x => x.TriplogId == log.Id && x.CNId > 0).Select(x => x.fk_CNMaster.CNNo).ToList());
                    context.WriteLine($"Found {cnlist.Count} CNs to be sent on GPS Provider");
                    var list = new List<string>();
                    foreach (var point in waypoints)
                    {
                        try
                        {
                            if (point.ToCity == point.FromCity || (doNotPostIt&& point.TypeId == 1616)) continue;

                            var totalkm = waypoints.Where(x => x.Order <= point.Order).Sum(x => x.KM);
                            var record = new GPSTripUploadViewModel
                            {
                                TripStartDate = log.TripStartDate.AddMinutes(point.Order == 1 ? 0 : point.Order),
                                ScheduledPlacementDate = log.ScheduledPlacementDate,
                                ScheduledDepartureDate = log.ScheduledDepartureDate,
                                LoadingReportDate = log.TripNatureId == 1076 /*Empty*/
                                    ? log.TripStartDate
                                    : (log.LoadingReachDate ?? log.TripStartDate),
                                LoadingDate = log.TripNatureId == 1076 /*Empty*/
                                    ? log.TripStartDate
                                    : (log.LoadingDate ?? log.TripStartDate),
                                TripNo = log.TriplogNo,
                                Remark = log.Remarks,
                                ETAHour = log.ExpTime,
                                ETA = log.ExpectedDeliveryDate,
                                KM = totalkm > 0 ? totalkm : log.TotalKmRun,
                                Qty = log.LoadingQty,
                                TripId = log.Id,
                                VehicleNo = vehicleInfo.VehicleNo,
                                Id = point.Id,
                                TenantId = tenantId,
                                RegistrationNo = vehicleInfo.RegistrationNo,
                                Order = point.Order,
                                PointKM = new decimal(point.KM),
                                StopageTime = point.StopageTime,
                                TravalTime = point.TravalTime,
                                Consignee = log.Consignee,
                                ConsigneeAddress = log.ConsigneeAddress,
                                Consignor = log.Consignor,
                                ConsignoreAddress = log.ConsignoreAddress,
                                DriverName = $"{log.DriverInfo?.DriverName}[{log.DriverInfo?.DriverCode}]",
                                DriverMobile = string.IsNullOrWhiteSpace(log.DriverPhone)?log.DriverInfo?.DriverContactNo1:log.DriverPhone,
                                TripNature = log.TripNatureId == 1076 ? "Empty" : log.TripNatureId == 1645 ? "Empty -> Loaded" : log.TripNatureId == 1075 ? "Loaded" : log.TripNatureId == 1646 ? "Loaded -> Empty" : log.TripNatureId == 1647 ? "Loaded -> Loaded" : log.TripNatureId == 1520 ? "ORM" : "None",
                                FromCity=point.FromCity,
                                PostalCode=point.PostalCode,
                                ToCity=point.ToCity,
                                ToCityStateName=point.ToCityStateName,
                                TypeId=point.TypeId,
                                RouteName=log.RouteName
                            };
                            if (cnlist != null && cnlist.Any())
                            {
                                record.CNNos = cnlist.JoinStrings(",");
                            }
                            var requestbody = $"{endpoint.ParameterTemplate}";
                            foreach (var pr in parameters)
                            {
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
                            if (endpoint.IsParameterInArray)
                            {
                                list.Add(requestbody);
                            }
                            else
                            {
                                try
                                {
                                    var stopwatch = new Stopwatch();
                                    stopwatch.Start();
                                    var response = SendGpsRequest(requestbody, endpoint,context);
                                    var elapsedTime = stopwatch.ElapsedMilliseconds;
                                    stopwatch.Stop();
                                    string tracknum = "";
                                    if (response.IsSuccessful && (response.Content ?? "").Contains("1|"))
                                    {
                                        if (!string.IsNullOrWhiteSpace(endpoint.ResultJsonPath)&&!string.IsNullOrWhiteSpace(response.Content))
                                        {
                                            var jt = JToken.Parse(response.Content);
                                            JToken acme = jt.SelectToken(endpoint.ResultJsonPath);
                                            tracknum = acme.ToString();
                                        }
                                        else
                                        {
                                            tracknum = response.Content?.Split('|')[1];
                                        }
                                    }                                    
                                    tdc.Database.ExecuteSqlCommand(
                                        $"UPDATE [dbo].[tPickDroplog] SET [Ref1]=@ref1,[Ref2]=@ref2 WHERE [Id]=@id",
                                        new SqlParameter("ref1", (string.IsNullOrWhiteSpace(tracknum) ? $"Url:{response.Request.Resource}\nResponseContent:{response.Content}\n{ response.ErrorMessage}\nStatusCode{response.StatusCode}\nException:{response.ErrorException?.StackTrace}" : "") + $". Took {elapsedTime} Millisecond"), new SqlParameter("ref2", tracknum), new SqlParameter("id", point.Id));
                                }
                                catch (Exception e)
                                {
                                    try
                                    {
                                        if (!Helper.HostedOnPremise)
                                            using (var db = new TenantDbContext())
                                            {
                                                db.ApiLog.Add(new WebApiUsage()
                                                {
                                                    IP = endpoint.Url,
                                                    ResponseContent = e.GetBaseException().Message + "\n" + e.StackTrace,
                                                    RequestMethod = endpoint.Method,
                                                    ResponseTimestamp = DateTime.Now,
                                                    RequestTimestamp = DateTime.Now,
                                                    RequestContent = JsonConvert.SerializeObject(new
                                                    {
                                                        RequestBody = requestbody,
                                                        EndPoint = endpoint
                                                    })
                                                });
                                                db.SaveChanges();
                                            }
                                    }
                                    catch (Exception)
                                    {
                                        //Ignore
                                    }
                                }
                            }
                        }
                        catch//
                        {
                            //Ignore
                        }
                    }

                    if (list.Any() && endpoint.IsParameterInArray)
                    {
                        var body = $"[{list.JoinStrings(",")}]";
                        try
                        {
                            var stopwatch = new Stopwatch();
                            stopwatch.Start();
                            var response = SendGpsRequest(body, endpoint,context);
                            var elapsedTime = stopwatch.ElapsedMilliseconds;
                            stopwatch.Stop();
                            string tracknum = "";
                            if (response.IsSuccessful && (response.Content ?? "").Contains("1|"))
                            {
                                if (!string.IsNullOrWhiteSpace(endpoint.ResultJsonPath)&&!string.IsNullOrWhiteSpace(response.Content))
                                {
                                    var jt = JToken.Parse(response.Content);
                                    JToken acme = jt.SelectToken(endpoint.ResultJsonPath);
                                    tracknum = acme.ToString();
                                }
                                else
                                {
                                    tracknum = response.Content?.Split('|')[1];
                                }
                                
                            }
                            tdc.Database.ExecuteSqlCommand(
                                $"UPDATE [dbo].[tPickDroplog] SET [Ref1]=@ref1,[Ref2]=@ref2 WHERE [TripLogId]=@id",
                                new SqlParameter("ref1", (string.IsNullOrWhiteSpace(tracknum) ? $"Url:{response.Request.Resource}\nResponseContent:{response.Content}\n{ response.ErrorMessage}\nStatusCode{response.StatusCode}\nException:{response.ErrorException?.StackTrace}" : "") + $". Took {elapsedTime} Millisecond"), new SqlParameter("ref2", tracknum), new SqlParameter("id", log.Id));
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                if (!Helper.HostedOnPremise)
                                    using (var db = new TenantDbContext())
                                    {
                                        db.ApiLog.Add(new WebApiUsage()
                                        {
                                            IP = endpoint.Url,
                                            ResponseContent = e.GetBaseException().Message + "\n" + e.StackTrace,
                                            RequestMethod = endpoint.Method,
                                            ResponseTimestamp = DateTime.Now,
                                            RequestTimestamp = DateTime.Now,
                                            RequestContent = JsonConvert.SerializeObject(new
                                            {
                                                RequestBody = body,
                                                EndPoint = endpoint
                                            })
                                        });
                                        db.SaveChanges();
                                    }
                            }
                            catch (Exception)
                            {
                                //Ignore
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                //context.WriteLine(ex);
                
                throw ex;
            }
        }
        private void LogRequest(IRestClient _restClient,IRestRequest request, IRestResponse response, long durationMs,PerformContext context = null)
        {
            if(response.IsSuccessful)return;
            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = _restClient.BuildUri(request),
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };
            context.WriteLine(ConsoleTextColor.Red,$"Request completed in {durationMs} ms, Request: {JsonConvert.SerializeObject(requestToLog)}, Response: {JsonConvert.SerializeObject(responseToLog)}");
            Trace.TraceError(string.Format("Request completed in {0} ms, Request: {1}, Response: {2}",
                durationMs,
                JsonConvert.SerializeObject(requestToLog),
                JsonConvert.SerializeObject(responseToLog)));
        }
        private dynamic LogRequest(IRestClient _restClient, IRestRequest request, IRestResponse response, long durationMs)
        {
            var requestToLog = new
            {
                resource = request.Resource,
                // Parameters are custom anonymous objects in order to have the parameter type as a nice string
                // otherwise it will just show the enum value
                parameters = request.Parameters.Select(parameter => new
                {
                    name = parameter.Name,
                    value = parameter.Value,
                    type = parameter.Type.ToString()
                }),
                // ToString() here to have the method as a nice string otherwise it will just show the enum value
                method = request.Method.ToString(),
                // This will generate the actual Uri used in the request
                uri = _restClient.BuildUri(request)
            };

            var responseToLog = new
            {
                statusCode = response.StatusCode,
                content = response.Content,
                headers = response.Headers,
                // The Uri that actually responded (could be different from the requestUri if a redirection occurred)
                responseUri = response.ResponseUri,
                errorMessage = response.ErrorMessage,
            };

            return new
            {
                requestToLog,
                responseToLog
            };
        }
        private IRestResponse SendGpsRequest(string requestbody, GpsEndPoint endpoint, PerformContext context=null)
        {
            
            try
            {
                context?.WriteLine(ConsoleTextColor.Cyan, $"Processing GPS Request under SendGpsRequest method URL:{endpoint.Url} of type {endpoint.Method.ToUpper()} with Authorization Header as { endpoint.Authorization}");
                var client = new RestSharp.RestClient(endpoint.Url);
                var request = new RestSharp.RestRequest((RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), endpoint.Method.ToUpper()));
                if (!string.IsNullOrWhiteSpace(endpoint.Authorization))
                {
                    request.AddHeader("Authorization", endpoint.Authorization);
                }
                context.WriteLine($"Sending Request with Headers:{endpoint._Headers}");
                if (endpoint.Headers==null&&!string.IsNullOrWhiteSpace(endpoint._Headers))
                {
                    endpoint.Headers = JsonConvert.DeserializeObject<IDictionary<string, object>>(endpoint._Headers);
                }

                if (endpoint.Headers != null && endpoint.Headers.Count > 0)
                {
                    endpoint.Headers.Keys.ToList().ForEach(x => {
                        request.AddHeader(x, endpoint.Headers[x].ToString());
                    });
                }
                if (endpoint.Method == "GET")
                {
                    if (!string.IsNullOrWhiteSpace(requestbody))
                    {
                        request.Resource = requestbody.Trim().Replace('\n', ' ');
                    }
                    var getresponse= client.ExecuteAsGet(request, endpoint.Method.ToUpper());                    
                    context?.WriteLine(getresponse.IsSuccessful ? ConsoleTextColor.Cyan : ConsoleTextColor.Red, $"GPS Request Processed with StatusCode {getresponse.StatusCode} and was {(getresponse.IsSuccessful?"sucessfull":$"unsuccessful with response {(!string.IsNullOrWhiteSpace(getresponse.Content)? getresponse.Content: getresponse.ErrorMessage?? getresponse.ErrorException?.GetBaseException().Message??"NA")}")}");
                    return getresponse;
                }
                if (endpoint.IsParameterInArray && !requestbody.StartsWith("["))
                {
                    requestbody = "[" + requestbody + "]";
                }
                if (!string.IsNullOrWhiteSpace(requestbody))
                {
                    request.AddParameter("application/json; charset=utf-8", requestbody, ParameterType.RequestBody);
                }
                var postResponse = client.Execute(request);
                LogRequest(client,request,postResponse,0);
                context?.WriteLine(postResponse.IsSuccessful? ConsoleTextColor.Cyan: ConsoleTextColor.Red, $"GPS Request Processed with StatusCode {postResponse.StatusCode} and was {(postResponse.IsSuccessful ? "sucessfull" : $"unsucessfull with response {(!string.IsNullOrWhiteSpace(postResponse.Content) ? postResponse.Content : postResponse.ErrorMessage ?? postResponse.ErrorException?.GetBaseException().Message ?? "NA")}")}");
                return postResponse;
            }
            catch (Exception ex)
            {
                try
                {
                    if (!Helper.HostedOnPremise)
                        using (var db = new TenantDbContext())
                    {
                        db.ApiLog.Add(new WebApiUsage()
                        {
                            IP = endpoint.Url,
                            ResponseContent = ex.GetBaseException().Message + "\n" + ex.StackTrace,
                            RequestMethod = endpoint.Method,
                            ResponseTimestamp = DateTime.Now,
                            RequestTimestamp = DateTime.Now,
                            RequestContent = JsonConvert.SerializeObject(new
                            {
                                RequestBody= requestbody,
                                EndPoint= endpoint
                            })
                        });
                        db.SaveChanges();
                    }
                }
                catch (Exception)
                {
                    //Ignore
                }
                throw;
            }
        }

        private const string RecentTripQuery = "SELECT * FROM [dbo].[GetLatestTripForEachVehicle](@p0)";
        

        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        public void SyncGPSStatusLog(PerformContext context)
        {
            
            var tenants = new Dictionary<string, string>();
            if (!Helper.HostedOnPremise)
            {
                using (var ctx = new TenantDbContext())
                {
                    tenants = ctx.Tenants.Where(x => x.IsActive&&!x.IsHostedOnPremise).Select(x => new { x.Id, x.ConnectionString }).ToDictionary(x => x.Id, x => x.ConnectionString);
                }
            }
            else
            {
                tenants = _gs.Tenants.Select(x => new { x.Value.Id, x.Value.ConnectionString}).ToDictionary(x => x.Id, x => x.ConnectionString);
            }

            if (!tenants.Any())
            {
                context.WriteLine("No Tenants found in Global Store");
            }
            //int time = -1;
            
            foreach (var tenant in tenants)
            {
                int timegap = 2;
                context.WriteLine($"Scheduling GPS Sync for {tenant.Key}");
                var conn = tenant.Value/*.Replace("Source=.;", "Source=africa.indiaweblab.com;")*/;
                BackgroundJob.Schedule(() => this.ScheduleGPSSync(null, conn, tenant.Key), TimeSpan.FromSeconds(timegap));
                //BackgroundJob.Enqueue(() => this.ScheduleGPSSync(null,conn, tenant.Key));
                continue;                

            }
        }

        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        public void ScheduleGPSSync(PerformContext context,string conn,string tenantId)
        {
            /*.Replace("Source=.;", "Source=africa.indiaweblab.com;")*/
            try
            {

            
            using (var db = new TrackoApiDbContext(_gs,conn))
            {
                var resultSet = new List<GPSStatusLog>();
                var endpoints =
                    db.IntegrationEndPoints.Where(x => x.ServiceTypeId == 1585).ToList();
                if (!resultSet.Any())
                {
                    context.WriteLine(ConsoleTextColor.Yellow, $"GPS Sync Integration not found for Tenant {db.Database.Connection.Database}");
                }
                else
                {
                    context.WriteLine($"GPS Sync Integration found for Tenant {db.Database.Connection.Database}");
                }
                foreach (var endpoint in endpoints)
                {
                    
                    try
                    {
                        context.WriteLine(ConsoleTextColor.DarkCyan, $"Running Database Query {RecentTripQuery} with GPSVendorId {endpoint.VendorId}");
                        var trips = db.Database.SqlQuery<RecentTripViewModel>(RecentTripQuery, (object)endpoint.VendorId ?? DBNull.Value).ToList();
                        if (!trips.Any())
                        {
                            context.WriteLine(ConsoleTextColor.Yellow, $"Database Query Result was empty");
                            continue;
                        }
                        context.WriteLine(ConsoleTextColor.Cyan, $"Database Query returned {trips.Count} records");
                        var resmapp = (endpoint.ResultMapping ?? "").Split('^');
                        if (string.IsNullOrWhiteSpace(endpoint.ParameterMapping))//No Parameters Needed
                        {
                            context.WriteLine(ConsoleTextColor.Cyan, $"Processing GPS Request without parameter");
                            var response = SendGpsRequest("", endpoint, context);
                            var result = response?.Content;
                            if (!string.IsNullOrWhiteSpace(result) && !result.Contains("Error") && response.IsSuccessful)
                            {
                                if (resmapp.Length > 0)
                                {
                                    resmapp.ForEach(pr =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(pr))
                                        {
                                            var r = pr.Split('=');
                                            result = result.Replace(r[0], r[1]);
                                        };
                                    });
                                }

                                var data = ParseGPSResult(context,result, endpoint, trips);
                                context.WriteLine($"Result Parsing completed");
                                resultSet.AddRange(data);

                            }
                        }
                        else
                        {
                            //.Select((x, index) => new { HireVehicleNo=x.fk_HireVehicle.VehicleNo,OwnVehicleNo=x.fk_Vehicle.VehicleRegNo,RowId=index+1}).ToList();
                            var parameters = (endpoint.ParameterMapping ?? "").Split('^');
                            if (endpoint.IsParameterInArray)
                            {
                                var requestbody = string.Empty;
                                if (!string.IsNullOrWhiteSpace(endpoint.ParameterTemplate))
                                {
                                    requestbody = "[";
                                    foreach (var p in trips)
                                    {
                                        var tt = endpoint.ParameterTemplate;
                                        if (parameters.Length > 0)
                                        {
                                            foreach (var pr in parameters)
                                            {
                                                var val = p.GetPropertyValue(pr.Replace("_", ""))?.ToString() ?? "";
                                                tt = tt.Replace(pr, val);
                                            }
                                        }
                                        if (p.Equals(trips.Last()))
                                        {
                                            requestbody += tt;
                                        }
                                        else
                                        {
                                            requestbody += tt + ",";
                                        }
                                    }
                                    requestbody += "]";
                                }

                                var response = SendGpsRequest(requestbody, endpoint, context);
                                var result = response?.Content;
                                if (string.IsNullOrWhiteSpace(result) || result.Contains("Error") || !response.IsSuccessful)
                                {
                                    continue;
                                }
                                if (resmapp.Length > 0)
                                {
                                    resmapp.ForEach(pr =>
                                    {
                                        if (!string.IsNullOrWhiteSpace(pr))
                                        {
                                            var r = pr.Split('=');
                                            result = result.Replace(r[0], r[1]);
                                        }

                                    });
                                }
                                var data = ParseGPSResult(context,result, endpoint, trips);
                                resultSet.AddRange(data);
                            }
                            else
                            {
                                if (string.IsNullOrWhiteSpace(endpoint.ParameterTemplate)) continue;
                                foreach (var p in trips)
                                {
                                    var requestbody = string.Empty;
                                    var tt = endpoint.ParameterTemplate;
                                    if (parameters.Length > 0)
                                    {
                                        foreach (var pr in parameters)
                                        {
                                            var val = p.GetPropertyValue(pr.Replace("_", ""))?.ToString() ?? "";
                                            tt = tt.Replace(pr, val);
                                        }
                                    }

                                    var response = SendGpsRequest(requestbody, endpoint, context);
                                    var result = response?.Content;
                                    if (string.IsNullOrWhiteSpace(result) || result.Contains("Error") || !response.IsSuccessful)
                                    {
                                        continue;
                                    }
                                    if (resmapp.Length > 0)
                                    {
                                        resmapp?.ForEach(pr =>
                                        {
                                            if (!string.IsNullOrWhiteSpace(pr))
                                            {
                                                var r = pr.Split('=');
                                                result = result.Replace(r[0], r[1]);
                                            }

                                        });
                                    }

                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                }

                if (!resultSet.Any())
                {
                    context.WriteLine(ConsoleTextColor.Yellow, $"Parsed ResultSet was empty");
                    return;
                }
                try
                {
                    context.WriteLine($"Executing bulk insert of Parsed ResultSet. Records are {resultSet.Count}");
                    db.BulkInsertNew(resultSet, new BulkInsertOptions
                    {
                        EnableStreaming = true,
                        BatchSize = 5000,
                        TimeOut = 180,
                        SqlBulkCopyOptions=SqlBulkCopyOptions.FireTriggers
                    }, conn);

                }
                catch (Exception e)
                {
                    throw;
                }

            }
            }
            catch (Exception ex)
            {
                context.WriteLine($"Unable to Sync GPS Status \n{ex.GetBaseException().Message}");
            }
        }
        
        private List<GPSStatusLog> ParseGPSResult(PerformContext context, string result,GpsEndPoint endpoint,List<RecentTripViewModel> trips)
        {
            try
            {
                context.WriteLine(ConsoleTextColor.Cyan, $"Parsing GPS Result set {result.Truncate(200)}");
                var setting = new JsonSerializerSettings
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    NullValueHandling = NullValueHandling.Ignore,
                    FloatParseHandling = FloatParseHandling.Decimal
                };
                if (!string.IsNullOrWhiteSpace(endpoint.DateFormat))
                {
                    setting.DateFormatString = endpoint.DateFormat;
                }
                context.WriteLine(ConsoleTextColor.Cyan, $"Deserializing Resultset.");
                if (!string.IsNullOrWhiteSpace(endpoint.ResultJsonPath)&&!string.IsNullOrWhiteSpace(result))
                {
                    var jt = JToken.Parse(result);
                    JToken acme = jt.SelectToken(endpoint.ResultJsonPath);
                    result = acme.ToString();
                }
                var res = JsonConvert.DeserializeObject<List<GPSTrackingResult>>(result, setting);                
                res?.RemoveAll(x => x == null);
                context.WriteLine(ConsoleTextColor.Cyan, $"Deserializing Resultset completed and result set contains {res.Count} records and requested records were {trips.Count}.");
                if (res.Count != trips.Count)
                {
                    var resultedvehicles = res.Select(x => x.VehicleNo).Distinct().ToList();
                    var requestedvehicles = trips.Select(x => x.VehicleNo).Distinct().ToList();
                    var diff = requestedvehicles.Where(x => !resultedvehicles.Contains(x)).ToList();
                    if (diff.Any())
                    {
                        context.WriteLine(ConsoleTextColor.Cyan, $"Resultset were missing these vehicles {diff.JoinStrings(",")}");
                    }
                }
                if (!res.Any()) return new List<GPSStatusLog>();
                return (from v in res
                            join l in trips
                                on v.VehicleNo equals l.VehicleNo
                        //let km = GPSStatusLog.DistanceBetween(l.Longitude, l.Latitude, v.Longitude, v.Latitude)
                        //where (Math.Abs(v.Latitude - l.Latitude) > 0) || (Math.Abs(v.Longitude - l.Longitude) > 0)
                        select new GPSStatusLog
                            {
                                BudgetedKM = (long)v.budgetedkm,
                                CDOE = DateTime.Now,
                                VehicleNo = l.VehicleNo,
                                GPSLocation = v.Location,
                                GPSTime = v.StatusDate ?? DateTime.Now,
                                IgnitionStatus = v.Ignition,
                                Latitude = v.Latitude,
                                Longitude = v.Longitude,
                                RemainingKM = (long)v.remainingkm,
                                Speed = (int)v.Speed,
                                TravelledKM = (long)(v.KMRun > 0 ? v.KMRun : v.totaltravelledkm),
                                //VTSId = l.VTSId,
                                VehicleId = l.VehicleId == 0 ? null : l.VehicleId,
                                HireVehicleId=l.HireVehicleId==0?null: l.HireVehicleId,
                                //TripLogId = l.TripId,
                                ODOMeter = v.ODOMeter, 
                                TripLogNo=v.TripNo,
                                KM=l.KM,
                                GofKM = GPSStatusLog.CalculateDiff(l.Latitude, l.Longitude, v.Latitude, v.Longitude, l.ErrorMargin, l.MaxDiffKM),
                                GPSVendorId = l.GPSVendorId ?? endpoint.VendorId,
                            Data1 = v.Data1,
                            Data2 = v.Data2,
                            Data3 = v.Data3,
                            Data4 = v.Data4
                            // KM =Math.Round((double.IsNaN(l.KM)?0:l.KM)+ (double.IsNaN(km) ? 0 : km),5),

                            //GeographyPoint =
                            //    v.Latitude > 0 || v.Longitude > 0
                            //        ? DbGeography.FromText($"POINT({v.Latitude} {v.Longitude})")
                            //: null
                        }).ToList();
            }
            catch (Exception e)
            {
                LogError(new WebApiUsage()
                {
                    IP = endpoint.Url,
                    ResponseContent = result,
                    RequestMethod = endpoint.Method,
                    ResponseTimestamp = DateTime.Now,
                    RequestTimestamp = DateTime.Now,
                    RequestContent = JsonConvert.SerializeObject(new
                    {
                        RequestBody = "",
                        EndPoint = endpoint
                    })
                });
                //Console.WriteLine(e);
                throw;
            }
        }
        public void ReRunAllCustomSchedule(PerformContext context)
        {
            try
            {
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Helper.CountryTimeZone);
                var tenants = new Dictionary<string, string>();
                if (!Helper.HostedOnPremise)
                {
                    using (var ctx = new TenantDbContext())
                    {
                        tenants = ctx.Tenants.Where(x => x.IsActive && !x.IsHostedOnPremise).Select(x => new { x.Id, x.ConnectionString }).ToDictionary(x => x.Id, x => x.ConnectionString);
                    }
                }
                else
                {
                    tenants = _gs.Tenants.Select(x => new { x.Value.Id, x.Value.ConnectionString }).ToDictionary(x => x.Id, x => x.ConnectionString);
                }

                if (!tenants.Any())
                {
                    context.WriteLine("No Tenants found in Global Store");
                }
                foreach (var tenant in tenants)
                {
                    try
                    {
                        using (var db = new TrackoApiDbContext(_gs, tenant.Value))
                        {
                            var schedules = db.ScheduleLogs.Select(x => new { x.HangfireId, x.CronText, x.Status, x.Id }).ToList();
                            foreach (var schedule in schedules)
                            {
                                if (schedule.Status == TrackoAPI.Models.Shared.MasterStatus.Active)
                                {
                                    RecurringJob.AddOrUpdate<IHangfireJobProcessor>(schedule.HangfireId, x => x.RunBusinessSchedule(null, schedule.Id, tenant.Key), schedule.CronText, timeZone: timeZoneInfo, queue: "business_queue");
                                }
                                else
                                {
                                    RecurringJob.RemoveIfExists(schedule.HangfireId);
                                }
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        context.WriteLine(color: ConsoleTextColor.Red, $"Error while connecting db {tenant.Value} {ex.GetBaseException().Message}");
                    }
                }
            }catch(Exception ex)
            {
                //context.WriteLine(color: ConsoleTextColor.Red, ex);
            }
        }
        public void LogError(WebApiUsage usage)
        {
            try
            {
                if (Helper.HostedOnPremise) return;
                using (var db = new TenantDbContext())
                {
                    db.ApiLog.Add(usage);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                //Ignore
            }
        }
        [Queue("fifo_event_automation"), DisableConcurrentExecution(60)]
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        public void RunFuelAutomation(long triplogId, string tenantId)
        {
            this.RunFuelAutomation(triplogId, 0, tenantId);
        }
        [Queue("fifo_event_automation"), DisableConcurrentExecution(60)]
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        public void RunFuelAutomationByVehicle(long vehicleid, string tenantId)
        {
            this.RunFuelAutomationByVehicle(vehicleid, 0, tenantId);
        }
        [Queue("fifo_event_automation"), DisableConcurrentExecution(60)]
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        public void RunFuelAutomationByVehicle(long vehicleid, long sessionId, string tenantId)
        {
            try
            {
                if (vehicleid == 0) return;
                using (TrackoApiDbContext _db = new TrackoApiDbContext(new TenantConnection { TenantId = tenantId }, _gs))
                {
                    var config = _db.GetApiConfig<int>("RunFuelAutomationProcess");
                    if (config == 0) return;
                    _db.ExecuteProcedure("[dbo].[Proc_TRANS_FuelAutomationHandle]",
                        new[] { new SqlParameter("VehicleId", vehicleid) });
                }
            }
            catch (Exception ex)
            {
                //PerformContext context = null;
                //context.WriteLine(ConsoleTextColor.Red, ex);
            }
        }
        [Queue("fifo_event_automation"), DisableConcurrentExecution(60)]
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        public void RunFuelAutomation(long triplogId, long sessionId, string tenantId)
        {
            try
            {
                if (triplogId == 0) return;
                using (TrackoApiDbContext _db = new TrackoApiDbContext(new TenantConnection { TenantId = tenantId }, _gs))
                {
                    var config = _db.GetApiConfig<int>("RunFuelAutomationProcess");
                    if (config == 0) return;
                    _db.ExecuteProcedure("[dbo].[Proc_TRANS_FuelAutomationHandle]",
                        new[] { new SqlParameter("TriplogId", triplogId) });
                }
            }
            catch (Exception ex)
            {
                //PerformContext context = null;
                //context.WriteLine(ConsoleTextColor.Red, ex);
            }
        }
        [Queue("fifo_post_transaction"), DisableConcurrentExecution(60)]
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        public void RunTripPostProcess(PerformContext context, long triplogId, long sessionId, string tenantId)
        {
            try
            {
                context.WriteLine(ConsoleTextColor.Cyan, $"Trip Post process started for TripId {triplogId} for tenant {tenantId}");
                if (triplogId == 0) return;
                using (TrackoApiDbContext _db = new TrackoApiDbContext(new TenantConnection { TenantId = tenantId }, _gs))
                {
                    var config = _db.GetApiConfig<int>("RunTripPostProcess");
                    context.WriteLine(ConsoleTextColor.Cyan, $"RunTripPostProcess:{config}");
                    if (config == 0) return;
                    _db.ExecuteProcedure("[dbo].[Proc_TRANS_RunPostTripProcess]",
                        new[] { new SqlParameter("triplogid", triplogId), new SqlParameter("sessionid", sessionId) });
                }
            }
            catch (Exception ex)
            {
                //context.WriteLine(ConsoleTextColor.Red, ex);
            }
        }

        //[Queue("fifo_post_transaction"), DisableConcurrentExecution(60)]
        //[AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        //public void RunTripPostProcess(PerformContext context, long triplogId, long sessionId, string tenantId)
        //{
        //    try
        //    {
        //        context.WriteLine(ConsoleTextColor.Cyan, $"Trip Post process has been queuened further in sub queue for TripId {triplogId} for tenant {tenantId}");
        //        if (triplogId == 0) return;
        //        IBackgroundJobClient client = new BackgroundJobClient();
        //        IState state = new EnqueuedState
        //        {
        //            Queue = $"fifo_{tenantId.Replace("-", "").ToLower()}",
        //        };
        //        client.Create(() => RunTripPostProcess_Sub(null, triplogId, sessionId, tenantId), state);
        //    }
        //    catch (Exception ex)
        //    {
        //        //context.WriteLine(ConsoleTextColor.Red, ex);
        //    }
        //}
        //[Queue("fifo_post_transaction"), DisableConcurrentExecution(60)]
        //[AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        //public void RunTripPostProcess_Sub(PerformContext context, long triplogId, long sessionId, string tenantId)
        //{
        //    try
        //    {
        //        context.WriteLine(ConsoleTextColor.Cyan, $"Trip Post process started for TripId {triplogId} for tenant {tenantId}");
        //        if (triplogId == 0) return;
        //        using (TrackoApiDbContext _db = new TrackoApiDbContext(new TenantConnection { TenantId = tenantId }, _gs))
        //        {
        //            var config = _db.GetApiConfig<int>("RunTripPostProcess");
        //            context.WriteLine(ConsoleTextColor.Cyan, $"RunTripPostProcess:{config}");
        //            if (config == 0) return;
        //            _db.ExecuteProcedure("[dbo].[Proc_TRANS_RunPostTripProcess]",
        //                new[] { new SqlParameter("triplogid", triplogId), new SqlParameter("sessionid", sessionId) });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //context.WriteLine(ConsoleTextColor.Red, ex);
        //    }
        //}
        [Queue("fifo_post_transaction"), DisableConcurrentExecution(60)]
        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime(1)]
        public void RunCNPostProcess(PerformContext context, long cnId, long sessionId, string tenantId)
        {
            try
            {
                context.WriteLine(ConsoleTextColor.Cyan, $"Trip CN process started for CNId {cnId} for tenant {tenantId}");
                if (cnId == 0) return;
                IBackgroundJobClient client = new BackgroundJobClient();
                IState state = new EnqueuedState
                {
                    Queue = $"fifo_{tenantId.Replace("-", "").ToLower()}"
                };
                client.Create(() => RunCNPostProcess_Sub(null, cnId, sessionId, tenantId), state);
            }
            catch (Exception ex)
            {
                //context.WriteLine(ConsoleTextColor.Red, ex);
            }
        }
        public void RunCNPostProcess_Sub(PerformContext context, long cnId, long sessionId, string tenantId)
        {
            try
            {
                context.WriteLine(ConsoleTextColor.Cyan, $"Trip CN process started for CNId {cnId} for tenant {tenantId}");
                if (cnId == 0) return;
                using (TrackoApiDbContext _db = new TrackoApiDbContext(new TenantConnection { TenantId = tenantId }, _gs))
                {
                    var config = _db.GetApiConfig<int>("RunCNPostProcess");
                    context.WriteLine(ConsoleTextColor.Cyan, $"RunCNPostProcess:{config}");
                    if (config == 0) return;
                    _db.ExecuteProcedure("[dbo].[Proc_TRANS_RunCNTripProcess]",
                        new[] { new SqlParameter("cnid", cnId), new SqlParameter("sessionid", sessionId) });
                }
            }
            catch (Exception ex)
            {
                //context.WriteLine(ConsoleTextColor.Red, ex);
            }
        }
        public async void ScheduleHttpCall(string batchId,string senderId,long procId=0, PerformContext context=null)
        {
            try
            {
                var conn = new TenantConnection { TenantId = senderId };
                List<HttpRequestPool> requests = null;
                using (var ctx = new TrackoApiDbContext(conn, _gs))
                {
                    requests = ctx.HttpRequestPools.Where(x => x.BatchId == batchId).AsNoTracking().ToList();
                }                
                
                if (requests==null||requests.Count <= 0) throw new BusinessException(ErrorCode.GLB106, "Invalid BatchId");
                //var urls = requests.Select(x => x.Uri).Distinct().ToList();
                var tasks = new List<Task>();
                RestClient client = new RestClient();
                int ctr = 0;
                foreach (var req in requests)
                {
                    tasks.Add(SingleHttpCall(client, req, ctr++));
                }
                await Task.WhenAll(tasks);
                using (var ctx = new TrackoApiDbContext(conn, _gs))
                {
                    foreach (var rec in requests)
                    {
                        ctx.HttpRequestPools.AddOrUpdate(rec);
                    }                    
                    
                    await ctx.SaveChangesAsync();
                    if (procId > 0)
                    {
                        var spname = await ctx.ReportProcedures.Where(x => x.Id == procId).Select(x => x.StoredProcedureName).FirstOrDefaultAsync();
                        if (!string.IsNullOrWhiteSpace(spname))
                        {
                            try
                            {
                                context.WriteLine(ConsoleTextColor.White, $"Executing Post Procedure {spname}'{batchId}')");
                                await ctx.ExecuteProcedureAsync($"{spname}", new SqlParameter("batchid", batchId));
                            }
                            catch (Exception ex)
                            {
                                context.WriteLine(ConsoleTextColor.Red, ex.GetBaseException().Message);
                            }
                        }
                    }

                    ctx.Dispose();
                }
                
                context.WriteLine(ConsoleTextColor.White, $"Fasttag Http Task Completed with BatchId:{batchId}");
            }
            catch (Exception ex)
            {
                //context.WriteLine(ConsoleTextColor.Red,ex);
            }
        }
        private async Task SingleHttpCall(IRestClient client, HttpRequestPool req,int delayinsecond)
        {
            bool isError = false;
            try
            {
                var contentType = "application/json";
                var watch = new Stopwatch();

                var method = (RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), req.Method.ToUpper());

                var request = new RestRequest(req.Uri, method);
                foreach (var item in req._headers)
                {
                    if (item.Key.ToLower() == "Content-Type" && !string.IsNullOrWhiteSpace(item.Value.ToString())) contentType = item.Value.ToString();
                    request.AddHeader(item.Key, item.Value.ToString());
                }
                if (req.Timeout <= 0)
                {
                    req.Timeout = 18000;
                }
                if (contentType.Contains("json"))
                {
                    request.RequestFormat = DataFormat.Json;
                }
                else if (contentType.Contains("xml"))
                {
                    request.RequestFormat = DataFormat.Xml;
                }
                if (!string.IsNullOrWhiteSpace(req.RequestBody))
                {
                    request.AddParameter(contentType, req.RequestBody, ParameterType.RequestBody);
                }
                IRestResponse res;
                do
                {
                    watch.Start();
                    #region adding delay if error occur for next call
                    try { await Task.Delay(delayinsecond*2000); } catch { }
                    #endregion
                    res = await client.ExecuteTaskAsync(request);
                    req.ExecutedTime = req.ProcessTime = DateTime.Now;
                    watch.Stop();
                    req.Result = res.Content;
                    if (string.IsNullOrWhiteSpace(req.Result) && res.ErrorException != null)
                    {
                        isError = true;
                        req.Result = res.ErrorException.GetBaseException().Message;
                    }
                    else if (string.IsNullOrWhiteSpace(req.Result))
                    {
                        isError = true;
                        req.Result = res.ErrorMessage;
                    }
                    else if (!string.IsNullOrWhiteSpace(req.Result) && !string.IsNullOrWhiteSpace(req.SuccessString) && !req.Result.StartsWith(req.SuccessString))
                    {
                        isError = true;
                    }
                    req.NoofAttempts--;
                }
                while (isError && req.NoofAttempts > 0);

                req.IsProceeded = !isError;
                if (req.LogRequest || isError)
                {
                    var log = LogRequest(client, request, res, watch.ElapsedMilliseconds);
                    req.LogData = JsonConvert.SerializeObject(log);
                }
            }
            catch (Exception ex)
            {
                req.Result = ex.Message;
            }
        }
        #region User Jobs
        [Queue("business_queue"), DisableConcurrentExecution(60)]
        public void RunBusinessSchedule(PerformContext context,long? scheduleId,string tenantId)
        {
            TrackoApiDbContext db=null;
            try
            {
                db = new TrackoApiDbContext(new TenantConnection() { TenantId = tenantId }, _gs);
                var jobsgroups = db.JobLogs.Where(x =>x.ScheduleId == scheduleId&& x.StartDate <= DateTime.Now && (x.EndDate==null||x.EndDate>=DateTime.Now)&&(x.MaxRetry<=0||x.MaxRetry>x.Logs.Count())).GroupBy(x => x.JobNatureId).ToList();

                if (jobsgroups.Count == 0)
                {
                    context.WriteLine($"No Jobs are attached");
                    return;
                }
                foreach (var jobgroup in jobsgroups)
                {
                    context.WriteLine($"Procesing Job Group Category {jobgroup.Key} No of Jobs {jobgroup.Count()}");
                    switch (jobgroup.Key)
                    {
                        case 1507://Alert
                            ProcessAlertJob(db, context, jobgroup.ToList(), scheduleId, tenantId);
                            break;
                        case 1508://APIJob
                            ProcessAPIJob(db, context, jobgroup.ToList(), scheduleId, tenantId);
                            break;
                        case 1509://SQLJob
                            ProcessSQLJob(db, context, jobgroup.ToList(), scheduleId, tenantId);
                            break;
                        default:
                            context.WriteLine($"Unable to process ({jobgroup.Count()}) jobs, As the job category {jobgroup.Key} was unkown or wasn't yet programmed");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(new WebApiUsage()
                {
                    IP = scheduleId.ToString(),
                    ResponseContent = ex.ToString(),
                    RequestMethod = "",
                    ResponseTimestamp = DateTime.Now,
                    RequestTimestamp = DateTime.Now,
                    RequestContent = "",
                    TenantKey=tenantId
                });
                context.WriteLine($"Schedule {scheduleId} for Tenant {tenantId} failed with Error\n {ex.GetBaseException()}");
            }
            finally
            {
                db?.Dispose();
            }
        }

        private void ProcessAPIJob(TrackoApiDbContext db, PerformContext context, List<JobLog> list, long? scheduleId, string tenantId)
        {
            throw new NotImplementedException();   
        }

        private void ProcessSQLJob(TrackoApiDbContext db, PerformContext context, List<JobLog> list, long? scheduleId, string tenantId)
        {
            var watch = new Stopwatch();
            foreach (var job in list)
            {                
                var joblog = job.LogExecution();
                watch.Start();
                using (var tran = db.Database.BeginTransaction(IsolationLevel.ReadCommitted))
                {
                    
                    try
                    {
                        if (job.ReportPoolId > 0)
                        {
                            var req = db.ReportsRequestPool.Where(x => x.Id == job.ReportPoolId).FirstOrDefault();
                            db.ExecuteProcedure(req.Query, req.BuildSqlParameters());
                        }
                        else
                        {
                            db.ExecuteProcedure(job.MessageBody);
                        }
                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        context.WriteLine(ConsoleTextColor.Red, $"Error while executing SQL Job Id {job.Id} with error \n {ex.GetBaseException().Message}");
                        job.LastJobStatus =job.LastJobStatus= TrackoAPI.Models.Shared.JobResult.Failed;
                        joblog.AppendResponse(ex.GetBaseException().Message);
                    }
                }
                joblog.Duration = watch.ElapsedMilliseconds;
                joblog.ExecutionEndTime = DateTime.Now;
                watch.Stop();
                db.JobRetryLogs.AddOrUpdate(joblog);
            }
            db.SaveChanges();
        }

        private void ProcessAlertJob(TrackoApiDbContext db, PerformContext context, List<JobLog> list, long? scheduleId, string tenantId)
        {
            var watch = new Stopwatch();
            foreach (var job in list)
            {
                var joblog = job.LogExecution();
                var session = db.ApiSessions.FirstOrDefault(x => x.Id == job.CreatedSessionId);
                try
                {                    
                    watch.Start();
                    switch (job.MessageType)
                    {
                        case TrackoAPI.Models.Shared.NotificationType.Email:
                            EmailJobProcessing(db, context, job, scheduleId, tenantId,joblog, session);
                            break;
                        case TrackoAPI.Models.Shared.NotificationType.SMS:
                            if (!string.IsNullOrWhiteSpace(job.MessageBody))
                            {
                                SMSJobProcessing(db, context, job, scheduleId, tenantId, joblog, session);
                            }
                            break;
                        case TrackoAPI.Models.Shared.NotificationType.WebHook:
                            WebHookJobProcessing(db, context, job, scheduleId, tenantId, joblog, session);
                            break;
                        case TrackoAPI.Models.Shared.NotificationType.WhatsApp:
                            if (!string.IsNullOrWhiteSpace(job.MessageBody))
                            {
                                WhatsAppJobProcessing(db, context, job, scheduleId, tenantId, joblog, session);
                            }
                            break;
                        case TrackoAPI.Models.Shared.NotificationType.VoiceMessage:
                            VoiceMessageJobProcessing(db, context, job, scheduleId, tenantId, joblog, session);
                            break;
                        case TrackoAPI.Models.Shared.NotificationType.Broadcast:
                            BroadcastJobProcessing(db, context, job, scheduleId, tenantId, joblog, session);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    context.WriteLine(ConsoleTextColor.Red, $"Error while executing {job.MessageType} Job Id {job.Id} with error \n {ex.GetBaseException().Message}");
                    context.WriteLine(ConsoleTextColor.Red, ex);
                    job.LastJobStatus = joblog.LastJobStatus = TrackoAPI.Models.Shared.JobResult.Failed;
                    joblog.AppendResponse(ex.GetBaseException().Message);
                }
                joblog.Duration = watch.ElapsedMilliseconds;
                joblog.ExecutionEndTime = DateTime.Now;
                db.JobRetryLogs.AddOrUpdate(joblog);
                watch.Stop();
            }
            db.SaveChanges();
        }
            #region alert Sub job processing
        public void EmailJobProcessing(TrackoApiDbContext db, PerformContext context, JobLog job, long? scheduleId, string tenantId, JobRetryLog joblog, ApiSession session)
        {
            try
            {
                var emailservice = Unity.Config.UnityCore.Container.Resolve<ISendGridEmailService>();
                context.WriteLine($"Discovered {emailservice.GetType().FullName} as Mail Service");
                var addresses = db.MessageAddresses.Where(x => x.JobId == job.Id).Select(x => new
                {
                    x.AddressType,
                    x.fk_Contact.LastName,
                    x.fk_Contact.MiddleName,
                    x.fk_Contact.FirstName,
                    x.fk_Contact.ContactValue
                }).ToList();

                var email = new SendGridEmailViewModel();
                email.Tos.AddRange(addresses.Where(x => x.AddressType == AddressType.To).Select(x => new EmailAddressModel()
                {
                    EmaillAddress = x.ContactValue,
                    Name = $"{x.FirstName} {x.MiddleName} {x.LastName}"
                }));
                if (email.Tos == null || !email.Tos.Any())
                {
                    var addrs = job.InlineAddresses?.Split(';');
                    if (addrs != null && addrs.Any())
                    {
                        email.Tos = new List<EmailAddressModel>(addrs.Select(x => new EmailAddressModel(x)));
                    }
                }
                email.Ccs.AddRange(addresses.Where(x => x.AddressType == AddressType.CC).Select(x => new EmailAddressModel()
                {
                    EmaillAddress = x.ContactValue,
                    Name = $"{x.FirstName} {x.MiddleName} {x.LastName}"
                }));
                email.Bccs.AddRange(addresses.Where(x => x.AddressType == AddressType.BCC).Select(x => new EmailAddressModel()
                {
                    EmaillAddress = x.ContactValue,
                    Name = $"{x.FirstName} {x.MiddleName} {x.LastName}"
                }));
                var reply = addresses.Where(x => x.AddressType == AddressType.ReplayTo).Select(x => new EmailAddressModel()
                {
                    EmaillAddress = x.ContactValue,
                    Name = $"{x.FirstName} {x.MiddleName} {x.LastName}"
                }).FirstOrDefault();
                if (reply != null)
                {
                    email.ReplyTo = reply;
                }
                StubbleVisitorRenderer stubble = null;
                var data = string.IsNullOrWhiteSpace(job._ExtendedInfo) ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(job._ExtendedInfo);
                if (job.BodyIsTemplate || job.SubjectIsTemplate)
                {
                    try
                    {
                        stubble = new StubbleBuilder().Build();                        
                        if (job.SubjectIsTemplate)
                        {
                            email.Subject = stubble.Render(job.Subject, data);
                        }
                        else
                        {
                            email.Subject = job.Subject;
                        }
                        if(!job.BodyHasEmbeddedData)
                        {
                            if (job.BodyIsTemplate)
                            {
                                email.HtmlBody = stubble.Render(job.MessageBody, data);
                            }
                            else
                            {
                                email.HtmlBody = job.MessageBody;
                            }
                        }
                        
                    }
                    catch (Exception ex)
                    {                        
                        joblog.AppendResponse(ex.GetBaseException().Message);
                        context.WriteLine(ConsoleTextColor.Red, $"Error while executing {job.MessageType} Job Id {job.Id} with error \n {ex.StackTrace}");
                    }
                }
                if (string.IsNullOrWhiteSpace(email.Subject))
                {
                    email.Subject = job.Subject;
                }
                if (string.IsNullOrWhiteSpace(email.HtmlBody)&& !job.BodyHasEmbeddedData)
                {
                    email.HtmlBody = job.MessageBody;
                }
                if (job.ReportPoolId > 0)
                {
                    var parameterInfo = "Report Parameter: ";
                    var param = data["UIParameter"];
                    var reportName = data["ReportName"] as string;
                    var sqlparam = (data["UIParameter"] as JObject)?.ToObject<GofSqlParameter>();
                    var req = db.ReportsRequestPool.Where(x => x.Id == job.ReportPoolId).FirstOrDefault();
                    var reportcustomization = (req.ReportId > 0 ? db.ReportCustomizations.Where(x=>x.ReportId==req.ReportId).Select(x => new
                    {
                        x.HiddenColumns,
                        x.GroupingColumns,
                        x.SummarizedColumns,
                        x.FreezeColumn,
                        x.CountColumns,
                        x.AvgColumns
                    }) : db.UserDefinedReports.Where(x=>x.Id==req.CustomReportId).Select(x => new
                    {
                        x.HiddenColumns,
                        x.GroupingColumns,
                        x.SummarizedColumns,
                        x.FreezeColumn,
                        x.CountColumns,
                        x.AvgColumns
                    })).FirstOrDefault();
                    if (sqlparam != null)
                    {
                        var parameters = (req.ReportId > 0 ? db.ReportParameters.Where(x => x.ReportId == req.ReportId).Select(x => new
                        {
                            ParamName = x.fk_Parameter.ConstantName,
                            x.ParameterCaption,
                            x.ProcParamName,
                            x.FieldTypeId
                        }) : db.UserDefinedReportParameters.Where(x => x.ReportId == req.ReportId).Select(x => new
                        {
                            ParamName = x.fk_Parameter.ConstantName,
                            x.ParameterCaption,
                            x.ProcParamName,
                            x.FieldTypeId
                        })).ToList();
                        try
                        {
                            var sp = GetProperties(req);//ServiceParam
                            var vp = GetProperties(sqlparam);//Gof params
                            
                            
                            foreach (var p in parameters.OrderBy(x=>x.ProcParamName))
                            {
                                var vpf = vp.FirstOrDefault(x => x.Name == p.ParamName);//Parameter1 
                                var vpfDisplay = "";
                                if (vpf != null)
                                {
                                    var vpfRawData = ((vpf.GetValue(sqlparam, null) as string) ?? "").Split('^');
                                    if (vpfRawData != null)
                                    {
                                        vpfDisplay = vpfRawData.Length > 1 ? vpfRawData[1] : vpfRawData[0];
                                    }
                                }

            #region BuildParameter
                                if (p.FieldTypeId == TrackoAPI.Models.Shared.ReportParameterType.DateTime)
                                {
                                    var spf = sp.FirstOrDefault(x => x.Name.Trim(' ') == p.ProcParamName.Trim(' '));
                                    if (spf != null)
                                    {
                                        var spf_raw = (spf.GetValue(req, null) as string) ?? "";
                                        if(!string.IsNullOrWhiteSpace(spf_raw))
                                        {
                                            var date = spf_raw.Length<11? DateTime.ParseExact(spf_raw, "yyyy-MM-dd", null): DateTime.ParseExact(spf_raw, "yyyy-MM-dd HH:mm", null);
                                            switch (job.IntervalTypeId)
                                            {
                                                case 1495:/*Hourly[IntervalType (0-23)]*/
                                                    date = date.AddHours(job.IntervalValue);
                                                    break;
                                                case 1496:/*Daily[IntervalType(1-31)]*/
                                                    date = date.AddDays(job.IntervalValue);
                                                    break;
                                                case 1497:/*Weekly[IntervalType (1 - 4)]*/
                                                    break;
                                                case 1498:/*Monthly[IntervalType (1-12 or JAN-DEC)]*/
                                                    date = date.AddMonths(job.IntervalValue);
                                                    break;
                                                case 1499:/*Quarterly[IntervalType (1-4)]*/
                                                    date = date.AddMonths(job.IntervalValue*3);
                                                    break;
                                                case 1500:/*Half Yearly[IntervalType (1 - 2)]*/
                                                    date = date.AddMonths(job.IntervalValue * 6);
                                                    break;
                                                case 1501:/*Yearly[IntervalType (1970–2099)]*/
                                                    date = date.AddYears(job.IntervalValue);
                                                    break;
                                                case 1510:/*Half Month[IntervalType (1-15 or 16-31)]*/
                                                    var days = date.Day > 15 ? 15 : DateTime.DaysInMonth(date.Year, date.Month) - 15;
                                                    date = date.AddDays(days);
                                                    break;
                                                case 1513:/*Day of month[IntervalType (1-31)]*/
                                                    date = new DateTime(date.Month==12?date.Year+1: date.Year, date.Month == 12 ? 1 : date.Month+1, job.IntervalValue);
                                                    break;
                                                case 1514:/*Day of week[IntervalType (0-6 or SUN-SAT)]*/
                                                    //var lastweekday = (int)date.DayOfWeek;
                                                    //if (lastweekday != job.IntervalValue)
                                                    //{
                                                    //    if(job.IntervalValue<=)
                                                    //}
                                                    //var scheduledWeekDay=lastweekday+job.IntervalValue;

                                                    break;
                                                case 1515:/*Minutes[Interval (0-59)]*/
                                                    date = date.AddMinutes(job.IntervalValue);
                                                    break;
                                                default:
                                                    date = DateTime.Now;
                                                    break;
                                            }
                                            spf_raw = date.ToString("yyyy-MM-dd HH:mm");
                                            vpfDisplay= date.ToString("dd-MM-yyyy HH:mm");
                                            spf?.SetValue(req, Convert.ChangeType(spf_raw, spf.PropertyType), null);
                                        }
                                        
                                    }
                                    if(req.ObjectState!=ObjectState.Modified)
                                    {
                                        req.ObjectState = ObjectState.Modified;
                                    }
                                }
            #endregion
            #region Build ParameterInfo                                                               
                                if (vpf == null) continue;
                                if (!string.IsNullOrWhiteSpace(vpfDisplay))
                                {
                                    parameterInfo += " " + p.ParameterCaption + " : " + vpfDisplay + ";";
                                }
            #endregion

                            }
                        }
                        catch (Exception ex)
                        {
                            job.LastJobStatus = joblog.LastJobStatus = TrackoAPI.Models.Shared.JobResult.Failed;
                            joblog.AppendResponse(ex.GetBaseException().Message);
                            context.WriteLine(ConsoleTextColor.Red, "Something went wrong while collecting Report Parameters.");
                            context.WriteLine(ConsoleTextColor.Red, $"Error while executing {job.MessageType} Job Id {job.Id} with error \n {ex.StackTrace}");
                            return;
                        }
                    }
                    
                    var dataTable = db.GetDataTableByProcedure(req.Query, req.BuildSqlParameters());
                    if (dataTable.Rows.Count > 0)
                    {
                        db.ReportsRequestPool.AddOrUpdate(req);
                    }
                    if (!string.IsNullOrWhiteSpace(reportcustomization?.HiddenColumns))
                    {
                        var cols = reportcustomization.HiddenColumns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var col in cols.Where(x=>dataTable.Columns.Contains(x)))
                        {
                            dataTable.Columns.Remove(dataTable.Columns[col]);
                        }
                    }
                    byte[] bytes = null;
                    using (ExcelPackage pck = new ExcelPackage())
                    {
                        ExcelWorksheet ws = pck.Workbook.Worksheets.Add("Report");
                        pck.Workbook.CalcMode = ExcelCalcMode.Automatic;
                        pck.Workbook.Date1904 = true;
                        
                        using (var prange = ws.Cells[1, 1, 2, dataTable.Columns.Count])
                        {
                            prange.Style.Font.Bold = true;                       
                            prange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            prange.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#2E7DB8"));
                            prange.Style.Font.Color.SetColor(Color.White);
                            prange.Merge = true;
                            prange.Value = parameterInfo;
                        }

                        using(var drange=ws.Cells["A3"].LoadFromDataTable(dataTable, true,TableStyles.Light13))
                        {
                            var tbl = ws.Tables.Add(drange,"report_data_range");
                            //tbl.ShowFilter = true;
                            int colNumber = 1;
                            foreach (DataColumn col in dataTable.Columns)
                            {
                                if (col.DataType == typeof(DateTime))
                                {
                                    ws.Column(colNumber).Style.Numberformat.Format = "dd-MMM-yyyy HH:mm";
                                }
                                colNumber++;
                            }
                            if (reportcustomization != null)
                            {
                                tbl.ShowTotal = true;
                                
                                string stylename = "StyleName";
                                var style = pck.Workbook.Styles.CreateNamedStyle(stylename);
                                style.Style.Numberformat.Format = "#,###.00";
                                if (!string.IsNullOrWhiteSpace(reportcustomization.SummarizedColumns))
                                {
                                    var sumcols = reportcustomization.SummarizedColumns.Split(new[] { ',' },StringSplitOptions.RemoveEmptyEntries);
                                    foreach(var col in sumcols)
                                    {
                                        var tcol = tbl.Columns[col];
                                        if (tcol != null)
                                        {
                                            tcol.TotalsRowFunction = RowFunctions.Sum;
                                            tcol.DataCellStyleName = stylename;
                                        }                                        
                                    }                                    
                                }
                                if (!string.IsNullOrWhiteSpace(reportcustomization.AvgColumns))
                                {
                                    var cols = reportcustomization.AvgColumns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var col in cols)
                                    {
                                        var tcol = tbl.Columns[col];
                                        if (tcol != null)
                                        {
                                            tcol.TotalsRowFunction = RowFunctions.Average;
                                            tcol.DataCellStyleName = stylename;
                                        }
                                    }
                                }
                                if (!string.IsNullOrWhiteSpace(reportcustomization.CountColumns))
                                {
                                    var cols = reportcustomization.CountColumns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                    foreach (var col in cols)
                                    {
                                        var tcol = tbl.Columns[col];
                                        if (tcol != null)
                                        {
                                            tcol.TotalsRowFunction = RowFunctions.Count;
                                            tcol.DataCellStyleName = stylename;                                            
                                        }
                                    }
                                }                                
                            }
                            drange.Calculate();
                            drange.AutoFitColumns();
                        }
                        using (var range = ws.Cells[3, 1, 3, dataTable.Columns.Count])  //Address "A3:A5"
                        {
                            range.Style.Font.Bold = true;
                            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                            range.Style.Fill.BackgroundColor.SetColor(ColorTranslator.FromHtml("#8FBEE1"));
                            range.Style.Font.Color.SetColor(Color.White);
                        }
                        //pck.Workbook.Calculate();
                        using (MemoryStream outputStream = new MemoryStream())
                        {
                            pck.SaveAs(outputStream);
                            bytes = outputStream.ToArray();
                        }

                        //bytes = pck.GetAsByteArray();
                        pck.Dispose();
                    }
                    string file = System.Convert.ToBase64String(bytes);
                    var attachment= new AttachmentDetail
                    {
                        Content = file,
                        Filename = $@"{reportName}[{req.ReportId??req.CustomReportId}].xlsx",
                        Disposition = "attachment",
                        Type = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        ContentId = Guid.NewGuid().ToString("D")
                    };
                    email.Attachments.Add(attachment);
                }
                context.WriteLine("Calling Mail Service");
                var res=emailservice.SendAsync(email, session.UserId, tenantId).Result;
                if (!res.IsSuccessful)
                {
                    job.LastJobStatus = joblog.LastJobStatus = TrackoAPI.Models.Shared.JobResult.Failed;
                }
                else
                {
                    job.LastJobStatus = joblog.LastJobStatus = TrackoAPI.Models.Shared.JobResult.Success;
                }
                joblog.AppendResponse(res.ToString());
            }
            catch (Exception ex)
            {
                var be = ex.GetBusinessException();
                if (be != null)
                {
                    job.LastJobStatus = joblog.LastJobStatus = TrackoAPI.Models.Shared.JobResult.Failed;
                    joblog.AppendResponse(be.ExtraInfo);
                    context.WriteLine(ConsoleTextColor.Red, $"Error while executing {job.MessageType} Job Id {job.Id} with error \n {be.ExtraInfo}");
                }
                else
                {
                    job.LastJobStatus = joblog.LastJobStatus = TrackoAPI.Models.Shared.JobResult.Failed;
                    joblog.AppendResponse(ex.GetBaseException().Message);
                    context.WriteLine(ConsoleTextColor.Red, $"Error while executing {job.MessageType} Job Id {job.Id} with error \n {ex}");
                }
            }
        }
        
        public void SMSJobProcessing(TrackoApiDbContext db, PerformContext context, JobLog job, long? scheduleId, string tenantId, JobRetryLog joblog, ApiSession session)
        {
            try
            {
                var sms_service = Unity.Config.UnityCore.Container.Resolve<ISMSService>();
                var addresses = db.MessageAddresses.Where(x => x.JobId == job.Id).Select(x => new
                {
                    x.fk_Contact.LastName,
                    x.fk_Contact.MiddleName,
                    x.fk_Contact.FirstName,
                    x.fk_Contact.ContactValue
                }).DistinctBy(x=>x.ContactValue).ToList();
                var sms = new SMSViewModel(job.MessageBody);
                if (addresses.Any())
                {
                    sms.To.AddRange(addresses.Select(x => x.ContactValue));
                }
                else
                {
                    var addrs=job.InlineAddresses?.Split(';');
                    if (addrs != null && addrs.Any())
                    {
                        sms.To.AddRange(addrs);
                    }
                }
                if (sms.To == null || !sms.To.Any() || string.IsNullOrWhiteSpace(sms.Message))
                {                    
                    return;
                }
                StubbleVisitorRenderer stubble = null;
                var data = string.IsNullOrWhiteSpace(job._ExtendedInfo) ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(job._ExtendedInfo);
                if (job.BodyIsTemplate)
                {
                    try
                    {
                        stubble = new StubbleBuilder().Build();
                        sms.Message = stubble.Render(job.MessageBody, data);
                    }
                    catch (Exception ex)
                    {
                        joblog.AppendResponse(ex.GetBaseException().Message);
                        context.WriteLine(ConsoleTextColor.Red, $"Error while executing {job.MessageType} Job Id {job.Id} with error \n {ex.StackTrace}");
                    }
                }
                else
                {
                    sms.Message = job.MessageBody;
                }
                context.WriteLine("Calling SMS Service");
                var sms_temp = new SMSTemplate()
                {
                    Country="91",
                    SMS=new List<SMSViewModel>() {sms }
                };
                var sms_result= sms_service.SendAsync(sms_temp, session?.UserId??0, tenantId).Result;
                context.WriteLine($"SMS Response :\n{JsonConvert.SerializeObject(sms_result)}");
                if (sms_result?.Status != System.Net.HttpStatusCode.OK)
                {
                    context.WriteLine(JsonConvert.SerializeObject(sms_temp));
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void WebHookJobProcessing(TrackoApiDbContext db, PerformContext context, JobLog job, long? scheduleId, string tenantId, JobRetryLog joblog, ApiSession session)
        {
            throw new NotImplementedException();
        }
        public void WhatsAppJobProcessing(TrackoApiDbContext db, PerformContext context, JobLog job, long? scheduleId, string tenantId, JobRetryLog joblog, ApiSession session)
        {
            throw new NotImplementedException();
        }
        public void VoiceMessageJobProcessing(TrackoApiDbContext db, PerformContext context, JobLog job, long? scheduleId, string tenantId, JobRetryLog joblog, ApiSession session)
        {
            throw new NotImplementedException();
        }
        public void BroadcastJobProcessing(TrackoApiDbContext db, PerformContext context, JobLog job, long? scheduleId, string tenantId, JobRetryLog joblog, ApiSession session)
        {
            throw new NotImplementedException();
        }
            #endregion
        private static PropertyInfo[] GetProperties(object obj)
        {
            return obj.GetType().GetProperties();
        }

        
        #endregion
    }
    public class DbBackgroundJobs: IDbBackgroundJobs
    {
        private readonly IGlobalStore _gs;

        public DbBackgroundJobs(IGlobalStore globalStore)
        {
            _gs = globalStore;
        }
        [Queue("fifo_event_stockmerge"), DisableConcurrentExecution(60), AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail), ProlongExpirationTime]
        public void MergeCNStock(PerformContext context, string tenantId, long cnid, long officeId, long logId,long existingLogId)
        {
            TrackoApiDbContext db = null;
            try
            {
                db = new TrackoApiDbContext(new TenantConnection() { TenantId = tenantId }, _gs);
                db.ExecuteProcedure("[dbo].[Proc_GLB_TRANS_MergeStock]", new SqlParameter("cnid", cnid), new SqlParameter("officeid", officeId), new SqlParameter("logid", logId), new SqlParameter("existinglogid", existingLogId));
            }
            catch (Exception ex)
            {
                LogError(new WebApiUsage()
                {
                    IP = $"{logId}-{cnid}-{officeId}",
                    ResponseContent = ex.ToString(),
                    RequestMethod = "",
                    ResponseTimestamp = DateTime.Now,
                    RequestTimestamp = DateTime.Now,
                    RequestContent = "",
                    TenantKey = tenantId
                });
                context.WriteLine($"StockLog Process {logId} for Tenant {tenantId} failed with Error\n {ex.GetBaseException()}");
            }
            finally
            {
                db?.Dispose();
            }
        }
        public void LogError(WebApiUsage usage)
        {
            try
            {
                if (Helper.HostedOnPremise) return;
                using (var db = new TenantDbContext())
                {
                    db.ApiLog.Add(usage);
                    db.SaveChanges();
                }
            }
            catch (Exception)
            {
                //Ignore
            }
        }
    }
}
