using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;

//using SendGrid;
//using SendGrid.Helpers.Mail; // Include if you want to use the Mail Helper
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit.Utils;
using MimeKit;
using TrackoApi.MessageService;
using System.Configuration;
using System.Text;
using System.Diagnostics;
using TrackoApi.Models.Global;
using System.IO;

namespace TrackoAPI.Infrastructure.Services
{
    public interface ISendGridEmailService
    {
        Task SendAsync(IdentityMessage message, long userId, string tenantId);
        Task<EmailResponse> SendAsync(SendGridEmailViewModel message, long userId, string tenantId);
        Task<EmailResponse> SendAsync(SendGridEmailViewModel message, HttpRequestPool req);
        Task<EmailResponse> SendOTPAsync(SendGridEmailViewModel message);
        Task SendAsync(IdentityMessage message);        
    }
    public class SendGridEmailService : ISendGridEmailService
    {
        private readonly TenantSMTPConfiguration _config;      
        public SendGridEmailService()
        {
            var wcval = ConfigurationManager.AppSettings.Get("SMTPSettings");
            if (string.IsNullOrWhiteSpace(wcval))
            {
                wcval = ConfigurationManager.AppSettings.Get($"SMTP_{Helper.LoggedInTenantId}");
            }
            if (!string.IsNullOrWhiteSpace(wcval))
            {
                var jsonbytes = Convert.FromBase64String(wcval);
                var jsontext = Encoding.UTF8.GetString(jsonbytes);
                LogToFile($"SMTP Config (decoded): {jsontext}"); // Add this line
                _config = JsonConvert.DeserializeObject<TenantSMTPConfiguration>(jsontext);
            }
            else
            {
                throw new KeyNotFoundException($"SMTP Configuration Not Found with Key \"SMTPSettings\" nor with \"SMTP_{Helper.LoggedInTenantId}\"");
            }
        }
        private void LogToFile(string message)
        {
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "email_log.txt");
                File.AppendAllText(logPath, $"{DateTime.Now}: {message}{Environment.NewLine}");
            }
            catch { }
        }
        /// <summary>
        /// Use only for OTP of free email Delivery
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public async Task SendAsync(IdentityMessage message)
        {
            await configSendGridasync(message);
        }
        private HttpRequestPool _req;
        public async Task<EmailResponse> SendAsync(SendGridEmailViewModel message, HttpRequestPool req=null)
        {
            _req = req;
            return await SendInternalAsync(message).ConfigureAwait(false);
        }
        public async Task SendAsync(IdentityMessage message, long userId, string tenantId)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            using (var db = !Helper.HostedOnPremise ? new TenantDbContext() : new CoreSettingDb("HostCoreConnection"))
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entity = new NotificationLog
                        {
                            Data = JsonConvert.SerializeObject(message),
                            NoOfNotification = 1,
                            NotificationType = TrackoAPI.Models.Shared.NotificationType.Email,
                            Status = "Pending",
                            TenantId = tenantId,
                            UserId = userId,
                            SentTime = DateTimeOffset.Now,
                            IsSent = false
                        };
                        db.NotificationLogs.Add(entity);
                        var entitysaved = db.SaveChanges();
                        if (entitysaved == 0)
                        {
                            tran.Rollback();
                            throw new BusinessException(ErrorCode.EventFailed, "Unable to save Log");
                        }

                        if (string.IsNullOrWhiteSpace(message.Body))
                        {
                            entity.Status = "Email Body was empty.";
                            entity.IsSent = false;
                            db.SaveChanges();
                            tran.Commit();
                            return;
                        }
                        if (string.IsNullOrWhiteSpace(message.Destination))
                        {
                            entity.Status = "Sender Email address was empty or null.";
                            entity.IsSent = false;
                            db.SaveChanges();
                            tran.Commit();
                            return;
                        }
                        var response = await configSendGridasync(message);
                        if (response.IsSuccessful)
                        {
                            entity.Status = "Sent";
                            entity.IsSent = true;
                            entity.MessageId = response.UniqueMessageId;
                            db.SaveChanges();

                        }
                        else
                        {
                            entity.Status = response.Message;
                            entity.IsSent = false;
                            entity.MessageId = response.UniqueMessageId;
                            db.SaveChanges();
                        }
                        tran.Commit();
                        return;

                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
        public Task<EmailResponse> SendOTPAsync(SendGridEmailViewModel message)
        {
            return SendInternalAsync(message);
        }
        public async Task<EmailResponse> SendAsync(SendGridEmailViewModel message, long userId, string tenantId)
        {
            using (var db = !Helper.HostedOnPremise ? new TenantDbContext() : new CoreSettingDb("HostCoreConnection"))
            {
                using (var tran = db.Database.BeginTransaction())
                {
                    try
                    {
                        var entity = new NotificationLog
                        {
                            Data = JsonConvert.SerializeObject(message),
                            NoOfNotification = 1,
                            NotificationType = TrackoAPI.Models.Shared.NotificationType.Email,
                            Status = "Pending",
                            TenantId = string.IsNullOrWhiteSpace(tenantId) ? Helper.LoggedInTenantId : tenantId,
                            UserId = userId == 0 ? Helper.GetLoggedInUserId() : userId,
                            SentTime = DateTimeOffset.Now,
                            IsSent = false
                        };
                        db.NotificationLogs.Add(entity);
                        var entitysaved = db.SaveChanges();
                        if (entitysaved == 0)
                        {
                            tran.Rollback();
                            throw new BusinessException(ErrorCode.EventFailed, "Unable to save Log");
                        }
                        if (string.IsNullOrWhiteSpace(message.HtmlBody) && string.IsNullOrWhiteSpace(message.PlanTextBody))
                        {
                            entity.Status = "Email Body was empty.";
                            entity.IsSent = false;
                            db.SaveChanges();
                            tran.Commit();
                            return new EmailResponse()
                            {
                                DateSent = DateTime.Now,
                                Message = "Email Body was empty.",
                                Status = System.Net.HttpStatusCode.BadRequest,
                                UniqueMessageId = null
                            };
                        }
                        if (message.Tos == null || !message.Tos.Any())
                        {
                            entity.Status = "Sender Email address was empty or null.";
                            entity.IsSent = false;
                            db.SaveChanges();
                            tran.Commit();
                            return new EmailResponse()
                            {
                                DateSent = DateTime.Now,
                                Message = "Sender Email address was empty or null.",
                                Status = System.Net.HttpStatusCode.BadRequest,
                                UniqueMessageId = null
                            };
                        }

                        if (message.CustomArgs == null)
                        {
                            message.CustomArgs = new Dictionary<string, string>();
                        }
                        if (!message.CustomArgs.ContainsKey("tenant_key"))
                        {
                            message.CustomArgs.Add("tenant_key", tenantId);
                        }
                        if (!message.CustomArgs.ContainsKey("sender_id"))
                        {
                            message.CustomArgs.Add("sender_id", userId.ToString());
                        }
                        if (!message.CustomArgs.ContainsKey("gofleet_id"))
                        {
                            message.CustomArgs.Add("gofleet_id", entity.Id.ToString());
                        }
                        var response = await SendInternalAsync(message).ConfigureAwait(false);
                        if (response.IsSuccessful)
                        {
                            entity.Status = "Sent";
                            entity.IsSent = true;
                            entity.MessageId = response.UniqueMessageId;
                            db.SaveChanges();
                        }
                        else
                        {
                            entity.Status = response.Message;
                            entity.IsSent = false;
                            entity.MessageId = response.UniqueMessageId;
                            db.SaveChanges();
                        }
                        tran.Commit();
                        return response;
                    }
                    catch (Exception)
                    {
                        tran.Rollback();
                        throw;
                    }
                }
            }
        }
        private async Task<EmailResponse> SendInternalAsync(SendGridEmailViewModel mail)
        {
            var message = new MimeMessage();
            var response = new EmailResponse();
            if (mail.From == null || string.IsNullOrWhiteSpace(mail.From.EmaillAddress))
            {
                message.From.Add(new MailboxAddress("Auto Mail", _config.FromEmail));
            }
            else
            {
                message.From.Add(new MailboxAddress(mail.From.Name, mail.From.EmaillAddress));
            }

            var toos = mail.Tos.Select(x => new MailboxAddress(x.Name, x.EmaillAddress)).ToList();
            message.To.AddRange(toos);
            message.Subject = mail.Subject;
            if (mail.Ccs.Any())
            {
                var ccs = mail.Ccs.Select(x => new MailboxAddress(x.Name, x.EmaillAddress)).ToList();
                message.Cc.AddRange(ccs);
            }

            message.Cc.Add(new MailboxAddress("GoFleet Africa", "support@gofleet.co.in"));
            message.ReplyTo.Add(new MailboxAddress("GoFleet Africa", "support@gofleet.co.in"));

            if (mail.Bccs.Any())
            {
                var bccs = mail.Bccs.Select(x => new MailboxAddress(x.Name, x.EmaillAddress)).ToList();
                message.Bcc.AddRange(bccs);
            }

            if (mail.ReplyTo == null || string.IsNullOrWhiteSpace(mail.ReplyTo.EmaillAddress))
            {
                message.ReplyTo.Add(new MailboxAddress("GoFleet Africa", "support@gofleet.co.in"));
            }
            else
            {
                message.ReplyTo.Add(new MailboxAddress(mail.ReplyTo.Name, mail.ReplyTo.EmaillAddress));
            }

            message.Importance = MessageImportance.Normal;
            if (mail.CustomArgs != null && mail.CustomArgs.Any())
            {
                foreach (var arg in mail.CustomArgs)
                {
                    message.Headers.Add(arg.Key, arg.Value);
                }
            }
            var builder = new BodyBuilder();
            if (mail.Attachments.Any())
            {
                foreach (var att in mail.Attachments)
                {
                    // create an image attachment for the file located at path
                    builder.Attachments.Add(att.Filename, Convert.FromBase64String(att.Content));
                    //.Add(att.Filename, att.Content, att.Type, att.Disposition, att.ContentId);
                }
            }
            if (!string.IsNullOrWhiteSpace(mail.HtmlBody))
            {
                builder.HtmlBody = mail.HtmlBody;
            }
            else
            {
                builder.TextBody = mail.PlanTextBody;
            }
            message.Date = DateTime.Now;
            var messageid = MimeUtils.GenerateMessageId();
            message.MessageId = messageid;
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    client.DeliveryStatusNotificationType = DeliveryStatusNotificationType.Full;
                    client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                    client.Timeout = (int)TimeSpan.FromMinutes(_config.Timeout).TotalMilliseconds;
                    client.MessageSent += (sender, evt) =>
                    {
                        response.DateSent = DateTime.Now;
                        response.Status = System.Net.HttpStatusCode.OK;
                        response.UniqueMessageId = messageid;
                        if (_req != null)
                        {
                            response.Message = evt.Response;
                            _req.Result = JsonConvert.SerializeObject(response);
                        }
                        response.Message = evt.Response;
                    };
                    await client.ConnectAsync(_config.SMTPAddress, _config.Port, SecureSocketOptions.StartTls);
                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync(_config.UserName, _config.Password);
                    if (!client.IsAuthenticated)
                    {
                        if (_req != null)
                        {
                            _req.Result = "SMTP authentication failed.";
                        }
                    }

                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
                catch (Exception ex)
                {
                    response.Status = System.Net.HttpStatusCode.ExpectationFailed;
                    response.Message = ex.GetBaseException().Message;
                    response.Errors = new List<EmailErrorMessage>() { new EmailErrorMessage { Message = ex.ToString() } };

                    if (_req != null)
                    {
                        _req.Result = $"Email Exception: {ex.ToString()}";
                    }

                    LogToFile($"[Email Error] {ex}");
                }
                return response;
            }
        }

        // Use NuGet to install SendGrid (Basic C# client lib) 
        private async Task<EmailResponse> configSendGridasync(IdentityMessage mail, EmailAddressModel sender = null)
        {
            var message = new MimeMessage();
            var response = new EmailResponse();
            var from = new MailboxAddress(sender?.Name ?? "GoFleet Africa", sender?.EmaillAddress ?? "no-reply@gofleet.co.in");
            if (string.IsNullOrWhiteSpace(from.Address)) throw new ArgumentNullException(nameof(from));
            var toos = mail.Destination.Split(';').Select(x => new MailboxAddress(x.Split('@')[0], x)).ToList();
            message.From.Add(from);
            message.To.AddRange(toos);
            message.Subject = mail.Subject;
            message.ReplyTo.Add(new MailboxAddress("GoFleet Africa", "support@gofleet.co.in"));
            message.Importance = MessageImportance.High;
            var builder = new BodyBuilder();
            builder.HtmlBody = mail.Body;
            message.Date = DateTime.Now;
            var messageid = MimeUtils.GenerateMessageId();
            message.MessageId = messageid;
            message.Body = builder.ToMessageBody();

            using (var client = new SmtpClient())
            {
                try
                {
                    client.DeliveryStatusNotificationType = DeliveryStatusNotificationType.Full;
                    client.SslProtocols = System.Security.Authentication.SslProtocols.Default;
                    client.Timeout = (int)TimeSpan.FromMinutes(_config.Timeout).TotalMilliseconds;
                    client.MessageSent += (obj, evt) =>
                    {
                        response.DateSent = DateTime.Now;
                        response.Message = evt.Response;
                        response.Status = System.Net.HttpStatusCode.OK;
                        response.UniqueMessageId = messageid;
                    };
                    await client.ConnectAsync(_config.SMTPAddress, _config.Port, _config.UseSSL);
                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync(_config.UserName, _config.Password);
                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
                catch (Exception ex)
                {
                    throw new EmailServiceException(System.Net.HttpStatusCode.BadRequest, ex.ToStringDemystified());
                }
                return response;
            }
        }
    }
}