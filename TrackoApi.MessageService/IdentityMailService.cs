using Microsoft.AspNet.Identity;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Core.Helpers;

namespace TrackoApi.MessageService
{
    public class IdentityMailService : IIdentityMessageService
    {
        private readonly TenantSMTPConfiguration _gmail;
        public IdentityMailService()
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
                _gmail = JsonConvert.DeserializeObject<TenantSMTPConfiguration>(jsontext);
            }
            else
            {
                throw new KeyNotFoundException($"SMTP Configuration Not Found with Key \"SMTPSettings\" nor with \"SMTP_{Helper.LoggedInTenantId}\"");
            }
        }
        public async Task SendAsync(IdentityMessage msg)
        {
            try
            {
                MailMessage message = new MailMessage();
                SmtpClient smtp = new SmtpClient();

                message.From = new MailAddress(_gmail.FromEmail, "GOFLEET AFRICA");
                message.To.Add("support@gofleet.co.in");
                foreach (var x in msg.Destination.Split(';'))
                {
                    message.To.Add(new MailAddress(x));
                }
                message.Subject = msg.Subject;
                message.Body = msg.Body;
                message.IsBodyHtml = true;
                smtp.Port = _gmail.Port;
                smtp.Host = _gmail.SMTPAddress;
                smtp.EnableSsl = _gmail.UseSSL;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new NetworkCredential(_gmail.UserName, _gmail.Password);
                //smtp.Credentials = new NetworkCredential("749683001@smtp-brevo.com", "Jj4nbxIWtHOzN7c5");
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                await smtp.SendMailAsync(message);
            }
            catch (Exception ex)
            {
                LogToFile($"[Email Error] {ex}");
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
    }
}
