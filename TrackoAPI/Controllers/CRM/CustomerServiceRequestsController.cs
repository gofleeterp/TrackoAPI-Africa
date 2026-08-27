using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.CRM;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Infrastructure.Services;
using Tenant.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CustomerServiceRequestsController:ODataController
    {
        private readonly IRepositoryAsync<CustomerServiceRequest> _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        private readonly ISendGridEmailService _mail;

        public CustomerServiceRequestsController(IUnitOfWorkAsync unitOfWorkAsync, IRepositoryAsync<CustomerServiceRequest> service, ISendGridEmailService mailservice)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
            _mail = mailservice;
        }
        // GET: odata/CustomerServiceRequests
        [HttpGet,EnableQuery]
        public IQueryable<CustomerServiceRequest> GetCustomerServiceRequests()
        {
            return _repo.Queryable();
        }
        // GET: odata/CustomerServiceRequests(5)
        [EnableQuery]
        public SingleResult<CustomerServiceRequest> GetCustomerServiceRequest([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/CustomerServiceRequests(5)
       public async Task<IHttpActionResult> Put(long key, CustomerServiceRequest CustomerServiceRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != CustomerServiceRequest.Id)
            {
                return BadRequest();
            }
            CustomerServiceRequest.ObjectState=ObjectState.Modified;
            _repo.Update(CustomerServiceRequest);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerServiceRequestExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(CustomerServiceRequest);
        }
        // POST: odata/CustomerServiceRequests
        public async Task<IHttpActionResult> Post(CustomerServiceRequest entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var apikey = TrackoApi.Core.Helpers.Helper.GetHash(TrackoApi.Core.Helpers.Helper.RandomString(30));
            entity.RFQStatus = RFQStatus.Active;
            entity.ObjectState = ObjectState.Added;
            if(entity.SendInvitation && !string.IsNullOrWhiteSpace(entity.EmailAddress))
            {
                entity.Source = apikey;
            }
            _repo.Insert(entity);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
                try
                {
                    if (entity.SendInvitation && !string.IsNullOrWhiteSpace(entity.EmailAddress))
                    {
                        using (var db = new Tenant.Models.TenantDbContext())
                        {
                            var token = new ThirdPartyToken()
                            {
                                Appidentity = entity.EmailAddress,
                                Interval = 0,
                                IsDeactivated = false,
                                TenantId = Helper.LoggedInTenantId,
                                Token = entity.Source,
                                ExpiryDate = DateTime.Now.AddDays(2)
                            };
                            db.ThirdPartyTokens.Add(token);
                            await db.SaveChangesAsync();
                        }
                        var replyto = Helper.TenantEmailAddress.Split(';').FirstOrDefault();
                        var mail = new SendGridEmailViewModel
                        {
                            Tos = new List<EmailAddressModel> { new EmailAddressModel(entity.EmailAddress, entity.ContactPerson) },
                            Bccs = new List<EmailAddressModel> { new EmailAddressModel("support@gofleet.co.in", "GoFleet Africa") },
                            From = new EmailAddressModel("support@gofleet.co.in", "GoFleet Africa"),
                            
                            Subject = string.IsNullOrWhiteSpace(entity.Subject) ? $"RFQ Invitation from {Helper.TenantName}" : entity.Subject,
                            HtmlBody = $"Dear {entity.ContactPerson},<p> You have been invited to submit RFQ for service offered by {Helper.TenantName}.</p><p> Please follow the below link to submit RFQ.\n <a href=\"{Helper.WebAppUrl}/datasharing/servicerequest?token={apikey}\">← Submit RFQ</a></p><p>Regards,<br>{Helper.TenantShortName} Team</p>"
                        };
                        if(!string.IsNullOrWhiteSpace(replyto))
                        {
                            mail.ReplyTo = new EmailAddressModel(replyto, $"{Helper.TenantShortName} Team");
                        }
                        await _mail.SendAsync(mail, Helper.GetLoggedInUserId(), Helper.LoggedInTenantId);
                    }
                }
                catch(EmailServiceException ex)
                {
                    return BadRequest($"The Request has been created but system was unable to send email invitation. Error was:ErrorMessage:{ex.Message},EmailServerResponse:{JsonConvert.SerializeObject(ex.Response)},Body:{ex.Body}");
                }
                catch(Exception ex)
                {
                    return BadRequest($"The Request has been created but system was unable to send email invitation. Error was:{ex.GetBaseException().Message}");
                }
            }
            catch (DbUpdateException)
            {
                throw;
            }

            return Created(entity);
        }
        //// PATCH: odata/CustomerServiceRequests(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CustomerServiceRequest> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CustomerServiceRequest CustomerServiceRequest = await _repo.FindAsync(key);

            if (CustomerServiceRequest == null)
            {
                return NotFound();
            }
            CustomerServiceRequest.ObjectState=ObjectState.Modified;
            patch.Patch(CustomerServiceRequest);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerServiceRequestExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(CustomerServiceRequest);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            CustomerServiceRequest CustomerServiceRequest = await _repo.FindAsync(key);

            if (CustomerServiceRequest == null)
            {
                return NotFound();
            }
            CustomerServiceRequest.ObjectState=ObjectState.Deleted;
            _repo.Delete(CustomerServiceRequest);
            await _unitOfWorkAsync.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }
        // GET: odata/CustomerServiceRequests(5)/ConstantValues
        [EnableQuery]
        public IQueryable<CustomerServiceRequestLog> GetServices([FromODataUri] long key)
        {
            return _repo.Queryable().Where(m => m.Id == key).SelectMany(m => m.Services);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool CustomerServiceRequestExists(long id)
        {
            return _repo.Query(e => e.Id == id).Select().Any();
        }
    }
}