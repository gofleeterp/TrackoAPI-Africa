using MailKit.Net.Smtp;
using MailKit.Security;

using Microsoft.AspNet.Identity;

using MimeKit;
using MimeKit.Utils;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

using TrackoAPI.Infrastructure.Services;

namespace TrackoApi.MessageService
{
    public class SMTPMailService : ISendGridEmailService
    {
        private readonly TenantSMTPConfiguration _config;

        public SMTPMailService()
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
                _config = JsonConvert.DeserializeObject<TenantSMTPConfiguration>(jsontext);
            }
            else
            {
                throw new KeyNotFoundException($"SMTP Configuration Not Found with Key \"SMTPSettings\" nor with \"SMTP_{Helper.LoggedInTenantId}\"");
            }
        }
        public Task SendAsync(IdentityMessage message, long userId, string tenantId)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (string.IsNullOrWhiteSpace(message.Body))
            {
                throw new BusinessException(ErrorCode.EventFailed, "Email Body was empty.");
            }
            if (string.IsNullOrWhiteSpace(message.Destination))
            {
                throw new BusinessException(ErrorCode.EventFailed, "Sender Email address was empty or null.");
            }
            return SendIdentityMessageAsync(message);
        }
        private HttpRequestPool _req;
        public async Task<EmailResponse> SendAsync(SendGridEmailViewModel message, HttpRequestPool req = null)
        {
            _req = req;
            return await SendMailWithAttechmentAsync(message).ConfigureAwait(false);
        }
        public async Task<EmailResponse> SendAsync(SendGridEmailViewModel message, long userId, string tenantId)
        {
            if (string.IsNullOrWhiteSpace(message.HtmlBody) && string.IsNullOrWhiteSpace(message.PlanTextBody))
            {
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
            return await SendMailWithAttechmentAsync(message).ConfigureAwait(false);
        }

        public Task SendAsync(IdentityMessage message)
        {
            return SendIdentityMessageAsync(message);
        }

        public Task<EmailResponse> SendOTPAsync(SendGridEmailViewModel message)
        {
            return SendMailWithAttechmentAsync(message);
        }
        private async Task<EmailResponse> SendIdentityMessageAsync(IdentityMessage mail, EmailAddressModel sender = null)
        {
            var message = new MimeMessage();
            var response = new EmailResponse();
            var from = new MailboxAddress(sender?.Name ?? "GoFleet Africa", sender?.EmaillAddress ?? _config.FromEmail);
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
        private async Task<EmailResponse> SendMailWithAttechmentAsync(SendGridEmailViewModel mail)
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
                    await client.ConnectAsync(_config.SMTPAddress, _config.Port, SecureSocketOptions.StartTlsWhenAvailable);
                    // Note: only needed if the SMTP server requires authentication
                    await client.AuthenticateAsync(_config.UserName, _config.Password);

                    await client.SendAsync(message);
                    client.Disconnect(true);
                }
                catch (Exception ex)
                {
                    response.Status = System.Net.HttpStatusCode.ExpectationFailed;
                    response.Message = ex.Message;
                    response.Errors = new List<EmailErrorMessage>() { new EmailErrorMessage() {
                    Message = ex.Message,
                    } };
                }
                return response;
            }
        }
        //private static EmailResponse ToMailResponse(Response response)
        //{
        //    if (response == null)
        //        return null;

        //    var headers = (HttpHeaders)response.Headers;
        //    var messageId = headers.GetValues(MessageId).FirstOrDefault();
        //    return new EmailResponse()
        //    {
        //        UniqueMessageId = messageId,
        //        DateSent = DateTime.UtcNow,
        //        Status = response.StatusCode
        //    };
        //}
    }

    public class TenantSMTPConfiguration
    {
        public string SMTPAddress { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public int Port { get; set; }
        public string FromEmail { get; set; }
        public bool UseSSL { get; set; }
        public int Timeout { get; set; }
    }
}
