using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using TrackoApi.MessageService;
using TrackoApi.Models.Base;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Infrastructure.Services;
using TrackoAPI.ViewModels.Global;

namespace TrackoAPI.Controllers.Infrastructure
{
    [RoutePrefix("api/v2/notifications"), AuthorizeEx]
    public class NotificationsController : ApiController
    {
        private TenantDbContext _tenantContext;
        private ISMSService _smsService;
        private ISendGridEmailService _emailService;

        public NotificationsController(TenantDbContext tenantContext, ISMSService smsService, ISendGridEmailService emailService)
        {
            _tenantContext = tenantContext;
            _smsService = smsService;
            _emailService = emailService;
        }
        // GET api/<controller>
        [Route("SentLogs"), ResponseType(typeof(IEnumerable<NotificationLogViewModel>)), HttpGet]
        public async Task<IEnumerable<NotificationLogViewModel>> GetAsync()
        {
            return await _tenantContext.NotificationLogs.Where(x => x.TenantId == Helper.LoggedInTenantId)
                .Select(x => new NotificationLogViewModel
                {
                    Data = x.Data,
                    Id = x.Id,
                    NoOfNotification = x.NoOfNotification,
                    NotificationType = x.NotificationType.ToString(),
                    IsSent = x.IsSent,
                    Status = x.Status,
                    SentTime = x.SentTime,
                    MessageId = x.MessageId,
                    PurchaseDate = x.fk_Purchase.PurchaseTime
                }).ToListAsync();
        }
        [Route("PurchaseLogs"), ResponseType(typeof(IEnumerable<NotificationPurchaseViewModel>)), HttpGet]
        public async Task<IEnumerable<NotificationPurchaseViewModel>> GetPurchaseLogAsync()
        {
            return await _tenantContext.NotificationPurchaseLog.Where(x => x.TenantId == Helper.LoggedInTenantId)
               .Select(x => new NotificationPurchaseViewModel
               {
                   Id = x.Id,
                   PurchaseCount = x.NoOfNotification,
                   NotificationType = x.NotificationType.ToString(),
                   ConsumedCount = x.Notifications.Count(),
                   PurchaseDate = x.PurchaseTime,
                   PurchaseRate = x.PurchaseRate
               }).ToListAsync();
        }

        [Route("SentLogs/{id}"), ResponseType(typeof(IEnumerable<NotificationPurchaseViewModel>)), HttpGet]
        public Task<NotificationLogViewModel> Get([FromUri]int id)
        {
            return _tenantContext.NotificationLogs.Where(x => x.TenantId == Helper.LoggedInTenantId)
                .Select(x => new NotificationLogViewModel
                {
                    Data = x.Data,
                    Id = x.Id,
                    NoOfNotification = x.NoOfNotification,
                    NotificationType = x.NotificationType.ToString(),
                    IsSent = x.IsSent,
                    Status = x.Status,
                    SentTime = x.SentTime,
                    MessageId = x.MessageId,
                    PurchaseDate = x.fk_Purchase.PurchaseTime
                }).FirstOrDefaultAsync();
        }

        // POST api/<controller>
        [Route("SendEmail"), ResponseType(typeof(EmailResponse)), HttpPost]
        public async Task<IHttpActionResult> SendEmailAsync([FromBody]SendGridEmailViewModel email)
        {
            if (string.IsNullOrWhiteSpace(email.HtmlBody) && string.IsNullOrWhiteSpace(email.PlanTextBody))
            {
                throw new BusinessException(ErrorCode.EventFailed, "Email Body is required");
            }
            if (string.IsNullOrWhiteSpace(email.Subject))
            {
                throw new BusinessException(ErrorCode.EventFailed, "Email Subject is required");
            }
            if (email.Tos == null || !email.Tos.Any())
            {
                throw new BusinessException(ErrorCode.EventFailed, "Email Recipients are required");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _emailService.SendAsync(email, Helper.GetLoggedInUserId(), Helper.LoggedInTenantId);
                return Ok(result);
            }
            catch (EmailServiceException ex)
            {
                return Ok(ex.Response);
            }
            catch(Exception ex)
            {
                throw;
            }
            
        }
        [Route("SendSMS"), ResponseType(typeof(SMSResult)), HttpPost]
        public async Task<IHttpActionResult> SendSmsAsync([FromBody]SMSViewModel sms)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (string.IsNullOrWhiteSpace(sms.Message))
            {
                throw new BusinessException(ErrorCode.EventFailed, "SMS Body is required");
            }
            if (sms.To == null || !sms.To.Any())
            {
                throw new BusinessException(ErrorCode.EventFailed, "SMS Recipients are required");
            }
            var smstemplate = new SMSTemplate
            {

            };
            smstemplate.SMS.Add(sms);
            var result= await _smsService.SendAsync(smstemplate, Helper.GetLoggedInUserId(), Helper.LoggedInTenantId);
            return Ok(result);
        }
    }
}