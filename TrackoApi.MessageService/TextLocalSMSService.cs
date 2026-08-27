using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;

namespace TrackoApi.MessageService
{
    public class TextLocalSMSService : ISMSService
    {
        private JsonSerializerSettings _jsonSetting;
        public TextLocalSMSService()
        {
            _jsonSetting = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }
        public async Task<SMSResult> SendAsync(SMSTemplate sms, long userId = 0, string tenantId = null)
        {
            using (var db =!Helper.HostedOnPremise? new TenantDbContext():new CoreSettingDb("HostCoreConnection"))
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
                        var client = new RestClient("https://africa.textlocal.in/send/");
                        client.Timeout = -1;
                        //if (string.IsNullOrWhiteSpace(sms.Sender))
                        //{
                            sms.Sender = "IWLTPL";
                        //}
                        SMSResult result=new SMSResult();
                        foreach (var req in sms.SMS)
                        {
                            var request = new RestRequest(Method.POST);
                            request.AddHeader("apiKey", "YjVhN2JiNWMzMzQ4MzE1ZDEzMWI2OTlkZjdkOTA4Yzg=");
                            request.AddHeader("format", "json");
                            request.AlwaysMultipartFormData = true;                           
                            request.AddParameter("apikey", "6e0be6251942f2ef4cd81920cf0f29f53c25f3c36f7028c2bc3c5a5c7c02d661");
                            request.AddParameter("sender",sms.Sender);
                            request.AddParameter("hash", "6e0be6251942f2ef4cd81920cf0f29f53c25f3c36f7028c2bc3c5a5c7c02d661");
                            request.AddParameter("username", "support@indiaweblab.com");
                            request.AddParameter("unicode", "true");

                            request.AddParameter("numbers", req.To.JoinStrings(","));
                            request.AddParameter("message", req.Message);    /*"आपके खाते में इंसेंटिव के रू. 700 डाले गए है। :- IWLTPL"*/                        
                            IRestResponse response = client.Execute(request);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                var lr = JsonConvert.DeserializeObject<TextLocalResponce>(response.Content);
                                result.Status = response.StatusCode;
                                result.Message = response.Content;
                                entity.Status = lr?.status;
                                entity.IsSent = lr?.status== "success";
                                entity.MessageId += lr?.messages.Select(x=>$"{x.id}-{x.recipient}").JoinStrings(",")??"";
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
                        }
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
        public async Task<SMSResult> SendAsync(string message, string sender = "IWLTPL", long userId = 0, string tenantId = null, params string[] recivers)
        {
            if (recivers == null || !recivers.Any()) throw new ArgumentNullException(nameof(message));
            using (var db = new TenantDbContext())
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
                        SMSResult result=new SMSResult();
                        if (string.IsNullOrWhiteSpace(message)) throw new ArgumentNullException(nameof(message));

                        var client = new RestClient("https://africa.textlocal.in/send/");
                        client.Timeout = -1;
                        if (string.IsNullOrWhiteSpace(sender))
                        {
                            sender = "IWLTPL";
                        }
                        var request = new RestRequest(Method.POST);
                        request.AddHeader("apiKey", "YjVhN2JiNWMzMzQ4MzE1ZDEzMWI2OTlkZjdkOTA4Yzg=");
                        request.AddHeader("format", "json");
                        request.AlwaysMultipartFormData = true;
                        request.AddParameter("apikey", "6e0be6251942f2ef4cd81920cf0f29f53c25f3c36f7028c2bc3c5a5c7c02d661");
                        request.AddParameter("sender", sender);
                        request.AddParameter("hash", "6e0be6251942f2ef4cd81920cf0f29f53c25f3c36f7028c2bc3c5a5c7c02d661");
                        request.AddParameter("username", "support@indiaweblab.com");
                        request.AddParameter("unicode", "true");

                        request.AddParameter("numbers", recivers.JoinStrings(","));
                        request.AddParameter("message", message);    /*"आपके खाते में इंसेंटिव के रू. 700 डाले गए है। :- IWLTPL"*/
                        IRestResponse response = client.Execute(request);
                        if (response.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            var lr = JsonConvert.DeserializeObject<TextLocalResponce>(response.Content);
                            result.Status = response.StatusCode;
                            result.Message = response.Content;
                            entity.Status = lr?.status;
                            entity.IsSent = lr?.status == "success";
                            entity.MessageId += lr?.messages.Select(x => $"{x.id}-{x.recipient}").JoinStrings(",") ?? "";
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
