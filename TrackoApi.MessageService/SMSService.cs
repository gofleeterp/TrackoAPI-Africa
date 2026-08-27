using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;

namespace TrackoApi.MessageService
{
    public class SMSService : ISMSService
    {
        private JsonSerializerSettings _jsonSetting;

        public SMSService()
        {
            _jsonSetting = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        public async Task<SMSResult> SendAsync(SMSTemplate sms, long userId = 0, string tenantId = null)
        {
            using (var db = !Helper.HostedOnPremise ? new TenantDbContext() : new CoreSettingDb("HostCoreConnection"))
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entity = new NotificationLog
                        {
                            Data = JsonConvert.SerializeObject(sms.SMS.GroupBy(x => x.Message).Select(x => new
                            {
                                Message = x.Key,
                                To = x.SelectMany(y => y.To, (p, c) => c)
                            })),
                            NoOfNotification = sms.SMS.SelectMany(x => x.To, (p, c) => c).Count(),
                            NotificationType = TrackoAPI.Models.Shared.NotificationType.SMS,
                            Status = "Pending",
                            TenantId = tenantId,
                            UserId = userId,
                            SentTime = DateTimeOffset.Now,
                            IsSent = false
                        };
                        db.NotificationLogs.Add(entity);
                        try
                        {
                            var entitysaved = await db.SaveChangesAsync();
                            if (entitysaved == 0)
                            {
                                tran.Rollback();
                                return new SMSResult { Type = "Unable to save Log", Message = "Database call failed", Status = HttpStatusCode.InternalServerError };
                            }
                        }
                        catch (BusinessException be)
                        {
                            tran.Rollback();
                            return new SMSResult { Type = $"{be.Message}. \n{be.ODataErrorDetails?.Select(x => x.Message).JoinStrings("\n")}", Message = "Database call failed", Status = HttpStatusCode.PaymentRequired };
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            return new SMSResult { Message = ex.GetBaseException().Message, Type = "Database call failed", Status = HttpStatusCode.InternalServerError };
                        }
                        SMSResult result;
                        if (sms == null) throw new ArgumentNullException(nameof(sms));
                        if (sms.SMS == null || !sms.SMS.Any())
                        {
                            entity.Status = "SMS Template doesn't contain any sms.";
                            entity.IsSent = false;
                            await db.SaveChangesAsync();
                            return new SMSResult { Message = "SMS Template doesn't contain any sms.", Type = "Database call failed", Status = HttpStatusCode.InternalServerError };
                        }
                        if (sms.SMS.Any(x => !x.To.Any()))
                        {
                            entity.Status = "SMS Template doesn't contain any sms.";
                            entity.IsSent = false;
                            await db.SaveChangesAsync();
                            return new SMSResult { Message = "SMS Template doesn't contain any sms.", Type = "Database call failed", Status = HttpStatusCode.InternalServerError };
                        }
                        var client = new RestClient("http://africa.msg91.com/api/v2/sendsms?country=91&sender=&route=&mobiles=&authkey=&encrypt=&message=&flash=&unicode=&schtime=&afterminutes=&response=&campaign=");
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("content-type", "application/json");
                        request.AddHeader("authkey", "205806ASVFCYBHpu5c98eb51");
                        sms.Sender = BuildSender(sms.Sender);
                        request.AddJsonBody(JsonConvert.SerializeObject(sms, _jsonSetting));
                        IRestResponse response = client.Execute(request);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            result = JsonConvert.DeserializeObject<SMSResult>(response.Content);
                            result.Status = response.StatusCode;
                            entity.Status = "Sent";
                            entity.IsSent = true;
                            entity.MessageId = result.Message;
                            await db.SaveChangesAsync();

                        }
                        else
                        {
                            result = new SMSResult
                            {
                                Message = !string.IsNullOrWhiteSpace(response.Content) ? response.Content : response.ErrorMessage,
                                Type = string.IsNullOrWhiteSpace(response.StatusDescription) ? SMSResult.GetStatusMessage((int)response.StatusCode) : response.StatusDescription,
                                Status = response.StatusCode
                            };
                            entity.Status = result.Message;
                            entity.IsSent = false;
                            await db.SaveChangesAsync();
                        };
                        tran.Commit();
                        return result;
                    }
                    catch (BusinessException be)
                    {
                        tran.Rollback();
                        return new SMSResult { Type = $"{be.Message}. \n{be.ODataErrorDetails?.Select(x => x.Message).JoinStrings("\n")}", Message = "Database call failed", Status = HttpStatusCode.PaymentRequired };
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return new SMSResult { Message = ex.GetBaseException().Message, Type = "Database call failed", Status = HttpStatusCode.InternalServerError };
                    }
                }
            }

        }
        private string BuildSender(string sender)
        {
            return sender.PadLeft(6, 'X');
        }
        public async Task<SMSResult> SendAsync(string message, string sender = "IWLT", long userId = 0, string tenantId = null, params string[] recivers)
        {
            if (recivers == null || !recivers.Any()) throw new ArgumentNullException(nameof(message));
            using (var db = !Helper.HostedOnPremise ? new TenantDbContext() : new CoreSettingDb("HostCoreConnection"))
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entity = new NotificationLog
                        {
                            Data = JsonConvert.SerializeObject(new List<dynamic>{
                            new {
                                Message = message,
                                To = recivers
                            } }),
                            NoOfNotification = recivers.Count(),
                            NotificationType = TrackoAPI.Models.Shared.NotificationType.SMS,
                            Status = "Pending",
                            TenantId = tenantId,
                            UserId = userId,
                            SentTime = DateTimeOffset.Now,
                            IsSent = false
                        };
                        db.NotificationLogs.Add(entity);
                        try
                        {
                            var entitysaved = await db.SaveChangesAsync();
                            if (entitysaved == 0)
                            {
                                tran.Rollback();
                                return new SMSResult { Type = "Unable to save Log", Message = "Database call failed", Status = HttpStatusCode.InternalServerError };
                            }
                        }
                        catch (BusinessException be)
                        {
                            tran.Rollback();
                            return new SMSResult { Type = $"{be.Message}. \n{be.ODataErrorDetails?.Select(x => x.Message).JoinStrings("\n")}", Message = "Database call failed", Status = HttpStatusCode.PaymentRequired };
                        }
                        catch (Exception ex)
                        {
                            tran.Rollback();
                            return new SMSResult { Message = ex.GetBaseException().Message, Type = "Database call failed", Status = HttpStatusCode.InternalServerError };
                        }
                        SMSResult result;
                        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(nameof(message));

                        var client = new RestClient("http://africa.msg91.com/api/v2/sendsms?country=91&sender=&route=&mobiles=&authkey=&encrypt=&message=&flash=&unicode=&schtime=&afterminutes=&response=&campaign=");
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("content-type", "application/json");
                        request.AddHeader("authkey", "205806ASVFCYBHpu5c98eb51");
                        var record = new SMSTemplate
                        {
                            Country = "91",
                            Route = "4",
                            Sender = BuildSender(sender),
                            SMS = new List<SMSViewModel>
                                    {
                                        new SMSViewModel
                                        {
                                            Message=message,
                                            To=recivers.ToList()
                                        }
                                    }
                        };
                        request.AddJsonBody(JsonConvert.SerializeObject(record, _jsonSetting));
                        IRestResponse response = client.Execute(request);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            result = JsonConvert.DeserializeObject<SMSResult>(response.Content);
                            result.Status = response.StatusCode;
                            entity.Status = "Sent";
                            entity.IsSent = true;
                            entity.MessageId = result.Message;
                            await db.SaveChangesAsync();

                        }
                        else
                        {
                            result = new SMSResult
                            {
                                Message = !string.IsNullOrWhiteSpace(response.Content) ? response.Content : response.ErrorMessage,
                                Type = string.IsNullOrWhiteSpace(response.StatusDescription) ? SMSResult.GetStatusMessage((int)response.StatusCode) : response.StatusDescription,
                                Status = response.StatusCode
                            };
                            entity.Status = result.Message;
                            entity.IsSent = false;
                            await db.SaveChangesAsync();
                        };
                        tran.Commit();
                        return result;
                    }
                    catch (BusinessException be)
                    {
                        tran.Rollback();
                        return new SMSResult { Type = $"{be.Message}. \n{be.ODataErrorDetails?.Select(x => x.Message).JoinStrings("\n")}", Message = "Database call failed", Status = HttpStatusCode.PaymentRequired };
                    }
                    catch (Exception ex)
                    {
                        tran.Rollback();
                        return new SMSResult { Message = ex.GetBaseException().Message, Type = "Database call failed", Status = HttpStatusCode.InternalServerError };
                    }
                }

            }
        }

        public async Task SendAsync(IdentityMessage message)
        {
            await SendAsync(message.Body, recivers: message.Destination, userId: Helper.GetLoggedInUserId(), tenantId: Helper.LoggedInTenantId, sender: Helper.TenantShortName);
        }
    }

}
