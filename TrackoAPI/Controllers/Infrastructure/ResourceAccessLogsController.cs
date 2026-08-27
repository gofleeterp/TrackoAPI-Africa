using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ResourceAccessLogsController : ODataController
    {
        private readonly IUserResourceAccessService _objApiResourceAccessLogService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public ResourceAccessLogsController(IUnitOfWorkAsync unitOfWorkAsync, IUserResourceAccessService service)
        {
            _objApiResourceAccessLogService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/ApiResourceAccessLog
        [HttpGet, EnableQuery]
        public IQueryable<ApiResourceAccessLog> Get()
        {
            var applicationId = this.GetClaimByKey<string>("ApplicationId");
            var userId = this.GetClaimByKey<long>("UserId");
            return _objApiResourceAccessLogService.Queryable().Where(x=>x.ApplicationId==applicationId && x.UserId==userId);
        }
        
        // POST: odata/ResourceAccessLogs
        public async Task<IHttpActionResult> Post(ApiResourceAccessLog log)
        {
            log.ApplicationId = this.GetClaimByKey<string>("ApplicationId"); 
            log.UserId = this.GetClaimByKey<long>("UserId");
            log.ResourceId = _objApiResourceAccessLogService.GetResourceId(log.ResourceName, log.ResourceType);
            if (!(log.ResourceId > 0))
            {
                return this.ApiResponse(HttpStatusCode.MethodNotAllowed, "Access Logging for this resource is not supported.");
            }
            _objApiResourceAccessLogService.AddOrUpdateResourceLog(log);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(log);
        }
        [EnableQuery,ODataRoute("SearchUserNames(searchTerm={searchTerm})"), HttpGet]
        public IQueryable<vwUserName> SearchUserNames([FromODataUri]string searchTerm)
        {
            var data = _unitOfWorkAsync.Context.Users.Where(x => x.UserName.Contains(searchTerm) || (x.FirstName+x.MiddleName+x.LastName).Contains(searchTerm)).Select(x => new vwUserName
            {
                Id = x.Id,
                UserName = x.UserName,
                FullName = x.FirstName+" "+x.MiddleName+" "+x.LastName
            }).Take(10);
            return data;
        }
        [EnableQuery, ODataRoute("SearchRoles(searchTerm={searchTerm})"), HttpGet]
        public IQueryable<vwRole> SearchRoless([FromODataUri]string searchTerm)
        {
            var data = _unitOfWorkAsync.Context.Roles.Where(x => x.Name.Contains(searchTerm)).Select(x => new vwRole
            {
                Id = x.Id,
                RoleName = x.Name
            }).Take(10);
            return data;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}