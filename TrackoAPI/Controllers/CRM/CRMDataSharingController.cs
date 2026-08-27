using Newtonsoft.Json;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using Service.Pattern;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.CRM;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Filters;
using Unity;

namespace TrackoAPI.Controllers.CRM
{
    [AuthorizeEx, RoutePrefix("internaldatasharing/crm"), CacheHeader]
    public class InternalCRMDataSharingController: BaseCRMDataSharingController
    {
        public InternalCRMDataSharingController(IUnityContainer container, IGlobalStore gs) : base(container, gs)
        {

        }
        [HttpGet, Route("Ping")]
        public override IHttpActionResult Ping() => base.Ping();
        [HttpGet, EnableQuery, Route("GetConstants")]
        public override IQueryable<ConstantValue> GetConstants() => base.GetConstants();
        [HttpGet, EnableQuery, Route("GetUnitMasters")]
        public override IQueryable<UnitMaster> GetUnitMasters() => base.GetUnitMasters();
        [HttpGet, EnableQuery, Route("GetServiceUnits")]
        public override IQueryable<ServiceUnit> GetServiceUnits() => base.GetServiceUnits();
        [HttpGet, EnableQuery, Route("GetServices")]
        public override IQueryable<ServiceMaster> GetServices() => base.GetServices();
        [HttpPost, Route("CreateServiceUnit")]
        public override Task<IHttpActionResult> CreateServiceUnit([FromBody] ServiceUnit service) => base.CreateServiceUnit(service);
        [HttpPost, Route("CreateService")]
        public override Task<IHttpActionResult> CreateServiceMaster([FromBody] ServiceMaster service) => base.CreateServiceMaster(service);
        [HttpGet, Route("GetRFQ")]
        public override Task<CustomerServiceRequest> GetRFQAsync() => base.GetRFQAsync();
        [HttpPost, Route("NewServiceRequest")]
        public override Task<IHttpActionResult> CreateServiceRequest([FromBody] CustomerServiceRequest serviceRequest) => base.CreateServiceRequest(serviceRequest);
    }
    [ApiKey, RoutePrefix("datasharing/crm")]
    public class CRMDataSharingController: BaseCRMDataSharingController
    {
        public CRMDataSharingController(IUnityContainer container, IGlobalStore gs):base(container,gs)
        {

        }
        [HttpGet, Route("Ping")]
        public override IHttpActionResult Ping() => base.Ping();
        [HttpGet, EnableQuery, Route("GetConstants")]
        public override IQueryable<ConstantValue> GetConstants() => base.GetConstants();
        [HttpGet, EnableQuery, Route("GetUnitMasters")]
        public override IQueryable<UnitMaster> GetUnitMasters() => base.GetUnitMasters();
        [HttpGet, EnableQuery, Route("GetServiceUnits")]
        public override IQueryable<ServiceUnit> GetServiceUnits() => base.GetServiceUnits();
        [HttpGet, EnableQuery, Route("GetServices")]
        public override IQueryable<ServiceMaster> GetServices() => base.GetServices();
        [HttpPost, Route("CreateServiceUnit")]
        public override Task<IHttpActionResult> CreateServiceUnit([FromBody] ServiceUnit service) => base.CreateServiceUnit(service);
        [HttpPost, Route("CreateService")]
        public override Task<IHttpActionResult> CreateServiceMaster([FromBody] ServiceMaster service) => base.CreateServiceMaster(service);
        [HttpGet, Route("GetRFQ")]
        public override Task<CustomerServiceRequest> GetRFQAsync() => base.GetRFQAsync();
        [HttpPost, Route("NewServiceRequest")]
        public override Task<IHttpActionResult> CreateServiceRequest([FromBody] CustomerServiceRequest serviceRequest) => base.CreateServiceRequest(serviceRequest);
    }
    
    public abstract class BaseCRMDataSharingController : ApiController
    {
        private readonly IUnityContainer _container;
        private readonly IGlobalStore _gs;

        public BaseCRMDataSharingController(IUnityContainer container,IGlobalStore gs)
        {
            _container = container;
            _gs = gs;
        }
        public virtual IHttpActionResult Ping()
        {
            return Ok();
        }
        
        public virtual IQueryable<UnitMaster> GetUnitMasters() => _container.Resolve<IRepositoryAsync<UnitMaster>>().Queryable();
        
        public virtual IQueryable<ServiceUnit> GetServiceUnits() => _container.Resolve<IRepositoryAsync<ServiceUnit>>().Queryable();
        
        public virtual IQueryable<ConstantValue> GetConstants() => _container.Resolve<IRepositoryAsync<ConstantValue>>().Queryable();
        public virtual IQueryable<ServiceMaster> GetServices() => _container.Resolve<IRepositoryAsync<ServiceMaster>>().Queryable();
 
        public virtual async Task<IHttpActionResult> CreateServiceUnit([FromBody] ServiceUnit service)
        {
            var repo = _container.Resolve<IRepositoryAsync<ServiceUnit>>();
            var uow = repo.UOW;
            try
            {
                service.ObjectState = ObjectState.Added;
                uow.BeginTransaction();
                repo.Insert(service);
                await repo.UOW.SaveChangesAsync();
                uow.Commit();
                return Ok(service);
            }
            catch (Exception ex)
            {
                uow.Rollback();
                //return BadRequest("Unable to Create Service Request");
                throw ex;
            }
        }
        public virtual async Task<IHttpActionResult> CreateServiceMaster([FromBody]ServiceMaster service)
        {
            var repo = _container.Resolve<IRepositoryAsync<ServiceMaster>>();
            var uow = repo.UOW;
            try
            {
                service.ObjectState = ObjectState.Added;
                uow.BeginTransaction();
                repo.Insert(service);
                await repo.UOW.SaveChangesAsync();
                uow.Commit();
                return Ok(service);
            }
            catch (Exception ex)
            {
                uow.Rollback();
                //return BadRequest("Unable to Create Service Request");
                throw ex;
            }
        }
        public virtual async Task<CustomerServiceRequest> GetRFQAsync()
        {
            var key = Helper.ApiKey;
            var repo = _container.Resolve<IRepositoryAsync<CustomerServiceRequest>>();
            return await repo.Queryable().FirstOrDefaultAsync(x => x.Source == key);
        }

       
        public virtual async Task<IHttpActionResult> CreateServiceRequest([FromBody]CustomerServiceRequest serviceRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            //if ((serviceRequest.Services?.Count ?? 0) <= 0)
            //{
            //    return BadRequest("You should atleast opt for one service");
            //}
            var repo = _container.Resolve<IRepositoryAsync<CustomerServiceRequest>>();
            var repolog = _container.Resolve<IRepositoryAsync<CustomerServiceRequestLog>>();
            if ((!string.IsNullOrWhiteSpace(serviceRequest.Source)) &&repo.Queryable().Any(x => x.Source == serviceRequest.Source&& x.RFQStatus != RFQStatus.Active))
            {
                return BadRequest("RFQ has already been submitted.");
            }
            if (serviceRequest.Id > 0) {
                serviceRequest.Source = repo.Queryable().Where(x => x.Id == serviceRequest.Id).Select(x => x.Source).FirstOrDefault();
            }
            var uow = repo.UOW;
            try
            {
                var key = Helper.ApiKey;
                var services = JsonConvert.SerializeObject(serviceRequest.Services);
                serviceRequest.Services.Clear();
                if (serviceRequest.Id > 0)
                {
                    serviceRequest.ObjectState = ObjectState.Modified;
                    repo.Update(serviceRequest);
                }
                else
                {
                    serviceRequest.ObjectState = ObjectState.Added;
                    repo.Insert(serviceRequest);
                }
                JsonConvert.DeserializeObject<List<CustomerServiceRequestLog>>(services)?.ForEach(x => {
                    x.ObjectState = x.Id>0? ObjectState.Modified: ObjectState.Added;
                    x.CSRId = serviceRequest.Id;
                    x.fk_CSR = serviceRequest;                    
                    if (x.Id > 0)
                    {
                        repolog.Update(x);
                    }
                    else
                    {
                        repolog.Insert(x);
                    }
                    
                });
                serviceRequest.Source = key;
                serviceRequest.RFQStatus = RFQStatus.Completed;
                uow.BeginTransaction();
                              
                //repo.InsertOrUpdateGraph(serviceRequest);
                await repo.UOW.SaveChangesAsync();
                uow.Commit();
               
                if (this.Request.Headers.Any(x => x.Key.ToLower() == "apikey")&&!string.IsNullOrWhiteSpace(key))
                {
                    using (var db = new Tenant.Models.TenantDbContext())
                    {
                        var keyrecord =await db.ThirdPartyTokens.FirstOrDefaultAsync(x => x.Token == key);
                        keyrecord.LastCalledTime = DateTime.Now;
                        keyrecord.IsDeactivated = true;
                        await db.SaveChangesAsync();
                        _gs.ClearThirdPartyTokens();
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                uow.Rollback();
                throw ex;
            }
        }
        public virtual IHttpActionResult GetHeaders([FromUri]string shortUrl)
        {
            var url = new Uri(shortUrl);
            return Ok();
        }
    }
}
