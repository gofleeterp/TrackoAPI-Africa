using Microsoft.AspNet.Identity;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using RestSharp;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Tenant.Models;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS.GPS;

namespace TrackoApi.MessageService
{
    public class GenericSMSService : ISMSService
    {
        private JsonSerializerSettings _jsonSetting;
        private readonly GpsEndPoint _endpoint;

        public GenericSMSService(GpsEndPoint endpoint)
        {
            _jsonSetting = new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            _endpoint = endpoint;
            if (_endpoint.Headers == null && !string.IsNullOrWhiteSpace(_endpoint._Headers))
            {
                _endpoint.Headers = JsonConvert.DeserializeObject<IDictionary<string, object>>(_endpoint._Headers);
            }
        }

        public Task<SMSResult> SendAsync(string message, string sender = "IWLT", long userId = 0, string tenantId = null, params string[] recivers)
        {
            try
            {

            }catch (Exception ex)
            {

            }
            throw new NotImplementedException();
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
                        var client = new RestClient(_endpoint.Url);
                        client.Timeout = 6000;
                        //if (string.IsNullOrWhiteSpace(sms.Sender))
                        //{
                        if (_endpoint.Headers.TryGetValue("sender", out var sender)) {
                            sms.Sender = sender.ToString();
                        }
                        //}
                        SMSResult result = new SMSResult();
                        foreach (var req in sms.SMS)
                        {
                            var requestBody = _endpoint.ParameterTemplate;
                            var request = new RestSharp.RestRequest((RestSharp.Method)Enum.Parse(typeof(RestSharp.Method), _endpoint.Method.ToUpper()));
                            

                            if (_endpoint.Headers != null && _endpoint.Headers.Count > 0)
                            {
                                _endpoint.Headers.Keys.ToList().ForEach(x => {
                                    request.AddHeader(x, _endpoint.Headers[x].ToString());
                                });
                            }
                            //request.AddHeader("apiKey", "YjVhN2JiNWMzMzQ4MzE1ZDEzMWI2OTlkZjdkOTA4Yzg=");
                            //request.AddHeader("format", "json");
                            //request.AlwaysMultipartFormData = true;
                            //request.AddParameter("apikey", "6e0be6251942f2ef4cd81920cf0f29f53c25f3c36f7028c2bc3c5a5c7c02d661");
                            //request.AddParameter("sender", sms.Sender);
                            //request.AddParameter("hash", "6e0be6251942f2ef4cd81920cf0f29f53c25f3c36f7028c2bc3c5a5c7c02d661");
                            //request.AddParameter("username", "support@indiaweblab.com");
                            //request.AddParameter("unicode", "true");

                            //request.AddParameter("numbers", req.To.JoinStrings(","));
                            //request.AddParameter("message", req.Message);    /*"आपके खाते में इंसेंटिव के रू. 700 डाले गए है। :- IWLTPL"*/

                            if (_endpoint.Method == "GET")
                            {
                                request.Resource = _endpoint.ParameterTemplate.Trim().Replace('\n', ' ');
                            }
                            else
                            {
                                request.AddParameter(string.IsNullOrWhiteSpace(_endpoint.ContentType)? "application/json;charset = utf - 8":_endpoint.ContentType, requestBody, ParameterType.RequestBody);
                            }
                            IRestResponse response = client.Execute(request);
                            if (response.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                //var lr = JsonConvert.DeserializeObject<TextLocalResponce>(response.Content);
                                result.Status = response.StatusCode;
                                result.Message = response.Content;

                                entity.Status = response.Content;
                                entity.IsSent = true;
                                //entity.MessageId += lr?.messages.Select(x => $"{x.id}-{x.recipient}").JoinStrings(",") ?? "";
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

        public Task SendAsync(IdentityMessage message)
        {
            throw new NotImplementedException();
        }
        private void VerifyPurchase()
        {

        }
    }
}
